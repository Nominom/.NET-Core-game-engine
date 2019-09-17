using System;
using System.Collections.Generic;
using System.Text;

namespace ECSCore {
	public class ECSWorld {
		public float fixedUpdateStep = 1 / 30f;

		public delegate void UpdateDelegate(float deltaTime, ECSWorld world);

		public event UpdateDelegate EarlyUpdate;
		public event UpdateDelegate Update;
		public event UpdateDelegate LateUpdate;
		public event UpdateDelegate FixedUpdate;
		public event UpdateDelegate BeforeRender;
		public event UpdateDelegate Render;
		public event UpdateDelegate AfterRender;

		public EntityManager EntityManager { get; }
		public ComponentManager ComponentManager { get; }
		public SystemManager SystemManager { get; }

		/// <summary>
		/// Indicates whether this world is the main world. Only the main world will call Render events.
		/// </summary>
		public bool IsMainWorld { get; }

		private bool initialized = false;

		public ECSWorld(bool mainWorld = true) {
			ComponentManager = new ComponentManager();
			EntityManager = new EntityManager(ComponentManager);
			SystemManager = new SystemManager(this);
			IsMainWorld = mainWorld;
		}

		public void Initialize(bool autoRegisterSystems = true) {
			initialized = true;

			if (autoRegisterSystems) {
				SystemManager.AutoRegisterSystems();
			}

			SystemManager.InitializeSystems();
		}

		public void ForceUpdate(float deltaTime) {
			EarlyUpdate?.Invoke(deltaTime, this);
			Update?.Invoke(deltaTime, this);
			LateUpdate?.Invoke(deltaTime, this);
		}

		public void ForceRender(float deltaTime)
		{
			BeforeRender?.Invoke(deltaTime, this);
			Render?.Invoke(deltaTime, this);
			AfterRender?.Invoke(deltaTime, this);
		}

		public void ForceFixedUpdate()
		{
			FixedUpdate?.Invoke(fixedUpdateStep, this);
		}
	}
}
