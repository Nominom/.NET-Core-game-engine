using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace Core.ECS.Numerics
{
	public static class Matrix4x4Sse
	{

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static void Transpose(in Vector128<float>row0, in Vector128<float> row1, in Vector128<float> row2, in Vector128<float> row3
		, out Vector128<float> outRow0, out Vector128<float> outRow1, out Vector128<float> outRow2, out Vector128<float> outRow3) {
			Vector128<float> tmp3, tmp2, tmp1, tmp0;
			//tmp0:= _mm_unpacklo_ps(row0, row1);
			tmp0 = Sse.UnpackLow(row0, row1);
			//tmp2:= _mm_unpacklo_ps(row2, row3);
			tmp2 = Sse.UnpackLow(row2, row3);
			//tmp1:= _mm_unpackhi_ps(row0, row1);
			tmp1 = Sse.UnpackHigh(row0, row1);
			//tmp3:= _mm_unpackhi_ps(row2, row3);
			tmp3 = Sse.UnpackHigh(row2, row3);
			//row0:= _mm_movelh_ps(tmp0, tmp2);
			outRow0 = Sse.MoveLowToHigh(tmp0, tmp2);
			//row1:= _mm_movehl_ps(tmp2, tmp0);
			outRow1 = Sse.MoveHighToLow(tmp2, tmp0);
			//row2:= _mm_movelh_ps(tmp1, tmp3);
			outRow2 = Sse.MoveLowToHigh(tmp1, tmp3);
			//row3:= _mm_movehl_ps(tmp3, tmp1);
			outRow3 = Sse.MoveHighToLow(tmp3, tmp1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static void TransposeRef(ref Vector128<float> row0, ref Vector128<float> row1, ref Vector128<float> row2, ref Vector128<float> row3)
		{
			Vector128<float> tmp3, tmp2, tmp1, tmp0;
			//tmp0:= _mm_unpacklo_ps(row0, row1);
			tmp0 = Sse.UnpackLow(row0, row1);
			//tmp2:= _mm_unpacklo_ps(row2, row3);
			tmp2 = Sse.UnpackLow(row2, row3);
			//tmp1:= _mm_unpackhi_ps(row0, row1);
			tmp1 = Sse.UnpackHigh(row0, row1);
			//tmp3:= _mm_unpackhi_ps(row2, row3);
			tmp3 = Sse.UnpackHigh(row2, row3);
			//row0:= _mm_movelh_ps(tmp0, tmp2);
			row0 = Sse.MoveLowToHigh(tmp0, tmp2);
			//row1:= _mm_movehl_ps(tmp2, tmp0);
			row1 = Sse.MoveHighToLow(tmp2, tmp0);
			//row2:= _mm_movelh_ps(tmp1, tmp3);
			row2 = Sse.MoveLowToHigh(tmp1, tmp3);
			//row3:= _mm_movehl_ps(tmp3, tmp1);
			row3 = Sse.MoveHighToLow(tmp3, tmp1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe void TransposeStore( Vector128<float> row0,  Vector128<float> row1,  Vector128<float> row2,  Vector128<float> row3, float* dest)
		{
			Vector128<float> tmp2, tmp0;
			//tmp0:= _mm_unpacklo_ps(row0, row1);
			tmp0 = Sse.UnpackLow(row0, row1);
			//tmp2:= _mm_unpacklo_ps(row2, row3);
			tmp2 = Sse.UnpackLow(row2, row3);

			Sse.Store(&dest[0], Sse.MoveLowToHigh(tmp0, tmp2));
			Sse.Store(&dest[4], Sse.MoveHighToLow(tmp2, tmp0));

			//tmp1:= _mm_unpackhi_ps(row0, row1);
			tmp0 = Sse.UnpackHigh(row0, row1);
			//tmp3:= _mm_unpackhi_ps(row2, row3);
			tmp2 = Sse.UnpackHigh(row2, row3);

			Sse.Store(&dest[8], Sse.MoveLowToHigh(tmp0, tmp2));
			Sse.Store(&dest[12], Sse.MoveHighToLow(tmp2, tmp0));
		}

		/*
		void M4x4_SSE(float *A, float *B, float *C) {
		    __m128 row1 = _mm_load_ps(&B[0]);
		    __m128 row2 = _mm_load_ps(&B[4]);
		    __m128 row3 = _mm_load_ps(&B[8]);
		    __m128 row4 = _mm_load_ps(&B[12]);
		    for(int i=0; i<4; i++) {
		        __m128 brod1 = _mm_set1_ps(A[4*i + 0]);
		        __m128 brod2 = _mm_set1_ps(A[4*i + 1]);
		        __m128 brod3 = _mm_set1_ps(A[4*i + 2]);
		        __m128 brod4 = _mm_set1_ps(A[4*i + 3]);
		        __m128 row = _mm_add_ps(
		                    _mm_add_ps(
		                        _mm_mul_ps(brod1, row1),
		                        _mm_mul_ps(brod2, row2)),
		                    _mm_add_ps(
		                        _mm_mul_ps(brod3, row3),
		                        _mm_mul_ps(brod4, row4)));
		        _mm_store_ps(&C[4*i], row);
		    }
		}
		 */
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe void Mul4x4(float* A, float* B, float* C)
		{
			Vector128<float> row1 = Sse.LoadVector128(&B[0]);
			Vector128<float> row2 = Sse.LoadVector128(&B[4]);
			Vector128<float> row3 = Sse.LoadVector128(&B[8]);
			Vector128<float> row4 = Sse.LoadVector128(&B[12]);
			for (int i = 0; i < 4; i++)
			{
				Vector128<float> brod1 = Vector128.Create(A[4 * i + 0]);
				Vector128<float> brod2 = Vector128.Create(A[4 * i + 1]);
				Vector128<float> brod3 = Vector128.Create(A[4 * i + 2]);
				Vector128<float> brod4 = Vector128.Create(A[4 * i + 3]);
				Vector128<float> row = Sse.Add(
					Sse.Add(
						Sse.Multiply(brod1, row1),
						Sse.Multiply(brod2, row2)),
					Sse.Add(
						Sse.Multiply(brod3, row3),
						Sse.Multiply(brod4, row4)));
				Sse.Store(&C[4 * i], row);
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe Matrix4x4 Multiply(Matrix4x4 left, Matrix4x4 right) {
			Matrix4x4 result = new Matrix4x4();

			Matrix4x4* leftP = &left;
			Matrix4x4* rightP = &right;
			Matrix4x4* resP = &result;

			float* lfP = (float*) leftP;
			float* rfP = (float*) rightP;
			float* res = (float*) resP;

			Mul4x4(lfP, rfP,res);

			return result;
		}



		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe Matrix4x4 Transpose(Matrix4x4 source)
		{
			Matrix4x4 result = new Matrix4x4();

			Matrix4x4* srcP = &source;
			Matrix4x4* resP = &result;

			float* src = (float*)srcP;
			float* res = (float*)resP;

			Vector128<float> r1 = Sse.LoadVector128(&src[0]);
			Vector128<float> r2 = Sse.LoadVector128(&src[4]);
			Vector128<float> r3 = Sse.LoadVector128(&src[8]);
			Vector128<float> r4 = Sse.LoadVector128(&src[12]);

			Vector128<float> out1, out2, out3, out4;

			Transpose(r1, r2, r3, r4, out out1, out out2, out out3, out out4);

			Sse.Store(&res[0], out1);
			Sse.Store(&res[4], out2);
			Sse.Store(&res[8], out3);
			Sse.Store(&res[12], out4);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe Matrix4x4 TransposeRef(Matrix4x4 source)
		{
			Matrix4x4 result = new Matrix4x4();

			Matrix4x4* srcP = &source;
			Matrix4x4* resP = &result;

			float* src = (float*)srcP;
			float* res = (float*)resP;

			Vector128<float> r1 = Sse.LoadVector128(&src[0]);
			Vector128<float> r2 = Sse.LoadVector128(&src[4]);
			Vector128<float> r3 = Sse.LoadVector128(&src[8]);
			Vector128<float> r4 = Sse.LoadVector128(&src[12]);

			TransposeRef(ref r1, ref r2, ref r3, ref r4);

			Sse.Store(&res[0], r1);
			Sse.Store(&res[4], r2);
			Sse.Store(&res[8], r3);
			Sse.Store(&res[12], r4);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe Matrix4x4 TransposeStore(Matrix4x4 source)
		{
			Matrix4x4 result = new Matrix4x4();

			Matrix4x4* srcP = &source;
			Matrix4x4* resP = &result;

			float* src = (float*)srcP;
			float* res = (float*)resP;

			Vector128<float> r1 = Sse.LoadVector128(&src[0]);
			Vector128<float> r2 = Sse.LoadVector128(&src[1]);
			Vector128<float> r3 = Sse.LoadVector128(&src[2]);
			Vector128<float> r4 = Sse.LoadVector128(&src[3]);

			TransposeStore( r1,  r2,  r3,  r4, res);

			return result;
		}



	}
}
