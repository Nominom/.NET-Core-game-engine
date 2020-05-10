using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core.AssetSystem;

namespace AssetPackager
{
	public interface IAssetWriter {
		IEnumerable<string> GetAssociatedInputExtensions();
		System.Type GetAssetType();
		void LoadAndWriteToStream(FileInfo inputFile, Stream outputStream);
	}

	public abstract class AssetWriter<T> : IAssetWriter where T : IAsset {
		public abstract IEnumerable<string> GetAssociatedInputExtensions();
		public Type GetAssetType() => typeof(T);
		public abstract void LoadAndWriteToStream(FileInfo inputFile, Stream outputStream);
	}
}
