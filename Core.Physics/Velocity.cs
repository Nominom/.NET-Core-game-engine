using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using GlmSharp;

namespace Core.Physics
{
	public struct Velocity : IComponent {
		public vec3 value;
	}

	public struct AngularVelocity : IComponent {
		public vec3 value;
	}
}
