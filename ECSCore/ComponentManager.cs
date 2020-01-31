using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;

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
		}

		internal void RemoveEntity(in Entity entity)
		{
			if (!IsEntityValid(entity))
			{
				throw new InvalidEntityException();
			}

			EntityBlockIndex old = entityList[entity.id];
			ComponentMemoryBlock block = GetMemoryBlock(old);

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
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));
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
			}

		}

		public void SetComponent<T>(Entity entity, in T component) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));
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
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));
			world.SyncPoint();

			EntityBlockIndex oldIndex = entityList[entity.id];
			EntityArchetype oldArchetype = GetArchetype(oldIndex);

			if (!oldArchetype.Has<T>()) //Entity doesn't have this component. Do nothing.
			{
				return;
			}
			ComponentMemoryBlock oldBlock = GetMemoryBlock(oldIndex);

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
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));
			world.SyncPoint();

			EntityBlockIndex index = entityList[entity.id];
			ComponentMemoryBlock block = GetMemoryBlock(index);

			DebugHelper.AssertThrow<ComponentNotFoundException>(block.archetype.Has<T>());

			return ref block.GetComponentData<T>()[index.elementIndex];
		}

		public bool HasComponent<T>(Entity entity) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));

			EntityBlockIndex index = entityList[entity.id];
			ComponentMemoryBlock block = GetMemoryBlock(index);

			return block.archetype.Has<T>();
		}


		public void AddSharedComponent<T>(Entity entity, T component) where T : class, ISharedComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));
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
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));
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
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));
			world.SyncPoint();
			EntityBlockIndex index = entityList[entity.id];
			var archetype = archetypeBlocks[index.archetypeIndex].archetype;
			return archetype.GetShared<T>();
		}

		public bool HasSharedComponent<T>(Entity entity) where T : class, ISharedComponent
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));

			EntityBlockIndex index = entityList[entity.id];
			return archetypeBlocks[index.archetypeIndex].archetype.HasShared<T>();
		}
		#endregion




		#region block_accessing
		/// <summary>
		/// Get the blocks that match the query. This method will cause a world sync.
		/// </summary>
		public IEnumerable<BlockAccessor> GetBlocks(ComponentQuery query)
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			world.SyncPoint();
			foreach (var block in FilterBlocks(query))
			{
				yield return block.GetAccessor();
			}
		}

		/// <summary>
		/// Get the blocks that match the query. This method will not cause a world sync.
		/// Use when blocks will be passed off to a Job for processing and access is synchronized with
		/// JobGroups.
		/// </summary>
		public IEnumerable<BlockAccessor> GetBlocksNoSync(ComponentQuery query)
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			foreach (var block in FilterBlocks(query))
			{
				yield return block.GetAccessor();
			}
		}

		/// <summary>
		/// Get the blocks that match the archetype. This method will cause a world sync.
		/// </summary>
		public IEnumerable<BlockAccessor> GetBlocks(EntityArchetype archetype)
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			world.SyncPoint();
			if (archetypeHashIndices.TryGetValue(archetype.Hash, out int index)) {
				foreach (var block in archetypeBlocks[index].blocks)
				{
					yield return block.GetAccessor();
				}
			}
		}

		/// <summary>
		/// Get the blocks that match the archetype. This method will not cause a world sync.
		/// Use when blocks will be passed off to a Job for processing and access is synchronized with
		/// JobGroups.
		/// </summary>
		public IEnumerable<BlockAccessor> GetBlocksNoSync(EntityArchetype archetype)
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (archetypeHashIndices.TryGetValue(archetype.Hash, out int index)) {
				foreach (var block in archetypeBlocks[index].blocks)
				{
					yield return block.GetAccessor();
				}
			}
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
	}
}
