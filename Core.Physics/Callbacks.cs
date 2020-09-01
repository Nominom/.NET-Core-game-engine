using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;

namespace Core.Physics
{
	public struct PoseIntergratorCallbacks : IPoseIntegratorCallbacks
	{
		Vector3 gravityDt;
		float linearDampingDt;
		float angularDampingDt;

		public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;


		public void PrepareForIntegration(float dt)
		{
			//No reason to recalculate gravity * dt for every body; just cache it ahead of time.
			gravityDt = Physics.Settings.Gravity * dt;
			//Since this doesn't use per-body damping, we can precalculate everything.
			linearDampingDt = MathF.Pow(MathHelper.Clamp(1 - Physics.Settings.LinearDamping, 0, 1), dt);
			angularDampingDt = MathF.Pow(MathHelper.Clamp(1 - Physics.Settings.AngularDamping, 0, 1), dt);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IntegrateVelocity(int bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity)
		{
			//Note that we avoid accelerating kinematics. Kinematics are any body with an inverse mass of zero (so a mass of ~infinity). No force can move them.
			if (localInertia.InverseMass > 0)
			{
				velocity.Linear = (velocity.Linear + gravityDt) * linearDampingDt;
				velocity.Angular = velocity.Angular * angularDampingDt;
			}
			//Implementation sidenote: Why aren't kinematics all bundled together separately from dynamics to avoid this per-body condition?
			//Because kinematics can have a velocity- that is what distinguishes them from a static object. The solver must read velocities of all bodies involved in a constraint.
			//Under ideal conditions, those bodies will be near in memory to increase the chances of a cache hit. If kinematics are separately bundled, the the number of cache
			//misses necessarily increases. Slowing down the solver in order to speed up the pose integrator is a really, really bad trade, especially when the benefit is a few ALU ops.

			//Note that you CAN technically modify the pose in IntegrateVelocity by directly accessing it through the Simulation.Bodies.ActiveSet.Poses, it just requires a little care and isn't directly exposed.
			//If the PositionFirstTimestepper is being used, then the pose integrator has already integrated the pose.
			//If the PositionLastTimestepper or SubsteppingTimestepper are in use, the pose has not yet been integrated.
			//If your pose modification depends on the order of integration, you'll want to take this into account.

			//This is also a handy spot to implement things like position dependent gravity or per-body damping.
		}

	}
	public unsafe struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
	{
		public SpringSettings ContactSpringiness;

		public void Initialize(Simulation simulation)
		{
			//Register list in PhysicsSystem
			_ = PhysicsSystem.collisionsToFire.Value;

			//Use a default if the springiness value wasn't initialized.
			if (ContactSpringiness.AngularFrequency == 0 && ContactSpringiness.TwiceDampingRatio == 0)
				ContactSpringiness = new SpringSettings(30, 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b)
		{
			//While the engine won't even try creating pairs between statics at all, it will ask about kinematic-kinematic pairs.
			//Those pairs cannot emit constraints since both involved bodies have infinite inertia. Since most of the demos don't need
			//to collect information about kinematic-kinematic pairs, we'll require that at least one of the bodies needs to be dynamic.
			return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
		{
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : struct, IContactManifold<TManifold>
		{
			pairMaterial.FrictionCoefficient = 1f;
			pairMaterial.MaximumRecoveryVelocity = 2f;
			pairMaterial.SpringSettings = ContactSpringiness;

			if (manifold.Count > 0)
			{
				Memory<CollisionData> data = PhysicsSystem.collisionDataPool.GetMemory(manifold.Count);
				var datas = data.Span;
				for (int i = 0; i < manifold.Count; i++)
				{
					CollisionData collision = new CollisionData();
					manifold.GetContact(i, out collision.offset, out collision.normal,
						out collision.depth, out collision.featureId);
					datas[i] = collision;
				}

				PhysicsSystem.collisionsToFire.Value.Add(new CollisionEventData()
				{
					pair = pair,
					collisionMemory = data
				});
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
		{
			return true;
		}

		public void Dispose()
		{
		}
	}
}
