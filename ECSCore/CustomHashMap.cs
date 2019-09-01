using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace ECSCore {
	internal struct CustomHashMapRecord<T> {
		public int Key;
		public T Value;
		public bool isSet;

		public CustomHashMapRecord(KeyValuePair<int, T> kp) {
			Key = kp.Key;
			Value = kp.Value;
			isSet = true;
		}

		public static implicit operator KeyValuePair<int, T>(in CustomHashMapRecord<T> record) {
			return new KeyValuePair<int, T>(record.Key, record.Value);
		}

		public static implicit operator CustomHashMapRecord<T>(in KeyValuePair<int, T> kp) {
			return new CustomHashMapRecord<T>(kp);
		}
	}

	public class CustomHashMap<T> : IDictionary<int, T> {
		private CustomHashMapRecord<T>[] data;
		private PooledArray<KeyValuePair<int, T>> cached_values;
		private int currentSize;
		private int mask;

		public CustomHashMap() {
			Clear();
		}

		private void Grow() {
			mask <<= 1;
			var newData = new CustomHashMapRecord<T>[mask + 1];
			Array.Copy(data, newData, data.Length);
			/*foreach (var record in data) {
				int newIndex = record.Key & mask;
				newData[newIndex] = record;
			}*/
			data = newData;
		}

		private void InvalidateCache() {
			if (!cached_values.IsEmpty()) {
				cached_values.Dispose();
				cached_values = PooledArray<KeyValuePair<int, T>>.Empty();
			}
		}

		public ReadOnlySpan<KeyValuePair<int, T>> GetCachedValues() {
			if (cached_values.IsEmpty()) {
				cached_values = PooledArray<KeyValuePair<int, T>>.Rent(currentSize);
				int j = 0;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isSet) {
						cached_values.array[j++] = data[i];
					}
				}
			}
			return new ReadOnlySpan<KeyValuePair<int, T>>(cached_values.array,0, currentSize);
		}

		public IEnumerator<KeyValuePair<int, T>> GetEnumerator() {
			/*var span = GetCachedValues();
			for (int i = 0; i < span.Length; i++) {
				yield return span[i];
			}*/
			for (int i = 0; i < data.Length; i++) {
				if (data[i].isSet) {
					yield return data[i];
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Add(KeyValuePair<int, T> item) {
			InvalidateCache();
			int index = item.Key & mask;
			CustomHashMapRecord<T> rec = data[index];
			if (rec.isSet && item.Key == rec.Key) {
				data[index] = item;
				return;
			}

			if (rec.isSet) {
				//Grow until hashes don't collide anymore
				do {
					Grow();
					index = item.Key & mask;
				} while (data[index].isSet && mask != -1);

				if (mask == -1) {
					throw new OverflowException("dictionary mask rolled over to -1");
				}
			}
			data[index] = item;
			currentSize++;
		}

		public void Clear() {
			currentSize = 0;
			mask = 0b1;
			data = new CustomHashMapRecord<T>[mask + 1];
			InvalidateCache();
		}

		public bool Contains(KeyValuePair<int, T> item) {
			int index = item.Key & mask;
			var record = data[index];
			if (record.isSet && record.Key == item.Key && record.Value.Equals(item.Value)) {
				return true;
			}
			else {
				return false;
			}
		}

		public void CopyTo(KeyValuePair<int, T>[] array, int arrayIndex) {
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<int, T> item) {
			return Remove(item.Key);
		}

		public int Count => currentSize;

		public bool IsReadOnly => false;

		public void Add(int key, T value) {
			Add(new KeyValuePair<int, T>(key, value));
		}

		public bool ContainsKey(int key) {
			int index = key & mask;
			return data[index].isSet && data[index].Key == key;
		}

		public bool Remove(int key) {
			int index = key & mask;
			if (data[index].isSet && key == data[index].Key) {
				data[index] = new CustomHashMapRecord<T>();
				InvalidateCache();
				return true;
			}
			else {
				return false;
			}
		}

		public bool TryGetValue(int key, out T value) {
			int index = key & mask;
			var record = data[index];
			if (!record.isSet || record.Key != key) {
				value = default(T);
				return false;
			}
			else {
				value = record.Value;
				return true;
			}
		}

		public T this[int key] {
			get {
				int index = key & mask;
				var record = data[index];
				if(!record.isSet || record.Key != key) throw new KeyNotFoundException();
				return record.Value;
			}
			set => Add(new KeyValuePair<int, T>(key, value));
		}

		public ICollection<int> Keys { get; }
		public ICollection<T> Values { get; }
	}
}
