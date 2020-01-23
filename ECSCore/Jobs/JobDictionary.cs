using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Jobs
{

	internal static class JobDictionary<T> where T : struct, IJob
	{
		private static readonly Dictionary<JobHandle, T> dictionary = new Dictionary<JobHandle, T>();


		internal static void AddJob(T job, JobHandle handle)
		{
			lock (dictionary) {
				dictionary.Add(handle, job);
			}
		}

		internal static bool GetJob(JobHandle handle, out T result)
		{
			lock (dictionary) {
				return dictionary.Remove(handle, out result);
			}
		}
	}
}
