using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AssetSystem
{
	public class ModelAsset : IAsset
	{
		public bool IsLoaded { get; }
		public void Load() {
			throw new NotImplementedException();
		}

		public void UnLoad() {
			throw new NotImplementedException();
		}
		public void Dispose()
		{
			throw new NotImplementedException();
		}

	}
}
