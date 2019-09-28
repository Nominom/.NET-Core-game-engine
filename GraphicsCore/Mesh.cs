using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualBasic;
using Veldrid;

namespace Core.Graphics
{
	public class Mesh : IDisposable {
		public SubMesh[] subMeshes;

		public Mesh(params SubMesh[] subMeshes) {
			this.subMeshes = subMeshes;
		}

		public void LoadSubMeshes() {
			foreach (SubMesh subMesh in subMeshes) {
				if (!subMesh.IsLoaded) {
					subMesh.LoadToGpu();
				}
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
			foreach (SubMesh subMesh in subMeshes) {
				subMesh.Dispose();
			}
		}
	}
}
