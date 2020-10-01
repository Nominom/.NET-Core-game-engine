using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using AssetPackager.DefaultAssetWriters;
using Core.AssetSystem;

namespace AssetPackager
{
	/// 
	/// Output file structure
	/// @ means data followed by datatype and name
	/// example: @Uint32 dataname
	///	===FILE BEGIN===
	/// @Uint32 numberOfAssets
	/// foreach Asset in assets {
	///		Header {
	///			@Uint32 AssetCompression // AssetCompression enum
	///			@Uint64 sizeInBytes // compressed or uncompressed
	///			@Uint64 uncompressedSizeInBytes
	///			@Uint32 assetNameLengthInBytes
	///			@byte[assetNameLengthInBytes] assetNameUtf8Encoded
	///			@Uint32 assetTypeNameLengthInBytes
	///			@byte[assetTypeNameLengthInBytes] assetTypeNameUtf8Encoded
	///		}
	///		@byte[sizeInBytes] assetRawData // Compressed or Uncompressed.
	/// }


	class Program
	{
		
		static void Main(string[] args)
		{
			if (args.Length < 2) {
				Console.WriteLine("Please pass arguments in the form of \"./AssetPackager AssetFolderName outputfilename.dat\"");
				Console.WriteLine("If you're using custom asset loaders, use \"./AssetPackager AssetFolderName outputfilename.dat loaderlibrary.dll\"");
				Console.WriteLine("Aborting build.");
				return;
			}

			string inputFolder = args[0];
			string outputFile = args[1];
			if (args.Length > 2) {
				string assemblyPath = args[2];
				AssetLoaderResolver.AddAssembly(LoadPlugin(assemblyPath));
			}

			AssetLoaderResolver.InitializeAndFindAssetWriters();

			if (!Directory.Exists(inputFolder)) {
				Console.WriteLine("Please ensure that the input folder is a valid path.");
				Console.WriteLine("Aborting build.");
				return;
			}

			List<FileInfo> inputFiles = ExploreFolder(inputFolder);
			List<FileInfo> supportedFiles = new List<FileInfo>();
			HashSet<string> inUseAssetNames = new HashSet<string>();

			foreach (FileInfo inputFile in inputFiles) {
				if (AssetLoaderResolver.SupportedExtension(inputFile.Extension)) {
					Console.WriteLine($"Found file: {inputFile}.");
					if (inUseAssetNames.Contains(AssetFileHelpers.GetAssetName(inputFile))) {
						Console.WriteLine($"Duplicate asset name found! {inputFile.FullName} collides with {supportedFiles.First(x => AssetFileHelpers.GetAssetName(x) == AssetFileHelpers.GetAssetName(inputFile)).FullName}");
						Console.WriteLine("Aborting build.");
						return;
					}

					supportedFiles.Add(inputFile);
					inUseAssetNames.Add(AssetFileHelpers.GetAssetName(inputFile));
				}
				else {
					Console.WriteLine($"Found unsupported file: {inputFile}.");
				}
			}

			Console.WriteLine("\n\nStarting build...\n\n");

			using FileStream outputFileStream = File.OpenWrite(outputFile);
			
			AssetPackageWriter.WriteAssetPackage(outputFileStream, supportedFiles, true);
		}

		

		private static List<FileInfo> ExploreFolder(string folderPath)
		{
			List<FileInfo> files = new List<FileInfo>();
			Stack<DirectoryInfo> directoriesToExplore = new Stack<DirectoryInfo>();
			directoriesToExplore.Push(new DirectoryInfo(folderPath));

			while (directoriesToExplore.TryPop(out var directory)) {
				foreach (var newDir in directory.EnumerateDirectories()) {
					directoriesToExplore.Push(newDir);
				}
				foreach (FileInfo file in directory.EnumerateFiles()) {
					files.Add(file);
				}
			}

			return files;
		}

		static Assembly LoadPlugin(string path)
		{
			
			string pluginLocation = Path.GetFullPath(path);
			Console.WriteLine($"Loading plugin from: {pluginLocation}");
			PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
			return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
		}
	}
}
