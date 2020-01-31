using System.Threading;

namespace Core.ECS.JobSystem
{
	internal static class JobManager
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
			return new JobGroup(nextJobGroupId, lastJobGroupId);
		}

		public static JobHandle QueueJob<T>(T job, JobGroup group) where T : struct, IJob
		{
			Interlocked.Increment(ref runningJobs);
			Interlocked.Increment(ref groupWorkLeft[group.groupId]);

			var jobHandle = new JobHandle(nextJobId, group, JobExecutor<T>.Instance);

			JobExecutor<T>.Instance.AddJob(jobHandle, job);
			workers[nextWorker].jobIds.Enqueue(jobHandle);

			if (workers[nextWorker].waiting)
			{
				workers[nextWorker].waitForWork.Set();
			}

			++nextWorker;
			nextWorker %= workerAmount;

			++nextJobId;
			return jobHandle;
		}

		public static void CompleteAll()
		{
			while (runningJobs > 0)
			{
				TryStealWork();
			}
		}

		internal static bool CanExecuteGroup(JobGroup group) {
			
			return Interlocked.Read(ref groupWorkLeft[group.dependency]) == 0;
		}

		internal static bool IsGroupComplete(JobGroup group) {
			return Interlocked.Read(ref groupWorkLeft[group.groupId]) == 0;
		}

		internal static bool TryStealWork() {
			bool stealSuccess = false;
			for (int i = 0; i < workerAmount; i++)
			{
				JobWorker stealTarget = workers[i];
				if (stealTarget.jobIds.TryDequeue(out var jobHandle))
				{
					if (!CanExecuteGroup(jobHandle.group))
					{
						stealTarget.jobIds.Enqueue(jobHandle);
						Thread.Sleep(0);
					}
					else if (jobHandle.executor.ExecuteJob(jobHandle))
					{
						SignalComplete(jobHandle);
					}

					stealSuccess = true;
				}
			}
			return stealSuccess;
		}

		internal static void SignalComplete(JobHandle handle)
		{
			Interlocked.Decrement(ref runningJobs);
			Interlocked.Decrement(ref groupWorkLeft[handle.group.groupId]);
		}
	}
}
