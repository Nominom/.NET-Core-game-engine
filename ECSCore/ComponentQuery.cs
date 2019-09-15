using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ECSCore.Collections;

namespace ECSCore {
	public struct ComponentQuery {
		private BitSet256 includeMask;
		private BitSet256 excludeMask;

		private BitSet256 sharedIncludeMask;
		private BitSet256 sharedExcludeMask;

		public void Include<T>() where T : IComponent
		{
			includeMask.Set(ComponentMask.GetComponentIndex<T>());
		}

		public void Exclude<T>() where T : IComponent
		{
			excludeMask.Set(ComponentMask.GetComponentIndex<T>());
		}

		public void IncludeShared<T>() where T : ISharedComponent
		{
			sharedIncludeMask.Set(SharedComponentMask.GetSharedComponentIndex<T>());
		}

		public void ExcludeShared<T>() where T : ISharedComponent
		{
			sharedExcludeMask.Set(SharedComponentMask.GetSharedComponentIndex<T>());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public bool Matches(EntityArchetype archetype) {
			if (!archetype.componentMask.ContainsAll(includeMask)) return false;
			if (!archetype.sharedComponentMask.ContainsAll(sharedIncludeMask)) return false;
			if (archetype.componentMask.ContainsAny(excludeMask)) return false;
			if (archetype.sharedComponentMask.ContainsAny(sharedExcludeMask)) return false;

			return true;
		}
	}
}
