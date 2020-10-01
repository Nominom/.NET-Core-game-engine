using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core.AssetSystem;

namespace AssetPackager
{
	public interface IAssetWriter {
		IEnumerable<string> GetAssociatedInputExtensions();
		void GetDefaultMeta(IAssetMeta meta);
		System.Type GetAssetType();
		void LoadAndWriteToStream(FileInfo inputFile, IAssetMeta meta, Stream outputStream);
	}

	public abstract class AssetWriter<T> : IAssetWriter where T : IAsset {
		public abstract IEnumerable<string> GetAssociatedInputExtensions();
		public abstract void GetDefaultMeta(IAssetMeta meta);
		public Type GetAssetType() => typeof(T);
		public abstract void LoadAndWriteToStream(FileInfo inputFile, IAssetMeta meta, Stream outputStream);
	}
}
