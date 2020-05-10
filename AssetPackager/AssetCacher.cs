using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetPackager
{
	public static class AssetCacher {
		public static string cacheFolder = "assetcache";

		public static bool HasCachedFile(FileInfo file)
		{
			return false;
		}

		public static void SaveCachedFile(FileInfo file, Stream data) {
			EnsureCacheFolder();

			var originalPosition = data.Position;
			data.Seek(0, SeekOrigin.Begin);

			var newPath = Path.ChangeExtension(file.Name, ".dat");
			newPath = Path.Combine(cacheFolder, newPath);

			using FileStream fs = File.OpenWrite(newPath);
			data.CopyTo(fs);
			
			data.Position = originalPosition;
		}

		private static void EnsureCacheFolder()
		{
			var directory = new DirectoryInfo(cacheFolder);
			if (!directory.Exists)
			{
				directory.Create();
			}
		}
	}
}
