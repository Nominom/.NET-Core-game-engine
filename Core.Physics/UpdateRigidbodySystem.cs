using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BepuUtilities;
using Core.ECS;
using Core.ECS.Components;
using GlmSharp;

namespace Core.Physics
{
	[ECSSystem(updateEvent:UpdateEvent.FixedUpdate, updateBefore:typeof(PhysicsSystem))]
	public class UpdateRigidbodySystem : ComponentSystem {
		public const float Threshold = 0.00001f;

		public override ComponentQuery GetQuery() {
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<RigidBody>();
			query.IncludeReadonly<InternalRigidBodyHandle>();
			query.IncludeReadonly<Position>();
			query.IncludeReadonly<Rotation>();
			query.IncludeReadonly<Velocity>();
			query.IncludeReadonly<AngularVelocity>();

			return query;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block) {
			var rbs = block.GetReadOnlyComponentData<RigidBody>();
			var rbh = block.GetReadOnlyComponentData<InternalRigidBodyHandle>();
			var pos = block.GetReadOnlyComponentData<Position>();
			var rot = block.GetReadOnlyComponentData<Rotation>();
			var vel = block.GetReadOnlyComponentData<Velocity>();
			var avel = block.GetReadOnlyComponentData<AngularVelocity>();

			for (int i = 0; i < block.length; i++) {
				var reference = PhysicsSystem.Simulation.Bodies.GetBodyReference(rbh[i].rigidBodyHandle);

				if (vec3.DistanceSqr(rbs[i].lastPosition, pos[i].value) > Threshold)
				{
					reference.Pose.Position = new Vector3(pos[i].value.x, pos[i].value.y, pos[i].value.z);
				}
				if (vec3.DistanceSqr(rbs[i].lastLinearVel, vel[i].value) > Threshold)
				{
					reference.Velocity.Linear = new Vector3(vel[i].value.x, vel[i].value.y, vel[i].value.z);
				}
				if (vec3.DistanceSqr(rbs[i].lastAngularVel, avel[i].value) > Threshold)
				{
					reference.Velocity.Angular = new Vector3(avel[i].value.x, avel[i].value.y, avel[i].value.z);
				}
				if (quat.Dot(rbs[i].lastRotation, rot[i].value) < 1 - Threshold)
				{
					reference.Pose.Orientation = new BepuUtilities.Quaternion(rot[i].value.x, rot[i].value.y, rot[i].value.z, rot[i].value.w);
				}
			}
		}
	}
}
