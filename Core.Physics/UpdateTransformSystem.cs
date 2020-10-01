using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Core.ECS.Filters;
using GlmSharp;

namespace Core.Physics
{
	[ECSSystem(updateEvent:UpdateEvent.FixedUpdate, 
		updateAfter:typeof(PhysicsSystem), 
		updateBefore:typeof(PhysicsBodyInitializerSystem))]
	public class UpdateTransformSystem : JobComponentSystem
	{
		public override ComponentQuery GetQuery() {
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<InternalRigidBodyHandle>();
			query.IncludeReadWrite<RigidBody>();
			query.IncludeReadWrite<Position>();
			query.IncludeReadWrite<Rotation>();
			query.IncludeReadWrite<Velocity>();
			query.IncludeReadWrite<AngularVelocity>();

			return query;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block) {
			var rbs = block.GetComponentData<RigidBody>();
			var rbh = block.GetReadOnlyComponentData<InternalRigidBodyHandle>();
			var pos = block.GetComponentData<Position>();
			var rot = block.GetComponentData<Rotation>();
			var vel = block.GetComponentData<Velocity>();
			var avel = block.GetComponentData<AngularVelocity>();

			for (int i = 0; i < block.length; i++) {
				var reference = PhysicsSystem.Simulation.Bodies.GetBodyReference(rbh[i].rigidBodyHandle);
				var p = reference.Pose.Position;
				var r = reference.Pose.Orientation;
				var lv = reference.Velocity.Linear;
				var av = reference.Velocity.Angular;


				pos[i].value = new vec3(p.X, p.Y, p.Z);
				rot[i].value = new quat(r.X, r.Y, r.Z, r.W);
				vel[i].value = new vec3(lv.X, lv.Y, lv.Z);
				avel[i].value = new vec3(av.X, av.Y, av.Z);

				rbs[i].lastPosition = pos[i].value;
				rbs[i].lastRotation = rot[i].value;
				rbs[i].lastLinearVel = vel[i].value;
				rbs[i].lastAngularVel = avel[i].value;

			}
		}
	}
}
