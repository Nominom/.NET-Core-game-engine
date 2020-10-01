using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Core.ECS;

namespace Core.Shared
{
	public static class SpanExtensions
	{
		public static Span<TTo> Cast<TFrom, TTo>(this Span<TFrom> span) where TFrom : unmanaged where TTo : unmanaged {
			int toSize = Unsafe.SizeOf<TTo>();
			int fromSize = Unsafe.SizeOf<TFrom>();
			if (toSize < fromSize) {
				DebugHelper.AssertThrow<InvalidOperationException>( fromSize % toSize == 0);
			}
			else {
				DebugHelper.AssertThrow<InvalidOperationException>( toSize % fromSize == 0);
			}
			return MemoryMarshal.Cast<TFrom, TTo>(span);
		} 

		public static ReadOnlySpan<TTo> Cast<TFrom, TTo>(this ReadOnlySpan<TFrom> span) where TFrom : unmanaged where TTo : unmanaged {
			int toSize = Unsafe.SizeOf<TTo>();
			int fromSize = Unsafe.SizeOf<TFrom>();
			if (toSize < fromSize) {
				DebugHelper.AssertThrow<InvalidOperationException>( fromSize % toSize == 0);
			}
			else {
				DebugHelper.AssertThrow<InvalidOperationException>( toSize % fromSize == 0);
			}
			return MemoryMarshal.Cast<TFrom, TTo>(span);
		} 
	}
}
