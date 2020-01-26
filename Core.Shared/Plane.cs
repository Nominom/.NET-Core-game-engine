using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

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
		public float SignedDistance(Vector3 point) {
			return a * point.X + b * point.Y + c * point.Z + d;
		}

		public HalfSpace ClassifyPoint(Vector3 point) {
			float distanceToPoint = SignedDistance(point);

			if (distanceToPoint < 0) return HalfSpace.Negative;
			if (distanceToPoint > 0) return HalfSpace.Positive;
			return HalfSpace.OnPlane;
		}

		public Vector3 Normal() {
			return new Vector3(a, b, c);
		}

		//Find the line of intersection between two planes.
//The inputs are two game objects which represent the planes.
//The outputs are a point on the line and a vector which indicates it's direction.
		public bool PlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, Plane otherPlane){
   
			linePoint = Vector3.Zero;
			lineVec = Vector3.Zero;
   
			//Get the normals of the planes.
			Vector3 plane1Normal = Normal();
			Vector3 plane2Normal = otherPlane.Normal();
   
			//We can get the direction of the line of intersection of the two planes by calculating the
			//cross product of the normals of the two planes. Note that this is just a direction and the line
			//is not fixed in space yet.
			lineVec = Vector3.Cross(plane1Normal, plane2Normal);
   
			//Next is to calculate a point on the line to fix it's position. This is done by finding a vector from
			//the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
			//errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
			//the cross product of the normal of plane2 and the lineDirection.      
			Vector3 ldir = Vector3.Cross(plane2Normal, lineVec);      
   
			float numerator = Vector3.Dot(plane1Normal, ldir);
   
			//Prevent divide by zero.
			if(MathF.Abs(numerator) > 0.000001f){
       
				Vector3 plane1ToPlane2 = (plane1Normal * d) - (plane2Normal * otherPlane.d);
				float t = Vector3.Dot(plane1Normal, plane1ToPlane2) / numerator;
				linePoint = (plane2Normal * otherPlane.d) + t * ldir;
				return true;
			}

			return false;
		}

		//Get the intersection between a line and a plane. 
		//If the line and plane are not parallel, the function outputs true, otherwise false.
		public bool LineIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec){
 
			float length;
			float dotNumerator;
			float dotDenominator;
			Vector3 vector;
			intersection = Vector3.Zero;
			Vector3 planeNormal = Normal();
			Vector3 planePoint = planeNormal * d;
 
			//calculate the distance between the linePoint and the line-plane intersection point
			dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
			dotDenominator = Vector3.Dot(lineVec, planeNormal);
 
			//line and plane are not parallel
			if(dotDenominator != 0.0f){
				length =  dotNumerator / dotDenominator;
 
				//create a vector from the linePoint to the intersection point
				vector =  Vector3.Normalize(lineVec) * length;
 
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
