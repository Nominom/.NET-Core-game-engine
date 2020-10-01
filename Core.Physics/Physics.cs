using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using Core.ECS;
using Core.Shared;
using GlmSharp;

namespace Core.Physics
{
	public static class Physics
	{
		public static PhysicsSettings Settings { get; set; } = new PhysicsSettings();

		private static RayHit[] hitBuffer = new RayHit[16384];
		private static int nextBufferIndex = 0;

		public static ReadOnlySpan<RayHit> RayCast(vec3 origin, vec3 direction, float maxDistance)
		{
			var hitHandler = new RayCastHitHandler();
			try
			{


				PhysicsSystem.Simulation.RayCast(new Vector3(origin.x, origin.y, origin.z),
					new Vector3(direction.x, direction.y, direction.z), maxDistance, ref hitHandler);

				if (hitHandler.hits == null || hitHandler.hits.Count == 0)
				{
					return ReadOnlySpan<RayHit>.Empty;
				}
				else
				{
					var length = hitHandler.hits.Count;
					int bufferIndexStart = Interlocked.Add(ref nextBufferIndex, length) - length;

					if (bufferIndexStart + length > hitBuffer.Length)
					{
						throw new IndexOutOfRangeException("No more space to store raycasts this frame");
					}

					hitHandler.hits.CopyTo(hitBuffer, bufferIndexStart);

					return new ReadOnlySpan<RayHit>(hitBuffer, bufferIndexStart, length);
				}
			}
			finally
			{
				hitHandler.hits?.Dispose();
			}
		}

		internal static void ResetFrame()
		{
			nextBufferIndex = 0;
		}

		private struct RayCastHitHandler : IRayHitHandler
		{
			public PooledList<RayHit> hits;

			public bool AllowTest(CollidableReference collidable)
			{
				return true;
			}

			public bool AllowTest(CollidableReference collidable, int childIndex)
			{
				return true;
			}

			public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable,
				int childIndex)
			{
				if (hits == null)
				{
					hits = PooledList<RayHit>.Create();
				}

				var hitPosition = ray.Origin + ray.Direction * t;
				Entity entity;
				if (collidable.Mobility == CollidableMobility.Static)
				{
					entity = PhysicsSystem.staticBodyEntityDictionary[collidable.Handle];
				}
				else
				{
					entity = PhysicsSystem.rigidBodyEntityDictionary[collidable.Handle];
				}

				var rayHit = new RayHit()
				{
					entity = entity,
					hitNormal = new vec3(normal.X, normal.Y, normal.Z),
					hitPosition = new vec3(hitPosition.X, hitPosition.Y, hitPosition.Z),
					t = t
				};

				hits.Add(rayHit);
			}

		}
	}

	public struct RayHit
	{
		public Entity entity;
		public float t;
		public vec3 hitPosition;
		public vec3 hitNormal;
	}

	public class PhysicsSettings
	{
		public Vector3 Gravity = Vector3.UnitY * -9;
		public float LinearDamping = .03f;
		public float AngularDamping = .03f;
		public int solverIterationCount = 6;
	}
}
