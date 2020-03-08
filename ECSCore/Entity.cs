using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.ECS
{
	public readonly struct Entity {
		public const int NULL_ID = 0;

		public readonly int id;
		public readonly int version;

		public Entity(int id, int version) {
			this.id = id;
			this.version = version;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool IsNull() {
			return id == NULL_ID;
		}

		#region equalityComparison

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool Equals (Entity other) {
			return id == other.id && version == other.version;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override readonly bool Equals (object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}

			return obj is Entity other && Equals(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override readonly int GetHashCode () {
			unchecked {
				return ((int)id * 397) ^ version;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator == (Entity left, Entity right) {
			return left.Equals(right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (Entity left, Entity right) {
			return !left.Equals(right);
		}

		#endregion

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override readonly string ToString() {
#if _DEBUG
			//find name from debug names
#endif
			return $"entity({id.ToString()}, {version.ToString()})";
		}
	}
}
