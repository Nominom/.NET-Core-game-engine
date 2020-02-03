using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Core.ECS.Collections;

namespace Core.ECS
{
	public struct ComponentQuery : IEquatable<ComponentQuery> {
		internal BitSet256 includeMask;
		internal BitSet256 excludeMask;
		internal BitSet256 includeWriteMask;

		internal BitSet256 sharedIncludeMask;
		internal BitSet256 sharedExcludeMask;

		#if DEBUG
		internal List<System.Type> includeTypes;
		internal List<System.Type> includeWriteTypes;
		internal List<System.Type> excludeTypes;
		internal List<System.Type> sharedIncludeTypes;
		internal List<System.Type> sharedExcludeTypes;
		private void AddToDebugList(ref List<System.Type> list, System.Type type) {
			if (list == null) {
				list = new List<Type>();
			}
			list.Add(type);
		}
		#endif

		public static ComponentQuery Empty { get; } = new ComponentQuery();

		

		public void IncludeReadWrite<T>() where T : IComponent
		{
			includeMask.Set(TypeHelper.Component<T>.componentIndex);
			includeWriteMask.Set(TypeHelper.Component<T>.componentIndex);
#if DEBUG
			AddToDebugList(ref includeTypes, typeof(T));
			AddToDebugList(ref includeWriteTypes, typeof(T));
#endif
		}

		public void IncludeReadonly<T>() where T : IComponent {
			includeMask.Set(TypeHelper.Component<T>.componentIndex);
#if DEBUG
			AddToDebugList(ref includeTypes, typeof(T));
#endif
		}

		public void Exclude<T>() where T : IComponent
		{
			excludeMask.Set(TypeHelper.Component<T>.componentIndex);
#if DEBUG
			AddToDebugList(ref excludeTypes, typeof(T));
#endif
		}

		public void IncludeShared<T>() where T : ISharedComponent
		{
			sharedIncludeMask.Set(TypeHelper.SharedComponent<T>.componentIndex);
#if DEBUG
			AddToDebugList(ref sharedIncludeTypes, typeof(T));
#endif
		}

		public void ExcludeShared<T>() where T : ISharedComponent
		{
			sharedExcludeMask.Set(TypeHelper.SharedComponent<T>.componentIndex);
#if DEBUG
			AddToDebugList(ref sharedExcludeTypes, typeof(T));
#endif
		}

		public readonly bool Includes<T>() where T : IComponent {
			return includeMask.Get(TypeHelper.Component<T>.componentIndex);
		}

		public readonly bool IncludesWrite<T>() where T : IComponent {
			return includeWriteMask.Get(TypeHelper.Component<T>.componentIndex);
		}

		public readonly bool IncludesShared<T>() where T : ISharedComponent {
			return sharedIncludeMask.Get(TypeHelper.SharedComponent<T>.componentIndex);
		}

		public readonly bool Includes(System.Type type) {
			int index = ComponentMask.GetComponentIndex(type);
			return includeMask.Get(index);
		}

		public readonly bool IncludesWrite(System.Type type) {
			int index = ComponentMask.GetComponentIndex(type);
			return includeWriteMask.Get(index);
		}

		public readonly bool IncludesShared(System.Type type) {
			int index = SharedComponentMask.GetSharedComponentIndex(type);
			return sharedIncludeMask.Get(index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public readonly bool Matches(EntityArchetype archetype) {
			if (!archetype.componentMask.ContainsAll(includeMask)) return false;
			if (!archetype.sharedComponentMask.ContainsAll(sharedIncludeMask)) return false;
			if (archetype.componentMask.ContainsAny(excludeMask)) return false;
			if (archetype.sharedComponentMask.ContainsAny(sharedExcludeMask)) return false;

			return true;
		}

		public readonly bool CollidesWith(ComponentQuery other) {
			if (other.includeMask.ContainsAny(includeWriteMask)) return true;
			if (other.includeWriteMask.ContainsAny(includeMask)) return true;
			if (other.sharedIncludeMask.ContainsAny(sharedIncludeMask)) return true;
			return false;
		}

		public readonly bool Equals(ComponentQuery other) {
			return includeMask.Equals(other.includeMask) && includeWriteMask.Equals(other.includeWriteMask) && excludeMask.Equals(other.excludeMask) && sharedIncludeMask.Equals(other.sharedIncludeMask) && sharedExcludeMask.Equals(other.sharedExcludeMask);
		}

		public override readonly bool Equals(object obj) {
			return obj is ComponentQuery other && Equals(other);
		}

		public override readonly int GetHashCode() {
			unchecked {
				int hashCode = includeMask.GetHashCode();
				hashCode = (hashCode * 397) ^ includeWriteMask.GetHashCode();
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

		internal BitSet256 GetIncludeMask() {
			return includeMask;
		}

		internal BitSet256 GetIncludeWriteMask() {
			return includeWriteMask;
		}

		internal BitSet256 GetSharedIncludeMask() {
			return sharedIncludeMask;
		}
		
	}
}
