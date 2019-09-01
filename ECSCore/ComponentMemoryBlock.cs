﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ECSCore {
	
	internal class ComponentMemoryBlock : IDisposable {
		private static readonly BlockAllocator allocator = BlockAllocator.KB16;

		private struct SliceValues {
			public int start;
			public int length;
			public int componentSize;
		}

		private int _size;
		private readonly int _maxSize;

		private readonly SliceValues _entitySlice;

		//private fixed byte _data[16384];
		private readonly BlockMemory _data;
		//private Dictionary<System.Type, SliceValues> _typeLocations;
		//private Dictionary<int, SliceValues> _typeLocations;
		private readonly CustomHashMap<SliceValues> _typeLocations;



		public readonly EntityArchetype archetype;
		public bool HasRoom => _size < _maxSize;
		public int Size => _size;
		public int MaxSize => _maxSize;

		public ComponentMemoryBlock(EntityArchetype archetype) {
			this.archetype = archetype;
			//_typeLocations = new Dictionary<Type, SliceValues>();
			//_typeLocations = new Dictionary<int, SliceValues>();
			_typeLocations = new CustomHashMap<SliceValues>();


			_data = allocator.Rent();
			_data.Memory.Span.Fill(0);

			int amountOfBytes = _data.Size();
			int entitySize = Marshal.SizeOf<Entity>();
			int bytesPerEntity = entitySize;
			foreach (var component in archetype.components) {
				bytesPerEntity += component.Value;
			}

			//calculate maximum size of entities held by this memory block
			_maxSize = (int) MathF.Floor((float)amountOfBytes / (float) bytesPerEntity);
			_size = 0;

			//place entities as the first "component"
			_entitySlice = new SliceValues { start = 0, length = entitySize * _maxSize, componentSize = entitySize};

			int nextIdx = entitySize * _maxSize; //start from after entities
			foreach (var component in archetype.components) {
				int componentLength = component.Value * _maxSize;
				_typeLocations.Add(component.Key.GetHashCode(), new SliceValues { start = nextIdx, length = componentLength, componentSize = component.Value});
				nextIdx += componentLength;
			}
		}

		internal Span<Entity> GetEntityData() {
			Span<byte> bytes = _data.Memory.Span.Slice(_entitySlice.start, _entitySlice.length);
			Span<Entity> span = MemoryMarshal.Cast<byte, Entity>(bytes);
			return span;
		}

		internal Span<T> GetComponentData<T>() where T : unmanaged, IComponent {
			//Type type = typeof(T);
			//Debug.Assert(_typeLocations.ContainsKey(type), $"No type of {type} found on this block");
			//SliceValues values = _typeLocations[type];

			SliceValues values = _typeLocations[TypeHelper<T>.hashCode];
			Span <byte> bytes = _data.Memory.Span.Slice(values.start, values.length);
			Span<T> span = MemoryMarshal.Cast<byte, T>(bytes);
			return span;
		}

		~ComponentMemoryBlock() {
			Free();
		}

		public void Dispose() {
			Free();
			GC.SuppressFinalize(this);
		}

		private void Free() {
			_data?.Dispose();
		}

		public int AddEntity(in Entity entity) {
			Debug.Assert(!entity.IsNull());
			Debug.Assert(_size < _maxSize);

			int idx = _size;
			GetEntityData()[idx] = entity;
			_size++;
			return idx;
		}



		//returns true if last entity was moved
		public bool RemoveEntityMoveLast(int e_idx, out Entity moved) {
			Debug.Assert(e_idx < _size);

			Span<Entity> entityData = GetEntityData();
			Span<byte> wholeMemory = _data.Memory.Span;
			int lastIdx = _size - 1;

			bool didMove = false;
			if (e_idx != lastIdx) {//Move all component memory from last in place of e_idx
				foreach (var slice in _typeLocations) {
					int cSize = slice.Value.componentSize; //component size in bytes
					int start = slice.Value.start;
					int idxOffset = cSize * e_idx;
					int lastIdxOffset = cSize * lastIdx;

					Span<byte> sFrom = wholeMemory.Slice(start + lastIdxOffset, cSize);
					Span<byte> sTo = wholeMemory.Slice(start + idxOffset, cSize);

					sFrom.CopyTo(sTo);
					sFrom.Fill(0);
				}

				moved = entityData[lastIdx];
				entityData[e_idx] = moved;
				entityData[lastIdx] = new Entity();
				didMove = true;
			}
			else { //remove last
				foreach (var slice in _typeLocations) {
					int cSize = slice.Value.componentSize; //component size in bytes
					int start = slice.Value.start;
					int lastIdxOffset = cSize * lastIdx;

					Span<byte> span = wholeMemory.Slice(start + lastIdxOffset, cSize);
					span.Fill(0);
				}
				entityData[lastIdx] = new Entity();
				moved = new Entity();
			}


			--_size;
			return didMove;
		}

		public int CopyEntityTo(int e_idx, in Entity e, ComponentMemoryBlock other) {
			Debug.Assert(e_idx < _size);
			Debug.Assert(!e.IsNull());
			Debug.Assert(GetEntityData()[e_idx] == e);

			Span<byte> srcMemory = _data.Memory.Span;
			Span<byte> destMemory = other._data.Memory.Span;

			int newIdx = other.AddEntity(e);
			foreach (var slice in other._typeLocations) {
				int hash = slice.Key;

				if (_typeLocations.TryGetValue(hash, out SliceValues src)) {
					SliceValues dest = slice.Value;

					int cSize = slice.Value.componentSize; //component size in bytes
					int srcStart = src.start;
					int srcIdxOffset = cSize * e_idx;

					int destStart = dest.start;
					int destIdxOffset = cSize * newIdx;

					Span<byte> sFrom = srcMemory.Slice(srcStart + srcIdxOffset, cSize);
					Span<byte> sTo = destMemory.Slice(destStart + destIdxOffset, cSize);

					sFrom.CopyTo(sTo);
				}
			}

			return newIdx;
		}
	}
}
