﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS
{
	public struct BlockAccessor
	{
		private readonly ComponentMemoryBlock block;

		public readonly int Length { get; }
		internal BlockAccessor(ComponentMemoryBlock block)
		{
			this.block = block;

			Length = block.Size;
		}

		public ReadOnlySpan<Entity> GetEntityData() {
			return block.GetEntityData().Slice(0, block.Size);
		}
		public Span<T> GetComponentData<T> () where T : unmanaged, IComponent {
			DebugHelper.AssertThrow<ComponentNotFoundException>(block.archetype.Has<T>());
			return block.GetComponentData<T>().Slice(0, block.Size);
		}
		public ReadOnlySpan<T> GetReadOnlyComponentData<T>() where T : unmanaged, IComponent
		{
			DebugHelper.AssertThrow<ComponentNotFoundException>(block.archetype.Has<T>());
			return block.GetComponentData<T>().Slice(0, block.Size);
		}

		public T GetSharedComponentData<T> () where T : class, ISharedComponent {
			return block.archetype.GetShared<T>();
		}
	}
}