using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics.VulkanBackend;
using Core.Shared;
using GlmSharp;

namespace Core.Graphics.RenderSystems
{

	[RenderSystem(RenderStage.RenderPostProcessing)]
	public class RenderAabbsSystem : IRenderSystem
	{
		
		private ComponentQuery query;
		private List<DeviceBuffer> instanceMatricesBuffers;
		private readonly uint instanceMatricesBufferSize = 262144; // 256kib
		private Mesh defaultMesh;
		private Material defaultMaterial;
		private ShaderPair defaultShader;

		public void OnCreate(ECSWorld world) {
			query.IncludeReadonly<BoundingBox>();
			query.ExcludeShared<CulledRenderTag>();
			
			defaultShader = ShaderPair.Load(GraphicsContext.graphicsDevice, "data/mesh_instanced.frag.spv", "data/mesh_instanced.vert.spv", ShaderType.Instanced);
			defaultMaterial = new Material(GraphicsContext.graphicsDevice,
				GraphicsContext.uniform0, defaultShader);
			defaultMaterial.wireframe = true;

			defaultMesh = RenderUtilities.UnitCube;

			instanceMatricesBuffers = new List<DeviceBuffer>(1);
			GrowInstanceMatrices();
		}

		public void OnDestroy(ECSWorld world) {
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

		private void RenderMeshInstances(CommandBuffer cmd, Mesh mesh, Material material, DeviceBuffer instanceBuffer, uint instanceAmount, uint instanceIndex, RenderContext context) {
			
			foreach (var subMesh in mesh.subMeshes) {

				var pipeline = material.GetPipeline();

				cmd.UsePipeline(pipeline);

				if (pipeline.instanced) {
					cmd.BindVertexBuffer(instanceBuffer, Pipeline.VERTEX_INSTANCE_BUFFER_BIND_ID);
					cmd.DrawIndexed(subMesh, instanceAmount, instanceIndex - instanceAmount);
				}
			}

		}

		public void Render(ECSWorld world, in RenderContext context) {
			var blocks = world.ComponentManager.GetBlocks(query);

			var cmd = context.GetCommandBuffer();
			cmd.BeginAndContinueRenderPass(context);

			int bufferIndex = 0;
			uint instanceAmount = 0;
			uint instanceIndex = 0;

			foreach (var block in blocks)
			{
				
				var boundingBox = block.GetReadOnlyComponentData<BoundingBox>();

				for (int i = 0; i < block.length; i++) {
					if ((instanceIndex+1) * Marshal.SizeOf<ObjectToWorld>() >= instanceMatricesBufferSize) {
						RenderMeshInstances(cmd, defaultMesh, defaultMaterial, instanceMatricesBuffers[bufferIndex], instanceAmount, instanceIndex, context);

						bufferIndex++;
						if (bufferIndex == instanceMatricesBuffers.Count) {
							GrowInstanceMatrices();
						}
						instanceAmount = 0;
						instanceIndex = 0;
					}

					var scale = mat4.Scale(boundingBox[i].value.Size);

					var matrix = mat4.Translate(boundingBox[i].value.Center) * scale;

					var inverse = matrix.Inverse;
					var normalMatrix = new mat3(inverse.Transposed);

					ObjectToWorld matrices = new ObjectToWorld() {
						model = matrix,
						normal = normalMatrix
					};

					instanceMatricesBuffers[bufferIndex].SetData(matrices, instanceIndex * (uint)Marshal.SizeOf<ObjectToWorld>());
					instanceAmount++;
					instanceIndex++;
				}
			}

			if (instanceAmount > 0) {
				RenderMeshInstances(cmd, defaultMesh, defaultMaterial, instanceMatricesBuffers[bufferIndex], instanceAmount, instanceIndex, context);
			}

			cmd.End();
			context.SubmitCommands(cmd);
		}
	}
}
