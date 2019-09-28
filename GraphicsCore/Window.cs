using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Core.Graphics
{
	public static class Window {
		internal static Sdl2Window window;
		internal static bool initialized;
		private static ECSWorld world;

		public static event Action OnWindowClose;
		public static event Action<int,int> OnWindowResize;


		public static void Initialize(ECSWorld world) {
			if (initialized) return;
			if(world == null) throw  new ArgumentNullException(nameof(world), "The ECSWorld provided can not be null.");
			if(!world.IsMainWorld) throw new ArgumentException("The ECSWorld of the window needs to be the mainWorld", nameof(world));

			WindowCreateInfo windowCI = new WindowCreateInfo()
			{
				X = 100,
				Y = 100,
				WindowWidth = 960,
				WindowHeight = 540,
				WindowTitle = "Veldrid Tutorial",
				WindowInitialState = WindowState.Normal
			};

			window = VeldridStartup.CreateWindow(ref windowCI);
			window.Closed += () => OnWindowClose?.Invoke();
			window.Resized += () => OnWindowResize?.Invoke(window.Width, window.Height);

			world.EarlyUpdate += WorldOnEarlyUpdate;

			initialized = true;
			GraphicsContext.Initialize(world);
		}

		public static bool Exists() => window.Exists;

		public static void Close() {
			window.Close();
		}

		private static void WorldOnEarlyUpdate(float deltatime, ECSWorld ecsWorld) {
			if (window.Exists) {
				window.PumpEvents();
			}
		}

		private static void EventHandler(ref SDL_Event ev) {
			//TODO: Handle events
		}
	}
}
