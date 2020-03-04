using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.ECS;
using Core.ECS.Events;
using Xunit;

namespace CoreTests
{
	public class ComponentEventTests : IDisposable
	{
		public ECSWorld world;
		public EventHandlerSystem eventSystem;

		public class EventHandlerSystem : ISystem, IEventHandler<ComponentAddedEvent<TestComponent1>>, IEventHandler<ComponentRemovedEvent<TestComponent1>> {
			public bool Enabled { get; set; }

			public List<ComponentAddedEvent<TestComponent1>> createdEvents = new List<ComponentAddedEvent<TestComponent1>>();
			public List<ComponentRemovedEvent<TestComponent1>> destroyedEvents = new List<ComponentRemovedEvent<TestComponent1>>();

			public void OnCreateSystem(ECSWorld world) {
			}

			public void OnDestroySystem(ECSWorld world) {
			}

			public void Update(float deltaTime, ECSWorld world) {
			}

			public void ProcessEvents(ReadOnlySpan<ComponentAddedEvent<TestComponent1>> events) {
				createdEvents.AddRange(events.ToArray());
			}

			public void ProcessEvents(ReadOnlySpan<ComponentRemovedEvent<TestComponent1>> events) {
				destroyedEvents.AddRange(events.ToArray());
			}
		}

		public ComponentEventTests() {
			world = new ECSWorld(false);
			world.Initialize(false);
			eventSystem = new EventHandlerSystem();

			world.SystemManager.RegisterSystem(eventSystem);
		}

		[Fact]
		public void CreateEvent()
		{
			EntityArchetype archetype = EntityArchetype.Empty.Add<TestComponent1>();
			var entity = world.EntityManager.CreateEntity(archetype);
			world.EventManager.DeliverEvents();

			Assert.Equal(eventSystem.createdEvents[0].entity, entity);
		}

		[Fact]
		public void AddEvent()
		{

			var entity = world.EntityManager.CreateEntity();

			world.ComponentManager.AddComponent(entity, new TestComponent1());
			world.EventManager.DeliverEvents();

			Assert.Equal(eventSystem.createdEvents[0].entity, entity);
		}

		[Fact]
		public void CreateManyEvent() {
			EntityArchetype archetype = EntityArchetype.Empty.Add<TestComponent1>();
			var entities = world.EntityManager.CreateEntities(100, archetype);
			world.EventManager.DeliverEvents();

			Assert.True(entities.Intersect(eventSystem.createdEvents.Select(x => x.entity)).Count() == entities.Length);
		}

		[Fact]
		public void DestroyEvent()
		{
			
			var entity = world.EntityManager.CreateEntity();
			world.ComponentManager.AddComponent(entity, new TestComponent1());
			world.EntityManager.DestroyEntity(entity);

			world.EventManager.DeliverEvents();

			Assert.Single(eventSystem.createdEvents);
			Assert.Equal(eventSystem.createdEvents[0].entity, entity);

			Assert.Single(eventSystem.destroyedEvents);
			Assert.Equal(eventSystem.destroyedEvents[0].entity, entity);
		}

		[Fact]
		public void RemoveEvent()
		{
			EntityArchetype archetype = EntityArchetype.Empty.Add<TestComponent1>();
			var entity = world.EntityManager.CreateEntity(archetype);

			world.ComponentManager.RemoveComponent<TestComponent1>(entity);
			world.EventManager.DeliverEvents();

			Assert.Single(eventSystem.createdEvents);
			Assert.Equal(eventSystem.createdEvents[0].entity, entity);

			Assert.Single(eventSystem.destroyedEvents);
			Assert.Equal(eventSystem.destroyedEvents[0].entity, entity);
		}

		public void Dispose() {
			world.CleanUp();
		}
	}
}
