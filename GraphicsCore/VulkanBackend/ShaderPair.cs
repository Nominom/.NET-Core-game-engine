using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Core.AssetSystem;
using Core.AssetSystem.Assets;
using Core.ECS;
using Core.Graphics.VulkanBackend.Utility;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Core.Graphics.VulkanBackend
{

	public enum ShaderType
	{
		Normal,
		Instanced
	}

	public unsafe class ShaderPair : IDisposable
	{
		public ShaderType shaderType;
		public VkShaderModule vkVert;
		public VkShaderModule vkFrag;
		public GraphicsDevice device;
		private static readonly FixedUtf8String entryPoint = "main"; //The entry point of the shader

		internal ShaderPair(GraphicsDevice device, ShaderType type, VkShaderModule frag, VkShaderModule vert)
		{
			this.device = device;
			this.shaderType = type;
			this.vkFrag = frag;
			this.vkVert = vert;
		}

		public static ShaderPair Load(GraphicsDevice device, string frag, string vert, ShaderType type)
		{
			var fragShader = loadShader(frag, device.device, VkShaderStageFlags.Fragment);
			var vertShader = loadShader(vert, device.device, VkShaderStageFlags.Vertex);

			return new ShaderPair(device, type, fragShader, vertShader);
		}

		public static ShaderPair Load(GraphicsDevice device, AssetReference<ShaderAsset> frag, AssetReference<ShaderAsset> vert, ShaderType type)
		{
			var fragShader = loadShader(frag, device.device, VkShaderStageFlags.Fragment);
			var vertShader = loadShader(vert, device.device, VkShaderStageFlags.Vertex);

			return new ShaderPair(device, type, fragShader, vertShader);
		}



		public VkPipelineShaderStageCreateInfo GetFragPipeline()
		{
			VkPipelineShaderStageCreateInfo shaderStage = VkPipelineShaderStageCreateInfo.New();
			shaderStage.stage = VkShaderStageFlags.Fragment;
			shaderStage.module = vkFrag;
			shaderStage.pName = entryPoint;
			Debug.Assert(shaderStage.module.Handle != 0);
			return shaderStage;
		}

		public VkPipelineShaderStageCreateInfo GetVertPipeline()
		{
			VkPipelineShaderStageCreateInfo shaderStage = VkPipelineShaderStageCreateInfo.New();
			shaderStage.stage = VkShaderStageFlags.Vertex;
			shaderStage.module = vkVert;
			shaderStage.pName = entryPoint;
			Debug.Assert(shaderStage.module.Handle != 0);
			return shaderStage;
		}


		public void Dispose()
		{
			vkDestroyShaderModule(device.device, vkFrag, null);
			vkDestroyShaderModule(device.device, vkVert, null);
		}


		private static VkShaderModule loadShader(string fileName, VkDevice device, VkShaderStageFlags stage)
		{
			using (var fs = File.OpenRead(fileName))
			{
				var length = fs.Length;
			}
			byte[] shaderCode = File.ReadAllBytes(fileName);
			ulong shaderSize = (ulong)shaderCode.Length;
			fixed (byte* scPtr = shaderCode)
			{
				// Create a new shader module that will be used for Pipeline creation
				VkShaderModuleCreateInfo moduleCreateInfo = VkShaderModuleCreateInfo.New();
				moduleCreateInfo.codeSize = new UIntPtr(shaderSize);
				moduleCreateInfo.pCode = (uint*)scPtr;

				Util.CheckResult(vkCreateShaderModule(device, ref moduleCreateInfo, null, out VkShaderModule shaderModule));

				return shaderModule;
			}
		}

		private static VkShaderModule loadShader(AssetReference<ShaderAsset> asset, VkDevice device, VkShaderStageFlags stage)
		{
			if (!asset.IsLoaded)
			{
				asset.LoadNow();
			}

			var shader = asset.Get();
			if ((stage & VkShaderStageFlags.Fragment) != 0)
			{
				DebugHelper.AssertThrow<InvalidOperationException>(shader.Type == ShaderAsset.ShaderType.Frag);
			}
			else if ((stage & VkShaderStageFlags.Vertex) != 0)
			{
				DebugHelper.AssertThrow<InvalidOperationException>(shader.Type == ShaderAsset.ShaderType.Vertex);
			}

			byte[] shaderCode = shader.GetBytes();
			ulong shaderSize = (ulong)shaderCode.Length;
			fixed (byte* scPtr = shaderCode)
			{
				// Create a new shader module that will be used for Pipeline creation
				VkShaderModuleCreateInfo moduleCreateInfo = VkShaderModuleCreateInfo.New();
				moduleCreateInfo.codeSize = new UIntPtr(shaderSize);
				moduleCreateInfo.pCode = (uint*)scPtr;

				Util.CheckResult(vkCreateShaderModule(device, ref moduleCreateInfo, null, out VkShaderModule shaderModule));

				return shaderModule;
			}
		}

	}
}
