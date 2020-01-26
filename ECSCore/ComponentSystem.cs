using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.ECS.Jobs;

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


	public abstract class AsyncComponentSystem : ComponentSystemBase
	{
		private ComponentQuery query;
		private bool initialized = false;
		protected EntityCommandBuffer afterUpdateCommands;
		private List<Task> runningTasks = new List<Task>();

		public virtual void BeforeUpdate() { }
		public virtual void AfterUpdate() { }

		public override void Update(float deltaTime, ECSWorld world)
		{
			if (!initialized)
			{
				afterUpdateCommands = new EntityCommandBuffer(world);
				query = GetQuery();
			}
			BeforeUpdate();

			IEnumerable<BlockAccessor> blocks = world.ComponentManager.GetBlocks(query);
			foreach (BlockAccessor block in blocks)
			{
				Task processTask = Task.Run(() => {
					ProcessBlock(deltaTime, block);
				});
				runningTasks.Add(processTask);
			}

			foreach (Task runningTask in runningTasks) {
				runningTask.GetAwaiter().GetResult();
			}

			runningTasks.Clear();

			afterUpdateCommands.Playback();
			AfterUpdate();
		}

		public abstract ComponentQuery GetQuery();
		public abstract void ProcessBlock(float deltaTime, BlockAccessor block);
	}

	public abstract class ComponentSystem : ComponentSystemBase
	{
		private ComponentQuery query;
		private bool initialized = false;
		protected EntityCommandBuffer afterUpdateCommands;

		public virtual void BeforeUpdate() { }
		public virtual void AfterUpdate() { }

		public override void Update(float deltaTime, ECSWorld world)
		{
			if (!initialized)
			{
				afterUpdateCommands = new EntityCommandBuffer(world);
				query = GetQuery();
			}
			BeforeUpdate();

			IEnumerable<BlockAccessor> blocks = world.ComponentManager.GetBlocks(query);
			foreach (BlockAccessor block in blocks)
			{
				ProcessBlock(deltaTime, block);
			}
			afterUpdateCommands.Playback();
			AfterUpdate();
		}

		public abstract ComponentQuery GetQuery();
		public abstract void ProcessBlock(float deltaTime, BlockAccessor block);
	}

	public abstract class JobComponentSystem : ComponentSystemBase
	{
		private ComponentQuery query;
		private bool initialized = false;
		protected EntityCommandBuffer afterUpdateCommands;
		private List<JobHandle> runningJobs = new List<JobHandle>();

		public struct ComponentProcessJob : IShortJob {
			public float deltaTime;
			public BlockAccessor block;
			public JobComponentSystem instance;

			public void DoJob() {
				instance.ProcessBlock(deltaTime, block);
			}
		}

		public virtual void BeforeUpdate() { }
		public virtual void AfterUpdate() { }

		public override void Update(float deltaTime, ECSWorld world)
		{
			if (!initialized)
			{
				afterUpdateCommands = new EntityCommandBuffer(world);
				query = GetQuery();
			}
			BeforeUpdate();

			IEnumerable<BlockAccessor> blocks = world.ComponentManager.GetBlocks(query);
			foreach (BlockAccessor block in blocks) {
				ComponentProcessJob processJob = new ComponentProcessJob() {
					block = block,
					deltaTime = deltaTime,
					instance = this
				};
				runningJobs.Add(processJob.Schedule());
			}

			foreach (var runningJob in runningJobs) {
				runningJob.Complete();
			}

			runningJobs.Clear();

			afterUpdateCommands.Playback();
			AfterUpdate();
		}

		public abstract ComponentQuery GetQuery();
		public abstract void ProcessBlock(float deltaTime, BlockAccessor block);
	}

}
