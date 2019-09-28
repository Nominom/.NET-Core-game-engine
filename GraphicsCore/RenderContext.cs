using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;

namespace Core.Graphics
{
	public struct RenderContext {
		public Camera activeCamera;
		public Vector3 activeCameraPosition;
		public Quaternion activeCameraRotation;
		public Framebuffer mainFrameBuffer;
		public Matrix4x4 viewMatrix;
		public Matrix4x4 projectionMatrix;

		public readonly CommandList CreateCommandList() {
			//TODO: Not thread-safe. Create a new CommandList from a pool?
			return GraphicsContext._commandList;
		}

		public readonly void SubmitCommands(CommandList cmd) {
			GraphicsContext._graphicsDevice.SubmitCommands(cmd);
		}

		public readonly unsafe void UpdateBuffer<T>(DeviceBuffer buffer, uint bufferOffsetInBytes, ReadOnlySpan<T> data) where T : unmanaged {
			fixed (T* ptr = data) {
				IntPtr p = new IntPtr(ptr);
				GraphicsContext._graphicsDevice.UpdateBuffer(
					buffer,
					bufferOffsetInBytes,
					p,
					Math.Min(
						buffer.SizeInBytes - bufferOffsetInBytes, 
						(uint)(data.Length * Marshal.SizeOf<T>())
						)
				);
			}
			
		}
	}
}
