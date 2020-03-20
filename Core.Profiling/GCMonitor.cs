using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Profiling
{
	public class GCMonitor {
		internal static WeakReference<GCMonitor> CurrentInstance = Create();
		private static int[] genxgcCount = new int[GC.MaxGeneration];

		private static WeakReference<GCMonitor> Create() {
			GCMonitor monitor = new GCMonitor();
			return new WeakReference<GCMonitor>(monitor);
		}

		~GCMonitor() {
			int gen = 0;
			for (int i = 0; i < GC.MaxGeneration; i++) {
				int collectCount = GC.CollectionCount(i);
				if (collectCount > genxgcCount[i]) {
					gen = i;
					genxgcCount[i] = collectCount;
				}
			}
			Profiler.RegisterGC(gen);
			CurrentInstance.SetTarget(new GCMonitor());
		}
	}
}
