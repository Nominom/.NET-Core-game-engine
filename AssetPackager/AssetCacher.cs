using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetPackager
{
	public static class AssetCacher {
		public static string cacheFolder = "assetcache";

		public static bool HasCachedFile(FileInfo file, IAssetMeta meta)
		{
			var cached = GetCacheFile(file, meta);
			return cached.Exists;
		}

		public static void SaveCachedFile(FileInfo file, IAssetMeta meta, Stream data) {
			EnsureCacheFolder();

			var originalPosition = data.Position;
			data.Seek(0, SeekOrigin.Begin);

			var newFile = GetCacheFile(file, meta);

			using FileStream fs = newFile.OpenWrite();
			data.CopyTo(fs);
			
			data.Position = originalPosition;
		}

		public static void LoadCachedFile(FileInfo file, IAssetMeta meta, Stream output) {
			var cached = GetCacheFile(file, meta);
			using var fs = cached.OpenRead();
			fs.CopyTo(output);
		}

		private static FileInfo GetCacheFile(FileInfo originalFile, IAssetMeta meta) {
			string hash = AssetHasher.GetAssetHash(originalFile, meta).Substring(0, 32);
			var newPath = Path.ChangeExtension(originalFile.Name, $".{hash}.dat");
			newPath = Path.Combine(cacheFolder, newPath);
			return new FileInfo(newPath);
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
