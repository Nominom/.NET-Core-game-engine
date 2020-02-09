using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Core.Shared;
using GlmSharp;

namespace Core.ECS.Components
{
	public struct Position : IComponent {
		public vec3 value;
	}

	public struct Scale : IComponent {
		public vec3 value;
	}

	public struct Rotation : IComponent {
		public quat value;
	}

	public struct ObjectToWorld : IComponent {
		public mat4 model; //Model matrix
		public mat3 normal; //Normal matrix
	}

	public struct BoundingBox : IComponent {
		public AabbBounds value;
	}
}
