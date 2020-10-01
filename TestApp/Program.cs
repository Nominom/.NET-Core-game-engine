using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BepuPhysics.Collidables;
using Core;
using Core.AssetSystem;
using Core.AssetSystem.Assets;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics;
using Core.Graphics.VulkanBackend;
using Core.Physics;
using Core.Profiling;
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

		//TODO: Texture flipping needs to happen somewhere in y axis
		static void InitializeWorld()
		{

			var world = CoreEngine.World;
			var cm = world.ComponentManager;

			Random random = new Random(1);
			var fragShader = Asset.Load<ShaderAsset>("mesh_instanced.frag");
			var vertShader = Asset.Load<ShaderAsset>("mesh_instanced.vert");

			var shader = new ShaderPipeline(fragShader, vertShader, ShaderType.Instanced);
			var playerModel = Asset.Load<MeshAsset>("PUSHILIN_rocket_ship");
			var playerTexture = Asset.Load<TextureAsset>("PUSHILIN_rocket_ship_color");


			var asteroidModel0 = Asset.Load<MeshAsset>("asteroid0");
			var asteroidTexture = Asset.Load<TextureAsset>("Asteroids_BaseColor");


			playerModel.StartLoad(LoadPriority.High);
			playerTexture.StartLoad(LoadPriority.Medium);
			asteroidModel0.StartLoad(LoadPriority.Medium);
			asteroidTexture.StartLoad(LoadPriority.Medium);


			var playerTex = Texture2D.Create(playerTexture);
			var playerMat = Material.Create(shader, Color.white, playerTex);

			MeshRenderer playerRenderer = new MeshRenderer(Mesh.Create(playerModel), playerMat);

			Prefab player = new Prefab();
			player.AddComponent(new Position() { value = vec3.UnitY });
			player.AddComponent(new Rotation() { value = quat.Identity });
			player.AddComponent(new Scale() { value = vec3.Ones });
			player.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			player.AddComponent(new BoundingBox());
			player.AddSharedComponent(playerRenderer);
			player.AddSharedComponent(RenderTag.Opaque);
			player.AddComponent(new Player() { movementSpeed = 5, turningSpeed = 5 });
			player.AddComponent(new RigidBody() { mass = 10, detectionMode = CollisionDetectionMode.Continuous });
			player.AddSharedComponent(new MeshCollider(playerRenderer.mesh.meshData, true));
			player.AddSharedComponent(new DebugMeshConvexHullRenderer(playerRenderer.mesh.meshData));
			player.AddComponent(new Velocity() { });
			player.AddComponent(new AngularVelocity() { });
			player.AddComponent(new PhysicsBodyLockAxis() { lockZ = true, lockRotX = true, lockRotY = true });


			//var satelliteMesh = Mesh.Create(satelliteModel);
			//var satelliteTex = Texture2D.Create(satelliteTexture);
			//var satelliteMaterial = Material.Create(shader, Color.white, satelliteTex);

			//MeshRenderer renderer = new MeshRenderer(satelliteMesh, satelliteMaterial);

			//Prefab satellite = new Prefab();
			//satellite.AddComponent(new Position() { value = vec3.Zero });
			//satellite.AddComponent(new Rotation() { value = quat.Identity });
			//satellite.AddComponent(new Scale() { value = vec3.Ones * 0.2f });
			//satellite.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			//satellite.AddComponent(new BoundingBox());
			//satellite.AddSharedComponent(renderer);
			//satellite.AddSharedComponent(RenderTag.Opaque);

			//satelliteModel.LoadNow();
			//satellite.AddComponent(new RigidBody() { mass = 10, detectionMode = CollisionDetectionMode.Continuous });
			//satellite.AddSharedComponent(new MeshCollider(satelliteMesh.meshData, true));
			//satellite.AddSharedComponent(new DebugMeshConvexHullRenderer(satelliteMesh.meshData));
			//satellite.AddComponent(new Velocity() { value = vec3.UnitY * 10 });
			//satellite.AddComponent(new AngularVelocity() { value = vec3.UnitX * 1 });


			var asteroidTex = Texture2D.Create(asteroidTexture);
			var asteroidMat = Material.Create(shader, Color.white, asteroidTex);

			var asteroidMesh0 = Mesh.Create(asteroidModel0);

			MeshRenderer asteroidRenderer0 = new MeshRenderer(asteroidMesh0, asteroidMat);

			Prefab asteroid0 = new Prefab();
			asteroid0.AddComponent(new Position() { value = vec3.Zero });
			asteroid0.AddComponent(new Rotation() { value = quat.Identity });
			asteroid0.AddComponent(new Scale() { value = vec3.Ones * 0.2f });
			asteroid0.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			asteroid0.AddComponent(new BoundingBox());
			asteroid0.AddSharedComponent(asteroidRenderer0);
			asteroid0.AddSharedComponent(RenderTag.Opaque);

			asteroid0.AddComponent(new RigidBody() { mass = 10, detectionMode = CollisionDetectionMode.Continuous });
			asteroid0.AddSharedComponent(new MeshCollider(asteroidMesh0.meshData, true));

			//asteroid0.AddComponent(new BoxCollider() { width = asteroidMesh0.bounds.Size.x, height = asteroidMesh0.bounds.Size.y, length = asteroidMesh0.bounds.Size.z });
			//asteroid0.AddSharedComponent(new DebugMeshConvexHullRenderer(asteroidMesh0.meshData));
			asteroid0.AddComponent(new Velocity() { value = vec3.UnitY * 10 });
			asteroid0.AddComponent(new AngularVelocity() { value = vec3.UnitX * 1 });
			asteroid0.AddComponent(new PhysicsBodyLockAxis() { lockZ = true });
			asteroid0.AddComponent(new Asteroid());

			

			var cubeTex = Texture2D.Create(playerTexture);
			var cubeMaterial = Material.Create(shader, Color.white, cubeTex);
			var cubeMeshRend = new MeshRenderer() { mesh = RenderUtilities.UnitCube, materials = new[] { cubeMaterial } };

			Prefab cube = new Prefab();
			cube.AddComponent(new Position() { value = vec3.Zero });
			cube.AddComponent(new Rotation() { value = quat.Identity });
			cube.AddComponent(new Scale() { value = vec3.Ones });
			cube.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			cube.AddComponent(new BoundingBox());
			cube.AddSharedComponent(cubeMeshRend);
			cube.AddSharedComponent(RenderTag.Opaque);

			cube.AddComponent(new RigidBody() { mass = 1 });
			cube.AddComponent(new BoxCollider() { height = 1f, length = 1f, width = 1f });
			cube.AddComponent(new Velocity());
			cube.AddComponent(new AngularVelocity() { value = vec3.UnitX * 10 });


			//Prefab floor = new Prefab();
			//floor.AddComponent(new Position() { value = -vec3.UnitY * 2 });
			//floor.AddComponent(new Rotation() { value = quat.Identity });
			//floor.AddComponent(new Scale() { value = new vec3(1000, 0.2f, 1000) });
			//floor.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			//floor.AddComponent(new BoundingBox());
			//floor.AddSharedComponent(cubeMeshRend);
			//floor.AddSharedComponent(RenderTag.Opaque);
			//floor.AddComponent(new StaticRigidBody());
			//floor.AddComponent(new BoxCollider() { height = 1f, length = 1f, width = 1f });



			//const int numCubes = 10;
			//const int tallCubes = 10;
			//for (int i = 0; i < tallCubes; i++)
			//{
			//	for (int j = 0; j < numCubes; j++)
			//	{
			//		var entity = world.Instantiate(cube);
			//		cm.SetComponent(entity, new Position()
			//		{
			//			value = new vec3(
			//				random.Next(-(int)Math.Sqrt(numCubes) * 10 - 5, (int)Math.Sqrt(numCubes) * 10 + 5),
			//				i * 3 + 10,
			//				random.Next(-(int)Math.Sqrt(numCubes) * 10 - 5, (int)Math.Sqrt(numCubes) * 10 + 5))
			//		});

			//	}
			//}

			const int numThings = 10000;

			for (int i = 0; i < numThings; i++)
			{
				var entity = world.Instantiate(asteroid0);
				const int multi = 10;
				cm.SetComponent(entity, new Position()
				{
					value = new vec3(
						random.Next(-(int)Math.Sqrt(numThings) * multi, (int)Math.Sqrt(numThings) * multi),
						random.Next(-(int)Math.Sqrt(numThings) * multi, (int)Math.Sqrt(numThings) * multi),
						0
						)
				});
				var maxVel = 10f;
				cm.SetComponent(entity, new Velocity()
				{
					value = new vec3(
					(float)random.NextDouble() * maxVel * 2 - maxVel,
					(float)random.NextDouble() * maxVel * 2 - maxVel,
					0
					)
				});
				cm.SetComponent(entity, new AngularVelocity()
				{
					value = new vec3(
					(float)random.NextDouble() * 10 - 5,
					(float)random.NextDouble() * 10 - 5,
					(float)random.NextDouble() * 10 - 5
				)
				});
				cm.SetComponent(entity, new Scale() { value = new vec3((float)random.NextDouble() + 0.5f) });
			}


			var playerEntity = world.Instantiate(player);

			var cameraEntity = world.Instantiate(Prefabs.Camera);
			var cameraPosition = new vec3(4, 4, 10);
			//var cameraRotation = MathHelper.LookAt(cameraPosition, vec3.Zero, vec3.UnitY);
			var cameraRotation = quat.Identity;
			cm.SetComponent(cameraEntity,
				new Position() { value = cameraPosition });
			cm.SetComponent(cameraEntity,
				new Rotation() { value = cameraRotation });
			//cm.AddComponent(cameraEntity,
			//	new CameraFlightComponent() { flightSpeed = 5 });
			cm.AddComponent(cameraEntity, new CameraFollow() { entityToFollow = playerEntity, zDistance = 40 });


			//var floorEnt = world.Instantiate(floor);

			world.SystemManager.RegisterSystem(new AsteroidSpawner()
			{
				asteroidPrefabs = new[] { asteroid0 },
				maxCount = 5000,
				playerEntity = playerEntity
			});

		}

		//TODO: Weird heap corruption bug happens sometimes. -1073740940
		static void Main(string[] args)
		{
			CoreEngine.Initialize();
			CoreEngine.targetFps = 0;

			Asset.LoadAssetPackage("data/assets.dat");

			//Physics.Settings.Gravity = Vector3.Zero;

			InitializeWorld();

			Profiler.ProfilingEnabled = true;

			Physics.Settings.Gravity = Vector3.Zero;
			Physics.Settings.solverIterationCount = 2;

			CoreEngine.Update += delegate
			{
				if (CoreEngine.FrameNumber == 500)
				{
					Profiler.WriteToFile(@"D:\ecs_profile_speedscope.json", ProfilingFormat.SpeedScope, FrameSelection.Shortest);
					Profiler.WriteToFile(@"D:\ecs_profile_chrometracing_95.json", ProfilingFormat.ChromeTracing, FrameSelection.Percentile95);
					Profiler.WriteToFile(@"D:\ecs_profile_chrometracing_median.json", ProfilingFormat.ChromeTracing, FrameSelection.Median);
					Profiler.WriteToFile(@"D:\ecs_profile_chrometracing_shortest.json", ProfilingFormat.ChromeTracing, FrameSelection.Shortest);
				}
			};

			CoreEngine.Run();
		}
	}
}
