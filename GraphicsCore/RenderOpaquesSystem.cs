using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics.VulkanBackend;
using Veldrid;

namespace Core.Graphics
{
	[RenderSystem(renderStage: RenderStage.RenderOpaques)]
	public class RenderOpaquesSystem : IRenderSystem
	{

		private ComponentQuery query;
		private List<DeviceBuffer> instanceMatricesBuffers;
		private readonly uint instanceMatricesBufferSize = 16384;
		private Material defaultMaterial;
		private ShaderPair defaultShader;

		public void OnCreate(ECSWorld world)
		{
			query.Include<ObjectToWorld>();
			query.IncludeShared<MeshRenderer>();
			query.IncludeShared<OpaqueRenderTag>();
			defaultShader = ShaderPair.Load(GraphicsContext.graphicsDevice, "data/mesh_instanced.frag.spv", "data/mesh_instanced.vert.spv", ShaderType.Instanced);
			defaultMaterial = new Material(GraphicsContext.graphicsDevice,
				GraphicsContext.uniform0, defaultShader);

			instanceMatricesBuffers = new List<DeviceBuffer>(1);
			GrowInstanceMatrices();
		}

		public void OnDestroy(ECSWorld world)
		{
			foreach (DeviceBuffer buffer in instanceMatricesBuffers) {
				buffer.Dispose();
			}
			instanceMatricesBuffers.Clear();
		}

		private void GrowInstanceMatrices()
		{
			instanceMatricesBuffers.Add(new DeviceBuffer(GraphicsContext.graphicsDevice, instanceMatricesBufferSize,
				BufferUsageFlags.VertexBuffer, BufferMemoryUsageHint.Dynamic));
		}

		private void RenderMeshInstances(MeshRenderer mesh, DeviceBuffer instanceBuffer, uint instanceAmount, uint instanceIndex, RenderContext context) {
			
			
			var cmd = context.GetCommandBuffer();
			cmd.BeginAndContinueRenderPass(context);

			int matIndex = 0;
			foreach (var subMesh in mesh.mesh.subMeshes) {

				var material = 
					mesh.materials.Length == 0
					? defaultMaterial
					:(matIndex < mesh.materials.Length ? mesh.materials[matIndex] : mesh.materials[0]);

				var pipeline = material.GetPipeline();

				cmd.UsePipeline(pipeline);

				if (pipeline.instanced) {
					cmd.BindVertexBuffer(instanceBuffer, Pipeline.VERTEX_INSTANCE_BUFFER_BIND_ID);
					cmd.DrawIndexed(subMesh, instanceAmount, instanceIndex - instanceAmount);
				}
				else {

					//TODO: Non-instanced rendering
				}
				
				
				//cmd.SetFramebuffer(context.mainFrameBuffer);

				//cmd.SetPipeline(GraphicsContext._pipeline_instanced);
				//cmd.SetGraphicsResourceSet(0, GraphicsContext._sharedResourceSet);
				//cmd.SetVertexBuffer(0, subMesh.vertexBuffer);
				//cmd.SetIndexBuffer(subMesh.indexBuffer, IndexFormat.UInt16);
				//cmd.SetVertexBuffer(1, instanceMatrices);

				//cmd.DrawIndexed(
				//	indexCount: subMesh.IndexCount,
				//	instanceCount: instanceAmount,
				//	indexStart: 0,
				//	vertexOffset: 0,
				//	instanceStart: instanceIndex - instanceAmount);
			}

			cmd.End();
			context.SubmitCommands(cmd);
			//GraphicsContext._graphicsDevice.WaitForIdle();
		}

		public unsafe void Render(ECSWorld world, in RenderContext context)
		{

			var blocks = world.ComponentManager.GetBlocks(query);

			MeshRenderer lastMeshRenderer = null;
			int bufferIndex = 0;
			uint instanceAmount = 0;
			uint instanceIndex = 0;

			foreach (var block in blocks)
			{
				MeshRenderer renderer = block.GetSharedComponentData<MeshRenderer>();

				if (lastMeshRenderer == null)
				{
					lastMeshRenderer = renderer;
					instanceAmount = 0;
				}
				else if (renderer.mesh != lastMeshRenderer.mesh)
				{
					RenderMeshInstances(lastMeshRenderer, instanceMatricesBuffers[bufferIndex], instanceAmount, instanceIndex, context);

					lastMeshRenderer = renderer;
					instanceAmount = 0;
				}

				if (instanceIndex + block.length > instanceMatricesBufferSize) {
					RenderMeshInstances(renderer, instanceMatricesBuffers[bufferIndex], instanceAmount, instanceIndex, context);

					bufferIndex++;
					if (bufferIndex == instanceMatricesBuffers.Count) {
						GrowInstanceMatrices();
					}
					instanceAmount = 0;
					instanceIndex = 0;
				}

				var objectToWorld = block.GetReadOnlyComponentData<ObjectToWorld>();

				instanceMatricesBuffers[bufferIndex].SetData(objectToWorld, instanceIndex * (uint)Marshal.SizeOf<ObjectToWorld>());
				//context.UpdateBuffer(instanceMatrices[bufferIndex], instanceIndex * (uint)Marshal.SizeOf<ObjectToWorld>(), objectToWorld);
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

			if (lastMeshRenderer != null && instanceAmount > 0) {
				RenderMeshInstances(lastMeshRenderer, instanceMatricesBuffers[bufferIndex], instanceAmount, instanceIndex, context);
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
