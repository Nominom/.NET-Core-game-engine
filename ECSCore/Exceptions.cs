using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS
{
	public class InvalidEntityException : Exception {
	}

	public class ComponentNotFoundException : Exception {
	}

	public class ThreadAccessException : Exception {
		public override string Message { get; } = "This method can only be accessed from the main thread.";
	}

	public class IllegalAccessException : Exception {
		
	}
}
