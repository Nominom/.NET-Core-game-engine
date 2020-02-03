using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.AssetSystem
{
	public class CompressedTextureAsset : IAsset
	{
		private KtxFile tex2D;
		public string Filename { get; set; }
		public bool IsLoaded { get; private set;}
		public void Load() {
			if (IsLoaded) {
				return;
			}

			using var fs = File.OpenRead(Filename);
			tex2D = KtxFile.Load(fs, false);
		}

		public void UnLoad() {
			tex2D = null;
			IsLoaded = false;
		}

		public void Dispose() {
			UnLoad();
		}

		public KtxFile GetTexture() {
			return tex2D;
		}
	}
}
