using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Shared
{
	public static class ArrayExtensions
	{
		public static T[] CopyResize<T>(this T[] original, int newSize) {
			T[] newArr = new T[newSize];
			int copyLen = Math.Min(original.Length, newArr.Length);
			Array.Copy(original, newArr, copyLen);
			return newArr;
		}
	}
}
