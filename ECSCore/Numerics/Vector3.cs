using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace ECSCore.Numerics
{
	[StructLayout(LayoutKind.Explicit, Size = 12)]
	public struct Vector3 : IEquatable<Vector3>
	{
		public static readonly Vector3 zero = new Vector3(0, 0, 0);
		public static readonly Vector3 one = new Vector3(1, 1, 1);

		public static readonly Vector3 right = new Vector3(1, 0, 0);
		public static readonly Vector3 up = new Vector3(0, 1, 0);
		public static readonly Vector3 forward = new Vector3(0, 0, 1);

		public static readonly Vector3 left = new Vector3(-1, 0, 0);
		public static readonly Vector3 down = new Vector3(0, -1, 0);
		public static readonly Vector3 backward = new Vector3(0, 0, -1);

		[FieldOffset(0)] public float x;
		[FieldOffset(4)] public float y;
		[FieldOffset(8)] public float z;

		public Vector3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator + (in Vector3 left, in Vector3 right)
		{
			return new Vector3(left.x + right.x, left.y + right.y, left.z + right.z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator -(in Vector3 left, in Vector3 right)
		{
			return new Vector3(left.x - right.x, left.y - right.y, left.z - right.z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator *(in Vector3 left, in Vector3 right)
		{
			return new Vector3(left.x * right.x, left.y * right.y, left.z * right.z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator *(in Vector3 left, in float right)
		{
			return new Vector3(left.x * right, left.y * right, left.z * right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator /(in Vector3 left, in Vector3 right)
		{
			return new Vector3(left.x / right.x, left.y / right.y, left.z / right.z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator /(in Vector3 left, in float right)
		{
			return new Vector3(left.x / right, left.y / right, left.z / right);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly float Length()
		{
			return MathF.Sqrt(x * x + y * y + z * z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly float Dot(in Vector3 other)
		{
			return (x * other.x + y * other.y + z * other.z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Vector3 Cross(in Vector3 other)
		{
			return new Vector3(
				this.y * other.z - this.z * other.y,
				this.z * other.x - this.x * other.z,
				this.x * other.y - this.y * other.x
				);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Vector3 Normalized()
		{
			float length = Length();
			if(length == 0)
			{
				return zero;
			}
			return this / Length();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Normalize()
		{
			float length = Length();
			if (length == 0)
			{
				return;
			}

			x = x / length;
			y = y / length;
			z = z / length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Vector3 Reflect(Vector3 normal)
		{
			//r = d−2(d⋅n)n

			float dot = Dot(normal);
			return this - (normal * dot * 2);
		}



#region EqualityComparison

		public static bool operator ==(Vector3 left, Vector3 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Vector3 left, Vector3 right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return obj is Vector3 vector && Equals(vector);
		}

		public bool Equals([AllowNull] Vector3 other)
		{
			return x == other.x &&
				   y == other.y &&
				   z == other.z;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(x, y, z);
		}

		public bool ApproximatelyEquals(Vector3 other)
		{
			return	MathF.Abs(x - other.x) <= float.Epsilon &&
					MathF.Abs(y - other.y) <= float.Epsilon &&
					MathF.Abs(z - other.z) <= float.Epsilon;
		}

#endregion

		public override string ToString() {
			return $"{x}, {y}, {z}";
		}
	}
}
