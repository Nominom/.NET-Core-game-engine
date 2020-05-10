using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core.AssetSystem;

namespace AssetPackager.DefaultAssetWriters
{
	public class KtxTextureWriter : AssetWriter<TextureAsset>
	{
		public override IEnumerable<string> GetAssociatedInputExtensions() {
			yield return ".ktx";
		}

		public override void LoadAndWriteToStream(FileInfo inputFile, Stream outputStream) {
			using FileStream fileReadStream = File.OpenRead(inputFile.FullName);
			fileReadStream.CopyTo(outputStream);
		}
	}
}
