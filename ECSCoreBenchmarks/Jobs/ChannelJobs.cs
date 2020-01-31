using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CoreBenchmarks.Jobs
{
	public static class ChannelJobs {
		public static Channel<IJob> jobChannel = Channel.CreateUnbounded<IJob>();
		public static ChannelWriter<IJob> writer = jobChannel.Writer;
		public static ChannelReader<IJob> reader = jobChannel.Reader;
		private static bool isSetup = false;
		private static int runningJobs = 0;

		public static void Setup() {
			if (isSetup) return;

			for (int i = 0; i < 7; i++) {
				_ = Work();
			}

			isSetup = true;
		}

		public static void QueueJob(IJob job) {
			Interlocked.Increment(ref runningJobs);
			writer.TryWrite(job);
		}

		public static void CompleteAll()
		{
			while(runningJobs > 0)
			{
				while (reader.TryRead(out IJob item)) {
					item.Execute();
					Interlocked.Decrement(ref runningJobs);
				}
			}
		}

		public static async Task Work()
		{
			while(await reader.WaitToReadAsync())
			{
				while (reader.TryRead(out IJob item)) {
					item.Execute();
					Interlocked.Decrement(ref runningJobs);
				}
			}
		}
	}
}
