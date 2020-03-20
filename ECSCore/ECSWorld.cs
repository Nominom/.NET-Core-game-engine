using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Core.ECS.Events;
using Core.ECS.JobSystem;
using Core.Profiling;

namespace Core.ECS
{
	public class ECSWorld
	{
		public delegate void UpdateDelegate(float deltaTime, ECSWorld world);

		public event UpdateDelegate EarlyUpdate;
		public event UpdateDelegate Update;
		public event UpdateDelegate LateUpdate;
		public event UpdateDelegate FixedUpdate;
		public event UpdateDelegate BeforeRender;
		public event UpdateDelegate Render;
		public event UpdateDelegate AfterRender;

		public event Action OnWorldDispose;

		public EntityManager EntityManager { get; }
		public ComponentManager ComponentManager { get; }
		public SystemManager SystemManager { get; }
		public EventManager EventManager { get; }

		private List<IEntityCommandBuffer> entityCommandBuffersToExecute = new List<IEntityCommandBuffer>();

		internal static Thread mainThread;

		/// <summary>
		/// Indicates whether this world is the main world. Only the main world will call Render events.
		/// </summary>
		public bool IsMainWorld { get; }

		private bool initialized = false;
		public ECSWorld()
		{
			ComponentManager = new ComponentManager(this);
			EntityManager = new EntityManager(this, ComponentManager);
			SystemManager = new SystemManager(this);
			EventManager = new EventManager(this);
			IsMainWorld = false;
			mainThread = Thread.CurrentThread;
			mainThread.Priority = ThreadPriority.Highest;
		}

		internal ECSWorld(bool mainWorld = true)
		{
			ComponentManager = new ComponentManager(this);
			EntityManager = new EntityManager(this, ComponentManager);
			SystemManager = new SystemManager(this);
			EventManager = new EventManager(this);
			IsMainWorld = mainWorld;
			mainThread = Thread.CurrentThread;
			Profiler.RegisterThread("MainThread");
		}

		internal static ECSWorld CreateMain() {
			ECSWorld world = new ECSWorld(true);
			return world;
		}

		public void Initialize(bool autoRegisterSystems = true)
		{
			DebugHelper.AssertThrow<ThreadAccessException>(CheckThreadIsMainThread());
			Jobs.Setup();
			initialized = true;

			if (autoRegisterSystems)
			{
				SystemManager.AutoRegisterSystems();
			}

			SystemManager.InitializeSystems();
		}

		public void CleanUp()
		{
			DebugHelper.AssertThrow<ThreadAccessException>(CheckThreadIsMainThread());
			SystemManager.CleanUp();
			ComponentManager.CleanUp();
			OnWorldDispose?.Invoke();
		}

		private void InvokeAllLogExceptions(float deltaTime, UpdateDelegate evt) {
			if (evt != null) {
				foreach (var @delegate in evt.GetInvocationList()) {
					try {
						var del = (UpdateDelegate)@delegate;
						del.Invoke(deltaTime, this);
					}catch(Exception ex)
					{
						Console.WriteLine(ex);
					}

				}
				Profiler.StartMethod("ExecuteCommandBuffers");
				foreach (IEntityCommandBuffer buffer in entityCommandBuffersToExecute) {
					buffer.Playback();
				}
				entityCommandBuffersToExecute.Clear();
				Profiler.EndMethod();

				Profiler.StartMethod("DeliverEvents");
				EventManager.DeliverEvents();
				Profiler.EndMethod();
			}
		}

		public void InvokeUpdate(float deltaTime)
		{
			DebugHelper.AssertThrow<ThreadAccessException>(CheckThreadIsMainThread());
			Profiler.StartMethod("EarlyUpdate");
			InvokeAllLogExceptions(deltaTime, EarlyUpdate);
			Profiler.EndMethod();

			Profiler.StartMethod("Update");
			InvokeAllLogExceptions(deltaTime, Update);
			Profiler.EndMethod();

			Profiler.StartMethod("LateUpdate");
			InvokeAllLogExceptions(deltaTime, LateUpdate);
			Profiler.EndMethod();
		}

		public void InvokeRender(float deltaTime)
		{
			DebugHelper.AssertThrow<ThreadAccessException>(CheckThreadIsMainThread());
			Profiler.StartMethod("BeforeRender");
			InvokeAllLogExceptions(deltaTime, BeforeRender);
			Profiler.EndMethod();

			Profiler.StartMethod("Render");
			InvokeAllLogExceptions(deltaTime, Render);
			Profiler.EndMethod();

			Profiler.StartMethod("AfterRender");
			InvokeAllLogExceptions(deltaTime, AfterRender);
			Profiler.EndMethod();
		}

		public void InvokeFixedUpdate(float fixedUpdateStep) {
			DebugHelper.AssertThrow<ThreadAccessException>(CheckThreadIsMainThread());
			Profiler.StartMethod("FixedUpdate");
			InvokeAllLogExceptions(fixedUpdateStep, FixedUpdate);
			Profiler.EndMethod();
		}

		public Entity Instantiate(EntityArchetype archetype)
		{
			DebugHelper.AssertThrow<ThreadAccessException>(CheckThreadIsMainThread());
			Entity entity = EntityManager.CreateEntity(archetype);
			return entity;
		}

		public Entity Instantiate(Prefab prefab)
		{
			DebugHelper.AssertThrow<ThreadAccessException>(CheckThreadIsMainThread());
			Entity entity = EntityManager.CreateEntity(prefab.archetype);
			ComponentManager.AddPrefabComponents(entity, prefab);
			return entity;
		}

		public void SyncPoint()
		{
			Profiler.StartMethod("Sync");
			Jobs.CompleteAllJobs();
			Profiler.EndMethod();
		}

		internal void RegisterForExecuteAfterUpdate(IEntityCommandBuffer buffer) {
			entityCommandBuffersToExecute.Add(buffer);
		}

		internal static bool CheckThreadIsMainThread() {
			return Thread.CurrentThread.ManagedThreadId == mainThread.ManagedThreadId;
		}
	}
}
