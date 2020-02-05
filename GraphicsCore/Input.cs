using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace Core.Graphics
{
	public static class Input
	{
		private static Dictionary<Key, bool> keyStateDict = new Dictionary<Key, bool>();
		private static Dictionary<Key, bool> keyDownDict = new Dictionary<Key, bool>();
		private static Dictionary<Key, bool> keyUpDict = new Dictionary<Key, bool>();

		private static Dictionary<MouseButton, bool> mouseStateDict = new Dictionary<MouseButton, bool>();
		private static Dictionary<MouseButton, bool> mouseDownDict = new Dictionary<MouseButton, bool>();
		private static Dictionary<MouseButton, bool> mouseUpDict = new Dictionary<MouseButton, bool>();

		private static Vector2 lastMousePosition;
		private static Vector2 mousePosition;
		private static float wheelDelta;

		internal static void UpdateInput(InputSnapshot snapshot) {
			keyDownDict.Clear();
			keyUpDict.Clear();
			mouseDownDict.Clear();
			mouseUpDict.Clear();

			lastMousePosition = mousePosition;
			mousePosition = snapshot.MousePosition;
			wheelDelta = snapshot.WheelDelta;

			foreach (KeyEvent keyEvent in snapshot.KeyEvents) {
				keyStateDict[keyEvent.Key] = keyEvent.Down;

				if (keyEvent.Down) {
					keyDownDict[keyEvent.Key] = true;
				}
				else {
					keyUpDict[keyEvent.Key] = true;
				}
			}

			foreach (MouseEvent mouseEvent in snapshot.MouseEvents) {
				mouseStateDict[mouseEvent.MouseButton] = mouseEvent.Down;

				if (mouseEvent.Down) {
					mouseDownDict[mouseEvent.MouseButton] = true;
				}
				else {
					mouseUpDict[mouseEvent.MouseButton] = true;
				}
			}
		}

		public static bool GetKeyDown(Key key) {
			if (keyDownDict.TryGetValue(key, out bool value)) {
				return value;
			}
			return false;
		}

		public static bool GetKeyUp(Key key) {
			if (keyUpDict.TryGetValue(key, out bool value)) {
				return value;
			}
			return false;
		}

		public static bool GetKey(Key key) {
			if (keyStateDict.TryGetValue(key, out bool value)) {
				return value;
			}
			return false;
		}

		public static bool GetMouseDown(MouseButton button) {
			if (mouseDownDict.TryGetValue(button, out bool value)) {
				return value;
			}
			return false;
		}

		public static bool GetMouseUp(MouseButton button) {
			if (mouseUpDict.TryGetValue(button, out bool value)) {
				return value;
			}
			return false;
		}

		public static bool GetMouseButton(MouseButton button) {
			if (mouseStateDict.TryGetValue(button, out bool value)) {
				return value;
			}
			return false;
		}

		public static Vector2 GetMousePosition() {
			return mousePosition;
		}

		public static Vector2 GetMouseDelta() {
			return mousePosition - lastMousePosition;
		}

		public static float GetMouseWheelDelta() {
			return wheelDelta;
		}
	}
}
