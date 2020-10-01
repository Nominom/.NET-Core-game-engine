using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.AssetSystem
{
	public enum AssetState
	{
		Unloaded,
		Loading,
		Loaded,
		FailedToLoad
	}

	internal static class AssetManager
	{
		private struct AssetHolder
		{
			public string assetName;
			public System.Type assetType;
			public IAsset asset;
			public AssetState state;
		}
		private static readonly List<AssetHolder> assets = new List<AssetHolder>();
		private static readonly Dictionary<string, int> assetNameToHolder = new Dictionary<string, int>();
		private static readonly List<AssetPackage> assetPackages = new List<AssetPackage>();
		private static readonly Dictionary<string, AssetPackage> assetNameToPackage =
			new Dictionary<string, AssetPackage>();

		public static void LoadAssetPackage(string filename)
		{
			FileInfo file = new FileInfo(filename);
			if (!file.Exists)
			{
				throw new FileNotFoundException("Asset package was not found at location " + file.FullName);
			}
			AssetPackage package = new AssetPackage(file);
			foreach (string assetName in package.GetAssetNames())
			{
				assetNameToPackage[assetName] = package;
			}
			assetPackages.Add(package);
		}

		public static T Get<T>(AssetReference<T> reference) where T : class, IAsset
		{
			if (reference.assetIndex == 0)
			{
				return null;
			}

			IAsset asset = assets[reference.assetIndex - 1].asset;
			if (asset is T value)
			{
				return value;
			}

			throw new ArgumentException("The given AssetReference does not point to an asset of this type.", nameof(reference));
		}

		public static AssetReference<T> LoadOrRegisterAsset<T>(string assetName) where T : class, IAsset, new()
		{
			if (assetNameToHolder.TryGetValue(assetName, out int idx))
			{
				if (assets[idx].assetType != typeof(T))
				{
					throw new AssetTypeException("The type of reference does not match type of asset");
				}
				return new AssetReference<T>(idx);
			}
			else
			{
				AssetPackage package = GetAssetPackageForAsset(assetName);
				var header = package.GetAssetHeader(assetName);
				if (header.assetType != typeof(T))
				{
					throw new AssetTypeException("The type of reference does not match type of asset");
				}
				AssetReference<T> newRef = new AssetReference<T>(assets.Count + 1);
				assets.Add(new AssetHolder()
				{
					asset = new T(),
					assetType = typeof(T),
					assetName = assetName,
					state = AssetState.Unloaded
				});
				return newRef;
			}
		}

		public static AssetPackage GetAssetPackageForAsset(string assetName)
		{
			if (assetNameToPackage.TryGetValue(assetName, out var value))
			{
				return value;
			}
			else
			{
				throw new AssetNotFoundException($"Asset with name {assetName} was not found.");
			}
		}

		public static string GetAssetName<T>(AssetReference<T> reference) where T : class, IAsset
		{
			if (reference.assetIndex == 0)
			{
				throw new InvalidOperationException("Cannot get the asset name of a null asset");
			}

			return assets[reference.assetIndex - 1].assetName;
		}

		public static AssetState GetState<T>(AssetReference<T> reference) where T : class, IAsset
		{
			if (reference.assetIndex == 0)
			{
				throw new InvalidOperationException("Cannot get the state of a null asset");
			}

			return assets[reference.assetIndex - 1].state;
		}

		internal static AssetState GetState(int assetIndex)
		{
			if (assetIndex == 0)
			{
				throw new InvalidOperationException("Cannot get the state of a null asset");
			}

			return assets[assetIndex - 1].state;
		}

		internal static void SetState(int assetIndex, AssetState newState)
		{
			if (assetIndex == 0)
			{
				throw new InvalidOperationException("Cannot set the state of a null asset");
			}

			var asset = assets[assetIndex - 1];
			asset.state = newState;
			assets[assetIndex - 1] = asset;
		}
	}

	internal class AssetNotFoundException : Exception
	{
		public AssetNotFoundException() : base() { }
		public AssetNotFoundException(string message) : base(message) { }
	}

	public class AssetTypeException : Exception
	{
		public AssetTypeException() : base() { }
		public AssetTypeException(string message) : base(message) { }
	}
}
