using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Core.Shared;

namespace AssetPackager
{
	static class AssetLoaderResolver
	{
		public static Dictionary<string, IAssetWriter> extensionToAssetWriter = new Dictionary<string, IAssetWriter>();

		public static void InitializeAndFindAssetWriters()
		{
			foreach (Assembly assembly in AssemblyHelper.GetAllUserAssemblies())
			{
				foreach (var type in AssemblyHelper.GetTypesWithInterface(assembly, typeof(IAssetWriter)))
				{
					if (type.IsAbstract || type.ContainsGenericParameters) continue;

					IAssetWriter writer = Activator.CreateInstance(type) as IAssetWriter;
					foreach (string extension in writer.GetAssociatedInputExtensions())
					{
						if (!extensionToAssetWriter.ContainsKey(extension))
						{
							extensionToAssetWriter.Add(extension, writer);
						}
						else
						{
							Console.WriteLine($"Conflicting filename extension {extension} between {extensionToAssetWriter[extension].GetType().Name} and {type.Name}. Using {extensionToAssetWriter[extension].GetType().Name}.");
						}
					}
				}
			}
		}

		public static void AddAssembly(Assembly assembly)
		{
			foreach (var type in AssemblyHelper.GetTypesWithInterface(assembly, typeof(IAssetWriter))) {
				if (type.IsAbstract || type.ContainsGenericParameters) continue;

				IAssetWriter writer = Activator.CreateInstance(type) as IAssetWriter;
				foreach (string filename in writer.GetAssociatedInputExtensions())
				{
					if (!extensionToAssetWriter.ContainsKey(filename))
					{
						extensionToAssetWriter.Add(filename, writer);
					}
				}
			}
		}

		public static bool SupportedExtension(string extension)
		{
			return extensionToAssetWriter.ContainsKey(extension);
		}

		public static IAssetWriter FindAssetWriter(string extension)
		{
			return extensionToAssetWriter[extension];
		}
	}
}
