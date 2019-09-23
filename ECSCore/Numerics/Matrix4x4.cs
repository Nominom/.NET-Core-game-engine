using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Core.ECS.Numerics
{
	[StructLayout(LayoutKind.Explicit, Size = 64)]
	public struct Matrix4x4 {
		[FieldOffset(0)]
		public float r1c1;
		[FieldOffset(4)]
		public float r1c2;
		[FieldOffset(8)]
		public float r1c3;
		[FieldOffset(12)]
		public float r1c4;
		[FieldOffset(16)]
		public float r2c1;
		[FieldOffset(20)]
		public float r2c2;
		[FieldOffset(24)]
		public float r2c3;
		[FieldOffset(28)]
		public float r2c4;
		[FieldOffset(32)]
		public float r3c1;
		[FieldOffset(36)]
		public float r3c2;
		[FieldOffset(40)]
		public float r3c3;
		[FieldOffset(44)]
		public float r3c4;
		[FieldOffset(48)]
		public float r4c1;
		[FieldOffset(52)]
		public float r4c2;
		[FieldOffset(56)]
		public float r4c3;
		[FieldOffset(60)]
		public float r4c4;

		public static Matrix4x4 Identity { get; } = 
			new Matrix4x4(
				1, 0, 0, 0, 
				0, 1, 0, 0, 
				0, 0, 1, 0, 
				0, 0, 0, 1);

		public Matrix4x4(float r1C1, float r1C2, float r1C3, float r1C4, float r2C1, float r2C2, float r2C3, float r2C4, float r3C1, float r3C2, float r3C3, float r3C4, float r4C1, float r4C2, float r4C3, float r4C4) {
			r1c1 = r1C1;
			r1c2 = r1C2;
			r1c3 = r1C3;
			r1c4 = r1C4;
			r2c1 = r2C1;
			r2c2 = r2C2;
			r2c3 = r2C3;
			r2c4 = r2C4;
			r3c1 = r3C1;
			r3c2 = r3C2;
			r3c3 = r3C3;
			r3c4 = r3C4;
			r4c1 = r4C1;
			r4c2 = r4C2;
			r4c3 = r4C3;
			r4c4 = r4C4;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static Matrix4x4 Multiply(Matrix4x4 left, Matrix4x4 right) {
			Matrix4x4 result = new Matrix4x4
			{
				r1c1 = left.r1c1 * right.r1c1 + left.r2c1 * right.r1c2 + left.r3c1 * right.r1c3 + left.r4c1 * right.r1c4,
				r1c2 = left.r1c2 * right.r1c1 + left.r2c2 * right.r1c2 + left.r3c2 * right.r1c3 + left.r4c2 * right.r1c4,
				r1c3 = left.r1c3 * right.r1c1 + left.r2c3 * right.r1c2 + left.r3c3 * right.r1c3 + left.r4c3 * right.r1c4,
				r1c4 = left.r1c4 * right.r1c1 + left.r2c4 * right.r1c2 + left.r3c4 * right.r1c3 + left.r4c4 * right.r1c4,

				r2c1 = left.r1c1 * right.r2c1 + left.r2c1 * right.r2c2 + left.r3c1 * right.r2c3 + left.r4c1 * right.r2c4,
				r2c2 = left.r1c2 * right.r2c1 + left.r2c2 * right.r2c2 + left.r3c2 * right.r2c3 + left.r4c2 * right.r2c4,
				r2c3 = left.r1c3 * right.r2c1 + left.r2c3 * right.r2c2 + left.r3c3 * right.r2c3 + left.r4c3 * right.r2c4,
				r2c4 = left.r1c4 * right.r2c1 + left.r2c4 * right.r2c2 + left.r3c4 * right.r2c3 + left.r4c4 * right.r2c4,

				r3c1 = left.r1c1 * right.r3c1 + left.r2c1 * right.r3c2 + left.r3c1 * right.r3c3 + left.r4c1 * right.r3c4,
				r3c2 = left.r1c2 * right.r3c1 + left.r2c2 * right.r3c2 + left.r3c2 * right.r3c3 + left.r4c2 * right.r3c4,
				r3c3 = left.r1c3 * right.r3c1 + left.r2c3 * right.r3c2 + left.r3c3 * right.r3c3 + left.r4c3 * right.r3c4,
				r3c4 = left.r1c4 * right.r3c1 + left.r2c4 * right.r3c2 + left.r3c4 * right.r3c3 + left.r4c4 * right.r3c4,

				r4c1 = left.r1c1 * right.r4c1 + left.r2c1 * right.r4c2 + left.r3c1 * right.r4c3 + left.r4c1 * right.r4c4,
				r4c2 = left.r1c2 * right.r4c1 + left.r2c2 * right.r4c2 + left.r3c2 * right.r4c3 + left.r4c2 * right.r4c4,
				r4c3 = left.r1c3 * right.r4c1 + left.r2c3 * right.r4c2 + left.r3c3 * right.r4c3 + left.r4c3 * right.r4c4,
				r4c4 = left.r1c4 * right.r4c1 + left.r2c4 * right.r4c2 + left.r3c4 * right.r4c3 + left.r4c4 * right.r4c4
			};

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static Matrix4x4 MultiplyRef(in Matrix4x4 left, in Matrix4x4 right)
		{
			Matrix4x4 result = new Matrix4x4
			{
				r1c1 = left.r1c1 * right.r1c1 + left.r2c1 * right.r1c2 + left.r3c1 * right.r1c3 + left.r4c1 * right.r1c4,
				r1c2 = left.r1c2 * right.r1c1 + left.r2c2 * right.r1c2 + left.r3c2 * right.r1c3 + left.r4c2 * right.r1c4,
				r1c3 = left.r1c3 * right.r1c1 + left.r2c3 * right.r1c2 + left.r3c3 * right.r1c3 + left.r4c3 * right.r1c4,
				r1c4 = left.r1c4 * right.r1c1 + left.r2c4 * right.r1c2 + left.r3c4 * right.r1c3 + left.r4c4 * right.r1c4,

				r2c1 = left.r1c1 * right.r2c1 + left.r2c1 * right.r2c2 + left.r3c1 * right.r2c3 + left.r4c1 * right.r2c4,
				r2c2 = left.r1c2 * right.r2c1 + left.r2c2 * right.r2c2 + left.r3c2 * right.r2c3 + left.r4c2 * right.r2c4,
				r2c3 = left.r1c3 * right.r2c1 + left.r2c3 * right.r2c2 + left.r3c3 * right.r2c3 + left.r4c3 * right.r2c4,
				r2c4 = left.r1c4 * right.r2c1 + left.r2c4 * right.r2c2 + left.r3c4 * right.r2c3 + left.r4c4 * right.r2c4,

				r3c1 = left.r1c1 * right.r3c1 + left.r2c1 * right.r3c2 + left.r3c1 * right.r3c3 + left.r4c1 * right.r3c4,
				r3c2 = left.r1c2 * right.r3c1 + left.r2c2 * right.r3c2 + left.r3c2 * right.r3c3 + left.r4c2 * right.r3c4,
				r3c3 = left.r1c3 * right.r3c1 + left.r2c3 * right.r3c2 + left.r3c3 * right.r3c3 + left.r4c3 * right.r3c4,
				r3c4 = left.r1c4 * right.r3c1 + left.r2c4 * right.r3c2 + left.r3c4 * right.r3c3 + left.r4c4 * right.r3c4,

				r4c1 = left.r1c1 * right.r4c1 + left.r2c1 * right.r4c2 + left.r3c1 * right.r4c3 + left.r4c1 * right.r4c4,
				r4c2 = left.r1c2 * right.r4c1 + left.r2c2 * right.r4c2 + left.r3c2 * right.r4c3 + left.r4c2 * right.r4c4,
				r4c3 = left.r1c3 * right.r4c1 + left.r2c3 * right.r4c2 + left.r3c3 * right.r4c3 + left.r4c3 * right.r4c4,
				r4c4 = left.r1c4 * right.r4c1 + left.r2c4 * right.r4c2 + left.r3c4 * right.r4c3 + left.r4c4 * right.r4c4
			};

			return result;
		}


		public static Matrix4x4 Transpose(Matrix4x4 src) {
			return new Matrix4x4(
				src.r1c1, src.r2c1, src.r3c1, src.r4c1,
				src.r1c2, src.r2c2, src.r3c2, src.r4c2,
				src.r1c3, src.r2c3, src.r3c3, src.r4c3,
				src.r1c4, src.r2c4, src.r3c4, src.r4c4);
		}

		public static Matrix4x4 TransposeRef(in Matrix4x4 src)
		{
			return new Matrix4x4(
				src.r1c1, src.r2c1, src.r3c1, src.r4c1,
				src.r1c2, src.r2c2, src.r3c2, src.r4c2,
				src.r1c3, src.r2c3, src.r3c3, src.r4c3,
				src.r1c4, src.r2c4, src.r3c4, src.r4c4);
		}
	}
}
