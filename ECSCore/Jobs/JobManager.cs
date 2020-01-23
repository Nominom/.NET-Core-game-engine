using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Core.ECS.Jobs
{

	internal static class JobManager {
		private static long nextJobId = 0;
		// JobHandles currently executing
		private static readonly HashSet<JobHandle> runningJobs = new HashSet<JobHandle>();
		// JobHandles queued
		private static readonly BlockingCollection<JobHandlePair> queuedJobs = new BlockingCollection<JobHandlePair>();
		private static readonly ConcurrentQueue<JobHandle> finishedJobs = new ConcurrentQueue<JobHandle>();

		private static void FlushFinished() {
			while (finishedJobs.TryDequeue(out JobHandle result)) {
				runningJobs.Remove(result);
			}
		}
		
		internal static JobHandle QueueShortJob<T>(T job) where T : struct, IShortJob {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			JobHandle handle;
			long jobId = Interlocked.Increment(ref nextJobId);
			handle = new JobHandle(jobId);
			JobDictionary<T>.AddJob(job, handle);
			runningJobs.Add(handle);
			queuedJobs.Add(new JobHandlePair(handle, JobExecutor<T>.Instance));
			return handle;
		}

		internal static bool IsJobComplete(JobHandle handle) {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			FlushFinished();
			return !runningJobs.Contains(handle);
		}

		internal static void CompleteAllWork() {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			while (runningJobs.Count > 0) {
				FlushFinished();
				Work(false);
			}
		}

		internal static bool Work(bool waitForWork) {
			JobHandlePair job;
			if (waitForWork) {
				job = queuedJobs.Take();
			}
			else {
				bool success = queuedJobs.TryTake(out job);
				if (!success) {
					return false;
				}
			}
			
			IJobExecutor executor = job.executor;

			try {
				executor.ExecuteWork(job.handle);
			}
			catch (Exception ex) {
				Console.WriteLine(ex);
				//TODO: Log
			}

			finishedJobs.Enqueue(job.handle);
			return true;
		}
	}

	public interface IJobExecutor {
		void ExecuteWork(JobHandle job);
	}

	internal class JobExecutor<T> : IJobExecutor where T : struct, IJob
	{
		public static JobExecutor<T> Instance { get; } = new JobExecutor<T>();

		public void ExecuteWork(JobHandle handle) {
			if (JobDictionary<T>.GetJob(handle, out var job)) {
				job.DoJob();
			}
			else {
				Console.WriteLine("A job was missing from the dictionary");
			}
		}
	}
}
