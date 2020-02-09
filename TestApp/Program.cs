using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Core;
using Core.AssetSystem;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics;
using Core.Graphics.VulkanBackend;
using Core.Shared;
using GlmSharp;

namespace TestApp
{
	public class Program
	{

		public struct RotateComponent : IComponent
		{
			public float rotationSpeed;
		}

		public class FaceCameraComponent : ISharedComponent {

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
		public class FaceTheCameraSystem : ComponentSystem {
			private ComponentQuery cameraQuery;

			private vec3 cameraPosition;

			public override ComponentQuery GetQuery() {
				ComponentQuery query = new ComponentQuery();
				query.IncludeShared<FaceCameraComponent>();
				query.IncludeReadWrite<Position>();
				query.IncludeReadWrite<Rotation>();
				return query;
			}

			public override void OnCreateSystem(ECSWorld world) {
				cameraQuery = new ComponentQuery();
				cameraQuery.IncludeShared<Camera>();
				cameraQuery.IncludeReadonly<Position>();
			}

			public override void BeforeUpdate(float deltaTime, ECSWorld world) {
				foreach (BlockAccessor block in world.ComponentManager.GetBlocks(cameraQuery)) {
					var poses = block.GetReadOnlyComponentData<Position>();
					for (int i = 0; i < block.length; i++) {
						cameraPosition = poses[i].value;
					}
				}
			}

			public override void ProcessBlock(float deltaTime, BlockAccessor block) {
				var positions = block.GetComponentData<Position>();
				var rotations = block.GetComponentData<Rotation>();
				for(int i = 0; i < block.length; i++) {
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

			MeshRenderer houseRenderer = new MeshRenderer() {mesh = Mesh.Create(houseModel)};

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
			satellite.AddComponent(new RotateComponent() { rotationSpeed = 1 });
			satellite.AddSharedComponent(renderer);
			satellite.AddSharedComponent(RenderTag.Opaque);

			var planeModel = Assets.Create<ModelAsset>("data/Porsche.obj");
			var planeMesh = Mesh.Create(planeModel);

			Prefab plane = new Prefab();
			plane.AddComponent(new Position() { value = vec3.Zero });
			plane.AddComponent(new Rotation() { value = quat.Identity });
			plane.AddComponent(new Scale() { value = vec3.Ones * 1f});
			plane.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			plane.AddComponent(new BoundingBox());
			plane.AddSharedComponent(new MeshRenderer() { mesh = planeMesh });
			plane.AddSharedComponent(RenderTag.Opaque);
			plane.AddSharedComponent(new FaceCameraComponent());

			Prefab cube = new Prefab();
			cube.AddComponent(new Position() { value = vec3.Zero });
			cube.AddComponent(new Rotation() { value = quat.Identity });
			//cube.AddComponent(new Scale() { value = vec3.Ones });
			cube.AddComponent(new ObjectToWorld() { model = mat4.Identity });
			cube.AddComponent(new BoundingBox());
			cube.AddComponent(new RotateComponent() { rotationSpeed = 1 });
			cube.AddSharedComponent(new MeshRenderer() { mesh = RenderUtilities.UnitCube });
			cube.AddSharedComponent(RenderTag.Opaque);

			var world = CoreEngine.World;
			var cm = world.ComponentManager;

			const int numThings = 10;

			for (int i = 0; i < numThings; i++)
			{
				var entity = world.Instantiate(satellite);
				cm.SetComponent(entity, new Position()
				{
					value = new vec3(
						random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5),
						0,
						random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5))
				});
				cm.SetComponent(entity, new RotateComponent() { rotationSpeed = (float)random.NextDouble() * 2 });
				//cm.RemoveComponent<RotateComponent>(entity);

				//entity = world.Instantiate(plane);
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

			var entity2 = world.Instantiate(house);
			cm.SetComponent(entity2, new Position() {
				value = new vec3(0, 0, 0)
			});
			cm.SetComponent(entity2, new RotateComponent() { rotationSpeed = (float)random.NextDouble() * 2 });


			entity2 = world.Instantiate(plane);
			cm.SetComponent(entity2, new Position() {
				value = new vec3(20, 0, 0)
			});
			cm.SetComponent(entity2, new RotateComponent() { rotationSpeed = 4 });


			var entity3 = world.Instantiate(satellite);

			var cameraEntity = world.Instantiate(Prefabs.Camera);
			var cameraPosition = new vec3(4 , 10, 20);
			var cameraRotation = MathHelper.LookAt(cameraPosition, vec3.Zero, vec3.UnitY);
			world.ComponentManager.SetComponent(cameraEntity, 
				new Position(){value = cameraPosition});
			world.ComponentManager.SetComponent(cameraEntity, 
				new Rotation(){value = cameraRotation});
			CoreEngine.Run();
		}
	}
}
