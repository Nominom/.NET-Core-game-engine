using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Shared
{
	public static class IEnumerableExtensions
	{

		public static T MaxBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector, IComparer<TKey> comparer = null) {
			if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			if (comparer == null) {
				comparer = Comparer<TKey>.Default;
			}
			T maxVal = default;
			bool first = true;
			foreach (T val in enumerable) {
				if (first) {
					maxVal = val;
					first = false;
				}else if(comparer.Compare(selector(val), selector(maxVal)) > 0){
					maxVal = val;
				}
			}

			return maxVal;
		}

		public static T MinBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector, IComparer<TKey> comparer = null) {
			if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			if (comparer == null) {
				comparer = Comparer<TKey>.Default;
			}
			T minVal = default;
			bool first = true;
			foreach (T val in enumerable) {
				if (first) {
					minVal = val;
					first = false;
				}else if(comparer.Compare(selector(val), selector(minVal)) < 0){
					minVal = val;
				}
			}

			return minVal;
		}
	}
}
