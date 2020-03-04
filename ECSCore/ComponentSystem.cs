using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.ECS.Filters;
using Core.ECS.JobSystem;

namespace Core.ECS
{

	public abstract class ComponentSystemBase : ISystem
	{
		public bool Enabled { get; set; }
		public virtual void OnCreateSystem(ECSWorld world) { }
		public virtual void OnDestroySystem(ECSWorld world) { }
		public abstract void Update(float deltaTime, ECSWorld world);
	}

	public abstract class ComponentSystem : ComponentSystemBase
	{
		private ComponentQuery query;
		private IComponentFilter filter;
		private bool initialized = false;
		protected EntityCommandBuffer afterUpdateCommands;

		public virtual void BeforeUpdate(float deltaTime, ECSWorld world) { }
		public virtual void AfterUpdate(float deltaTime, ECSWorld world) { }

		public sealed override void Update(float deltaTime, ECSWorld world)
		{
			if (!initialized)
			{
				afterUpdateCommands = new EntityCommandBuffer(world);
				query = GetQuery();
				filter = GetComponentFilter();
				initialized = true;
			}
			BeforeUpdate(deltaTime, world);

			var blocks = world.ComponentManager.GetBlocks(query, filter);
			foreach (BlockAccessor block in blocks)
			{
				ProcessBlock(deltaTime, block);
			}
			afterUpdateCommands.Playback();
			AfterUpdate(deltaTime, world);
		}

		public abstract ComponentQuery GetQuery();

		public virtual IComponentFilter GetComponentFilter() {
			return ComponentFilters.Empty();
		}

		public abstract void ProcessBlock(float deltaTime, BlockAccessor block);
	}

	public abstract class JobComponentSystem : ComponentSystemBase
	{
		private ComponentQuery query;
		private IComponentFilter filter;
		private bool initialized = false;
		protected ConcurrentEntityCommandBuffer afterUpdateCommands;

		public struct ComponentProcessJob : IJob {
			public float deltaTime;
			public BlockAccessor block;
			public JobComponentSystem instance;

			public void Execute() {
				instance.ProcessBlock(deltaTime, block);
			}
		}

		public virtual void BeforeUpdate(float deltaTime, ECSWorld world) { }
		public sealed override void Update(float deltaTime, ECSWorld world)
		{
			if (!initialized)
			{
				afterUpdateCommands = new ConcurrentEntityCommandBuffer(world);
				query = GetQuery();
				filter = GetComponentFilter();
				initialized = true;
			}
			BeforeUpdate(deltaTime, world);

			afterUpdateCommands.PlaybackAfterUpdate();

			var blocks = world.ComponentManager.GetBlocksNoSync(query, filter);
			var group = Jobs.StartNewGroup(query);
			foreach (BlockAccessor block in blocks) {
				ComponentProcessJob processJob = new ComponentProcessJob() {
					block = block,
					deltaTime = deltaTime,
					instance = this
				};
				processJob.Schedule(group);
			}
		}

		public abstract ComponentQuery GetQuery();
		public virtual IComponentFilter GetComponentFilter() {
			return ComponentFilters.Empty();
		}
		public abstract void ProcessBlock(float deltaTime, BlockAccessor block);
	}

}
