using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.Shared;

namespace Core.Graphics
{
	public class DebugMeshConvexHullRenderer : ISharedComponent
	{
		public Mesh mesh;

		public DebugMeshConvexHullRenderer(MeshData data) {
			var hull = data.ConvexHull;
			MeshData newData = new MeshData();
			newData.subMeshes = new SubMeshData[]{hull};
			mesh = new Mesh(GraphicsContext.graphicsDevice, newData);
		}
	}
}
