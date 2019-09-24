using System;
using System.Diagnostics;
using System.Threading;
using Core.ECS;
using Core.Graphics;

namespace Core
{
	public static class CoreEngine
	{
		public delegate void EngineUpdateDelegate(float deltaTime);

		public static event EngineUpdateDelegate Update;
		public static event EngineUpdateDelegate Render;
		public static event EngineUpdateDelegate FixedUpdate;


		public static ECSWorld World { get; private set; }
		public static float Time => elapsedMs / 1000f;

		public static float fixedUpdateStep = 1 / 30f;
		public static float targetFps = 60;

		private static bool shouldRun = false;
		private static long elapsedMs = 0;


		//TODO: Add settings and other stuff
		public static void Initialize() {
			World = ECSWorld.CreateMain();
			Window.Initialize(World);
			World.Initialize(true);

			Update += World.InvokeUpdate;
			Render += World.InvokeRender;
			FixedUpdate += World.InvokeFixedUpdate;

			Window.OnWindowClose += Stop;
		}

		public static void Run()
		{
			shouldRun = true;

			Stopwatch watch = new Stopwatch();
			watch.Start();
			elapsedMs = watch.ElapsedMilliseconds;
			float fixedUpdateTimer = 0;
			while (shouldRun)
			{
				long newTimeMs = watch.ElapsedMilliseconds;
				float deltaTime = (newTimeMs - elapsedMs) / 1000f;
				elapsedMs = newTimeMs;
				fixedUpdateTimer += deltaTime;

				Update?.Invoke(deltaTime);

				if (fixedUpdateTimer > fixedUpdateStep)
				{
					FixedUpdate?.Invoke(fixedUpdateStep);
					fixedUpdateTimer -= fixedUpdateStep;
				}

				Render?.Invoke(deltaTime);

				if (targetFps > 0)
				{
					float targetFrameTime = 1f / targetFps;
					float sleepTime = targetFrameTime - deltaTime;
					if (sleepTime > 0)
					{
						Thread.Sleep((int)(sleepTime * 1000f));
					}
				}
			}

			Update -= World.InvokeUpdate;
			Render -= World.InvokeRender;
			FixedUpdate -= World.InvokeFixedUpdate;

			//Cleanup
			World.CleanUp();
			if (Window.window.Exists) {
				Window.Close();
			}
		}

		public static void Stop()
		{
			shouldRun = false;
		}

	}
}
