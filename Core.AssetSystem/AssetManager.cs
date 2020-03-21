using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AssetSystem
{
	public enum AssetState {
		Unloaded,
		Loading,
		Loaded
	}

	internal static class AssetManager
	{
		private struct AssetHolder {
			public string filename;
			public System.Type AssetType;
			public IAsset asset;
			public AssetState state;
		}
		private static readonly List<AssetHolder> assets = new List<AssetHolder>();

		public static T Get<T>(AssetReference<T> reference) where T : class, IAsset {
			if (reference.assetIndex == 0) {
				return null;
			}

			IAsset asset = assets[reference.assetIndex - 1].asset;
			if (asset is T value) {
				return value;
			}

			throw new ArgumentException("The given AssetReference does not point to an asset of this type.", nameof(reference));
		}

		public static AssetReference<T> RegisterAsset<T>(T asset, string filename) where T : class, IAsset {
			AssetReference<T> newRef = new AssetReference<T>(assets.Count + 1);
			assets.Add(new AssetHolder() {
				asset =  asset,
				AssetType = typeof(T),
				filename = filename,
				state = AssetState.Unloaded
			});
			return newRef;
		}

		public static string GetFilename<T>(AssetReference<T> reference) where T : class, IAsset {
			if (reference.assetIndex == 0) {
				throw new InvalidOperationException("Cannot get the filename of a null asset");
			}

			return assets[reference.assetIndex - 1].filename;
		}

		public static AssetState GetState<T>(AssetReference<T> reference) where T : class, IAsset {
			if (reference.assetIndex == 0) {
				throw new InvalidOperationException("Cannot get the state of a null asset");
			}

			return assets[reference.assetIndex - 1].state;
		}
	}
}
