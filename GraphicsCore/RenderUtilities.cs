using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Graphics
{
	public static class RenderUtilities {
		public static Mesh FullScreenQuad { get; } = BasicMeshConstructor.FullScreenQuad();
		public static Mesh UnitCube { get; } = BasicMeshConstructor.UnitCube();



		internal static void DisposeAllUtils() {
			FullScreenQuad.Dispose();
			UnitCube.Dispose();
		}
	}
}
