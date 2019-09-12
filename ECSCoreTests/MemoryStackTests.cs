using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ECSCore;
using Xunit;

namespace ECSCoreTests
{
	public class MemoryStackTests
	{

		[Fact]
		public void Default() {
			var stack = MemoryStack.Default;
			Assert.NotNull(stack);
		}

		private MemoryStack stackThread1;
		private MemoryStack stackThread2;

		private void SetStack1() {
			stackThread1 = MemoryStack.Default;
		}

		private void SetStack2() {
			stackThread2 = MemoryStack.Default;
		}

		[Fact]
		public void ThreadDifferent()
		{
			Thread t1 = new Thread(SetStack1);
			Thread t2 = new Thread(SetStack2);

			t1.Start();
			t2.Start();

			t1.Join();
			t2.Join();

			Assert.NotSame(stackThread1, stackThread2);
		}

		[Theory]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(1000)]
		public void GetAligned(int numElements) {
			var stack = MemoryStack.Default;
			stack.Reset();

			var span = stack.GetAligned<float>(numElements);

			unsafe {
				fixed (float* ptr = span) {
					Assert.True(((long)ptr % 32 == 0), "Span was not aligned memory");
				}
			}

			Assert.Equal(numElements, span.Length);
		}

		[Theory]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(1000)]
		[InlineData(1_000_000)]
		public void Get(int numElements)
		{
			var stack = MemoryStack.Default;
			stack.Reset();

			var span = stack.Get<float>(numElements);

			Assert.Equal(numElements, span.Length);
		}

		[Theory]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(1000)]
		public void GetManyAligned(int numElements)
		{
			var stack = MemoryStack.Default;
			stack.Reset();

			for (int i = 0; i < 10; i++) {
				var span = stack.GetAligned<float>(numElements);
				unsafe
				{
					fixed (float* ptr = span)
					{
						Assert.True(((long)ptr % 32 == 0), $"Span was not aligned memory alignment: {(long)ptr % 32}");
					}
				}
				Assert.Equal(numElements, span.Length);
			}
			
		}

		[Theory]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(1000)]
		public void GetMany(int numElements)
		{
			var stack = MemoryStack.Default;
			stack.Reset();

			for (int i = 0; i < 10; i++)
			{
				var span = stack.Get<float>(numElements);
				Assert.Equal(numElements, span.Length);
			}

		}

	}
}
