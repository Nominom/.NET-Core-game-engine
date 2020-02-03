using System;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics.RenderSystems;
using Core.Shared;

namespace Core.Graphics.Systems
{

	[ECSSystem(UpdateEvent.BeforeRender, updateAfter : typeof(BoundingBoxSystem))] 
	public class CullingSystem : JobComponentSystem {

		private Frustum cameraFrustum;

		public override ComponentQuery GetQuery() {
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<BoundingBox>();
			query.ExcludeShared<CulledRenderTag>();
			return query;
		}

		public override void BeforeUpdate() {
			cameraFrustum = RenderManagerSystem.camera.GetFrustum(RenderManagerSystem.cameraPosition,
				RenderManagerSystem.cameraRotation);
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block) {
			var entities = block.GetEntityData();
			var aabbs = block.GetReadOnlyComponentData<BoundingBox>();
			for (int i = 0; i < block.length; i++) {
				if (cameraFrustum.AabbCull(aabbs[i].value)) {
					afterUpdateCommands.AddSharedComponent(entities[i], CulledRenderTag.Instance);
				}
			}
		}
	}
	[ECSSystem(UpdateEvent.BeforeRender, updateAfter : typeof(CullingSystem))]
	public class UnCullingSystem : JobComponentSystem
	{

		private Frustum cameraFrustum;

		public override ComponentQuery GetQuery() {
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<BoundingBox>();
			query.IncludeShared<CulledRenderTag>();
			return query;
		}

		public override void BeforeUpdate() {
			cameraFrustum = RenderManagerSystem.camera.GetFrustum(RenderManagerSystem.cameraPosition,
				RenderManagerSystem.cameraRotation);
		}


		public override void ProcessBlock(float deltaTime, BlockAccessor block) {
			var entities = block.GetEntityData();
			var aabbs = block.GetReadOnlyComponentData<BoundingBox>();
			for (int i = 0; i < block.length; i++) {
				if (!cameraFrustum.AabbCull(aabbs[i].value)) {
					afterUpdateCommands.RemoveSharedComponent<CulledRenderTag>(entities[i]);
				}
			}
		}
	}
}
