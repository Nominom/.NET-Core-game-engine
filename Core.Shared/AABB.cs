using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using GlmSharp;

namespace Core.Shared
{
	public struct AabbBounds {
		public readonly vec3 min;
		public readonly vec3 max;

		public AabbBounds(vec3 point) {
			min = point;
			max = point;
		}

		public AabbBounds(vec3 min, vec3 max) {
			this.min = min;
			this.max = max;
		}

		public vec3 Center => (min + max) / 2;
		public vec3 Size => max - min;
		public vec3 Extents => Size * 2;

		public AabbBounds Encapsulate(vec3 point) {
			vec3 newMin = new vec3(
				MathF.Min(min.x, point.x),
				MathF.Min(min.y, point.y),
				MathF.Min(min.z, point.z)
				);
			vec3 newMax = new vec3(
				MathF.Max(max.x, point.x),
				MathF.Max(max.y, point.y),
				MathF.Max(max.z, point.z)
			);
			return new AabbBounds(newMin, newMax);
		}

		public bool Contains(vec3 point) {
			if (point.x < min.x || point.x > max.x ||
			    point.y < min.y || point.y > max.y ||
			    point.z < min.z || point.x > max.z) {
				return false;
			}
			return true;
		}

		public AabbBounds TransformAabb(mat4 transformation) {
			Span<vec3> points = stackalloc []{
				new vec3(max.x, max.y, max.z),
				new vec3(max.x, max.y, min.z),
				new vec3(max.x, min.y, max.z),
				new vec3(max.x, min.y, min.z),
				new vec3(min.x, max.y, max.z),
				new vec3(min.x, max.y, min.z),
				new vec3(min.x, min.y, max.z),
				new vec3(min.x, min.y, min.z)
			};

			points[0] =  (transformation * new vec4(points[0], 1)).xyz; //vec3.Transform(points[0], transformation);
			AabbBounds newBounds = new AabbBounds(points[0]);

			for (int i = 1; i < points.Length; i++) {
				points[i] = (transformation * new vec4(points[i], 1)).xyz;
				newBounds = newBounds.Encapsulate(points[i]);
			}

			return newBounds;
		}
	}
}
