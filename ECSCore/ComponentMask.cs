using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Core.ECS.Collections;

namespace Core.ECS
{
	public static class ComponentMask {
		private static Dictionary<int, int> CreateComponentDictionary() {
			Dictionary<int, int> dict = new Dictionary<int, int>();
			int i = 0;
			foreach (Assembly assembly in AssemblyHelper.GetAllUserAssemblies()) {
				foreach (Type type in AssemblyHelper.GetTypesWithInterface(assembly, typeof(IComponent))) {
					int typeHash = type.GetHashCode();
					if (dict.ContainsKey(typeHash)) {
						Console.WriteLine("ComponentDictionary already contains hash of type " + type.FullName);
						continue;
					}
					dict.Add(typeHash, i);
					i++;
				}
			}
			if (i > 255) {
				Console.WriteLine("Maximum amount of components exceeded.");
			}
			return dict;
		}

		private static Dictionary<int, int> componentHashToIndex { get; } = CreateComponentDictionary();
		private static int componentAmount { get; } = componentHashToIndex.Count;

		public static BitSet256 GetComponentMask(IEnumerable<Type> types) {
			if (types == null) {
				return default;
			}

			BitSet256 result = new BitSet256();
			foreach (var type in types) {
				int hash = type.GetHashCode();
				DebugHelper.AssertThrow(componentHashToIndex.ContainsKey(hash), new ArgumentException("Input type has to be IComponent"));
				result.Set(componentHashToIndex[hash]);
			}
			return result;
		}

		public static int GetComponentIndex<T>() where T : IComponent {
			return componentHashToIndex[TypeHelper<T>.hashCode];
		}

		public static int GetComponentIndex(Type type) {
			int hash = type.GetHashCode();
			DebugHelper.AssertThrow(componentHashToIndex.ContainsKey(hash), new ArgumentException("Input type has to be IComponent"));
			return componentHashToIndex[hash];
		}

	}

	public static class SharedComponentMask
	{
		private static Dictionary<int, int> CreateComponentDictionary()
		{
			Dictionary<int, int> dict = new Dictionary<int, int>();
			int i = 0;
			foreach (Assembly assembly in AssemblyHelper.GetAllUserAssemblies())
			{
				foreach (Type type in AssemblyHelper.GetTypesWithInterface(assembly, typeof(ISharedComponent)))
				{
					int typeHash = type.GetHashCode();
					if (dict.ContainsKey(typeHash))
					{
						Console.WriteLine("ComponentDictionary already contains hash of type " + type.FullName);
						continue;
					}
					dict.Add(typeHash, i);
					i++;
				}
			}
			if (i > 255)
			{
				Console.WriteLine("Maximum amount of shared components exceeded.");
			}
			return dict;
		}

		private static Dictionary<int, int> componentHashToIndex { get; } = CreateComponentDictionary();
		private static int componentAmount { get; } = componentHashToIndex.Count;

		public static BitSet256 GetSharedComponentMask(IEnumerable<Type> types)
		{
			if (types == null)
			{
				return default;
			}

			BitSet256 result = new BitSet256();
			foreach (var type in types)
			{
				int hash = type.GetHashCode();
				DebugHelper.AssertThrow(componentHashToIndex.ContainsKey(hash), new ArgumentException("Input type has to be IComponent"));
				result.Set(componentHashToIndex[hash]);
			}
			return result;
		}

		public static int GetSharedComponentIndex<T>() where T : ISharedComponent
		{
			return componentHashToIndex[TypeHelper<T>.hashCode];
		}

		public static int GetSharedComponentIndex(Type type)
		{
			int hash = type.GetHashCode();
			DebugHelper.AssertThrow(componentHashToIndex.ContainsKey(hash), new ArgumentException("Input type has to be IComponent"));
			return componentHashToIndex[hash];
		}

	}
}
