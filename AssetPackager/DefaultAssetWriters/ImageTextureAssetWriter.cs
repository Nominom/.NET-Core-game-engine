using System.Collections.Generic;
using System.IO;
using System.Linq;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using Core.AssetSystem;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace AssetPackager.DefaultAssetWriters
{
	public class ImageTextureAssetWriter : AssetWriter<TextureAsset>
	{
		public override IEnumerable<string> GetAssociatedInputExtensions() {
			yield return ".png";
			yield return ".jpg";
			yield return ".bmp";
			yield return ".gif";
		}

		public override unsafe void LoadAndWriteToStream(FileInfo inputFile, Stream outputStream) {
			//var image = Image.Load<Rgba32>(inputFile.FullName);
			bool compressed = true;
			bool generateMips = true;

			using FileStream inputFs = File.OpenRead(inputFile.FullName);

			using Image<Rgba32> imageFile = Image.Load<Rgba32>(inputFs);
			bool hasTransparency = imageFile.GetPixelSpan().ToArray().Any(x => x.A < 255);
			BcEncoder encoder = new BcEncoder();

			if (!compressed) {
				encoder.OutputOptions.generateMipMaps = generateMips;
				encoder.OutputOptions.format = CompressionFormat.RGBA;
				encoder.OutputOptions.fileFormat = OutputFileFormat.Ktx;
				encoder.Encode(imageFile, outputStream);
			}
			else {

				encoder.OutputOptions.generateMipMaps = generateMips;
				encoder.OutputOptions.format = CompressionFormat.BC7;
				encoder.OutputOptions.quality = EncodingQuality.Balanced;
				encoder.OutputOptions.fileFormat = OutputFileFormat.Ktx;
				encoder.Encode(imageFile, outputStream);
			}
			
			
			/*
			compressor.Process(out DDSContainer dds);
			using BinaryWriter writer = new BinaryWriter(outputStream);

			using (dds) {
				KtxHeader header = new KtxHeader();
				byte[] id = new byte[] {0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A};
				Unsafe.Copy(header.Identifier, ref id[0]);
				header.Endianness = 0x04030201;
				if (compressed) {
					header.GlType = 0; //For compressed textures
					header.GlTypeSize = 1; //For compressed textures
					header.GlFormat = 0; //For compressed textures
					header.GlInternalFormat = hasTransparency
						? GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC
						: GlInternalFormat.GL_COMPRESSED_SRGB8_ETC2;
				}
				else {
					header.GlType = 0x1401; //GL_UNSIGNED_BYTE
					header.GlTypeSize = 1; //1 byte
					header.GlFormat = 0x1908; //GL_RGBA
				}
				

				header.PixelWidth = (uint)dds.MipChains[0][0].Width;
				header.PixelHeight = dds.Dimension == TextureDimension.One ? 
					0 : (uint)dds.MipChains[0][0].Height;
				header.PixelDepth = dds.Dimension == TextureDimension.Three ? 
					(uint)dds.MipChains[0][0].Depth : 0;

				header.NumberOfArrayElements = 0;
				header.NumberOfFaces = (uint)dds.MipChains.Count;
				header.NumberOfMipmapLevels = (uint) dds.MipChains[0].Count;
				header.BytesOfKeyValueData = 0;

				WriteStruct(writer, header);
				bool isCubemap = header.NumberOfFaces == 6 && header.NumberOfArrayElements == 0;
				
				for (int i = 0; i < header.NumberOfMipmapLevels; i++) {
					uint imageSize = (uint)dds.MipChains[0][i].SizeInBytes;
					writer.Write(imageSize);

					for (int j = 0; j < header.NumberOfFaces; j++) {
						var face = dds.MipChains[j][i];
						Span<byte> bytes = new Span<byte>(face.Data.ToPointer(), face.SizeInBytes);
						writer.Write(bytes);
						uint cubePadding = 0u;
						if (isCubemap)
						{
							cubePadding = 3 - ((imageSize + 3) % 4);
						}

						AddPadding(writer, cubePadding);
					}

					uint mipPaddingBytes = 3 - ((imageSize + 3) % 4);
					AddPadding(writer, mipPaddingBytes);
				}
				writer.Flush();
			}
			*/
		}
	}
}
