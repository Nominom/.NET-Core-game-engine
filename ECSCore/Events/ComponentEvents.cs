using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Core.ECS.Events
{
	

	internal static class ComponentEventHelper
	{
		private static Dictionary<System.Type, IComponentEventCreator> FindComponentListeners() {
			var componentListeners = new Dictionary<Type, IComponentEventCreator>();

			foreach (var assembly in AssemblyHelper.GetAllUserAssemblies()) {

				var typesWithComponentHandlers = assembly.GetTypes().Where(x =>
					x.GetInterfaces().Any(
						y =>
						y.IsGenericType && 
						y.GetGenericTypeDefinition() == typeof(IEventHandler<>) && 
						y.GetGenericArguments().Any(z =>
							z.IsGenericType &&
							(
							z.GetGenericTypeDefinition() == typeof(ComponentAddedEvent<>) ||
							z.GetGenericTypeDefinition() == typeof(ComponentRemovedEvent<>)
							)
							)
						)
					);
				foreach (Type type in typesWithComponentHandlers) {
					var interfaces = type.GetInterfaces().Where(x =>
						x.IsGenericType &&
						x.GetGenericTypeDefinition() == typeof(IEventHandler<>) &&
						x.GetGenericArguments().Any(z =>
							z.IsGenericType &&
							(
								z.GetGenericTypeDefinition() == typeof(ComponentAddedEvent<>) ||
								z.GetGenericTypeDefinition() == typeof(ComponentRemovedEvent<>)
							)
						)
					);

					foreach (var @interface in interfaces) {
						var componentType = @interface.GetGenericArguments().First()
							.GetGenericArguments().First();

						if (!componentListeners.TryGetValue(componentType, out var value)) {
							var newType = typeof(ComponentEventCreator<>).MakeGenericType(componentType);
							value = Activator.CreateInstance(newType) as IComponentEventCreator;
							componentListeners.Add(componentType, value);
						}
					}

				}
				
			}

			return componentListeners;
		}

		private static Dictionary<System.Type, IComponentEventCreator> eventCreators = FindComponentListeners();

		internal static void FireComponentAddedEvent(ECSWorld world, Entity entity, System.Type componentType)
		{
			if(eventCreators.TryGetValue(componentType, out var creator))
			{
				creator.FireComponentAddedEvent(world, entity);
			}
		}

		internal static void FireComponentRemovedEvent(ECSWorld world, Entity entity, System.Type componentType)
		{
			if(eventCreators.TryGetValue(componentType, out var creator))
			{
				creator.FireComponentRemovedEvent(world, entity);
			}
		}

		private interface IComponentEventCreator {
			void FireComponentAddedEvent(ECSWorld world, Entity entity);
			void FireComponentRemovedEvent(ECSWorld world, Entity entity);
		}

		private class ComponentEventCreator<T> : IComponentEventCreator where T : unmanaged, IComponent
		{
			public void FireComponentAddedEvent(ECSWorld world, Entity entity) {
				new ComponentAddedEvent<T>(entity).Fire(world);
			}

			public void FireComponentRemovedEvent(ECSWorld world, Entity entity) {
				new ComponentRemovedEvent<T>(entity).Fire(world);
			}
		}
	}

	public struct ComponentAddedEvent<T> : IEvent where T : unmanaged, IComponent {
		public Entity entity;
		public ComponentAddedEvent(Entity entity) {
			this.entity = entity;
		}
	}

	public struct ComponentRemovedEvent<T> : IEvent where T : unmanaged, IComponent {
		public Entity entity;
		public ComponentRemovedEvent(Entity entity) {
			this.entity = entity;
		}
	}
}
