using System;

namespace Core.ECS.JobSystem
{
	public readonly struct JobHandle : IEquatable<JobHandle> {
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

	public readonly struct JobGroup : IEquatable<JobGroup> {
		public readonly long groupId;
		public readonly long dependency;

		public JobGroup(long groupId, long dependency) {
			this.groupId = groupId;
			this.dependency = dependency;
		}

		public bool IsComplete() {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			return JobManager.IsGroupComplete(this);
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
}
