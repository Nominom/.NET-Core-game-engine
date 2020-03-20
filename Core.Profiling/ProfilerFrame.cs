using System;
using System.Collections.Generic;
using System.Text;
using Core.Shared;

namespace Core.Profiling
{
	public struct ProfilerFrame : IDisposable {
		public long frameNumber;
		public double frameLength;
		public PooledList<ValueTuple<string, PooledList<ProfilerMethod>>> threadMethods;
		public PooledList<ProfilerMarker> markers;

		public void Dispose() {
			if (threadMethods != null) {
				foreach (var tuple in threadMethods) {
					tuple.Item2?.Dispose();
				}
				threadMethods.Dispose();
			}
			markers?.Dispose();
		}
	}
}
