using System;
using System.Collections.Concurrent;
using System.Threading;

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
					waiting = true;
					waitForWork.Wait();
					waiting = false;
					while (JobManager.jobQueue.TryDequeue(out var jobHandle)) {
						if (!JobManager.CanExecuteGroup(jobHandle.group))
						{
							JobManager.jobQueue.Enqueue(jobHandle);
						}
						else if (jobHandle.executor.ExecuteJob(jobHandle))
						{
							JobManager.SignalComplete(jobHandle);
						}
					}
					waitForWork.Reset();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}
	}
}
