using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

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

		/// <summary>
		/// Indicates whether this world is the main world. Only the main world will call Render events.
		/// </summary>
		public bool IsMainWorld { get; }

		private bool initialized = false;
		public ECSWorld()
		{
			ComponentManager = new ComponentManager();
			EntityManager = new EntityManager(ComponentManager);
			SystemManager = new SystemManager(this);
			IsMainWorld = false;
		}

		internal ECSWorld(bool mainWorld = true)
		{
			ComponentManager = new ComponentManager();
			EntityManager = new EntityManager(ComponentManager);
			SystemManager = new SystemManager(this);
			IsMainWorld = mainWorld;
		}

		internal static ECSWorld CreateMain() {
			ECSWorld world = new ECSWorld(true);
			return world;
		}

		public void Initialize(bool autoRegisterSystems = true)
		{
			initialized = true;

			if (autoRegisterSystems)
			{
				SystemManager.AutoRegisterSystems();
			}

			SystemManager.InitializeSystems();
		}

		public void CleanUp()
		{
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
			}
		}

		public void InvokeUpdate(float deltaTime)
		{
			InvokeAllLogExceptions(deltaTime, EarlyUpdate); 
			InvokeAllLogExceptions(deltaTime, Update); 
			InvokeAllLogExceptions(deltaTime, LateUpdate);
		}

		public void InvokeRender(float deltaTime)
		{
			InvokeAllLogExceptions(deltaTime, BeforeRender);
			InvokeAllLogExceptions(deltaTime, Render);
			InvokeAllLogExceptions(deltaTime, AfterRender);
		}

		public void InvokeFixedUpdate(float fixedUpdateStep) {
			InvokeAllLogExceptions(fixedUpdateStep, FixedUpdate);
		}

		public Entity Instantiate(EntityArchetype archetype)
		{
			Entity entity = EntityManager.CreateEntity(archetype);
			return entity;
		}

		public Entity Instantiate(Prefab prefab)
		{
			Entity entity = EntityManager.CreateEntity(prefab.archetype);
			ComponentManager.AddPrefabComponents(entity, prefab);
			return entity;
		}
	}
}
