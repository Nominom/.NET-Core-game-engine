using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Graphics
{
	public struct Color {
		public float r;
		public float g;
		public float b;
		public float a;

		public Color(float r, float g, float b) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = 1;
		}
	}
}
