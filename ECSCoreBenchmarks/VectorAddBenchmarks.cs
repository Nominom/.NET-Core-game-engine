using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECSCoreBenchmarks
{
	[Config(typeof(NormalAsmConfig))]
	public class VectorAddBenchmarks
	{
		private ECSCore.Numerics.Vector3[] vec1;
		private ECSCore.Numerics.Vector3[] vec2;

		private System.Numerics.Vector3[] systemVec1;
		private System.Numerics.Vector3[] systemVec2;

		[Params(10, 100, 1000, 100_000)]
		public int numVectors;

		[GlobalSetup]
		public void Setup()
		{
			vec1 = new ECSCore.Numerics.Vector3[numVectors];
			vec2 = new ECSCore.Numerics.Vector3[numVectors];

			systemVec1 = new System.Numerics.Vector3[numVectors];
			systemVec2 = new System.Numerics.Vector3[numVectors];

			for (int i = 0; i < numVectors; i++)
			{
				vec1[i] = new ECSCore.Numerics.Vector3(i, i, i);
				vec2[i] = new ECSCore.Numerics.Vector3(i, i, i);

				systemVec1[i] = new System.Numerics.Vector3(i, i, i);
				systemVec2[i] = new System.Numerics.Vector3(i, i, i);
			}
		}

		[Benchmark(Baseline = true)]
		[BenchmarkCategory("Add")]
		public float BaselineAdd()
		{
			for (int i = 0; i < numVectors; i++)
			{
				vec1[i] = vec1[i] + vec2[i];
			}


			return vec1[0].x;
		}


		[Benchmark]
		[BenchmarkCategory("Add")]
		public float SystemNumericsAdd()
		{
			for (int i = 0; i < numVectors; i++)
			{
				systemVec1[i] = System.Numerics.Vector3.Add(systemVec1[i], systemVec2[i]);
			}

			return systemVec1[0].X;
		}

		[Benchmark]
		[BenchmarkCategory("Add")]
		public float SseAdd()
		{
			ECSCore.Numerics.Vector3Sse.Add(vec1, vec2, vec1);

			return vec1[0].x;
		}

		[Benchmark]
		[BenchmarkCategory("Add")]
		public float AvxAdd()
		{
			ECSCore.Numerics.Vector3Avx.Add(vec1, vec2, vec1);

			return vec1[0].x;
		}





		[Benchmark]
		[BenchmarkCategory("Multiply")]
		public float BaselineMultiply()
		{
			for (int i = 0; i < numVectors; i++)
			{
				vec1[i] = vec1[i] * vec2[i];
			}


			return vec1[0].x;
		}

		//[Benchmark]
		//[BenchmarkCategory("Multiply")]
		//public float SystemNumericsMultiply()
		//{
		//	for (int i = 0; i < numVectors; i++)
		//	{
		//		systemVec1[i] = System.Numerics.Vector3.Multiply(systemVec1[i], systemVec2[i]);
		//	}

		//	return systemVec1[0].X;
		//}

		[Benchmark]
		[BenchmarkCategory("Multiply")]
		public float SseMultiply()
		{
			ECSCore.Numerics.Vector3Sse.Multiply(vec1, vec2, vec1);

			return vec1[0].x;
		}

		[Benchmark]
		[BenchmarkCategory("Multiply")]
		public float AvxMultiply()
		{
			ECSCore.Numerics.Vector3Avx.Multiply(vec1, vec2, vec1);

			return vec1[0].x;
		}



		[Benchmark]
		[BenchmarkCategory("Normalize")]
		public float BaselineNormalize()
		{
			for (int i = 0; i < numVectors; i++)
			{
				vec1[i] = vec2[i].Normalized();
			}

			return vec1[0].x;
		}

		[Benchmark]
		[BenchmarkCategory("Normalize")]
		public float SystemNumericsNormalize()
		{
			for (int i = 0; i < numVectors; i++)
			{
				systemVec1[i] = System.Numerics.Vector3.Normalize(systemVec2[i]);
			}

			return systemVec1[0].X;
		}

		[Benchmark]
		[BenchmarkCategory("Normalize")]
		public float AvxNormalize()
		{
			ECSCore.Numerics.Vector3Avx.Normalize(vec2, vec1);

			return vec1[0].x;
		}
	}
}
