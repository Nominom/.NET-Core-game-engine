using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CoreBenchmarks.Jobs
{
	public static class JobWorkersManager
	{
		private static long nextJobId = 0;
		private static int nextWorker = 0;
		private static int runningJobs = 0;

		public const int workerAmount = 7;
		public static JobWorker[] workers = new JobWorker[workerAmount];


		private const long numGroups = 2048;
		private static long lastJobGroupId = 0;
		private static long nextJobGroupId = 0;
		private static readonly long[] groupWorkLeft = new long[numGroups];

		private static bool isSetup = false;

		public static void Setup()
		{
			if (isSetup) return;
			for (int i = 0; i < workerAmount; i++)
			{
				workers[i] = new JobWorker();
				Thread thread = new Thread(workers[i].Run);
				workers[i].thread = thread;
				thread.IsBackground = true;
				thread.Name = "JobWorker" + i.ToString();
				thread.Priority = ThreadPriority.Normal;
				thread.Start();
			}

			isSetup = true;
		}

		public static JobGroup StartJobGroup()
		{
			lastJobGroupId = nextJobGroupId;
			++nextJobGroupId;
			nextJobGroupId %= numGroups;
			long nextId = nextJobGroupId;
			return new JobGroup(nextId, lastJobGroupId);
		}

		public static void QueueJob<T>(T job, JobGroup group) where T : struct, IJob
		{
			Interlocked.Increment(ref runningJobs);
			Interlocked.Increment(ref groupWorkLeft[group.groupId]);

			var jobHandle = new JobHandle(nextJobId, group, JobExecutor<T>.Instance);

			JobExecutor<T>.Instance.AddJob(jobHandle, job);
			workers[nextWorker].jobIds.Enqueue(jobHandle);

			if (!workers[nextWorker].waitForWork.IsSet)
			{
				workers[nextWorker].waitForWork.Set();
			}

			++nextWorker;
			nextWorker %= workerAmount;

			++nextJobId;
		}

		public static void CompleteAll()
		{
			while (runningJobs > 0)
			{
				TryStealWork();
			}
		}

		internal static bool CanExecuteGroup(JobGroup group)
		{
			return groupWorkLeft[group.dependency] == 0;
		}

		internal static bool TryStealWork()
		{
			for (int i = 0; i < workerAmount; i++)
			{
				JobWorker stealTarget = workers[i];
				if (stealTarget.jobIds.TryDequeue(out var jobHandle))
				{
					while (!CanExecuteGroup(jobHandle.group))
					{
						Thread.Sleep(0);
					}
					if (jobHandle.executor.ExecuteJob(jobHandle))
					{
						SignalComplete(jobHandle);
					}
					return true;
				}
			}
			return false;
		}

		internal static void SignalComplete(JobHandle handle)
		{
			Interlocked.Decrement(ref runningJobs);
			Interlocked.Decrement(ref groupWorkLeft[handle.group.groupId]);
		}
	}

	public class JobWorker
	{
		public Thread thread;
		public ConcurrentQueue<JobHandle> jobIds = new ConcurrentQueue<JobHandle>();
		public ManualResetEventSlim waitForWork = new ManualResetEventSlim(false);

		public void Run()
		{
			while (true)
			{
				try
				{
					waitForWork.Wait();
					while (jobIds.TryDequeue(out var jobHandle))
					{
						while (!JobWorkersManager.CanExecuteGroup(jobHandle.group))
						{
							Thread.Sleep(0);
						}

						if (jobHandle.executor.ExecuteJob(jobHandle))
						{
							JobWorkersManager.SignalComplete(jobHandle);
						}
					}

					while (JobWorkersManager.TryStealWork()) { }

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
