using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using GlmSharp;

namespace Core.Shared
{
	public struct Plane {
		public readonly float a, b, c, d;

		public enum HalfSpace
		{
			Negative = -1,
			OnPlane = 0,
			Positive = 1
		}

		public Plane(float a, float b, float c, float d) {
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
		}

		public Plane Normalize() {
			float mag = MathF.Sqrt(a * a + b * b + c * c);
			return new Plane(
				a / mag,
				b / mag, 
				c / mag, 
				d / mag);
		}

		public bool IsNormalized() {
			return MathF.Abs((a * a + b * b + c * c) - 1f) < float.Epsilon;
		}

		/// <summary>
		/// Signed distance to the plane. Will not return the "true" distance if the plane is not normalized.
		/// If the distance is < 0 the point lies in the negative halfspace.
		/// If the distance is 0 the point lies in the plane.
		/// If the distance is > 0 the point lies in the positive halfspace.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public float SignedDistance(vec3 point) {
			return a * point.x + b * point.y + c * point.z + d;
		}

		public HalfSpace ClassifyPoint(vec3 point) {
			float distanceToPoint = SignedDistance(point);

			if (distanceToPoint < 0) return HalfSpace.Negative;
			if (distanceToPoint > 0) return HalfSpace.Positive;
			return HalfSpace.OnPlane;
		}

		public vec3 Normal() {
			return new vec3(a, b, c);
		}


		public bool PlaneIntersection(out vec3 linePoint, out vec3 lineVec, Plane otherPlane){
   
			linePoint = vec3.Zero;
			lineVec = vec3.Zero;
   
			vec3 plane1Normal = Normal();
			vec3 plane2Normal = otherPlane.Normal();
   
			lineVec = vec3.Cross(plane1Normal, plane2Normal);
			
			vec3 ldir = vec3.Cross(plane2Normal, lineVec);      
   
			float numerator = vec3.Dot(plane1Normal, ldir);
   
			//Prevent divide by zero.
			if(MathF.Abs(numerator) > 0.000001f){
       
				vec3 plane1ToPlane2 = (plane1Normal * d) - (plane2Normal * otherPlane.d);
				float t = vec3.Dot(plane1Normal, plane1ToPlane2) / numerator;
				linePoint = (plane2Normal * otherPlane.d) + t * ldir;
				return true;
			}

			return false;
		}

		//Get the intersection between a line and a plane. 
		//If the line and plane are not parallel, the function outputs true, otherwise false.
		public bool LineIntersection(out vec3 intersection, vec3 linePoint, vec3 lineVec){
 
			float length;
			float dotNumerator;
			float dotDenominator;
			vec3 vector;
			intersection = vec3.Zero;
			vec3 planeNormal = Normal();
			vec3 planePoint = planeNormal * d;
 
			
			dotNumerator = vec3.Dot((planePoint - linePoint), planeNormal);
			dotDenominator = vec3.Dot(lineVec, planeNormal);
 
			//line and plane are not parallel
			if(dotDenominator != 0.0f){
				length =  dotNumerator / dotDenominator;
 
				//create a vector from the linePoint to the intersection point
				vector =  lineVec.Normalized * length;
 
				//get the coordinates of the line-plane intersection point
				intersection = linePoint + vector;	
 
				return true;	
			}
 
			//output not valid
			else{
				return false;
			}
		}
	}
}
