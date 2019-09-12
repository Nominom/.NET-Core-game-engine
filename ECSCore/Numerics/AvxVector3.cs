using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace ECSCore.Numerics
{
	public ref struct AvxVector3 
	
	{
		public const int length = 8;
		public Vector256<float> x;
		public Vector256<float> y;
		public Vector256<float> z;

		public unsafe AvxVector3(ReadOnlySpan<Vector3> origin, int offset) {
			fixed (Vector3* ptr = origin) 
			{
				float* fp = (float*) &ptr[offset];
				Load8xyz(fp, out x, out y, out z);
			}
		}

		public unsafe void Store(Span<Vector3> destination, int offset) {
			fixed (Vector3* ptr = destination)
			{
				float* fp = (float*)&ptr[offset];
				Store8xyz(fp, x, y, z);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AvxVector3 Add(in AvxVector3 other) {

			return new AvxVector3()
			{
				x = Avx.Add(x, other.x),
				y = Avx.Add(y, other.y),
				z = Avx.Add(z, other.z)
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AvxVector3 Multiply(in AvxVector3 other)
		{
			return new AvxVector3()
			{
				x = Avx.Multiply(x, other.x),
				y = Avx.Multiply(y, other.y),
				z = Avx.Multiply(z, other.z)
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe AvxVector3 Multiply(float scalar) {

			Vector256<float> scalarV = Avx.BroadcastScalarToVector256(&scalar);
			return new AvxVector3()
			{
				x = Avx.Multiply(x, scalarV),
				y = Avx.Multiply(y, scalarV),
				z = Avx.Multiply(z, scalarV)
			};
		}









		/// <summary>
		/// https://software.intel.com/en-us/articles/3d-vector-normalization-using-256-bit-intel-advanced-vector-extensions-intel-avx
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe void Load8xyz(float* src, out Vector256<float> x, out Vector256<float> y, out Vector256<float> z, bool aligned = false)
		{
			//__m256 m03;
			//__m256 m14;
			//__m256 m25;
			//m03  = _mm256_castps128_ps256(m[0]); // load lower halves
			//m14  = _mm256_castps128_ps256(m[1]);
			//m25  = _mm256_castps128_ps256(m[2]);
			//m03  = _mm256_insertf128_ps(m03 ,m[3],1);  // load upper halves
			//m14  = _mm256_insertf128_ps(m14 ,m[4],1);
			//m25  = _mm256_insertf128_ps(m25 ,m[5],1);
			Vector256<float> m03 = Vector256.Create(Sse.LoadVector128(&src[0]), Sse.LoadVector128(&src[3 * 4]));
			Vector256<float> m14 = Vector256.Create(Sse.LoadVector128(&src[1 * 4]), Sse.LoadVector128(&src[4 * 4]));
			Vector256<float> m25 = Vector256.Create(Sse.LoadVector128(&src[2 * 4]), Sse.LoadVector128(&src[5 * 4]));

			//__m256 xy = _mm256_shuffle_ps(m14, m25, _MM_SHUFFLE( 2,1,3,2)); // upper x's and y's 
			Vector256<float> xy = Avx.Shuffle(m14, m25, 0b_10_01_11_10);
			//__m256 yz = _mm256_shuffle_ps(m03, m14, _MM_SHUFFLE( 1,0,2,1)); // lower y's and z's
			Vector256<float> yz = Avx.Shuffle(m03, m14, 0b_01_00_10_01);
			//__m256 x  = _mm256_shuffle_ps(m03, xy , _MM_SHUFFLE( 2,0,3,0)); 
			x = Avx.Shuffle(m03, xy, 0b_10_00_11_00);
			//__m256 y  = _mm256_shuffle_ps(yz , xy , _MM_SHUFFLE( 3,1,2,0)); 
			y = Avx.Shuffle(yz, xy, 0b_11_01_10_00);
			//__m256 z  = _mm256_shuffle_ps(yz , m25, _MM_SHUFFLE( 3,0,3,1));
			z = Avx.Shuffle(yz, m25, 0b_11_00_11_01);
		}

		/// <summary>
		/// https://software.intel.com/en-us/articles/3d-vector-normalization-using-256-bit-intel-advanced-vector-extensions-intel-avx
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe void Store8xyz(float* dst, in Vector256<float> x, in Vector256<float> y, in Vector256<float> z, bool aligned = false)
		{
			//  __m256 rxy = _mm256_shuffle_ps(x,y, _MM_SHUFFLE(2,0,2,0)); 
			Vector256<float> rxy = Avx.Shuffle(x, y, 0b_10_00_10_00);
			//  __m256 ryz = _mm256_shuffle_ps(y,z, _MM_SHUFFLE(3,1,3,1)); 
			Vector256<float> ryz = Avx.Shuffle(y, z, 0b_11_01_11_01);
			//  __m256 rzx = _mm256_shuffle_ps(z,x, _MM_SHUFFLE(3,1,2,0)); 
			Vector256<float> rzx = Avx.Shuffle(z, x, 0b_11_01_10_00);
			//  __m256 r03 = _mm256_shuffle_ps(rxy, rzx, _MM_SHUFFLE(2,0,2,0));  
			Vector256<float> r03 = Avx.Shuffle(rxy, rzx, 0b_10_00_10_00);
			//  __m256 r14 = _mm256_shuffle_ps(ryz, rxy, _MM_SHUFFLE(3,1,2,0)); 
			Vector256<float> r14 = Avx.Shuffle(ryz, rxy, 0b_11_01_10_00);
			//  __m256 r25 = _mm256_shuffle_ps(rzx, ryz, _MM_SHUFFLE(3,1,3,1)); 
			Vector256<float> r25 = Avx.Shuffle(rzx, ryz, 0b_11_01_11_01);

			//  m[0] = _mm256_castps256_ps128( r03 );
			Sse.Store(&dst[0], Vector256.GetLower(r03));
			//  m[1] = _mm256_castps256_ps128( r14 );
			Sse.Store(&dst[1 * 4], Vector256.GetLower(r14));
			//  m[2] = _mm256_castps256_ps128( r25 );
			Sse.Store(&dst[2 * 4], Vector256.GetLower(r25));
			//  m[3] = _mm256_extractf128_ps( r03 ,1);
			Sse.Store(&dst[3 * 4], Vector256.GetUpper(r03));
			//  m[4] = _mm256_extractf128_ps( r14 ,1);
			Sse.Store(&dst[4 * 4], Vector256.GetUpper(r14));
			//  m[5] = _mm256_extractf128_ps( r25 ,1);
			Sse.Store(&dst[5 * 4], Vector256.GetUpper(r25));
		}
	}
}
