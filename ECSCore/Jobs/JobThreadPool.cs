using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Core.ECS.Jobs
{
	internal static class JobThreadPool {
		public static Thread[] fastWorkers;

		public static void Setup() {
			if (fastWorkers != null) {
				return;
			}

			int processors = Environment.ProcessorCount;
			if (processors < 2) processors = 2;
			fastWorkers = new Thread[processors - 1];
			for (int i = 0; i < fastWorkers.Length; i++) {
				Thread t = new Thread(DoFastWork) {
					IsBackground = true, Priority = ThreadPriority.Normal, Name = "JobWorker" + i.ToString()
				};
				t.Start();
				fastWorkers[i] = t;
			}
		}

		public static void DoFastWork() {
			while (true) {
				try {
					JobManager.Work(true);
				}
				catch (Exception ex) {
					Console.WriteLine(ex);
				}
			}
		}
	}
}
