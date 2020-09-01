using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using GlmSharp;

namespace Core.Physics
{
	public static class Physics
	{
		public static PhysicsSettings Settings { get; set; } = new PhysicsSettings();
	}

	public class PhysicsSettings
	{
		public Vector3 Gravity = Vector3.UnitY * -9;
		public float LinearDamping = .03f;
		public float AngularDamping = .03f;
	}
}
