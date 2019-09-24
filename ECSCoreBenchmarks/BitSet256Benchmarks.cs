using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;
using BenchmarkDotNet.Attributes;
using Core.ECS.Collections;
using Core.ECS.Collections.JavaPort;

namespace CoreBenchmarks
{
	[Config(typeof(NormalAsmConfig))]
	public class BitSet256Benchmarks {

		private BitSet256 bitSet1;
		private BitSet256 bitSet2;

		private BitSet javaBitSet1;
		private BitSet javaBitSet2;

		[GlobalSetup]
		public void Setup() {
			bitSet1.Set(0);
			bitSet1.Set(35);
			bitSet1.Set(79);
			bitSet1.Set(120);

			bitSet2.Set(0);
			bitSet2.Set(35);
			bitSet2.Set(79);
			bitSet2.Set(120);

			javaBitSet1 = new BitSet(256);
			javaBitSet2 = new BitSet(256);

			javaBitSet1.Set(0);
			javaBitSet1.Set(35);
			javaBitSet1.Set(79);
			javaBitSet1.Set(120);

			javaBitSet2.Set(0);
			javaBitSet2.Set(35);
			javaBitSet2.Set(79);
			javaBitSet2.Set(120);
		}


		//[Benchmark]
		//public BitSet SetJava()
		//{
		//	javaBitSet1.Set(11);
		//	javaBitSet2.Set(12);

		//	return javaBitSet1;
		//}

		//[Benchmark]
		//public ref BitSet256 SetBitSet256()
		//{
		//	bitSet1.Set(11);
		//	bitSet2.Set(12);

		//	return ref bitSet1;
		//}

		//[Benchmark]
		//public BitSet AndJava()
		//{
		//	javaBitSet1.And(javaBitSet2);

		//	return javaBitSet1;
		//}

		//[Benchmark]
		//public BitSet256 AndBitSet256()
		//{
		//	BitSet256 newSet = bitSet1.And(bitSet2);
		//	return newSet;
		//}

		//[Benchmark]
		//public bool ContainsAllJava()
		//{
		//	return javaBitSet1.ContainsAll(javaBitSet2);
		//}

		//[Benchmark]
		//public bool ContainsAll()
		//{
		//	return bitSet1.ContainsAll(bitSet2);
		//}

		//[Benchmark]
		//public bool IntersectsJava()
		//{
		//	return javaBitSet1.Intersects(javaBitSet2);
		//}

		//[Benchmark]
		//public bool Intersects()
		//{
		//	return bitSet1.ContainsAny(bitSet2);
		//}


		[Benchmark]
		public int HashCode() {
			return bitSet1.GetHashCode();
		}

	}
}
