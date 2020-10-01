using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics;
using Core.Physics;
using GlmSharp;
using Veldrid;

namespace TestApp
{
	public struct Player : IComponent
	{
		public float movementSpeed;
		public float turningSpeed;
	}

	[ECSSystem(UpdateEvent.Update)]
	public class PlayerControls : ComponentSystem
	{
		public override ComponentQuery GetQuery() {
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<Player>();
			query.IncludeReadonly<Rotation>();
			query.IncludeReadWrite<Velocity>();
			query.IncludeReadWrite<AngularVelocity>();
			return query;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block) {
			var players = block.GetReadOnlyComponentData<Player>();
			var rotations = block.GetReadOnlyComponentData<Rotation>();
			var velocities = block.GetComponentData<Velocity>();
			var angularVelocities = block.GetComponentData<AngularVelocity>();

			for (int i = 0; i < block.length; i++) {


				if (Input.GetKey(Key.W)) {
					var dir = rotations[i].value * vec3.UnitY;
					velocities[i].value += dir * deltaTime * players[i].movementSpeed;
				}

				if (Input.GetKey(Key.A)) {
					angularVelocities[i].value -= vec3.UnitZ * deltaTime * players[i].turningSpeed;
				}

				if (Input.GetKey(Key.D)) {
					angularVelocities[i].value += vec3.UnitZ * deltaTime * players[i].turningSpeed;
				}
			}
		}
	}
}
