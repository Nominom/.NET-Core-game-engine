using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Profiling
{
	public readonly struct ProfilerMethod {
		public readonly string methodName;
		public readonly int depth;
		public readonly double startTime;
		public readonly double endTime;
		public readonly int threadId;

		public ProfilerMethod(string methodName, int depth, double startTime, double endTime, int threadId) {
			this.methodName = methodName;
			this.depth = depth;
			this.startTime = startTime;
			this.endTime = endTime;
			this.threadId = threadId;
		}
	}

	public readonly struct ProfilerMarker {
		public readonly string markName;
		public readonly double time;

		public ProfilerMarker(string markName, double time) {
			this.markName = markName;
			this.time = time;
		}
	}
}
