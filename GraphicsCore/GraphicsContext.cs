using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Core.Graphics;
using ECSCore;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace GraphicsCore
{
	public static class GraphicsContext
	{
		internal static CommandList _commandList;
		internal static Shader[] _shaders;
		internal static Pipeline _pipeline;
		internal static GraphicsDevice _graphicsDevice;
		internal static ResourceFactory factory;
		internal static bool initialized;

		private const string VertexCode = @"
#version 450

layout(location = 0) in vec3 Position;
layout(location = 1) in vec4 color;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 1);
    fsin_Color = color;

	// End of Vulkan vertex shader
	//gl_Position.y = -gl_Position.y; //Needed to translate Direct3D coordinates to Vulkan coordinate space

	// End of OpenGL vertex shader
	//gl_Position.z = gl_Position.z * 2.0 - gl_Position.w; // Correct for OpenGL clip coordinates

}";

		private const string FragmentCode = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

		internal static void Initialize(ECSWorld world)
		{
			if (initialized) return;
			if (!Window.initialized) throw new InvalidOperationException("Window should be initialized before initializing Graphics.");
			initialized = true;

			_graphicsDevice = VeldridStartup.CreateGraphicsDevice(Window.window, new GraphicsDeviceOptions() { PreferDepthRangeZeroToOne = true });

			CreateResources();

			Window.OnWindowClose += DisposeResources;
		}

		public static void CreateResources()
		{
			factory = _graphicsDevice.ResourceFactory;


			VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
				new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
				new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));


			ShaderDescription vertexShaderDesc = new ShaderDescription(
				ShaderStages.Vertex,
				Encoding.UTF8.GetBytes(VertexCode),
				"main");
			ShaderDescription fragmentShaderDesc = new ShaderDescription(
				ShaderStages.Fragment,
				Encoding.UTF8.GetBytes(FragmentCode),
				"main");

			_shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

			GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
			pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

			pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
				depthTestEnabled: true,
				depthWriteEnabled: true,
				comparisonKind: ComparisonKind.LessEqual);

			pipelineDescription.RasterizerState = new RasterizerStateDescription(
				cullMode: FaceCullMode.Back,
				fillMode: PolygonFillMode.Solid,
				frontFace: FrontFace.Clockwise,
				depthClipEnabled: true,
				scissorTestEnabled: false);

			pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
			pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();

			pipelineDescription.ShaderSet = new ShaderSetDescription(
				vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
				shaders: _shaders);

			pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
			_pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

			_commandList = factory.CreateCommandList();
		}

		public static void DisposeResources() {
			if (!initialized) return;
			try
			{
				RenderUtilities.DisposeAllUtils();
				_pipeline.Dispose();
				_shaders[0].Dispose();
				_shaders[1].Dispose();
				_commandList.Dispose();
				_graphicsDevice.Dispose();
				initialized = false;
			}
			catch(Exception ex) {
				Console.WriteLine(ex);
			}
		}
	}

	struct VertexPositionColor
	{
		public Vector3 Position; // This is the position, in normalized device coordinates.
		public RgbaFloat Color; // This is the color of the vertex.
		public VertexPositionColor(Vector3 position, RgbaFloat color)
		{
			Position = position;
			Color = color;
		}
		public const uint SizeInBytes = 28;
	}

}
