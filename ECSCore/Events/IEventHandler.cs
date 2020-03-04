using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Events
{
	public interface IEventHandler<T> where T : struct, IEvent {
		void ProcessEvents(ReadOnlySpan<T> events);
	}
}
