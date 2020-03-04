using System;
using System.Collections.Generic;
using System.Text;
using BepuPhysics.Collidables;
using Core.ECS;
using GlmSharp;

namespace Core.Physics
{

	public enum CollisionDetectionMode {
		Discrete,
		Passive,
		Continuous
	}

	public struct RigidBody : IComponent {
		public float mass;
		public CollisionDetectionMode detectionMode;
		internal vec3 lastPosition;
		internal quat lastRotation;
		internal vec3 lastLinearVel;
		internal vec3 lastAngularVel;
	}

	public struct StaticRigidBody : IComponent { }

	internal struct InternalRigidBodyHandle : IComponent {
		public int rigidBodyHandle;
	}

	internal struct InternalStaticBodyHandle : IComponent {
		public int staticBodyHandle;
	}
}
