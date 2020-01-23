using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Jobs
{
	public static class JobExtensions
	{
		/// <summary>
		/// Schedule a short job with no dependencies
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="job"></param>
		/// <returns></returns>
		public static JobHandle Schedule<T>(this T job) where T : struct, IShortJob {
			return JobManager.QueueShortJob(job);
		}

		/// <summary>
		/// Schedule a short job with a dependency
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="job"></param>
		/// <param name="dependency"></param>
		/// <returns></returns>
		public static JobHandle Schedule<T>(this T job, JobHandle dependency) where T : struct, IShortJob {
			throw new NotImplementedException();
		}
	}
}
