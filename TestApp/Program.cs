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

		static void InitializeWorld()
		{
			Random random = new Random(1);
			var houseModel = Assets.Create<MeshAsset>("House");
			var satelliteModel = Assets.Create<MeshAsset>("voyager");
			var satelliteTexture = Assets.Create<TextureAsset>("voyager_etc2_unorm");
			var placeholderTexture = Assets.Create<TextureAsset>("test_gradient_1_512");


			houseModel.StartLoad(LoadPriority.High);
			satelliteModel.StartLoad(LoadPriority.Medium);
			satelliteTexture.StartLoad(LoadPriority.Medium);
			placeholderTexture.StartLoad(LoadPriority.Medium);

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
			satellite.AddSharedComponent(renderer);
			satellite.AddSharedComponent(RenderTag.Opaque);

			satelliteModel.LoadNow();
			satellite.AddComponent(new RigidBody() { mass = 10, detectionMode = CollisionDetectionMode.Continuous });
			satellite.AddSharedComponent(new MeshCollider() { mesh = satelliteModel.Get().GetMeshData(), convex = true });
			satellite.AddSharedComponent(new DebugMeshConvexHullRenderer(satelliteModel.Get().GetMeshData()));
			satellite.AddComponent(new Velocity() { value = vec3.UnitY * 10 });
			satellite.AddComponent(new AngularVelocity() { value = vec3.UnitX * 1 });


			
			var cubeTex = Texture2D.Create(placeholderTexture);
			var cubeMaterial = Material.Create(Color.white, cubeTex);
			var cubeMeshRend = new MeshRenderer() { mesh = RenderUtilities.UnitCube, materials = new []{cubeMaterial}};

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



			Prefab floor = new Prefab();
			floor.AddComponent(new Position() { value = -vec3.UnitY * 2 });
			floor.AddComponent(new Rotation() { value = quat.Identity });
			floor.AddComponent(new Scale() { value = new vec3(1000, 0.2f, 1000) });
			floor.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			floor.AddComponent(new BoundingBox());
			floor.AddSharedComponent(cubeMeshRend);
			floor.AddSharedComponent(RenderTag.Opaque);
			floor.AddComponent(new StaticRigidBody());
			floor.AddComponent(new BoxCollider() { height = 1f, length = 1f, width = 1f });


			var world = CoreEngine.World;
			var cm = world.ComponentManager;

			const int numCubes = 100;
			const int tallCubes = 100;
			for (int i = 0; i < tallCubes; i++)
			{
				for (int j = 0; j < numCubes; j++)
				{
					var entity = world.Instantiate(cube);
					cm.SetComponent(entity, new Position()
					{
						value = new vec3(
							random.Next(-(int)Math.Sqrt(numCubes) * 10 - 5, (int)Math.Sqrt(numCubes) * 10 + 5),
							i * 3 + 10,
							random.Next(-(int)Math.Sqrt(numCubes) * 10 - 5, (int)Math.Sqrt(numCubes) * 10 + 5))
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
				cm.SetComponent(entity, new Scale() { value = new vec3((float)random.NextDouble() * 0.5f) });
			}


			var entity3 = world.Instantiate(satellite);

			var entity2 = world.Instantiate(house);

			var cameraEntity = world.Instantiate(Prefabs.Camera);
			var cameraPosition = new vec3(4, 30, 20);
			var cameraRotation = MathHelper.LookAt(cameraPosition, vec3.Zero, vec3.UnitY);
			cm.SetComponent(cameraEntity,
				new Position() { value = cameraPosition });
			cm.SetComponent(cameraEntity,
				new Rotation() { value = cameraRotation });


			var floorEnt = world.Instantiate(floor);
		}

		//TODO: Weird heap corruption bug happens sometimes. -1073740940
		static void Main(string[] args)
		{
			CoreEngine.Initialize();
			CoreEngine.targetFps = 0;

			Assets.LoadAssetPackage("data/assets.dat");

			//var asset = Assets.Create<TextureAsset>("bootman");
			//asset.LoadNow();

			InitializeWorld();

			Profiler.ProfilingEnabled = true;

			CoreEngine.Update += delegate(float time) {
				if (CoreEngine.FrameNumber == 500) {
					Profiler.WriteToFile(@"D:\ecs_profile_speedscope.json", ProfilingFormat.SpeedScope, FrameSelection.Shortest);
					Profiler.WriteToFile(@"D:\ecs_profile_chrometracing.json", ProfilingFormat.ChromeTracing, FrameSelection.Median);
				}
			};

			CoreEngine.Run();
		}
	}
}
