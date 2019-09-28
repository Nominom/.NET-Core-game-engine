using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Core.Graphics;
using Core.ECS;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace Core.Graphics
{
	internal static class GraphicsContext
	{
		internal static CommandList _commandList;


		internal static ResourceSet _sharedResourceSet;
		internal static DeviceBuffer _cameraProjViewBuffer;

		internal static Shader[] _shaders_normal;
		internal static Pipeline _pipeline_normal;

		internal static Shader[] _shaders_instanced;
		internal static Pipeline _pipeline_instanced;

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


		private const string InstanceVertexCode = @"
#version 450

layout (location = 0) in vec3 Position;
layout (location = 1) in vec4 color;
layout (location = 2) in vec4 model1; //loc2, loc3, loc4, loc5
layout (location = 3) in vec4 model2; //loc2, loc3, loc4, loc5
layout (location = 4) in vec4 model3; //loc2, loc3, loc4, loc5
layout (location = 5) in vec4 model4; //loc2, loc3, loc4, loc5



layout(set = 0, binding = 0) uniform ProjView
{
    mat4 view;
    mat4 proj;
};

layout(location = 0) out vec4 fsin_Color;

void main()
{
	mat4 model = mat4(model1, model2, model3, model4);

    gl_Position = proj * view * model * vec4(Position, 1);
    fsin_Color = color;

	// End of Vulkan vertex shader
	//gl_Position.y = -gl_Position.y; //Needed to translate Direct3D coordinates to Vulkan coordinate space

	// End of OpenGL vertex shader
	//gl_Position.z = gl_Position.z * 2.0 - gl_Position.w; // Correct for OpenGL clip coordinates

}";

		private const string InstanceFragmentCode = @"
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
			Window.OnWindowResize += (w, h) => { _graphicsDevice.ResizeMainWindow((uint)w,(uint)h); };
		}

		public static void CreateNormalPipeline() {
			
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

			_shaders_normal = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

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
				shaders: _shaders_normal);

			pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
			_pipeline_normal = factory.CreateGraphicsPipeline(pipelineDescription);
		}


		public static void CreateInstancedPipeline() {
			VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
				new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
				new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

			VertexLayoutDescription matrixLayout = new VertexLayoutDescription(stride:16,instanceStepRate:1,
				new VertexElementDescription("model1", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
				new VertexElementDescription("model2", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
				new VertexElementDescription("model3", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
				new VertexElementDescription("model4", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
				);

			ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(new ResourceLayoutElementDescription("ProjView", ResourceKind.UniformBuffer, ShaderStages.Vertex));
			ResourceLayout sharedLayout = factory.CreateResourceLayout(resourceLayoutDescription);

			BindableResource[] bindableResources = { _cameraProjViewBuffer };
			ResourceSetDescription resourceSetDescription = new ResourceSetDescription(sharedLayout, bindableResources);
			_sharedResourceSet = factory.CreateResourceSet(resourceSetDescription);



			ShaderDescription vertexShaderDesc = new ShaderDescription(
				ShaderStages.Vertex,
				Encoding.UTF8.GetBytes(InstanceVertexCode),
				"main");
			ShaderDescription fragmentShaderDesc = new ShaderDescription(
				ShaderStages.Fragment,
				Encoding.UTF8.GetBytes(InstanceFragmentCode),
				"main");

			_shaders_instanced = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

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
			pipelineDescription.ResourceLayouts = new[] {
				sharedLayout
			};

			pipelineDescription.ShaderSet = new ShaderSetDescription(
				vertexLayouts: new VertexLayoutDescription[] { vertexLayout, matrixLayout },
				shaders: _shaders_instanced);

			pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
			_pipeline_instanced = factory.CreateGraphicsPipeline(pipelineDescription);
		}

		public static void CreateResources()
		{
			factory = _graphicsDevice.ResourceFactory;

			_cameraProjViewBuffer = factory.CreateBuffer(
				new BufferDescription((uint)(Marshal.SizeOf<Matrix4x4>() * 2), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

			CreateNormalPipeline();
			CreateInstancedPipeline();

			_commandList = factory.CreateCommandList();
		}

		public static void DisposeResources() {
			if (!initialized) return;
			try
			{
				RenderUtilities.DisposeAllUtils();
				_pipeline_normal.Dispose();
				_pipeline_instanced.Dispose();
				_shaders_normal[0].Dispose();
				_shaders_normal[1].Dispose();
				_shaders_instanced[0].Dispose();
				_shaders_instanced[1].Dispose();
				_commandList.Dispose();
				_graphicsDevice.Dispose();
				_cameraProjViewBuffer.Dispose();
				_sharedResourceSet.Dispose();
				initialized = false;
			}
			catch(Exception ex) {
				Console.WriteLine(ex);
			}
		}
	}

}
