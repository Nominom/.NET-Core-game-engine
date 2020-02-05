using System;
using System.Linq;
using System.Numerics;
using Assimp;
using Core.ECS;
using Core.Shared;

namespace Core.AssetSystem
{
	public static class ModelLoader
	{

		public static MeshData LoadModel(string filename) {
			PostProcessSteps assimpFlags = PostProcessSteps.FlipWindingOrder | PostProcessSteps.Triangulate | PostProcessSteps.PreTransformVertices
				| PostProcessSteps.GenerateUVCoords | PostProcessSteps.GenerateNormals;

			var scene = new AssimpContext().ImportFile(filename, assimpFlags);

			// Generate vertex buffer from ASSIMP scene data
			float scale = 1.0f;

			MeshData newMesh = new MeshData();
			newMesh.subMeshes = new SubMeshData[scene.MeshCount];

			for (int m = 0; m < scene.MeshCount; m++)
			{
				Mesh mesh = scene.Meshes[m];
				Vertex[] vertices = new Vertex[mesh.VertexCount];
				Face[] faces = mesh.Faces.Where(x => x.IndexCount == 3).ToArray(); // Remove any degenerate faces
				UInt16[] indices = new UInt16[faces.Length * 3];

				DebugHelper.AssertThrow<OverflowException>(faces.Length * 3 <= UInt16.MaxValue);

				for (int v = 0; v < mesh.VertexCount; v++)
				{
					Vertex vertex;

					vertex.position = new Vector3(mesh.Vertices[v].X, mesh.Vertices[v].Y, mesh.Vertices[v].Z) * scale;
					vertex.normal = new Vector3(mesh.Normals[v].X, mesh.Normals[v].Y, mesh.Normals[v].Z);
					if (mesh.HasTextureCoords(0)) {
						vertex.uv0 = new Vector2(mesh.TextureCoordinateChannels[0][v].X, mesh.TextureCoordinateChannels[0][v].Y);
					}
					else {
						vertex.uv0 = Vector2.Zero;
					}

					vertices[v] = vertex;
				}

				for (int i = 0; i < faces.Length; i++) {
					indices[i * 3 + 0] = (UInt16)faces[i].Indices[0];
					indices[i * 3 + 1] = (UInt16)faces[i].Indices[1];
					indices[i * 3 + 2] = (UInt16)faces[i].Indices[2];
				}

				newMesh.subMeshes[m] = new SubMeshData(vertices, indices);
			}

			return newMesh;
		}
	}
}
