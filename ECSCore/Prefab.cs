﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Core.ECS
{
	public class Prefab {
		public string name;
		internal List<ComponentSliceValues> componentSizes;
		internal List<Type> componentTypes;
		internal byte[] componentData;
		internal EntityArchetype archetype;

		public EntityArchetype Archetype {
			get => archetype;
		} 

		public Prefab() {
			name = "Empty prefab";
			componentSizes = new List<ComponentSliceValues>();
			componentTypes = new List<Type>();
			componentData = Array.Empty<byte>();
			archetype = EntityArchetype.Empty;
		}

		public Prefab(EntityArchetype archetype) {
			name = "Prefab";
			componentSizes = new List<ComponentSliceValues>();
			componentTypes = new List<Type>();
			componentData = Array.Empty<byte>();
			this.archetype = archetype;
		}

		public Prefab Clone()
		{
			Prefab prefab = new Prefab();
			prefab.name = this.name;
			prefab.archetype = this.archetype;
			prefab.componentSizes = new List<ComponentSliceValues>(this.componentSizes);
			prefab.componentTypes = new List<Type>(this.componentTypes);
			prefab.componentData = new byte[this.componentData.Length];
			this.componentData.CopyTo(prefab.componentData, 0);
			return prefab;
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
				archetype = archetype.Add<T>();
				int cSize = Unsafe.SizeOf<T>();
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
			}
		}

		public void AddSharedComponent<T>(T value) where T : class, ISharedComponent {
			archetype = archetype.AddShared(value);
		}
	}
}
