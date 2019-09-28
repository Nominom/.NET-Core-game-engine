using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AssetSystem
{
	public static class AssetManager
	{
		private static readonly List<IAsset> assets = new List<IAsset>();

		public static T Get<T>(AssetReference<T> reference) where T : class, IAsset {

			if (reference.assetIndex == 0) {
				return null;
			}

			IAsset asset = assets[reference.assetIndex - 1];
			if (asset is T value) {
				return value;
			}

			throw new ArgumentException("The given AssetReference does not point to an asset of this type.", nameof(reference));
		}

		public static AssetReference<T> RegisterAsset<T>(T asset) where T : class, IAsset
		{
			AssetReference<T> newRef = new AssetReference<T>(assets.Count + 1);
			assets.Add(asset);
			return newRef;
		}
	}
}
