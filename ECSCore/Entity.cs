using System;
using System.Collections.Generic;
using System.Text;

namespace ECSCore {
	public struct Entity {
		public const int NULL_ID = 0;

		public int id;
		public int version;

		public bool IsNull() {
			return id == NULL_ID;
		}

		#region equalityComp

		public bool Equals (Entity other) {
			return id == other.id && version == other.version;
		}

		public override bool Equals (object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}

			return obj is Entity other && Equals(other);
		}

		public override int GetHashCode () {
			unchecked {
				return ((int)id * 397) ^ version;
			}
		}

		public static bool operator == (Entity left, Entity right) {
			return left.Equals(right);
		}

		public static bool operator != (Entity left, Entity right) {
			return !left.Equals(right);
		}

		#endregion
	}
}
