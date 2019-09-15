using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace ECSCore
{

	internal struct EntityArchetypeBlock
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
			for (int i = 0; i < blocks.Count; i++)
			{
				if (blocks[i].HasRoom) return i;
			}
			blocks.Add(new ComponentMemoryBlock(archetype));
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

			public bool ValidateEntityCorrect(in Entity e)
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

		private int CreateNewArchetypeBlock(in EntityArchetype archetype)
		{
			archetypeBlocks.Add(new EntityArchetypeBlock(archetype));
			int idx = archetypeBlocks.Count - 1;
			archetypeHashIndices.Add(archetype.Hash, idx);
			return idx;
		}

		private int FindOrCreateArchetypeBlockIndex(in EntityArchetype archetype)
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

		private EntityBlockIndex GetFreeBlockOf(in EntityArchetype archetype)
		{
			EntityBlockIndex newIndex = new EntityBlockIndex();
			newIndex.archetypeIndex = FindOrCreateArchetypeBlockIndex(archetype);
			newIndex.blockIndex = archetypeBlocks[newIndex.archetypeIndex].GetOrCreateFreeBlockIndex();
			return newIndex;
		}

		private int ArchetypeAddComponent<T>(in EntityArchetype archetype) where T : unmanaged, IComponent
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

		private int ArchetypeRemoveComponent<T>(in EntityArchetype archetype) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<InvalidOperationException>(archetype.Has<T>());
			int newHash = archetype.Hash ^ TypeHelper<T>.hashCode;

			if (archetypeHashIndices.TryGetValue(newHash, out int found))
			{
				return found;
			}
			else
			{
				EntityArchetype newArchetype = archetype.Remove<T>();
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

		internal bool IsEntityValid(in Entity entity)
		{
			if (entity.id >= entityList.Length) return false;
			if (entity.IsNull()) return false;
			return entityList[entity.id].ValidateEntityCorrect(entity);
		}
		#endregion


		#region public

		public ComponentManager()
		{
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

		public void AddComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));

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

		public void RemoveComponent<T>(in Entity entity) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));

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

		public ref T GetComponent<T>(in Entity entity) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));

			EntityBlockIndex index = entityList[entity.id];
			ComponentMemoryBlock block = GetMemoryBlock(index);

			DebugHelper.AssertThrow<ComponentNotFoundException>(block.archetype.Has<T>());

			return ref block.GetComponentData<T>()[index.elementIndex];
		}
		#endregion

		public bool HasComponent<T>(Entity entity) where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<InvalidEntityException>(IsEntityValid(entity));

			EntityBlockIndex index = entityList[entity.id];
			ComponentMemoryBlock block = GetMemoryBlock(index);

			return block.archetype.Has<T>();
		}
	}
}
