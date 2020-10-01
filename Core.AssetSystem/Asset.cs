using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AssetSystem
{
	public static class Asset {
		public static void LoadAssetPackage(string filename) => AssetManager.LoadAssetPackage(filename);
		public static AssetReference<T> Load<T>(string assetName) where T : class, IAsset, new() {
			return AssetManager.LoadOrRegisterAsset<T>(assetName);
		}
	}
}
