using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ECSCore
{
	internal static class TypeHelper<T>
	{
		public static readonly System.Type type = typeof(T);
		public static readonly string typeName = typeof(T).FullName;
		public static readonly int hashCode = typeof(T).GetHashCode();
	}

	internal static class TypeHelper
	{

		internal static class Component<T> where T : IComponent {
			public static readonly int componentIndex = ComponentMask.GetComponentIndex<T>();
		}

		internal static class SharedComponent<T> where T : ISharedComponent
		{
			public static readonly int componentIndex = SharedComponentMask.GetSharedComponentIndex<T>();
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
