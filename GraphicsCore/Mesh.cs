using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Core.AssetSystem;
using Core.Graphics.VulkanBackend;
using Core.Shared;
using Microsoft.VisualBasic;
using Veldrid;

namespace Core.Graphics
{
	public class Mesh : IDisposable {
		public GpuMesh[] subMeshes;
		public AabbBounds bounds;
		public readonly bool readWrite;

		public Mesh(GraphicsDevice device, MeshData meshData, bool readWrite = false) {
			this.readWrite = readWrite;
			subMeshes = new GpuMesh[meshData.subMeshes.Length];
			bounds = new AabbBounds(meshData.subMeshes[0].vertices[0].position);

			for (int i = 0; i < meshData.subMeshes.Length; i++) {
				var subMesh = meshData.subMeshes[i];
				subMeshes[i] = new GpuMesh(device, subMesh.vertices, subMesh.indices, readWrite);
				foreach (var vertex in subMesh.vertices) {
					bounds = bounds.Encapsulate(vertex.position);
				}
			}
		}

		public static Mesh Create(AssetReference<MeshAsset> asset, bool readWrite = false) {
			if (!asset.IsLoaded)
			{
				asset.LoadNow();
			}
			MeshData meshData = asset.Get().GetMeshData();

			return new Mesh(GraphicsContext.graphicsDevice, meshData, readWrite);
		}

		~Mesh() {
			Free();
		}

		public void Dispose() {
			Free();
			GC.SuppressFinalize(this);
		}

		private void Free() {
			if (subMeshes == null) return;
			foreach (GpuMesh subMesh in subMeshes) {
				subMesh.Dispose();
			}
		}
	}
}
