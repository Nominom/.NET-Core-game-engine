using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ECSCore {
	public class EntityArchetype {

		public static readonly EntityArchetype Empty = new EntityArchetype();
		//Component Type and size in bytes
		internal Dictionary<System.Type, int> components = new Dictionary<System.Type, int>();
		//SharedComponent Type and index into array
		internal Dictionary<System.Type, ISharedComponentHandle> sharedComponents = new Dictionary<Type, ISharedComponentHandle>();
		private int _hash = 0;

		public int Hash => _hash;

		public EntityArchetype Add<T>() where T : unmanaged, IComponent {
			EntityArchetype archetype = this.Clone() as EntityArchetype;
			archetype.components.Add(typeof(T), Marshal.SizeOf<T>());
			archetype.CalculateHash();
			return archetype;
		}

		public EntityArchetype Remove<T> () where T : unmanaged, IComponent {
			EntityArchetype archetype = this.Clone() as EntityArchetype;
			archetype.components.Remove(typeof(T));
			archetype.CalculateHash();
			return archetype;
		}

		public EntityArchetype AddShared<T> (in SharedComponentHandle<T> handle) where T : ISharedComponent {
			EntityArchetype archetype = this.Clone() as EntityArchetype;
			archetype.sharedComponents.Add(typeof(T), handle);
			archetype.CalculateHash();
			return archetype;
		}

		public EntityArchetype RemoveShared<T> () where T : ISharedComponent {
			EntityArchetype archetype = this.Clone() as EntityArchetype;
			archetype.components.Remove(typeof(T));
			archetype.CalculateHash();
			return archetype;
		}

		public SharedComponentHandle<T> GetShared<T>() where T : ISharedComponent {
			ISharedComponentHandle handle = sharedComponents[typeof(T)];
			return handle is SharedComponentHandle<T> shared ? shared : throw new NullReferenceException();
		}

		public EntityArchetype Clone() {
			EntityArchetype archetype = new EntityArchetype();
			archetype.components = new Dictionary<System.Type, int>(components);
			archetype.sharedComponents = new Dictionary<Type, ISharedComponentHandle>(sharedComponents);
			return archetype;
		}

		private void CalculateHash() {
			_hash = 0;
			foreach (var kp in components) {
				_hash ^= kp.Key.GetHashCode();
			}

			foreach (var kp in sharedComponents) {
				_hash ^= kp.Key.GetHashCode();
				_hash ^= kp.Value.GetHashCode();
			}
		}

		public bool Has<T>() where T : unmanaged, IComponent {
			return components.ContainsKey(typeof(T));
		}

		public bool HasShared<T> () where T : ISharedComponent {
			return sharedComponents.ContainsKey(typeof(T));
		}
	}
}
