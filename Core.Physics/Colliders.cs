using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Core.AssetSystem;
using Core.AssetSystem.Assets;
using Core.ECS;
using Core.Shared;
using GlmSharp;

namespace Core.Physics
{
	public struct BoxCollider : IComponent {
		public float width;
		public float height;
		public float length;
	}

	public class MeshCollider : ISharedComponent {
		public MeshData mesh;
		public bool convex;
		public MeshCollider(MeshData mesh, bool convex = false) {
			this.mesh = mesh;
			this.convex = convex;
		}

		private Buffer<Triangle>? triangles;
		public Buffer<Triangle> GetTriangles() {

			if (triangles == null) {

				if (!convex) {
					int totalTriangles = mesh.subMeshes.Sum(x => x.indices.Length / 3);
					PhysicsSystem.BufferPool.Take<Triangle>(totalTriangles, out var buffer);

					int nextIdx = 0;
					foreach (SubMeshData subMesh in mesh.subMeshes) {
						for (int i = 0; i < subMesh.indices.Length; i += 3) {
							//Bepuphysics uses flipped winding order.
							vec3 a = subMesh.vertices[subMesh.indices[i]].position;
							vec3 b = subMesh.vertices[subMesh.indices[i+2]].position;
							vec3 c = subMesh.vertices[subMesh.indices[i+1]].position;
							Triangle triangle = new Triangle(
								new Vector3(a.x, a.y, a.z),
								new Vector3(b.x, b.y, b.z),
								new Vector3(c.x, c.y, c.z)
							);
							buffer[nextIdx++] = triangle;
						}
					}
					triangles = buffer;
				}
				else {
					var convexMesh = mesh.ConvexHull;
					PhysicsSystem.BufferPool.Take<Triangle>(convexMesh.indices.Length / 3, out var buffer);
					int nextIdx = 0;
					for (int i = 0; i < convexMesh.indices.Length; i += 3) {
						//Bepuphysics uses flipped winding order.
						vec3 a = convexMesh.vertices[convexMesh.indices[i]].position;
						vec3 b = convexMesh.vertices[convexMesh.indices[i+2]].position;
						vec3 c = convexMesh.vertices[convexMesh.indices[i+1]].position;
						Triangle triangle = new Triangle(
							new Vector3(a.x, a.y, a.z),
							new Vector3(b.x, b.y, b.z),
							new Vector3(c.x, c.y, c.z)
						);
						buffer[nextIdx++] = triangle;
					}

					triangles = buffer;
				}
			}

			return triangles.GetValueOrDefault();
		}
	}

	internal struct InternalColliderHandle : IComponent {
		public TypedIndex shapeIdx;
		public BodyInertia inertia;
	}
}
