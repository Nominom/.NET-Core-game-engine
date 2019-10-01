using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Veldrid;

namespace Core.Graphics
{
	[RenderSystem(renderStage: RenderStage.RenderOpaques)]
	public class RenderOpaquesSystem : IRenderSystem
	{

		private ComponentQuery query;
		private DeviceBuffer instanceMatrices;
		private readonly uint instanceMatricesBufferSize = 16384;

		public void OnCreate(ECSWorld world)
		{
			query.Include<ObjectToWorld>();
			query.IncludeShared<MeshRenderer>();
			query.IncludeShared<OpaqueRenderTag>();

			instanceMatrices = GraphicsContext.factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<Matrix4x4>() * instanceMatricesBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
		}

		public void OnDestroy(ECSWorld world)
		{
			instanceMatrices.Dispose();
		}

		//private void GrowInstanceMatrices(int newAmount) {
		//	instanceMatrices?.Dispose();

		//	instanceMatrices = GraphicsContext.factory.CreateBuffer(
		//		new BufferDescription((uint) (Marshal.SizeOf<Matrix4x4>() * newAmount), BufferUsage.VertexBuffer|BufferUsage.Dynamic));
		//	instanceMatricesBufferSize = newAmount;
		//}

		private void RenderMeshInstances(Mesh mesh, uint instanceAmount, uint instanceIndex, RenderContext context)
		{
			var cmd = context.CreateCommandList();
			cmd.Begin();

			foreach (SubMesh subMesh in mesh.subMeshes)
			{

				cmd.SetFramebuffer(context.mainFrameBuffer);

				cmd.SetPipeline(GraphicsContext._pipeline_instanced);
				cmd.SetGraphicsResourceSet(0, GraphicsContext._sharedResourceSet);
				cmd.SetVertexBuffer(0, subMesh.vertexBuffer);
				cmd.SetIndexBuffer(subMesh.indexBuffer, IndexFormat.UInt16);
				cmd.SetVertexBuffer(1, instanceMatrices);

				cmd.DrawIndexed(
					indexCount: subMesh.IndexCount,
					instanceCount: instanceAmount,
					indexStart: 0,
					vertexOffset: 0,
					instanceStart: instanceIndex - instanceAmount);
			}

			cmd.End();
			context.SubmitCommands(cmd);
			//GraphicsContext._graphicsDevice.WaitForIdle();
		}

		public unsafe void Render(ECSWorld world, in RenderContext context)
		{

			var blocks = world.ComponentManager.GetBlocks(query);

			MeshRenderer lastMeshRenderer = null;
			Mesh mesh = null;
			uint instanceAmount = 0;
			uint instanceIndex = 0;

			foreach (var block in blocks)
			{
				MeshRenderer renderer = block.GetSharedComponentData<MeshRenderer>();

				if (lastMeshRenderer == null)
				{
					mesh = renderer.mesh;
					mesh.LoadSubMeshes();
					lastMeshRenderer = renderer;
					instanceAmount = 0;
				}
				else if (renderer.mesh != mesh)
				{
					RenderMeshInstances(mesh, instanceAmount, instanceIndex, context);

					mesh = renderer.mesh;
					mesh.LoadSubMeshes();
					lastMeshRenderer = renderer;
					instanceAmount = 0;
				}

				if (instanceIndex + block.length > instanceMatricesBufferSize)
				{
					//Clear buffer and wait for idle, before rendering new things
					RenderMeshInstances(mesh, instanceAmount, instanceIndex, context);
					GraphicsContext._graphicsDevice.WaitForIdle();
					instanceAmount = 0;
					instanceIndex = 0;
				}

				var objectToWorld = block.GetReadOnlyComponentData<ObjectToWorld>();

				context.UpdateBuffer(instanceMatrices, instanceIndex * (uint)Marshal.SizeOf<ObjectToWorld>(), objectToWorld);
				instanceAmount += (uint) block.length;
				instanceIndex += (uint) block.length;

				//fixed (ObjectToWorld* ptr = objectToWorld)
				//{
				//	IntPtr p = new IntPtr(ptr);

				//	cmd.UpdateBuffer(instanceMatrices, 0, p, (uint)(objectToWorld.Length * Marshal.SizeOf<ObjectToWorld>()));
				//}

				//if (instanceAmount > 2048) {
				//	RenderMeshInstances(mesh, instanceAmount, instanceIndex, context);
				//	instanceAmount = 0;
				//}



				//context.UpdateBuffer(instanceMatrices, 0, objectToWorld);
			}

			if (mesh != null && instanceAmount > 0) {
				RenderMeshInstances(mesh, instanceAmount, instanceIndex, context);
			}


			/*
			Mesh quad = RenderUtilities.FullScreenQuad;

			if (!quad.subMeshes[0].IsLoaded)
			{
				quad.subMeshes[0].LoadToGpu();
			}

			var cmd = context.CreateCommandList();
			cmd.Begin();
			cmd.SetFramebuffer(context.mainFrameBuffer);

			cmd.SetVertexBuffer(0, quad.subMeshes[0].vertexBuffer);
			cmd.SetIndexBuffer(quad.subMeshes[0].indexBuffer, IndexFormat.UInt16);
			cmd.SetPipeline(GraphicsContext._pipeline_normal);
			cmd.DrawIndexed(
				indexCount: quad.subMeshes[0].IndexCount,
				instanceCount: 1,
				indexStart: 0,
				vertexOffset: 0,
				instanceStart: 0);

			cmd.End();
			context.SubmitCommands(cmd);
			*/

			//GraphicsContext._commandList.Begin();
			//GraphicsContext._commandList.SetFramebuffer(GraphicsContext._graphicsDevice.SwapchainFramebuffer);


			//GraphicsContext._commandList.SetVertexBuffer(0, quad.subMeshes[0].vertexBuffer);
			//GraphicsContext._commandList.SetIndexBuffer(quad.subMeshes[0].indexBuffer, IndexFormat.UInt16);
			//GraphicsContext._commandList.SetPipeline(GraphicsContext._pipeline_normal);
			//GraphicsContext._commandList.DrawIndexed(
			//	indexCount: quad.subMeshes[0].IndexCount,
			//	instanceCount: 1,
			//	indexStart: 0,
			//	vertexOffset: 0,
			//	instanceStart: 0);

			//GraphicsContext._commandList.End();
			//GraphicsContext._graphicsDevice.SubmitCommands(GraphicsContext._commandList);

		}
	}
}
