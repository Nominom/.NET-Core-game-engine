using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Core.ECS;

namespace CoreBenchmarks {

	struct TestComponent : IComponent {
		public int x;
		public int y;
		public int z;
	}

	[HardwareCounters(HardwareCounter.BranchMispredictions)]
	public class ComponentBenchmarks {
		private const int numEntities = 100_000;
		private ComponentManager cm;
		private Entity[] entities;

		public ComponentBenchmarks() {
			ECSWorld world = new ECSWorld(false);
			cm = world.ComponentManager;
			entities = new Entity[numEntities];

			for (int i = 0; i < numEntities; i++) {
				Entity e = new Entity { id = i + 1, version = 0 };
				entities[i] = e;
				cm.AddEntity(e, EntityArchetype.Empty);
				cm.AddComponent(e, new TestComponent());
			}
		}

		[Benchmark]
		public void GetComponent() {
			for (int i = 0; i < entities.Length; i++) {
				cm.GetComponent<TestComponent>(entities[i]).x = i;
			}
		}
	}
}
