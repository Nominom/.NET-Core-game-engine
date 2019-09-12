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
using ECSCore;
using ECSCore.Numerics;

namespace ECSCoreBenchmarks
{
	[Config(typeof(NormalAsmConfig))]
	public class VectorBenchmarks
	{
		private ECSCore.Numerics.Vector3[] vec1;
		private ECSCore.Numerics.Vector3[] vec2;

		private System.Numerics.Vector3[] systemVec1;
		private System.Numerics.Vector3[] systemVec2;

		[Params(10, 100, 1000, 10_000)]
		public int numVectors;

		private float deltaTime = 0.1f;

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
		public float Baseline()
		{
			for (int i = 0; i < numVectors; i++) {
				Vector3 position = vec1[i];
				Vector3 velocity = vec2[i];

				Vector3 posDelta = velocity * deltaTime;
				vec1[i] = position + posDelta;
			}


			return vec1[0].x;
		}

//		[Benchmark]
//		public float SystemNumerics()
//		{
//			for (int i = 0; i < numVectors; i++)
//			{
//				var position = systemVec1[i];
//				var velocity = systemVec2[i];

//				var posDelta = System.Numerics.Vector3.Multiply(velocity, deltaTime);
//				var newPos = System.Numerics.Vector3.Add(position, posDelta);
//				systemVec1[i] = newPos;
//;			}

//			return systemVec1[0].X;
//		}

		[Benchmark]
		public float Sse() {
			var stack = MemoryStack.Default;
			stack.Reset();

			Span<Vector3> posDelta = stack.Get<Vector3>(vec1.Length);
			Vector3Sse.MultiplyScalar(vec2, deltaTime, posDelta);
			Vector3Sse.Add(vec1, posDelta, vec1);

			return vec1[0].x;
		}

		[Benchmark]
		public float Avx()
		{
			var stack = MemoryStack.Default;
			stack.Reset();

			Span<Vector3> posDelta = stack.Get<Vector3>(vec1.Length);
			Vector3Avx.MultiplyScalar(vec2, deltaTime, posDelta);
			Vector3Avx.Add(vec1, posDelta, vec1);

			return vec1[0].x;
		}

		[Benchmark]
		public float AvxVector() {
			int i;
			int leftovers = vec1.Length % AvxVector3.length;
			for (i = 0; i < vec1.Length - leftovers; i += AvxVector3.length) {
				var position = new AvxVector3(vec1, i);
				var velocity = new AvxVector3(vec2, i);

				var posDelta = velocity.Multiply(deltaTime);

				position = position.Add(posDelta);
				position.Store(vec1, i);
			}

			for (; i < vec1.Length; i++) {
				var position = vec1[i];
				var velocity = vec2[i];

				vec1[i] = position + (velocity * deltaTime);
			}

			return vec1[0].x;
		}

		[Benchmark]
		public float SseVector()
		{
			int i;
			int leftovers = vec1.Length % SseVector3.length;
			for (i = 0; i < vec1.Length - leftovers; i += SseVector3.length)
			{
				var position = new SseVector3(vec1, i);
				var velocity = new SseVector3(vec2, i);

				var posDelta = velocity.Multiply(deltaTime);

				position = position.Add(posDelta);
				position.Store(vec1, i);
			}

			for (; i < vec1.Length; i++)
			{
				var position = vec1[i];
				var velocity = vec2[i];

				vec1[i] = position + (velocity * deltaTime);
			}

			return vec1[0].x;
		}

	}
}
