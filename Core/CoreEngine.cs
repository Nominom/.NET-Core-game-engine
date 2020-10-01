using System;
using System.Diagnostics;
using System.Threading;
using Core.AssetSystem;
using Core.ECS;
using Core.Graphics;
using Core.Profiling;



namespace Core
{
	public static class CoreEngine
	{
		public delegate void EngineUpdateDelegate(float deltaTime);

		public static event EngineUpdateDelegate Update;
		public static event EngineUpdateDelegate Render;
		public static event EngineUpdateDelegate FixedUpdate;


		public static ECSWorld World { get; private set; }
		public static long FrameNumber { get; private set; } = 0;
		public static float Time => elapsedMs / 1000f;

		public static float fixedUpdateStep = 1 / 30f;
		public static float maxFixedUpdateCatchup = 0.1f;
		public static float targetFps = 60;

		private static bool shouldRun = false;
		private static long elapsedMs = 0;


		//TODO: Add settings and other stuff
		public static void Initialize() {
			World = ECSWorld.CreateMain();
			Window.Initialize(World);
			
			Asset.LoadAssetPackage("data/defaultassets.dat");

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
			Stopwatch frameTimeWatch = new Stopwatch();
			watch.Start();
			elapsedMs = watch.ElapsedMilliseconds;
			float fixedUpdateTimer = 0;
			
			while (shouldRun)
			{
				Profiler.StartFrame(FrameNumber);
				frameTimeWatch.Restart();

				long newTimeMs = watch.ElapsedMilliseconds;
				float deltaTime = (newTimeMs - elapsedMs) / 1000f;
				if (deltaTime > 1) {
					deltaTime = fixedUpdateStep;
				}
				elapsedMs = newTimeMs;
				fixedUpdateTimer += deltaTime;

				int fixedUpdatesPerformed = 0;
				while (fixedUpdateTimer > fixedUpdateStep) {
					fixedUpdatesPerformed++;
					FixedUpdate?.Invoke(fixedUpdateStep);
					fixedUpdateTimer -= fixedUpdateStep;
					if (frameTimeWatch.ElapsedMilliseconds / 1000f > maxFixedUpdateCatchup) {
						fixedUpdateTimer = 0;
						deltaTime = fixedUpdateStep * fixedUpdatesPerformed;
						elapsedMs = watch.ElapsedMilliseconds;
					}
				}

				Update?.Invoke(deltaTime);

				Render?.Invoke(deltaTime);

				Profiler.EndFrame();
				frameTimeWatch.Stop();

				if (targetFps > 0)
				{
					float targetFrameTime = 1f / targetFps;
					float sleepTime = targetFrameTime - (frameTimeWatch.ElapsedMilliseconds / 1000f);
					if (sleepTime > 0)
					{
						Thread.Sleep((int)(sleepTime * 1000f));
					}
				}

				FrameNumber++;
			}

			Update -= World.InvokeUpdate;
			Render -= World.InvokeRender;
			FixedUpdate -= World.InvokeFixedUpdate;

			//Cleanup
			World.CleanUp();
			if (Window.window.Exists) {
				Window.Close();
			}
			GraphicsContext.DisposeResources(); // Needs to be called after everything else has been cleaned up
		}

		public static void Stop()
		{
			shouldRun = false;
		}

	}
}
