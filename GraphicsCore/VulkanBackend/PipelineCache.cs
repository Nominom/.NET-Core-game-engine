using System;
using System.Collections.Generic;
using System.Text;
using Core.Graphics.VulkanBackend.Utility;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Core.Graphics.VulkanBackend
{
	public unsafe class PipelineCache : IDisposable
	{
		public VkPipelineCache vkPipelineCache;
		private GraphicsDevice device;
		public PipelineCache(GraphicsDevice device) {
			this.device = device;
			VkPipelineCacheCreateInfo pipelineCacheCreateInfo = VkPipelineCacheCreateInfo.New();
			Util.CheckResult(vkCreatePipelineCache(device.device, ref pipelineCacheCreateInfo, null, out vkPipelineCache));
		}

		public void Dispose() {
			vkDestroyPipelineCache(device.device, vkPipelineCache, null);
		}
	}
}
