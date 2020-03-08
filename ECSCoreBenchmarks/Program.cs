using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Core.ECS;
using CoreBenchmarks.Jobs;


namespace CoreBenchmarks
{
	class Program
	{

		static void Main(string[] args)
		{
			/*
			Stopwatch sw = new Stopwatch();
			var benchmark = new UnsafeVsManagedBlockBenchmarks();
			benchmark.numEntities = 5000;
			benchmark.Setup();

			sw.Start();

			const int loops = 10_000;

			for (int j = 0; j < loops; j++)
			{
				benchmark.Managed();
				benchmark.Unsafe();
			}
			sw.Stop();

			benchmark.CleanUp();

			Console.WriteLine(sw.ElapsedMilliseconds + "ms.");
			Console.WriteLine(sw.ElapsedMilliseconds / (float)loops + "ms per 5000 ops");


	*/
			Console.WriteLine("SSE supported: " + Sse.IsSupported);
			Console.WriteLine("SSE2 supported: " + Sse2.IsSupported);
			Console.WriteLine("AVX supported: " + Avx.IsSupported);
			Console.WriteLine("AVX2 supported: " + Avx2.IsSupported);

			BenchmarkRunner.Run<EntityAccessBenchmarks>();
			//var eab = new EntityAccessBenchmarks();
			//eab.numEntities = 1_000_000;
			//eab.Setup();
			//for (int i = 0; i < 100; i++)
			//{
			//	eab.EntityRefAccess();
			//}
			//for (int i = 0; i < 100; i++)
			//{
			//	eab.EntitySetAccess();
			//}
			//for (int i = 0; i < 100; i++)
			//{
			//	eab.QueryAccess();
			//}
			//for (int i = 0; i < 100; i++)
			//{
			//	eab.QueryJobAccess();
			//}
			//eab.Cleanup();

			Console.WriteLine("done!");

			Console.ReadKey();
		}

	}
}
