using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Core.Shared;

namespace Core.ECS.Components
{
	public struct Position : IComponent {
		public Vector3 value;
	}

	public struct Scale : IComponent {
		public Vector3 value;
	}

	public struct Rotation : IComponent {
		public Quaternion value;
	}

	public struct ObjectToWorld : IComponent {
		public Matrix4x4 model; //Model matrix
		public Matrix3x3 normal; //Normal matrix
	}
}
