using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;

namespace Core.Graphics.Systems
{
	[ECSSystem(updateEvent:UpdateEvent.BeforeRender, updateBefore: typeof(CullingSystem))]
	public class CameraAspectScaler : ISystem
	{
		public bool Enabled { get; set; }

		private float previousAspect = 1;
		private ComponentQuery query;

		public void OnCreateSystem(ECSWorld world) {
			query = new ComponentQuery();
			query.IncludeShared<Camera>();
			query.IncludeShared<CameraAutoScaleAspectComponent>();
		}

		public void OnDestroySystem(ECSWorld world) {
		}

		public void OnEnableSystem(ECSWorld world) {
		}

		public void OnDisableSystem(ECSWorld world) {
		}

		public void Update(float deltaTime, ECSWorld world) {
			if (Math.Abs(Window.Aspect - previousAspect) < 0.01) {
				return;
			}

			foreach (BlockAccessor block in world.ComponentManager.GetBlocks(query)) {
				var camera = block.GetSharedComponentData<Camera>();
				camera.aspect = Window.Aspect;
			}
		}
	}
}
