using System;
using System.Collections.Generic;
using System.Text;

namespace ECSCore {

	public interface ISharedComponentHandle {
	}

	public struct SharedComponentHandle<T> : ISharedComponentHandle where T : ISharedComponent {
		public readonly int index;

		internal SharedComponentHandle(int index) {
			this.index = index;
		}

		public ref T Get() {
			throw new NotImplementedException();
		}

		public override int GetHashCode() {
			return TypeHelper<T>.hashCode ^ index;
		}

		public override bool Equals(object obj) {
			if (obj is SharedComponentHandle<T> other) {
				return other.index == index;
			}
			else {
				return false;
			}
		}
	}
}
