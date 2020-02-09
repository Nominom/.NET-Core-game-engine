using Assimp;
using Core.ECS;
using Core.ECS.Components;
using Core.ECS.Filters;
using Core.Shared;
using GlmSharp;

namespace Core.Graphics.Systems
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
	//				vec3 pos = position[i].value;
	//				oToW[i].model = mat4.CreateTranslation(pos);
	//				mat4.Invert(oToW[i].model, out var inverse);
	//				oToW[i].normal = new mat3(mat4.Transpose(inverse));
	//			}
	//		}

	//		foreach (var block in world.ComponentManager.GetBlocks(posRotQuery))
	//		{
	//			var position = block.GetReadOnlyComponentData<Position>();
	//			var rotation = block.GetReadOnlyComponentData<Rotation>();
	//			var oToW = block.GetComponentData<ObjectToWorld>();

	//			for (int i = 0; i < block.length; i++)
	//			{
	//				vec3 pos = position[i].value;
	//				quat rot = rotation[i].value;

	//				var matrix = mat4.CreateWorld(pos, vec3.Transform(vec3.UnitZ, rot), vec3.Transform(vec3.UnitY, rot));
	//				oToW[i].model = matrix;
	//				mat4.Invert(oToW[i].model, out var inverse);
	//				oToW[i].normal = new mat3(mat4.Transpose(inverse));
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
	//				vec3 pos = position[i].value;
	//				quat rot = rotation[i].value;
	//				vec3 scl = scale[i].value;

	//				var p = mat4.CreateTranslation(pos);
	//				var s = mat4.CreateScale(scl);

	//				var matrix = mat4.Transform(s, rot);
	//				matrix = mat4.Multiply(matrix, p);
	//				oToW[i].model = matrix;
	//				mat4.Invert(oToW[i].model, out var inverse);
	//				oToW[i].normal = new mat3(mat4.Transpose(inverse));
	//			}
	//		}

	//		foreach (var block in world.ComponentManager.GetBlocks(posScaleQuery))
	//		{
	//			var position = block.GetReadOnlyComponentData<Position>();
	//			var scale = block.GetReadOnlyComponentData<Scale>();
	//			var oToW = block.GetComponentData<ObjectToWorld>();

	//			for (int i = 0; i < block.length; i++)
	//			{
	//				vec3 pos = position[i].value;
	//				vec3 scl = scale[i].value;

	//				var p = mat4.CreateTranslation(pos);
	//				var s = mat4.CreateScale(scl);

	//				var matrix = mat4.Multiply(s, p);
	//				oToW[i].model = matrix;
	//				mat4.Invert(oToW[i].model, out var inverse);
	//				oToW[i].normal = new mat3(mat4.Transpose(inverse));
	//			}
	//		}

	//	}
	//}

	[ECSSystem(UpdateEvent.BeforeRender, updateAfter : typeof(ObjectToWorldSystemPR))]
	public class ObjectToWorldSystemPRS : JobComponentSystem
	{
		private ComponentQuery posRotScaleQuery;

		public override ComponentQuery GetQuery()
		{
			posRotScaleQuery.IncludeReadonly<Position>();
			posRotScaleQuery.IncludeReadonly<Rotation>();
			posRotScaleQuery.IncludeReadonly<Scale>();
			posRotScaleQuery.IncludeReadWrite<ObjectToWorld>();
			return posRotScaleQuery;
		}

		public override IComponentFilter GetComponentFilter() {
			return ComponentFilters.ChangedAny<Position, Rotation, Scale>();
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var position = block.GetReadOnlyComponentData<Position>();
			var rotation = block.GetReadOnlyComponentData<Rotation>();
			var scale = block.GetReadOnlyComponentData<Scale>();
			var oToW = block.GetComponentData<ObjectToWorld>();

			for (int i = 0; i < block.length; i++)
			{
				vec3 pos = position[i].value;
				quat rot = rotation[i].value;
				vec3 scl = scale[i].value;

				var p = mat4.Translate(pos);
				var s = mat4.Scale(scl);
				var r = rot.ToMat4;
				var matrix = p * r * s;
				oToW[i].model = matrix;
				var inverse = matrix.Inverse;
				var normalMatrix = new mat3(inverse.Transposed);
				oToW[i].normal = normalMatrix;
			}
		}
	}

	[ECSSystem(UpdateEvent.BeforeRender, updateAfter : typeof(ObjectToWorldSystemPS))]
	public class ObjectToWorldSystemPR : JobComponentSystem
	{
		private ComponentQuery posRotQuery;

		public override ComponentQuery GetQuery()
		{
			posRotQuery.IncludeReadonly<Position>();
			posRotQuery.IncludeReadonly<Rotation>();
			posRotQuery.Exclude<Scale>();
			posRotQuery.IncludeReadWrite<ObjectToWorld>();
			return posRotQuery;
		}

		public override IComponentFilter GetComponentFilter() {
			
			return ComponentFilters.ChangedAny<Position, Rotation>();
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var position = block.GetReadOnlyComponentData<Position>();
			var rotation = block.GetReadOnlyComponentData<Rotation>();
			var oToW = block.GetComponentData<ObjectToWorld>();

			for (int i = 0; i < block.length; i++)
			{
				vec3 pos = position[i].value;
				quat rot = rotation[i].value;
				var p = mat4.Translate(pos);

				var matrix = p * rot.ToMat4;
				oToW[i].model = matrix;
				oToW[i].normal = new mat3(oToW[i].model);
			}
		}
	}

	[ECSSystem(UpdateEvent.BeforeRender, updateAfter : typeof(ObjectToWorldSystemP))]
	public class ObjectToWorldSystemPS : JobComponentSystem
	{
		private ComponentQuery posScaleQuery;

		public override ComponentQuery GetQuery()
		{
			posScaleQuery.IncludeReadonly<Position>();
			posScaleQuery.Exclude<Rotation>();
			posScaleQuery.IncludeReadonly<Scale>();
			posScaleQuery.IncludeReadWrite<ObjectToWorld>();
			return posScaleQuery;
		}

		public override IComponentFilter GetComponentFilter() {
			return ComponentFilters.ChangedAny<Position, Scale>();
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var position = block.GetReadOnlyComponentData<Position>();
			var scale = block.GetReadOnlyComponentData<Scale>();
			var oToW = block.GetComponentData<ObjectToWorld>();

			for (int i = 0; i < block.length; i++)
			{
				vec3 pos = position[i].value;
				vec3 scl = scale[i].value;

				var p = mat4.Translate(pos);
				var s = mat4.Scale(scl);

				var matrix = p * s;
				oToW[i].model = matrix;
				var inverse = matrix.Inverse;
				var normalMatrix = new mat3(inverse.Transposed);
				oToW[i].normal = normalMatrix;
			}
		}
	}

	[ECSSystem(UpdateEvent.BeforeRender)]
	public class ObjectToWorldSystemP : JobComponentSystem
	{
		private ComponentQuery posQuery;

		public override ComponentQuery GetQuery()
		{
			posQuery.IncludeReadonly<Position>();
			posQuery.Exclude<Rotation>();
			posQuery.Exclude<Scale>();
			posQuery.IncludeReadWrite<ObjectToWorld>();
			return posQuery;
		}

		public override IComponentFilter GetComponentFilter() {
			return ComponentFilters.Changed<Position>();
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{

			var position = block.GetReadOnlyComponentData<Position>();
			var oToW = block.GetComponentData<ObjectToWorld>();

			for (int i = 0; i < block.length; i++)
			{
				vec3 pos = position[i].value;
				oToW[i].model = mat4.Translate(pos);
				oToW[i].normal = mat3.Identity;
			}

		}
	}
}
