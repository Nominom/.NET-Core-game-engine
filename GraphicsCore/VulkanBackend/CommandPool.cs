using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Core.Graphics.VulkanBackend.Utility;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Core.Graphics.VulkanBackend
{
	public unsafe class CommandPool : IDisposable {
		public const uint MAX_BUFFERS = 2048;
		public readonly VkCommandPool vkCmdPool;
		public readonly GraphicsDevice device;
		public NativeList<VkCommandBuffer> primaryCmdBuffers { get; protected set; } = new NativeList<VkCommandBuffer>();
		public NativeList<VkCommandBuffer> secondaryCmdBuffers { get; protected set; } = new NativeList<VkCommandBuffer>();

		public Queue<VkCommandBuffer> freePrimaryBuffers = new Queue<VkCommandBuffer>();
		public Queue<VkCommandBuffer> freeSecondaryBuffers = new Queue<VkCommandBuffer>();

		public  CommandPool(GraphicsDevice device, Swapchain swapchain) {
			this.device = device;

			VkCommandPoolCreateInfo cmdPoolInfo = VkCommandPoolCreateInfo.New();
			cmdPoolInfo.queueFamilyIndex = swapchain.vkSwapchain.QueueNodeIndex;
			cmdPoolInfo.flags = 
				VkCommandPoolCreateFlags.ResetCommandBuffer
				| VkCommandPoolCreateFlags.Transient;
			Util.CheckResult(vkCreateCommandPool(device.device, &cmdPoolInfo, null, out vkCmdPool));

			AllocateBuffers(VkCommandBufferLevel.Primary, 2);
			AllocateBuffers(VkCommandBufferLevel.Secondary, 2);
		}

		public void Dispose() {
			vkFreeCommandBuffers(device.device, vkCmdPool, primaryCmdBuffers.Count, primaryCmdBuffers.Data);
			vkFreeCommandBuffers(device.device, vkCmdPool, secondaryCmdBuffers.Count, secondaryCmdBuffers.Data);
			vkDestroyCommandPool(device.device, vkCmdPool, null);
		}

		private void AllocateBuffers(VkCommandBufferLevel level, uint numBuffers) {
			NativeList<VkCommandBuffer> buffers =
				level == VkCommandBufferLevel.Primary ? primaryCmdBuffers : secondaryCmdBuffers;
			var queue = 
				level == VkCommandBufferLevel.Primary ? freePrimaryBuffers : freeSecondaryBuffers;
			
			// Create one command buffer for each swap chain image and reuse for rendering
			buffers.Resize(numBuffers);
			buffers.Count = numBuffers;

			if (buffers.Count > MAX_BUFFERS) {
				throw new InvalidOperationException("Hit max buffer amount. Please check if buffers are not being reused correctly.");
			}

			VkCommandBufferAllocateInfo cmdBufAllocateInfo =
				Initializers.CommandBufferAllocateInfo(vkCmdPool, level, buffers.Count);

			Util.CheckResult(vkAllocateCommandBuffers(device.device, ref cmdBufAllocateInfo, (VkCommandBuffer*)buffers.Data));

			foreach (VkCommandBuffer buffer in buffers) {
				queue.Enqueue(buffer);
			}
		}

		private void GrowBuffers(VkCommandBufferLevel level, uint numBuffers) {
			NativeList<VkCommandBuffer> buffers =
				level == VkCommandBufferLevel.Primary ? primaryCmdBuffers : secondaryCmdBuffers;

			uint oldBuffers = buffers.Count;

			if (numBuffers <= oldBuffers) {
				return;
			}

			// Create one command buffer for each swap chain image and reuse for rendering
			buffers.Resize(numBuffers);
			buffers.Count = numBuffers;

			if (buffers.Count > MAX_BUFFERS) {
				throw new InvalidOperationException("Hit max buffer amount. Please check if buffers are not being reused correctly.");
			}

			VkCommandBufferAllocateInfo cmdBufAllocateInfo =
				Initializers.CommandBufferAllocateInfo(vkCmdPool, level, buffers.Count - oldBuffers);

			Util.CheckResult(vkAllocateCommandBuffers(device.device, ref cmdBufAllocateInfo, (VkCommandBuffer*)buffers.GetAddress(oldBuffers)));

			var queue = 
				level == VkCommandBufferLevel.Primary ? freePrimaryBuffers : freeSecondaryBuffers;

			for (uint i = oldBuffers; i < buffers.Count; i++) {
				queue.Enqueue(buffers[i]);
			}
		}

		public CommandBuffer Rent(VkCommandBufferLevel level = VkCommandBufferLevel.Primary) {
			if (level == VkCommandBufferLevel.Primary) {
				if (freePrimaryBuffers.TryDequeue(out var result)) {
					return new CommandBuffer(result, this, level);
				}
				else {
					GrowBuffers(level, primaryCmdBuffers.Count + 10);
					result = freePrimaryBuffers.Dequeue();
					return new CommandBuffer(result, this, level);
				}
			}
			else {
				if (freeSecondaryBuffers.TryDequeue(out var result)) {
					return new CommandBuffer(result, this, level);
				}
				else {
					GrowBuffers(level, secondaryCmdBuffers.Count + 10);
					result = freeSecondaryBuffers.Dequeue();
					return new CommandBuffer(result, this, level);
				}
			}
		}

		public void Return(CommandBuffer buffer) {
			Debug.Assert(buffer.pool == this);
			var queue = 
				buffer.level == VkCommandBufferLevel.Primary ? freePrimaryBuffers : freeSecondaryBuffers;

			queue.Enqueue(buffer.vkCmd);
			vkResetCommandBuffer(buffer.vkCmd, VkCommandBufferResetFlags.ReleaseResources);
		}

		public void ReturnAll() {
			freePrimaryBuffers.Clear();
			freeSecondaryBuffers.Clear();
			for (int i = 0; i < primaryCmdBuffers.Count; i++) {
				vkResetCommandBuffer(primaryCmdBuffers[i], VkCommandBufferResetFlags.ReleaseResources);
				freePrimaryBuffers.Enqueue(primaryCmdBuffers[i]);
			}

			for (int i = 0; i < secondaryCmdBuffers.Count; i++) {
				vkResetCommandBuffer(secondaryCmdBuffers[i], VkCommandBufferResetFlags.ReleaseResources);
				freeSecondaryBuffers.Enqueue(secondaryCmdBuffers[i]);
			}
		}
	}
}
