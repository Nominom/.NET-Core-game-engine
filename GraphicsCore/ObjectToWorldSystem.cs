﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Core.Shared;

namespace Core.Graphics
{
	//[ECSSystem(UpdateEvent.BeforeRender)] 
	//public class ObjectToWorldSystem : ISystem
	//{
	//	public bool Enabled { get; set; }

	//	private ComponentQuery posQuery;
	//	private ComponentQuery posRotQuery;
	//	private ComponentQuery posRotScaleQuery;
	//	private ComponentQuery posScaleQuery;

	//	public void OnCreateSystem(ECSWorld world) {
	//		posQuery.Include<Position>();
	//		posQuery.Exclude<Rotation>();
	//		posQuery.Exclude<Scale>();
	//		posQuery.Include<ObjectToWorld>();

	//		posRotQuery.Include<Position>();
	//		posRotQuery.Include<Rotation>();
	//		posRotQuery.Exclude<Scale>();
	//		posRotQuery.Include<ObjectToWorld>();

	//		posRotScaleQuery.Include<Position>();
	//		posRotScaleQuery.Include<Rotation>();
	//		posRotScaleQuery.Include<Scale>();
	//		posRotScaleQuery.Include<ObjectToWorld>();

	//		posScaleQuery.Include<Position>();
	//		posScaleQuery.Exclude<Rotation>();
	//		posScaleQuery.Include<Scale>();
	//		posScaleQuery.Include<ObjectToWorld>();
	//	}

	//	public void OnDestroySystem(ECSWorld world) {
	//	}

	//	public void OnEnableSystem(ECSWorld world) {
	//	}

	//	public void OnDisableSystem(ECSWorld world) {
	//	}

	//	public void Update(float deltaTime, ECSWorld world) {

	//		foreach (var block in world.ComponentManager.GetBlocks(posQuery)) {
	//			var position = block.GetReadOnlyComponentData<Position>();
	//			var oToW = block.GetComponentData<ObjectToWorld>();

	//			for (int i = 0; i < block.length; i++) {
	//				Vector3 pos = position[i].value;
	//				oToW[i].model = Matrix4x4.CreateTranslation(pos);
	//				Matrix4x4.Invert(oToW[i].model, out var inverse);
	//				oToW[i].normal = new Matrix3x3(Matrix4x4.Transpose(inverse));
	//			}
	//		}

	//		foreach (var block in world.ComponentManager.GetBlocks(posRotQuery))
	//		{
	//			var position = block.GetReadOnlyComponentData<Position>();
	//			var rotation = block.GetReadOnlyComponentData<Rotation>();
	//			var oToW = block.GetComponentData<ObjectToWorld>();

	//			for (int i = 0; i < block.length; i++)
	//			{
	//				Vector3 pos = position[i].value;
	//				Quaternion rot = rotation[i].value;

	//				var matrix = Matrix4x4.CreateWorld(pos, Vector3.Transform(Vector3.UnitZ, rot), Vector3.Transform(Vector3.UnitY, rot));
	//				oToW[i].model = matrix;
	//				Matrix4x4.Invert(oToW[i].model, out var inverse);
	//				oToW[i].normal = new Matrix3x3(Matrix4x4.Transpose(inverse));
	//			}
	//		}

	//		foreach (var block in world.ComponentManager.GetBlocks(posRotScaleQuery))
	//		{
	//			var position = block.GetReadOnlyComponentData<Position>();
	//			var rotation = block.GetReadOnlyComponentData<Rotation>();
	//			var scale = block.GetReadOnlyComponentData<Scale>();
	//			var oToW = block.GetComponentData<ObjectToWorld>();

	//			for (int i = 0; i < block.length; i++)
	//			{
	//				Vector3 pos = position[i].value;
	//				Quaternion rot = rotation[i].value;
	//				Vector3 scl = scale[i].value;

	//				var p = Matrix4x4.CreateTranslation(pos);
	//				var s = Matrix4x4.CreateScale(scl);

	//				var matrix = Matrix4x4.Transform(s, rot);
	//				matrix = Matrix4x4.Multiply(matrix, p);
	//				oToW[i].model = matrix;
	//				Matrix4x4.Invert(oToW[i].model, out var inverse);
	//				oToW[i].normal = new Matrix3x3(Matrix4x4.Transpose(inverse));
	//			}
	//		}

	//		foreach (var block in world.ComponentManager.GetBlocks(posScaleQuery))
	//		{
	//			var position = block.GetReadOnlyComponentData<Position>();
	//			var scale = block.GetReadOnlyComponentData<Scale>();
	//			var oToW = block.GetComponentData<ObjectToWorld>();

	//			for (int i = 0; i < block.length; i++)
	//			{
	//				Vector3 pos = position[i].value;
	//				Vector3 scl = scale[i].value;

	//				var p = Matrix4x4.CreateTranslation(pos);
	//				var s = Matrix4x4.CreateScale(scl);

	//				var matrix = Matrix4x4.Multiply(s, p);
	//				oToW[i].model = matrix;
	//				Matrix4x4.Invert(oToW[i].model, out var inverse);
	//				oToW[i].normal = new Matrix3x3(Matrix4x4.Transpose(inverse));
	//			}
	//		}

	//	}
	//}

	[ECSSystem(UpdateEvent.BeforeRender)]
	public class ObjectToWorldSystemPRS : JobComponentSystem
	{
		private ComponentQuery posRotScaleQuery;

		public override ComponentQuery GetQuery()
		{
			posRotScaleQuery.Include<Position>();
			posRotScaleQuery.Include<Rotation>();
			posRotScaleQuery.Include<Scale>();
			posRotScaleQuery.Include<ObjectToWorld>();
			return posRotScaleQuery;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
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
				Matrix4x4.Invert(oToW[i].model, out var inverse);
				oToW[i].normal = new Matrix3x3(Matrix4x4.Transpose(inverse));
			}
		}
	}

	[ECSSystem(UpdateEvent.BeforeRender)]
	public class ObjectToWorldSystemPR : JobComponentSystem
	{
		private ComponentQuery posRotQuery;

		public override ComponentQuery GetQuery()
		{
			posRotQuery.Include<Position>();
			posRotQuery.Include<Rotation>();
			posRotQuery.Exclude<Scale>();
			posRotQuery.Include<ObjectToWorld>();
			return posRotQuery;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var position = block.GetReadOnlyComponentData<Position>();
			var rotation = block.GetReadOnlyComponentData<Rotation>();
			var oToW = block.GetComponentData<ObjectToWorld>();

			for (int i = 0; i < block.length; i++)
			{
				Vector3 pos = position[i].value;
				Quaternion rot = rotation[i].value;
				
				var p = Matrix4x4.CreateTranslation(pos);
				var matrix = Matrix4x4.Transform(Matrix4x4.Identity, rot);
				matrix = Matrix4x4.Multiply(matrix, p);

				oToW[i].model = matrix;
				oToW[i].normal = new Matrix3x3(oToW[i].model);
			}
		}
	}

	[ECSSystem(UpdateEvent.BeforeRender)]
	public class ObjectToWorldSystemPS : JobComponentSystem
	{
		private ComponentQuery posScaleQuery;

		public override ComponentQuery GetQuery()
		{
			posScaleQuery.Include<Position>();
			posScaleQuery.Exclude<Rotation>();
			posScaleQuery.Include<Scale>();
			posScaleQuery.Include<ObjectToWorld>();
			return posScaleQuery;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var position = block.GetReadOnlyComponentData<Position>();
			var scale = block.GetReadOnlyComponentData<Scale>();
			var oToW = block.GetComponentData<ObjectToWorld>();

			for (int i = 0; i < block.length; i++)
			{
				Vector3 pos = position[i].value;
				Vector3 scl = scale[i].value;

				var p = Matrix4x4.CreateTranslation(pos);
				var s = Matrix4x4.CreateScale(scl);

				var matrix = Matrix4x4.Multiply(s, p);
				oToW[i].model = matrix;
				Matrix4x4.Invert(oToW[i].model, out var inverse);
				oToW[i].normal = new Matrix3x3(Matrix4x4.Transpose(inverse));
			}
		}
	}

	public class ObjectToWorldSystemP : JobComponentSystem
	{
		private ComponentQuery posQuery;

		public override ComponentQuery GetQuery()
		{
			posQuery.Include<Position>();
			posQuery.Exclude<Rotation>();
			posQuery.Exclude<Scale>();
			posQuery.Include<ObjectToWorld>();
			return posQuery;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{

			var position = block.GetReadOnlyComponentData<Position>();
			var oToW = block.GetComponentData<ObjectToWorld>();

			for (int i = 0; i < block.length; i++)
			{
				Vector3 pos = position[i].value;
				oToW[i].model = Matrix4x4.CreateTranslation(pos);
				oToW[i].normal = new Matrix3x3(oToW[i].model);
			}

		}
	}
}
