using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using GlmSharp;

namespace Core.Shared
{
	public struct Frustum {
		public Plane left, right, bottom, top, near, far;

		public Frustum(Plane left, Plane right, Plane bottom, Plane top, Plane near, Plane far) {
			this.left = left;
			this.right = right;
			this.bottom = bottom;
			this.top = top;
			this.near = near;
			this.far = far;
		}


		private bool AabbCullPlaneCheck(AabbBounds aabb, Plane plane) {
			vec3 planeNormal = plane.Normal();
			vec3 axisVert = new vec3();
			// x-axis
			if(plane.a < 0.0f)    // Which AABB vertex is furthest down (plane normals direction) the x axis
				axisVert.x = aabb.min.x; // min x plus tree positions x
			else
				axisVert.x = aabb.max.x; // max x plus tree positions x

			// y-axis
			if(plane.b < 0.0f)    // Which AABB vertex is furthest down (plane normals direction) the y axis
				axisVert.y = aabb.min.y;
			else
				axisVert.y = aabb.max.y;
            
			// z-axis
			if(plane.c < 0.0f)    // Which AABB vertex is furthest down (plane normals direction) the z axis
				axisVert.z = aabb.min.z;
			else
				axisVert.z = aabb.max.z;

			// Now we get the signed distance from the AABB vertex that's furthest down the frustum planes normal,
			// and if the signed distance is negative, then the entire bounding box is behind the frustum plane, which means
			// that it should be culled

			//ClassifyPoint
			return vec3.Dot(planeNormal, axisVert) + plane.d < 0.0f;
		}

		public bool AabbCull(AabbBounds aabb) {
			if (AabbCullPlaneCheck(aabb, left)) return true;
			if (AabbCullPlaneCheck(aabb, right)) return true;
			if (AabbCullPlaneCheck(aabb, bottom)) return true;
			if (AabbCullPlaneCheck(aabb, top)) return true;
			if (AabbCullPlaneCheck(aabb, near)) return true;
			if (AabbCullPlaneCheck(aabb, far)) return true;
			return false;
		}


		private bool Plane3Intersect(Plane plane1, Plane plane2, Plane plane3, out vec3 point) {
			point = vec3.Zero;
			if (!plane1.PlaneIntersection(out vec3 linePoint, out vec3 lineVec, plane2)) {
				return false;
			}
			if (!plane3.LineIntersection(out point, linePoint, lineVec)) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// Fills an array of vec3's with this Frustums vertices.
		/// Order is near_tl, near_tr near_br near_bl, far_tl, far_tr far_br far_bl
		/// </summary>
		/// <param name="array"></param>
		public void Vertices(vec3[] array) {
			if(array == null || array.Length != 8)
			{
				throw new InvalidOperationException();
			}

			Plane3Intersect(near, top, left, out array[0]);
			Plane3Intersect(near, top, right, out array[1]);
			Plane3Intersect(near, bottom, right, out array[2]);
			Plane3Intersect(near, bottom, left, out array[3]);

			Plane3Intersect(far, top, left, out array[4]);
			Plane3Intersect(far, top, right, out array[5]);
			Plane3Intersect(far, bottom, right, out array[6]);
			Plane3Intersect(far, bottom, left, out array[7]);
		}
	}
}
