using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Core.ECS;
using Xunit;

namespace CoreTests {
	public class ComponentMemoryBlockTests {

		public EntityArchetype testArchetype =
			EntityArchetype.Empty
				.Add<ComponentTests.TestComponentWithInt>()
				.Add<ComponentTests.TestEmptyComponent>();

		[Fact]
		public void Size () {
			ComponentMemoryBlock block = new ComponentMemoryBlock(testArchetype);
			Assert.Equal(0, block.Size);
			block.AddEntity(new Entity() {id = 1, version = 1});
			Assert.Equal(1, block.Size);

			block.Dispose();
		}

		[Fact]
		public void MaxSize () {
			ComponentMemoryBlock block = new ComponentMemoryBlock(testArchetype);
			Assert.True(block.MaxSize > 1);
		}

		[Fact]
		public void HasRoom () {
			ComponentMemoryBlock block = new ComponentMemoryBlock(testArchetype);

			for (int i = 0; i < block.MaxSize; i++) {
				Assert.True(block.HasRoom);
				Entity e = new Entity(){id = i + 1, version = 1};
				block.AddEntity(e);
			}

			Assert.False(block.HasRoom);
		}

		[Fact]
		public void GetComponentData() {
			ComponentMemoryBlock block = new ComponentMemoryBlock(testArchetype);

			var componentData = block.GetComponentData<ComponentTests.TestComponentWithInt>();
			Assert.Equal(block.MaxSize, componentData.Length);

			var componentData2 = block.GetComponentData<ComponentTests.TestEmptyComponent>();
			Assert.Equal(block.MaxSize, componentData2.Length);
		}

		[Fact]
		public void GetEntityData () {
			ComponentMemoryBlock block = new ComponentMemoryBlock(testArchetype);

			var entityData = block.GetEntityData();
			Assert.Equal(block.MaxSize, entityData.Length);
		}

		[Fact]
		public void ModifyComponentData () {
			ComponentMemoryBlock block = new ComponentMemoryBlock(testArchetype);

			var componentData = block.GetComponentData<ComponentTests.TestComponentWithInt>();

			for (int i = 0; i < block.MaxSize; i++) {
				block.AddEntity(new Entity() {id = i + 1});
				componentData[i].someInt = i;
			}

			componentData = block.GetComponentData<ComponentTests.TestComponentWithInt>();

			for (int i = 0; i < block.MaxSize; i++) {
				Assert.Equal(i, componentData[i].someInt);
			}

		}

		[Fact]
		public void NonOverlapping() {
			ComponentMemoryBlock block = new ComponentMemoryBlock(testArchetype);


			var entityData = block.GetEntityData();
			var componentData = block.GetComponentData<ComponentTests.TestComponentWithInt>();
			var componentData2 = block.GetComponentData<ComponentTests.TestEmptyComponent>();

			Span<byte> entityBytes = MemoryMarshal.Cast<Entity, byte>(entityData);
			Span<byte> com1Bytes = MemoryMarshal.Cast<ComponentTests.TestComponentWithInt, byte>(componentData);
			Span<byte> com2Bytes = MemoryMarshal.Cast<ComponentTests.TestEmptyComponent, byte>(componentData2);


			Assert.False(entityBytes.Overlaps(com1Bytes));
			Assert.False(entityBytes.Overlaps(com2Bytes));
			Assert.False(com1Bytes.Overlaps(com2Bytes));
		}
	}
}
