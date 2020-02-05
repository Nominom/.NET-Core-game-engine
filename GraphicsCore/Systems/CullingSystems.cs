using System;
using System.Numerics;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics.RenderSystems;
using Core.Shared;

namespace Core.Graphics.Systems
{

	[ECSSystem(UpdateEvent.BeforeRender, updateAfter: typeof(BoundingBoxSystem))]
	public class CullingSystem : JobComponentSystem
	{

		private Frustum[] cameraFrustums = Array.Empty<Frustum>();
		private ComponentQuery cameraQuery;

		public override void OnCreateSystem(ECSWorld world)
		{
			cameraQuery = new ComponentQuery();
			cameraQuery.IncludeShared<Camera>();
			cameraQuery.IncludeReadonly<Position>();
			cameraQuery.IncludeReadonly<Rotation>();
		}

		public override ComponentQuery GetQuery()
		{
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<BoundingBox>();
			query.ExcludeShared<CulledRenderTag>();
			return query;
		}

		public override void BeforeUpdate(float deltaTime, ECSWorld world)
		{
			Camera camera = null;
			Vector3 cameraPosition = Vector3.Zero;
			Quaternion cameraRotation = Quaternion.Identity;

			var cameraBlock = world.ComponentManager.GetBlocksNoSync(cameraQuery);
			int nextIdx = 0;
			foreach (var accessor in cameraBlock)
			{
				if (cameraFrustums.Length < nextIdx + accessor.length)
				{
					cameraFrustums = cameraFrustums.CopyResize(nextIdx + accessor.length);
				}
				camera = accessor.GetSharedComponentData<Camera>();
				for (int i = 0; i < accessor.length; i++)
				{
					cameraPosition = accessor.GetReadOnlyComponentData<Position>()[i].value;
					cameraRotation = accessor.GetReadOnlyComponentData<Rotation>()[i].value;
					cameraFrustums[nextIdx++] = camera.GetFrustum(cameraPosition, cameraRotation);
				}
			}
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var entities = block.GetEntityData();
			var aabbs = block.GetReadOnlyComponentData<BoundingBox>();
			for (int i = 0; i < block.length; i++)
			{
				bool anyCameraSees = false;
				for (int j = 0; j < cameraFrustums.Length; j++)
				{
					if (!cameraFrustums[j].AabbCull(aabbs[i].value))
					{
						anyCameraSees = true;
						break;
					}
				}
				if (!anyCameraSees)
				{
					afterUpdateCommands.AddSharedComponent(entities[i], CulledRenderTag.Instance);
				}
			}
		}
	}
	[ECSSystem(UpdateEvent.BeforeRender, updateAfter: typeof(CullingSystem))]
	public class UnCullingSystem : JobComponentSystem
	{

		private Frustum[] cameraFrustums = Array.Empty<Frustum>();
		private ComponentQuery cameraQuery;

		public override void OnCreateSystem(ECSWorld world)
		{
			cameraQuery = new ComponentQuery();
			cameraQuery.IncludeShared<Camera>();
			cameraQuery.IncludeReadonly<Position>();
			cameraQuery.IncludeReadonly<Rotation>();
		}

		public override ComponentQuery GetQuery()
		{
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<BoundingBox>();
			query.IncludeShared<CulledRenderTag>();
			return query;
		}

		public override void BeforeUpdate(float deltaTime, ECSWorld world)
		{
			Camera camera = null;
			Vector3 cameraPosition = Vector3.Zero;
			Quaternion cameraRotation = Quaternion.Identity;

			var cameraBlock = world.ComponentManager.GetBlocksNoSync(cameraQuery);
			int nextIdx = 0;
			foreach (var accessor in cameraBlock)
			{
				if (cameraFrustums.Length < nextIdx + accessor.length)
				{
					cameraFrustums = cameraFrustums.CopyResize(nextIdx + accessor.length);
				}
				camera = accessor.GetSharedComponentData<Camera>();
				for (int i = 0; i < accessor.length; i++)
				{
					cameraPosition = accessor.GetReadOnlyComponentData<Position>()[i].value;
					cameraRotation = accessor.GetReadOnlyComponentData<Rotation>()[i].value;
					cameraFrustums[nextIdx++] = camera.GetFrustum(cameraPosition, cameraRotation);
				}
			}
		}


		public override void ProcessBlock(float deltaTime, BlockAccessor block)
		{
			var entities = block.GetEntityData();
			var aabbs = block.GetReadOnlyComponentData<BoundingBox>();
			for (int i = 0; i < block.length; i++)
			{
				bool anyCameraSees = false;
				for (int j = 0; j < cameraFrustums.Length; j++)
				{
					if (!cameraFrustums[j].AabbCull(aabbs[i].value))
					{
						anyCameraSees = true;
						break;
					}
				}
				if (anyCameraSees)
				{
					afterUpdateCommands.RemoveSharedComponent<CulledRenderTag>(entities[i]);
				}

			}
		}
	}
}
