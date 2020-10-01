using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Shared
{
	public static class ListExtensions
	{

		public static void SortWithComparer<T>(this List<T> list, Comparison<T> cmp)
		{
			for (int i = 0; i < list.Count; i++) {

				for (int j = i; j < list.Count; j++) {
					if (i == j) continue;
					var result = cmp.Invoke(list[i], list[j]);
					if (result > 0) {
						var tmp = list[i];
						list[i] = list[j];
						list[j] = tmp;
					}
				}
			}
		}
	}
}
