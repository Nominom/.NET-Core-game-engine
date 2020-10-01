using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core.AssetSystem.Assets;

namespace AssetPackager.DefaultAssetWriters
{
	public class ShaderWriter : AssetWriter<ShaderAsset>
	{
		public override IEnumerable<string> GetAssociatedInputExtensions()
		{
			yield return ".spv";
		}

		public override void GetDefaultMeta(IAssetMeta meta)
		{
			meta.SetValue("shaderType", ShaderAsset.ShaderType.Frag);
		}

		public override void LoadAndWriteToStream(FileInfo inputFile, IAssetMeta meta, Stream outputStream)
		{
			ShaderAsset.ShaderType type = ShaderAsset.ShaderType.Frag;
			if (inputFile.Name.ToLower().Contains("frag"))
			{
				type = ShaderAsset.ShaderType.Frag;
			}
			else if (inputFile.Name.ToLower().Contains("vert"))
			{
				type = ShaderAsset.ShaderType.Vertex;
			}
			else
			{
				type = meta.GetValue("shaderType", ShaderAsset.ShaderType.Frag);
			}

			using var ms = new MemoryStream();
			using var fs = inputFile.OpenRead();
			using var br = new BinaryWriter(outputStream, Encoding.UTF8, true);

			fs.CopyTo(ms);

			var data = ms.ToArray();

			br.Write((int)type);
			br.Write(data.Length);
			br.Write(data);
		}
	}
}
