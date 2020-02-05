﻿using System;
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

namespace TestApp
{
	public class Program
	{

		public struct RotateComponent : IComponent
		{
			public float rotationSpeed;
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

					var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, speed[i].rotationSpeed * deltaTime);
					rot[i].value = Quaternion.Multiply(rot[i].value, rotation);
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
			house.AddComponent(new Position() { value = Vector3.Zero });
			house.AddComponent(new Rotation() { value = Quaternion.Identity });
			house.AddComponent(new Scale() { value = Vector3.One * 0.01f });
			house.AddComponent(new ObjectToWorld() { model = Matrix4x4.Identity });
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
			satellite.AddComponent(new Position() { value = Vector3.Zero });
			satellite.AddComponent(new Rotation() { value = Quaternion.Identity });
			satellite.AddComponent(new Scale() { value = Vector3.One * 0.2f });
			satellite.AddComponent(new ObjectToWorld() { model = Matrix4x4.Identity });
			satellite.AddComponent(new BoundingBox());
			satellite.AddComponent(new RotateComponent() { rotationSpeed = 1 });
			satellite.AddSharedComponent(renderer);
			satellite.AddSharedComponent(RenderTag.Opaque);

			Prefab plane = new Prefab();
			plane.AddComponent(new Position() { value = Vector3.Zero });
			plane.AddComponent(new Rotation() { value = Quaternion.Identity });
			//plane.AddComponent(new Scale() { value = Vector3.One });
			plane.AddComponent(new ObjectToWorld() { model = Matrix4x4.Identity });
			plane.AddComponent(new BoundingBox());
			plane.AddComponent(new RotateComponent() { rotationSpeed = 1 });
			plane.AddSharedComponent(new MeshRenderer() { mesh = RenderUtilities.FullScreenQuad });
			plane.AddSharedComponent(RenderTag.Opaque);


			Prefab cube = new Prefab();
			cube.AddComponent(new Position() { value = Vector3.Zero });
			cube.AddComponent(new Rotation() { value = Quaternion.Identity });
			//cube.AddComponent(new Scale() { value = Vector3.One });
			cube.AddComponent(new ObjectToWorld() { model = Matrix4x4.Identity });
			cube.AddComponent(new BoundingBox());
			cube.AddComponent(new RotateComponent() { rotationSpeed = 1 });
			cube.AddSharedComponent(new MeshRenderer() { mesh = RenderUtilities.UnitCube });
			cube.AddSharedComponent(RenderTag.Opaque);

			var world = CoreEngine.World;
			var cm = world.ComponentManager;

			const int numThings = 100;

			for (int i = 0; i < numThings; i++)
			{
				var entity = world.Instantiate(satellite);
				cm.SetComponent(entity, new Position()
				{
					value = new Vector3(
						random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5),
						0,
						random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5))
				});
				cm.SetComponent(entity, new RotateComponent() { rotationSpeed = (float)random.NextDouble() * 2 });
				//cm.RemoveComponent<RotateComponent>(entity);

				//entity = world.Instantiate(plane);
				//cm.SetComponent(entity, new Position()
				//{
				//	value = new Vector3(
				//		random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5),
				//		0,
				//		random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5))
				//});
				//cm.SetComponent(entity, new RotateComponent() { rotationSpeed = (float)random.NextDouble() * 4 });
				//cm.RemoveComponent<RotateComponent>(entity);
			}

			var entity2 = world.Instantiate(house);
			cm.SetComponent(entity2, new Position() {
				value = new Vector3(0, 0, 0)
			});
			cm.SetComponent(entity2, new RotateComponent() { rotationSpeed = (float)random.NextDouble() * 2 });


			entity2 = world.Instantiate(plane);
			cm.SetComponent(entity2, new Position() {
				value = new Vector3(2, 0, 0)
			});
			cm.SetComponent(entity2, new RotateComponent() { rotationSpeed = 4 });


			var entity3 = world.Instantiate(satellite);

			var cameraEntity = world.Instantiate(Prefabs.Camera);
			var cameraPosition = new Vector3(4 , 10, 20);
			var cameraRotation = MathHelper.LookAt(cameraPosition, Vector3.Zero, Vector3.UnitY);
			world.ComponentManager.SetComponent(cameraEntity, 
				new Position(){value = cameraPosition});
			world.ComponentManager.SetComponent(cameraEntity, 
				new Rotation(){value = cameraRotation});
			CoreEngine.Run();
		}
	}
}
