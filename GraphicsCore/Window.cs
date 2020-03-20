using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Core.ECS;
using Core.Profiling;
using Veldrid;
using Veldrid.Sdl2;

namespace Core.Graphics
{
	public static class Window {
		public static IntPtr WindowInstance { get; private set; }
		internal static Sdl2Window window;
		internal static bool initialized;
		private static ECSWorld world;

		public static event Action OnWindowClose;
		public static event Action<int,int> OnWindowResize;
		public static event Action<MouseWheelEventArgs> OnMouseWheel;
		public static event Action<MouseMoveEventArgs> OnMouseMove;
		public static event Action<MouseEvent> OnMouseDown;
		public static event Action<KeyEvent> OnKeyDown;

		public static float Aspect => ((float)window.Width / (float)window.Height);

		public static void Initialize(ECSWorld world) {
			if (initialized) return;
			if(world == null) throw  new ArgumentNullException(nameof(world), "The ECSWorld provided can not be null.");
			if(!world.IsMainWorld) throw new ArgumentException("The ECSWorld of the window needs to be the mainWorld", nameof(world));
			
			WindowInstance = Process.GetCurrentProcess().SafeHandle.DangerousGetHandle();
			window = new Sdl2Window("ECS", 50, 50, 1280, 720, SDL_WindowFlags.Resizable, threadedProcessing: false);
			window.X = 50;
			window.Y = 50;
			window.Visible = true;
			window.MouseWheel += (x) => OnMouseWheel?.Invoke(x);
			window.MouseMove += (x) => OnMouseMove?.Invoke(x);
			window.MouseDown += (x) => OnMouseDown?.Invoke(x);
			window.KeyDown += (x) => OnKeyDown?.Invoke(x);
			window.Closed += () => OnWindowClose?.Invoke();
			window.Resized += () => OnWindowResize?.Invoke(window.Width, window.Height);

			world.EarlyUpdate += PumpEvents;

			initialized = true;
			GraphicsContext.Initialize(world);
		}

		public static bool Exists() => window.Exists;

		public static void Close() {
			window.Close();
		}

		private static void PumpEvents(float deltatime, ECSWorld ecsWorld) {
			Profiler.StartMethod("PumpEvents");
			if (window.Exists) {
				InputSnapshot snapshot = window.PumpEvents();
				Input.UpdateInput(snapshot);
			}
			Profiler.EndMethod();
		}
	}
}
