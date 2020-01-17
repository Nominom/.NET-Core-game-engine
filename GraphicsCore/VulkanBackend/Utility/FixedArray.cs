// Code from: mellinoe/vk repository https://github.com/mellinoe/vk
namespace Core.Graphics.VulkanBackend.Utility
{
	
	public struct FixedArray2<T> where T : unmanaged
	{
		public T First;
		public T Second;

		public FixedArray2(T first, T second) { First = first; Second = second; }

		public uint Count => 2;
	}

	public struct FixedArray3<T> where T : unmanaged
	{
		public T First;
		public T Second;
		public T Third;

		public FixedArray3(T first, T second, T third) { First = first; Second = second; Third = third; }

		public uint Count => 3;
	}

	public struct FixedArray4<T> where T : unmanaged
	{
		public T First;
		public T Second;
		public T Third;
		public T Fourth;

		public FixedArray4(T first, T second, T third, T fourth) { First = first; Second = second; Third = third; Fourth = fourth; }

		public uint Count => 4;
	}

	public struct FixedArray5<T> where T : unmanaged
	{
		public T First;
		public T Second;
		public T Third;
		public T Fourth;
		public T Fifth;

		public FixedArray5(T first, T second, T third, T fourth, T fifth) { First = first; Second = second; Third = third; Fourth = fourth; Fifth = fifth;}

		public uint Count => 5;
	}

	public struct FixedArray6<T> where T : unmanaged
	{
		public T First;
		public T Second;
		public T Third;
		public T Fourth;
		public T Fifth;
		public T Sixth;

		public FixedArray6(T first, T second, T third, T fourth, T fifth, T sixth) { First = first; Second = second; Third = third; Fourth = fourth; Fifth = fifth; Sixth = sixth; }

		public uint Count => 6;
	}
}
