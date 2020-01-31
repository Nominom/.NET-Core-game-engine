namespace Core.ECS.JobSystem
{
	public static class JobExtensions
	{
		/// <summary>
		/// Schedule a short job with a specified group
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="job"></param>
		/// <param name="group"></param>
		/// <returns></returns>
		public static JobHandle Schedule<T>(this T job, JobGroup group) where T : struct, IJob {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			return JobManager.QueueJob(job, group);
		}
	}
}
