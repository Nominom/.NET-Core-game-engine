using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Core.ECS
{
	public class EntityManager {
		private int nextIdx;
		private readonly Stack<Entity> freeEntities;
		private readonly ComponentManager componentManager;

		public EntityManager(ComponentManager cm) {
			componentManager = cm;
			freeEntities = new Stack<Entity>();
			nextIdx = 1;
		}

		public Entity CreateEntity(EntityArchetype archetype = null) {
			if (archetype == null) {
				archetype = EntityArchetype.Empty;
			}
			if (freeEntities.TryPop(out Entity result)) {
				result.version++;
				componentManager.AddEntity(result, archetype);
				return result;
			}
			else {
				Entity e = new Entity{id = NextIndex(), version = 0};
				componentManager.AddEntity(e, archetype);
				return e;
			}
		}

		public Entity[] CreateEntities(int numEntities, EntityArchetype archetype = null) {
			if (archetype == null) {
				archetype = EntityArchetype.Empty;
			}
			Entity[] arr = new Entity[numEntities];

			for (int i = 0; i < arr.Length; i++) {
				if (freeEntities.TryPop(out Entity result)) {
					result.version++;
					componentManager.AddEntity(result, archetype);
					arr[i] = result;
				} else {
					Entity e = new Entity { id = NextIndex(), version = 0 };
					componentManager.AddEntity(e, archetype);
					arr[i] = e;
				}
			}

			return arr;
		}

		public void DestroyEntity(in Entity e) {
			if (!componentManager.IsEntityValid(e)) {
				throw new InvalidEntityException();
			}
			componentManager.RemoveEntity(e);
			freeEntities.Push(e);
		}

		private int NextIndex() {
			int idx = nextIdx;
			++nextIdx;
			return idx;
		}
	}
}
