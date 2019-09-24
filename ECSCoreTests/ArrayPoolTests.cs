using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Xunit;

namespace CoreTests {
	public class ArrayPoolTests {

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(1000)]
		public void Size(int arrSize) {
			PooledArray<byte> arr = PooledArray<byte>.Rent(arrSize);
			Assert.True(arr.array.Length >= arrSize);
		}

		[Fact]
		public void NegativeArrSize() {
			Assert.Throws<ArgumentOutOfRangeException>(() => PooledArray<byte>.Rent(-1));
		}

		[Fact]
		public void NotSame() {
			PooledArray<int> arr1 = PooledArray<int>.Rent(10);
			PooledArray<int> arr2 = PooledArray<int>.Rent(10);

			Assert.NotEqual(arr1.array.GetHashCode(), arr2.array.GetHashCode());

			arr1.Dispose();
			PooledArray<int> arr3 = PooledArray<int>.Rent(10);
			Assert.NotEqual(arr2.array.GetHashCode(), arr3.array.GetHashCode());

			arr2.Dispose();
			arr3.Dispose();
		}

		[Fact]
		public void RentReturnRent() {
			PooledArray<int> arr1 = PooledArray<int>.Rent(10);

			int hash = arr1.GetHashCode();

			arr1.Dispose();

			PooledArray<int> arr2 = PooledArray<int>.Rent(10);

			int hash2 = arr2.GetHashCode();

			Assert.Equal(hash, hash2);

			arr2.Dispose();
		}

		[Theory]
		[InlineData(0, true)]
		[InlineData(1, false)]
		[InlineData(10, false)]
		public void IsEmpty (int arrSize, bool expected) {
			PooledArray<byte> arr = PooledArray<byte>.Rent(arrSize);
			Assert.Equal(arr.IsEmpty(), expected);
		}
	}
}
