using System;
using System.Collections.Concurrent;
using System.Threading;
using Core.Profiling;

namespace Core.ECS.JobSystem
{
	public class JobWorker
	{
		public Thread thread;
		public ManualResetEventSlim waitForWork = new ManualResetEventSlim(false);
		public volatile bool waiting = false;

		public void Run()
		{
			while (true)
			{
				try {
					//waiting = true;
					//waitForWork.Wait();
					//waiting = false;
					if(!JobManager.HasWorkToDo())
						JobManager.WaitForWorkSignal();

					while (JobManager.HasWorkToDo()) {
						if (!JobManager.TryWork()) {
							Thread.Sleep(0);
						}
					}

					for (int i = 0; i < 200; i++) {
						if (JobManager.HasWorkToDo()) break;
						Thread.Sleep(0);
					}
					//waitForWork.Reset();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}
	}
}
