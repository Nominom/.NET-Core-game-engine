using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AssetSystem
{
	public readonly struct AssetReference<T> where T : class, IAsset {

		/// <summary>
		/// Index to the internal AssetManager asset list. Incremented by one, because default 0 is null.
		/// </summary>
		internal readonly int assetIndex;

		internal AssetReference(int index) {
			this.assetIndex = index;
		}

		public T Get() {
			return AssetManager.Get(this);
		}

		public string GetAssetName() {
			return AssetManager.GetAssetName(this);
		}

		public AssetState State => AssetManager.GetState(this);

		public bool IsLoaded => State == AssetState.Loaded;
		public bool IsLoading => State == AssetState.Loading;
		public bool IsUnloaded => State == AssetState.Unloaded;
		public bool FailedToLoad => State == AssetState.FailedToLoad;

		public void StartLoad(LoadPriority priority) {
			var state = State;
			if (state == AssetState.Loading || state == AssetState.Loaded) {
				return;
			}
			AssetLoader.QueueAssetLoad(this, priority);
		}

		public void LoadNow() {
			var state = State;
			if (state == AssetState.Loaded) {
				return;
			}
			if (state == AssetState.Unloaded) {
				AssetLoader.QueueAssetLoad(this, LoadPriority.High);
			}
			AssetLoader.WaitForAssetToLoad(assetIndex);
		}
	}
}
