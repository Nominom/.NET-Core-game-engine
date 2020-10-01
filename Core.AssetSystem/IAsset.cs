using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.AssetSystem
{
	public interface IAsset : IDisposable {
		void LoadFromStream(Stream stream);
	}
}
