using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Core.Graphics;
using Core.ECS;
using Core.Graphics.VulkanBackend;

namespace Core.Graphics
{
	internal static class GraphicsContext
	{
		public static GraphicsDevice graphicsDevice;
		public static UniformBuffer<UniformBufferObject> uniform0;
		public static ShaderPair defaultShader;
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
			
			graphicsDevice = new GraphicsDevice(false);
			graphicsDevice.ConnectToWindow(Window.window.SdlWindowHandle);
			graphicsDevice.SetupFrameBuffersAndRenderPass();

			CreateResources();

			Window.OnWindowResize += Resized;
		}

		private static void Resized(int width, int height) {
			graphicsDevice.RecreateSwapchainAndFrameBuffers();
		}


		public static void CreateResources() {
			uniform0 = new UniformBuffer<UniformBufferObject>(graphicsDevice, 0);
			defaultShader = ShaderPair.Load(GraphicsContext.graphicsDevice, "data/mesh_instanced.frag.spv", "data/mesh_instanced.vert.spv", ShaderType.Instanced);
		}

		public static void DisposeResources() {
			if (!initialized) return;
			try
			{
				RenderUtilities.DisposeAllUtils();
				uniform0.Dispose();
				graphicsDevice.Dispose();
				initialized = false;
			}
			catch(Exception ex) {
				Console.WriteLine(ex);
			}
		}
	}

}
