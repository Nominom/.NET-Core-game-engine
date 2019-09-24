using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Veldrid;

namespace Core.Graphics
{
	[ECSSystem(UpdateEvent.Render)]
	public class RenderSystem : ISystem
	{
		public bool Enabled { get; set; }
		public void OnCreateSystem(ECSWorld world) {
		}

		public void OnDestroySystem(ECSWorld world) {
		}

		public void OnEnableSystem(ECSWorld world) {
		}

		public void OnDisableSystem(ECSWorld world) {
		}

		public void Update(float deltaTime, ECSWorld world) {
			if (!GraphicsContext.initialized) return;

			Mesh quad = RenderUtilities.FullScreenQuad;

			//if (!quad.IsLoaded) {
			//	quad.LoadToGpu();
			//}


			GraphicsContext._commandList.Begin();
			GraphicsContext._commandList.SetFramebuffer(GraphicsContext._graphicsDevice.SwapchainFramebuffer);
			GraphicsContext._commandList.ClearColorTarget(0, RgbaFloat.Black);

			//GraphicsContext._commandList.SetVertexBuffer(0, quad.vertexBuffer);
			//GraphicsContext._commandList.SetIndexBuffer(quad.indexBuffer, IndexFormat.UInt16);
			//GraphicsContext._commandList.SetPipeline(GraphicsContext._pipeline);
			//GraphicsContext._commandList.DrawIndexed(
			//	indexCount: quad.IndexCount,
			//	instanceCount: 1,
			//	indexStart: 0,
			//	vertexOffset: 0,
			//	instanceStart: 0);

			GraphicsContext._commandList.End();
			GraphicsContext._graphicsDevice.SubmitCommands(GraphicsContext._commandList);
			GraphicsContext._graphicsDevice.SwapBuffers();
		}
	}
}
