using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AssetSystem
{
	public struct AssetReference<T> where T : class, IAsset {

		/// <summary>
		/// Index to the internal AssetManager asset list. Incremented by one, because default 0 is null.
		/// </summary>
		internal int assetIndex;

		internal AssetReference(int index) {
			this.assetIndex = index;
		}

		public T Get() {
			return AssetManager.Get(this);
		}
	}
}
