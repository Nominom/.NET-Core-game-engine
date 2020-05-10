﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AssetSystem
{
	public static class Assets {
		public static void LoadAssetPackage(string filename) => AssetManager.LoadAssetPackage(filename);
		public static AssetReference<T> Create<T>(string assetName) where T : class, IAsset, new() {
			return AssetManager.LoadOrRegisterAsset<T>(assetName);
		}
	}
}
