using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Core.AssetSystem
{
	public class ImageTextureAsset : IAsset {

		private Image<Rgba32> image;
		//public string Filename { get; set; }
		//public bool IsLoaded { get; private set; }

		//public void Load() {
		//	if (IsLoaded) {
		//		return;
		//	}

		//	image = Image.Load<Rgba32>(Filename);
		//	IsLoaded = true;
		//}

		//public void UnLoad() {
		//	image?.Dispose();
		//	image = null;
		//	IsLoaded = false;
		//}

		public void Dispose() {
			image?.Dispose();
			image = null;
		}

		public void LoadFromStream(Stream file) {
			image = Image.Load<Rgba32>(file);
		}

		public Image<Rgba32> GetTexture() {
			return image;
		}
	}
}
