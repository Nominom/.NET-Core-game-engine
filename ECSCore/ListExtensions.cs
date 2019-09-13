using System;
using System.Collections.Generic;
using System.Text;

namespace ECSCore
{
	public static class ListExtensions
	{
		public static void Swap<T>(this List<T> list, int idx1, int idx2) {
			T item = list[idx1];
			list[idx1] = list[idx2];
			list[idx2] = item;
		}
	}
}
