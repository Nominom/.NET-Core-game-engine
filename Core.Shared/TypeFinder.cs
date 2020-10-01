using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Core.Shared
{
	public static class TypeFinder
	{
		private static Dictionary<string, Type> cachedTypes = new Dictionary<string, Type>();

		public static Type FindType(string fullTypeName)
		{
			var type = Type.GetType(fullTypeName);
			if (type != null)
				return type;

			if (cachedTypes.TryGetValue(fullTypeName, out type)) {
				return type;
			}

			try {
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();

				// First check loaded assemblies
				foreach (var assembly in AssemblyHelper.LoadAllAssemblies())
				{
					type = assembly.GetType(fullTypeName);
					if (type != null)
						break;
				}

				if (type != null) {
					cachedTypes.Add(fullTypeName, type);
					return type;
				}
			}
			catch(Exception exception) {
				Console.WriteLine(exception);
			}

			if(type == null)
			{
				throw new KeyNotFoundException($"No type found with the name {fullTypeName}");
			}

			return type;
		}
	}
}
