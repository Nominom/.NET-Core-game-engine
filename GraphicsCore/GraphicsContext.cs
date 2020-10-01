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
		internal static bool initialized;


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
