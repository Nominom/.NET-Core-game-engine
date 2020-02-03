using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS.Filters;

namespace Core.ECS
{
	public struct BlockAccessor
	{
		private readonly ComponentMemoryBlock block;
		private readonly ComponentQuery query;
		private bool preIncremented;

		public readonly int length;
		internal BlockAccessor(ComponentMemoryBlock block, ComponentQuery query)
		{
			this.block = block;
			this.query = query;
			this.preIncremented = false;
			length = block.Size;
		}

		public long Id => block.id;

		public ReadOnlySpan<Entity> GetEntityData() {
			return block.GetEntityData().Slice(0, block.Size);
		}
		public Span<T> GetComponentData<T> () where T : unmanaged, IComponent {
			DebugHelper.AssertThrow<ComponentNotFoundException>(block.archetype.Has<T>());
			DebugHelper.AssertThrow<IllegalAccessException>(query.IncludesWrite<T>());
			if (preIncremented)
			{
				return block.GetComponentDataNoVersionIncrement<T>().Slice(0, block.Size);
			}
			else {
				return block.GetComponentData<T>().Slice(0, block.Size);
			}
		}
		public ReadOnlySpan<T> GetReadOnlyComponentData<T>() where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ComponentNotFoundException>(block.archetype.Has<T>());
			DebugHelper.AssertThrow<IllegalAccessException>(query.Includes<T>());
			return block.GetReadOnlyComponentData<T>().Slice(0, block.Size);
		}
		public long GetComponentVersion<T>() where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ComponentNotFoundException>(block.archetype.Has<T>());
			return block.GetComponentVersion<T>();
		}

		public long GetEntityVersion() {
			return block.EntityVersion;
		}

		public T GetSharedComponentData<T> () where T : class, ISharedComponent {
			DebugHelper.AssertThrow<IllegalAccessException>(query.IncludesShared<T>());
			return block.archetype.GetShared<T>();
		}

		internal void CommitVersion()
		{
			block.IncrementVersionsByQuery(query);
			preIncremented = true;
		}
	}

	public struct UnsafeBlockAccessor
	{
		private readonly UnsafeComponentMemoryBlock block;

		public readonly int length;
		internal UnsafeBlockAccessor(UnsafeComponentMemoryBlock block)
		{
			this.block = block;

			length = block.Size;
		}

		public ReadOnlySpan<Entity> GetEntityData()
		{
			return block.GetEntityData().Slice(0, block.Size);
		}
		public Span<T> GetComponentData<T>() where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ComponentNotFoundException>(block.archetype.Has<T>());
			return block.GetComponentData<T>().Slice(0, block.Size);
		}
		public ReadOnlySpan<T> GetReadOnlyComponentData<T>() where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ComponentNotFoundException>(block.archetype.Has<T>());
			return block.GetComponentData<T>().Slice(0, block.Size);
		}

		public T GetSharedComponentData<T>() where T : class, ISharedComponent
		{
			return block.archetype.GetShared<T>();
		}
	}
}
