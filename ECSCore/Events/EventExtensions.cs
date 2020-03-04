using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Events
{
	public static class EventExtensions
	{
		public static void Fire<T>(this T _event, ECSWorld world) where T : struct, IEvent {
			world.EventManager.QueueEvent(_event);
		}
	}
}
