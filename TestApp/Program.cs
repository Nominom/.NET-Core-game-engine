using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Core;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics;

namespace TestApp
{
	public class Program
	{

		public struct RotateComponent : IComponent {
			public float rotationSpeed;
		}


		[ECSSystem]
		public class RotatorSystem : ComponentSystem {
			public override ComponentQuery GetQuery() {
				ComponentQuery query = new ComponentQuery();
				query.Include<Rotation>();
				query.Include<RotateComponent>();
				return query;
			}

			public override void ProcessBlock(float deltaTime, BlockAccessor accessor) {
				var rot = accessor.GetComponentData<Rotation>();
				var speed = accessor.GetReadOnlyComponentData<RotateComponent>();

				for (int i = 0; i < accessor.length; i++) {

					var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, speed[i].rotationSpeed * deltaTime);
					rot[i].value = Quaternion.Multiply(rot[i].value, rotation);
				}

			}
		}

		static void Main(string[] args)
		{
			Random random = new Random();
			CoreEngine.Initialize();

			Prefab cube = new Prefab();
			cube.AddComponent(new Position(){value = Vector3.Zero});
			cube.AddComponent(new Rotation(){value = Quaternion.Identity});
			cube.AddComponent(new Scale(){value = Vector3.One});
			cube.AddComponent(new ObjectToWorld(){value = Matrix4x4.Identity});
			cube.AddComponent(new RotateComponent(){rotationSpeed = 1});
			cube.AddSharedComponent(new MeshRenderer(){mesh = RenderUtilities.UnitCube});
			cube.AddSharedComponent(RenderTag.Opaque);

			var world = CoreEngine.World;
			var cm = world.ComponentManager;

			const int numThings = 100;

			for (int i = 0; i < numThings; i++) {
				var entity = world.Instantiate(cube);
				cm.SetComponent(entity, new Position() {
					value = new Vector3(
						random.Next(-(int)Math.Sqrt(numThings), (int)Math.Sqrt(numThings)),
						0, 
						random.Next(-(int)Math.Sqrt(numThings), (int)Math.Sqrt(numThings)))
				});
			}
			

			CoreEngine.Run();
		}
	}
}
