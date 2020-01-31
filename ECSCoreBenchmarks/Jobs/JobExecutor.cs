using System;
using System.Collections.Generic;
using System.Text;

namespace CoreBenchmarks.Jobs
{

	public struct JobHandle : IEquatable<JobHandle> {
		public readonly long jobId;
		public readonly JobGroup group;
		internal readonly IJobExecutor executor;

		internal JobHandle(long jobId, JobGroup group, IJobExecutor executor) {
			this.jobId = jobId;
			this.group = group;
			this.executor = executor;
		}

		public bool Equals(JobHandle other) {
			return jobId == other.jobId && group.Equals(other.group);
		}

		public override bool Equals(object obj) {
			return obj is JobHandle other && Equals(other);
		}

		public override int GetHashCode() {
			unchecked {
				int hashCode = jobId.GetHashCode();
				hashCode = (hashCode * 397) ^ group.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(JobHandle left, JobHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(JobHandle left, JobHandle right) {
			return !left.Equals(right);
		}
	}

	public struct JobGroup : IEquatable<JobGroup> {
		public readonly long groupId;
		public readonly long dependency;

		public JobGroup(long groupId, long dependency) {
			this.groupId = groupId;
			this.dependency = dependency;
		}

		public bool Equals(JobGroup other) {
			return groupId == other.groupId;
		}

		public override bool Equals(object obj) {
			return obj is JobGroup other && Equals(other);
		}

		public override int GetHashCode() {
			return groupId.GetHashCode();
		}

		public static bool operator ==(JobGroup left, JobGroup right) {
			return left.Equals(right);
		}

		public static bool operator !=(JobGroup left, JobGroup right) {
			return !left.Equals(right);
		}
	}

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
