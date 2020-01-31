using System;
using System.Collections.Generic;

namespace Core.ECS.JobSystem
{

	internal interface IJobExecutor {
		bool ExecuteJob(JobHandle jobId);
	}

	internal class JobExecutor<T> : IJobExecutor where T : struct, IJob
	{
		public static JobExecutor<T> Instance { get; } = new JobExecutor<T>();
		public Dictionary<JobHandle, T> jobs = new Dictionary<JobHandle, T>();

		public void AddJob(JobHandle jobId, T job) {
			lock (jobs) {
				jobs.Add(jobId, job);
			}
		}

		public bool ExecuteJob(JobHandle jobId) {
			try {
				T job;
				lock (jobs) {
					if (!jobs.Remove(jobId, out job)) {
						return false;
					}
				}
				job.Execute();
			}
			catch (Exception ex) {
				Console.WriteLine(ex);
			}
			
			return true;
		}
	}
}
