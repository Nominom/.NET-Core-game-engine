﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Veldrid;

namespace Core.Graphics
{
	[RenderSystem(renderStage:RenderStage.RenderOpaques)]
	public class RenderOpaquesSystem : IRenderSystem {

		private ComponentQuery query;
		private DeviceBuffer instanceMatrices;

		public void OnCreate(ECSWorld world) {
			query.Include<ObjectToWorld>();
			query.IncludeShared<MeshRenderer>();
			query.IncludeShared<OpaqueRenderTag>();

			instanceMatrices = GraphicsContext.factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<Matrix4x4>(), BufferUsage.VertexBuffer|BufferUsage.Dynamic));
		}

		public void OnDestroy(ECSWorld world) {
			instanceMatrices.Dispose();
		}

		public void Render(ECSWorld world, in RenderContext context) {


			foreach (var block in world.ComponentManager.GetBlocks(query)) {
				MeshRenderer renderer = block.GetSharedComponentData<MeshRenderer>();
				Mesh mesh = renderer.mesh;
				mesh.LoadSubMeshes();

				var objectToWorld = block.GetComponentData<ObjectToWorld>();

				var cmd = context.CreateCommandList();

				cmd.Begin();

				foreach (SubMesh subMesh in mesh.subMeshes) {

					cmd.SetFramebuffer(context.mainFrameBuffer);
					
					cmd.SetPipeline(GraphicsContext._pipeline_instanced);
					cmd.SetGraphicsResourceSet(0, GraphicsContext._sharedResourceSet);
					cmd.SetVertexBuffer(0, subMesh.vertexBuffer);
					cmd.SetIndexBuffer(subMesh.indexBuffer, IndexFormat.UInt16);
					cmd.SetVertexBuffer(1, instanceMatrices);

					for (int i = 0; i < objectToWorld.Length; i++) {
						ObjectToWorld ow = objectToWorld[i];
						cmd.UpdateBuffer(instanceMatrices,0, ow); //Only for small buffers. Will imply a double copy?

						cmd.DrawIndexed(
							indexCount: subMesh.IndexCount,
							instanceCount: 1,
							indexStart: 0,
							vertexOffset: 0,
							instanceStart: 0);
					}
				}

				cmd.End();
				context.SubmitCommands(cmd);
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
