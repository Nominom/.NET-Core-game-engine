using System;

namespace Core.Shared
{
	public class MeshData {
		public SubMeshData[] subMeshes;
	}

	public class SubMeshData {
		public Vertex[] vertices;
		public UInt32[] indices;

		public SubMeshData(Vertex[] vertices, UInt32[] indices) {
			this.vertices = vertices;
			this.indices = indices;
		}
	}
}
