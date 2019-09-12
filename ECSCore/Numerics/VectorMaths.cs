using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace ECSCore.Numerics
{
	public static class VectorMaths {

		public static void Add(Span<Vector3> left, ReadOnlySpan<Vector3> right) => Add(left, right, left);

		public static void Add(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<Vector3> result)
		{
			if (Avx.IsSupported)
			{
				Vector3Avx.Add(left, right, result);
			}
			else if (Sse.IsSupported)
			{
				Vector3Sse.Add(left, right, result);
			}
			else
			{
				Vector3Fallbacks.Add(left, right, result);
			}
		}

		public static void Subtract(Span<Vector3> left, ReadOnlySpan<Vector3> right) => Subtract(left, right, left);

		public static void Subtract(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<Vector3> result)
		{
			if (Avx.IsSupported)
			{
				Vector3Avx.Subtract(left, right, result);
			}
			else if (Sse.IsSupported)
			{
				Vector3Sse.Subtract(left, right, result);
			}
			else
			{
				Vector3Fallbacks.Subtract(left, right, result);
			}
		}

		public static void Multiply(Span<Vector3> left, ReadOnlySpan<Vector3> right) => Multiply(left, right, left);

		public static void Multiply(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<Vector3> result)
		{
			if (Avx.IsSupported)
			{
				Vector3Avx.Multiply(left, right, result);
			}
			else if (Sse.IsSupported)
			{
				Vector3Sse.Multiply(left, right, result);
			}
			else
			{
				Vector3Fallbacks.Multiply(left, right, result);
			}
		}

		public static void Multiply(Span<Vector3> left, float right) => Multiply(left, right, left);

		public static void Multiply(ReadOnlySpan<Vector3> left, float right, Span<Vector3> result)
		{
			if (Avx.IsSupported)
			{
				Vector3Avx.MultiplyScalar(left, right, result);
			}
			else if (Sse.IsSupported)
			{
				Vector3Sse.MultiplyScalar(left, right, result);
			}
			else
			{
				Vector3Fallbacks.MultiplyAllScalar(left, right, result);
			}
		}


		public static void Dot(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<float> result)
		{
			if (Avx.IsSupported)
			{
				Vector3Avx.Dot(left, right, result);
			}
			else if (Sse.IsSupported)
			{
				Vector3Sse.Dot(left, right, result);
			}
			else
			{
				Vector3Fallbacks.Dot(left, right, result);
			}
		}

		public static void Normalize(Span<Vector3> vectors) => Normalize(vectors, vectors);

		public static void Normalize(ReadOnlySpan<Vector3> vectors, Span<Vector3> result)
		{
			if (Avx2.IsSupported) {
				Vector3Avx.NormalizeAvx2(vectors, result);
			}
			else if (Avx.IsSupported)
			{
				Vector3Avx.Normalize(vectors, result);
			}
			else if (Sse.IsSupported)
			{
				Vector3Sse.Normalize(vectors, result);
			}
			else
			{
				Vector3Fallbacks.Normalize(vectors, result);
			}
		}

		public static void Cross(Span<Vector3> left, ReadOnlySpan<Vector3> right) => Cross(left, right, left);

		public static void Cross(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<Vector3> result)
		{
			if (Avx.IsSupported)
			{
				Vector3Avx.Cross(left, right, result);
			}
			else if (Sse.IsSupported)
			{
				Vector3Sse.Cross(left, right, result);
			}
			else
			{
				Vector3Fallbacks.Cross(left, right, result);
			}
		}

		public static void Reflect(Span<Vector3> vectors, ReadOnlySpan<Vector3> normals) => Reflect(vectors, normals, vectors);

		public static void Reflect(ReadOnlySpan<Vector3> vectors, ReadOnlySpan<Vector3> normals, Span<Vector3> result)
		{
			if (Avx.IsSupported)
			{
				Vector3Avx.Reflect(vectors, normals, result);
			}
			else if (Sse.IsSupported)
			{
				Vector3Sse.Reflect(vectors, normals, result);
			}
			else
			{
				Vector3Fallbacks.Reflect(vectors, normals, result);
			}
		}
	}
}
