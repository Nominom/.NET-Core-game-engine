using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AssetSystem
{
	public interface IAsset : IDisposable {
		bool IsLoaded { get; }
		void Load();
		void UnLoad();
	}
}
