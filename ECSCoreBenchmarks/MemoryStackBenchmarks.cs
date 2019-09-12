using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;

namespace ECSCoreBenchmarks
{
	[Config(typeof(Config))]
	public class MemoryStackBenchmarks
	{
		private class Config : ManualConfig
		{

			public Config()
			{
				var summaryStyle = new SummaryStyle(false, SizeUnit.KB, TimeUnit.Nanosecond, true);

				this.SummaryStyle = summaryStyle;

				Add(MemoryDiagnoser.Default);
				Add(
					Job
						.Default
						.With(Runtime.Core)
						.WithWarmupCount(5)
						.WithIterationCount(10)
						.WithIterationTime(TimeInterval.FromMilliseconds(100))
						.WithLaunchCount(1)
						.With(new GcMode()
						{
							Force = false
						})
				);
			}
		}

		[Params(10, 100, 1000, 10_000, 100_000)]
		public int numElements;

		[Benchmark(Baseline = true)]
		public void Array() {
			float[] array = new float[numElements];

			array[0] = 1;
		}

		[Benchmark]
		public void ArrayPool() {
			var pool = ArrayPool<float>.Shared;
			float[] array = pool.Rent(numElements);

			array[0] = 1;
			pool.Return(array);
		}

		[Benchmark]
		public void MemoryStack() {
			var stack = ECSCore.MemoryStack.Default;
			stack.Reset();

			Span<float> floats = stack.Get<float>(numElements);

			floats[0] = 1;
		}

		[Benchmark]
		public void MemoryStackAligned()
		{
			var stack = ECSCore.MemoryStack.Default;
			stack.Reset();

			Span<float> floats = stack.GetAligned<float>(numElements);

			floats[0] = 1;
		}
	}
}
