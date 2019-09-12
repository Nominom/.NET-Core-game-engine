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
	public class VectorMultiplyScalarBenchmarks {
		private const float scalar = 5f;
		private ECSCore.Numerics.Vector3[] vec1;

		private System.Numerics.Vector3[] systemVec1;

		[Params(10, 100, 1000, 100_000)]
		public int numVectors;

		[GlobalSetup]
		public void Setup()
		{
			vec1 = new ECSCore.Numerics.Vector3[numVectors];

			systemVec1 = new System.Numerics.Vector3[numVectors];

			for (int i = 0; i < numVectors; i++)
			{
				vec1[i] = new ECSCore.Numerics.Vector3(i, i, i);

				systemVec1[i] = new System.Numerics.Vector3(i, i, i);
			}
		}


		[Benchmark(Baseline = true)]
		public float BaselineMultiplyScalar()
		{
			for (int i = 0; i < numVectors; i++)
			{
				vec1[i] = vec1[i] * scalar;
			}


			return vec1[0].x;
		}

		[Benchmark]
		public float SystemNumericsMultiplyScalar()
		{
			for (int i = 0; i < numVectors; i++)
			{
				systemVec1[i] = System.Numerics.Vector3.Multiply(systemVec1[i], scalar);
			}

			return systemVec1[0].X;
		}

		[Benchmark]
		public float SseMultiplyScalar()
		{
			ECSCore.Numerics.Vector3Sse.MultiplyScalar(vec1, scalar, vec1);

			return vec1[0].x;
		}

		[Benchmark]
		public float AvxMultiplyScalar()
		{
			ECSCore.Numerics.Vector3Avx.MultiplyScalar(vec1, scalar, vec1);

			return vec1[0].x;
		}

	}
}
