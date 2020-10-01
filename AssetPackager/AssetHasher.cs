using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AssetPackager
{
	public static class AssetHasher
	{
		public static string GetAssetHash(FileInfo assetFile, IAssetMeta metaFile) {
			using var hash = SHA256.Create();
			using var fs = assetFile.OpenRead();
			using var ms = new MemoryStream();

			metaFile.WriteToStream(ms);
			ms.Seek(0, SeekOrigin.Begin);

			var fileHash = hash.ComputeHash(fs);
			var metaHash = hash.ComputeHash(ms);

			for (int i = 0; i < fileHash.Length; i++) {
				fileHash[i] ^= metaHash[i];
			}

			return ByteArrayToString(fileHash);
		}

		private static string ByteArrayToString(byte[] byteArr) {
			StringBuilder sb = new StringBuilder(byteArr.Length*2);
			foreach (byte b in byteArr) {
				sb.Append(b.ToString("X2"));
			}

			return sb.ToString();
		}
	}
}
