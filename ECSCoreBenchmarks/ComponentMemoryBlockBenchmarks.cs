using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.Roslyn;
using ECSCore;

namespace ECSCoreBenchmarks {

	[Config(typeof(Config))]
	public class ComponentMemoryBlockBenchmarks {
		private class Config : ManualConfig {

			public Config () {
				Add(
					HardwareCounter.BranchInstructions,
					//HardwareCounter.CacheMisses,
					HardwareCounter.BranchMispredictions
				);
				Add(
					Job
						.Default
						.With(Runtime.Core)
						.WithWarmupCount(10)
						.WithIterationCount(25)
						.WithIterationTime(TimeInterval.FromMilliseconds(200))
						.WithLaunchCount(1)
				);
			}
		}

		private struct TestComponent1 : IComponent {
			public int testInt;
			public double testDouble;
		}

		private struct TestComponent2 : IComponent {
			public int testInt;
			public float testFloat;
		}

		private EntityArchetype archetype = EntityArchetype.Empty.Add<TestComponent1>().Add<TestComponent2>();
		private ComponentMemoryBlock[] testBlocks;
		private List<TestComponent1[]> comp1Arrays;
		private List<TestComponent2[]> comp2Arrays;

		[Params(1, 100, 1000)]
		public int blocks; 

		[GlobalSetup]
		public void Setup() {
			testBlocks = new ComponentMemoryBlock[blocks];

			comp1Arrays = new List<TestComponent1[]>();
			comp2Arrays = new List<TestComponent2[]>();

			for (int n = 0; n < blocks; n++) {
				testBlocks[n] = new ComponentMemoryBlock(archetype);

				comp1Arrays.Add(new TestComponent1[testBlocks[n].MaxSize]);
				comp2Arrays.Add(new TestComponent2[testBlocks[n].MaxSize]);

				var data1 = testBlocks[n].GetComponentData<TestComponent1>();
				var data2 = testBlocks[n].GetComponentData<TestComponent2>();
				for (int i = 0; i < testBlocks[n].MaxSize; i++) {
					testBlocks[n].AddEntity(new Entity() { id = i + 1, version = i + 1 });
					data1[i].testInt = i;
					data1[i].testDouble = i * 1.5;
					data2[i].testInt = i * 2;
					data2[i].testFloat = i * 1.33f;


					comp1Arrays[n][i].testInt = data1[i].testInt;
					comp1Arrays[n][i].testDouble = data1[i].testDouble;
					comp2Arrays[n][i].testInt = data2[i].testInt;
					comp2Arrays[n][i].testFloat = data2[i].testFloat;
				}
			}

			
		}

		[GlobalCleanup]
		public void Cleanup() {
			for (int i = 0; i < blocks; i++) {
				testBlocks[i].Dispose();
			}
			comp1Arrays.Clear();
			comp2Arrays.Clear();
		}

		[Benchmark]
		public int WriteToBlock() {
			Span<TestComponent1> d1 = Span<TestComponent1>.Empty;
			Span<TestComponent2> d2 = Span<TestComponent2>.Empty;

			for (int n = 0; n < blocks; n++) {
				d1 = testBlocks[n].GetComponentData<TestComponent1>();
				d2 = testBlocks[n].GetComponentData<TestComponent2>();

				for (int i = 0; i < d1.Length; i++) {
					d1[i].testInt = i;
					d1[i].testDouble = i * 1.5;
					d2[i].testInt = i * 2;
					d2[i].testFloat = i * 1.33f;
				}
			}

			return d1[0].testInt;
		}

		[Benchmark(Baseline = true)]
		public int WriteToArray () {
			for (int n = 0; n < blocks; n++) {
				for (int i = 0; i < comp1Arrays[n].Length; i++) {
					comp1Arrays[n][i].testInt = i;
					comp1Arrays[n][i].testDouble = i * 1.5;
					comp2Arrays[n][i].testInt = i * 2;
					comp2Arrays[n][i].testFloat = i * 1.33f;
				}
			}

			return comp1Arrays[0][0].testInt;
		}

	}
}
