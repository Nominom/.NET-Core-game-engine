using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ECSCore {
	internal class TypeHelper<T> {
		public static readonly System.Type type = typeof(T);
		public static readonly string typeName = typeof(T).FullName;
		public static readonly int hashCode = typeof(T).GetHashCode();
	}
}
