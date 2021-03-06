﻿using System;
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
		public readonly VkIndexType indexType = VkIndexType.Uint32;
		public readonly uint indexCount;

		public GpuMesh(GraphicsDevice device, ReadOnlySpan<Vertex> vertices, ReadOnlySpan<UInt32> indices, bool readWrite) {
			indexCount = (uint)indices.Length;
			this.vertices = DeviceBuffer.CreateFrom(device, vertices, BufferUsageFlags.VertexBuffer, 
				readWrite ? BufferMemoryUsageHint.Dynamic : BufferMemoryUsageHint.Static);
			this.indices = DeviceBuffer.CreateFrom(device, indices, BufferUsageFlags.IndexBuffer, 
				readWrite ? BufferMemoryUsageHint.Dynamic : BufferMemoryUsageHint.Static);
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
