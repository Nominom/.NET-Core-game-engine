using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Graphics
{
	public static class RenderUtilities {


		private static Mesh fullScreenQuad = null;
		private static Mesh unitCube = null;

		public static Mesh FullScreenQuad {
			get {
				if (fullScreenQuad != null) return fullScreenQuad;
				if (GraphicsContext.initialized) {
					fullScreenQuad = new Mesh(GraphicsContext.graphicsDevice, BasicMeshConstructor.FullScreenQuad());
				}
				return fullScreenQuad;
			}
		}

		public static Mesh UnitCube {
			get {
				if (unitCube != null) return unitCube;
				if (GraphicsContext.initialized) {
					unitCube = new Mesh(GraphicsContext.graphicsDevice, BasicMeshConstructor.UnitCube());
				}
				return unitCube;
			}
		}



		internal static void DisposeAllUtils() {
			FullScreenQuad?.Dispose();
			UnitCube?.Dispose();
		}
	}
}
