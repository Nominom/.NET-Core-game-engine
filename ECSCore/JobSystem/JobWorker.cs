using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Core.ECS.JobSystem
{
	public class JobWorker
	{
		public Thread thread;
		public ConcurrentQueue<JobHandle> jobIds = new ConcurrentQueue<JobHandle>();
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
					do {
						while (jobIds.TryDequeue(out var jobHandle)) {
							if (!JobManager.CanExecuteGroup(jobHandle.group))
							{
								jobIds.Enqueue(jobHandle);
								Thread.Sleep(0);
							}
							else if (jobHandle.executor.ExecuteJob(jobHandle)) {
								JobManager.SignalComplete(jobHandle);
							}
							if (jobIds.IsEmpty) {
								Thread.Sleep(0);
							}
						}
					} while (JobManager.TryStealWork());

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
