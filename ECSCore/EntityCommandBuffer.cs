using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Core.ECS.JobSystem;

namespace Core.ECS
{
	public static class EntityCommandBufferExtensions
	{
		public static void PlaybackAfterUpdate(this EntityCommandBuffer buffer)
		{
			buffer.world.RegisterForExecuteAfterUpdate(buffer);
		}
		public static void PlaybackAfterUpdate(this ConcurrentEntityCommandBuffer buffer)
		{
			buffer.world.RegisterForExecuteAfterUpdate(buffer);
		}
	}

	public interface IEntityCommandBuffer
	{
		void CreateEntity(EntityArchetype archetype);

		void CreateEntity(Prefab prefab);

		void DestroyEntity(Entity entity);

		void SetComponent<T>(T component) where T : unmanaged, IComponent;

		void AddComponent<T>(T component) where T : unmanaged, IComponent;

		void AddSharedComponent<T>(T component) where T : class, ISharedComponent;

		void SetComponent<T>(Entity entity, T component) where T : unmanaged, IComponent;

		void AddComponent<T>(Entity entity, T component) where T : unmanaged, IComponent;

		void AddSharedComponent<T>(Entity entity, T component) where T : class, ISharedComponent;

		void RemoveComponent<T>(Entity entity) where T : unmanaged, IComponent;

		void RemoveSharedComponent<T>(Entity entity) where T : class, ISharedComponent;

		void Playback();

		bool IsEmpty();

	}

	public sealed class ConcurrentEntityCommandBuffer : IEntityCommandBuffer
	{

		private readonly ThreadLocal<EntityCommandBuffer> threadBuffers = new ThreadLocal<EntityCommandBuffer>(true);
		private List<EntityCommandBuffer> threadBuffersCache = null;
		internal readonly ECSWorld world;

		public ConcurrentEntityCommandBuffer(ECSWorld world)
		{
			this.world = world;
		}

		private EntityCommandBuffer GetLocal()
		{
			if (!threadBuffers.IsValueCreated)
			{
				threadBuffers.Value = new EntityCommandBuffer(world);
				threadBuffersCache = null;
			}
			return threadBuffers.Value;
		}

		public void CreateEntity(Prefab prefab)
		{
			GetLocal().CreateEntity(prefab);
		}

		public void DestroyEntity(Entity entity) {
			GetLocal().DestroyEntity(entity);
		}

		public void SetComponent<T>(T component) where T : unmanaged, IComponent
		{
			GetLocal().SetComponent(component);
		}

		public void AddComponent<T>(T component) where T : unmanaged, IComponent
		{
			GetLocal().AddComponent(component);
		}

		public void AddSharedComponent<T>(T component) where T : class, ISharedComponent
		{
			GetLocal().AddSharedComponent(component);
		}

		public void SetComponent<T>(Entity entity, T component) where T : unmanaged, IComponent
		{
			GetLocal().SetComponent(entity, component);
		}

		public void AddComponent<T>(Entity entity, T component) where T : unmanaged, IComponent
		{
			GetLocal().AddComponent(entity, component);
		}

		public void AddSharedComponent<T>(Entity entity, T component) where T : class, ISharedComponent
		{
			GetLocal().AddSharedComponent(entity, component);
		}

		public void RemoveComponent<T>(Entity entity) where T : unmanaged, IComponent
		{
			GetLocal().RemoveComponent<T>(entity);
		}

		public void RemoveSharedComponent<T>(Entity entity) where T : class, ISharedComponent
		{
			GetLocal().RemoveSharedComponent<T>(entity);
		}

		public void Playback()
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (threadBuffersCache == null)
			{
				threadBuffersCache = threadBuffers.Values as List<EntityCommandBuffer>;
			}
			world.SyncPoint();
			foreach (EntityCommandBuffer buffer in threadBuffersCache)
			{
				buffer.Playback();
			}
		}

		public bool IsEmpty()
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			Jobs.CompleteAllJobs();
			if (threadBuffersCache == null)
			{
				threadBuffersCache = threadBuffers.Values as List<EntityCommandBuffer>;
			}
			foreach (EntityCommandBuffer buffer in threadBuffersCache)
			{
				if (!buffer.IsEmpty()) return false;
			}

			return true;
		}

		public void CreateEntity(EntityArchetype archetype)
		{
			GetLocal().CreateEntity(archetype);
		}
	}

	public sealed class EntityCommandBuffer : IEntityCommandBuffer
	{
		private interface IModification
		{
			void Execute(EntityCommandBuffer buffer);
		}

		private class CreateEntityArchetypeMod : IModification
		{
			public EntityArchetype archetype;
			public CreateEntityArchetypeMod(EntityArchetype archetype)
			{
				this.archetype = archetype;
			}

			public void Execute(EntityCommandBuffer buffer)
			{
				buffer.entityTarget = buffer.world.Instantiate(archetype);
			}
		}

		private class CreateEntityPrefabMod : IModification
		{
			public Prefab prefab;

			public CreateEntityPrefabMod(Prefab prefab)
			{
				this.prefab = prefab;
			}

			public void Execute(EntityCommandBuffer buffer)
			{
				buffer.entityTarget = buffer.world.Instantiate(prefab);
			}
		}

		private class DestroyEntityMod : IModification {
			public Entity entity;

			public DestroyEntityMod(Entity entity)
			{
				this.entity = entity;
			}

			public void Execute(EntityCommandBuffer buffer)
			{
				buffer.world.EntityManager.DestroyEntity(entity);
			}
		}

		private class SetComponentMod<T> : IModification where T : unmanaged, IComponent
		{
			public T component;

			public SetComponentMod(T component)
			{
				this.component = component;
			}

			public void Execute(EntityCommandBuffer buffer)
			{
				buffer.world.ComponentManager.SetComponent(buffer.entityTarget, component);
			}
		}

		private class AddComponentMod<T> : IModification where T : unmanaged, IComponent
		{
			public T component;

			public AddComponentMod(T component)
			{
				this.component = component;
			}

			public void Execute(EntityCommandBuffer buffer)
			{
				buffer.world.ComponentManager.AddComponent(buffer.entityTarget, component);
			}
		}

		private class AddSharedComponentMod<T> : IModification where T : class, ISharedComponent
		{
			public T component;

			public AddSharedComponentMod(T component)
			{
				this.component = component;
			}

			public void Execute(EntityCommandBuffer buffer)
			{
				buffer.world.ComponentManager.AddSharedComponent(buffer.entityTarget, component);

			}
		}

		private class SetComponentEntityMod<T> : IModification where T : unmanaged, IComponent
		{
			public Entity entity;
			public T component;

			public SetComponentEntityMod(Entity entity, T component)
			{
				this.entity = entity;
				this.component = component;
			}

			public void Execute(EntityCommandBuffer buffer)
			{
				buffer.world.ComponentManager.SetComponent(entity, component);
			}
		}

		private class AddComponentEntityMod<T> : IModification where T : unmanaged, IComponent
		{
			public Entity entity;
			public T component;

			public AddComponentEntityMod(Entity entity, T component)
			{
				this.entity = entity;
				this.component = component;
			}

			public void Execute(EntityCommandBuffer buffer)
			{
				buffer.world.ComponentManager.AddComponent(entity, component);

			}
		}

		private class AddSharedComponentEntityMod<T> : IModification where T : class, ISharedComponent
		{
			public Entity entity;
			public T component;

			public AddSharedComponentEntityMod(Entity entity, T component)
			{
				this.entity = entity;
				this.component = component;
			}

			public void Execute(EntityCommandBuffer buffer)
			{
				buffer.world.ComponentManager.AddSharedComponent(entity, component);
			}
		}

		private class RemoveComponentEntityMod<T> : IModification where T : unmanaged, IComponent
		{
			public Entity entity;

			public RemoveComponentEntityMod(Entity entity)
			{
				this.entity = entity;
			}

			public void Execute(EntityCommandBuffer buffer)
			{
				buffer.world.ComponentManager.RemoveComponent<T>(entity);
			}
		}

		private class RemoveSharedComponentEntityMod<T> : IModification where T : class, ISharedComponent
		{
			public Entity entity;

			public RemoveSharedComponentEntityMod(Entity entity)
			{
				this.entity = entity;
			}

			public void Execute(EntityCommandBuffer buffer)
			{
				buffer.world.ComponentManager.RemoveSharedComponent<T>(entity);
			}
		}

		internal readonly ECSWorld world;
		private Entity entityTarget;
		private int nextIndex = 0;
		private IModification?[] modList = Array.Empty<IModification>();
		private ArrayPool<IModification> pool = ArrayPool<IModification>.Shared;
		private bool hasEntityTarget = false;

		private void GrowModificationList()
		{
			int newListLength = modList.Length * 2;
			if (modList.Length == 0)
			{
				newListLength = 32;
			}

			IModification?[] newList = pool.Rent(newListLength);
			Span<IModification?> newSpan = newList;
			Span<IModification?> oldSpan = modList;

			newSpan.Clear();
			oldSpan.CopyTo(newSpan);

			pool.Return(modList);
			modList = newList;
		}

		public EntityCommandBuffer(ECSWorld world)
		{
			this.world = world;
		}

		public void CreateEntity(EntityArchetype archetype)
		{
			if (nextIndex >= modList.Length)
			{
				GrowModificationList();
			}
			hasEntityTarget = true;

			modList[nextIndex++] = new CreateEntityArchetypeMod(archetype);
		}

		public void CreateEntity(Prefab prefab)
		{
			if (nextIndex >= modList.Length)
			{
				GrowModificationList();
			}
			hasEntityTarget = true;

			modList[nextIndex++] = new CreateEntityPrefabMod(prefab);
		}

		public void DestroyEntity(Entity entity) {
			if (nextIndex >= modList.Length)
			{
				GrowModificationList();
			}

			modList[nextIndex++] = new DestroyEntityMod(entity);
		}

		public void SetComponent<T>(T component) where T : unmanaged, IComponent
		{
			if (nextIndex >= modList.Length)
			{
				GrowModificationList();
			}
			DebugHelper.AssertThrow(hasEntityTarget, () => new InvalidOperationException("An entity creation function has to be called before calling component modifier functions."));

			modList[nextIndex++] = new SetComponentMod<T>(component);
		}

		public void AddComponent<T>(T component) where T : unmanaged, IComponent
		{
			if (nextIndex >= modList.Length)
			{
				GrowModificationList();
			}
			DebugHelper.AssertThrow(hasEntityTarget, () => new InvalidOperationException("An entity creation function has to be called before calling component modifier functions."));

			modList[nextIndex++] = new AddComponentMod<T>(component);
		}

		public void AddSharedComponent<T>(T component) where T : class, ISharedComponent
		{
			if (nextIndex >= modList.Length)
			{
				GrowModificationList();
			}
			DebugHelper.AssertThrow(hasEntityTarget, () => new InvalidOperationException("An entity creation function has to be called before calling component modifier functions."));

			modList[nextIndex++] = new AddSharedComponentMod<T>(component);
		}

		public void SetComponent<T>(Entity entity, T component) where T : unmanaged, IComponent
		{
			if (nextIndex >= modList.Length)
			{
				GrowModificationList();
			}

			modList[nextIndex++] = new SetComponentEntityMod<T>(entity, component);
		}

		public void AddComponent<T>(Entity entity, T component) where T : unmanaged, IComponent
		{
			if (nextIndex >= modList.Length)
			{
				GrowModificationList();
			}

			modList[nextIndex++] = new AddComponentEntityMod<T>(entity, component);
		}

		public void AddSharedComponent<T>(Entity entity, T component) where T : class, ISharedComponent
		{
			if (nextIndex >= modList.Length)
			{
				GrowModificationList();
			}

			modList[nextIndex++] = new AddSharedComponentEntityMod<T>(entity, component);

		}

		public void RemoveComponent<T>(Entity entity) where T : unmanaged, IComponent
		{
			if (nextIndex >= modList.Length)
			{
				GrowModificationList();
			}

			modList[nextIndex++] = new RemoveComponentEntityMod<T>(entity);
		}

		public void RemoveSharedComponent<T>(Entity entity) where T : class, ISharedComponent
		{
			if (nextIndex >= modList.Length)
			{
				GrowModificationList();
			}

			modList[nextIndex++] = new RemoveSharedComponentEntityMod<T>(entity);
		}

		public void Playback()
		{
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			if (!IsEmpty())
			{
				Jobs.CompleteAllJobs();
				int i = 0;
				while (i < modList.Length)
				{
					IModification? mod = modList[i++];
					if (mod == null)
					{
						break;
					}

					try {
						mod.Execute(this);
					}
					catch(InvalidEntityException _){ }
					catch(ComponentNotFoundException _){ }
				}

				pool.Return(modList);
				modList = Array.Empty<IModification?>();
				entityTarget = default;
				hasEntityTarget = false;
				nextIndex = 0;
			}
		}

		public bool IsEmpty()
		{
			return nextIndex == 0;
		}
	}
}
