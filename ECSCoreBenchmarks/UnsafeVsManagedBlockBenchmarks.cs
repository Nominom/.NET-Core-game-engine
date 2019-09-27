using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BenchmarkDotNet.Attributes;
using Core.ECS;
using Core.ECS.Components;

namespace CoreBenchmarks
{
	[Config(typeof(FullProfileConfig))]
	public class UnsafeVsManagedBlockBenchmarks {
		private List<ComponentMemoryBlock> normalBlocks;
		private List<UnsafeComponentMemoryBlock> unsafeBlocks;

		[GlobalSetup]
		public void Setup()
		{
			EntityArchetype archetype = EntityArchetype.Empty
				.Add<Position>()
				.Add<Rotation>()
				.Add<Scale>()
				.Add<ObjectToWorld>();

			normalBlocks = new List<ComponentMemoryBlock>();
			unsafeBlocks = new List<UnsafeComponentMemoryBlock>();

			ComponentMemoryBlock currentNormalBlock = new ComponentMemoryBlock(archetype);
			UnsafeComponentMemoryBlock currentUnsafeBlock = new UnsafeComponentMemoryBlock(archetype);
			normalBlocks.Add(currentNormalBlock);
			unsafeBlocks.Add(currentUnsafeBlock);
			for (int i = 0; i < numEntities; i++)
			{
				if (!currentNormalBlock.HasRoom || currentNormalBlock.Size > 100) {
					currentNormalBlock = new ComponentMemoryBlock(archetype);
					normalBlocks.Add(currentNormalBlock);
				}

				if (!currentUnsafeBlock.HasRoom || currentUnsafeBlock.Size > 100)
				{
					currentUnsafeBlock = new UnsafeComponentMemoryBlock(archetype);
					unsafeBlocks.Add(currentUnsafeBlock);
				}

				Entity e = new Entity(){id = i +1, version = 1};

				currentNormalBlock.AddEntity(e);
				currentUnsafeBlock.AddEntity(e);
			}
		}

		[GlobalCleanup]
		public void CleanUp() {
			foreach (UnsafeComponentMemoryBlock unsafeBlock in unsafeBlocks) {
				unsafeBlock.Dispose();
			}

			foreach (ComponentMemoryBlock normalBlock in normalBlocks)
			{
				normalBlock.Dispose();
			}
		}

		[Params(10_000, 1_000_000)]
		public int numEntities;


		[Benchmark(Baseline = true)]
		public void Managed()
		{
			foreach (var block in normalBlocks) {
				var access = block.GetAccessor();
				var position = access.GetReadOnlyComponentData<Position>();
				var rotation = access.GetReadOnlyComponentData<Rotation>();
				var scale = access.GetReadOnlyComponentData<Scale>();
				var oToW = block.GetComponentData<ObjectToWorld>();

				for (int i = 0; i < access.length; i++)
				{
					Vector3 pos = position[i].value;
					Quaternion rot = rotation[i].value;
					Vector3 scl = scale[i].value;

					var p = Matrix4x4.CreateTranslation(pos);
					var s = Matrix4x4.CreateScale(scl);

					var matrix = Matrix4x4.Transform(s, rot);
					matrix = Matrix4x4.Multiply(matrix, p);
					oToW[i].value = matrix;
				}
			}
		}

		[Benchmark]
		public void Unsafe()
		{
			foreach (var block in unsafeBlocks)
			{
				var access = block.GetAccessor();
				var position = access.GetReadOnlyComponentData<Position>();
				var rotation = access.GetReadOnlyComponentData<Rotation>();
				var scale = access.GetReadOnlyComponentData<Scale>();
				var oToW = block.GetComponentData<ObjectToWorld>();

				for (int i = 0; i < access.length; i++)
				{
					Vector3 pos = position[i].value;
					Quaternion rot = rotation[i].value;
					Vector3 scl = scale[i].value;

					var p = Matrix4x4.CreateTranslation(pos);
					var s = Matrix4x4.CreateScale(scl);

					var matrix = Matrix4x4.Transform(s, rot);
					matrix = Matrix4x4.Multiply(matrix, p);
					oToW[i].value = matrix;
				}
			}
		}
	}
}
