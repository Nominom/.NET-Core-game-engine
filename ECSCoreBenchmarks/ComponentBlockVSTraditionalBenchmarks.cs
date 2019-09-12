using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using ECSCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECSCoreBenchmarks {
	[Config(typeof(Config))]
	public class ComponentBlockVSTraditionalBenchmarks {
		private class Config : ManualConfig {

			public Config () {
				var summaryStyle = new SummaryStyle ( false, SizeUnit.KB, TimeUnit.Millisecond, true);

				this.SummaryStyle = summaryStyle;

				Add(
					HardwareCounter.BranchInstructions,
					//HardwareCounter.CacheMisses,
					HardwareCounter.BranchMispredictions
				);
				Add(
					Job
						.Default
						.With(Runtime.Core)
						.WithWarmupCount(5)
						.WithIterationCount(10)
						.WithIterationTime(TimeInterval.FromMilliseconds(100))
						.WithLaunchCount(1)
				);
			}
		}

		private struct TestComponent1 : IComponent {
			public float x;
			public float y;
			public float z;
		}

		private class TestComponentClass {
			public float x;
			public float y;
			public float z;
		}

		private class TraditionalEntity {
			public TestComponentClass testComponent = new TestComponentClass();
		}


		private EntityArchetype archetype = EntityArchetype.Empty.Add<TestComponent1>();

		private List<ComponentMemoryBlock> entityBlocks;
		private List<TraditionalEntity> entities;
		private TestComponent1[] components;

		[Params(10, 1_000, 10_000, 1_000_000/*, 10_000_000*/)]
		public int numEntities;

		[GlobalSetup]
		public void Setup () {
			entityBlocks = new List<ComponentMemoryBlock>();
			entities = new List<TraditionalEntity>();
			components = new TestComponent1[numEntities];

			ComponentMemoryBlock currentBlock = new ComponentMemoryBlock(archetype);
			entityBlocks.Add(currentBlock);
			for (int i = 0; i < numEntities; i++) {
				var entity = new TraditionalEntity();
				entities.Add(entity);

				if (!currentBlock.HasRoom) {
					currentBlock = new ComponentMemoryBlock(archetype);
					entityBlocks.Add(currentBlock);
				}

				currentBlock.AddEntity(new Entity() {id = i + 1, version = 1});
			}
		}

		[GlobalCleanup]
		public void Cleanup() {
			foreach (var block in entityBlocks) {
				block.Dispose();
			}
			entities.Clear();
			entityBlocks.Clear();
		}


		[Benchmark(Baseline = true)]
		public float Traditional() {

			for (int i = 0; i < entities.Count; i++) {
				var data = entities[i].testComponent;
				data.x += i;
				data.y += i;
				data.z += i;
			}

			return entities[0].testComponent.x;
		}

		[Benchmark]
		public float ComponentBlocks () {
			for (int n = 0; n < entityBlocks.Count; n++) {
				Span<TestComponent1> d1 = entityBlocks[n].GetComponentData<TestComponent1>();

				for (int i = 0; i < d1.Length; i++) {
					d1[i].x += i;
					d1[i].y += i;
					d1[i].z += i;
				}
			}

			return entityBlocks[0].GetComponentData<TestComponent1>()[0].x;
		}

		[Benchmark]
		public float ComponentBlocksVectorized () {
			for (int n = 0; n < entityBlocks.Count; n++) {
				Span<byte> d1 = entityBlocks[n].GetRawComponentData<TestComponent1>();
				
				Span<Vector3> df = MemoryMarshal.Cast<byte, Vector3>(d1);
				Vector3 addition = new Vector3(n,n,n);

				for (int i = 0; i < df.Length; i++) {
					df[i] += addition;
				}
			}

			return entityBlocks[0].GetComponentData<TestComponent1>()[0].x;
		}

		[Benchmark]
		public float OnlyComponents () {
			Span<TestComponent1> d1 = components;

			for (int i = 0; i < d1.Length; i++) {
				d1[i].x += i;
				d1[i].y += i;
				d1[i].z += i;
			}

			return d1[0].x;
		}
	}
}
