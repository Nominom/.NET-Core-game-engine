using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Core.ECS;
using Core.ECS.Components;

namespace Core.Graphics
{
	[ECSSystem(UpdateEvent.BeforeRender)] 
	public class ObjectToWorldSystem : ISystem
	{
		public bool Enabled { get; set; }

		private ComponentQuery posQuery;
		private ComponentQuery posRotQuery;
		private ComponentQuery posRotScaleQuery;
		private ComponentQuery posScaleQuery;

		public void OnCreateSystem(ECSWorld world) {
			posQuery.Include<Position>();
			posQuery.Exclude<Rotation>();
			posQuery.Exclude<Scale>();
			posQuery.Include<ObjectToWorld>();

			posRotQuery.Include<Position>();
			posRotQuery.Include<Rotation>();
			posRotQuery.Exclude<Scale>();
			posRotQuery.Include<ObjectToWorld>();

			posRotScaleQuery.Include<Position>();
			posRotScaleQuery.Include<Rotation>();
			posRotScaleQuery.Include<Scale>();
			posRotScaleQuery.Include<ObjectToWorld>();

			posScaleQuery.Include<Position>();
			posScaleQuery.Exclude<Rotation>();
			posScaleQuery.Include<Scale>();
			posScaleQuery.Include<ObjectToWorld>();
		}

		public void OnDestroySystem(ECSWorld world) {
		}

		public void OnEnableSystem(ECSWorld world) {
		}

		public void OnDisableSystem(ECSWorld world) {
		}

		public void Update(float deltaTime, ECSWorld world) {

			foreach (var block in world.ComponentManager.GetBlocks(posQuery)) {
				var position = block.GetReadOnlyComponentData<Position>();
				var oToW = block.GetComponentData<ObjectToWorld>();

				for (int i = 0; i < block.Length; i++) {
					Vector3 pos = position[i].value;
					oToW[i].value = Matrix4x4.CreateTranslation(pos);
				}
			}

			foreach (var block in world.ComponentManager.GetBlocks(posRotQuery))
			{
				var position = block.GetReadOnlyComponentData<Position>();
				var rotation = block.GetReadOnlyComponentData<Rotation>();
				var oToW = block.GetComponentData<ObjectToWorld>();

				for (int i = 0; i < block.Length; i++)
				{
					Vector3 pos = position[i].value;
					Quaternion rot = rotation[i].value;

					var matrix = Matrix4x4.CreateWorld(pos, Vector3.Transform(Vector3.UnitZ, rot), Vector3.Transform(Vector3.UnitY, rot));
					oToW[i].value = matrix;
				}
			}

			foreach (var block in world.ComponentManager.GetBlocks(posRotScaleQuery))
			{
				var position = block.GetReadOnlyComponentData<Position>();
				var rotation = block.GetReadOnlyComponentData<Rotation>();
				var scale = block.GetReadOnlyComponentData<Scale>();
				var oToW = block.GetComponentData<ObjectToWorld>();

				for (int i = 0; i < block.Length; i++)
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

			foreach (var block in world.ComponentManager.GetBlocks(posScaleQuery))
			{
				var position = block.GetReadOnlyComponentData<Position>();
				var scale = block.GetReadOnlyComponentData<Scale>();
				var oToW = block.GetComponentData<ObjectToWorld>();

				for (int i = 0; i < block.Length; i++)
				{
					Vector3 pos = position[i].value;
					Vector3 scl = scale[i].value;

					var p = Matrix4x4.CreateTranslation(pos);
					var s = Matrix4x4.CreateScale(scl);

					var matrix = Matrix4x4.Multiply(s, p);
					oToW[i].value = matrix;
				}
			}

		}
	}
}
