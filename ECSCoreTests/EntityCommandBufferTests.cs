using System;
using System.Collections.Generic;
using System.Text;
using ECSCore;
using ECSCore.Numerics;
using Xunit;

namespace ECSCoreTests
{
	public class EntityCommandBufferTests
	{

		[Fact]
		public void CreateEntityFromArchetype() {
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			EntityArchetype archetype = EntityArchetype.Empty;
			archetype = archetype.Add<TestComponent1>();
			archetype = archetype.AddShared(shared1);


			buffer.CreateEntity(archetype);
			buffer.Playback();

			Assert.Collection(world.ComponentManager.GetBlocks(archetype), accessor => {
					Assert.Equal(1, accessor.GetEntityData().Length);
				});
		}


		[Fact]
		public void CreateEntityFromPrefab()
		{
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			Prefab prefab = new Prefab();
			prefab.AddComponent(new TestComponent1{i = 1, d = 2, f = 3});
			prefab.AddSharedComponent(shared1);


			buffer.CreateEntity(prefab);
			buffer.Playback();

			Assert.Collection(world.ComponentManager.GetBlocks(prefab.Archetype), accessor => {
				Assert.Equal(1, accessor.GetEntityData().Length);
				var cData = accessor.GetComponentData<TestComponent1>();
				Assert.Equal(1, cData[0].i);
				Assert.Equal(2, cData[0].d, 3);
				Assert.Equal(3, cData[0].f, 3);

				var shared = accessor.GetSharedComponentData<SharedComponent1>();
				Assert.Same(shared1, shared);
			});
		}


		[Fact]
		public void CreateEntitySetComponent()
		{
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			Prefab prefab = new Prefab();
			prefab.AddComponent(new TestComponentVector3() { value = Vector3.up});
			prefab.AddSharedComponent(shared1);


			buffer.CreateEntity(prefab);
			buffer.CreateEntity(prefab);
			buffer.SetComponent(new TestComponentVector3 { value = Vector3.right });
			buffer.CreateEntity(prefab.Archetype);
			buffer.SetComponent(new TestComponentVector3{value = Vector3.left});
			buffer.Playback();

			Assert.Collection(world.ComponentManager.GetBlocks(prefab.Archetype), accessor => {
				Assert.Equal(3, accessor.GetEntityData().Length);
				var cData = accessor.GetComponentData<TestComponentVector3>();
				Assert.Equal(Vector3.up, cData[0].value);
				Assert.Equal(Vector3.right, cData[1].value);
				Assert.Equal(Vector3.left, cData[2].value);

				var shared = accessor.GetSharedComponentData<SharedComponent1>();
				Assert.Same(shared1, shared);
			});
		}

		[Fact]
		public void CreateEntityAddComponent()
		{
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			Prefab prefab = new Prefab();
			prefab.AddComponent(new TestComponentVector3() { value = Vector3.up });
			prefab.AddSharedComponent(shared1);


			buffer.CreateEntity(prefab);
			buffer.CreateEntity(prefab);
			buffer.AddComponent(new TestComponent2() { i = 1, d = 2, f = 3});
			buffer.CreateEntity(prefab.Archetype);
			buffer.AddComponent(new TestComponent1() { i = 1, d = 2, f = 3 });
			buffer.Playback();

			Assert.Collection(world.ComponentManager.GetBlocks(prefab.Archetype), accessor => {
				Assert.Equal(1, accessor.GetEntityData().Length);
				var cData = accessor.GetComponentData<TestComponentVector3>();
				Assert.Equal(Vector3.up, cData[0].value);

				var shared = accessor.GetSharedComponentData<SharedComponent1>();
				Assert.Same(shared1, shared);
			});

			Assert.Collection(world.ComponentManager.GetBlocks(prefab.Archetype.Add<TestComponent2>()), accessor => {
				Assert.Equal(1, accessor.GetEntityData().Length);
				var cData = accessor.GetComponentData<TestComponentVector3>();
				Assert.Equal(Vector3.up, cData[0].value);
				var cData2 = accessor.GetComponentData<TestComponent2>();
				Assert.Equal(1, cData2[0].i);
				Assert.Equal(2, cData2[0].d, 3);
				Assert.Equal(3, cData2[0].f, 3);

				var shared = accessor.GetSharedComponentData<SharedComponent1>();
				Assert.Same(shared1, shared);
			});

			Assert.Collection(world.ComponentManager.GetBlocks(prefab.Archetype.Add<TestComponent1>()), accessor => {
				Assert.Equal(1, accessor.GetEntityData().Length);
				var cData = accessor.GetComponentData<TestComponentVector3>();
				Assert.Equal(Vector3.zero, cData[0].value);
				var cData2 = accessor.GetComponentData<TestComponent1>();
				Assert.Equal(1, cData2[0].i);
				Assert.Equal(2, cData2[0].d,3);
				Assert.Equal(3, cData2[0].f,3);

				var shared = accessor.GetSharedComponentData<SharedComponent1>();
				Assert.Same(shared1, shared);
			});
		}

		[Fact]
		public void CreateEntityAddShared()
		{
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			Prefab prefab = new Prefab();
			prefab.AddComponent(new TestComponentVector3() { value = Vector3.up });


			buffer.CreateEntity(prefab);
			buffer.CreateEntity(prefab);
			buffer.AddSharedComponent(shared1);
			buffer.Playback();

			Assert.Collection(world.ComponentManager.GetBlocks(prefab.Archetype), accessor => {
				Assert.Equal(1, accessor.GetEntityData().Length);
				var cData = accessor.GetComponentData<TestComponentVector3>();
				Assert.Equal(Vector3.up, cData[0].value);

				Assert.Throws<ComponentNotFoundException>(accessor.GetSharedComponentData<SharedComponent1>);
			});

			Assert.Collection(world.ComponentManager.GetBlocks(prefab.Archetype.AddShared(shared1)), accessor => {
				Assert.Equal(1, accessor.GetEntityData().Length);
				var cData = accessor.GetComponentData<TestComponentVector3>();
				Assert.Equal(Vector3.up, cData[0].value);

				var shared = accessor.GetSharedComponentData<SharedComponent1>();
				Assert.Same(shared1, shared);
			});
		}




		[Fact]
		public void PlaybackTwice()
		{
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			Prefab prefab = new Prefab();
			prefab.AddComponent(new TestComponentVector3() { value = Vector3.up });
			prefab.AddSharedComponent(shared1);


			buffer.CreateEntity(prefab);
			buffer.CreateEntity(prefab);
			buffer.CreateEntity(prefab.Archetype);

			buffer.Playback();
			buffer.Playback();


			Assert.Collection(world.ComponentManager.GetBlocks(prefab.Archetype), accessor => {
				Assert.Equal(3, accessor.GetEntityData().Length);
				var cData = accessor.GetComponentData<TestComponentVector3>();
				Assert.Equal(Vector3.up, cData[0].value);
				Assert.Equal(Vector3.up, cData[1].value);
				Assert.Equal(Vector3.zero, cData[2].value);

				var shared = accessor.GetSharedComponentData<SharedComponent1>();
				Assert.Same(shared1, shared);
			});
		}

		[Fact]
		public void AddComponentToEntity()
		{
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			EntityArchetype archetype = EntityArchetype.Empty;
			archetype = archetype.Add<TestComponent1>();
			archetype = archetype.AddShared(shared1);

			Entity target = world.Instantiate(archetype);

			buffer.AddComponent(target, new TestComponent2{ i = 2, d = 3, f = 4 });
			buffer.Playback();

			Assert.Collection(world.ComponentManager.GetBlocks(archetype.Add<TestComponent2>()), accessor => {
				Assert.Equal(1, accessor.GetEntityData().Length);

				var cData = accessor.GetComponentData<TestComponent2>();
				Assert.Equal(2, cData[0].i);
				Assert.Equal(3, cData[0].d, 3);
				Assert.Equal(4, cData[0].f, 3);
			});
		}

		[Fact]
		public void SetComponentToEntity()
		{
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			EntityArchetype archetype = EntityArchetype.Empty;
			archetype = archetype.Add<TestComponent1>();
			archetype = archetype.AddShared(shared1);

			Entity target = world.Instantiate(archetype);

			buffer.SetComponent(target, new TestComponent1 { i = 2, d = 3, f = 4});
			buffer.Playback();

			Assert.Collection(world.ComponentManager.GetBlocks(archetype), accessor => {
				Assert.Equal(1, accessor.GetEntityData().Length);

				var cData = accessor.GetComponentData<TestComponent1>();
				Assert.Equal(2, cData[0].i);
				Assert.Equal(3, cData[0].d, 3);
				Assert.Equal(4, cData[0].f, 3);
			});
		}

		[Fact]
		public void AddSharedComponentToEntity()
		{
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			EntityArchetype archetype = EntityArchetype.Empty;
			archetype = archetype.Add<TestComponent1>();

			Entity target = world.Instantiate(archetype);

			buffer.AddSharedComponent(target, shared1);
			buffer.Playback();

			Assert.Collection(world.ComponentManager.GetBlocks(archetype.AddShared(shared1)), accessor => {
				Assert.Equal(1, accessor.GetEntityData().Length);

				var shared = accessor.GetSharedComponentData<SharedComponent1>();
				Assert.Same(shared1, shared);
			});
		}

		[Fact]
		public void RemoveComponentFromEntity()
		{
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			EntityArchetype archetype = EntityArchetype.Empty;
			archetype = archetype.Add<TestComponent1>();
			archetype = archetype.AddShared(shared1);

			Entity target = world.Instantiate(archetype);

			buffer.RemoveComponent<TestComponent1>(target);
			buffer.Playback();

			Assert.Collection(world.ComponentManager.GetBlocks(archetype.Remove<TestComponent1>()), accessor => {
				Assert.Equal(1, accessor.GetEntityData().Length);
			});
		}

		[Fact]
		public void RemoveSharedComponentFromEntity()
		{
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			EntityArchetype archetype = EntityArchetype.Empty;
			archetype = archetype.Add<TestComponent1>();
			archetype = archetype.AddShared(shared1);

			Entity target = world.Instantiate(archetype);

			buffer.RemoveSharedComponent<SharedComponent1>(target);
			buffer.Playback();

			Assert.Collection(world.ComponentManager.GetBlocks(archetype.RemoveShared<SharedComponent1>()), accessor => {
				Assert.Equal(1, accessor.GetEntityData().Length);
			});
		}

		[Fact]
		public void Empty() {
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			Prefab prefab = new Prefab();
			prefab.AddComponent(new TestComponentVector3() { value = Vector3.up });
			prefab.AddSharedComponent(shared1);

			Assert.True(buffer.Empty());

			buffer.CreateEntity(prefab);

			Assert.False(buffer.Empty());

			buffer.Playback();

			Assert.True(buffer.Empty());
		}

#if DEBUG
		[Fact]
		public void ThrowIfNoTarget()
		{
			ECSWorld world = new ECSWorld();
			var buffer = new EntityCommandBuffer(world);

			SharedComponent1 shared1 = new SharedComponent1();
			Prefab prefab = new Prefab();
			prefab.AddComponent(new TestComponentVector3() { value = Vector3.up });
			prefab.AddSharedComponent(shared1);

			Assert.Throws<InvalidOperationException>(() => buffer.AddComponent(new TestComponent1()));
			buffer.CreateEntity(prefab);
			buffer.AddComponent(new TestComponent1());
			buffer.Playback();
			Assert.Throws<InvalidOperationException>(() => buffer.AddComponent(new TestComponent1()));
		}
#endif

	}
}
