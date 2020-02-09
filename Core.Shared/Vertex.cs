using System.Numerics;
using GlmSharp;

namespace Core.Shared
{
	public struct Vertex {
		public vec3 position;
		public vec3 normal;
		public vec2 uv0;

		public Vertex(vec3 position, vec3 normal, vec2 uv) {
			this.position = position;
			this.normal = normal;
			this.uv0 = uv;
		}
	}
}
