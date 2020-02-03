using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AssetSystem
{
	public static class Assets
	{
		public static T Create<T>(string filename) where T : class, IAsset, new() {
			T asset = new T();
			asset.Filename = filename;
			return asset;
		}
	}
}
