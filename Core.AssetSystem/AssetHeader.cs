using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core.Shared;

namespace Core.AssetSystem
{
	public enum AssetCompression : int
	{
		NotCompressed,
		Deflate,
		Gzip
	}

	public struct AssetHeader {
		public string assetName;
		public System.Type assetType;
		public AssetCompression compression;
		public long sizeInBytes;
		public long uncompressedBytes;

		public static void WriteAssetHeader(BinaryWriter writer, string assetName, Type assetType,
			AssetCompression compression, long uncompressedBytes, long sizeInBytes) {

			writer.Write((int)compression);
			writer.Write(sizeInBytes);
			writer.Write(uncompressedBytes);
			writer.WriteUtf8String(assetName);
			writer.WriteUtf8String(assetType.FullName);
			writer.Flush();
		}

		public static AssetHeader ReadAssetHeader(BinaryReader reader) {
			AssetHeader header = new AssetHeader();
			header.compression = (AssetCompression) reader.ReadInt32();
			header.sizeInBytes = reader.ReadInt64();
			header.uncompressedBytes = reader.ReadInt64();
			header.assetName = reader.ReadUtf8String();
			string typeName = reader.ReadUtf8String();
			header.assetType = TypeFinder.FindType(typeName);
			return header;
		}
	}
}
