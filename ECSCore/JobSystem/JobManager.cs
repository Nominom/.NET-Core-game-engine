using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Threading;
using Core.Profiling;
using GlmSharp;

namespace Core.ECS.JobSystem
{

	internal static class JobManager
	{
		private static long nextJobId = 0;
		private static int nextWorker = 0;
		private static int runningJobs = 0;

		public static int workerAmount { get; private set; }
		public static JobWorker[] workers;

		internal static ConcurrentQueue<JobHandle>[] jobQueues = new ConcurrentQueue<JobHandle>[numGroups];

		private const long numGroups = 256;
		private static long nextJobGroupId = 0;
		private static readonly int[] groupWorkLeft = new int[numGroups];
		private static readonly int[] groupWorkLeftToGrab = new int[numGroups];
		private static readonly ComponentQuery[] groupDependencies = new ComponentQuery[numGroups];
		private static readonly JobGroup[] jobGroups = new JobGroup[numGroups];

		private static bool isSetup = false;

		private static AutoResetEvent waitForWorkEvent = new AutoResetEvent(false);
		private static int waitingThreads = 0; 

		public static void Setup()
		{
			if (isSetup) return;
			for (int i = 0; i < jobQueues.Length; i++) {
				jobQueues[i] = new ConcurrentQueue<JobHandle>();
			}

			workerAmount = Math.Max(1, Environment.ProcessorCount - 1);
			workers = new JobWorker[workerAmount];

			for (int i = 0; i < workerAmount; i++)
			{
				workers[i] = new JobWorker();
				Thread thread = new Thread(workers[i].Run);
				workers[i].thread = thread;
				thread.IsBackground = true;
				thread.Name = "JobWorker" + i.ToString();
				thread.Priority = ThreadPriority.Normal;
				thread.Start();
				Profiler.RegisterThread(thread, thread.Name);
			}

			isSetup = true;
		}

		public static JobGroup StartJobGroup()
		{
			++nextJobGroupId;
			nextJobGroupId %= numGroups;

			if(groupWorkLeft[nextJobGroupId] > 0) CompleteAll();

			groupDependencies[nextJobGroupId] = ComponentQuery.Empty;
			var result = new JobGroup(nextJobGroupId, -1);
			jobGroups[nextJobGroupId] = result;
			groupWorkLeftToGrab[nextJobGroupId] = 0;
			return result;
		}

		public static JobGroup StartJobGroup(ComponentQuery dependencies)
		{
			++nextJobGroupId;
			nextJobGroupId %= numGroups;

			if(groupWorkLeft[nextJobGroupId] > 0) CompleteAll();

			long dependency = -1;
			if (runningJobs > 0) {

				long jobGroupToCheck = nextJobGroupId - 1;
				if (jobGroupToCheck < 0) {
					jobGroupToCheck = numGroups - 1;
				}
				while (jobGroupToCheck != nextJobGroupId) {
					
					if (groupWorkLeft[jobGroupToCheck] > 0 && groupDependencies[jobGroupToCheck].CollidesWith(dependencies)) {
						dependency = jobGroupToCheck;
						break;
					}
					jobGroupToCheck--;
					if (jobGroupToCheck < 0) {
						jobGroupToCheck = numGroups - 1;
					}
				}
			}

			groupDependencies[nextJobGroupId] = dependencies;
			var result = new JobGroup(nextJobGroupId, dependency);
			jobGroups[nextJobGroupId] = result;
			groupWorkLeftToGrab[nextJobGroupId] = 0;
			return result;
		}

		public static JobGroup StartJobGroup(JobGroup dependency)
		{
			++nextJobGroupId;
			nextJobGroupId %= numGroups;

			if(groupWorkLeft[nextJobGroupId] > 0) CompleteAll();

			groupDependencies[nextJobGroupId] = ComponentQuery.Empty;
			var result = new JobGroup(nextJobGroupId, dependency.groupId);
			jobGroups[nextJobGroupId] = result;
			groupWorkLeftToGrab[nextJobGroupId] = 0;
			return result;
		}

		public static JobHandle QueueJob<T>(T job, JobGroup group) where T : struct, IJob
		{
			Interlocked.Increment(ref runningJobs);
			Interlocked.Increment(ref groupWorkLeft[group.groupId]);
			Interlocked.Increment(ref groupWorkLeftToGrab[group.groupId]);

			var jobHandle = new JobHandle(nextJobId, group, JobExecutor<T>.Instance);

			JobExecutor<T>.Instance.AddJob(jobHandle, job);
			jobQueues[group.groupId].Enqueue(jobHandle);

			//if (workers[nextWorker].waiting) {
			//	workers[nextWorker].waitForWork.Set();
			//}

			if (waitingThreads > 0) {
				waitForWorkEvent.Set();
				Interlocked.Decrement(ref waitingThreads);
			}

			//++nextWorker;
			//nextWorker %= workerAmount;

			++nextJobId;
			return jobHandle;
		}

		public static void CompleteAll()
		{
			while (runningJobs > 0)
			{
				TryWork();
			}
		}

		internal static bool CanExecuteGroup(JobGroup group) {
			if (group.dependency == -1) return true;
			return groupWorkLeft[group.dependency] == 0;
		}

		internal static bool IsGroupComplete(JobGroup group) {
			return groupWorkLeft[group.groupId] == 0;
		}

		internal static bool HasWorkToDo() {
			return runningJobs > 0;
		}

		internal static bool TryWork() {
			bool success = false;
			for (int i = 0; i < numGroups; i++) {
				if (groupWorkLeftToGrab[i] > 0) {
					if (!CanExecuteGroup(jobGroups[i])) {
						continue;
					}
					while (jobQueues[i].TryDequeue(out var jobHandle)) {
						Interlocked.Decrement(ref groupWorkLeftToGrab[i]);
						if (jobHandle.executor.ExecuteJob(jobHandle))
						{
							SignalComplete(jobHandle);
							success =  true;
						}
						else {
							Console.WriteLine("Job couldn't be completed.");
							Interlocked.Increment(ref groupWorkLeftToGrab[i]);
						}
					}
				}
			}
			return success;
		}

		internal static void SignalComplete(JobHandle handle)
		{
			Interlocked.Decrement(ref runningJobs);
			Interlocked.Decrement(ref groupWorkLeft[handle.group.groupId]);
		}

		internal static void WaitForWorkSignal() {
			Interlocked.Increment(ref waitingThreads);
			waitForWorkEvent.WaitOne();
		}
	}
}
