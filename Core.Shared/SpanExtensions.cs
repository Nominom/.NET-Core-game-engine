using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Core.ECS;

namespace Core.Shared
{
	public static class SpanExtensions
	{
		public static Span<TTo> Cast<TFrom, TTo>(this Span<TFrom> span) where TFrom : unmanaged where TTo : unmanaged {
			DebugHelper.AssertThrow<InvalidOperationException>( Marshal.SizeOf<TTo>() % Marshal.SizeOf<TFrom>() == 0);
			return MemoryMarshal.Cast<TFrom, TTo>(span);
		} 

		public static ReadOnlySpan<TTo> Cast<TFrom, TTo>(this ReadOnlySpan<TFrom> span) where TFrom : unmanaged where TTo : unmanaged {
			DebugHelper.AssertThrow<InvalidOperationException>( Marshal.SizeOf<TTo>() % Marshal.SizeOf<TFrom>() == 0);
			return MemoryMarshal.Cast<TFrom, TTo>(span);
		} 
	}
}
