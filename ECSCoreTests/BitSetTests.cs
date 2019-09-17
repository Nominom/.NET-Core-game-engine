using System;
using System.Collections.Generic;
using System.Text;
using ECSCore.Collections;
using Xunit;

namespace ECSCoreTests
{
	public class BitSetTests
	{

		[Fact]
		public void ContainsAll() {
			BitSet256 left = new BitSet256();
			BitSet256 right = new BitSet256();

			left.Set(0);
			left.Set(12);
			left.Set(70);
			left.Set(128);
			left.Set(156);
			left.Set(255);

			right.Set(0);
			right.Set(12);
			right.Set(70);
			right.Set(128);
			right.Set(255);

			Assert.True(left.ContainsAll(right));
			Assert.False(right.ContainsAll(left));
		}

		[Fact]
		public void HashCode() {
			for (int i = 0; i < 256; i++) {
				for (int j = 0; j < 256; j++) {
					BitSet256 left = new BitSet256();
					BitSet256 right = new BitSet256();

					left.Set(i);
					right.Set(j);

					int leftHash = left.GetHashCode();
					int rightHash = right.GetHashCode();

					if (i == j) {
						Assert.Equal(leftHash, rightHash);
					}
					else {
						Assert.NotEqual(leftHash, rightHash);
					}
				}
			}


			Random r = new Random(1234);
			for (int i = 0; i < 1000; i++) {
				BitSet256 left = new BitSet256();
				BitSet256 right = new BitSet256();
				for (int j = 0; j < 12; j++) {
					left.Set(r.Next(0, 256));
					right.Set(r.Next(0, 256));
				}

				int leftHash = left.GetHashCode();
				int rightHash = right.GetHashCode();

				if (left == right)
				{
					Assert.Equal(leftHash, rightHash);
				}
				else
				{
					Assert.NotEqual(leftHash, rightHash);
				}
			}
		}
	}
}
