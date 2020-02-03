using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Core.ECS.Collections;

namespace Core.ECS
{
	public class EntityArchetype {

		public static readonly EntityArchetype Empty = new EntityArchetype();
		//Component Type and size in bytes
		internal Dictionary<System.Type, int> components = new Dictionary<System.Type, int>();
		//SharedComponent Type and reference to component
		internal Dictionary<System.Type, ISharedComponent> sharedComponents = new Dictionary<Type, ISharedComponent>();
		private int _hash = 0;

		public int Hash => _hash;
		internal BitSet256 componentMask;
		internal BitSet256 sharedComponentMask;

		public EntityArchetype Add<T>() where T : unmanaged, IComponent {
			EntityArchetype archetype = this.Clone() as EntityArchetype;
			archetype.components.Add(typeof(T), Marshal.SizeOf<T>());
			archetype.CalculateHashAndMask();
			return archetype;
		}

		public EntityArchetype Remove<T> () where T : unmanaged, IComponent {
			EntityArchetype archetype = this.Clone() as EntityArchetype;
			archetype.components.Remove(typeof(T));
			archetype.CalculateHashAndMask();
			return archetype;
		}

		public EntityArchetype AddShared<T>(T component) where T : class, ISharedComponent {
			EntityArchetype archetype = this.Clone() as EntityArchetype;
			archetype.sharedComponents[typeof(T)] = component;
			archetype.CalculateHashAndMask();
			return archetype;
		}

		public EntityArchetype RemoveShared<T> () where T : class, ISharedComponent {
			EntityArchetype archetype = this.Clone() as EntityArchetype;
			archetype.sharedComponents.Remove(typeof(T));
			archetype.CalculateHashAndMask();
			return archetype;
		}

		public T GetShared<T>() where T : class, ISharedComponent {
			if (!sharedComponents.TryGetValue(typeof(T), out ISharedComponent component)) {
				throw new ComponentNotFoundException();
			}
			return component as T;
		}

		private EntityArchetype Clone() {
			EntityArchetype archetype = new EntityArchetype();
			archetype.components = new Dictionary<System.Type, int>(components);
			archetype.sharedComponents = new Dictionary<Type, ISharedComponent>(sharedComponents);
			archetype.CalculateHashAndMask();
			return archetype;
		}

		private void CalculateHashAndMask() {
			componentMask = new BitSet256();
			sharedComponentMask = new BitSet256();
			_hash = 0;
			foreach (var kp in components) {
				_hash ^= kp.Key.GetHashCode();
				componentMask.Set(ComponentMask.GetComponentIndex(kp.Key));
			}

			foreach (var kp in sharedComponents) {
				_hash ^= kp.Key.GetHashCode();
				_hash ^= kp.Value.GetHashCode();
				sharedComponentMask.Set(SharedComponentMask.GetSharedComponentIndex(kp.Key));
			}
		}

		public bool Has<T>() where T : unmanaged, IComponent {
			return componentMask.Get(TypeHelper.Component<T>.componentIndex);
			//return components.ContainsKey(typeof(T));
		}

		public bool HasShared<T> () where T : class, ISharedComponent {
			return sharedComponentMask.Get(TypeHelper.SharedComponent<T>.componentIndex);
			//return sharedComponents.ContainsKey(typeof(T));
		}
	}
}
