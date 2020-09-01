using System;
using System.Collections.Generic;
using System.Text;

namespace AssetPackager
{
	public static class EnumParser<T> {
		private static Dictionary<string, T> stringToEnum = BuildDictionary();

		private static Dictionary<string, T> BuildDictionary() {
			if (!typeof(T).IsEnum)
			{
				return new Dictionary<string, T>();
			}

			var dictionary = new Dictionary<string, T>();

			var values = Enum.GetNames(typeof(T));
			foreach (string value in values) {
				string lowerCase = value.ToLower();
				dictionary[lowerCase] = (T)Enum.Parse(typeof(T), value);
			}

			return dictionary;
		}

		public static bool TryParse(string enumString, out T value) {
			string lowerCase = enumString.ToLower();
			if(stringToEnum.TryGetValue(lowerCase, out  value)) {
				return true;
			}
			else {
				return false;
			}
		}
	}
}
