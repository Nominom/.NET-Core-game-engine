using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using ECSCore;

namespace ECSCoreBenchmarks {
	[Config(typeof(Config))]
	public class PooledArrayBenchmarks {

		private class Config : ManualConfig {

			public Config() {
				Add(MemoryDiagnoser.Default);
				Add(
					Job
					.Default
					.WithWarmupCount(2)
					.WithIterationCount(25)
					.WithIterationTime(TimeInterval.FromMilliseconds(100))
					.With(new GcMode() {
						Force = false
					})
					.WithLaunchCount(1)
					);
			}
		}

		[Params(10, 100, 1_000, 10_000, 100_000, 1_000_000, 10_000_000)]
		public int ArrSize { get; set; }

		[Benchmark]
		public int ECS_PooledArray() {
			int l;
			using (PooledArray<int> array = PooledArray<int>.Rent(ArrSize)) {
				l = array.array.Length;
			}
			return l;
			
		}

		[Benchmark]
		public int Std_ArrayPool () {
			int[] array = ArrayPool<int>.Shared.Rent(ArrSize);
			int l = array.Length;
			ArrayPool<int>.Shared.Return(array);
			return l;
		}

		[Benchmark]
		public int NewArray () {
			int[] arr = new int[ArrSize];
			return arr.Length;
		}
	}
}
