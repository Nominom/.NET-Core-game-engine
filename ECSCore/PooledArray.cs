using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Core.ECS
{
	public class ECSArrayPool<T> {
		public static ECSArrayPool<T> Shared { get; } = new ECSArrayPool<T>();

		private T[][] buckets;
		private int[] nextFrees;

		private int minSize = 0b1 << 4;
		private readonly int arraysPerBucket = 10;
		private readonly int bucketCount = 20;

		//Thread local buckets
		[ThreadStatic] private static T[][] tl_buckets;

		internal ECSArrayPool() {
			buckets = new T[bucketCount * arraysPerBucket][];
			nextFrees = new int[bucketCount];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetBucketIndex(int minLength) {
			int i = 0;
			int size = minSize;
			while (size < minLength) {
				size <<= 1;
				i++;
			}
			return i;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T[] GetFreeArray(int bucket) {
			if (bucket >= bucketCount) {
				return null;
			}
			//Span<T[]> arrays = new Span<T[]>(buckets, bucket * arraysPerBucket, arraysPerBucket);
			lock (this) {
				int nextSlot = nextFrees[bucket];
				if (nextSlot == 0) {
					return GetFreeArray(bucket + 1);
				}
				else {
					int idx = bucket * arraysPerBucket + nextSlot - 1;
					//T[] arr = arrays[nextFree-1];
					T[] arr = buckets[idx];
					buckets[idx] = null;
					nextFrees[bucket]--;
					return arr;
				}
			}
			/*
			for (int i = 0; i < arrays.length; i++) {
				T[] arr = arrays[i];
				if (arr != null) {
					return arr;
				}
			}*/
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ReturnArray(T[] arr, int bucket) {
			if (bucket >= bucketCount) {
				return;
			}
			//Span<T[]> arrays = new Span<T[]>(buckets, bucket * arraysPerBucket, arraysPerBucket);
			lock (this) {
				int nextFree = nextFrees[bucket];
				if (nextFree < arraysPerBucket) {
					buckets[bucket * arraysPerBucket + nextFree] = arr;
					//arrays[nextFree] = arr;
					nextFrees[bucket]++;
				}
			}
		}

		private T[] AllocateNew(int bucket) {
			return new T[minSize << bucket];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PooledArray<T> Rent(int minLength) {
			if (minLength < 0) { 
				throw new ArgumentOutOfRangeException(nameof(minLength));
			}
			if (minLength == 0) {
				return new PooledArray<T>(Array.Empty<T>(), this);
			}

			int bucket = GetBucketIndex(minLength);

			if (bucket >= bucketCount) {
				return new PooledArray<T>(new T[minLength], this);
			}

			T[] arr = null;
			if (tl_buckets != null) {
				arr = tl_buckets[bucket];
				tl_buckets[bucket] = null;
			}
			if (arr == null) {
				arr = GetFreeArray(bucket) ?? AllocateNew(bucket);
			}
			return new PooledArray<T>(arr, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void Return(T[] arr) {
			if (arr == null || arr.Length == 0) {
				return;
			}

			int bucket = GetBucketIndex(arr.Length);

			if (bucket >= bucketCount) {
				return;
			}
			if (arr.Length != (minSize << bucket)) {
				throw new ArgumentException();
			}

			if (tl_buckets == null) {
				tl_buckets = new T[bucketCount][];
			}
			if (tl_buckets[bucket] != null) {
				ReturnArray(tl_buckets[bucket], bucket);
			}
			tl_buckets[bucket] = arr;
		} 
	}

	public struct PooledArray<T> : IDisposable {
		public readonly T[] array;
		private readonly ECSArrayPool<T> origin;

		internal PooledArray(T[] arr, ECSArrayPool<T> origin) {
			array = arr;
			this.origin = origin;
		}

		public void Dispose() {
			origin?.Return(array);
		}

		public bool IsEmpty() {
			if (array == null) return true;
			return array.Length == 0;
		}

		public static PooledArray<T> Rent (int minLength) => ECSArrayPool<T>.Shared.Rent(minLength);

		public static PooledArray<T> Empty() {
			return new PooledArray<T>(Array.Empty<T>(), null);
		}
	}
}
