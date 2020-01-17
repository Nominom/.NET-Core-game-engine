using System;
using System.Collections.Generic;
using System.Text;
using Vulkan;

namespace Core.Graphics.VulkanBackend
{
	public unsafe class Swapchain {
		public readonly VulkanSwapchain vkSwapchain = new VulkanSwapchain();
		public GraphicsDevice device;
		public uint width { get; private set; }
		public uint height { get; private set; }

		public Swapchain(GraphicsDevice device) {
			this.device = device;
		}

		public void Connect(VkInstance instance, VkPhysicalDevice physicalDevice, VkDevice device) {
			vkSwapchain.Connect(instance, physicalDevice, device);
		}

		public void InitSurface(IntPtr windowHandle) {
			vkSwapchain.InitSurface(windowHandle);
		}

		public void RecreateSwapchain()
		{
			uint width, height;
			vkSwapchain.Create(&width, &height, device.Settings.VSync);

			this.width = width;
			this.height = height;
		}
	}
}
