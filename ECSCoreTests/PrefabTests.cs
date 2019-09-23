using System;
using System.Collections.Generic;
using System.Text;
using ECSCore;
using ECSCore.Numerics;
using Xunit;

namespace ECSCoreTests
{
	public class PrefabTests {
		private Prefab prefab1, prefab2;
		private SharedComponent1 shared;

		private const int numIterations = 100;

		public PrefabTests() {
			prefab1 = new Prefab();
			shared = new SharedComponent1() {array = new[] {1, 2, 3}};

			prefab1.AddComponent(new TestComponent1{i = 1, d = 2, f = 3});
			prefab1.AddComponent(new TestComponent2{i = 3, d = 4, f = 5});
			prefab1.AddSharedComponent(shared);

			prefab2 = new Prefab();
			prefab2.AddComponent(new TestComponentVector3(){value = Vector3.one});
		}
		[Fact]
		public void CreateWithComponent() {
			ECSWorld world = new ECSWorld();

			Entity instantiated = world.Instantiate(prefab2);

			Assert.True(world.ComponentManager.HasComponent<TestComponentVector3>(instantiated));

			TestComponentVector3 component = world.ComponentManager.GetComponent<TestComponentVector3>(instantiated);
			
			Assert.Equal(component.value, Vector3.one);
		}

		[Fact]
		public void CreateMultiple() {
			ECSWorld world = new ECSWorld();

			Entity[] entities = new Entity[numIterations];

			for (int i = 0; i < numIterations; i++) {
				entities[i] = world.Instantiate(prefab1);
			}

			for (int i = 0; i < numIterations; i++) {
				Assert.True(world.ComponentManager.HasComponent<TestComponent1>(entities[i]));

				var component = world.ComponentManager.GetComponent<TestComponent1>(entities[i]);
				var component2 = world.ComponentManager.GetComponent<TestComponent2>(entities[i]);

				Assert.Same(shared, world.ComponentManager.GetSharedComponent<SharedComponent1>(entities[i]));

				Assert.Equal(1, component.i);
				Assert.Equal(2, component.d, 3);
				Assert.Equal(3, component.f, 3);

				Assert.Equal(3, component2.i);
				Assert.Equal(4, component2.d, 3);
				Assert.Equal(5, component2.f, 3);
			}
		}
	}
}
