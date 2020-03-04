using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Xunit;

namespace CoreTests
{
	public class SystemRegisterTests
	{
		private static List<System.Type> updateOrder = new List<System.Type>();

		[ECSSystem(updateBefore: typeof(TestSystem2))]
		public class TestSystem1 : ISystem {
			public bool Enabled { get; set; }
			public void OnCreateSystem(ECSWorld world) { }
			public void OnDestroySystem(ECSWorld world) { }
			public void Update(float deltaTime, ECSWorld world) 
			{
				updateOrder.Add(this.GetType());
			}
		}

		[ECSSystem(updateAfter: typeof(TestSystem3))]
		public class TestSystem2 : ISystem
		{
			public bool Enabled { get; set; }
			public void OnCreateSystem(ECSWorld world) { }
			public void OnDestroySystem(ECSWorld world) { }
			public void Update(float deltaTime, ECSWorld world)
			{
				updateOrder.Add(this.GetType());
			}
		}

		[ECSSystem]
		public class TestSystem3 : ISystem
		{
			public bool Enabled { get; set; }
			public void OnCreateSystem(ECSWorld world) { }
			public void OnDestroySystem(ECSWorld world) { }
			public void Update(float deltaTime, ECSWorld world)
			{
				updateOrder.Add(this.GetType());
			}
		}

		[Fact]
		public void AutoFindUpdateOrder() {
			updateOrder.Clear();

			ECSWorld world = new ECSWorld();
			world.Initialize();
			world.InvokeUpdate(1);

			Assert.Equal(3, updateOrder.Count);
			Assert.Equal(typeof(TestSystem1), updateOrder[0]);
			Assert.Equal(typeof(TestSystem3), updateOrder[1]);
			Assert.Equal(typeof(TestSystem2), updateOrder[2]);
		}
	}
}
