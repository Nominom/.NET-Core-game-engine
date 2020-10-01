using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assimp;
using Assimp.Configs;
using Core.AssetSystem.Assets;
using Core.Shared;
using GlmSharp;

namespace AssetPackager.DefaultAssetWriters
{
	public class MeshAssetWriter : AssetWriter<MeshAsset>
	{
		public override IEnumerable<string> GetAssociatedInputExtensions() {
			yield return ".obj";
			yield return ".fbx";
			yield return ".dae";
			yield return ".3ds";
			yield return ".gltf";
			yield return ".glb";
			yield return ".blend";
			yield return ".xgl";
			yield return ".zgl";
			yield return ".ply";
			yield return ".stl";
		}

		public override void GetDefaultMeta(IAssetMeta meta) {
			
		}

		public override void LoadAndWriteToStream(FileInfo inputFile, IAssetMeta meta, Stream outputStream) {
			PostProcessSteps assimpFlags = PostProcessSteps.FlipWindingOrder
										   | PostProcessSteps.Triangulate
										   | PostProcessSteps.PreTransformVertices
										   | PostProcessSteps.GenerateUVCoords
										   | PostProcessSteps.GenerateSmoothNormals
										   | PostProcessSteps.FlipUVs
										   ;

			var context = new AssimpContext();
			context.SetConfig(new FloatPropertyConfig("AI_CONFIG_PP_GSN_MAX_SMOOTHING_ANGLE", 80f));

			string extension = inputFile.Extension;
			using FileStream fs = inputFile.OpenRead();

			var scene = context.ImportFileFromStream(fs, assimpFlags, extension);

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


			//Write mesh to stream
			using BinaryWriter writer = new BinaryWriter(outputStream, Encoding.UTF8, true);
			writer.Write(newMesh.subMeshes.Length);
			foreach (SubMeshData subMesh in newMesh.subMeshes) {
				writer.Write(subMesh.vertices.Length);
				Span<byte> verts = new Span<Vertex>(subMesh.vertices).Cast<Vertex, byte>();
				writer.Write(verts);

				writer.Write(subMesh.indices.Length);
				Span<uint> indsU = subMesh.indices;
				var inds = indsU.Cast<UInt32, byte>();
				writer.Write(inds);
			}
			writer.Flush();
		}
	}
}
