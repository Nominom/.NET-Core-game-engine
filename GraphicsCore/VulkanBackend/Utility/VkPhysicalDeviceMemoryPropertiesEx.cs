// Code from: mellinoe/vk repository https://github.com/mellinoe/vk
using Vulkan;

namespace Core.Graphics.VulkanBackend.Utility
{
	public static unsafe class VkPhysicalDeviceMemoryPropertiesEx
	{
		public static VkMemoryType GetMemoryType(this VkPhysicalDeviceMemoryProperties memoryProperties, uint index)
		{
			return (&memoryProperties.memoryTypes_0)[index];
		}


		public static VkMemoryHeap GetMemoryHeap(this VkPhysicalDeviceMemoryProperties memoryProperties, uint index)
		{
			return (&memoryProperties.memoryHeaps_0)[index];
		}
	}
}
