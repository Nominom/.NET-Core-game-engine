using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Core.ECS;

namespace Core.Shared
{
	public static class SpanExtensions
	{
		[Conditional("DEBUG")]
		private static void CheckSize<TFrom, TTo>() {
			int fromSize = Marshal.SizeOf<TFrom>();
			int toSize = Marshal.SizeOf<TTo>();
			if (fromSize > toSize && fromSize % toSize != 0) {
				throw new InvalidOperationException(
					$"Cannot cast type {typeof(TFrom).Name} to {typeof(TTo).Name}. Because their sizes are not divisible.");
			}else if (fromSize < toSize && toSize % fromSize != 0) {
				throw new InvalidOperationException(
					$"Cannot cast type {typeof(TFrom).Name} to {typeof(TTo).Name}. Because their sizes are not divisible.");
			}
		}

		public static Span<TTo> Cast<TFrom, TTo>(this Span<TFrom> span) where TFrom : unmanaged where TTo : unmanaged {
			CheckSize<TFrom, TTo>();
			return MemoryMarshal.Cast<TFrom, TTo>(span);
		} 

		public static ReadOnlySpan<TTo> Cast<TFrom, TTo>(this ReadOnlySpan<TFrom> span) where TFrom : unmanaged where TTo : unmanaged {
			CheckSize<TFrom, TTo>();
			return MemoryMarshal.Cast<TFrom, TTo>(span);
		} 
	}
}
