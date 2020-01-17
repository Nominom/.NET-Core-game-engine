using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Core.Graphics.VulkanBackend.Utility;
using Vulkan;
using static Vulkan.VulkanNative;


namespace Core.Graphics.VulkanBackend
{
	public unsafe class DepthStencil : IDisposable
	{
		public VkImage vkImage;
		public VkDeviceMemory vkMem;
		public VkImageView vkView;
		private GraphicsDevice device;
		private Swapchain swapchain;
		private bool disposed = true;

		public DepthStencil(GraphicsDevice device, Swapchain swapchain) {
			this.device = device;
			this.swapchain = swapchain;
			Setup();
		}

		public void Recreate() {
			Dispose();
			Setup();
		}

		public void Dispose() {
			if (!disposed) {
				vkDestroyImageView(device.device, vkView, null);
				vkDestroyImage(device.device, vkImage, null);
				vkFreeMemory(device.device, vkMem, null);
				disposed = true;
			}
		}

		private void Setup() {
			disposed = false;
			VkImageCreateInfo image = VkImageCreateInfo.New();
            image.imageType = VkImageType.Image2D;
            image.format = device.DepthFormat;
            image.extent = new VkExtent3D() { width = swapchain.width, height = swapchain.height, depth = 1 };
            image.mipLevels = 1;
            image.arrayLayers = 1;
            image.samples = VkSampleCountFlags.Count1;
            image.tiling = VkImageTiling.Optimal;
            image.usage = (VkImageUsageFlags.DepthStencilAttachment | VkImageUsageFlags.TransferSrc);
            image.flags = 0;

            VkMemoryAllocateInfo mem_alloc = VkMemoryAllocateInfo.New();
            mem_alloc.allocationSize = 0;
            mem_alloc.memoryTypeIndex = 0;

            VkImageViewCreateInfo depthStencilView = VkImageViewCreateInfo.New();
            depthStencilView.viewType = VkImageViewType.Image2D;
            depthStencilView.format = device.DepthFormat;
            depthStencilView.flags = 0;
            depthStencilView.subresourceRange = new VkImageSubresourceRange();
            depthStencilView.subresourceRange.aspectMask = (VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil);
            depthStencilView.subresourceRange.baseMipLevel = 0;
            depthStencilView.subresourceRange.levelCount = 1;
            depthStencilView.subresourceRange.baseArrayLayer = 0;
            depthStencilView.subresourceRange.layerCount = 1;

            Util.CheckResult(vkCreateImage(device.device, &image, null, out vkImage));
            vkGetImageMemoryRequirements(device.device, vkImage, out VkMemoryRequirements memReqs);
            mem_alloc.allocationSize = memReqs.size;
            mem_alloc.memoryTypeIndex = device.vulkanDevice.getMemoryType(memReqs.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal);
            Util.CheckResult(vkAllocateMemory(device.device, &mem_alloc, null, out vkMem));
            Util.CheckResult(vkBindImageMemory(device.device, vkImage, vkMem, 0));

            depthStencilView.image = vkImage;
            Util.CheckResult(vkCreateImageView(device.device, ref depthStencilView, null, out vkView));
		}
	}
}
