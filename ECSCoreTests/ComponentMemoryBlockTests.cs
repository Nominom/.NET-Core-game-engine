using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Core.ECS;
using Core.Shared;
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
			block.AddEntity(new Entity (1,0));
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
				Entity e = new Entity (i + 1,  0 );
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
				block.AddEntity(new Entity (i+1,0));
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

			Span<byte> entityBytes = entityData.Cast<Entity, byte>();
			Span<byte> com1Bytes = componentData.Cast<ComponentTests.TestComponentWithInt, byte>();
			Span<byte> com2Bytes = componentData2.Cast<ComponentTests.TestEmptyComponent, byte>();


			Assert.False(entityBytes.Overlaps(com1Bytes));
			Assert.False(entityBytes.Overlaps(com2Bytes));
			Assert.False(com1Bytes.Overlaps(com2Bytes));
		}

		[Fact]
		public void Aligned() {
			ComponentMemoryBlock block = new ComponentMemoryBlock(testArchetype);


			var entityData = block.GetEntityData();
			var componentData = block.GetComponentData<ComponentTests.TestComponentWithInt>();
			var componentData2 = block.GetComponentData<ComponentTests.TestEmptyComponent>();

			Span<byte> entityBytes = entityData.Cast<Entity, byte>();
			Span<byte> com1Bytes = componentData.Cast<ComponentTests.TestComponentWithInt, byte>();
			Span<byte> com2Bytes = componentData2.Cast<ComponentTests.TestEmptyComponent, byte>();
			unsafe {
				fixed (byte* bytes = entityBytes) {
					Assert.True((long)bytes % 32 == 0);
				}
				fixed (byte* bytes = com1Bytes) {
					Assert.True((long)bytes % 32 == 0);
				}
				fixed (byte* bytes = com2Bytes) {
					Assert.True((long)bytes % 32 == 0);
				}
			}
			
		}
	}
}
