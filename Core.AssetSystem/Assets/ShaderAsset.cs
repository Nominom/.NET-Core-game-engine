using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.AssetSystem.Assets
{
	public class ShaderAsset : IAsset
	{
		public enum ShaderType {
			Frag,
			Vertex
		}

		public ShaderType Type { get; private set; }
		private byte[] shaderBytes;

		public void Dispose() {

		}

		public void LoadFromStream(Stream stream) {
			using BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true);

			Type = (ShaderType)br.ReadInt32();
			int length = br.ReadInt32();

			shaderBytes = new byte[length];
			br.Read(shaderBytes);
		}

		public byte[] GetBytes() {
			return shaderBytes;
		}
	}
}
