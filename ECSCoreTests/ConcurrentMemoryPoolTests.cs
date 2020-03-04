using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Shared;
using Xunit;

namespace CoreTests
{
	public class ConcurrentMemoryPoolTests {
		private const int numThreads = 16;
		public ConcurrentMemoryPool<int> memoryPool = new ConcurrentMemoryPool<int>(1024 * numThreads);
		public ConcurrentBag<Memory<int>> fetched = new ConcurrentBag<Memory<int>>();

		[Fact]
		public void Size() {
			var mem = memoryPool.GetMemory(100);
			Assert.Equal(100, mem.Length);
		}

		[Fact]
		public void FreeAll() {
			var mem1 = memoryPool.GetMemory(100);
			
			memoryPool.FreeAll();

			var mem2 = memoryPool.GetMemory(100);

			mem1.Span[0] = 4;

			Assert.Equal(4, mem2.Span[0]);

			Assert.True(mem1.Span.Overlaps(mem2.Span));
		}

		[Fact]
		public void ConcurrentAccess() {
			Task[] threads = new Task[numThreads];
			for(int i = 0; i < numThreads; i++) {
				threads[i] = Task.Run(() => {
					for (int j = 0; j < 16; j++) {
						var mem = memoryPool.GetMemory(64);
						fetched.Add(mem);
					}
				});
			}

			Task.WaitAll(threads);

			var array = fetched.ToArray();

			Assert.Equal(16 * numThreads, array.Length);

			for (int i = 0; i < array.Length; i++) {
				for (int j = 0; j < array.Length; j++) {
					if (i == j) continue;
					Assert.False(array[i].Span.Overlaps(array[j].Span));
				}
			}
		}
	}
}
