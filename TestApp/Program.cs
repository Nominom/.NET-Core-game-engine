using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BepuPhysics.Collidables;
using Core;
using Core.AssetSystem;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics;
using Core.Graphics.VulkanBackend;
using Core.Physics;
using Core.Shared;
using GlmSharp;
using Mesh = Core.Graphics.Mesh;

namespace TestApp
{
	public class Program
	{

		public struct RotateComponent : IComponent
		{
			public float rotationSpeed;
		}

		public class FaceCameraComponent : ISharedComponent
		{

		}


		[ECSSystem]
		public class RotatorSystem : JobComponentSystem
		{
			public override ComponentQuery GetQuery()
			{
				ComponentQuery query = new ComponentQuery();
				query.IncludeReadWrite<Rotation>();
				query.IncludeReadonly<RotateComponent>();
				return query;
			}

			public override void ProcessBlock(float deltaTime, BlockAccessor block)
			{
				var rot = block.GetComponentData<Rotation>();
				var speed = block.GetReadOnlyComponentData<RotateComponent>();

				for (int i = 0; i < block.length; i++)
				{

					var rotation = quat.FromAxisAngle(speed[i].rotationSpeed * deltaTime, vec3.UnitY);
					rot[i].value = rot[i].value * rotation;
				}

			}
		}

		[ECSSystem(UpdateEvent.Update)]
		public class FaceTheCameraSystem : ComponentSystem
		{
			private ComponentQuery cameraQuery;

			private vec3 cameraPosition;

			public override ComponentQuery GetQuery()
			{
				ComponentQuery query = new ComponentQuery();
				query.IncludeShared<FaceCameraComponent>();
				query.IncludeReadWrite<Position>();
				query.IncludeReadWrite<Rotation>();
				return query;
			}

			public override void OnCreateSystem(ECSWorld world)
			{
				cameraQuery = new ComponentQuery();
				cameraQuery.IncludeShared<Camera>();
				cameraQuery.IncludeReadonly<Position>();
			}

			public override void BeforeUpdate(float deltaTime, ECSWorld world)
			{
				foreach (BlockAccessor block in world.ComponentManager.GetBlocks(cameraQuery))
				{
					var poses = block.GetReadOnlyComponentData<Position>();
					for (int i = 0; i < block.length; i++)
					{
						cameraPosition = poses[i].value;
					}
				}
			}

			public override void ProcessBlock(float deltaTime, BlockAccessor block)
			{
				var positions = block.GetComponentData<Position>();
				var rotations = block.GetComponentData<Rotation>();
				for (int i = 0; i < block.length; i++)
				{
					rotations[i].value = MathHelper.LookAt(positions[i].value, cameraPosition, vec3.UnitY);
					//positions[i].value += rotations[i].value * vec3.UnitZ * deltaTime;
				}
			}
		}

		static void Main(string[] args)
		{
			Random random = new Random(1);
			CoreEngine.Initialize();
			CoreEngine.targetFps = 0;

			var houseModel = Assets.Create<ModelAsset>("data/House.fbx");

			MeshRenderer houseRenderer = new MeshRenderer() { mesh = Mesh.Create(houseModel) };

			Prefab house = new Prefab();
			house.AddComponent(new Position() { value = vec3.Zero });
			house.AddComponent(new Rotation() { value = quat.Identity });
			house.AddComponent(new Scale() { value = vec3.Ones * 0.01f });
			house.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			house.AddComponent(new BoundingBox());
			house.AddComponent(new RotateComponent() { rotationSpeed = 1 });
			house.AddSharedComponent(houseRenderer);
			house.AddSharedComponent(RenderTag.Opaque);

			var satelliteModel = Assets.Create<ModelAsset>("data/voyager.dae");
			var satelliteTexture = Assets.Create<CompressedTextureAsset>("data/voyager_etc2_unorm.ktx");

			var satelliteMesh = Mesh.Create(satelliteModel);
			var satelliteTex = Texture2D.Create(satelliteTexture);
			var satelliteMaterial = Material.Create(Color.white, satelliteTex);

			MeshRenderer renderer = new MeshRenderer(satelliteMesh, satelliteMaterial);

			Prefab satellite = new Prefab();
			satellite.AddComponent(new Position() { value = vec3.Zero });
			satellite.AddComponent(new Rotation() { value = quat.Identity });
			satellite.AddComponent(new Scale() { value = vec3.Ones * 0.2f });
			satellite.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			satellite.AddComponent(new BoundingBox());
			//satellite.AddComponent(new RotateComponent() { rotationSpeed = 1 });
			satellite.AddSharedComponent(renderer);
			satellite.AddSharedComponent(RenderTag.Opaque);

			satellite.AddComponent(new RigidBody() { mass = 10, detectionMode = CollisionDetectionMode.Continuous});
			satellite.AddSharedComponent(new MeshCollider(){mesh = satelliteModel.GetMeshData(), convex = true});
			satellite.AddSharedComponent(new DebugMeshConvexHullRenderer(satelliteModel.GetMeshData()));
			satellite.AddComponent(new Velocity(){value = vec3.UnitY * 10});
			satellite.AddComponent(new AngularVelocity(){value = vec3.UnitX * 1});

			var planeModel = Assets.Create<ModelAsset>("data/Porsche.obj");
			var planeMesh = Mesh.Create(planeModel);

			Prefab plane = new Prefab();
			plane.AddComponent(new Position() { value = vec3.Zero });
			plane.AddComponent(new Rotation() { value = quat.Identity });
			plane.AddComponent(new Scale() { value = vec3.Ones * 1f });
			plane.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			plane.AddComponent(new BoundingBox());
			plane.AddSharedComponent(new MeshRenderer() { mesh = planeMesh });
			plane.AddSharedComponent(RenderTag.Opaque);
			//plane.AddSharedComponent(new FaceCameraComponent());

			plane.AddComponent(new RigidBody() { mass = 100, detectionMode = CollisionDetectionMode.Continuous});
			plane.AddSharedComponent(new MeshCollider(){mesh = planeModel.GetMeshData(), convex = true});
			plane.AddSharedComponent(new DebugMeshConvexHullRenderer(planeModel.GetMeshData()));
			plane.AddComponent(new Velocity(){value = vec3.UnitY * 10});
			plane.AddComponent(new AngularVelocity(){value = vec3.UnitX * 20});

			var cubeMeshRend = new MeshRenderer() { mesh = RenderUtilities.UnitCube };

			Prefab cube = new Prefab();
			cube.AddComponent(new Position() { value = vec3.Zero });
			cube.AddComponent(new Rotation() { value = quat.Identity });
			//cube.AddComponent(new Scale() { value = vec3.Ones });
			cube.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			cube.AddComponent(new BoundingBox());
			cube.AddSharedComponent(cubeMeshRend);
			cube.AddSharedComponent(RenderTag.Opaque);

			cube.AddComponent(new RigidBody() { mass = 1 });
			cube.AddComponent(new BoxCollider() { height = 1f, length = 1f, width = 1f });
			cube.AddComponent(new Velocity());
			cube.AddComponent(new AngularVelocity(){value = vec3.UnitX * 10});

			

			Prefab floor = new Prefab();
			floor.AddComponent(new Position() { value = -vec3.UnitY * 2 });
			floor.AddComponent(new Rotation() { value = quat.Identity });
			floor.AddComponent(new Scale() { value = new vec3(100, 0.2f, 100) });
			floor.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			floor.AddComponent(new BoundingBox());
			floor.AddSharedComponent(cubeMeshRend);
			floor.AddSharedComponent(RenderTag.Opaque);
			floor.AddComponent(new StaticRigidBody());
			floor.AddComponent(new BoxCollider() { height = 1f, length = 1f, width = 1f });


			var world = CoreEngine.World;
			var cm = world.ComponentManager;

			const int numCubes = 3;
			const int tallCubes = 3;
			for (int i = 0; i < tallCubes; i++)
			{
				for (int j = 0; j < numCubes; j++)
				{
					var entity = world.Instantiate(cube);
					cm.SetComponent(entity, new Position()
					{
						value = new vec3(
							random.Next(-(int)Math.Sqrt(numCubes) - 5, (int)Math.Sqrt(numCubes) + 5),
							i * 3 + 10,
							random.Next(-(int)Math.Sqrt(numCubes) - 5, (int)Math.Sqrt(numCubes) + 5))
					});

				}
			}

			const int numThings = 10;

			for (int i = 0; i < numThings; i++)
			{
				var entity = world.Instantiate(satellite);
				cm.SetComponent(entity, new Position()
				{
					value = new vec3(
						random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5),
						100,
						random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5))
				});
				//cm.SetComponent(entity, new RotateComponent() { rotationSpeed = (float)random.NextDouble() * 2 });
				cm.SetComponent(entity, new Scale(){value = new vec3((float)random.NextDouble()* 0.5f)});

				//entity = world.Instantiate(cube);
				//cm.SetComponent(entity, new Position()
				//{
				//	value = new vec3(
				//		random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5),
				//		0,
				//		random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5))
				//});
				//cm.SetComponent(entity, new RotateComponent() { rotationSpeed = (float)random.NextDouble() * 4 });
				//cm.RemoveComponent<RotateComponent>(entity);
			}

			//var entity2 = world.Instantiate(house);
			//cm.SetComponent(entity2, new Position()
			//{
			//	value = new vec3(0, 0, 0)
			//});
			//cm.SetComponent(entity2, new RotateComponent() { rotationSpeed = (float)random.NextDouble() * 2 });


			var entity2 = world.Instantiate(plane);
			cm.SetComponent(entity2, new Position()
			{
				value = new vec3(20, 0, 0)
			});
			cm.SetComponent(entity2, new RotateComponent() { rotationSpeed = 4 });


			var entity3 = world.Instantiate(satellite);

			var cameraEntity = world.Instantiate(Prefabs.Camera);
			var cameraPosition = new vec3(4, 30, 20);
			var cameraRotation = MathHelper.LookAt(cameraPosition, vec3.Zero, vec3.UnitY);
			cm.SetComponent(cameraEntity,
				new Position() { value = cameraPosition });
			cm.SetComponent(cameraEntity,
				new Rotation() { value = cameraRotation });


			var floorEnt = world.Instantiate(floor);


			CoreEngine.Run();
		}
	}
}
