using System;
using System.Collections.Generic;
using System.IO;
using Core.AssetSystem.Assets;

namespace AssetPackager.DefaultAssetWriters
{
	public class KtxTextureWriter : AssetWriter<TextureAsset>
	{
		public override IEnumerable<string> GetAssociatedInputExtensions() {
			yield return ".ktx";
		}

		public override void GetDefaultMeta(IAssetMeta meta) {
			
		}

		public override void LoadAndWriteToStream(FileInfo inputFile, IAssetMeta meta, Stream outputStream) {
			using FileStream fileReadStream = File.OpenRead(inputFile.FullName);
			fileReadStream.CopyTo(outputStream);
		}
	}
}
