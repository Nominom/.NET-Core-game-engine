using System;
using System.Collections.Generic;
using System.Text;
using Core.Shared;
using Vulkan;

namespace Core.Graphics.VulkanBackend
{
	public class GpuMesh : IDisposable
	{
		public readonly DeviceBuffer vertices;
		public readonly DeviceBuffer indices;
		public readonly VkIndexType indexType = VkIndexType.Uint16;
		public readonly uint indexCount;

		public GpuMesh(GraphicsDevice device, ReadOnlySpan<Vertex> vertices, ReadOnlySpan<UInt16> indices) {
			indexCount = (uint)indices.Length;
			this.vertices = DeviceBuffer.CreateFrom(device, vertices, BufferUsageFlags.VertexBuffer, BufferMemoryUsageHint.Static);
			this.indices = DeviceBuffer.CreateFrom(device, indices, BufferUsageFlags.IndexBuffer, BufferMemoryUsageHint.Static);
		}

		public void Dispose() {
			Free();
			#if DEBUG
			GC.SuppressFinalize(this);
			#endif
		}

		#if DEBUG
		~GpuMesh() {
			Console.WriteLine("A subMesh was not disposed of correctly.");
			Free();
		}
		#endif

		private void Free() {
			vertices?.Dispose();
			indices?.Dispose();
		}
	}
}
