using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Core.Graphics.VulkanBackend.Utility;
using Vulkan;
using static Vulkan.VulkanNative;


namespace Core.Graphics.VulkanBackend
{
	public unsafe class FrameBuffer : IDisposable {

		public VkFramebuffer vkFrameBuffer;
		public GraphicsDevice device;
		public RenderPass renderPass;
		public DepthStencil depthStencil;
		public Swapchain swapchain;
		public uint swapchainImageIndex;
		private bool disposed = true;

		public FrameBuffer(GraphicsDevice device, RenderPass renderPass, DepthStencil depthStencil, Swapchain swapchain, uint swapchainImageIndex) {
			this.device = device;
			this.renderPass = renderPass;
			this.depthStencil = depthStencil;
			this.swapchain = swapchain;
			this.swapchainImageIndex = swapchainImageIndex;
			Setup();
		}

		public void Recreate() {
			Dispose();
			Setup();
		}

		public void Dispose() {
			if (!disposed) {
				vkDestroyFramebuffer(device.device, vkFrameBuffer, null);
				disposed = true;
			}
		}


		private void Setup() {
			using (NativeList<VkImageView> attachments = new NativeList<VkImageView>(2))
			{
				attachments.Count = 2;
				// Depth/Stencil attachment is the same for all frame buffers
				attachments[1] = depthStencil.vkView;

				VkFramebufferCreateInfo frameBufferCreateInfo = VkFramebufferCreateInfo.New();
				frameBufferCreateInfo.renderPass = renderPass.vkRenderPass;
				frameBufferCreateInfo.attachmentCount = 2;
				frameBufferCreateInfo.pAttachments = (VkImageView*)attachments.Data;
				frameBufferCreateInfo.width = swapchain.width;
				frameBufferCreateInfo.height = swapchain.height;
				frameBufferCreateInfo.layers = 1;

				attachments[0] = swapchain.vkSwapchain.Buffers[swapchainImageIndex].View;
				Util.CheckResult(vkCreateFramebuffer(device.device, ref frameBufferCreateInfo, null, out vkFrameBuffer));
			}

			disposed = false;
		}
	}
}
