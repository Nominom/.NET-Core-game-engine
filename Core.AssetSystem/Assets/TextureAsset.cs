using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.AssetSystem.Assets
{
	public class TextureAsset : IAsset
	{
		private KtxFile tex2D;

		public void LoadFromStream(Stream file) {
			tex2D = KtxFile.Load(file, false);
		}

		public void UnLoad() {
			
		}

		public void Dispose() {
			tex2D = null;
		}

		public KtxFile GetTexture() {
			return tex2D;
		}
	}
}
