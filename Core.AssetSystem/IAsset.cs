using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AssetSystem
{
	public interface IAsset : IDisposable {
		string Filename { get; set; }
		bool IsLoaded { get; }
		void Load();
		void UnLoad();
	}
}
