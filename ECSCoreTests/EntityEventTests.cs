using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.ECS;
using Core.ECS.Events;
using Xunit;

namespace CoreTests
{
	public class EntityEventTests {
		public ECSWorld world;
		public EventHandlerSystem eventSystem;

		public class EventHandlerSystem : ISystem, IEventHandler<EntityCreatedEvent>, IEventHandler<EntityDestroyedEvent> {
			public bool Enabled { get; set; }

			public List<EntityCreatedEvent> createdEvents = new List<EntityCreatedEvent>();
			public List<EntityDestroyedEvent> destroyedEvents = new List<EntityDestroyedEvent>();

			public void OnCreateSystem(ECSWorld world) {
			}

			public void OnDestroySystem(ECSWorld world) {
			}

			public void Update(float deltaTime, ECSWorld world) {
			}

			public void ProcessEvents(ECSWorld world, ReadOnlySpan<EntityCreatedEvent> events) {
				createdEvents.AddRange(events.ToArray());
			}

			public void ProcessEvents(ECSWorld world, ReadOnlySpan<EntityDestroyedEvent> events) {
				destroyedEvents.AddRange(events.ToArray());
			}
		}

		public EntityEventTests() {
			world = new ECSWorld(false);
			world.Initialize(false);
			eventSystem = new EventHandlerSystem();

			world.SystemManager.RegisterSystem(eventSystem);
		}

		[Fact]
		public void CreateEvent()
		{

			var entity = world.EntityManager.CreateEntity();

			world.EventManager.DeliverEvents();

			Assert.Single(eventSystem.createdEvents);
			Assert.Equal(eventSystem.createdEvents[0].entity, entity);
		}

		[Fact]
		public void CreateManyEvent() {
			var entities = world.EntityManager.CreateEntities(100);
			world.EventManager.DeliverEvents();

			Assert.True(entities.Intersect(eventSystem.createdEvents.Select(x => x.entity)).Count() == entities.Length);
		}

		[Fact]
		public void DestroyEvent()
		{
			
			var entity = world.EntityManager.CreateEntity();
			world.EntityManager.DestroyEntity(entity);

			world.EventManager.DeliverEvents();

			Assert.Single(eventSystem.createdEvents);
			Assert.Equal(eventSystem.createdEvents[0].entity, entity);

			Assert.Single(eventSystem.destroyedEvents);
			Assert.Equal(eventSystem.destroyedEvents[0].entity, entity);
		}
	}
}
