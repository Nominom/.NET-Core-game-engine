namespace Core.ECS.JobSystem
{
	public static class Jobs
	{

		public static void Setup() {
			JobManager.Setup();
		}
		public static JobGroup StartNewGroup() {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			return JobManager.StartJobGroup();
		}

		public static JobGroup StartNewGroup(ComponentQuery dependencies) {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			return JobManager.StartJobGroup(dependencies);
		}

		public static JobGroup StartNewGroup(JobGroup dependency) {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			return JobManager.StartJobGroup(dependency);
		}

		public static JobHandle QueueJob<T>(T job, JobGroup group) where T : struct, IJob {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			return JobManager.QueueJob(job, group);
		}

		public static void CompleteAllJobs() {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			JobManager.CompleteAll();
		}
	}
}
