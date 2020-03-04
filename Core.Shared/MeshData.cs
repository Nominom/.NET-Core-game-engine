using System;
using System.Linq;
using GlmSharp;

namespace Core.Shared
{
	public class MeshData {
		public SubMeshData[] subMeshes;
		private SubMeshData _convexHull;

		public SubMeshData ConvexHull {
			get {
				if (_convexHull == null) {
					ConvexHullGenerator gen = new ConvexHullGenerator();
					var points = subMeshes.SelectMany(x => x.vertices)
						.Select(x => x.position).ToList();
					gen.GenerateHull(points, false, out var verts,
						out var indices, out var normals);

					var vertices = new Vertex[verts.Count];
					for (int i = 0; i < vertices.Length; i++) {
						vertices[i] = new Vertex(verts[i], vec3.UnitY, vec2.Zero);
					}
					_convexHull = new SubMeshData(vertices, indices.ToArray());
				}

				return _convexHull;
			}
		}
	}

	public class SubMeshData {
		public Vertex[] vertices;
		public UInt32[] indices;

		public SubMeshData(Vertex[] vertices, UInt32[] indices) {
			this.vertices = vertices;
			this.indices = indices;
		}


		public void RecalculateNormals(bool smooth) {
			throw new NotImplementedException();
		}
	}
}
