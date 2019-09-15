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
	}
}
