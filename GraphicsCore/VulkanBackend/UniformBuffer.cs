using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Core.Graphics.VulkanBackend
{

	public abstract class UniformBuffer : DeviceBuffer {
		public uint location;
		protected UniformBuffer(GraphicsDevice device, ulong size, BufferUsageFlags flags,
			BufferMemoryUsageHint usageHint, uint location) : base(device, size, flags, usageHint) {
			this.location = location;
		}
	}
	public class UniformBuffer<T> : UniformBuffer where T : unmanaged {
		public UniformBuffer(GraphicsDevice device, uint location)
			: base(device, (ulong)Marshal.SizeOf<T>(), BufferUsageFlags.UniformBuffer, BufferMemoryUsageHint.Dynamic, location) { }
	}
}
