using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AssetSystem
{
	public static class Assets
	{
		public static AssetReference<T> Create<T>(string filename) where T : class, IAsset, new() {
			T asset = new T();
			return AssetManager.RegisterAsset(asset, filename);
		}
	}
}
