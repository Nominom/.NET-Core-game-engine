using System;
using System.Collections.Generic;
using System.Text;
using Core.Profiling;

namespace Core.ECS.Events
{
	public sealed class EventManager{
		internal Dictionary<int, IEventManager> eventManagers = new Dictionary<int, IEventManager>();
		internal List<IEventManager> allEventManagers = new List<IEventManager>();
		internal ECSWorld world;
		private List<ISystem> systems;
		internal EventManager(ECSWorld world) {
			this.world = world;
		}

		public void UpdateSubscribers(List<ISystem> systems) {
			this.systems = systems;
			foreach (var manager in allEventManagers) {
				manager.UpdateSubscribers(this.systems);
			}
		}

		public void QueueEvent<T>(T _event) where T : struct, IEvent {
			DebugHelper.AssertThrow<ThreadAccessException>(ECSWorld.CheckThreadIsMainThread());
			int hash = TypeHelper<T>.hashCode;
			if (!eventManagers.TryGetValue(hash, out IEventManager manager)) {
				manager = new EventManager<T>();
				if (systems != null) {
					manager.UpdateSubscribers(systems);
				}
				eventManagers.Add(hash, manager);
				allEventManagers.Add(manager);
			}
			((EventManager<T>) manager).QueueEvent(_event);
		}

		public void DeliverEvents()
		{
			world.SyncPoint();
			for (int i = 0; i < allEventManagers.Count; i++) {
				var manager = allEventManagers[i];
				manager.DeliverEvents(world);
				manager.ClearEvents();
			}
		}
	}

	internal interface IEventManager {
		void UpdateSubscribers(List<ISystem> systems);
		void DeliverEvents(ECSWorld world);
		void ClearEvents();
	}
	internal sealed class EventManager<T> : IEventManager where T : struct, IEvent {
		
		internal List<IEventHandler<T>> subscribers = new List<IEventHandler<T>>();

		private T[] waitingEvents = Array.Empty<T>();
		private int numEvents = 0;

		internal EventManager(){}

		private void GrowEvents() {
			Span<T> old = waitingEvents;
			T[] newArray = new T[waitingEvents.Length == 0? 2 : waitingEvents.Length * 2];
			Span<T> newS = newArray;
			old.CopyTo(newS);
			waitingEvents = newArray;
		}

		public void QueueEvent(T _event) {
			if (subscribers.Count == 0) {
				return;
			}
			if (numEvents >= waitingEvents.Length) {
				GrowEvents();
			}
			waitingEvents[numEvents] = _event;
			numEvents++;
		}

		public void UpdateSubscribers(List<ISystem> systems) {
			subscribers.Clear();
			foreach (ISystem system in systems) {
				if (system is IEventHandler<T> handler) {
					subscribers.Add(handler);
				}
			}
		}

		public void DeliverEvents(ECSWorld world) {
			if (numEvents > 0) {
				ReadOnlySpan<T> events = waitingEvents;
				events = events.Slice(0, numEvents);

				foreach (var subscriber in subscribers) {
					Profiler.StartMethod(subscriber.GetType().Name);
					subscriber.ProcessEvents(world, events);
					Profiler.EndMethod();
				}
			}
		}

		public void ClearEvents() {
			if (numEvents > 0) {
				Span<T> events = waitingEvents;
				events = events.Slice(0, numEvents);
				events.Clear();
			}
			numEvents = 0;
		}
	}
}
