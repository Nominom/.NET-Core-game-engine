using System;

namespace Core.Shared
{
	public class MeshData {
		public SubMeshData[] subMeshes;
	}

	public class SubMeshData {
		public Vertex[] vertices;
		public UInt16[] indices;

		public SubMeshData(Vertex[] vertices, UInt16[] indices) {
			this.vertices = vertices;
			this.indices = indices;
		}
	}
}
