using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;

namespace CoreTests
{
	public struct TestComponent1 : IComponent {
		public int i;
		public float f;
		public double d;
	}

	public struct TestComponent2 : IComponent
	{
		public Entity entity;
		public int i;
		public float f;
		public double d;
	}

	public struct TestComponent3 : IComponent
	{
		public int i;
	}

	public struct TestComponent4 : IComponent
	{
		public double d;
	}

	public struct TestComponentFloat : IComponent
	{
		public float f;
	}

	public struct TestComponentVector3 : IComponent {
		public System.Numerics.Vector3 value;
	}

	public class SharedComponent1 : ISharedComponent {
		public int[] array;
	}

	public class SharedComponent2 : ISharedComponent {
		public Entity entity;
	}
}
