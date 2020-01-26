using System.Collections.Generic;
using System.Runtime.InteropServices;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics.VulkanBackend;

namespace Core.Graphics.RenderSystems
{
	[RenderSystem(renderStage: RenderStage.RenderOpaques)]
	public class RenderOpaquesSystem : IRenderSystem
	{

		private ComponentQuery query;
		private List<DeviceBuffer> instanceMatricesBuffers;
		private readonly uint instanceMatricesBufferSize = 262144; // 256kib
		private Material defaultMaterial;
		private ShaderPair defaultShader;

		public void OnCreate(ECSWorld world)
		{
			query.Include<ObjectToWorld>();
			query.IncludeShared<MeshRenderer>();
			query.IncludeShared<OpaqueRenderTag>();
			query.ExcludeShared<CulledRenderTag>();
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

		private void RenderMeshInstances(CommandBuffer cmd, MeshRenderer mesh, DeviceBuffer instanceBuffer, uint instanceAmount, uint instanceIndex, RenderContext context) {
			

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
				
				
			}
		}

		public unsafe void Render(ECSWorld world, in RenderContext context)
		{

			var cmd = context.GetCommandBuffer();
			cmd.BeginAndContinueRenderPass(context);

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
