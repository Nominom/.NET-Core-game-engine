using System;
using System.Collections.Generic;
using System.Text;
using BepuPhysics.CollisionDetection;
using BepuPhysics.CollisionDetection.CollisionTasks;
using Core.ECS;
using Core.ECS.Events;

namespace Core.Physics
{
	internal struct CollisionEventData {
		public CollidablePair pair;
		public Memory<CollisionData> collisionMemory;
	}

	public struct CollisionEvent : IEvent {
		public Entity A;
		public Entity B;
		internal Memory<CollisionData> collisions;
		public Span<CollisionData> Collisions => collisions.Span;
	}

	public struct CollisionEnterEvent : IEvent {
		public Entity A;
		public Entity B;
		internal Memory<CollisionData> collisions;
		public Span<CollisionData> Collisions => collisions.Span;
	}

	public struct CollisionExitEvent : IEvent {
		public Entity A;
		public Entity B;
	}
}
