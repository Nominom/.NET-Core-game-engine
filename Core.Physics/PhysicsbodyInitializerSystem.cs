using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuUtilities;
using Core.ECS;
using Core.ECS.Components;
using Core.ECS.Filters;
using GlmSharp;

namespace Core.Physics
{
	[ECSSystem(updateEvent: UpdateEvent.FixedUpdate, updateAfter: typeof(PhysicsSystem), updateBefore:typeof(PhysicsBodyInitializerSystem))]
	public class BoxColliderInitializerSystem : ComponentSystem
	{
		public override ComponentQuery GetQuery()
		{
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<BoxCollider>();
			query.Exclude<InternalColliderHandle>();
			return query;
		}

		public override IComponentFilter GetComponentFilter()
		{
			return ComponentFilters.EntityChanged();
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var entities = block.GetEntityData();
			var colliders = block.GetReadOnlyComponentData<BoxCollider>();
			var hasRigidBody = block.TryGetComponentData(out Span<RigidBody> rbs);
			var hasScale = block.TryGetComponentData(out Span<Scale> scales);

			vec3 defaultScale = vec3.Ones;
			for (int i = 0; i < block.length; i++)
			{
				vec3 scale = hasScale ? scales[i].value : defaultScale;
	
				Box box = new Box(colliders[i].width * scale.x, colliders[i].height * scale.y, colliders[i].length * scale.z);

				TypedIndex shapeIdx = PhysicsSystem.Simulation.Shapes.Add(box);

				BodyInertia inertia = new BodyInertia();
				if (hasRigidBody)
				{
					box.ComputeInertia(rbs[i].mass, out inertia);
				}

				afterUpdateCommands.AddComponent(entities[i], 
					new InternalColliderHandle() {
						inertia = inertia,
						shapeIdx = shapeIdx
					});
			}
		}
	}

	[ECSSystem(updateEvent: UpdateEvent.FixedUpdate, updateAfter: typeof(PhysicsSystem), updateBefore:typeof(PhysicsBodyInitializerSystem))]
	public class MeshColliderInitializerSystem : ComponentSystem
	{
		public override ComponentQuery GetQuery()
		{
			ComponentQuery query = new ComponentQuery();
			query.IncludeShared<MeshCollider>();
			query.Exclude<InternalColliderHandle>();
			return query;
		}

		public override IComponentFilter GetComponentFilter()
		{
			return ComponentFilters.EntityChanged();
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var entities = block.GetEntityData();
			var collider = block.GetSharedComponentData<MeshCollider>();
			var hasRigidBody = block.TryGetComponentData(out Span<RigidBody> rbs);
			var hasScale = block.TryGetComponentData(out Span<Scale> scales);

			Vector3 defaultScale = Vector3.One;

			for (int i = 0; i < block.length; i++) {
				Vector3 scale = defaultScale;
				if (hasScale) {
					scale = new Vector3(scales[i].value.x, scales[i].value.y, scales[i].value.z);
				}
				Mesh mesh = new Mesh(collider.GetTriangles(), scale, PhysicsSystem.BufferPool);

				TypedIndex shapeIdx = PhysicsSystem.Simulation.Shapes.Add(mesh);

				BodyInertia inertia = new BodyInertia();
				if (hasRigidBody)
				{
					mesh.ComputeOpenInertia(rbs[i].mass, out inertia);
				}

				afterUpdateCommands.AddComponent(entities[i], 
					new InternalColliderHandle() {
						inertia = inertia,
						shapeIdx = shapeIdx
					});
			}
		}
	}


	[ECSSystem(updateEvent: UpdateEvent.FixedUpdate, updateAfter: typeof(PhysicsSystem))]
	public class PhysicsBodyInitializerSystem : ComponentSystem
	{
		public override ComponentQuery GetQuery()
		{
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadWrite<RigidBody>();
			query.IncludeReadonly<InternalColliderHandle>();
			query.Exclude<InternalRigidBodyHandle>();
			query.Exclude<StaticRigidBody>();
			return query;
		}

		public override IComponentFilter GetComponentFilter()
		{
			return ComponentFilters.EntityChanged();
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var entities = block.GetEntityData();
			var rbs = block.GetComponentData<RigidBody>();
			var colliders = block.GetReadOnlyComponentData<InternalColliderHandle>();

			for (int i = 0; i < block.length; i++) {

				var shapeIdx = colliders[i].shapeIdx;


				int handle;
				if (rbs[i].mass == 0) {
					afterUpdateCommands.AddComponent(entities[i],
						new InternalRigidBodyHandle()
						{
							rigidBodyHandle = handle =
								PhysicsSystem.Simulation.Bodies.Add(
									BodyDescription.CreateKinematic(RigidPose.Identity,
										new CollidableDescription(
											shapeIdx, 0.1f,
											new ContinuousDetectionSettings { Mode = (ContinuousDetectionMode)rbs[i].detectionMode }),
										new BodyActivityDescription(sleepThreshold: 0.001f)))
						});
				}
				else {
					afterUpdateCommands.AddComponent(entities[i],
						new InternalRigidBodyHandle()
						{
							rigidBodyHandle = handle =
								PhysicsSystem.Simulation.Bodies.Add(
									BodyDescription.CreateDynamic(RigidPose.Identity, colliders[i].inertia,
										new CollidableDescription(
											shapeIdx, 0.1f,
											new ContinuousDetectionSettings {  Mode = (ContinuousDetectionMode)rbs[i].detectionMode }),
										new BodyActivityDescription(sleepThreshold: 0.001f)))
						});
				}

				PhysicsSystem.rigidBodyEntityDictionary.Add(handle, entities[i]);
			}
		}
	}



	[ECSSystem(updateEvent: UpdateEvent.FixedUpdate, updateAfter: typeof(PhysicsSystem))]
	public class StaticBodyInitializerSystem : ComponentSystem
	{
		public override ComponentQuery GetQuery()
		{
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<StaticRigidBody>();
			query.IncludeReadWrite<InternalColliderHandle>();
			query.IncludeReadonly<Position>();
			query.IncludeReadonly<Rotation>();
			query.Exclude<InternalStaticBodyHandle>();
			query.Exclude<RigidBody>();
			return query;
		}

		public override IComponentFilter GetComponentFilter()
		{
			return ComponentFilters.EntityChanged();
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var entities = block.GetEntityData();
			var colliders = block.GetComponentData<InternalColliderHandle>();
			var pos = block.GetReadOnlyComponentData<Position>();
			var rot = block.GetReadOnlyComponentData<Rotation>();

			for (int i = 0; i < block.length; i++) {

				TypedIndex shapeIdx = colliders[i].shapeIdx;

				Vector3 p = new Vector3(pos[i].value.x, pos[i].value.y, pos[i].value.z); 
				BepuUtilities.Quaternion r = new BepuUtilities.Quaternion(rot[i].value.x, rot[i].value.y, rot[i].value.z, rot[i].value.w);

				int handle;
				afterUpdateCommands.AddComponent(entities[i],
					new InternalStaticBodyHandle()
					{
						staticBodyHandle = handle =
							PhysicsSystem.Simulation.Statics.Add(
								new StaticDescription()
								{
									Collidable = new CollidableDescription(
										shapeIdx, 0.1f,
										ContinuousDetectionSettings.Passive),
									Pose = new RigidPose() {
										Position = p,
										Orientation = r
									}
								}
							)
					});
				
				PhysicsSystem.staticBodyEntityDictionary.Add(handle, entities[i]);
			}
		}
	}
}
