using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Core.ECS.Collections;

namespace Core.ECS
{
	public struct ComponentQuery : IEquatable<ComponentQuery> {
		private BitSet256 includeMask;
		private BitSet256 excludeMask;

		private BitSet256 sharedIncludeMask;
		private BitSet256 sharedExcludeMask;

		public static ComponentQuery Empty { get; } = new ComponentQuery();

		public void Include<T>() where T : IComponent
		{
			includeMask.Set(TypeHelper.Component<T>.componentIndex);
		}

		public void Exclude<T>() where T : IComponent
		{
			excludeMask.Set(TypeHelper.Component<T>.componentIndex);
		}

		public void IncludeShared<T>() where T : ISharedComponent
		{
			sharedIncludeMask.Set(TypeHelper.SharedComponent<T>.componentIndex);
		}

		public void ExcludeShared<T>() where T : ISharedComponent
		{
			sharedExcludeMask.Set(TypeHelper.SharedComponent<T>.componentIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public bool Matches(EntityArchetype archetype) {
			if (!archetype.componentMask.ContainsAll(includeMask)) return false;
			if (!archetype.sharedComponentMask.ContainsAll(sharedIncludeMask)) return false;
			if (archetype.componentMask.ContainsAny(excludeMask)) return false;
			if (archetype.sharedComponentMask.ContainsAny(sharedExcludeMask)) return false;

			return true;
		}

		public bool CollidesWith(ComponentQuery other) {
			if (other.includeMask.ContainsAny(includeMask)) return true;
			if (other.sharedIncludeMask.ContainsAny(sharedIncludeMask)) return true;
			return false;
		}

		public bool Equals(ComponentQuery other) {
			return includeMask.Equals(other.includeMask) && excludeMask.Equals(other.excludeMask) && sharedIncludeMask.Equals(other.sharedIncludeMask) && sharedExcludeMask.Equals(other.sharedExcludeMask);
		}

		public override bool Equals(object obj) {
			return obj is ComponentQuery other && Equals(other);
		}

		public override int GetHashCode() {
			unchecked {
				int hashCode = includeMask.GetHashCode();
				hashCode = (hashCode * 397) ^ excludeMask.GetHashCode();
				hashCode = (hashCode * 397) ^ sharedIncludeMask.GetHashCode();
				hashCode = (hashCode * 397) ^ sharedExcludeMask.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(ComponentQuery left, ComponentQuery right) {
			return left.Equals(right);
		}

		public static bool operator !=(ComponentQuery left, ComponentQuery right) {
			return !left.Equals(right);
		}


	}
}
