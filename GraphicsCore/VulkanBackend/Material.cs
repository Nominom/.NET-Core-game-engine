using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Core.Shared;

namespace Core.Graphics.VulkanBackend
{
	public unsafe class Material : IDisposable {
		private Pipeline pipeline;
		private UniformBuffer uniformBuffer; // Not disposed here.
		public ShaderPair shaderPair;
		public readonly GraphicsDevice device;
		public Texture2D mainTexture;
		public Color mainColor;
		public bool wireframe = false;

		public Material(GraphicsDevice device, UniformBuffer uniformBuffer, ShaderPair shaders, Color mainColor, Texture2D mainTexture = null) {
			this.device = device;
			this.uniformBuffer = uniformBuffer;
			this.shaderPair = shaders;
			this.mainColor = mainColor;
			this.mainTexture = mainTexture ?? Texture2D.White;
		}

		public Material(GraphicsDevice device, UniformBuffer uniformBuffer, ShaderPair shaders, Texture2D mainTexture = null) {
			this.device = device;
			this.uniformBuffer = uniformBuffer;
			this.shaderPair = shaders;
			this.mainColor = Color.white;
			this.mainTexture = mainTexture ?? Texture2D.White;
		}


		public Pipeline GetPipeline() {
			if (pipeline == null) {
				InitializePipeline();
			}
			return pipeline;
		}

		private void InitializePipeline() {
			using PipelineCreator pipelineCreator = new PipelineCreator();
			this.pipeline = pipelineCreator.CreatePipeline(device, device.renderPass, this, uniformBuffer);
		}

		public void Dispose() {
			pipeline?.Dispose();
			pipeline = null;
		}

		public static Material Create(Color mainColor, Texture2D mainTexture)
		{
			Material material = new Material(GraphicsContext.graphicsDevice, GraphicsContext.uniform0,
				GraphicsContext.defaultShader, mainColor, mainTexture);

			return material;
		}

		public static Material Create(Color mainColor)
		{
			Material material = new Material(GraphicsContext.graphicsDevice, GraphicsContext.uniform0,
				GraphicsContext.defaultShader, mainColor, Texture2D.White);

			return material;
		}

	}
}
