using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Jobs
{
	public struct JobHandle : IEquatable<JobHandle> {
		public bool Equals(JobHandle other) {
			return jobId == other.jobId;
		}

		public override bool Equals(object obj) {
			return obj is JobHandle other && Equals(other);
		}

		public static bool operator ==(JobHandle left, JobHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(JobHandle left, JobHandle right) {
			return !left.Equals(right);
		}

		public readonly long jobId;

		internal JobHandle(long jobId) {
			this.jobId = jobId;
		}

		public bool IsCompleted() {
			return JobManager.IsJobComplete(this);
		}

		public void Complete() {
			while (!IsCompleted()) {
				JobManager.Work(false);
			}
		}

		public override int GetHashCode() {
			return (int) (jobId ^ (jobId << 32));
		}
	}

	internal struct JobHandlePair : IEquatable<JobHandlePair> {
		public bool Equals(JobHandlePair other) {
			return handle.Equals(other.handle);
		}

		public override bool Equals(object obj) {
			return obj is JobHandlePair other && Equals(other);
		}

		public override int GetHashCode() {
			return handle.GetHashCode();
		}

		public static bool operator ==(JobHandlePair left, JobHandlePair right) {
			return left.Equals(right);
		}

		public static bool operator !=(JobHandlePair left, JobHandlePair right) {
			return !left.Equals(right);
		}

		public readonly JobHandle handle;
		public readonly IJobExecutor executor;

		public JobHandlePair(JobHandle handle, IJobExecutor executor) {
			this.handle = handle;
			this.executor = executor;
		}
	}
}
