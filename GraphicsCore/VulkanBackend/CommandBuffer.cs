using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.ECS;
using Core.Graphics.VulkanBackend.Utility;
using Vulkan;
using static Vulkan.VulkanNative;


namespace Core.Graphics.VulkanBackend
{
	public unsafe class CommandBuffer : IDisposable {
		public readonly VkCommandBuffer vkCmd;
		public readonly CommandPool pool;
		public readonly VkCommandBufferLevel level;
		internal bool begun;
		internal bool inRenderPass;
		internal bool ended;
		internal bool renderPassUseSecondaryBuffers;

		public CommandBuffer(VkCommandBuffer internalBuffer, CommandPool pool, VkCommandBufferLevel level) {
			vkCmd = internalBuffer;
			this.pool = pool;
			this.level = level;
			begun = false;
			inRenderPass = false;
			ended = false;
			renderPassUseSecondaryBuffers = false;
		}

		public void Dispose() {
			pool.Return(this);
#if DEBUG
			GC.SuppressFinalize(this);
#endif
		}

#if DEBUG
		~CommandBuffer()
		{
			if (vkCmd.Handle != NullHandle)
			{
				Console.WriteLine($"A CommandBuffer was not properly disposed.\n");
				pool.Return(this);
			}
		}
#endif

		public void Begin() {
			CheckNotBegun();
			if (vkCmd.Handle == NullHandle) {
				return;
			}
			VkCommandBufferBeginInfo cmdBufInfo = Initializers.commandBufferBeginInfo();
			cmdBufInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;
			Util.CheckResult(vkBeginCommandBuffer(vkCmd, &cmdBufInfo));

			begun = true;
		}

		public void BeginAndContinueRenderPass(RenderContext context) {
			if (vkCmd.Handle == NullHandle) {
				return;
			}
			CheckNotBegun();
			CheckSecondary();

			VkCommandBufferBeginInfo cmdBufInfo = Initializers.commandBufferBeginInfo();
			VkCommandBufferInheritanceInfo inheritanceInfo = VkCommandBufferInheritanceInfo.New();

			cmdBufInfo.flags = VkCommandBufferUsageFlags.RenderPassContinue;

			inheritanceInfo.renderPass = context.currentRenderPass.vkRenderPass;
			inheritanceInfo.subpass = context.currentSubPassIndex;
			inheritanceInfo.framebuffer = context.currentFrameBuffer.vkFrameBuffer;

			cmdBufInfo.pInheritanceInfo = &inheritanceInfo;

			Util.CheckResult(vkBeginCommandBuffer(vkCmd, &cmdBufInfo));
			begun = true;
			inRenderPass = true;

			//Does not draw anything without viewport and scissor
			SetViewportScissor(context);
		}

		public void End() {
			if (vkCmd.Handle == NullHandle) {
				return;
			}
			CheckBegun();

			Util.CheckResult(vkEndCommandBuffer(vkCmd));
			ended = true;
		}

		internal void CopyBufferTo(VkBuffer srcBuffer, VkBuffer dstBuffer, ulong srcOffset, ulong dstOffset, ulong numBytes) {
			if (vkCmd.Handle == NullHandle) {
				return;
			}
			CheckBegun();
			CheckNotInRenderPass();

			VkBufferCopy copyRegion = new VkBufferCopy();

			copyRegion.size = numBytes;
			copyRegion.srcOffset = srcOffset;
			copyRegion.dstOffset = dstOffset;

			vkCmdCopyBuffer(
				vkCmd,
				srcBuffer,
				dstBuffer,
				1,
				&copyRegion);
		}

		public void CopyBufferTo(DeviceBuffer srcBuffer, DeviceBuffer dstBuffer, ulong srcOffset, ulong dstOffset,
			ulong numBytes)
			=> CopyBufferTo(srcBuffer.vkBuffer, dstBuffer.vkBuffer, srcOffset, dstOffset, numBytes);

		public void UpdateBuffer<T>(T value, DeviceBuffer dstBuffer, ulong dstOffset) where T : unmanaged {
			if (vkCmd.Handle == NullHandle) {
				return;
			}
			ulong size = (ulong)Unsafe.SizeOf<T>();
			DebugHelper.AssertThrow<InvalidOperationException>(size <= 65536);
			CheckBegun();
			CheckNotInRenderPass();

			vkCmdUpdateBuffer(vkCmd, dstBuffer.vkBuffer, dstOffset, size, &value);
		}

		public void UpdateBuffer<T>(ReadOnlySpan<T> values, DeviceBuffer dstBuffer, ulong dstOffset) where T : unmanaged {
			if (vkCmd.Handle == NullHandle) {
				return;
			}
			ulong size = (ulong)Unsafe.SizeOf<T>() * (ulong)values.Length;
			DebugHelper.AssertThrow<InvalidOperationException>(size <= 65536);
			CheckBegun();
			CheckNotInRenderPass();

			fixed (T* ptr = values) {
				vkCmdUpdateBuffer(vkCmd, dstBuffer.vkBuffer, dstOffset, size, &ptr);
			}
		}

		/// <summary>
		/// Begin a renderpass without clearing color or depth.
		/// </summary>
		public void BeginRenderPass(RenderPass renderPass, FrameBuffer frameBuffer, bool useSecondaryCommandBuffers) {
			CheckBegun();
			CheckNotInRenderPass();
			//FixedArray2<VkClearValue> clearValues = new FixedArray2<VkClearValue>();
			//clearValues.First.color = new VkClearColorValue(0, 0, 0);
			//clearValues.Second.depthStencil = new VkClearDepthStencilValue() { depth = 1.0f, stencil = 0 };

			VkRenderPassBeginInfo renderPassBeginInfo = Initializers.renderPassBeginInfo();
			renderPassBeginInfo.renderPass = renderPass.vkRenderPass;
			renderPassBeginInfo.renderArea.offset.x = 0;
			renderPassBeginInfo.renderArea.offset.y = 0;
			renderPassBeginInfo.renderArea.extent.width = frameBuffer.swapchain.width;
			renderPassBeginInfo.renderArea.extent.height = frameBuffer.swapchain.height;
			//renderPassBeginInfo.clearValueCount = 2;
			//renderPassBeginInfo.pClearValues = &clearValues.First;
			renderPassBeginInfo.framebuffer = frameBuffer.vkFrameBuffer;

			renderPassUseSecondaryBuffers = useSecondaryCommandBuffers;
			VkSubpassContents subPassContents = useSecondaryCommandBuffers
				? VkSubpassContents.SecondaryCommandBuffers
				: VkSubpassContents.Inline;
			vkCmdBeginRenderPass(vkCmd, &renderPassBeginInfo, subPassContents);

			VkViewport viewport = Initializers.viewport((float)frameBuffer.swapchain.width, (float)frameBuffer.swapchain.height, 0.0f, 1.0f);
			vkCmdSetViewport(vkCmd, 0, 1, &viewport);

			VkRect2D scissor = Initializers.rect2D(frameBuffer.swapchain.width, frameBuffer.swapchain.height, 0, 0);
			vkCmdSetScissor(vkCmd, 0, 1, &scissor);
			inRenderPass = true;
		}

		/// <summary>
		/// Begin renderpass and clear the color and depth of the supplied framebuffer.
		/// </summary>
		/// <param name="renderPass"></param>
		/// <param name="frameBuffer"></param>
		/// <param name="clearColor"></param>
		/// <param name="clearDepth"></param>
		public void BeginRenderPassClearColorDepth(RenderPass renderPass, FrameBuffer frameBuffer, VkClearColorValue clearColor, VkClearDepthStencilValue clearDepth, bool useSecondaryCommandBuffers) {
			CheckBegun();
			CheckNotInRenderPass();
			FixedArray2<VkClearValue> clearValues = new FixedArray2<VkClearValue>();
			clearValues.First.color = clearColor;
			clearValues.Second.depthStencil = clearDepth;

			VkRenderPassBeginInfo renderPassBeginInfo = Initializers.renderPassBeginInfo();
			renderPassBeginInfo.renderPass = renderPass.vkRenderPass;
			renderPassBeginInfo.renderArea.offset.x = 0;
			renderPassBeginInfo.renderArea.offset.y = 0;
			renderPassBeginInfo.renderArea.extent.width = frameBuffer.swapchain.width;
			renderPassBeginInfo.renderArea.extent.height = frameBuffer.swapchain.height;
			renderPassBeginInfo.clearValueCount = 2;
			renderPassBeginInfo.pClearValues = &clearValues.First;
			renderPassBeginInfo.framebuffer = frameBuffer.vkFrameBuffer;

			renderPassUseSecondaryBuffers = useSecondaryCommandBuffers;
			VkSubpassContents subPassContents = useSecondaryCommandBuffers
				? VkSubpassContents.SecondaryCommandBuffers
				: VkSubpassContents.Inline;

			vkCmdBeginRenderPass(vkCmd, &renderPassBeginInfo, subPassContents);

			VkViewport viewport = Initializers.viewport((float)frameBuffer.swapchain.width, (float)frameBuffer.swapchain.height, 0.0f, 1.0f);
			vkCmdSetViewport(vkCmd, 0, 1, &viewport);

			VkRect2D scissor = Initializers.rect2D(frameBuffer.swapchain.width, frameBuffer.swapchain.height, 0, 0);
			vkCmdSetScissor(vkCmd, 0, 1, &scissor);
			inRenderPass = true;
		}

		public void NextSubPass(bool useSecondaryCommandBuffers, ref RenderContext graphicsContext) {
			CheckInRenderPass();
			renderPassUseSecondaryBuffers = useSecondaryCommandBuffers;
			VkSubpassContents subPassContents = useSecondaryCommandBuffers
				? VkSubpassContents.SecondaryCommandBuffers
				: VkSubpassContents.Inline;

			vkCmdNextSubpass(vkCmd, subPassContents);
			graphicsContext.currentSubPassIndex++;
		}

		public void EndRenderPass() {
			CheckBegun();
			CheckInRenderPass();
			vkCmdEndRenderPass(vkCmd);
			inRenderPass = false;
		}


		/// <summary>
		/// Must be called outside of a renderpass scope.
		/// </summary>
		/// <param name="depthStencil"></param>
		/// <param name="clearValue"></param>
		public void ClearColor(Swapchain swapchain, uint swapchainImageIndex, VkClearColorValue clearColor) {
			CheckBegun();
			CheckNotInRenderPass();

			VkClearValue clearValue = new VkClearValue();
			clearValue.color = clearColor;

			VkImageSubresourceRange imageRange = new VkImageSubresourceRange();
			imageRange.aspectMask = VkImageAspectFlags.Color;
			imageRange.levelCount = 1;
			imageRange.layerCount = 1;
			vkCmdClearColorImage(vkCmd, swapchain.vkSwapchain.Images[swapchainImageIndex], VkImageLayout.General, &clearColor, 1, &imageRange);
		}

		/// <summary>
		/// Must be called outside of a renderpass scope.
		/// </summary>
		/// <param name="depthStencil"></param>
		/// <param name="clearValue"></param>
		public void ClearDepthStencil(DepthStencil depthStencil, VkClearDepthStencilValue clearValue) {
			CheckBegun();
			CheckNotInRenderPass();
			VkImageSubresourceRange imageRange = new VkImageSubresourceRange();
			imageRange.aspectMask = VkImageAspectFlags.Depth;
			imageRange.levelCount = 1;
			imageRange.layerCount = 1;

			vkCmdClearDepthStencilImage(vkCmd, depthStencil.vkImage, VkImageLayout.General, &clearValue, 1, &imageRange);
		}

		public void SetViewportScissor(RenderContext context) {
			VkViewport viewport = Initializers.viewport((float)context.currentFrameBuffer.swapchain.width, (float)context.currentFrameBuffer.swapchain.height, 0.0f, 1.0f);
			vkCmdSetViewport(vkCmd, 0, 1, &viewport);

			VkRect2D scissor = Initializers.rect2D(context.currentFrameBuffer.swapchain.width, context.currentFrameBuffer.swapchain.height, 0, 0);
			vkCmdSetScissor(vkCmd, 0, 1, &scissor);
		}

		public void UsePipeline(Pipeline pipeline) {
			CheckInRenderPass();
			vkCmdBindDescriptorSets(vkCmd, VkPipelineBindPoint.Graphics, pipeline.pipelineLayout, 0, 1, ref pipeline.descriptorSet, 0, null);
			vkCmdBindPipeline(vkCmd, VkPipelineBindPoint.Graphics, pipeline.vkPipeline);
		}

		public void BindVertexBuffer(DeviceBuffer buffer, uint bindingIndex) {
			CheckBegun();
			CheckInRenderPass();
			ulong offsets = 0;
			vkCmdBindVertexBuffers(vkCmd, bindingIndex, 1, ref buffer.vkBuffer, ref offsets);
		}

		public void BindIndexBuffer(DeviceBuffer buffer, VkIndexType indexType, ulong offset = 0) {
			CheckBegun();
			CheckInRenderPass();
			vkCmdBindIndexBuffer(vkCmd, buffer.vkBuffer, offset, indexType);
		}

		public void DrawIndexed(uint indexCount, uint instanceCount, uint indexOffset, int vertexOffset, uint instanceOffset) {
			CheckBegun();
			CheckInRenderPass();

			vkCmdDrawIndexed(vkCmd, indexCount, instanceCount, indexOffset, vertexOffset, instanceOffset);
		}

		public void DrawIndexed(GpuMesh mesh, uint instanceCount, uint instanceOffset) {
			CheckBegun();
			CheckInRenderPass();

			BindVertexBuffer(mesh.vertices, 0);
			BindIndexBuffer(mesh.indices, mesh.indexType, 0);
			vkCmdDrawIndexed(vkCmd, mesh.indexCount, instanceCount, 0, 0, instanceOffset);
		}

		public void ExecuteSecondaryBuffers(NativeList<VkCommandBuffer> commandBuffers) {
			CheckBegun();
			CheckPrimary();
			CheckRenderPassUseSecondary();

			vkCmdExecuteCommands(vkCmd, commandBuffers.Count, commandBuffers.Data);
		}

		public void ExecuteSecondaryBuffer(CommandBuffer cmd) {
			CheckBegun();
			CheckPrimary();
			CheckRenderPassUseSecondary();

			Debug.Assert(cmd.level == VkCommandBufferLevel.Secondary);

			VkCommandBuffer vc = cmd.vkCmd;
			vkCmdExecuteCommands(vkCmd, 1, &vc);
		}




		[Conditional("DEBUG")]
		private void CheckBegun() {
			if (!begun) {
				throw new InvalidOperationException("Cannot call this method before calling Begin(). Please call one of the Begin() methods first.");
			}
		}

		[Conditional("DEBUG")]
		private void CheckNotBegun() {
			if (begun) {
				throw new InvalidOperationException($"Cannot call {nameof(Begin)} methods twice.");
			}
		}

		[Conditional("DEBUG")]
		private void CheckInRenderPass() {
			if (!inRenderPass) {
				throw new InvalidOperationException($"Cannot call this method outside of a RenderPass instance. Please call one of the {nameof(BeginRenderPass)} methods or " +
				                                    $"the {nameof(BeginAndContinueRenderPass)} method if this is a secondary command buffer.");
			}
		}

		[Conditional("DEBUG")]
		private void CheckNotInRenderPass() {
			if (inRenderPass) {
				throw new InvalidOperationException($"Cannot call this method inside of a RenderPass instance. Please call EndRenderPass() first");
			}
		}

		[Conditional("DEBUG")]
		private void CheckPrimary() {
			if (level != VkCommandBufferLevel.Primary) {
				throw new InvalidOperationException($"Cannot call this method in a secondary CommandBuffer.");
			}
		}

		[Conditional("DEBUG")]
		private void CheckSecondary() {
			if (level != VkCommandBufferLevel.Secondary) {
				throw new InvalidOperationException($"Cannot call this method in a primary CommandBuffer.");
			}
		}

		[Conditional("DEBUG")]
		private void CheckRenderPassUseSecondary() {
			if (inRenderPass && !renderPassUseSecondaryBuffers) {
				throw new InvalidOperationException($"Please call BeginRenderPass with the useSecondaryBuffers set to true");
			}
		}
	}
}
