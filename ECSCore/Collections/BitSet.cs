using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

namespace ECSCore.Collections
{
	public unsafe struct BitSet256
	{
		private const int length = 4;
		private fixed long bits[length];

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void Set(int pos)
		{
			int arrIndex = pos >> 6;
			DebugHelper.AssertThrow(arrIndex < 4, new IndexOutOfRangeException($"Position {pos} is out of range for this BitSet."));
			bits[arrIndex] |= 1L << pos;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void Unset(int pos)
		{
			int arrIndex = pos >> 6;
			DebugHelper.AssertThrow(arrIndex < 4, new IndexOutOfRangeException($"Position {pos} is out of range for this BitSet."));
			bits[arrIndex] &= ~(1L << pos);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public bool Get(int pos)
		{
			int arrIndex = pos >> 6;
			DebugHelper.AssertThrow(arrIndex < 4, new IndexOutOfRangeException($"Position {pos} is out of range for this BitSet."));
			return (bits[arrIndex] & (1L << pos)) != 0;
		}

		public bool this[int index] {
			get => Get(index);
			set {
				if (value) {
					Set(index);
				}
				else {
					Unset(index);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public readonly BitSet256 And(BitSet256 right)
		{
			BitSet256 result;
			if (Avx2.IsSupported)
			{
				fixed (long* l = this.bits)
				{
					long* r = right.bits;
					var lV = Avx.LoadVector256(l);
					var rV = Avx.LoadVector256(r);

					Avx.Store(result.bits, Avx2.And(lV, rV));
				}
			}
			else
			{
				result.bits[0] = bits[0] & right.bits[0];
				result.bits[1] = bits[1] & right.bits[1];
				result.bits[2] = bits[2] & right.bits[2];
				result.bits[3] = bits[3] & right.bits[3];
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public readonly BitSet256 Or(BitSet256 right)
		{
			BitSet256 result;
			if (Avx2.IsSupported)
			{
				fixed (long* l = this.bits)
				{
					long* r = right.bits;
					var lV = Avx.LoadVector256(l);
					var rV = Avx.LoadVector256(r);

					Avx.Store(result.bits, Avx2.Or(lV, rV));
				}
			}
			else
			{
				result.bits[0] = bits[0] | right.bits[0];
				result.bits[1] = bits[1] | right.bits[1];
				result.bits[2] = bits[2] | right.bits[2];
				result.bits[3] = bits[3] | right.bits[3];
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public readonly BitSet256 Xor(BitSet256 right)
		{
			BitSet256 result;
			if (Avx2.IsSupported)
			{
				fixed (long* l = this.bits)
				{
					long* r = right.bits;
					var lV = Avx.LoadVector256(l);
					var rV = Avx.LoadVector256(r);

					Avx.Store(result.bits, Avx2.Xor(lV, rV));
				}
			}
			else
			{
				result.bits[0] = bits[0] ^ right.bits[0];
				result.bits[1] = bits[1] ^ right.bits[1];
				result.bits[2] = bits[2] ^ right.bits[2];
				result.bits[3] = bits[3] ^ right.bits[3];
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public readonly BitSet256 Invert()
		{
			BitSet256 result;

			result.bits[0] = ~bits[0];
			result.bits[1] = ~bits[1];
			result.bits[2] = ~bits[2];
			result.bits[3] = ~bits[3];

			return result;
		}

		public static BitSet256 operator &(in BitSet256 left, in BitSet256 right) => left.And(right);
		public static BitSet256 operator |(in BitSet256 left, in BitSet256 right) => left.Or(right);
		public static BitSet256 operator ^(in BitSet256 left, in BitSet256 right) => left.Xor(right);
		public static BitSet256 operator ~(in BitSet256 input) => input.Invert();


		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public readonly bool ContainsAll(BitSet256 other)
		{
			if ((bits[0] & other.bits[0]) != other.bits[0]) return false;
			if ((bits[1] & other.bits[1]) != other.bits[1]) return false;
			if ((bits[2] & other.bits[2]) != other.bits[2]) return false;
			if ((bits[3] & other.bits[3]) != other.bits[3]) return false;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public readonly bool ContainsAny(BitSet256 other)
		{
			if ((bits[0] & other.bits[0]) != 0) return true;
			if ((bits[1] & other.bits[1]) != 0) return true;
			if ((bits[2] & other.bits[2]) != 0) return true;
			if ((bits[3] & other.bits[3]) != 0) return true;
			return false;
		}



		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public bool Equals(BitSet256 other) {
			return
				bits[0] == other.bits[0] &&
				bits[1] == other.bits[1] &&
				bits[2] == other.bits[2] &&
				bits[3] == other.bits[3];
		}

		public override bool Equals(object obj) {
			return obj is BitSet256 other && Equals(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private ulong murmur64(ulong l) {
			unchecked {
				l ^= l >> 33;
				l *= 0xff51afd7ed558ccdL;
				l ^= l >> 33;
				l *= 0xc4ceb9fe1a85ec53L;
				l ^= l >> 33;
				return l;
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public override int GetHashCode() {
			unchecked {
				ulong b0 = murmur64((ulong)bits[0]);
				ulong b1 = (b0 * 359) ^ murmur64((ulong)bits[1]);
				ulong b2 = (b1 * 359) ^ murmur64((ulong)bits[2]);
				ulong b3 = (b2 * 359) ^ murmur64((ulong)bits[3]);

				return (int)((b3 >> 32) ^ b3);
			}
		}

		public static bool operator ==(BitSet256 left, BitSet256 right) {
			return left.Equals(right);
		}

		public static bool operator !=(BitSet256 left, BitSet256 right) {
			return !left.Equals(right);
		}
	}
}
