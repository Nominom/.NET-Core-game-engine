using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ECSCore
{

	struct SystemHolder
	{
		public ISystem system;
		public System.Type systemType;
		public System.Type updateBefore;
		public System.Type updateAfter;
	}

	public class SystemManager
	{

		private ECSWorld world;

		private readonly Dictionary<System.Type, ISystem> systems = new Dictionary<Type, ISystem>();

		private readonly List<SystemHolder> earlyUpdateSystems = new List<SystemHolder>();
		private readonly List<SystemHolder> updateSystems = new List<SystemHolder>();
		private readonly List<SystemHolder> lateUpdateSystems = new List<SystemHolder>();

		private readonly List<SystemHolder> fixedUpdateSystems = new List<SystemHolder>();

		private readonly List<SystemHolder> beforeRenderSystems = new List<SystemHolder>();
		private readonly List<SystemHolder> renderSystems = new List<SystemHolder>();
		private readonly List<SystemHolder> afterRenderSystems = new List<SystemHolder>();

		public SystemManager(ECSWorld world)
		{
			this.world = world;
			world.EarlyUpdate += EarlyUpdate;
			world.Update += OnUpdate;
			world.LateUpdate += OnLateUpdate;
			world.FixedUpdate += OnFixedUpdate;
			world.BeforeRender += OnBeforeRender;
			world.Render += OnRender;
			world.AfterRender += OnAfterRender;
		}


		private void RegisterSystemsWithAttribute()
		{
			//TODO: Cache these because enumerating all the assemblies is pretty slow.
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(ass => ass.IsDynamic == false);
			foreach (var assembly in assemblies) {
				string name = assembly.FullName;
				if (name.StartsWith("Microsoft")) {
					continue;
				}
				foreach (Type type in TypeHelper.GetTypesWithAttribute(assembly, typeof(ECSSystemAttribute)))
				{
					try
					{
						if (!type.GetInterfaces().Contains(typeof(ISystem)))
						{
							continue;
						}

						ECSSystemAttribute attribute =
							type.GetCustomAttribute(typeof(ECSSystemAttribute)) as ECSSystemAttribute;
						ISystem instance = Activator.CreateInstance(type) as ISystem;

						RegisterSystem(instance, type, attribute.updateEvent, attribute.updateBefore,
							attribute.updateAfter);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"could not create an instance of type {type.Name}. Does it have an empty constructor? \n{ex.StackTrace}");
					}
				}
			}
		}

		internal void AutoRegisterSystems()
		{
			RegisterSystemsWithAttribute();
		}

		private void SortSystemList(List<SystemHolder> list)
		{
			list.Sort((a, b) =>
			{
				if (a.updateBefore == b.systemType || b.updateAfter == a.systemType)
				{
					return -1;
				}

				if (a.updateAfter == b.systemType || b.updateBefore == a.systemType)
				{
					return 1;
				}

				return 0;
			});
		}

		private void RegisterSystemTo(List<SystemHolder> list, ISystem system, System.Type systemType, System.Type updateBefore, System.Type updateAfter)
		{
			if (list == null)
			{
				Console.WriteLine("List was null while registering a system.");
				return;
			}

			if (systems.ContainsKey(systemType))
			{
				Console.WriteLine($"System {systemType.Name} is already registered!");
				return;
			}

			systems.Add(systemType, system);

			list.Add(new SystemHolder()
			{
				system = system,
				systemType = systemType,
				updateBefore = updateBefore,
				updateAfter = updateAfter
			});

			SortSystemList(list);
		}

		private void RegisterSystem(ISystem system, System.Type systemType, UpdateEvent updateEvent, System.Type updateBefore, System.Type updateAfter)
		{
			switch (updateEvent)
			{
				case UpdateEvent.Update:
					RegisterSystemTo(updateSystems, system, systemType, updateBefore, updateAfter);
					break;
				case UpdateEvent.EarlyUpdate:
					RegisterSystemTo(earlyUpdateSystems, system, systemType, updateBefore, updateAfter);
					break;
				case UpdateEvent.LateUpdate:
					RegisterSystemTo(lateUpdateSystems, system, systemType, updateBefore, updateAfter);
					break;
				case UpdateEvent.FixedUpdate:
					RegisterSystemTo(fixedUpdateSystems, system, systemType, updateBefore, updateAfter);
					break;
				case UpdateEvent.BeforeRender:
					RegisterSystemTo(beforeRenderSystems, system, systemType, updateBefore, updateAfter);
					break;
				case UpdateEvent.Render:
					RegisterSystemTo(renderSystems, system, systemType, updateBefore, updateAfter);
					break;
				case UpdateEvent.AfterRender:
					RegisterSystemTo(afterRenderSystems, system, systemType, updateBefore, updateAfter);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(updateEvent), updateEvent, null);
			}
		}

		public void RegisterSystem<T>(UpdateEvent updateEvent = UpdateEvent.Update, System.Type updateBefore = null, System.Type updateAfter = null) where T : class, ISystem, new()
		{
			ISystem instance = Activator.CreateInstance<T>();
			RegisterSystem(instance, typeof(T), updateEvent, updateBefore, updateAfter);
		}

		public void RegisterSystem<T>(T system, UpdateEvent updateEvent = UpdateEvent.Update, System.Type updateBefore = null, System.Type updateAfter = null) where T : class, ISystem
		{
			RegisterSystem(system, typeof(T), updateEvent, updateBefore, updateAfter);
		}

		public T GetSystem<T>() where T : class, ISystem
		{
			if (systems.TryGetValue(typeof(T), out ISystem val))
			{
				return val as T;
			}
			else
			{
				return null;
			}
		}

		internal void InitializeSystems()
		{
			foreach (var system in systems) {
				system.Value.OnCreateSystem();
				system.Value.Enabled = true;
			}
		}

		internal void CleanUpSystems()
		{
			foreach (var system in systems)
			{
				system.Value.OnDisableSystem();
				system.Value.OnDestroySystem();
			}
		}

		internal void EarlyUpdate(float deltaTime, ECSWorld world)
		{
			foreach (var system in earlyUpdateSystems)
			{
				try
				{
					if (system.system.Enabled)
					{
						system.system.Update(deltaTime, world);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

		internal void OnUpdate(float deltaTime, ECSWorld world)
		{
			foreach (var system in updateSystems)
			{
				try
				{
					if (system.system.Enabled)
					{
						system.system.Update(deltaTime, world);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

		internal void OnLateUpdate(float deltaTime, ECSWorld world)
		{
			foreach (var system in lateUpdateSystems)
			{
				try
				{
					if (system.system.Enabled)
					{
						system.system.Update(deltaTime, world);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

		internal void OnFixedUpdate(float deltaTime, ECSWorld world)
		{
			foreach (var system in fixedUpdateSystems)
			{
				try
				{
					if (system.system.Enabled)
					{
						system.system.Update(deltaTime, world);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

		internal void OnBeforeRender(float deltaTime, ECSWorld world)
		{
			foreach (var system in beforeRenderSystems)
			{
				try
				{
					if (system.system.Enabled)
					{
						system.system.Update(deltaTime, world);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

		internal void OnRender(float deltaTime, ECSWorld world)
		{
			foreach (var system in renderSystems)
			{
				try
				{
					if (system.system.Enabled)
					{
						system.system.Update(deltaTime, world);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

		internal void OnAfterRender(float deltaTime, ECSWorld world)
		{
			foreach (var system in afterRenderSystems)
			{
				try
				{
					if (system.system.Enabled)
					{
						system.system.Update(deltaTime, world);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}
	}
}
