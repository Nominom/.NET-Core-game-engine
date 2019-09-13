using System;
using System.Collections.Generic;
using System.Text;

namespace ECSCore {

	public struct BlockAccessor {
		private readonly ComponentMemoryBlock block;
#if DEBUG
	
#else
	public ReadOnlySpan<Entity> GetEntityData () => block.GetEntityData();
	public Span<T> GetComponentData<T> () where T : unmanaged, IComponent => block.GetComponentData<T>();
	public T GetSharedComponentData<T> () where T : ISharedComponent => throw new NotImplementedException();
#endif
	}

	public abstract class ComponentSystemBase : ISystem {
		public bool Enabled { get; set; }
		public virtual void OnCreateSystem () { }
		public virtual void OnDestroySystem () { }
		public virtual void OnEnableSystem () { }
		public virtual void OnDisableSystem () { }
		public abstract void Update(float deltaTime, ECSWorld world);
	}
	public abstract class ComponentSystem : ComponentSystemBase {
		public abstract ComponentQuery GetQuery();
		public abstract void ProcessBlock(BlockAccessor accessor);
	}
}
