using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace ECSCore.Numerics
{
	public static class Vector3Avx
	{

		private static readonly int vectorCount = Vector256<float>.Count;


		/// <summary>
		/// https://software.intel.com/en-us/articles/3d-vector-normalization-using-256-bit-intel-advanced-vector-extensions-intel-avx
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe void Load8xyz(float* src, out Vector256<float> x, out Vector256<float> y, out Vector256<float> z, bool aligned = false) {
			//__m256 m03;
			//__m256 m14;
			//__m256 m25;
			//m03  = _mm256_castps128_ps256(m[0]); // load lower halves
			//m14  = _mm256_castps128_ps256(m[1]);
			//m25  = _mm256_castps128_ps256(m[2]);
			//m03  = _mm256_insertf128_ps(m03 ,m[3],1);  // load upper halves
			//m14  = _mm256_insertf128_ps(m14 ,m[4],1);
			//m25  = _mm256_insertf128_ps(m25 ,m[5],1);
			Vector256<float> m03;
			Vector256<float> m14;
			Vector256<float> m25;
			if (aligned) {
				m03 = Vector256.Create(Sse.LoadAlignedVector128(&src[0]), Sse.LoadAlignedVector128(&src[3 * 4]));
				m14 = Vector256.Create(Sse.LoadAlignedVector128(&src[1 * 4]), Sse.LoadAlignedVector128(&src[4 * 4]));
				m25 = Vector256.Create(Sse.LoadAlignedVector128(&src[2 * 4]), Sse.LoadAlignedVector128(&src[5 * 4]));
			}
			else {
				m03 = Vector256.Create(Sse.LoadVector128(&src[0]), Sse.LoadVector128(&src[3 * 4]));
				m14 = Vector256.Create(Sse.LoadVector128(&src[1 * 4]), Sse.LoadVector128(&src[4 * 4]));
				m25 = Vector256.Create(Sse.LoadVector128(&src[2 * 4]), Sse.LoadVector128(&src[5 * 4]));
			}


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

			if (aligned) {
				//  m[0] = _mm256_castps256_ps128( r03 );
				Sse.StoreAligned(&dst[0], Vector256.GetLower(r03));
				//  m[1] = _mm256_castps256_ps128( r14 );
				Sse.StoreAligned(&dst[1 * 4], Vector256.GetLower(r14));
				//  m[2] = _mm256_castps256_ps128( r25 );
				Sse.StoreAligned(&dst[2 * 4], Vector256.GetLower(r25));
				//  m[3] = _mm256_extractf128_ps( r03 ,1);
				Sse.StoreAligned(&dst[3 * 4], Vector256.GetUpper(r03));
				//  m[4] = _mm256_extractf128_ps( r14 ,1);
				Sse.StoreAligned(&dst[4 * 4], Vector256.GetUpper(r14));
				//  m[5] = _mm256_extractf128_ps( r25 ,1);
				Sse.StoreAligned(&dst[5 * 4], Vector256.GetUpper(r25));
			}
			else {
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


		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe void Add(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<Vector3> result)
		{
			if (left.Length != right.Length || left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			fixed (Vector3* lefts = left)
			fixed (Vector3* rights = right)
			fixed (Vector3* res = result)
			{
				float* lfP = (float*)lefts;
				float* rfP = (float*)rights;
				float* dst = (float*)res;


				bool aligned = ((int)lfP % 32 == 0 && (int)rfP % 32 == 0 && (int)dst % 32 == 0);

				int length = left.Length * 3;
				int vectorCount = Vector256<float>.Count;
				int leftovers = length % vectorCount;

				int i = 0;

				if (aligned)
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Avx.LoadAlignedVector256(&lfP[i]);
						var r = Avx.LoadAlignedVector256(&rfP[i]);
						Avx.StoreAligned(&dst[i], Avx.Add(l, r));
					}
				}
				else
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Avx.LoadVector256(&lfP[i]);
						var r = Avx.LoadVector256(&rfP[i]);
						Avx.Store(&dst[i], Avx.Add(l, r));
					}
				}


				//Process the rest sequentally
				for (; i < length; i++)
				{
					dst[i] = lfP[i] + rfP[i];
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe void Subtract(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<Vector3> result)
		{
			if (left.Length != right.Length || left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			fixed (Vector3* lefts = left)
			fixed (Vector3* rights = right)
			fixed (Vector3* res = result)
			{
				float* lfP = (float*)lefts;
				float* rfP = (float*)rights;
				float* dst = (float*)res;


				bool aligned = ((int)lfP % 32 == 0 && (int)rfP % 32 == 0 && (int)dst % 32 == 0);

				int vectorCount = Vector256<float>.Count;
				int length = left.Length * 3;
				int leftovers = length % vectorCount;

				int i = 0;

				if (aligned)
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Avx.LoadAlignedVector256(&lfP[i]);
						var r = Avx.LoadAlignedVector256(&rfP[i]);
						Avx.StoreAligned(&dst[i], Avx.Subtract(l, r));
					}
				}
				else
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Avx.LoadVector256(&lfP[i]);
						var r = Avx.LoadVector256(&rfP[i]);
						Avx.Store(&dst[i], Avx.Subtract(l, r));
					}
				}


				//Process the rest sequentally
				for (; i < length; i++)
				{
					dst[i] = lfP[i] - rfP[i];
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe void Multiply(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right,
			Span<Vector3> result) {
			if (left.Length != right.Length || left.Length != result.Length) {
				throw new ArgumentException("All spans should be the same length");
			}

			fixed (Vector3* lefts = left)
			fixed (Vector3* rights = right)
			fixed (Vector3* res = result) {
				float* lfP = (float*) lefts;
				float* rfP = (float*) rights;
				float* dst = (float*) res;


				bool aligned = ((int) lfP % 32 == 0 && (int) rfP % 32 == 0 && (int) dst % 32 == 0);

				int vectorCount = Vector256<float>.Count;
				int length = left.Length * 3;
				int leftovers = length % vectorCount;

				int i = 0;

				if (aligned) {
					for (i = 0; i < length - leftovers; i += vectorCount) {
						var l = Avx.LoadAlignedVector256(&lfP[i]);
						var r = Avx.LoadAlignedVector256(&rfP[i]);
						Avx.StoreAligned(&dst[i], Avx.Multiply(l, r));
					}
				}
				else {
					for (i = 0; i < length - leftovers; i += vectorCount) {
						var l = Avx.LoadVector256(&lfP[i]);
						var r = Avx.LoadVector256(&rfP[i]);
						Avx.Store(&dst[i], Avx.Multiply(l, r));
					}
				}


				//Process the rest sequentally
				for (; i < length; i++) {
					dst[i] = lfP[i] * rfP[i];
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe void MultiplyScalars(ReadOnlySpan<Vector3> left, ReadOnlySpan<float> right, Span<Vector3> result)
		{
			if (left.Length != right.Length || left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			fixed (Vector3* lefts = left)
			fixed (float* rfP = right)
			fixed (Vector3* res = result)
			{
				float* lfP = (float*)lefts;
				float* dst = (float*)res;


				bool aligned = ((int)lfP % 32 == 0 && (int)rfP % 32 == 0 && (int)dst % 32 == 0);

				int inc = 24; //sizeof 8 vector3's
				int length = left.Length * 3;
				int leftovers = left.Length % 8;

				int i = 0;
				int j = 0;
				for (i = 0; i < length - (leftovers * 3); i += inc, j += 8)
				{
					Vector256<float> x, y, z;
					Load8xyz(&lfP[i], out x, out y, out z);

					var r = Avx.LoadVector256(&rfP[j]);

					x = Avx.Multiply(x, r);
					y = Avx.Multiply(y, r);
					z = Avx.Multiply(z, r);

					Store8xyz(&dst[i], x, y, z);
				}


				//Process the rest sequentally
				for (; i < length; i += 3, j++)
				{
					float scalar = rfP[j];

					dst[i + 0] = lfP[i + 0] * scalar;
					dst[i + 1] = lfP[i + 1] * scalar;
					dst[i + 2] = lfP[i + 2] * scalar;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe void MultiplyScalar(ReadOnlySpan<Vector3> left, float right, Span<Vector3> result)
		{
			if (left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			fixed (Vector3* lefts = left)
			fixed (Vector3* res = result)
			{
				float* lfP = (float*)lefts;
				float* dst = (float*)res;


				bool aligned = ((int)lfP % 32 == 0 && (int)dst % 32 == 0);

				int vectorCount = Vector256<float>.Count;
				int length = left.Length * 3;
				int leftovers = length % vectorCount;

				int i = 0;

				Vector256<float> r = Avx.BroadcastScalarToVector256(&right);

				if (aligned)
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Avx.LoadAlignedVector256(&lfP[i]);
						Avx.StoreAligned(&dst[i], Avx.Multiply(l, r));
					}
				}
				else
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Avx.LoadVector256(&lfP[i]);
						Avx.Store(&dst[i], Avx.Multiply(l, r));
					}
				}


				//Process the rest sequentally
				for (; i < length; i++)
				{
					dst[i] = lfP[i] * right;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe void Normalize(ReadOnlySpan<Vector3> source, Span<Vector3> destination)
		{
			if (source.Length != destination.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			fixed (Vector3* srcP = source)
			fixed (Vector3* dstP = destination)
			{
				float* src = (float*)srcP;
				float* dst = (float*)dstP;

				int inc = 24; //sizeof 8 vector3's
				int length = source.Length * 3;
				int leftovers = source.Length % 8;

				int i = 0;

				for (i = 0; i < length - (leftovers * 3); i += inc) {

					Vector256<float> x, y, z;
					Load8xyz(&src[i], out x, out y, out z);


					Vector256<float> sqX = Avx.Multiply(x, x);
					Vector256<float> sqY = Avx.Multiply(y, y);
					Vector256<float> sqZ = Avx.Multiply(z, z);

					Vector256<float> len = Avx.Add(sqX, Avx.Add(sqY, sqZ));

					len = Avx.Sqrt(len);

					x = Avx.Divide(x, len);
					y = Avx.Divide(y, len);
					z = Avx.Divide(z, len);

					Vector128<float> zeroMaskL = Sse.CompareNotEqual(len.GetLower(), Vector128<float>.Zero);
					Vector128<float> zeroMaskU = Sse.CompareNotEqual(len.GetUpper(), Vector128<float>.Zero);
					Vector256<float> zeroMask = Vector256.Create(zeroMaskL, zeroMaskU);

					//prevent NaN by zeroing out any where len=0
					x = Avx.And(x, zeroMask);
					y = Avx.And(y, zeroMask);
					z = Avx.And(z, zeroMask);

					Store8xyz(&dst[i], x, y, z);
				}

				for (; i < length; i += 3)
				{
					float x = src[i];
					float y = src[i + 1];
					float z = src[i + 2];


					float lastLength = MathF.Sqrt(x * x + y * y + z * z);

					if (lastLength == 0)
					{
						dst[i] = 0;
						dst[i + 1] = 0;
						dst[i + 2] = 0;
					}
					else
					{
						dst[i] = x / lastLength;
						dst[i + 1] = y / lastLength;
						dst[i + 2] = z / lastLength;
					}
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe void NormalizeAvx2(ReadOnlySpan<Vector3> source, Span<Vector3> destination)
		{
			if (source.Length != destination.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			fixed (Vector3* srcP = source)
			fixed (Vector3* dstP = destination)
			{
				float* src = (float*)srcP;
				float* dst = (float*)dstP;

				int inc = 24; //sizeof 8 vector3's
				int length = source.Length * 3;
				int leftovers = source.Length % 8;

				int i = 0;

				for (i = 0; i < length - (leftovers * 3); i += inc)
				{
					Vector256<float> x, y, z;
					Load8xyz(&src[i], out x, out y, out z);

					Vector256<float> sqX = Avx.Multiply(x, x);
					Vector256<float> sqY = Avx.Multiply(y, y);
					Vector256<float> sqZ = Avx.Multiply(z, z);

					Vector256<float> len = Avx.Add(sqX, Avx.Add(sqY, sqZ));

					len = Avx.Sqrt(len);

					x = Avx.Divide(x, len);
					y = Avx.Divide(y, len);
					z = Avx.Divide(z, len);

					Vector256<float> zeroMask;

					var comp = Avx2.CompareEqual(len.AsInt32(), Vector256<int>.Zero);
					zeroMask = Avx2.CompareEqual(comp, Vector256<int>.Zero).AsSingle();

					//prevent NaN by zeroing out any where len=0
					x = Avx.And(x, zeroMask);
					y = Avx.And(y, zeroMask);
					z = Avx.And(z, zeroMask);

					Store8xyz(&dst[i], x, y, z);
				}

				for (; i < length; i += 3)
				{
					float x = src[i];
					float y = src[i + 1];
					float z = src[i + 2];


					float lastLength = MathF.Sqrt(x * x + y * y + z * z);

					if (lastLength == 0)
					{
						dst[i] = 0;
						dst[i + 1] = 0;
						dst[i + 2] = 0;
					}
					else
					{
						dst[i] = x / lastLength;
						dst[i + 1] = y / lastLength;
						dst[i + 2] = z / lastLength;
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe void Dot(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<float> result)
		{
			if (left.Length != right.Length || left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			fixed (Vector3* lefts = left)
			fixed (Vector3* rights = right)
			fixed (float* dst = result)
			{
				float* lfP = (float*)lefts;
				float* rfP = (float*)rights;

				int inc = 24; //sizeof 8 vector3's
				int length = left.Length * 3;
				int leftovers = left.Length % 8;

				int i = 0;
				for (i = 0; i < length - (leftovers * 3); i += inc)
				{
					Vector256<float> lx, ly, lz;
					Vector256<float> rx, ry, rz;
					Load8xyz(&lfP[i], out lx, out ly, out lz);
					Load8xyz(&rfP[i], out rx, out ry, out rz);

					var x = Avx.Multiply(lx, rx);
					var y = Avx.Multiply(ly, ry);
					var z = Avx.Multiply(lz, rz);

					var dot = Avx.Add(x, Avx.Add(y, z));

					Avx.Store(&dst[i / 3], dot); //destination is 3 times smaller than sources
				}

				for (; i < length; i += 3)
				{
					var lx = lfP[i];
					var ly = lfP[i + 1];
					var lz = lfP[i + 2];

					var rx = rfP[i];
					var ry = rfP[i + 1];
					var rz = rfP[i + 2];

					dst[i / 3] = (lx * rx) + (ly * ry) + (lz * rz);
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe void Cross(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<Vector3> result)
		{
			if (left.Length != right.Length || left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			fixed (Vector3* lefts = left)
			fixed (Vector3* rights = right)
			fixed (Vector3* res = result)
			{
				float* lfP = (float*)lefts;
				float* rfP = (float*)rights;
				float* dst = (float*)res;

				int inc = 24; //sizeof 8 vector3's
				int length = left.Length * 3;
				int leftovers = left.Length % 8;

				int i = 0;
				for (i = 0; i < length - (leftovers * 3); i += inc)
				{
					Vector256<float> lx, ly, lz;
					Vector256<float> rx, ry, rz;
					Load8xyz(&lfP[i], out lx, out ly, out lz);
					Load8xyz(&rfP[i], out rx, out ry, out rz);

					//return new Vector3(
					//	this.y * other.z - this.z * other.y,
					//	this.z * other.x - this.x * other.z,
					//	this.x * other.y - this.y * other.x
					//);

					Vector256<float> lyrz = Avx.Multiply(ly, rz);
					Vector256<float> lzry = Avx.Multiply(lz, ry);
					Vector256<float> x = Avx.Subtract(lyrz, lzry);

					Vector256<float> lzrx = Avx.Multiply(lz, rx);
					Vector256<float> lxrz = Avx.Multiply(lx, rz);
					Vector256<float> y = Avx.Subtract(lzrx, lxrz);

					Vector256<float> lxry = Avx.Multiply(lx, ry);
					Vector256<float> lyrx = Avx.Multiply(ly, rx);
					Vector256<float> z = Avx.Subtract(lxry, lyrx);

					Store8xyz(&dst[i], x, y, z);
				}

				for (; i < length; i += 3)
				{
					var lx = lfP[i];
					var ly = lfP[i + 1];
					var lz = lfP[i + 2];

					var rx = rfP[i];
					var ry = rfP[i + 1];
					var rz = rfP[i + 2];

					dst[i + 0] = ly * rz - lz * ry;
					dst[i + 1] = lz * rx - lx * rz;
					dst[i + 2] = lx * ry - ly * rx;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe void Reflect(ReadOnlySpan<Vector3> dirs, ReadOnlySpan<Vector3> normals, Span<Vector3> result)
		{
			if (dirs.Length != normals.Length || dirs.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			fixed (Vector3* lefts = dirs)
			fixed (Vector3* rights = normals)
			fixed (Vector3* res = result)
			{
				float* lfP = (float*)lefts;
				float* rfP = (float*)rights;
				float* dst = (float*)res;

				int inc = 24; //sizeof 4 vector3's
				int length = dirs.Length * 3;
				int leftovers = dirs.Length % 8;

				Vector256<float> two = Vector256.Create(2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f);

				int i = 0;
				for (i = 0; i < length - (leftovers * 3); i += inc)
				{
					Vector256<float> lx, ly, lz; //lefts
					Vector256<float> rx, ry, rz; //rights
					Load8xyz(&lfP[i], out lx, out ly, out lz);
					Load8xyz(&rfP[i], out rx, out ry, out rz);

					//float dot = Dot(normal);
					//return this - (normal * dot * 2);

					var x = Avx.Multiply(lx, rx);
					var y = Avx.Multiply(ly, ry);
					var z = Avx.Multiply(lz, rz);

					var dot = Avx.Add(x, Avx.Add(y, z));

					rx = Avx.Multiply(rx, dot);
					ry = Avx.Multiply(ry, dot);
					rz = Avx.Multiply(rz, dot);

					rx = Avx.Multiply(rx, two);
					ry = Avx.Multiply(ry, two);
					rz = Avx.Multiply(rz, two);

					x = Avx.Subtract(lx, rx);
					y = Avx.Subtract(ly, ry);
					z = Avx.Subtract(lz, rz);

					Store8xyz(&dst[i], x, y, z);
				}

				for (; i < length; i += 3)
				{
					var lx = lfP[i];
					var ly = lfP[i + 1];
					var lz = lfP[i + 2];

					var rx = rfP[i];
					var ry = rfP[i + 1];
					var rz = rfP[i + 2];

					var x = lx * rx;
					var y = ly * ry;
					var z = lz * rz;

					var dot = x + y + z;


					//return this - (normal * dot * 2);

					dst[i + 0] = lx - (rx * dot * 2);
					dst[i + 1] = ly - (ry * dot * 2);
					dst[i + 2] = lz - (rz * dot * 2);
				}
			}
		}
	}
}
