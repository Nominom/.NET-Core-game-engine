using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Core.ECS
{
	public class DebugHelper {

		[Conditional("DEBUG")]
		public static void AssertThrow<T>(bool condition) where T : Exception, new() {
			if (!condition) {
				throw new T();
			}
		}

		[Conditional("DEBUG")]
		public static void AssertThrow (bool condition, Exception ex) {
			if (!condition) {
				throw ex;
			}
		}
	}
}
