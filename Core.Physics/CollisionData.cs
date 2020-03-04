using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BepuPhysics.CollisionDetection;

namespace Core.Physics
{
	public struct CollisionData {
		public Vector3 offset;
		public Vector3 normal;
		public float depth;
		public int featureId;
	}
}
