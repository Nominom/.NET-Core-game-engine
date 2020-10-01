using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using GlmSharp;
using GlmSharp.Swizzle;

namespace Core.Physics
{
	public struct PhysicsBodyLockAxis : IComponent
	{
		public bool lockX;
		public bool lockY;
		public bool lockZ;

		public bool lockRotX;
		public bool lockRotY;
		public bool lockRotZ;
	}

	struct PhysicsBodyLockedValues : IComponent
	{
		public vec3 pos;
		public quat rot;
	}

	[ECSSystem(UpdateEvent.FixedUpdate, updateBefore: typeof(PhysicsBodyLockSaveSystem))]
	class PhysicsBodyLockAddComponentSystem : ComponentSystem
	{
		public override ComponentQuery GetQuery()
		{
			var query = new ComponentQuery();
			query.IncludeReadonly<PhysicsBodyLockAxis>();
			query.Exclude<PhysicsBodyLockedValues>();

			return query;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var entities = block.GetEntityData();
			for (int i = 0; i < block.length; i++)
			{
				afterUpdateCommands.AddComponent(entities[i], new PhysicsBodyLockedValues());
			}
		}
	}

	[ECSSystem(UpdateEvent.FixedUpdate, updateBefore: typeof(PhysicsSystem))]
	class PhysicsBodyLockSaveSystem : JobComponentSystem
	{
		public override ComponentQuery GetQuery()
		{
			var query = new ComponentQuery();
			query.IncludeReadonly<Position>();
			query.IncludeReadonly<Rotation>();
			query.IncludeReadWrite<PhysicsBodyLockedValues>();
			query.IncludeReadonly<PhysicsBodyLockAxis>();
			return query;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var positions = block.GetReadOnlyComponentData<Position>();
			var rotations = block.GetReadOnlyComponentData<Rotation>();
			var values = block.GetComponentData<PhysicsBodyLockedValues>();

			for (int i = 0; i < block.length; i++)
			{
				values[i].pos = positions[i].value;
				values[i].rot = rotations[i].value;
			}
		}
	}

	[ECSSystem(UpdateEvent.FixedUpdate, updateAfter: typeof(UpdateTransformSystem))]
	class PhysicsBodyLockRestoreSystem : JobComponentSystem
	{
		public override ComponentQuery GetQuery()
		{
			var query = new ComponentQuery();
			query.IncludeReadWrite<Position>();
			query.IncludeReadWrite<Rotation>();
			query.IncludeReadWrite<Velocity>();
			query.IncludeReadWrite<AngularVelocity>();
			query.IncludeReadonly<PhysicsBodyLockedValues>();
			query.IncludeReadonly<PhysicsBodyLockAxis>();
			return query;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var positions = block.GetComponentData<Position>();
			var velocities = block.GetComponentData<Velocity>();
			var rotations = block.GetComponentData<Rotation>();
			var angularVelocities = block.GetComponentData<AngularVelocity>();
			var values = block.GetReadOnlyComponentData<PhysicsBodyLockedValues>();
			var locked = block.GetReadOnlyComponentData<PhysicsBodyLockAxis>();

			for (int i = 0; i < block.length; i++)
			{
				var newPosition = positions[i].value;
				var newVelocity = velocities[i].value;
				if (locked[i].lockX)
				{
					newPosition.x = values[i].pos.x;
					newVelocity.x = 0;
				}
				if (locked[i].lockY)
				{
					newPosition.y = values[i].pos.y;
					newVelocity.y = 0;
				}
				if (locked[i].lockZ)
				{
					newPosition.z = values[i].pos.z;
					newVelocity.z = 0;
				}

				var newRot = (vec3)rotations[i].value.EulerAngles;
				var oldRot = (vec3)values[i].rot.EulerAngles;
				var newRotVel = angularVelocities[i].value;

				if (locked[i].lockRotX)
				{
					newRot.x = oldRot.x;
					newRotVel.x = 0;
				}
				if (locked[i].lockRotY)
				{
					newRot.y = oldRot.y;
					newRotVel.y = 0;
				}
				if (locked[i].lockRotZ)
				{
					newRot.z = oldRot.z;
					newRotVel.z = 0;
				}

				positions[i].value = newPosition;
				rotations[i].value = new quat(newRot);
				velocities[i].value = newVelocity;
				angularVelocities[i].value = newRotVel;
			}
		}
	}
}
