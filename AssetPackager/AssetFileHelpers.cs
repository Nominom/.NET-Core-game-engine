using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetPackager
{
	internal static class AssetFileHelpers
	{

		public static string GetAssetName(FileInfo file) {
			return Path.GetFileNameWithoutExtension(file.FullName);
		}

		public static FileInfo GetMetaFile(FileInfo assetFile) {
			return new FileInfo(Path.ChangeExtension(assetFile.FullName, ".meta"));
		}
	}
}
