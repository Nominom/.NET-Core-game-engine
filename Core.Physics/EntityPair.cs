using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;

namespace Core.Physics
{
	internal struct EntityPair : IEquatable<EntityPair> {
		public Entity A;
		public Entity B;
		public EntityPair(Entity a, Entity b) {
			A = a;
			B = b;
		}

		public bool Equals(EntityPair other) {
			return A.Equals(other.A) && B.Equals(other.B);
		}

		public override bool Equals(object obj) {
			return obj is EntityPair other && Equals(other);
		}

		public override int GetHashCode() {
			unchecked {
				return A.GetHashCode() ^ B.GetHashCode();
			}
		}

		public static bool operator ==(EntityPair left, EntityPair right) {
			return left.Equals(right);
		}

		public static bool operator !=(EntityPair left, EntityPair right) {
			return !left.Equals(right);
		}
	}
}
