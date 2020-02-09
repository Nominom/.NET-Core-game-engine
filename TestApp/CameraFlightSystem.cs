using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics;
using GlmSharp;
using Veldrid;

namespace TestApp
{
	[ECSSystem(UpdateEvent.Update)]
	public class CameraFlightSystem : ComponentSystem
	{
		public override ComponentQuery GetQuery() {
			var query = new ComponentQuery();
			query.IncludeShared<Camera>();
			query.IncludeReadWrite<Position>();
			query.IncludeReadWrite<Rotation>();
			return query;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block) {
			var positions = block.GetComponentData<Position>();
			var rotations = block.GetComponentData<Rotation>();

			float flightSpeed = 5;
			for (int i = 0; i < block.length; i++) {
				if (Input.GetKey(Key.Space)) {
					vec2 mouseDelta = Input.GetMouseDelta() * 0.002f;
					dvec3 euler = rotations[i].value.EulerAngles;
					euler.x += mouseDelta.y;
					euler.y += mouseDelta.x;


					//rotations[i].value = new quat(new vec3((float) euler.x, (float) euler.y, (float) euler.z));

					quat inverse = rotations[i].value.Inverse;
					quat x = quat.FromAxisAngle(- mouseDelta.x, inverse * vec3.UnitY);
					quat y = quat.FromAxisAngle(mouseDelta.y, vec3.UnitX);
					rotations[i].value = rotations[i].value * x * y;
				}

				//if (Input.GetKey(Key.W)) {
				//	var dir = Vector3.Transform(Vector3.UnitZ, rotations[i].value);
				//	positions[i].value += dir * deltaTime * flightSpeed;
				//}
				//if (Input.GetKey(Key.S)) {
				//	var dir = Vector3.Transform(-Vector3.UnitZ, rotations[i].value);
				//	positions[i].value += dir * deltaTime * flightSpeed;
				//}
				//if (Input.GetKey(Key.A)) {
				//	var dir = Vector3.Transform(Vector3.UnitX, rotations[i].value);
				//	positions[i].value += dir * deltaTime * flightSpeed;
				//}
				//if (Input.GetKey(Key.D)) {
				//	var dir = Vector3.Transform(-Vector3.UnitX, rotations[i].value);
				//	positions[i].value += dir * deltaTime * flightSpeed;
				//}

				if (Input.GetKey(Key.W)) {
					var dir = rotations[i].value * vec3.UnitZ;
					positions[i].value += dir * deltaTime * flightSpeed;
				}
				if (Input.GetKey(Key.S))
				{
					var dir = rotations[i].value * -vec3.UnitZ;
					positions[i].value += dir * deltaTime * flightSpeed;
				}
				if (Input.GetKey(Key.A))
				{
					var dir = rotations[i].value * vec3.UnitX;
					positions[i].value += dir * deltaTime * flightSpeed;
				}
				if (Input.GetKey(Key.D))
				{
					var dir = rotations[i].value * -vec3.UnitX;
					positions[i].value += dir * deltaTime * flightSpeed;
				}

			}
		}
	}
}
