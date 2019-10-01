using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;

namespace Core.ECS
{
	internal class BigChunk {
		public readonly byte[] data;
		public readonly int length;
		public readonly bool[] inUse;

		public BigChunk (int chunkSizeKb, int chunkAmount) {
			data = new byte[1024 * chunkSizeKb * chunkAmount];
			length = chunkAmount;
			inUse = new bool[chunkAmount];
		}

		public bool HasFreeBlocks() {
			for (int i = 0; i < inUse.Length; i++) {
				if (!inUse[i]) return true;
			}
			return false;
		}

		public int Reserve() {
			for (int i = 0; i < inUse.Length; i++) {
				if (!inUse[i]) {
					inUse[i] = true;
					return i;
				}
			}
			return -1;
		}

		public void Free(int index) {
			inUse[index] = false;
		}
	}

	internal class BlockMemory : IMemoryOwner<byte> {
		private readonly BigChunk chunk;
		private readonly int chunkIndex;
		private readonly int chunkSize;
		public readonly Memory<byte> memory;

		public BlockMemory(BigChunk chunk, int cIndex, int cSize) {
			this.chunk = chunk;
			chunkIndex = cIndex;
			chunkSize = cSize;
			memory = new Memory<byte>(chunk.data, chunkSize * chunkIndex, chunkSize);
		}

		public void Dispose() {
			chunk.Free(chunkIndex);
		}

		public int Size() {
			return chunkSize;
		}

		public Memory<byte> Memory => memory;
	}


	internal class BlockAllocator{
		private const int bigChunkSizeKb = 2048;

		public static BlockAllocator KB16 { get; } = new BlockAllocator(16);
		public static BlockAllocator KB32 { get; } = new BlockAllocator(32);
		public static BlockAllocator KB64 { get; } = new BlockAllocator(64);
		public static BlockAllocator KB128 { get; } = new BlockAllocator(128);


		private int numKB;
		private List<BigChunk> datablocks;

		public int NumBytes => numKB * 1024;


		public BlockAllocator (int numKB) {
			this.numKB = numKB;
			datablocks = new List<BigChunk>();
		}

		public BlockMemory Rent() {
			lock (this) {
				foreach (BigChunk chunk in datablocks) {
					if (chunk.HasFreeBlocks()) {
						int index = chunk.Reserve();
						BlockMemory memory = new BlockMemory(chunk, index, NumBytes);
						return memory;
					}
				}

				BigChunk newChunk = new BigChunk(numKB, bigChunkSizeKb / numKB);
				datablocks.Add(newChunk);

				int i = newChunk.Reserve();
				BlockMemory m = new BlockMemory(newChunk, i, NumBytes);
				return m;
			}
		}
	}







	internal unsafe class UnsafeBlockMemory : IDisposable {
		private readonly IntPtr ptr;
		private readonly int size;

		public Span<byte> Span => new Span<byte>(ptr.ToPointer(), size);

		public UnsafeBlockMemory(IntPtr ptr, int size) {
			this.ptr = ptr;
			this.size = size;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Free();
		}

		public int Size()
		{
			return size;
		}

		public void* GetPointer() {
			return ptr.ToPointer();
		}

		~UnsafeBlockMemory() {
			Free();
		}

		private void Free() {
			Marshal.FreeHGlobal(ptr);
		}
	}


	internal unsafe class UnsafeBlockAllocator
	{
		private const int bigChunkSizeKb = 2048;

		public static UnsafeBlockAllocator KB16 { get; } = new UnsafeBlockAllocator(16);
		public static UnsafeBlockAllocator KB32 { get; } = new UnsafeBlockAllocator(32);


		private int numKB;

		public int NumBytes => numKB * 1024;


		public UnsafeBlockAllocator(int numKB)
		{
			this.numKB = numKB;
		}

		public UnsafeBlockMemory Rent() {
			var ptr = Marshal.AllocHGlobal(NumBytes);
			return new UnsafeBlockMemory(ptr, NumBytes);
		}
	}
}
