using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;

namespace Core.Graphics
{
	public class SubMesh : IDisposable
	{
		internal DeviceBuffer vertexBuffer;
		internal DeviceBuffer indexBuffer;
		public bool IsLoaded { get; private set; } = false;
		public uint IndexCount => (uint)(indexData.Length);

		internal VertexData[] vertexData;
		internal ushort[] indexData;

		public SubMesh(VertexData[] vertices, ushort[] indices)
		{
			this.vertexData = vertices;
			this.indexData = indices;
		}

		public void LoadToGpu()
		{
			if (vertexData == null || vertexData.Length == 0)
			{
				throw new InvalidOperationException("Cannot load empty or null vertexData to GPU.");
			}
			if (IsLoaded)
			{
				vertexBuffer?.Dispose();
				indexBuffer?.Dispose();
			}

			vertexBuffer = GraphicsContext.factory.CreateBuffer(
				new BufferDescription((uint)(vertexData.Length * Marshal.SizeOf<VertexData>()),
					BufferUsage.VertexBuffer));
			indexBuffer = GraphicsContext.factory.CreateBuffer(
				new BufferDescription((uint)(sizeof(ushort) * indexData.Length), BufferUsage.IndexBuffer));


			GraphicsContext._graphicsDevice.UpdateBuffer(vertexBuffer, 0, vertexData);
			GraphicsContext._graphicsDevice.UpdateBuffer(indexBuffer, 0, indexData);


			IsLoaded = true;
		}

		public void Dispose()
		{
			vertexBuffer?.Dispose();
			indexBuffer?.Dispose();
			vertexBuffer = null;
			indexBuffer = null;
			IsLoaded = false;
		}
	}
}
