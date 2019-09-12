using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ECSCore;
using Xunit;

namespace ECSCoreTests {
	public class ComponentTests {


		const int loop_amount = 1_000;

		public struct TestEmptyComponent : IComponent { }

		public struct TestComponentWithInt : IComponent {
			public int someInt;
		}
		[Fact]
		public void Size() {
			unsafe {
				Assert.Equal(sizeof(TestEmptyComponent), 1);

				Assert.Equal(4, Marshal.SizeOf<TestComponentWithInt>());
				Assert.Equal(sizeof(TestComponentWithInt), Marshal.SizeOf<TestComponentWithInt>());

			}
		}

		[Fact]
		public void ComponentMemoryBlock() {
			EntityArchetype archetype = new EntityArchetype().Add<TestComponentWithInt>();
			using (var block = new ComponentMemoryBlock(archetype)) {
				Span<TestComponentWithInt> span = block.GetComponentData<TestComponentWithInt>();

				for (int i = 0; i < span.Length; i++) {
					span[i].someInt = i;
				}
				Span<TestComponentWithInt> span2 = block.GetComponentData<TestComponentWithInt>();
				for (int i = 0; i < span2.Length; i++) {
					Assert.Equal(i, span2[i].someInt);
				}
			}
		}


		[Fact]
		public void AddGetComponent() {
			ECSWorld world = new ECSWorld();

			for (int i = 0; i < loop_amount; i++) {
				Entity entity = world.entityManager.CreateEntity();
				world.componentManager.AddComponent(entity, new TestComponentWithInt { someInt = 10 });
				TestComponentWithInt test = world.componentManager.GetComponent<TestComponentWithInt>(entity);
				Assert.Equal(10, test.someInt);
				world.componentManager.GetComponent<TestComponentWithInt>(entity).someInt = 12;

				Assert.Equal(12, world.componentManager.GetComponent<TestComponentWithInt>(entity).someInt);
			}
			
		}

		[Fact]
		public void RemoveComponent () {
			ECSWorld world = new ECSWorld();

			Entity[] entities = new Entity[loop_amount];

			for (int i = 0; i < loop_amount; i++) {
				Entity entity = world.entityManager.CreateEntity();
				world.componentManager.AddComponent(entity, new TestComponentWithInt { someInt = 10 });
				Assert.True(world.componentManager.HasComponent<TestComponentWithInt>(entity));
				entities[i] = entity;
			}

			for (int i = 0; i < loop_amount; i++) {
				Assert.True(world.componentManager.HasComponent<TestComponentWithInt>(entities[i]));
				world.componentManager.RemoveComponent<TestComponentWithInt>(entities[i]);
				Assert.False(world.componentManager.HasComponent<TestComponentWithInt>(entities[i]));
#if DEBUG
				Assert.Throws<ComponentNotFoundException>(() => { world.componentManager.GetComponent<TestComponentWithInt>(entities[i]); });
#endif
}

		}
	}
}
