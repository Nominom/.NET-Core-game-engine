using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using Core.ECS;
using Core.ECS.JobSystem;

namespace CoreBenchmarks
{
	[Config(typeof(Config))]
	public class EntityAccessBenchmarks
	{

		private class Config : ManualConfig {

			public Config() {
				var summaryStyle = new SummaryStyle(false, SizeUnit.KB, TimeUnit.Millisecond, true);

				this.SummaryStyle = summaryStyle;
				Add(MemoryDiagnoser.Default);
				Add(
					Job
						.Default
						.WithWarmupCount(2)
						.WithIterationCount(25)
						.WithIterationTime(TimeInterval.FromMilliseconds(200))
						.With(new GcMode() {
							Force = false
						})
						.WithLaunchCount(1)
				);
			}
		}

		[Params(10, 100, 10_000, 1_000_000)]
		public int numEntities { get; set; }
		private ECSWorld world;
		private Entity[] entities;

		private ComponentQuery query;

		[GlobalSetup]
		public void Setup() {
			world = new ECSWorld();

			var archetype = EntityArchetype.Empty.Add<TestComponent>();
			entities = world.EntityManager.CreateEntities(numEntities, archetype);
			foreach (Entity entity in entities) {
				entity.SetComponent(world, new TestComponent(){x = 1, y = 2, z = 3});
			}

			ComponentQuery query = new ComponentQuery();
			query.IncludeReadWrite<TestComponent>();

			Core.ECS.JobSystem.Jobs.Setup();
		}

		[GlobalCleanup]
		public void Cleanup() {
			world.CleanUp();
		}

		[Benchmark]
		public void EntitySetAccess() {
			foreach (var entity in entities) {
				var component = entity.GetComponent<TestComponent>(world);
				component.x = component.y * component.z;
				entity.SetComponent(world, component);
			}
		}

		[Benchmark]
		public void EntityRefAccess() {
			foreach (var entity in entities) {
				ref var component = ref entity.GetComponent<TestComponent>(world);
				component.x = component.y * component.z;
			}
		}

		[Benchmark(Baseline = true)]
		public void QueryAccess()
		{
			foreach (BlockAccessor block in world.ComponentManager.GetBlocks(query)) {
				var components = block.GetComponentData<TestComponent>();
				for (int i = 0; i < block.length; i++) {
					components[i].x = components[i].y * components[i].z;
				}
			}
		}

		struct BlockJob : IJob {
			public BlockAccessor block;
			public void Execute() {
				var components = block.GetComponentData<TestComponent>();
				for (int i = 0; i < block.length; i++) {
					components[i].x = components[i].y * components[i].z;
				}
			}
		}

		[Benchmark]
		public void QueryJobAccess() {
			JobGroup group = Core.ECS.JobSystem.Jobs.StartNewGroup(query);
			foreach (BlockAccessor block in world.ComponentManager.GetBlocks(query)) {
				new BlockJob() {block = block}.Schedule(group);
			}
			Core.ECS.JobSystem.Jobs.CompleteAllJobs();
		}
	}
}
