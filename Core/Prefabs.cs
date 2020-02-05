using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics;

namespace Core
{
	public static class Prefabs {

		private static EntityArchetype cameraArchetype { get; } = CreateCameraArchetype();
		public static Prefab Camera => new Prefab(cameraArchetype); 

		private static EntityArchetype CreateCameraArchetype()
		{
			EntityArchetype archetype = new EntityArchetype();

			Camera camera = new Camera();
			camera.aspect = (float)Window.window.Width / (float)Window.window.Height;
			camera.fow = 60f;
			camera.nearPlane = 0.01f;
			camera.farPlane = 1000f;

			archetype = archetype.AddShared(camera);
			archetype = archetype.AddShared(CameraAutoScaleAspectComponent.Instance);
			archetype = archetype.AddShared(MainCameraTag.Instance);
			archetype = archetype.Add<Position>();
			archetype = archetype.Add<Rotation>();
			return archetype;
		}
	}
}
