using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Core.Shared
{
	public class ConcurrentMemoryPool<T> where T : struct
	{
		private T[] data = new T[256];
		private int next = 0;
		private bool needToGrow = false;

		public ConcurrentMemoryPool(){ }

		public ConcurrentMemoryPool(int initialCapacity) {
			if (initialCapacity <= 0) {
				data = new T[256];
			}
			else {
				data = new T[initialCapacity];
			}
		}

		private void Grow() {
			data = new T[data.Length * 2];
		}

		public void FreeAll() {
			next = 0;
			if (needToGrow)
			{
				Grow();
				needToGrow = false;
			}
		}

		public Memory<T> GetMemory(int count) {
			int start = Interlocked.Add(ref next, count) - count;
			if (start + count > data.Length) {
				needToGrow = true;
				return new Memory<T>(new T[count]);
			}
			else
			{
				return new Memory<T>(data, start, count);
			}
		}
	}
}
