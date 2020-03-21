using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Assimp;
using Core.ECS;
using Core.Shared;
using GlmSharp;

namespace Core.AssetSystem
{
	public static class ModelLoader
	{

		public static MeshData LoadModel(Stream file) {
			PostProcessSteps assimpFlags = PostProcessSteps.FlipWindingOrder | PostProcessSteps.Triangulate | PostProcessSteps.PreTransformVertices
				| PostProcessSteps.GenerateUVCoords | PostProcessSteps.GenerateNormals;

			string extension = null;
			if (file is FileStream fs) {
				extension = Path.GetExtension(fs.Name);
			}
			var scene = new AssimpContext().ImportFileFromStream(file, assimpFlags, extension);

			// Generate vertex buffer from ASSIMP scene data
			float scale = 1.0f;

			MeshData newMesh = new MeshData();
			newMesh.subMeshes = new SubMeshData[scene.MeshCount];

			for (int m = 0; m < scene.MeshCount; m++)
			{
				Mesh mesh = scene.Meshes[m];
				Vertex[] vertices = new Vertex[mesh.VertexCount];
				Face[] faces = mesh.Faces.Where(x => x.IndexCount == 3).ToArray(); // Remove any degenerate faces
				UInt32[] indices = new UInt32[faces.Length * 3];

				//DebugHelper.AssertThrow<OverflowException>(faces.Length * 3 <= UInt32.MaxValue);

				for (int v = 0; v < mesh.VertexCount; v++)
				{
					Vertex vertex;

					vertex.position = new vec3(mesh.Vertices[v].X, mesh.Vertices[v].Y, mesh.Vertices[v].Z) * scale;
					vertex.normal = new vec3(mesh.Normals[v].X, mesh.Normals[v].Y, mesh.Normals[v].Z);
					if (mesh.HasTextureCoords(0)) {
						vertex.uv0 = new vec2(mesh.TextureCoordinateChannels[0][v].X, mesh.TextureCoordinateChannels[0][v].Y);
					}
					else {
						vertex.uv0 = vec2.Zero;
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
