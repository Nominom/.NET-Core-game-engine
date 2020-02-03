using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;

namespace Core.ECS
{

	internal struct ComponentSliceValues
	{
		public int start;
		public int length;
		public int componentSize;
	}

	internal sealed class ComponentMemoryBlock : IDisposable {
		private static long nextId = 0;
		private static readonly BlockAllocator defaultAllocator = BlockAllocator.KB16;

		private int _size;
		private readonly int _maxSize;

		private readonly ComponentSliceValues entityComponentSlice;

		//private fixed byte _data[16384];
		private readonly BlockMemory _data;
		//private Dictionary<System.Type, ComponentSliceValues> _typeLocations;
		//private Dictionary<int, ComponentSliceValues> _typeLocations;
		private readonly Dictionary<int, ComponentSliceValues> _typeLocations;
		private readonly Dictionary<int, long> _componentVersions; 

		/// <summary>
		/// Unique identifier of this block.
		/// </summary>
		public readonly long id;

		public readonly EntityArchetype archetype;
		public bool HasRoom => _size < _maxSize;
		public int Size => _size;
		public int MaxSize => _maxSize;
		public long EntityVersion { get; private set; }

		public ComponentMemoryBlock(EntityArchetype archetype) : this(archetype, defaultAllocator){}

		public ComponentMemoryBlock(EntityArchetype archetype, BlockAllocator allocator) {
			id = nextId++;
			this.archetype = archetype;
			//_typeLocations = new Dictionary<Type, ComponentSliceValues>();
			//_typeLocations = new Dictionary<int, ComponentSliceValues>();
			_typeLocations = new Dictionary<int, ComponentSliceValues>();
			_componentVersions = new Dictionary<int, long>();

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
			entityComponentSlice = new ComponentSliceValues { start = 0, length = entitySize * _maxSize, componentSize = entitySize};

			int nextIdx = entitySize * _maxSize; //start from after entities
			foreach (var component in archetype.components) {
				int componentLength = component.Value * _maxSize;
				_typeLocations.Add(component.Key.GetHashCode(), new ComponentSliceValues { start = nextIdx, length = componentLength, componentSize = component.Value});
				_componentVersions.Add(component.Key.GetHashCode(), 0);
				nextIdx += componentLength;
			}
		}

		internal void IncrementComponentVersions()
		{
			foreach (var component in archetype.components) {
				int hash = component.Key.GetHashCode();
				IncrementComponentVersion(hash);
			}
		}

		internal void IncrementComponentVersion(int hash) {
			_componentVersions[hash] = _componentVersions[hash] + 1;
		}

		internal void RollbackComponentVersion<T>() {
			int hash = TypeHelper<T>.hashCode;
			_componentVersions[hash] = _componentVersions[hash] - 1;
		}

		internal long GetComponentVersion<T>() {
			return _componentVersions[TypeHelper<T>.hashCode];
		}

		internal Span<Entity> GetEntityData() {
			Span<byte> bytes = _data.Memory.Span.Slice(entityComponentSlice.start, entityComponentSlice.length);
			Span<Entity> span = MemoryMarshal.Cast<byte, Entity>(bytes);
			return span;
		}

		internal Span<T> GetComponentData<T>() where T : unmanaged, IComponent {
			DebugHelper.AssertThrow<ComponentNotFoundException>(_typeLocations.ContainsKey(TypeHelper<T>.hashCode));
			int hash = TypeHelper<T>.hashCode;
			_componentVersions[hash] = _componentVersions[hash] + 1;

			ComponentSliceValues componentSlice = _typeLocations[hash];
			Span <byte> bytes = _data.Memory.Span.Slice(componentSlice.start, componentSlice.length);
			Span<T> span = MemoryMarshal.Cast<byte, T>(bytes);
			return span;
		}

		internal Span<T> GetComponentDataNoVersionIncrement<T>() where T : unmanaged, IComponent {
			DebugHelper.AssertThrow<ComponentNotFoundException>(_typeLocations.ContainsKey(TypeHelper<T>.hashCode));
			int hash = TypeHelper<T>.hashCode;

			ComponentSliceValues componentSlice = _typeLocations[hash];
			Span <byte> bytes = _data.Memory.Span.Slice(componentSlice.start, componentSlice.length);
			Span<T> span = MemoryMarshal.Cast<byte, T>(bytes);
			return span;
		}

		internal ReadOnlySpan<T> GetReadOnlyComponentData<T>() where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ComponentNotFoundException>(_typeLocations.ContainsKey(TypeHelper<T>.hashCode));
			ComponentSliceValues componentSlice = _typeLocations[TypeHelper<T>.hashCode];
			ReadOnlySpan <byte> bytes = _data.Memory.Span.Slice(componentSlice.start, componentSlice.length);
			ReadOnlySpan<T> span = MemoryMarshal.Cast<byte, T>(bytes);
			return span;
		}

		internal Span<byte> GetRawComponentData<T>() where T : unmanaged, IComponent {
			DebugHelper.AssertThrow<ComponentNotFoundException>(_typeLocations.ContainsKey(TypeHelper<T>.hashCode));
			
			int hash = TypeHelper<T>.hashCode;
			_componentVersions[hash] = _componentVersions[hash] + 1;

			ComponentSliceValues componentSlice = _typeLocations[hash];
			Span<byte> bytes = _data.Memory.Span.Slice(componentSlice.start, componentSlice.length);
			return bytes;
		}

		internal Span<byte> GetRawComponentData(System.Type type) {
			int hash = type.GetHashCode();
			DebugHelper.AssertThrow<ComponentNotFoundException>(_typeLocations.ContainsKey(hash));
			
			_componentVersions[hash] = _componentVersions[hash] + 1;

			ComponentSliceValues componentSlice = _typeLocations[hash];
			Span<byte> bytes = _data.Memory.Span.Slice(componentSlice.start, componentSlice.length);
			return bytes;
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
			DebugHelper.AssertThrow<InvalidEntityException>(!entity.IsNull());
			DebugHelper.AssertThrow<InvalidOperationException>(_size < _maxSize);
			int idx = _size;
			GetEntityData()[idx] = entity;
			_size++;
			EntityVersion++;
			return idx;
		}



		//returns true if last entity was moved
		public bool RemoveEntityMoveLast(int e_idx, out Entity moved) {
			DebugHelper.AssertThrow<IndexOutOfRangeException>(e_idx < _size);

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

			EntityVersion++;
			--_size;
			return didMove;
		}

		public int CopyEntityTo(int e_idx, in Entity e, ComponentMemoryBlock other) {
			DebugHelper.AssertThrow<IndexOutOfRangeException>(e_idx < _size);
			DebugHelper.AssertThrow<InvalidEntityException>(!e.IsNull());
			DebugHelper.AssertThrow<ArgumentException>(GetEntityData()[e_idx] == e);

			Span<byte> srcMemory = _data.Memory.Span;
			Span<byte> destMemory = other._data.Memory.Span;

			int newIdx = other.AddEntity(e);
			foreach (var slice in other._typeLocations) {
				int hash = slice.Key;

				if (_typeLocations.TryGetValue(hash, out ComponentSliceValues src)) {
					ComponentSliceValues dest = slice.Value;

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

		public BlockAccessor GetAccessor(ComponentQuery query) {
			return new BlockAccessor(this, query);
		}

		public void IncrementVersionsByQuery(ComponentQuery query) {
			var bitset = query.GetIncludeWriteMask();
			for (int i = 0; i < ComponentMask.ComponentAmount; i++) {
				if (bitset.Get(i)) {
					var type = ComponentMask.ComponentIndexToType(i);
					int hash = type.GetHashCode();
					IncrementComponentVersion(hash);
				}
			}
		}
	}





	internal sealed unsafe class UnsafeComponentMemoryBlock : IDisposable
	{
		private struct UnsafeComponentSlice {
			public void* ptr;
			public int length;
			public int componentSize;
		}

		private static readonly UnsafeBlockAllocator allocator = UnsafeBlockAllocator.KB16;

		private int _size;
		private readonly int _maxSize;

		private readonly UnsafeComponentSlice entityComponentSlice;

		//private fixed byte _data[16384];
		private readonly UnsafeBlockMemory _data;
		//private Dictionary<System.Type, ComponentSliceValues> _typeLocations;
		//private Dictionary<int, ComponentSliceValues> _typeLocations;
		private readonly Dictionary<int, UnsafeComponentSlice> _typeLocations;



		public readonly EntityArchetype archetype;
		public bool HasRoom => _size < _maxSize;
		public int Size => _size;
		public int MaxSize => _maxSize;

		public UnsafeComponentMemoryBlock(EntityArchetype archetype)
		{
			this.archetype = archetype;
			//_typeLocations = new Dictionary<Type, ComponentSliceValues>();
			//_typeLocations = new Dictionary<int, ComponentSliceValues>();
			_typeLocations = new Dictionary<int, UnsafeComponentSlice>();


			_data = allocator.Rent();
			_data.Span.Clear();

			int amountOfBytes = _data.Size();
			int entitySize = Marshal.SizeOf<Entity>();
			int bytesPerEntity = entitySize;
			foreach (var component in archetype.components)
			{
				bytesPerEntity += component.Value;
			}

			//calculate maximum size of entities held by this memory block
			_maxSize = (int)MathF.Floor((float)amountOfBytes / (float)bytesPerEntity);
			_size = 0;

			byte* basePtr = (byte*)_data.GetPointer();

			//place entities as the first "component"
			entityComponentSlice = new UnsafeComponentSlice { ptr = basePtr, length = entitySize * _maxSize, componentSize = entitySize };

			int nextIdx = entitySize * _maxSize; //start from after entities
			foreach (var component in archetype.components)
			{
				int componentLength = component.Value * _maxSize;
				_typeLocations.Add(component.Key.GetHashCode(), new UnsafeComponentSlice { ptr = basePtr + nextIdx, length = componentLength, componentSize = component.Value });
				nextIdx += componentLength;
			}
		}

		internal Span<Entity> GetEntityData()
		{
			Span<Entity> span = new Span<Entity>(entityComponentSlice.ptr, MaxSize);
			return span;
		}

		internal Span<T> GetComponentData<T>() where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ComponentNotFoundException>(_typeLocations.ContainsKey(TypeHelper<T>.hashCode));

			UnsafeComponentSlice componentSlice = _typeLocations[TypeHelper<T>.hashCode];
			Span<T> span = new Span<T>(componentSlice.ptr, MaxSize);
			return span;
		}

		internal Span<byte> GetRawComponentData<T>() where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ComponentNotFoundException>(_typeLocations.ContainsKey(TypeHelper<T>.hashCode));

			UnsafeComponentSlice componentSlice = _typeLocations[TypeHelper<T>.hashCode];
			Span<byte> bytes = new Span<byte>(componentSlice.ptr, componentSlice.length);
			return bytes;
		}

		internal Span<byte> GetRawComponentData(System.Type type)
		{
			int hash = type.GetHashCode();
			DebugHelper.AssertThrow<ComponentNotFoundException>(_typeLocations.ContainsKey(hash));

			UnsafeComponentSlice componentSlice = _typeLocations[hash];
			Span<byte> bytes = new Span<byte>(componentSlice.ptr, componentSlice.length);
			return bytes;
		}

		~UnsafeComponentMemoryBlock()
		{
			Free();
		}

		public void Dispose()
		{
			Free();
			GC.SuppressFinalize(this);
		}

		private void Free()
		{
			_data?.Dispose();
		}

		public int AddEntity(in Entity entity)
		{
			DebugHelper.AssertThrow<InvalidEntityException>(!entity.IsNull());
			DebugHelper.AssertThrow<InvalidOperationException>(_size < _maxSize);

			int idx = _size;
			GetEntityData()[idx] = entity;
			_size++;
			return idx;
		}



		//returns true if last entity was moved
		public bool RemoveEntityMoveLast(int e_idx, out Entity moved)
		{
			DebugHelper.AssertThrow<IndexOutOfRangeException>(e_idx < _size);

			Span<Entity> entityData = GetEntityData();
			//Span<byte> wholeMemory = _data.Memory.Span;
			int lastIdx = _size - 1;

			bool didMove = false;
			if (e_idx != lastIdx)
			{//Move all component memory from last in place of e_idx
				foreach (var slice in _typeLocations)
				{
					int cSize = slice.Value.componentSize; //component size in bytes
					byte* start = (byte*)slice.Value.ptr;
					//int start = slice.Value.start;
					int idxOffset = cSize * e_idx;
					int lastIdxOffset = cSize * lastIdx;

					//Span<byte> sFrom = wholeMemory.Slice(start + lastIdxOffset, cSize);
					Span<byte> sFrom = new Span<byte>(start + lastIdxOffset, cSize);
					//Span<byte> sTo = wholeMemory.Slice(start + idxOffset, cSize);
					Span<byte> sTo = new Span<byte>(start + idxOffset, cSize);

					sFrom.CopyTo(sTo);
					sFrom.Fill(0);
				}

				moved = entityData[lastIdx];
				entityData[e_idx] = moved;
				entityData[lastIdx] = new Entity();
				didMove = true;
			}
			else
			{ //remove last
				foreach (var slice in _typeLocations)
				{
					int cSize = slice.Value.componentSize; //component size in bytes
					//int start = slice.Value.start;
					byte* start = (byte*)slice.Value.ptr;
					int lastIdxOffset = cSize * lastIdx;

					//Span<byte> span = wholeMemory.Slice(start + lastIdxOffset, cSize);
					Span<byte> span = new Span<byte>(start + lastIdxOffset, cSize);
					span.Fill(0);
				}
				entityData[lastIdx] = new Entity();
				moved = new Entity();
			}


			--_size;
			return didMove;
		}

		public int CopyEntityTo(int e_idx, in Entity e, UnsafeComponentMemoryBlock other)
		{
			DebugHelper.AssertThrow<IndexOutOfRangeException>(e_idx < _size);
			DebugHelper.AssertThrow<InvalidEntityException>(!e.IsNull());
			DebugHelper.AssertThrow<ArgumentException>(GetEntityData()[e_idx] == e);

			//Span<byte> srcMemory = _data.Memory.Span;
			//Span<byte> destMemory = other._data.Memory.Span;

			int newIdx = other.AddEntity(e);
			foreach (var slice in other._typeLocations)
			{
				int hash = slice.Key;

				if (_typeLocations.TryGetValue(hash, out UnsafeComponentSlice src))
				{
					UnsafeComponentSlice dest = slice.Value;

					int cSize = slice.Value.componentSize; //component size in bytes
					//int srcStart = src.start;
					byte* srcStart = (byte*) src.ptr;
					int srcIdxOffset = cSize * e_idx;

					//int destStart = dest.start;
					byte* destStart = (byte*)dest.ptr;
					int destIdxOffset = cSize * newIdx;

					//Span<byte> sFrom = srcMemory.Slice(srcStart + srcIdxOffset, cSize);
					Span<byte> sFrom = new Span<byte>(srcStart + srcIdxOffset, cSize);
					//Span<byte> sTo = destMemory.Slice(destStart + destIdxOffset, cSize);
					Span<byte> sTo = new Span<byte>(destStart + destIdxOffset, cSize);

					sFrom.CopyTo(sTo);
				}
			}

			return newIdx;
		}

		public UnsafeBlockAccessor GetAccessor()
		{
			return new UnsafeBlockAccessor(this);
		}
	}
}
