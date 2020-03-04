﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Events
{

	public sealed class EventManager{
		internal Dictionary<int, IEventManager> eventManagers = new Dictionary<int, IEventManager>();
		internal ECSWorld world;
		private List<ISystem> systems;
		internal EventManager(ECSWorld world) {
			this.world = world;
		}

		public void UpdateSubscribers(List<ISystem> systems) {
			this.systems = systems;
			foreach (var manager in eventManagers) {
				manager.Value.UpdateSubscribers(this.systems);
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
			}
			((EventManager<T>) manager).QueueEvent(_event);
		}

		public void DeliverEvents()
		{
			foreach (var manager in eventManagers) {
				manager.Value.DeliverEvents();
				manager.Value.ClearEvents();
			}
		}
	}

	internal interface IEventManager {
		void UpdateSubscribers(List<ISystem> systems);
		void DeliverEvents();
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

		public void DeliverEvents() {
			if (numEvents > 0) {
				foreach (var subscriber in subscribers) {
					ReadOnlySpan<T> events = waitingEvents;
					events = events.Slice(0, numEvents);
					subscriber.ProcessEvents(events);
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
