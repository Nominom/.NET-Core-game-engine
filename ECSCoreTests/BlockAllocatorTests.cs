﻿using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Xunit;

namespace CoreTests {
	public class BlockAllocatorTests {

		[Fact]
		public void Size () {
			BlockAllocator kb16 = BlockAllocator.KB16;

			Assert.Equal(1024 * 16, kb16.NumBytes);

			using (var block = kb16.Rent()) {
				Assert.Equal(1024 * 16, block.Size());
				Assert.Equal(1024 * 16, block.Memory.Length);
			}

			BlockAllocator kb32 = BlockAllocator.KB32;

			Assert.Equal(1024 * 32, kb32.NumBytes);

			using (var block = kb32.Rent()) {
				Assert.Equal(1024 * 32, block.Size());
				Assert.Equal(1024 * 32, block.Memory.Length);
			}


			BlockAllocator kb64 = BlockAllocator.KB64;

			Assert.Equal(1024 * 64, kb64.NumBytes);

			using (var block = kb64.Rent()) {
				Assert.Equal(1024 * 64, block.Size());
				Assert.Equal(1024 * 64, block.Memory.Length);
			}

			BlockAllocator kb128 = BlockAllocator.KB128;

			Assert.Equal(1024 * 128, kb128.NumBytes);

			using (var block = kb128.Rent()) {
				Assert.Equal(1024 * 128, block.Size());
				Assert.Equal(1024 * 128, block.Memory.Length);
			}
		}

		[Theory]
		[InlineData(2)]
		[InlineData(4)]
		[InlineData(16)]
		[InlineData(128)]
		[InlineData(1028)]
		public void RentMany (int amount) {
			BlockAllocator kb16 = BlockAllocator.KB16;

			BlockMemory[] data = new BlockMemory[amount];
			for (int i = 0; i < amount; i++) {
				data[i] = kb16.Rent();
				Assert.Equal(1024 * 16, data[i].Memory.Length);
			}

			for (int i = 0; i < amount; i++) {
				for (int j = 0; j < amount; j++) {
					if(i == j) continue;
					
					Assert.False(data[i].Memory.Span.Overlaps(data[j].Memory.Span));
				}
			}

			for (int i = 0; i < amount; i++) {
				data[i].Dispose();
			}
		}

		[Theory]
		[InlineData(2)]
		[InlineData(4)]
		[InlineData(16)]
		[InlineData(128)]
		[InlineData(1028)]
		public void Alignment (int amount) {
			BlockAllocator kb16 = BlockAllocator.KB16;

			BlockMemory[] data = new BlockMemory[amount];
			for (int i = 0; i < amount; i++) {
				data[i] = kb16.Rent();
				unsafe {
					fixed (byte* bytes = data[i].memory.Span) {
						Assert.True((long)bytes % 32 == 0);
					}
				}
			}
			for (int i = 0; i < amount; i++) {
				data[i].Dispose();
			}
		}

		[Fact]
		public void RentReturnRent() {
			BlockAllocator kb16 = BlockAllocator.KB16;

			BlockMemory block = kb16.Rent();
			
			block.Dispose();

			BlockMemory block2 = kb16.Rent();

			Assert.True(block.Memory.Span.Overlaps(block2.Memory.Span));
		}
	}
}
