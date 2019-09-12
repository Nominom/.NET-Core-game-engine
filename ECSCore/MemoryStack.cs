using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ECSCore
{
	public unsafe class MemoryStack
	{
		[ThreadStatic]
		private static MemoryStack _default;

		public static MemoryStack Default {
			get {
				if (_default == null)
				{
					_default = new MemoryStack(1024 * 1024);
				}

				return _default;
			}
		}

		private IntPtr buffer;
		private IntPtr next;
		private long numBytes;

		internal MemoryStack(int bytes)
		{
			buffer = Marshal.AllocHGlobal(bytes);
			next = buffer;
			numBytes = bytes;
		}

		~MemoryStack()
		{
			Marshal.FreeHGlobal(buffer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Align()
		{
			long current = next.ToInt64();
			long alignmentVal = 32 - (current % 32);
			next = new IntPtr(current + alignmentVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Increment<T>(int numElements) where T : unmanaged
		{
			long elementSize = Marshal.SizeOf<T>();
			long current = next.ToInt64();
			next = new IntPtr(current + (elementSize * numElements));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool HasRoom<T>(int numElements) where T : unmanaged
		{
			long elementSize = Marshal.SizeOf<T>();
			long currentPtr = next.ToInt64();
			long bufferEnd = buffer.ToInt64() + numBytes;

			return (currentPtr + (elementSize * numElements)) < bufferEnd;
		}


		public void Reset()
		{
			next = buffer;
		}

		public Span<T> GetAligned<T>(int numElements, bool clear = false) where T : unmanaged
		{
			Align();

			if (!HasRoom<T>(numElements)) {
				return new T[numElements];
			}

			Span<T> span = new Span<T>(next.ToPointer(), numElements);

			Increment<T>(numElements);

			if (clear)
			{
				span.Clear();
			}

			return span;
		}

		public Span<T> Get<T>(int numElements, bool clear = false) where T : unmanaged
		{
			if (!HasRoom<T>(numElements))
			{
				return new T[numElements];
			}

			Span<T> span = new Span<T>(next.ToPointer(), numElements);

			Increment<T>(numElements);

			if (clear) {
				span.Clear();
			}

			return span;
		}
	}
}
