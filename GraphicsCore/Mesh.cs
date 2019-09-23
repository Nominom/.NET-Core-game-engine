using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using GraphicsCore;
using Microsoft.VisualBasic;
using Veldrid;

namespace Core.Graphics
{
	public class Mesh : IDisposable {
		public SubMesh[] subMeshes;

		public Mesh(params SubMesh[] subMeshes) {
			this.subMeshes = subMeshes;
		}

		public void Dispose() {
			if (subMeshes == null) return;
			foreach (SubMesh subMesh in subMeshes) {
				subMesh.Dispose();
			}
		}
	}
}
