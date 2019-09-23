using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Core.ECS.Numerics;
using ECSCoreBenchmarks;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace CoreBenchmarks
{
	[Config(typeof(NormalAsmConfig))]
	public class Matrix4x4TransposeBenchmarks
	{

		private System.Numerics.Matrix4x4 numericsMat1;

		private Core.ECS.Numerics.Matrix4x4 mat1;

		[GlobalSetup]
		public void Setup()
		{
			numericsMat1 = Matrix4x4.CreateFromYawPitchRoll(10, 3, 3);

			mat1 = new Core.ECS.Numerics.Matrix4x4(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
		}

		[Benchmark(Baseline = true)]
		public Core.ECS.Numerics.Matrix4x4 TransposeNormal() {
			return Core.ECS.Numerics.Matrix4x4.Transpose(mat1);
		}

		[Benchmark]
		public Core.ECS.Numerics.Matrix4x4 TransposeRef()
		{
			return Core.ECS.Numerics.Matrix4x4.TransposeRef(mat1);
		}

		[Benchmark]
		public Matrix4x4 NumericsTranspose()
		{
			return Matrix4x4.Transpose(numericsMat1);
		}

		[Benchmark]
		public Core.ECS.Numerics.Matrix4x4 SseTranspose()
		{
			return Matrix4x4Sse.Transpose(mat1);
		}

		[Benchmark]
		public Core.ECS.Numerics.Matrix4x4 SseTransposeRef()
		{
			return Matrix4x4Sse.TransposeRef(mat1);
		}

		[Benchmark]
		public Core.ECS.Numerics.Matrix4x4 SseTransposeStore()
		{
			return Matrix4x4Sse.TransposeStore(mat1);
		}

		[Benchmark]
		public Core.ECS.Numerics.Matrix4x4 AvxTranspose()
		{
			return Matrix4x4Avx.Transpose(mat1);
		}

		[Benchmark]
		public Core.ECS.Numerics.Matrix4x4 AvxTransposeLoadArray()
		{
			return Matrix4x4Avx.TransposeLoadArray(mat1);
		}
	}
}
