using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using GlmSharp;

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
		public static quat LookAt(vec3 sourcePoint, vec3 destPoint, vec3 up) {

			var mat = mat4.LookAt(vec3.Zero, sourcePoint - destPoint, up);
			return mat.ToQuaternion.Inverse;
			//vec3 forwardVector = vec3.Normalize(sourcePoint - destPoint);

			//vec3 zaxis = forwardVector;
			//vec3 xaxis = vec3.Normalize(vec3.Cross(up, zaxis));
			//vec3 yaxis = vec3.Cross(zaxis, xaxis);

			//mat4 result = mat4.Identity;

			//result.M11 = xaxis.x;
			//result.M12 = yaxis.x;
			//result.M13 = zaxis.x;
			//result.M14 = 0.0f;
			//result.M21 = xaxis.y;
			//result.M22 = yaxis.y;
			//result.M23 = zaxis.y;
			//result.M24 = 0.0f;
			//result.M31 = xaxis.z;
			//result.M32 = yaxis.z;
			//result.M33 = zaxis.z;
			//result.M34 = 0.0f;
			//result.M44 = 1.0f;
			//return Quaternion.CreateFromRotationMatrix(result);

			//return LookRotation(forwardVector, vec3.UnitY);

			//float dot = vec3.Dot(vec3.UnitZ, forwardVector);

			//if (Math.Abs(dot - (-1.0f)) < 0.000001f)
			//{
			//	return new Quaternion(vec3.UnitY.x, vec3.UnitY.y, vec3.UnitY.z, 3.1415926535897932f);
			//}
			//if (Math.Abs(dot - (1.0f)) < 0.000001f)
			//{
			//	return Quaternion.Identity;
			//}

			//float rotAngle = (float)Math.Acos(dot);
			//vec3 rotAxis = vec3.Cross(vec3.UnitZ, forwardVector);
			//rotAxis = vec3.Normalize(rotAxis);
			//return Quaternion.CreateFromAxisAngle(rotAxis, rotAngle);
		}

		public static void vec3OrthoNormalize(ref vec3 normal, ref vec3 tangent) {

			normal = normal.Normalized;
			vec3 proj = normal * vec3.Dot(tangent, normal);
			tangent = (tangent - proj);
			tangent = tangent.Normalized;
		}

		public static quat LookRotation(vec3 lookAt, vec3 up)
		{
			/*Vector forward = lookAt.Normalized();
			Vector right = Vector::Cross(up.Normalized(), forward);
			Vector up = Vector::Cross(forward, right);*/

			vec3 forward = lookAt.Normalized;
			vec3OrthoNormalize(ref up, ref forward); // Keeps up the same, make forward orthogonal to up
			vec3 right = vec3.Cross(up, forward);

			quat ret = new quat();
			ret.w = MathF.Sqrt(1.0f + right.x + up.y + forward.z) * 0.5f;
			float w4_recip = 1.0f / (4.0f * ret.w);
			ret.x = (forward.y - up.z) * w4_recip;
			ret.y = (right.z - forward.x) * w4_recip;
			ret.z = (up.x - right.y) * w4_recip;

			return ret;
		}
	}
}
