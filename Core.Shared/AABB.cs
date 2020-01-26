using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Core.Shared
{
	public struct AabbBounds {
		public readonly Vector3 min;
		public readonly Vector3 max;

		public AabbBounds(Vector3 point) {
			min = point;
			max = point;
		}

		public AabbBounds(Vector3 min, Vector3 max) {
			this.min = min;
			this.max = max;
		}

		public Vector3 Center => Vector3.Divide(Vector3.Add(min, max), 2);
		public Vector3 Size => Vector3.Subtract(max, min);
		public Vector3 Extents => Vector3.Divide(Size, 2);

		public AabbBounds Encapsulate(Vector3 point) {
			Vector3 newMin = new Vector3(
				MathF.Min(min.X, point.X),
				MathF.Min(min.Y, point.Y),
				MathF.Min(min.Z, point.Z)
				);
			Vector3 newMax = new Vector3(
				MathF.Max(max.X, point.X),
				MathF.Max(max.Y, point.Y),
				MathF.Max(max.Z, point.Z)
			);
			return new AabbBounds(newMin, newMax);
		}

		public bool Contains(Vector3 point) {
			if (point.X < min.X || point.X > max.X ||
			    point.Y < min.Y || point.Y > max.Y ||
			    point.Z < min.Z || point.X > max.Z) {
				return false;
			}
			return true;
		}

		public AabbBounds TransformAabb(Matrix4x4 transformation) {
			Span<Vector3> points = stackalloc []{
				new Vector3(max.X, max.Y, max.Z),
				new Vector3(max.X, max.Y, min.Z),
				new Vector3(max.X, min.Y, max.Z),
				new Vector3(max.X, min.Y, min.Z),
				new Vector3(min.X, max.Y, max.Z),
				new Vector3(min.X, max.Y, min.Z),
				new Vector3(min.X, min.Y, max.Z),
				new Vector3(min.X, min.Y, min.Z)
			};

			points[0] = Vector3.Transform(points[0], transformation);
			AabbBounds newBounds = new AabbBounds(points[0]);

			for (int i = 1; i < points.Length; i++) {
				points[i] = Vector3.Transform(points[i], transformation);
				newBounds = newBounds.Encapsulate(points[i]);
			}

			return newBounds;
		}
	}
}
