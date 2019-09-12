using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace ECSCore.Numerics
{
	public static class Vector3Sse
	{
		/// <summary>
		/// https://software.intel.com/en-us/articles/3d-vector-normalization-using-256-bit-intel-advanced-vector-extensions-intel-avx
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe void Load4xyz(float* src, out Vector128<float> x, out Vector128<float> y,
			out Vector128<float> z, bool aligned = false) {
			Vector128<float> x0y0z0x1;
			Vector128<float> y1z1x2y2;
			Vector128<float> z2x3y3z3;
			if (aligned) {
				//__m128 x0y0z0x1 = _mm_load_ps(p + 0);
				x0y0z0x1 = Sse.LoadAlignedVector128(&src[0]);
				//__m128 y1z1x2y2 = _mm_load_ps(p + 4);
				y1z1x2y2 = Sse.LoadAlignedVector128(&src[4]);
				//__m128 z2x3y3z3 = _mm_load_ps(p + 8);
				z2x3y3z3 = Sse.LoadAlignedVector128(&src[8]);
			}
			else {
				//__m128 x0y0z0x1 = _mm_load_ps(p + 0);
				x0y0z0x1 = Sse.LoadVector128(&src[0]);
				//__m128 y1z1x2y2 = _mm_load_ps(p + 4);
				y1z1x2y2 = Sse.LoadVector128(&src[4]);
				//__m128 z2x3y3z3 = _mm_load_ps(p + 8);
				z2x3y3z3 = Sse.LoadVector128(&src[8]);
			}
			
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
			in Vector128<float> z, bool aligned = false)
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

			if (aligned) {
				//_mm_store_ps(p + 0, rx0y0z0x1);
				Sse.StoreAligned(&dst[0], x0y0z0x1);
				//_mm_store_ps(p + 4, ry1z1x2y2);
				Sse.StoreAligned(&dst[4], y1z1x2y2);
				//_mm_store_ps(p + 8, rz2x3y3z3);
				Sse.StoreAligned(&dst[8], z2x3y3z3);
			}
			else {
				//_mm_store_ps(p + 0, rx0y0z0x1);
				Sse.Store(&dst[0], x0y0z0x1);
				//_mm_store_ps(p + 4, ry1z1x2y2);
				Sse.Store(&dst[4], y1z1x2y2);
				//_mm_store_ps(p + 8, rz2x3y3z3);
				Sse.Store(&dst[8], z2x3y3z3);
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

				int vectorCount = Vector128<float>.Count;
				int length = left.Length * 3;
				int leftovers = length % vectorCount;

				int i = 0;

				if (aligned)
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Sse.LoadAlignedVector128(&lfP[i]);
						var r = Sse.LoadAlignedVector128(&rfP[i]);
						Sse.StoreAligned(&dst[i], Sse.Add(l, r));
					}
				}
				else
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Sse.LoadVector128(&lfP[i]);
						var r = Sse.LoadVector128(&rfP[i]);
						Sse.Store(&dst[i], Sse.Add(l, r));
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

				int vectorCount = Vector128<float>.Count;
				int length = left.Length * 3;
				int leftovers = length % vectorCount;

				int i = 0;

				if (aligned)
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Sse.LoadAlignedVector128(&lfP[i]);
						var r = Sse.LoadAlignedVector128(&rfP[i]);
						Sse.StoreAligned(&dst[i], Sse.Subtract(l, r));
					}
				}
				else
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Sse.LoadVector128(&lfP[i]);
						var r = Sse.LoadVector128(&rfP[i]);
						Sse.Store(&dst[i], Sse.Subtract(l, r));
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
		public static unsafe void Multiply(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<Vector3> result)
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

				int vectorCount = Vector128<float>.Count;
				int length = left.Length * 3;
				int leftovers = length % vectorCount;

				int i = 0;

				if (aligned)
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Sse.LoadAlignedVector128(&lfP[i]);
						var r = Sse.LoadAlignedVector128(&rfP[i]);
						Sse.StoreAligned(&dst[i], Sse.Multiply(l, r));
					}
				}
				else
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Sse.LoadVector128(&lfP[i]);
						var r = Sse.LoadVector128(&rfP[i]);
						Sse.Store(&dst[i], Sse.Multiply(l, r));
					}
				}


				//Process the rest sequentally
				for (; i < length; i++)
				{
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

				int inc = 12; //sizeof 4 vector3's
				int length = left.Length * 3;
				int leftovers = left.Length % 4;

				int i = 0;
				int j = 0;
				for (i = 0; i < length - leftovers; i += inc, j += 4)
				{
					Vector128<float> x, y, z;
					Load4xyz(&lfP[i], out x, out y, out z, aligned);

					var r = Sse.LoadVector128(&rfP[j]);

					x = Sse.Multiply(x, r);
					y = Sse.Multiply(y, r);
					z = Sse.Multiply(z, r);

					Store4xyz(&dst[i], x, y, z);
				}


				//Process the rest sequentally
				for (; i < length; i+=3, j++) {
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

				int vectorCount = Vector128<float>.Count;
				int length = left.Length * 3;
				int leftovers = length % vectorCount;

				int i = 0;

				Vector128<float> r = Vector128.Create(right, right, right, right);

				if (aligned)
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Sse.LoadAlignedVector128(&lfP[i]);
						Sse.StoreAligned(&dst[i], Sse.Multiply(l, r));
					}
				}
				else
				{
					for (i = 0; i < length - leftovers; i += vectorCount)
					{
						var l = Sse.LoadVector128(&lfP[i]);
						Sse.Store(&dst[i], Sse.Multiply(l, r));
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

				int inc = 12; //sizeof 4 vector3's
				int length = source.Length * 3;
				int leftovers = source.Length % 4;

				int i = 0;

				for (i = 0; i < length - (leftovers * 3); i += inc) {
					Vector128<float> x, y, z;
					Load4xyz(&src[i], out x, out y, out z);

					Vector128<float> sqX = Sse.Multiply(x, x);
					Vector128<float> sqY = Sse.Multiply(y, y);
					Vector128<float> sqZ = Sse.Multiply(z, z);

					Vector128<float> len = Sse.Add(sqX, Sse.Add(sqY, sqZ));
					len = Sse.Sqrt(len);

					var zero = Vector128<float>.Zero;

					Vector128<float> zeroMask = Sse.CompareNotEqual(len, zero);

					x = Sse.Divide(x, len);
					y = Sse.Divide(y, len);
					z = Sse.Divide(z, len);

					//prevent NaN by zeroing out any where len=0
					x = Sse.And(x, zeroMask);
					y = Sse.And(y, zeroMask);
					z = Sse.And(z, zeroMask);

					Store4xyz(&dst[i], x, y, z);

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

				int inc = 12; //sizeof 4 vector3's
				int length = left.Length * 3;
				int leftovers = left.Length % 4;

				int i = 0;
				for (i = 0; i < length - (leftovers * 3); i += inc) {
					Vector128<float> lx, ly, lz;
					Vector128<float> rx, ry, rz;
					Load4xyz(&lfP[i], out lx, out ly, out lz);
					Load4xyz(&rfP[i], out rx, out ry, out rz);

					var x = Sse.Multiply(lx, rx);
					var y = Sse.Multiply(ly, ry);
					var z = Sse.Multiply(lz, rz);

					var dot = Sse.Add(x, Sse.Add(y, z));

					Sse.Store(&dst[i / 3], dot); //destination is 3 times smaller than sources
				}

				for (; i < length; i += 3) {
					var lx = lfP[i];
					var ly = lfP[i+1];
					var lz = lfP[i+2];

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

				int inc = 12; //sizeof 4 vector3's
				int length = left.Length * 3;
				int leftovers = left.Length % 4;

				int i = 0;
				for (i = 0; i < length - (leftovers * 3); i += inc)
				{
					Vector128<float> lx, ly, lz;
					Vector128<float> rx, ry, rz;
					Load4xyz(&lfP[i], out lx, out ly, out lz);
					Load4xyz(&rfP[i], out rx, out ry, out rz);

					//return new Vector3(
					//	this.y * other.z - this.z * other.y,
					//	this.z * other.x - this.x * other.z,
					//	this.x * other.y - this.y * other.x
					//);

					Vector128<float> lyrz = Sse.Multiply(ly, rz);
					Vector128<float> lzry = Sse.Multiply(lz, ry);
					Vector128<float> x = Sse.Subtract(lyrz, lzry);

					Vector128<float> lzrx = Sse.Multiply(lz, rx);
					Vector128<float> lxrz = Sse.Multiply(lx, rz);
					Vector128<float> y = Sse.Subtract(lzrx, lxrz);

					Vector128<float> lxry = Sse.Multiply(lx, ry);
					Vector128<float> lyrx = Sse.Multiply(ly, rx);
					Vector128<float> z = Sse.Subtract(lxry, lyrx);

					Store4xyz(&dst[i], x, y, z);
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

				int inc = 12; //sizeof 4 vector3's
				int length = dirs.Length * 3;
				int leftovers = dirs.Length % 4;

				Vector128<float> two = Vector128.Create(2f, 2f, 2f, 2f);

				int i = 0;
				for (i = 0; i < length - (leftovers * 3); i += inc)
				{
					Vector128<float> lx, ly, lz; //lefts
					Vector128<float> rx, ry, rz; //rights
					Load4xyz(&lfP[i], out lx, out ly, out lz);
					Load4xyz(&rfP[i], out rx, out ry, out rz);

					//float dot = Dot(normal);
					//return this - (normal * dot * 2);

					var x = Sse.Multiply(lx, rx);
					var y = Sse.Multiply(ly, ry);
					var z = Sse.Multiply(lz, rz);

					var dot = Sse.Add(x, Sse.Add(y, z));

					rx = Sse.Multiply(rx, dot);
					ry = Sse.Multiply(ry, dot);
					rz = Sse.Multiply(rz, dot);

					rx = Sse.Multiply(rx, two);
					ry = Sse.Multiply(ry, two);
					rz = Sse.Multiply(rz, two);

					x = Sse.Subtract(lx, rx);
					y = Sse.Subtract(ly, ry);
					z = Sse.Subtract(lz, rz);

					Store4xyz(&dst[i], x, y, z);
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
