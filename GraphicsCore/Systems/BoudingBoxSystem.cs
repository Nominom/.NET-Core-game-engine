using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Core.ECS.Filters;

namespace Core.Graphics.Systems
{
	[ECSSystem(UpdateEvent.BeforeRender, updateAfter : typeof(ObjectToWorldSystemPRS))] 
	public class BoundingBoxSystem : JobComponentSystem
	{
		public override ComponentQuery GetQuery() {
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadWrite<BoundingBox>();
			query.IncludeReadonly<ObjectToWorld>();
			query.IncludeShared<MeshRenderer>();
			return query;
		}

		public override IComponentFilter GetComponentFilter() {
			return ComponentFilters.Changed<ObjectToWorld>();
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block) {
			var mesh = block.GetSharedComponentData<MeshRenderer>();
			var boxes = block.GetComponentData<BoundingBox>();
			var objToW = block.GetReadOnlyComponentData<ObjectToWorld>();
			var initialBox = mesh.mesh.bounds;

			for (int i = 0; i < block.length; i++) {
				boxes[i].value = initialBox.TransformAabb(objToW[i].model);
			}
		}
	}
}
