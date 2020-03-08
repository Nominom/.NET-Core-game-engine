using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Core.ECS.Events;
using Core.Physics;

namespace TestApp
{
	[ECSSystem(UpdateEvent.Update)]
	public class CollisionEventConsumer : ISystem, IEventHandler<CollisionEnterEvent>, IEventHandler<CollisionExitEvent>
	{
		public bool Enabled { get; set; }

		public void OnCreateSystem(ECSWorld world) {
			Console.WriteLine("System created");
		}

		public void OnDestroySystem(ECSWorld world) {}

		public void Update(float deltaTime, ECSWorld world) {}
		public void ProcessEvents(ECSWorld world, ReadOnlySpan<CollisionEnterEvent> events) {
			foreach (var _event in events) {
				Console.WriteLine($"Collision between {_event.A.ToString()} and {_event.B.ToString()} started.");
				_event.A.Destroy(world);
			}
		}

		public void ProcessEvents(ECSWorld world, ReadOnlySpan<CollisionExitEvent> events) {
			foreach (var _event in events) {
				Console.WriteLine($"Collision between {_event.A.ToString()} and {_event.B.ToString()} ended.");
			}
		}
	}
}
