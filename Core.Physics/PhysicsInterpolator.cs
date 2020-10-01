using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Core.Shared;
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
			
			if (Fma.IsSupported && Avx.IsSupported) {
				unsafe {
					Vector128<float> deltaF = Vector128.Create(deltaTime);
					fixed (float* oldPosFloats = pos.Cast<Position, float>()) 
					fixed (float* posFloats = pos.Cast<Position, float>()) 
					fixed (float* velFloats = vel.Cast<Velocity, float>()) {
						int i = 0;
						for(; i < block.length; i += 4) {
							var op = Sse.LoadAlignedVector128(&oldPosFloats[i]);
							var p = Sse.LoadAlignedVector128(&posFloats[i]);
							var v = Sse.LoadAlignedVector128(&velFloats[i]);
							var result = Fma.MultiplyAdd(deltaF, v, p);
							var bools = Sse.CompareEqual(op, p);

							Avx.MaskStore(&posFloats[i], bools, result);
						}

						for (i -= 4; i < block.length; i++) {
							if (oldPosFloats[i] == posFloats[i]) {
								posFloats[i] = posFloats[i] + velFloats[i] * deltaTime;
							}
						}
					}
				}
				
				for (int i = 0; i < block.length; i++) {
					if (pos[i].value == rbs[i].lastPosition && rot[i].value == rbs[i].lastRotation) {
						quat x = quat.FromAxisAngle(avel[i].value.x * deltaTime, vec3.UnitX);
						quat y = quat.FromAxisAngle(avel[i].value.y * deltaTime, vec3.UnitY);
						quat z = quat.FromAxisAngle(avel[i].value.z * deltaTime, vec3.UnitZ);
						rot[i].value = rot[i].value * x * y * z;

						rbs[i].lastPosition = pos[i].value;
						rbs[i].lastRotation = rot[i].value;
					}
				}

				
			}
			else {
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
}
