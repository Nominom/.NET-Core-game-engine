using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Core.ECS
{
	public static class AssemblyHelper {
		private static Assembly[] cachedUserAssemblies;

		public static IEnumerable<Assembly> GetAllUserAssemblies() {
			if (cachedUserAssemblies == null) {
				var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(ass => ass.IsDynamic == false);
				cachedUserAssemblies = assemblies.Where(x => !x.FullName.StartsWith("Microsoft")).ToArray();
			}

			return cachedUserAssemblies;
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
				if (type.GetInterfaces().Contains(interfaceType))
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
