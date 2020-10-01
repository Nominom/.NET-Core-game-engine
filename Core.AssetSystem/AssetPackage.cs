using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Shared;

namespace Core.AssetSystem
{
	public class AssetPackage : IDisposable {
		private FileInfo assetFile;
		private Dictionary<int, FileStream> fileStreams = new Dictionary<int, FileStream>();
		private Dictionary<string, long> assetPositions;
		private Dictionary<string, AssetHeader> assetHeaders;

		public AssetPackage(FileInfo assetFile) {
			this.assetFile = assetFile;

			assetPositions = new Dictionary<string, long>();
			assetHeaders = new Dictionary<string, AssetHeader>();
			ReadHeaders();
		}

		public Stream GetUncompressedFileStream(string assetName) {
			if (!assetHeaders.ContainsKey(assetName)) {
				throw new ArgumentException($"No asset found with name {assetName} in package {assetFile.Name}");
			}

			AssetHeader header = assetHeaders[assetName];
			long position = assetPositions[assetName];
			FileStream fs = GetThreadStream();
			fs.Seek(position, SeekOrigin.Begin);

			var source = new NonClosingFileStreamReaderWrapper(fs, header.sizeInBytes);

			//var source = new DeflateStream(stream, CompressionMode.Decompress, true);

			//var output = new byte[header.uncompressedBytes];
			//MemoryStream outStream = new MemoryStream(output);
			//source.CopyTo(outStream);

			switch (header.compression) {
				case AssetCompression.NotCompressed:
					return source;
					break;
				case AssetCompression.Deflate:
					return new DeflateStream(source, CompressionMode.Decompress, true);
					break;
				case AssetCompression.Gzip:
					return new GZipStream(source, CompressionMode.Decompress, true);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public AssetHeader GetAssetHeader(string assetName) {
			return assetHeaders[assetName];
		}

		public IEnumerable<string> GetAssetNames() {
			return assetHeaders.Keys;
		}


		private void ReadHeaders() {
			var stream = GetThreadStream();
			stream.Seek(0, SeekOrigin.Begin);

			BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true);
			
			int numberOfAssets = reader.ReadInt32(); // Read number of assets

			for (int i = 0; i < numberOfAssets; i++) {
				AssetHeader header = AssetHeader.ReadAssetHeader(reader);
				long assetPosition = stream.Position;
				assetPositions.Add(header.assetName, assetPosition);
				assetHeaders.Add(header.assetName, header);
				stream.Seek(header.sizeInBytes, SeekOrigin.Current);
			}
		}

		private FileStream GetThreadStream()
		{
			Thread thread = Thread.CurrentThread;
			if (!fileStreams.TryGetValue(thread.ManagedThreadId, out var fs)) {
				fs = new FileStream(assetFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
				fileStreams.Add(thread.ManagedThreadId, fs);
			}

			return fs;
		}


		~AssetPackage()
		{
			Dispose(false);
		}

		private void Dispose(bool disposing) {
			foreach (var fileStream in fileStreams) {
				fileStream.Value?.Dispose();
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
