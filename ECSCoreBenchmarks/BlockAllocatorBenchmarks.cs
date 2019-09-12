using System;
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
	public class BlockAllocatorBenchmarks {
		private class Config : ManualConfig {

			public Config () {
				Add(MemoryDiagnoser.Default);
				Add(
					Job
						.Default
						.WithWarmupCount(10)
						.WithIterationCount(100)
						.WithIterationTime(TimeInterval.FromMilliseconds(200))
						.With(new GcMode() {
							Force = false
						})
						.WithLaunchCount(1)
				);
			}
		}


		private BlockAllocator allocator = ECSCore.BlockAllocator.KB32;
		private int bytes = 1024 * 32;

		[Benchmark]
		public int NormalArray() {
			byte b = 0;

			byte[] arr = new byte[bytes];
			for (int i = 0; i < arr.Length; i++) {
				arr[i] = b++;
				b %= 255;
			}

			for (int i = 0; i < arr.Length; i++) {
				b ^= arr[i];
			}

			return b;
		}

		[Benchmark]
		public int NormalArraySpan () {
			byte b = 0;

			Span<byte> span = new Span<byte>(new byte[bytes]);
			for (int i = 0; i < span.Length; i++) {
				span[i] = b++;
				b %= 255;
			}

			for (int i = 0; i < span.Length; i++) {
				b ^= span[i];
			}

			return b;
		}

		[Benchmark]
		public int BlockAllocator () {
			byte b = 0;

			using (var block = allocator.Rent()) {
				var span = block.Memory.Span;

				for (int i = 0; i < span.Length; i++) {
					span[i] = b++;
					b %= 255;
				}

				for (int i = 0; i < span.Length; i++) {
					b ^= span[i];
				}
			}

			return b;
		}
	}
}
