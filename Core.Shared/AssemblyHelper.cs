using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Shared
{
	public static class AssemblyHelper {
		private static Assembly[] cachedUserAssemblies;
		private static Assembly[] cachedAllAssemblies;

		public static Assembly[] LoadAllAssemblies() {
			if (cachedAllAssemblies != null) {
				return cachedAllAssemblies;
			}

			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(ass => ass.IsDynamic == false);

			List<Assembly> loadedAssemblies = assemblies.ToList();

			for (int i = 0; i < loadedAssemblies.Count; i++) {
				var loadedAssembly = loadedAssemblies[i];
				foreach (AssemblyName referencedAssemblyName in loadedAssembly.GetReferencedAssemblies())
				{

					var found = loadedAssemblies.Any(x => x.FullName == referencedAssemblyName.FullName);
					
					if (!found)
					{
						try
						{
							var referencedAssembly = Assembly.Load(referencedAssemblyName);
							loadedAssemblies.Add(referencedAssembly);
						}
						catch {
							// ignored
						}
					}
				}
			}

			cachedAllAssemblies = loadedAssemblies.ToArray();
			return cachedAllAssemblies;
		}

		public static IEnumerable<Assembly> GetAllUserAssemblies() {
			if (cachedUserAssemblies == null) {
				cachedUserAssemblies = LoadAllAssemblies()
					.Where(x => !x.FullName.StartsWith("Microsoft")).ToArray();
			}

			return cachedUserAssemblies;
		}

		public static IEnumerable<Type> GetTypes(Assembly assembly) {
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				types = Array.Empty<Type>();
			}
			return types;
		}

		public static IEnumerable<System.Type> GetTypesWithInterface(Assembly assembly, System.Type interfaceType)
		{
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				types = Array.Empty<Type>();
			}
			foreach (Type type in types)
			{
				if (type.GetInterfaces().Contains(interfaceType) && !type.IsInterface)
				{
					yield return type;
				}
			}
		}

		public static IEnumerable<Type> GetTypesWithAttribute(Assembly assembly, System.Type attributeType, bool inherit = false)
		{
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				types = Array.Empty<Type>();
			}
			foreach (Type type in types)
			{
				if (type.GetCustomAttributes(attributeType, inherit).Length > 0)
				{
					yield return type;
				}
			}

		}
	}
}
