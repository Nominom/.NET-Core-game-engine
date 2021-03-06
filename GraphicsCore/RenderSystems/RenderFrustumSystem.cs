﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Core.AssetSystem;
using Core.AssetSystem.Assets;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics.VulkanBackend;
using Core.Shared;
using GlmSharp;

namespace Core.Graphics.RenderSystems
{
	
	//[RenderSystem(RenderStage.RenderPostProcessing)]
	public class RenderFrustumSystem : IRenderSystem
	{
		private DeviceBuffer instanceMatrix;
		private Mesh defaultMesh;
		private Material defaultMaterial;
		private ShaderPair defaultShader;
		private vec3[] frustumCorners = new vec3[8];
		private Vertex[] vertices = new Vertex[8];
		private uint[] indices = new uint[] {
			//near
			0, 1, 2,
			1, 2, 3,
			//far
			4, 5, 6,
			5, 6, 7
		};

		public void OnCreate(ECSWorld world) {
			var fragShader = Asset.Load<ShaderAsset>("mesh_instanced_default.frag");
			var vertShader = Asset.Load<ShaderAsset>("mesh_instanced_default.vert");
			var shader = new ShaderPipeline(fragShader, vertShader, ShaderType.Instanced);
			defaultShader = shader.ShaderPair;

			defaultMaterial = new Material(GraphicsContext.graphicsDevice,
				GraphicsContext.uniform0, defaultShader);
			defaultMaterial.wireframe = true;

			instanceMatrix = new DeviceBuffer(GraphicsContext.graphicsDevice, (ulong)Unsafe.SizeOf<ObjectToWorld>(),
				BufferUsageFlags.VertexBuffer, BufferMemoryUsageHint.Dynamic);
			

			var matrix = mat4.Identity;
			var normalMatrix = new mat3(matrix);

			ObjectToWorld matrices = new ObjectToWorld() {
				model = matrix,
				normal = normalMatrix
			};

			instanceMatrix.SetData(matrices, 0);

			MeshData initialData = new MeshData();
			initialData.subMeshes = new SubMeshData[1];
			initialData.subMeshes[0] = new SubMeshData(vertices, indices);

			defaultMesh = new Mesh(GraphicsContext.graphicsDevice, initialData, true);
		}

		public void OnDestroy(ECSWorld world) {
			instanceMatrix.Dispose();
		}

		private void RenderMesh(Mesh mesh, Material material, DeviceBuffer instanceBuffer, RenderContext context) {
			
			
			var cmd = context.GetCommandBuffer();
			cmd.BeginAndContinueRenderPass(context);

			foreach (var subMesh in mesh.subMeshes) {

				var pipeline = material.GetPipeline();

				cmd.UsePipeline(pipeline);

				if (pipeline.instanced) {
					cmd.BindVertexBuffer(instanceBuffer, Pipeline.VERTEX_INSTANCE_BUFFER_BIND_ID);
					cmd.DrawIndexed(subMesh, 1, 0);
				}
			}

			cmd.End();
			context.SubmitCommands(cmd);
		}

		public void Render(ECSWorld world, in RenderContext context) {
			//var frustum = new Frustum();
			var frustum = context.activeCamera.GetFrustum(context.cameraPosition, context.cameraRotation);
			
			frustum.Vertices(frustumCorners);

			for (int i = 0; i < frustumCorners.Length; i++) {
				vertices[i] = new Vertex(frustumCorners[i], vec3.UnitY, vec2.Zero);
			}
			defaultMesh.subMeshes[0].vertices.SetData(vertices, 0);

			RenderMesh(defaultMesh, defaultMaterial, instanceMatrix, context);
		}
	}
}
