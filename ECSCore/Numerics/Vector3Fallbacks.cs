using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ECSCore.Numerics
{
	public static class Vector3Fallbacks
	{

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void Add(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<Vector3> result)
		{
			
			if (left.Length != right.Length || left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			int length = left.Length;

			for(int i = 0; i < length; i++)
			{
				result[i] = left[i] + right[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void Subtract(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<Vector3> result)
		{

			if (left.Length != right.Length || left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			int length = left.Length;

			for (int i = 0; i < length; i++)
			{
				result[i] = left[i] - right[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void Multiply(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<Vector3> result)
		{

			if (left.Length != right.Length || left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			int length = left.Length;

			for (int i = 0; i < length; i++)
			{
				result[i] = left[i] * right[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void MultiplyAllScalar(ReadOnlySpan<Vector3> left, float right, Span<Vector3> result)
		{

			if (left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			int length = left.Length;

			for (int i = 0; i < length; i++)
			{
				result[i] = left[i] * right;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void MultiplyScalar(ReadOnlySpan<Vector3> left, ReadOnlySpan<float> right, Span<Vector3> result)
		{

			if (left.Length != right.Length || left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			int length = left.Length;

			for (int i = 0; i < length; i++)
			{
				result[i] = left[i] * right[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void Normalize(ReadOnlySpan<Vector3> src, Span<Vector3> dst)
		{

			if (src.Length != dst.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			int length = src.Length;

			for (int i = 0; i < length; i++) {
				dst[i] = src[i].Normalized();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void Dot(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<float> result)
		{

			if (left.Length != right.Length || left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			int length = left.Length;

			for (int i = 0; i < length; i++)
			{
				result[i] = left[i].Dot(right[i]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void Cross(ReadOnlySpan<Vector3> left, ReadOnlySpan<Vector3> right, Span<Vector3> result)
		{

			if (left.Length != right.Length || left.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			int length = left.Length;

			for (int i = 0; i < length; i++)
			{
				result[i] = left[i].Cross(right[i]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void Reflect(ReadOnlySpan<Vector3> dirs, ReadOnlySpan<Vector3> normals, Span<Vector3> result)
		{

			if (dirs.Length != normals.Length || dirs.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			int length = dirs.Length;

			for (int i = 0; i < length; i++)
			{
				result[i] = dirs[i].Reflect(normals[i]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void Length(ReadOnlySpan<Vector3> src, Span<float> result)
		{

			if (src.Length != result.Length)
			{
				throw new ArgumentException("All spans should be the same length");
			}

			int length = src.Length;

			for (int i = 0; i < length; i++) {
				result[i] = src[i].Length();
			}
		}
	}
}
