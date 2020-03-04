using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Core.ECS.Events;

namespace Core.ECS
{
	public class EntityManager {
		private int nextIdx;
		private readonly Stack<Entity> freeEntities;
		private readonly ComponentManager componentManager;
		private ECSWorld world;

		public EntityManager(ECSWorld world, ComponentManager cm) {
			this.world = world;
			componentManager = cm;
			freeEntities = new Stack<Entity>();
			nextIdx = 1;
		}

		public Entity CreateEntity(EntityArchetype archetype = null) {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			world.SyncPoint();
			if (archetype == null) {
				archetype = EntityArchetype.Empty;
			}
			if (freeEntities.TryPop(out Entity result)) {
				result.version++;
				componentManager.AddEntity(result, archetype);
				new EntityCreatedEvent(result).Fire(world);
				return result;
			}
			else {
				Entity e = new Entity{id = NextIndex(), version = 0};
				componentManager.AddEntity(e, archetype);
				new EntityCreatedEvent(e).Fire(world);
				return e;
			}
		}

		public Entity[] CreateEntities(int numEntities, EntityArchetype archetype = null) {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			world.SyncPoint();
			if (archetype == null) {
				archetype = EntityArchetype.Empty;
			}
			Entity[] arr = new Entity[numEntities];

			for (int i = 0; i < arr.Length; i++) {
				if (freeEntities.TryPop(out Entity result)) {
					result.version++;
					componentManager.AddEntity(result, archetype);
					arr[i] = result;
					new EntityCreatedEvent(result).Fire(world);
				} else {
					Entity e = new Entity { id = NextIndex(), version = 0 };
					componentManager.AddEntity(e, archetype);
					arr[i] = e;
					new EntityCreatedEvent(e).Fire(world);
				}
			}

			return arr;
		}

		public void DestroyEntity(in Entity e) {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			world.SyncPoint();
			if (!componentManager.IsEntityValid(e)) {
				throw new InvalidEntityException();
			}
			componentManager.RemoveEntity(e);
			freeEntities.Push(e);
			
			new EntityDestroyedEvent(e).Fire(world);
		}

		private int NextIndex() {
			int idx = nextIdx;
			++nextIdx;
			return idx;
		}
	}
}
