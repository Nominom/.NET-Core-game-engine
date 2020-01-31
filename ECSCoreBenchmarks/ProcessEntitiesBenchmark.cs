﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BenchmarkDotNet.Attributes;
using Core.ECS;
using Core.ECS.Components;

namespace CoreBenchmarks
{
	[Config(typeof(FullProfileConfig))]
	public class ProcessEntitiesBenchmark
	{
		private ECSWorld world;
		private ComponentQuery query;

		[GlobalSetup]
		public void Setup()
		{
			world = new ECSWorld(false);
			world.Initialize(false);

			EntityArchetype archetype = EntityArchetype.Empty
				.Add<Position>()
				.Add<Rotation>()
				.Add<Scale>()
				.Add<ObjectToWorld>();

			query.Include<Position>();
			query.Include<Rotation>();
			query.Include<Scale>();
			query.Include<ObjectToWorld>();

			for (int i = 0; i < numEntities; i++)
			{
				world.Instantiate(archetype);
			}
		}

		[Params(100, 1000, 10_000)]
		public int numEntities;


		[Benchmark(Baseline = true)]
		public void NormalStyle()
		{
			foreach (var block in world.ComponentManager.GetBlocks(query))
			{
				var position = block.GetReadOnlyComponentData<Position>();
				var rotation = block.GetReadOnlyComponentData<Rotation>();
				var scale = block.GetReadOnlyComponentData<Scale>();
				var oToW = block.GetComponentData<ObjectToWorld>();

				for (int i = 0; i < block.length; i++)
				{
					Vector3 pos = position[i].value;
					Quaternion rot = rotation[i].value;
					Vector3 scl = scale[i].value;

					var p = Matrix4x4.CreateTranslation(pos);
					var s = Matrix4x4.CreateScale(scl);

					var matrix = Matrix4x4.Transform(s, rot);
					matrix = Matrix4x4.Multiply(matrix, p);
					oToW[i].model = matrix;
				}
			}
		}
	}
}
