using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Core.Graphics.VulkanBackend;
using Core.Shared;
using Microsoft.VisualBasic;
using Veldrid;

namespace Core.Graphics
{
	public class Mesh : IDisposable {
		public GpuMesh[] subMeshes;

		public Mesh(GraphicsDevice device, MeshData meshData) {
			subMeshes = new GpuMesh[meshData.subMeshes.Length];
			for (int i = 0; i < meshData.subMeshes.Length; i++) {
				var subMesh = meshData.subMeshes[i];
				subMeshes[i] = new GpuMesh(device, subMesh.vertices, subMesh.indices);
			}
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
