using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using GlmSharp;

namespace TestApp
{

	public struct CameraFollow : IComponent
	{
		public Entity entityToFollow;
		public float zDistance;
	}


	[ECSSystem(updateEvent: UpdateEvent.LateUpdate)]
	class CameraFollowSystem : WorldComponentSystem
	{
		public override ComponentQuery GetQuery()
		{
			var query = new ComponentQuery();
			query.IncludeReadonly<CameraFollow>();
			query.IncludeReadWrite<Position>();
			return query;
		}

		public override void ProcessBlock(float deltaTime, BlockAccessor block, ECSWorld world)
		{
			var cameras = block.GetReadOnlyComponentData<CameraFollow>();
			var positions = block.GetComponentData<Position>();

			for (int i = 0; i < block.length; i++)
			{
				var followTarget = cameras[i].entityToFollow;
				try
				{
					if (world.EntityExists(followTarget))
					{
						var pos = world.ComponentManager.GetComponent<Position>(followTarget);
						positions[i].value = pos.value - vec3.UnitZ * cameras[i].zDistance;
					}
				}
				catch (ComponentNotFoundException) { }
			}
		}
	}
}
