﻿using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Core;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics;
using Core.Graphics.VulkanBackend;

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
				query.Include<Rotation>();
				query.Include<RotateComponent>();
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
			Random random = new Random();
			CoreEngine.Initialize();
			CoreEngine.targetFps = 0;

			Prefab plane = new Prefab();
			plane.AddComponent(new Position() { value = Vector3.Zero });
			plane.AddComponent(new Rotation() { value = Quaternion.Identity });
			//plane.AddComponent(new Scale() { value = Vector3.One });
			plane.AddComponent(new ObjectToWorld() { model = Matrix4x4.Identity });
			plane.AddComponent(new RotateComponent() { rotationSpeed = 1 });
			plane.AddSharedComponent(new MeshRenderer() { mesh = RenderUtilities.FullScreenQuad });
			plane.AddSharedComponent(RenderTag.Opaque);


			Prefab cube = new Prefab();
			cube.AddComponent(new Position() { value = Vector3.Zero });
			cube.AddComponent(new Rotation() { value = Quaternion.Identity });
			//cube.AddComponent(new Scale() { value = Vector3.One });
			cube.AddComponent(new ObjectToWorld() { model = Matrix4x4.Identity });
			cube.AddComponent(new RotateComponent() { rotationSpeed = 1 });
			cube.AddSharedComponent(new MeshRenderer() { mesh = RenderUtilities.UnitCube });
			cube.AddSharedComponent(RenderTag.Opaque);

			var world = CoreEngine.World;
			var cm = world.ComponentManager;

			const int numThings = 10000;

			for (int i = 0; i < numThings; i++)
			{
				var entity = world.Instantiate(cube);
				cm.SetComponent(entity, new Position()
				{
					value = new Vector3(
						random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5),
						0,
						random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5))
				});
				cm.SetComponent(entity, new RotateComponent(){rotationSpeed =  (float)random.NextDouble() * 2});


				entity = world.Instantiate(plane);
				cm.SetComponent(entity, new Position()
				{
					value = new Vector3(
						random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5),
						0,
						random.Next(-(int)Math.Sqrt(numThings) - 5, (int)Math.Sqrt(numThings) + 5))
				});
				cm.SetComponent(entity, new RotateComponent() { rotationSpeed = (float)random.NextDouble() * 4 });
			}


			CoreEngine.Run();
		}
	}
}
