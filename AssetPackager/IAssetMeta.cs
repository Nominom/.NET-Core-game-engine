using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetPackager
{
	public interface IAssetMeta {
		T GetValue<T>(string key, T defaultValue);
		bool TryGetValue<T>(string key, out T value);
		void SetValue<T>(string key, T value);
		void WriteToStream(Stream stream);
		void ReadFromStream(Stream stream);
	}
}
