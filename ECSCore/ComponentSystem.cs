using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS
{

	public abstract class ComponentSystemBase : ISystem
	{
		public bool Enabled { get; set; }
		public virtual void OnCreateSystem(ECSWorld world) { }
		public virtual void OnDestroySystem(ECSWorld world) { }
		public virtual void OnEnableSystem(ECSWorld world) { }
		public virtual void OnDisableSystem(ECSWorld world) { }
		public abstract void Update(float deltaTime, ECSWorld world);
	}
	public abstract class ComponentSystem : ComponentSystemBase
	{
		private ComponentQuery query;
		private bool initialized = false;

		public override void Update(float deltaTime, ECSWorld world)
		{
			if (!initialized)
			{
				query = GetQuery();
			}

			IEnumerable<BlockAccessor> blocks = world.ComponentManager.GetBlocks(query);
			foreach (BlockAccessor block in blocks)
			{
				ProcessBlock(deltaTime, block);
			}
		}

		public abstract ComponentQuery GetQuery();
		public abstract void ProcessBlock(float deltaTime, BlockAccessor accessor);
	}
}
