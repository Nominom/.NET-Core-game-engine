using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Events
{
	public interface IEvent
	{
	}

	public interface IDisposableEvent : IEvent, IDisposable
	{
	}
}
