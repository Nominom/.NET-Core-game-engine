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
	public class Matrix4x4MultiplyBenchmarks
	{

		private System.Numerics.Matrix4x4 numericsMat1;
		private System.Numerics.Matrix4x4 numericsMat2;

		private Core.ECS.Numerics.Matrix4x4 mat1;
		private Core.ECS.Numerics.Matrix4x4 mat2;

		[GlobalSetup]
		public void Setup()
		{
			numericsMat1 = Matrix4x4.CreateFromYawPitchRoll(10, 3, 3);
			numericsMat2 = Matrix4x4.CreateOrthographic(10, 10, 0.1f, 1000);

			mat1 = new Core.ECS.Numerics.Matrix4x4(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
			mat2 = new Core.ECS.Numerics.Matrix4x4(11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26);
		}

		[Benchmark(Baseline = true)]
		public Core.ECS.Numerics.Matrix4x4 MultiplyNormal() {
			return Core.ECS.Numerics.Matrix4x4.Multiply(mat1, mat2);
		}

		[Benchmark]
		public Core.ECS.Numerics.Matrix4x4 MultiplyRef()
		{
			return Core.ECS.Numerics.Matrix4x4.MultiplyRef(mat1, mat2);
		}

		[Benchmark]
		public Matrix4x4 NumericsMultiply()
		{
			return Matrix4x4.Multiply(numericsMat1, numericsMat2);
		}

		[Benchmark]
		public Core.ECS.Numerics.Matrix4x4 SseMultiply()
		{
			return Matrix4x4Sse.Multiply(mat1, mat2);
		}

	}
}
