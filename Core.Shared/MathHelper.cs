using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Core.Shared
{
	public static class MathHelper
	{
		/// <summary>
		/// Evaluates a rotation needed to be applied to an object positioned at sourcePoint to face destPoint
		/// </summary>
		/// <param name="sourcePoint">Coordinates of source point</param>
		/// <param name="destPoint">Coordinates of destionation point</param>
		/// <returns></returns>
		public static Quaternion LookAt(Vector3 sourcePoint, Vector3 destPoint, Vector3 up)
		{
			Vector3 forwardVector = Vector3.Normalize(sourcePoint - destPoint);

			Vector3 zaxis = forwardVector;
			Vector3 xaxis = Vector3.Normalize(Vector3.Cross(up, zaxis));
			Vector3 yaxis = Vector3.Cross(zaxis, xaxis);
 
			Matrix4x4 result = Matrix4x4.Identity;
 
			result.M11 = xaxis.X;
			result.M12 = yaxis.X;
			result.M13 = zaxis.X;
			result.M14 = 0.0f;
			result.M21 = xaxis.Y;
			result.M22 = yaxis.Y;
			result.M23 = zaxis.Y;
			result.M24 = 0.0f;
			result.M31 = xaxis.Z;
			result.M32 = yaxis.Z;
			result.M33 = zaxis.Z;
			result.M34 = 0.0f;
			result.M44 = 1.0f;
			return Quaternion.CreateFromRotationMatrix(result);

			//return LookRotation(forwardVector, Vector3.UnitY);

			float dot = Vector3.Dot(Vector3.UnitZ, forwardVector);

			if (Math.Abs(dot - (-1.0f)) < 0.000001f)
			{
				return new Quaternion(Vector3.UnitY.X, Vector3.UnitY.Y, Vector3.UnitY.Z, 3.1415926535897932f);
			}
			if (Math.Abs(dot - (1.0f)) < 0.000001f)
			{
				return Quaternion.Identity;
			}

			float rotAngle = (float)Math.Acos(dot);
			Vector3 rotAxis = Vector3.Cross(Vector3.UnitZ, forwardVector);
			rotAxis = Vector3.Normalize(rotAxis);
			return Quaternion.CreateFromAxisAngle(rotAxis, rotAngle);
		}

		public static void Vector3OrthoNormalize(ref Vector3 normal, ref Vector3 tangent) {
			normal = Vector3.Normalize(normal);
			Vector3 proj = Vector3.Multiply(normal, Vector3.Dot(tangent, normal));
			tangent = Vector3.Subtract(tangent, proj);
			tangent = Vector3.Normalize(tangent);
		}

		public static Quaternion LookRotation(Vector3 lookAt, Vector3 up)
		{
			/*Vector forward = lookAt.Normalized();
			Vector right = Vector::Cross(up.Normalized(), forward);
			Vector up = Vector::Cross(forward, right);*/

			Vector3 forward = Vector3.Normalize(lookAt);
			Vector3OrthoNormalize(ref up, ref forward); // Keeps up the same, make forward orthogonal to up
			Vector3 right = Vector3.Cross(up, forward);

			Quaternion ret = new Quaternion();
			ret.W = MathF.Sqrt(1.0f + right.X + up.Y + forward.Z) * 0.5f;
			float w4_recip = 1.0f / (4.0f * ret.W);
			ret.X = (forward.Y - up.Z) * w4_recip;
			ret.Y = (right.Z - forward.X) * w4_recip;
			ret.Z = (up.X - right.Y) * w4_recip;

			return ret;
		}
	}
}
