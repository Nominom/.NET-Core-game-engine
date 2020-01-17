

namespace Core.Shared
{
	public struct Color {
		public float r;
		public float g;
		public float b;
		public float a;

		public static readonly Color white = new Color(1,1,1);
		public static readonly Color black = new Color(0,0,0);
		public static readonly Color transparent = new Color(1,1,1, 0);
		public static readonly Color red = new Color(1,0,0);
		public static readonly Color green = new Color(0,1,0);
		public static readonly Color blue = new Color(0,0,1);


		public Color(float r, float g, float b) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = 1;
		}

		public Color(float r, float g, float b, float a) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}
	}
}
