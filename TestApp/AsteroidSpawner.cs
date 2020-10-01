using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Core.Physics;
using Core.Shared;
using GlmSharp;

namespace TestApp
{

	public struct Asteroid : IComponent
	{

	}

	public class AsteroidSpawner : ComponentSystem
	{

		private int count = 0;
		public int maxCount;
		public Prefab[] asteroidPrefabs;
		public Entity playerEntity;
		private Random random = new Random();

		public override ComponentQuery GetQuery()
		{
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<Asteroid>();
			return query;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			count += block.length;
		}

		public override void BeforeUpdate(float deltaTime, ECSWorld world)
		{
			count = 0;
		}

		public override void AfterUpdate(float deltaTime, ECSWorld world)
		{
			if (count < 10000) {
				var position = world.ComponentManager.GetComponent<Position>(playerEntity);
				var offset = random.RandomOnCircle(random.Range(100, 400));
				afterUpdateCommands.CreateEntity(asteroidPrefabs[random.Next(0, asteroidPrefabs.Length)]);

				afterUpdateCommands.SetComponent(new Position()
				{
					value = offset + position.value
				});
				var maxVel = 10f;
				afterUpdateCommands.SetComponent(new Velocity(){value = new vec3(
					(float)random.NextDouble() * maxVel * 2 -maxVel,
					(float)random.NextDouble() * maxVel * 2 -maxVel,
					0
				)});
				afterUpdateCommands.SetComponent(new AngularVelocity(){value = new vec3(
					(float)random.NextDouble() * 10 -5,
					(float)random.NextDouble() * 10 -5,
					(float)random.NextDouble() * 10 -5
				)});
				afterUpdateCommands.SetComponent(new Scale() { value = new vec3((float)random.NextDouble() + 0.5f) });
			}
		}
	}
}
