using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Core;

namespace TestApp
{
	public class Program
	{
		public static unsafe void TestThing() {
			float* matrix = stackalloc float[16] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15};

			var avx1 = Avx.LoadVector256(matrix);
			var avx2 = Avx.LoadVector256(&matrix[8]);

			Vector256<int> permuter = Vector256.Create(0, 2, 4, 6, 1, 3, 5, 7);

			var tmp1 = Avx2.PermuteVar8x32(avx1, permuter);
			var tmp2 = Avx2.PermuteVar8x32(avx2, permuter);

			var res1 = Avx.Shuffle(tmp1, tmp2, 0b10_00_10_00);
			var res2 = Avx.Shuffle(tmp1, tmp2, 0b11_01_11_01);

			Console.WriteLine(res1);
			Console.WriteLine(res2);
		}

		static void Main(string[] args)
		{
			/*
			CoreEngine.Initialize();
			CoreEngine.Run();*/

			TestThing();
			Console.ReadKey();
		}
	}
}
