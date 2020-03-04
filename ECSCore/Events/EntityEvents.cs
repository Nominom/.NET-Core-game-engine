using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Events
{
	public struct EntityCreatedEvent : IEvent {
		public Entity entity;
		public EntityCreatedEvent(Entity entity) {
			this.entity = entity;
		}
	}

	public struct EntityDestroyedEvent : IEvent {
		public Entity entity;
		public EntityDestroyedEvent(Entity entity) {
			this.entity = entity;
		}
	}
}
