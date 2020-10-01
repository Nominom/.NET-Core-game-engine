using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.AssetSystem;
using Core.AssetSystem.Assets;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics.VulkanBackend;

namespace Core.Graphics.RenderSystems
{
	[RenderSystem(renderStage: RenderStage.RenderPostProcessing)]
	public class DebugMeshConvexHullRenderSystem : IRenderSystem
	{

		private ComponentQuery query;
		private List<DeviceBuffer> instanceMatricesBuffers;
		private readonly uint instanceMatricesBufferSize = 262144; // 256kib
		private Material defaultMaterial;
		private ShaderPair defaultShader;

		public void OnCreate(ECSWorld world)
		{
			query.IncludeReadonly<ObjectToWorld>();
			query.IncludeShared<DebugMeshConvexHullRenderer>();
			query.ExcludeShared<CulledRenderTag>();
			
			var fragShader = Asset.Load<ShaderAsset>("mesh_instanced_default.frag");
			var vertShader = Asset.Load<ShaderAsset>("mesh_instanced_default.vert");
			var shader = new ShaderPipeline(fragShader, vertShader, ShaderType.Instanced);
			defaultShader = shader.ShaderPair;

			defaultMaterial = new Material(GraphicsContext.graphicsDevice,
				GraphicsContext.uniform0, defaultShader);
			defaultMaterial.wireframe = true;

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

		private void RenderMeshInstances(CommandBuffer cmd, DebugMeshConvexHullRenderer mesh, DeviceBuffer instanceBuffer, uint instanceAmount, uint instanceIndex, RenderContext context) {
			

			int matIndex = 0;
			foreach (var subMesh in mesh.mesh.subMeshes) {

				var material = defaultMaterial;
					

				var pipeline = material.GetPipeline();

				cmd.UsePipeline(pipeline);

				if (pipeline.instanced) {
					cmd.BindVertexBuffer(instanceBuffer, Pipeline.VERTEX_INSTANCE_BUFFER_BIND_ID);
					cmd.DrawIndexed(subMesh, instanceAmount, instanceIndex - instanceAmount);
				}
				else {

					//TODO: Non-instanced rendering
				}
				
				
			}
		}

		public unsafe void Render(ECSWorld world, in RenderContext context)
		{

			var cmd = context.GetCommandBuffer();
			cmd.BeginAndContinueRenderPass(context);

			var blocks = world.ComponentManager.GetBlocks(query);

			DebugMeshConvexHullRenderer lastMeshRenderer = null;
			int bufferIndex = 0;
			uint instanceAmount = 0;
			uint instanceIndex = 0;

			foreach (var block in blocks)
			{
				DebugMeshConvexHullRenderer renderer = block.GetSharedComponentData<DebugMeshConvexHullRenderer>();

				if (lastMeshRenderer == null)
				{
					lastMeshRenderer = renderer;
					instanceAmount = 0;
				}
				else if (renderer.mesh != lastMeshRenderer.mesh)
				{
					RenderMeshInstances(cmd, lastMeshRenderer, instanceMatricesBuffers[bufferIndex], instanceAmount, instanceIndex, context);

					lastMeshRenderer = renderer;
					instanceAmount = 0;
				}

				if ((instanceIndex + block.length) * Marshal.SizeOf<ObjectToWorld>() >= instanceMatricesBufferSize) {
					if (instanceAmount > 0) {
						RenderMeshInstances(cmd, renderer, instanceMatricesBuffers[bufferIndex], instanceAmount, instanceIndex, context);
					}

					bufferIndex++;
					if (bufferIndex == instanceMatricesBuffers.Count) {
						GrowInstanceMatrices();
					}
					instanceAmount = 0;
					instanceIndex = 0;
				}

				var objectToWorld = block.GetReadOnlyComponentData<ObjectToWorld>();

				instanceMatricesBuffers[bufferIndex].SetData(objectToWorld, instanceIndex * (uint)Marshal.SizeOf<ObjectToWorld>());
				instanceAmount += (uint) block.length;
				instanceIndex += (uint) block.length;

			}

			if (lastMeshRenderer != null && instanceAmount > 0) {
				RenderMeshInstances(cmd, lastMeshRenderer, instanceMatricesBuffers[bufferIndex], instanceAmount, instanceIndex, context);
			}

			cmd.End();
			context.SubmitCommands(cmd);
			
		}
	}
}
