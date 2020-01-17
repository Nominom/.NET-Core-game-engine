// Code from: mellinoe/vk repository https://github.com/mellinoe/vk
using System;
using Vulkan;

namespace Core.Graphics.VulkanBackend.Utility
{
	public static class Util
	{

		public static void CheckResult(VkResult result)
		{
			if (result != VkResult.Success)
			{
				throw new InvalidOperationException("Vulkan call failed: "+ result);
			}
		}
	}

	public static class Strings
	{
		public static string VK_KHR_SURFACE_EXTENSION_NAME = "VK_KHR_surface";
		public static string VK_KHR_WIN32_SURFACE_EXTENSION_NAME = "VK_KHR_win32_surface";
		public static string VK_KHR_XCB_SURFACE_EXTENSION_NAME = "VK_KHR_xcb_surface";
		public static string VK_KHR_XLIB_SURFACE_EXTENSION_NAME = "VK_KHR_xlib_surface";
		public static string VK_KHR_SWAPCHAIN_EXTENSION_NAME = "VK_KHR_swapchain";
		public static string VK_EXT_DEBUG_REPORT_EXTENSION_NAME = "VK_EXT_debug_report";
		public static string StandardValidationLayerName = "VK_LAYER_LUNARG_standard_validation";
	}
}
