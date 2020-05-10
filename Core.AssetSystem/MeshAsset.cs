using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core.Shared;

namespace Core.AssetSystem
{
	public class MeshAsset : IAsset
	{
		//public string Filename { get; set; }
		//public bool IsLoaded { get; private set; }

		private MeshData meshData;

		//public void Load() {
		//	if (IsLoaded) {
		//		return;
		//	}

		//	meshData = ModelLoader.LoadModel(Filename);
		//	IsLoaded = true;
		//}

		//public void UnLoad() {
		//	meshData = null;
		//	IsLoaded = false;
		//}

		public void Dispose()
		{
			meshData = null;	
		}

		public void LoadFromStream(Stream stream) {
			using BinaryReader reader = new BinaryReader(stream);
			meshData = new MeshData();
			meshData.subMeshes = new SubMeshData[reader.ReadInt32()];
			for (int i = 0; i < meshData.subMeshes.Length; i++) {
				Vertex[] verts = new Vertex[reader.ReadInt32()];
				Span<byte> vBytes = new Span<Vertex>(verts).Cast<Vertex, byte>();
				reader.Read(vBytes);

				uint[] inds = new uint[reader.ReadInt32()];
				Span<byte> iBytes = new Span<uint>(inds).Cast<uint, byte>();
				reader.Read(iBytes);

				meshData.subMeshes[i] = new SubMeshData(verts, inds);
			}
		}

		public MeshData GetMeshData() {
			return meshData;
		}
	}
}
