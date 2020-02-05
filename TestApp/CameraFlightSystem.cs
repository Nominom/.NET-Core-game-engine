using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics;
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
					Vector2 mouseDelta = Input.GetMouseDelta() * 0.002f;
					Quaternion quat = Quaternion.CreateFromYawPitchRoll(mouseDelta.X, mouseDelta.Y, 0);
					rotations[i].value = Quaternion.Multiply(rotations[i].value, quat);
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

				if (Input.GetKey(Key.W))
				{
					var dir = Vector3.Transform(Vector3.Normalize( - Vector3.UnitZ - Vector3.UnitY), rotations[i].value);
					positions[i].value += dir * deltaTime * flightSpeed;
				}
				if (Input.GetKey(Key.S))
				{
					var dir = Vector3.Transform(-Vector3.UnitZ, rotations[i].value);
					positions[i].value += dir * deltaTime * flightSpeed;
				}
				if (Input.GetKey(Key.A))
				{
					var dir = Vector3.Transform(Vector3.UnitX, rotations[i].value);
					positions[i].value += dir * deltaTime * flightSpeed;
				}
				if (Input.GetKey(Key.D))
				{
					var dir = Vector3.Transform(-Vector3.UnitX, rotations[i].value);
					positions[i].value += dir * deltaTime * flightSpeed;
				}

			}
		}
	}
}
