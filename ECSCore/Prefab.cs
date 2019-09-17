using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace ECSCore
{
	public class Prefab {
		public string name;
		private List<ComponentSliceValues> componentSizes;
		private List<Type> componentTypes;
		private byte[] componentData;
		private Dictionary<Type, ISharedComponent> sharedComponents;
		private EntityArchetype archetype;

		public Prefab() {
			name = "Empty prefab";
			componentSizes = new List<ComponentSliceValues>();
			componentTypes = new List<Type>();
			componentData = Array.Empty<byte>();
			sharedComponents = new Dictionary<Type, ISharedComponent>();
			archetype = EntityArchetype.Empty;
		}

		private void GrowComponentData(int newSize) {
			var newBytes = new byte[newSize];
			ReadOnlySpan<byte> old = componentData;
			Span<byte> nu = newBytes;
			old.CopyTo(nu);
			componentData = newBytes;
		}

		public void AddComponent<T>(T value) where T : unmanaged, IComponent {
			var type = typeof(T);
			var i = componentTypes.IndexOf(type);
			if (i != -1) {
				var slice = componentSizes[i];
				Span<byte> bytes = componentData;
				Span<T> span = MemoryMarshal.Cast<byte, T>(bytes.Slice(slice.start, slice.length));
				span[0] = value;
			}
			else {
				int cSize = Marshal.SizeOf<T>();
				ComponentSliceValues slice = new ComponentSliceValues() {
					componentSize = cSize,
					start = componentData.Length,
					length = cSize
				};
				GrowComponentData(componentData.Length + cSize);
				Span<byte> bytes = componentData;
				Span<T> span = MemoryMarshal.Cast<byte, T>(bytes.Slice(slice.start, slice.length));
				span[0] = value;

				componentTypes.Add(type);
				componentSizes.Add(slice);
				archetype = archetype.Add<T>();
			}
		}

		public void AddSharedComponent<T>(T value) where T : class, ISharedComponent {
			sharedComponents[typeof(T)] = value;
			archetype = archetype.AddShared(value);
		}
	}
}
