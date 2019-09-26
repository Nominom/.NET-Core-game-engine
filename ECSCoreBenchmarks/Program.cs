using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Core.ECS;


namespace CoreBenchmarks
{
	class Program
	{

		static void Main(string[] args)
		{

			Stopwatch sw = new Stopwatch();
			var benchmark = new ProcessEntitiesBenchmark();
			benchmark.numEntities = 1_000_000;
			benchmark.Setup();

			sw.Start();

			const int loops = 100;

			for (int j = 0; j < loops; j++)
			{
				benchmark.NormalStyle();
			}


			sw.Stop();
			Console.WriteLine(sw.ElapsedMilliseconds / loops + "ms per million ops");

			Console.WriteLine("SSE supported: " + Sse.IsSupported);
			Console.WriteLine("SSE2 supported: " + Sse2.IsSupported);
			Console.WriteLine("AVX supported: " + Avx.IsSupported);
			Console.WriteLine("AVX2 supported: " + Avx2.IsSupported);

			//BenchmarkRunner.Run<ProcessEntitiesBenchmark>();

			//Console.ReadKey();
		}
	}
}
