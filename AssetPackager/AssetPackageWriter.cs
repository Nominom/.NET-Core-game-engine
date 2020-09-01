using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using Core.AssetSystem;

namespace AssetPackager
{
	public static class AssetPackageWriter
	{

		public static void WriteAssetPackage(FileStream outputFileStream, IList<FileInfo> inputFiles,
			bool useCache) {
			using BinaryWriter headerWriter = new BinaryWriter(outputFileStream, Encoding.UTF8);
			
			//Write amount of assets
			headerWriter.Write(inputFiles.Count);
			headerWriter.Flush();

			foreach (FileInfo file in inputFiles) {
				var writer = AssetLoaderResolver.FindAssetWriter(file.Extension);

				var metaFile = AssetFileHelpers.GetMetaFile(file);
				IAssetMeta meta = new JsonAssetMeta();

				writer.GetDefaultMeta(meta);

				if (metaFile.Exists) {
					using var fs = metaFile.OpenRead();
					meta.ReadFromStream(fs);
				}
				
				Stopwatch watch = new Stopwatch();
				watch.Start();

				Console.WriteLine($"Processing file: {file}.");

				using MemoryStream uncompressedOutput = new MemoryStream();
				using MemoryStream compressedOutput = new MemoryStream();

				if (AssetCacher.HasCachedFile(file, meta) && useCache) {
					AssetCacher.LoadCachedFile(file, meta, uncompressedOutput);
					Console.WriteLine("Cached found!");
				}
				else {
					Console.WriteLine("No cached asset found");
					writer.LoadAndWriteToStream(file, meta, uncompressedOutput);
					AssetCacher.SaveCachedFile(file, meta, uncompressedOutput);
				}

				Console.WriteLine($"Processing took: {watch.ElapsedMilliseconds} milliseconds.");

				watch.Restart();
				Console.WriteLine("Compressing...");

				using DeflateStream deflate = new DeflateStream(compressedOutput, CompressionLevel.Optimal, true);
				
				uncompressedOutput.Seek(0, SeekOrigin.Begin);
				uncompressedOutput.CopyTo(deflate);
				deflate.Flush();

				string assetName = AssetFileHelpers.GetAssetName(file);
				System.Type assetType = writer.GetAssetType();

				long length = uncompressedOutput.Length;
				long compressedLength = compressedOutput.Length;

				AssetHeader.WriteAssetHeader(headerWriter, assetName, assetType, AssetCompression.Deflate, length, compressedLength);
				compressedOutput.Seek(0, SeekOrigin.Begin);
				compressedOutput.CopyTo(outputFileStream);

				watch.Stop();
				Console.WriteLine($"Compression took: {watch.ElapsedMilliseconds} milliseconds.");
			}
		}
	}
}
