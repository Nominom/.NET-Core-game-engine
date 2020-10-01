using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Shared
{
	public class PooledList<T> : IDisposable, IList<T>
	{
		private T[] array = Array.Empty<T>();
		private int capacity = 0;
		private int size = 0;
		private bool disposed;

		private static readonly ConcurrentQueue<PooledList<T>> ListPool = new ConcurrentQueue<PooledList<T>>();
#if DEBUG
		private PooledList(string methodName, int lineNumber,
			string filePath) {
			this.allocStackTrace = $"{filePath}.{methodName}:{lineNumber.ToString()}";
			disposed = false;
		}

		public static PooledList<T> Create([CallerMemberName] string methodName = "", [CallerLineNumber] int lineNumber = 0,
			[CallerFilePath] string filePath = "")
		{
			if (ListPool.TryDequeue(out var result)) {
				result.disposed = false;
				result.allocStackTrace = $"{filePath}.{methodName}:{lineNumber.ToString()}";
				GC.ReRegisterForFinalize(result);
				return result;
			}
			else
			{
				return new PooledList<T>(methodName, lineNumber, filePath);
			}
		}
#else
		private PooledList() {
			disposed = false;
		}

		public static PooledList<T> Create()
		{
			if (ListPool.TryDequeue(out var result))
			{
				result.disposed = false;
				return result;
			}
			else
			{
				return new PooledList<T>();
			}
		}
#endif

#if DEBUG
		private string allocStackTrace;
		~PooledList()
		{
			Console.WriteLine("Dispose was not called correctly on a PooledList allocated at: " + allocStackTrace);
		}
#endif

		public void Dispose() {
			if (disposed) {
				throw new InvalidOperationException("Cannot dispose an already disposed PooledList.");
			}
			Clear();
			disposed = true;
			ListPool.Enqueue(this);
#if DEBUG
			GC.SuppressFinalize(this);
#endif
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (disposed) {
				throw new InvalidOperationException("Cannot enumerate a disposed PooledList.");
			}
			for (int i = 0; i < size; i++)
			{
				yield return array[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void Resize()
		{
			if (capacity == 0)
			{
				array = new T[64];
				capacity = array.Length;
			}
			else
			{
				var newArray = new T[capacity * 2];
				Array.Copy(array, newArray, size);
				array = newArray;
				capacity = array.Length;
			}
		}

		public void Add(T item)
		{
			if (disposed) {
				throw new InvalidOperationException("Cannot Add to a disposed PooledList.");
			}
			if (size >= capacity)
			{
				Resize();
			}
			array[size] = item;
			size++;
		}

		public void AddRange(IEnumerable<T> range)
		{
			if (disposed) {
				throw new InvalidOperationException("Cannot Add to a disposed PooledList.");
			}
			foreach (var v in range)
			{
				Add(v);
			}
		}

		public void Clear()
		{
			if (disposed) {
				throw new InvalidOperationException("Cannot Clear a disposed PooledList.");
			}
			Span<T> items = array;
			items.Slice(0, size).Clear();
			size = 0;
		}

		public bool Contains(T item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(T[] array, int arrayIndex) {
			Span<T> source = this.array;
			source = source.Slice(0, size);
			Span<T> dest = array;
			dest = dest.Slice(arrayIndex);

			source.CopyTo(dest);
		}

		public bool Remove(T item)
		{
			throw new NotImplementedException();
		}

		public int Count => size;
		public bool IsReadOnly => false;

		public int IndexOf(T item)
		{
			throw new NotImplementedException();
		}

		public void Insert(int index, T item)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		public T this[int index] {
			get {
				if (index >= size || index < 0) throw new IndexOutOfRangeException();
				if (disposed) throw new InvalidOperationException("Cannot access a disposed PooledList.");
				return array[index];
			}
			set {
				if (index >= size || index < 0) throw new IndexOutOfRangeException();
				if (disposed) throw new InvalidOperationException("Cannot access a disposed PooledList.");
				array[index] = value;
			}
		}
	}
}
