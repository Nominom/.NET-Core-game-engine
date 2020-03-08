using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Core.ECS.Events;
using Core.ECS.Filters;

namespace Core.ECS
{

	internal sealed class EntityArchetypeBlock
	{
		public EntityArchetype archetype;
		public List<ComponentMemoryBlock> blocks;
		private int lastUsedIndex;

		public EntityArchetypeBlock(EntityArchetype archetype)
		{
			this.archetype = archetype;
			blocks = new List<ComponentMemoryBlock>();
			lastUsedIndex = -1;
		}

		public int GetOrCreateFreeBlockIndex()
		{
			if (lastUsedIndex != -1 && blocks[lastUsedIndex].HasRoom) {
				return lastUsedIndex;
			}
			for (int i = blocks.Count - 1; i >= 0 ; i--)
			{
				if (blocks[i].HasRoom) {
					lastUsedIndex = i;
					return i;
				}
			}

			if (blocks.Count == 0) {
				blocks.Add(new ComponentMemoryBlock(archetype, BlockAllocator.KB16));
			}
			else {
				blocks.Add(new ComponentMemoryBlock(archetype, BlockAllocator.KB64));
			}

			lastUsedIndex = blocks.Count - 1;
			return blocks.Count - 1;
		}
	}

	public class ComponentManager
	{
		internal struct EntityBlockIndex
		{
			public int entityVersion;
			public int archetypeIndex;
			public int blockIndex;
			public int elementIndex;

			public static EntityBlockIndex Invalid => new EntityBlockIndex() { entityVersion = -1 };

			public bool ValidateEntityCorrect(Entity e)
			{
				return e.version == entityVersion;
			}

			public bool IsValid()
			{
				return entityVersion >= 0;
			}
		}

		#region variables

		private EntityBlockIndex[] entityList;
		private readonly List<EntityArchetypeBlock> archetypeBlocks;
		private readonly Dictionary<int, int> archetypeHashIndices;
		private ECSWorld world;
		#endregion



		#region private

		private void GrowEntityList()
		{
			var newList = new EntityBlockIndex[entityList.Length * 2];
			Array.Copy(entityList, newList, entityList.Length);
			Span<EntityBlockIndex> span = newList.AsSpan(start: entityList.Length);
			span.Fill(EntityBlockIndex.Invalid);
			entityList = newList;
		}

		private int CreateNewArchetypeBlock(EntityArchetype archetype)
		{
			archetypeBlocks.Add(new EntityArchetypeBlock(archetype));
			int idx = archetypeBlocks.Count - 1;
			archetypeHashIndices.Add(archetype.Hash, idx);
			return idx;
		}

		private int FindOrCreateArchetypeBlockIndex(EntityArchetype archetype)
		{
			if (archetypeHashIndices.TryGetValue(archetype.Hash, out int found))
			{
				return found;
			}
			else
			{
				return CreateNewArchetypeBlock(archetype);
			}
		}

		private EntityBlockIndex GetFreeBlockOf(EntityArchetype archetype)
		{
			EntityBlockIndex newIndex = new EntityBlockIndex();
			newIndex.archetypeIndex = FindOrCreateArchetypeBlockIndex(archetype);
			newIndex.blockIndex = archetypeBlocks[newIndex.archetypeIndex].GetOrCreateFreeBlockIndex();
			return newIndex;
		}

		private int ArchetypeAddComponent<T>(EntityArchetype archetype) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<InvalidOperationException>(!archetype.Has<T>());

			int newHash = archetype.Hash ^ TypeHelper<T>.hashCode;

			if (archetypeHashIndices.TryGetValue(newHash, out int found))
			{
				return found;
			}
			else
			{
				EntityArchetype newArchetype = archetype.Add<T>();
				int idx = CreateNewArchetypeBlock(newArchetype);
				return idx;
			}
		}

		private int ArchetypeRemoveComponent<T>(EntityArchetype archetype) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<InvalidOperationException>(archetype.Has<T>());
			int newHash = archetype.Hash ^ TypeHelper<T>.hashCode;

			if (archetypeHashIndices.TryGetValue(newHash, out int found))
			{
				return found;
			}
			else {
				EntityArchetype newArchetype = archetype.Remove<T>();
				int idx = CreateNewArchetypeBlock(newArchetype);
				return idx;
			}
		}

		private int ArchetypeAddSharedComponent<T>(EntityArchetype archetype, T value) where T : class, ISharedComponent {
			int newHash = archetype.Hash;
			int valueHash = value.GetHashCode();
			if (archetype.HasShared<T>()) {
				newHash ^= archetype.GetShared<T>().GetHashCode();
				newHash ^= valueHash;
			}
			else {
				newHash ^= TypeHelper<T>.hashCode;
				newHash ^= valueHash;
			}

			EntityArchetype newArchetype;
#if DEBUG
			newArchetype = archetype.AddShared<T>(value);
			DebugHelper.AssertThrow<ArithmeticException>(newArchetype.Hash == newHash);
#endif

			if (archetypeHashIndices.TryGetValue(newHash, out int found))
			{
				return found;
			}
			else
			{
				newArchetype = archetype.AddShared<T>(value);
				int idx = CreateNewArchetypeBlock(newArchetype);
				return idx;
			}
		}

		private int ArchetypeRemoveSharedComponent<T>(EntityArchetype archetype) where T : class, ISharedComponent {
			DebugHelper.AssertThrow<InvalidOperationException>(archetype.HasShared<T>());
			int newHash = archetype.Hash;
			if (archetype.HasShared<T>()) {
				newHash ^= archetype.GetShared<T>().GetHashCode();
				newHash ^= TypeHelper<T>.hashCode;
			}

			EntityArchetype newArchetype;
#if DEBUG
			newArchetype = archetype.RemoveShared<T>();
			DebugHelper.AssertThrow<ArithmeticException>(newArchetype.Hash == newHash);
#endif

			if (archetypeHashIndices.TryGetValue(newHash, out int found)) {
				return found;
			}
			else {
				newArchetype = archetype.RemoveShared<T>();
				int idx = CreateNewArchetypeBlock(newArchetype);
				return idx;
			}
		}

		private ComponentMemoryBlock GetMemoryBlock(in EntityBlockIndex index)
		{
			return archetypeBlocks[index.archetypeIndex].blocks[index.blockIndex];
		}

		private EntityArchetype GetArchetype(in EntityBlockIndex index)
		{
			return archetypeBlocks[index.archetypeIndex].archetype;
		}

		#endregion

		#region internal
		internal void AddEntity(in Entity entity, EntityArchetype archetype)
		{
			if (entity.IsNull())
			{
				throw new InvalidEntityException();
			}

			if (entityList.Length <= entity.id)
			{
				GrowEntityList();
			}
			var idx = GetFreeBlockOf(archetype);
			idx.elementIndex = GetMemoryBlock(idx).AddEntity(entity);
			idx.entityVersion = entity.version;

			entityList[entity.id] = idx;

			foreach (var componentType in archetype.components) {
				ComponentEventHelper.FireComponentAddedEvent(world, entity, componentType.Key);
			}
		}

		internal void RemoveEntity(in Entity entity)
		{
			if (!IsEntityValid(entity))
			{
				throw new InvalidEntityException();
			}

			EntityBlockIndex old = entityList[entity.id];
			ComponentMemoryBlock block = GetMemoryBlock(old);

			foreach (var componentType in archetypeBlocks[old.archetypeIndex].archetype.components) {
				var eventCreator = ComponentEventHelper.GetEventCreator(componentType.Key);
				// EventCreator is only available if a system is subscribed to the component
				if (eventCreator != null) {
					var data = block.GetRawReadonlyComponentDataAtIndex(componentType.Key, old.elementIndex);
					eventCreator.FireComponentRemovedEvent(world, entity, data);
				}
			}

			if (block.RemoveEntityMoveLast(old.elementIndex, out Entity moved))
			{
				entityList[moved.id].elementIndex = old.elementIndex;
			}

			entityList[entity.id] = EntityBlockIndex.Invalid;

			
		}

		internal bool IsEntityValid(Entity entity)
		{
			if (entity.id >= entityList.Length) return false;
			if (entity.IsNull()) return false;
			return entityList[entity.id].ValidateEntityCorrect(entity);
		}

		internal IEnumerable<ComponentMemoryBlock> FilterBlocks(ComponentQuery query) {
			foreach (EntityArchetypeBlock archetypeBlock in archetypeBlocks) {
				if (query.Matches(archetypeBlock.archetype)) {
					foreach (ComponentMemoryBlock block in archetypeBlock.blocks) {
						if (block.Size > 0) {
							yield return block;
						}
					}
				}
			}
		}

		internal void AddPrefabComponents(Entity entity, Prefab prefab) {
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));
			world.SyncPoint();
			EntityBlockIndex index = entityList[entity.id];
			ComponentMemoryBlock block = GetMemoryBlock(index);

			Span<byte> prefabBytes = prefab.componentData;

			for (int i = 0; i < prefab.componentTypes.Count; i++) {
				Type componentType = prefab.componentTypes[i];
				var slice = prefab.componentSizes[i];

				Span<byte> rawBlockData = block.GetRawComponentData(componentType);
				Span<byte> source = prefabBytes.Slice(slice.start, slice.length);
				Span<byte> destination = rawBlockData.Slice(index.elementIndex * slice.componentSize, slice.length);

				source.CopyTo(destination);
			}
		}

		#endregion


		#region public

		public ComponentManager(ECSWorld world) {
			this.world = world;
			entityList = new EntityBlockIndex[16];
			for (var i = 0; i < entityList.Length; i++)
			{
				entityList[i] = EntityBlockIndex.Invalid;
			}
			archetypeBlocks = new List<EntityArchetypeBlock>();
			archetypeHashIndices = new Dictionary<int, int>();

			//add default archetype with hash zero
			archetypeBlocks.Add(new EntityArchetypeBlock(EntityArchetype.Empty));
			archetypeHashIndices.Add(0, 0);
		}

		public void AddComponent<T>(Entity entity, in T component) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (!IsEntityValid(entity)) {
				throw new InvalidEntityException();
			}
			world.SyncPoint();

			EntityBlockIndex oldIndex = entityList[entity.id];
			EntityArchetype oldArchetype = GetArchetype(oldIndex);
			ComponentMemoryBlock oldBlock = GetMemoryBlock(oldIndex);


			if (oldArchetype.Has<T>()) // Entity already has component. Just set new value.
			{
				oldBlock.GetComponentData<T>()[oldIndex.elementIndex] = component;
			}
			else
			{

				EntityBlockIndex newIndex = oldIndex;
				newIndex.archetypeIndex = ArchetypeAddComponent<T>(oldArchetype);
				newIndex.blockIndex = archetypeBlocks[newIndex.archetypeIndex].GetOrCreateFreeBlockIndex();

				ComponentMemoryBlock newBlock = GetMemoryBlock(newIndex);

				newIndex.elementIndex = oldBlock.CopyEntityTo(oldIndex.elementIndex, entity, newBlock);

				if (oldBlock.RemoveEntityMoveLast(oldIndex.elementIndex, out Entity moved))
				{
					entityList[moved.id].elementIndex = oldIndex.elementIndex;
				}

				newBlock.GetComponentData<T>()[newIndex.elementIndex] = component;
				entityList[entity.id] = newIndex;

				ComponentEventHelper.FireComponentAddedEvent(world, entity, typeof(T));
			}

		}

		public void SetComponent<T>(Entity entity, in T component) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (!IsEntityValid(entity)) {
				throw new InvalidEntityException();
			}
			world.SyncPoint();

			EntityBlockIndex index = entityList[entity.id];
			EntityArchetype archetype = GetArchetype(index);
			ComponentMemoryBlock block = GetMemoryBlock(index);


			if (archetype.Has<T>())
			{
				block.GetComponentData<T>()[index.elementIndex] = component;
			}
		}

		public void RemoveComponent<T>(Entity entity) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (!IsEntityValid(entity)) {
				throw new InvalidEntityException();
			}
			world.SyncPoint();

			EntityBlockIndex oldIndex = entityList[entity.id];
			EntityArchetype oldArchetype = GetArchetype(oldIndex);

			if (!oldArchetype.Has<T>()) //Entity doesn't have this component. Do nothing.
			{
				return;
			}
			ComponentMemoryBlock oldBlock = GetMemoryBlock(oldIndex);

			T oldData = oldBlock.GetReadOnlyComponentData<T>()[oldIndex.elementIndex];
			new ComponentRemovedEvent<T>(entity, oldData).Fire(world);

			EntityBlockIndex newIndex = oldIndex;
			newIndex.archetypeIndex = ArchetypeRemoveComponent<T>(oldArchetype);
			newIndex.blockIndex = archetypeBlocks[newIndex.archetypeIndex].GetOrCreateFreeBlockIndex();

			ComponentMemoryBlock newBlock = GetMemoryBlock(newIndex);

			newIndex.elementIndex = oldBlock.CopyEntityTo(oldIndex.elementIndex, entity, newBlock);

			if (oldBlock.RemoveEntityMoveLast(oldIndex.elementIndex, out Entity moved))
			{
				entityList[moved.id].elementIndex = oldIndex.elementIndex;
			}

			entityList[entity.id] = newIndex;
		}

		public ref T GetComponent<T>(Entity entity) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (!IsEntityValid(entity)) {
				throw new InvalidEntityException();
			}
			world.SyncPoint();

			EntityBlockIndex index = entityList[entity.id];
			ComponentMemoryBlock block = GetMemoryBlock(index);

			if (!block.archetype.Has<T>()) {
				throw new ComponentNotFoundException();
			}

			return ref block.GetComponentData<T>()[index.elementIndex];
		}

		public bool TryGetComponent<T>(Entity entity, out T component) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (!IsEntityValid(entity)) {
				throw new InvalidEntityException();
			}
			world.SyncPoint();

			EntityBlockIndex index = entityList[entity.id];
			ComponentMemoryBlock block = GetMemoryBlock(index);

			if (!block.archetype.Has<T>()) {
				component = default;
				return false;
			}

			component = block.GetReadOnlyComponentData<T>()[index.elementIndex];
			return true;
		}

		public bool HasComponent<T>(Entity entity) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (!IsEntityValid(entity)) {
				throw new InvalidEntityException();
			}

			EntityBlockIndex index = entityList[entity.id];
			ComponentMemoryBlock block = GetMemoryBlock(index);

			return block.archetype.Has<T>();
		}


		public void AddSharedComponent<T>(Entity entity, T component) where T : class, ISharedComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (!IsEntityValid(entity)) {
				throw new InvalidEntityException();
			}
			world.SyncPoint();

			EntityBlockIndex oldIndex = entityList[entity.id];
			EntityArchetype oldArchetype = GetArchetype(oldIndex);
			ComponentMemoryBlock oldBlock = GetMemoryBlock(oldIndex);

			EntityBlockIndex newIndex = oldIndex;
			newIndex.archetypeIndex = ArchetypeAddSharedComponent<T>(oldArchetype, component);

			if (oldIndex.archetypeIndex != newIndex.archetypeIndex) {
				newIndex.blockIndex = archetypeBlocks[newIndex.archetypeIndex].GetOrCreateFreeBlockIndex();

				ComponentMemoryBlock newBlock = GetMemoryBlock(newIndex);

				newIndex.elementIndex = oldBlock.CopyEntityTo(oldIndex.elementIndex, entity, newBlock);

				if (oldBlock.RemoveEntityMoveLast(oldIndex.elementIndex, out Entity moved))
				{
					entityList[moved.id].elementIndex = oldIndex.elementIndex;
				}

				entityList[entity.id] = newIndex;
			}
		}

		public void RemoveSharedComponent<T>(Entity entity) where T : class, ISharedComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (!IsEntityValid(entity)) {
				throw new InvalidEntityException();
			}
			world.SyncPoint();

			EntityBlockIndex oldIndex = entityList[entity.id];
			EntityArchetype oldArchetype = GetArchetype(oldIndex);

			if (!oldArchetype.HasShared<T>()) //Entity doesn't have this component. Do nothing.
			{
				return;
			}
			ComponentMemoryBlock oldBlock = GetMemoryBlock(oldIndex);

			EntityBlockIndex newIndex = oldIndex;
			newIndex.archetypeIndex = ArchetypeRemoveSharedComponent<T>(oldArchetype);
			newIndex.blockIndex = archetypeBlocks[newIndex.archetypeIndex].GetOrCreateFreeBlockIndex();

			ComponentMemoryBlock newBlock = GetMemoryBlock(newIndex);

			newIndex.elementIndex = oldBlock.CopyEntityTo(oldIndex.elementIndex, entity, newBlock);

			if (oldBlock.RemoveEntityMoveLast(oldIndex.elementIndex, out Entity moved))
			{
				entityList[moved.id].elementIndex = oldIndex.elementIndex;
			}

			entityList[entity.id] = newIndex;
		}

		public T GetSharedComponent<T>(Entity entity) where T : class, ISharedComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (!IsEntityValid(entity)) {
				throw new InvalidEntityException();
			}
			world.SyncPoint();
			EntityBlockIndex index = entityList[entity.id];
			var archetype = archetypeBlocks[index.archetypeIndex].archetype;
			return archetype.GetShared<T>();
		}

		public bool TryGetSharedComponent<T>(Entity entity, out T component) where T : class, ISharedComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (!IsEntityValid(entity)) {
				throw new InvalidEntityException();
			}
			world.SyncPoint();
			EntityBlockIndex index = entityList[entity.id];
			var archetype = archetypeBlocks[index.archetypeIndex].archetype;

			if (!archetype.HasShared<T>()) {
				component = null;
				return false;
			}

			component = archetype.GetShared<T>();
			return true;
		}

		public bool HasSharedComponent<T>(Entity entity) where T : class, ISharedComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (!IsEntityValid(entity)) {
				throw new InvalidEntityException();
			}

			EntityBlockIndex index = entityList[entity.id];
			return archetypeBlocks[index.archetypeIndex].archetype.HasShared<T>();
		}
		#endregion




		#region block_accessing
		///// <summary>
		///// Get the blocks that match the query. This method will cause a world sync.
		///// </summary>
		//public IEnumerable<BlockAccessor> GetBlocks(ComponentQuery query)
		//{
		//	DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
		//	world.SyncPoint();
		//	foreach (var block in FilterBlocks(query))
		//	{
		//		yield return block.GetAccessor(query);
		//	}
		//}

		///// <summary>
		///// Get the blocks that match the query and the filter. This method will cause a world sync.
		///// </summary>
		//public IEnumerable<BlockAccessor> GetBlocks(ComponentQuery query, IComponentFilter filter) {
		//	if (filter == null) filter = ComponentFilters.Empty();

		//	DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
		//	world.SyncPoint();
		//	foreach (var block in FilterBlocks(query)) {
		//		var accessor = block.GetAccessor(query);
		//		if (!filter.FilterBlock(accessor)) {
		//			accessor.CommitVersion();
		//			filter.UpdateFilter(accessor);
		//			yield return accessor;
		//		}
		//	}
		//}

		///// <summary>
		///// Get the blocks that match the query. This method will not cause a world sync.
		///// Use when blocks will be passed off to a Job for processing and access is synchronized with
		///// JobGroups.
		///// </summary>
		//public IEnumerable<BlockAccessor> GetBlocksNoSync(ComponentQuery query)
		//{
		//	DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
		//	foreach (var block in FilterBlocks(query)) {
		//		var accessor = block.GetAccessor(query);
		//		accessor.CommitVersion(); // Pre increment versions because filters might not filter correctly if versions are incremented in job.
		//		yield return accessor;
		//	}
		//}

		//public IEnumerable<BlockAccessor> GetBlocksNoSync(ComponentQuery query, IComponentFilter filter) {
		//	if (filter == null) filter = ComponentFilters.Empty();
		//	DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());

		//	foreach (var block in FilterBlocks(query)) {
		//		var accessor = block.GetAccessor(query);
		//		if (!filter.FilterBlock(accessor)) {
		//			accessor.CommitVersion(); // Pre increment versions because filters might not filter correctly if versions are incremented in job.
		//			filter.UpdateFilter(accessor);
		//			yield return accessor;
		//		}
		//	}
		//}

		/// <summary>
		/// Get the blocks that match the query. This method will cause a world sync.
		/// </summary>
		public BlockEnumerable GetBlocks(ComponentQuery query)
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			world.SyncPoint();
			
			return new BlockEnumerable(this, query, null);
		}

		/// <summary>
		/// Get the blocks that match the query and the filter. This method will cause a world sync.
		/// </summary>
		public BlockEnumerable GetBlocks(ComponentQuery query, IComponentFilter filter) {
			if (filter == null) filter = ComponentFilters.Empty();

			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			world.SyncPoint();
			
			return new BlockEnumerable(this, query, filter);
		}

		/// <summary>
		/// Get the blocks that match the query. This method will not cause a world sync.
		/// Use when blocks will be passed off to a Job for processing and access is synchronized with
		/// JobGroups.
		/// </summary>
		public BlockEnumerable GetBlocksNoSync(ComponentQuery query)
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			
			return new BlockEnumerable(this, query, null);
		}

		public BlockEnumerable GetBlocksNoSync(ComponentQuery query, IComponentFilter filter) {
			if (filter == null) filter = ComponentFilters.Empty();
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());

			return new BlockEnumerable(this, query, filter);
		}

		#endregion

		internal void CleanUp() {
			foreach (var archetypeBlock in archetypeBlocks) {
				foreach (var block in archetypeBlock.blocks) {
					block.Dispose();
				}
				archetypeBlock.blocks.Clear();
			}
		}

		public struct BlockEnumerable : IEnumerable<BlockAccessor> {

			internal ComponentManager cm;
			internal IComponentFilter filter;
			internal ComponentQuery query;

			internal BlockEnumerable(ComponentManager cm, ComponentQuery query, IComponentFilter filter) {
				this.cm = cm;
				this.filter = filter;
				this.query = query;
			}


			/// <summary>
			/// For foreach to work, but with 0 allocations
			/// </summary>
			public BlockEnumerator GetEnumerator() {
				return new BlockEnumerator(cm, query, filter);
			}

			IEnumerator<BlockAccessor> IEnumerable<BlockAccessor>.GetEnumerator() {
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}
		}


		public struct BlockEnumerator : IEnumerator<BlockAccessor> {
			internal ComponentManager cm;
			internal IComponentFilter filter;
			internal ComponentQuery query;
			private int nextArchetypeIdx;
			private int currentBlockIdx;
			private EntityArchetypeBlock currentArchetypeBlock;
			private BlockAccessor currentAccessor;

			internal BlockEnumerator(ComponentManager cm, ComponentQuery query, IComponentFilter filter) {
				this.cm = cm;
				this.query = query;
				this.filter = filter;

				nextArchetypeIdx = 0;
				currentBlockIdx = -1;
				currentArchetypeBlock = null;
				currentAccessor = new BlockAccessor();
			}

			private bool ScanToNextBlock() {
				while (nextArchetypeIdx < cm.archetypeBlocks.Count) {
					if (query.Matches(cm.archetypeBlocks[nextArchetypeIdx].archetype)) {
						currentArchetypeBlock = cm.archetypeBlocks[nextArchetypeIdx];
						nextArchetypeIdx++;
						return true;
					}
					else {
						nextArchetypeIdx++;
					}
				}
				return false;
			}

			public bool MoveNext() {
				if (cm.archetypeBlocks.Count == 0) return false;
				if (currentArchetypeBlock == null) {
					if (!ScanToNextBlock()) {
						return false;
					}
				}

				while (true) {
					currentBlockIdx++;
					if (currentBlockIdx >= currentArchetypeBlock.blocks.Count) {
						if(!ScanToNextBlock())
						{
							return false;
						}
						currentBlockIdx = 0;
					}

					if (currentArchetypeBlock.blocks[currentBlockIdx].Size == 0) {
						continue;
					}

					var accessor = currentArchetypeBlock.blocks[currentBlockIdx].GetAccessor(query);
					if (filter != null &&
					    filter.FilterBlock(accessor)) {
						continue;
					}

					currentAccessor = accessor;
					currentAccessor.CommitVersion();
					filter?.UpdateFilter(currentAccessor);
					return true;
				}
			}

			public void Reset() {
				nextArchetypeIdx = 0;
				currentBlockIdx = -1;
				currentAccessor = new BlockAccessor();
			}

			public BlockAccessor Current => currentAccessor;

			object IEnumerator.Current => Current;

			public void Dispose() {
				Reset();
			}
		}
	}
}
