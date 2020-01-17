using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Core.Graphics.VulkanBackend.Utility;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Core.Graphics.VulkanBackend
{
	[Flags]
	public enum BufferUsageFlags {
		None = 0,
		TransferSrc = 1,
		TransferDst = 2,
		UniformTexelBuffer = 4,
		StorageTexelBuffer = 8,
		UniformBuffer = 16, // 0x00000010
		StorageBuffer = 32, // 0x00000020
		IndexBuffer = 64, // 0x00000040
		VertexBuffer = 128, // 0x00000080
		IndirectBuffer = 256, // 0x00000100
	}

	public enum BufferMemoryUsageHint {
		/// <summary>
		/// Memory is set once, then accessed by GPU only
		/// </summary>
		Static = 0,
		/// <summary>
		/// Memory is written to every frame by the CPU, and read by the GPU
		/// </summary>
		Dynamic = 1
	}

	public unsafe class DeviceBuffer : IDisposable {
		public VkBuffer vkBuffer;
		//public VkDeviceMemory vkMemory;
		public VulkanMemorySlice memory;
		internal GraphicsDevice device;
		public readonly UInt64 size;
		private VkBufferUsageFlags bufferUsageFlags;
		private BufferMemoryUsageHint usageHint;


		public DeviceBuffer(GraphicsDevice device, UInt64 size, BufferUsageFlags flags, BufferMemoryUsageHint usageHint) {
			this.device = device;
			this.size = size;
			this.bufferUsageFlags = (VkBufferUsageFlags) flags;
			this.usageHint = usageHint;
			Allocate();
		}

		internal DeviceBuffer(GraphicsDevice device, UInt64 size, void* data, BufferUsageFlags flags, BufferMemoryUsageHint usageHint) {
			this.device = device;
			this.size = size;
			this.bufferUsageFlags = (VkBufferUsageFlags) flags;
			this.usageHint = usageHint;
			Allocate();
			StagingTransfer(0, size, data);
		}

		public static DeviceBuffer CreateFrom<T>(GraphicsDevice device, ReadOnlySpan<T> data, BufferUsageFlags flags, BufferMemoryUsageHint usageHint) where T : unmanaged {
			fixed (T* ptr = data) {
				return new DeviceBuffer(device, (ulong)(data.Length * Marshal.SizeOf<T>()), ptr, flags, usageHint);
			}
		}


		public void SetData<T>(ReadOnlySpan<T> data, UInt64 dstOffsetInBytes = 0) where T : unmanaged {
			fixed (T* ptr = data) {
				uint len = (uint)Math.Min(size - dstOffsetInBytes, (ulong)(data.Length * Marshal.SizeOf<T>()));

				if (memory.hostVisible) {

					Unsafe.CopyBlock((byte*)memory.Mapped + dstOffsetInBytes, ptr, len);
					//Flush(dstOffsetInBytes, len);
				}
				else {
					StagingTransfer(dstOffsetInBytes, len, ptr);
				}
			}
		}

		public void SetData<T>(Span<T> data, UInt64 dstOffsetInBytes = 0) where T : unmanaged
			=> SetData((ReadOnlySpan<T>) data, dstOffsetInBytes);

		public void SetData<T>(T data, UInt64 dstOffsetInBytes = 0) where T : unmanaged {
			uint len = (uint)Math.Min(size - dstOffsetInBytes, (ulong)Marshal.SizeOf<T>());
			if (memory.hostVisible) {
				Unsafe.Copy((byte*)memory.Mapped + dstOffsetInBytes, ref data);
				//Flush(dstOffsetInBytes, len);
			}
			else {
				
				StagingTransfer(dstOffsetInBytes, (UInt64)(len), &data);
			}
		}


		private void StagingTransfer(UInt64 dstOffset, UInt64 transferSize, void* data) {
			VkBuffer stagingBuffer;
			VkDeviceMemory stagingMemory;

			// Create staging buffers
			// Vertex data
			Util.CheckResult(device.vulkanDevice.createBuffer(
				VkBufferUsageFlags.TransferSrc,
				VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent,
				transferSize,
				&stagingBuffer,
				&stagingMemory,
				data));

			//CopyTo buffer
			var cmdPool = device.GetCommandPool();

			using (var buffer = cmdPool.Rent()) {
				buffer.Begin();
				buffer.CopyBufferTo(stagingBuffer, vkBuffer, 0, dstOffset, transferSize);
				buffer.End();
				device.FlushCommandBuffer(buffer);
			}

			vkDestroyBuffer(device.device, stagingBuffer, null);
			vkFreeMemory(device.device, stagingMemory, null);
		}

		//private void Flush(UInt64 start, UInt64 length) {
		//	VkMappedMemoryRange memoryRange = VkMappedMemoryRange.New();
		//	memoryRange.memory = vkMemory;
		//	memoryRange.size = length;
		//	memoryRange.offset = start;

		//	Util.CheckResult(vkFlushMappedMemoryRanges(device.device, 1, ref memoryRange));
		//}


		private void Allocate() {

			if (usageHint == BufferMemoryUsageHint.Static) {
				bufferUsageFlags |= VkBufferUsageFlags.TransferDst;
			}

			// Create the buffer handle
			VkBufferCreateInfo bufferCreateInfo = Initializers.bufferCreateInfo(bufferUsageFlags, size);
			bufferCreateInfo.sharingMode = VkSharingMode.Exclusive;
			Util.CheckResult(vkCreateBuffer(device.device, &bufferCreateInfo, null, out vkBuffer));

			// Create the memory backing up the buffer handle
			VkMemoryRequirements memReqs;
			vkGetBufferMemoryRequirements(device.device, vkBuffer, &memReqs);
			
			var hostVisible = usageHint == BufferMemoryUsageHint.Dynamic;
			memory = device.memoryAllocator.Allocate(memReqs, hostVisible);

			// Attach the memory to the buffer object
			Util.CheckResult(vkBindBufferMemory(device.device, vkBuffer, memory.vkDeviceMemory, memory.offset));
		}


		public void Dispose() {
			if (vkBuffer.Handle != 0)
			{
				vkDestroyBuffer(device.device, vkBuffer, null);
			}
			memory.Dispose();
		}

		public VkDescriptorBufferInfo GetVkDescriptor() {
			VkDescriptorBufferInfo bufferInfo = new VkDescriptorBufferInfo();
			bufferInfo.buffer = vkBuffer;
			bufferInfo.offset = 0;
			bufferInfo.range = size;
			return bufferInfo;
		}
	}
}
