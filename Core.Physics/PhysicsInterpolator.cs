using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using GlmSharp;

namespace Core.Physics
{
	[ECSSystem(updateEvent:UpdateEvent.Update)]
	public class PhysicsInterpolator : JobComponentSystem
	{
		public override ComponentQuery GetQuery() {
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadWrite<RigidBody>();
			query.IncludeReadWrite<Position>();
			query.IncludeReadWrite<Rotation>();
			query.IncludeReadonly<Velocity>();
			query.IncludeReadonly<AngularVelocity>();
			return query;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block) {
			var rbs = block.GetComponentData<RigidBody>();
			var pos = block.GetComponentData<Position>();
			var rot = block.GetComponentData<Rotation>();
			var vel = block.GetReadOnlyComponentData<Velocity>();
			var avel = block.GetReadOnlyComponentData<AngularVelocity>();
			for (int i = 0; i < block.length; i++) {
				if (pos[i].value == rbs[i].lastPosition && rot[i].value == rbs[i].lastRotation) {
					pos[i].value += vel[i].value * deltaTime;

					quat x = quat.FromAxisAngle(avel[i].value.x * deltaTime, vec3.UnitX);
					quat y = quat.FromAxisAngle(avel[i].value.y * deltaTime, vec3.UnitY);
					quat z = quat.FromAxisAngle(avel[i].value.z * deltaTime, vec3.UnitZ);
					rot[i].value = rot[i].value * x * y * z;

					rbs[i].lastPosition = pos[i].value;
					rbs[i].lastRotation = rot[i].value;
				}
			}
		}
	}
}
