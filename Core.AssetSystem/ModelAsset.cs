using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core.Shared;

namespace Core.AssetSystem
{
	public class ModelAsset : IAsset
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

		public void LoadFromStream(Stream file) {
			meshData = ModelLoader.LoadModel(file);
		}

		public MeshData GetMeshData() {
			return meshData;
		}
	}
}
