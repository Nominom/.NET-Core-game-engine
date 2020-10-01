using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using GlmSharp;

namespace TestApp
{
	public struct FollowTarget : IComponent {
		public Entity target;
		public vec3 offset;
		public vec3 angleOffset;
	}

	public class FollowTargetSystem : WorldComponentSystem{
		public override ComponentQuery GetQuery() {
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<FollowTarget>();
			query.IncludeReadWrite<Position>();
			query.IncludeReadWrite<Rotation>();
			return query;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block, ECSWorld world) {
			var positions = block.GetComponentData<Position>();
			var follows = block.GetReadOnlyComponentData<FollowTarget>();
			var rotations = block.GetComponentData<Rotation>();

			for (int i = 0; i < block.length; i++) {
				if (!world.ComponentManager.TryGetComponent(follows[i].target, out Position targetPos)) continue;
				if (!world.ComponentManager.TryGetComponent(follows[i].target, out Rotation targetRot)) {
					targetRot = new Rotation();
				}

				var p = targetPos.value + follows[i].offset;
				var rOff = new quat(follows[i].angleOffset);
				var r = targetRot.value * rOff;

				positions[i].value = p;
				rotations[i].value = r;
			}
		}
	}
}
