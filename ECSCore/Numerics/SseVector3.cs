using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace ECSCore.Numerics
{
	public ref struct SseVector3
	{
		public const int length = 8;
		public Vector128<float> x;
		public Vector128<float> y;
		public Vector128<float> z;

		public unsafe SseVector3(ReadOnlySpan<Vector3> origin, int offset)
		{
			fixed (Vector3* ptr = origin)
			{
				float* fp = (float*)&ptr[offset];
				Load4xyz(fp, out x, out y, out z);
			}
		}

		public unsafe void Store(Span<Vector3> destination, int offset)
		{
			fixed (Vector3* ptr = destination)
			{
				float* fp = (float*)&ptr[offset];
				Store4xyz(fp, x, y, z);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SseVector3 Add(in SseVector3 other)
		{

			return new SseVector3()
			{
				x = Sse.Add(x, other.x),
				y = Sse.Add(y, other.y),
				z = Sse.Add(z, other.z)
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SseVector3 Multiply(in SseVector3 other)
		{
			return new SseVector3()
			{
				x = Sse.Multiply(x, other.x),
				y = Sse.Multiply(y, other.y),
				z = Sse.Multiply(z, other.z)
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe SseVector3 Multiply(float scalar)
		{
			Vector128<float> scalarV = Vector128.Create(scalar, scalar, scalar, scalar);
			return new SseVector3()
			{
				x = Sse.Multiply(x, scalarV),
				y = Sse.Multiply(y, scalarV),
				z = Sse.Multiply(z, scalarV)
			};
		}








		/// <summary>
		/// https://software.intel.com/en-us/articles/3d-vector-normalization-using-256-bit-intel-advanced-vector-extensions-intel-avx
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe void Load4xyz(float* src, out Vector128<float> x, out Vector128<float> y,
			out Vector128<float> z)
		{
			//__m128 x0y0z0x1 = _mm_load_ps(p + 0);
			Vector128<float> x0y0z0x1 = Sse.LoadVector128(&src[0]);
			//__m128 y1z1x2y2 = _mm_load_ps(p + 4);
			Vector128<float> y1z1x2y2 = Sse.LoadVector128(&src[4]);
			//__m128 z2x3y3z3 = _mm_load_ps(p + 8);
			Vector128<float> z2x3y3z3 = Sse.LoadVector128(&src[8]);

			//__m128 x2y2x3y3 = _mm_shuffle_ps(y1z1x2y2, z2x3y3z3, _MM_SHUFFLE(2, 1, 3, 2));
			Vector128<float> x2y2x3y3 = Sse.Shuffle(y1z1x2y2, z2x3y3z3, 0b_10_01_11_10);
			//__m128 y0z0y1z1 = _mm_shuffle_ps(x0y0z0x1, y1z1x2y2, _MM_SHUFFLE(1, 0, 2, 1));
			Vector128<float> y0z0y1z1 = Sse.Shuffle(x0y0z0x1, y1z1x2y2, 0b_01_00_10_01);

			//__m128 x = _mm_shuffle_ps(x0y0z0x1, x2y2x3y3, _MM_SHUFFLE(2, 0, 3, 0)); // x0x1x2x3
			x = Sse.Shuffle(x0y0z0x1, x2y2x3y3, 0b_10_00_11_00);
			//__m128 y = _mm_shuffle_ps(y0z0y1z1, x2y2x3y3, _MM_SHUFFLE(3, 1, 2, 0)); // y0y1y2y3
			y = Sse.Shuffle(y0z0y1z1, x2y2x3y3, 0b_11_01_10_00);
			//__m128 z = _mm_shuffle_ps(y0z0y1z1, z2x3y3z3, _MM_SHUFFLE(3, 0, 3, 1)); // z0z1z2z3
			z = Sse.Shuffle(y0z0y1z1, z2x3y3z3, 0b_11_00_11_01);
		}

		/// <summary>
		/// https://software.intel.com/en-us/articles/3d-vector-normalization-using-256-bit-intel-advanced-vector-extensions-intel-avx
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe void Store4xyz(float* dst, in Vector128<float> x, in Vector128<float> y,
			in Vector128<float> z)
		{
			//__m128 x0x2y0y2 = _mm_shuffle_ps(x, y, _MM_SHUFFLE(2, 0, 2, 0));
			Vector128<float> x0x2y0y2 = Sse.Shuffle(x, y, 0b_10_00_10_00);
			//__m128 y1y3z1z3 = _mm_shuffle_ps(y, z, _MM_SHUFFLE(3, 1, 3, 1));
			Vector128<float> y1y3z1z3 = Sse.Shuffle(y, z, 0b_11_01_11_01);
			//__m128 z0z2x1x3 = _mm_shuffle_ps(z, x, _MM_SHUFFLE(3, 1, 2, 0));
			Vector128<float> z0z2x1x3 = Sse.Shuffle(z, x, 0b_11_01_10_00);

			//__m128 rx0y0z0x1 = _mm_shuffle_ps(x0x2y0y2, z0z2x1x3, _MM_SHUFFLE(2, 0, 2, 0));
			Vector128<float> x0y0z0x1 = Sse.Shuffle(x0x2y0y2, z0z2x1x3, 0b_10_00_10_00);
			//__m128 ry1z1x2y2 = _mm_shuffle_ps(y1y3z1z3, x0x2y0y2, _MM_SHUFFLE(3, 1, 2, 0));
			Vector128<float> y1z1x2y2 = Sse.Shuffle(y1y3z1z3, x0x2y0y2, 0b_11_01_10_00);
			//__m128 rz2x3y3z3 = _mm_shuffle_ps(z0z2x1x3, y1y3z1z3, _MM_SHUFFLE(3, 1, 3, 1));
			Vector128<float> z2x3y3z3 = Sse.Shuffle(z0z2x1x3, y1y3z1z3, 0b_11_01_11_01);

			//_mm_store_ps(p + 0, rx0y0z0x1);
			Sse.Store(&dst[0], x0y0z0x1);
			//_mm_store_ps(p + 4, ry1z1x2y2);
			Sse.Store(&dst[4], y1z1x2y2);
			//_mm_store_ps(p + 8, rz2x3y3z3);
			Sse.Store(&dst[8], z2x3y3z3);
		}
	}
}
