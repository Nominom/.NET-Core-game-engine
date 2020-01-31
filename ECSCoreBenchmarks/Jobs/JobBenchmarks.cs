using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;

namespace CoreBenchmarks.Jobs
{
	[Config(typeof(Config))]
	public class JobBenchmarks
	{
		public struct BenchJob : IJob {
			public float f1;
			public float f2;
			public float result;

			public void Execute() {
				result = f1 + f2;
			}
		}

		public class Config : ManualConfig
		{
			public Config()
			{
				var summaryStyle = new SummaryStyle(false, SizeUnit.KB, TimeUnit.Millisecond, true);

				this.SummaryStyle = summaryStyle;

				Add(MemoryDiagnoser.Default);
				//Add(
				//	HardwareCounter.BranchInstructions,
				//	//HardwareCounter.CacheMisses,
				//	HardwareCounter.BranchMispredictions
				//);
				Add(
					Job
						.Core
						.With(Runtime.Core)
						.WithWarmupCount(2)
						.WithIterationCount(30)
						.WithIterationTime(TimeInterval.FromMilliseconds(200))
						.WithLaunchCount(1)
						.With(new GcMode()
						{
							Force = false
						})
				);
			}
		}

		private const int numJobs = 10000;

		[GlobalSetup]
		public void Setup() {
			ChannelJobs.Setup();
			JobWorkersManager.Setup();
		}


		[Benchmark(Baseline = true)]
		public void SystemTasks()
		{
			Task[] tasks = new Task[numJobs];
			for (int i = 0; i < numJobs; i++) {
				BenchJob job = new BenchJob();
				job.f1 = i;
				job.f2 = i + 1;
				tasks[i] = Task.Run(() => job.Execute());
			}

			Task.WaitAll(tasks);
		}

		[Benchmark]
		public void ChannelSimpleJobs()
		{
			for (int i = 0; i < numJobs; i++) {
				BenchJob job = new BenchJob();
				job.f1 = i;
				job.f2 = i + 1;
				ChannelJobs.QueueJob(job);
			}

			ChannelJobs.CompleteAll();
		}

		[Benchmark]
		public void ConcurrentQueueWorkers() {
			var group = JobWorkersManager.StartJobGroup();
			for (int i = 0; i < numJobs; i++) {
				BenchJob job = new BenchJob();
				job.f1 = i;
				job.f2 = i + 1;
				JobWorkersManager.QueueJob(job, group);
			}

			JobWorkersManager.CompleteAll();
		}
	}
}
