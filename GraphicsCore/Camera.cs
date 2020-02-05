using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Core.ECS;
using Core.Shared;
using Plane = Core.Shared.Plane;

namespace Core.Graphics
{
	public class Camera : ISharedComponent {
		public float farPlane;
		public float nearPlane;
		public float fow;
		public float aspect;

		public Matrix4x4 ViewMatrix(Vector3 position, Quaternion rotation) {
			//var matrix = Matrix4x4.Identity;
			//matrix = Matrix4x4.Transform(matrix, rotation);
			//var res = Matrix4x4.Multiply(matrix, Matrix4x4.CreateTranslation(-position));
			//Matrix4x4.Invert(res, out res);
			//return res;
			//return Matrix4x4.CreateLookAt(position, Vector3.Transform(Vector3.UnitZ, rotation),
			//	Vector3.Transform(Vector3.UnitY, rotation));

			rotation = Quaternion.Inverse(rotation);


			Vector3 zaxis = Vector3.Transform(Vector3.UnitZ, rotation);
			Vector3 xaxis = Vector3.Transform(Vector3.UnitX, rotation);
			Vector3 yaxis = Vector3.Transform(Vector3.UnitY, rotation);
 
			Matrix4x4 result;
 
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
			result.M41 = -Vector3.Dot(xaxis, position);
			result.M42 = -Vector3.Dot(yaxis, position);
			result.M43 = -Vector3.Dot(zaxis, position);
			result.M44 = 1.0f;
 
			return result;
		}

		public Matrix4x4 ProjectionMatrix(bool invertY) {
			float fowRadians = (MathF.PI / 180) * fow;
			var proj = Matrix4x4.CreatePerspectiveFieldOfView(fowRadians, aspect, nearPlane, farPlane);
			//Vulkan silliness
			if (invertY) {
				proj.M22 = -proj.M22;
			}
			return proj;
		}

		public Frustum GetFrustum(Vector3 position, Quaternion rotation) {
			Matrix4x4 mat = Matrix4x4.Multiply(ViewMatrix(position, rotation), ProjectionMatrix(false));

			Frustum frustum = new Frustum();
			//frustum.left = new Plane(
			//	mat.M41 + mat.M11,
			//	mat.M42 + mat.M12,
			//	mat.M43 + mat.M13,
			//	mat.M44 + mat.M14
			//).Normalize();
			//frustum.right = new Plane(
			//	mat.M41 - mat.M11,
			//	mat.M42 - mat.M12,
			//	mat.M43 - mat.M13,
			//	mat.M44 - mat.M14
			//).Normalize();
			//frustum.bottom = new Plane(
			//	mat.M41 + mat.M21,
			//	mat.M42 + mat.M22,
			//	mat.M43 + mat.M23,
			//	mat.M44 + mat.M24
			//).Normalize();
			//frustum.top = new Plane(
			//	mat.M41 - mat.M21,
			//	mat.M42 - mat.M22,
			//	mat.M43 - mat.M23,
			//	mat.M44 - mat.M24
			//).Normalize();
			//frustum.near = new Plane(
			//	mat.M41 + mat.M31,
			//	mat.M42 + mat.M32,
			//	mat.M43 + mat.M33,
			//	mat.M44 + mat.M34
			//).Normalize();
			//frustum.far = new Plane(
			//	mat.M41 - mat.M31,
			//	mat.M42 - mat.M32,
			//	mat.M43 - mat.M33,
			//	mat.M44 - mat.M34
			//).Normalize();

			frustum.left = new Plane(
				mat.M14 + mat.M11,
				mat.M24 + mat.M21,
				mat.M34 + mat.M31,
				mat.M44 + mat.M41
			).Normalize();
			frustum.right = new Plane(
				mat.M14 - mat.M11,
				mat.M24 - mat.M21,
				mat.M34 - mat.M31,
				mat.M44 - mat.M41
			).Normalize();
			frustum.bottom = new Plane(
				mat.M14 + mat.M12,
				mat.M24 + mat.M22,
				mat.M34 + mat.M32,
				mat.M44 + mat.M42
			).Normalize();
			frustum.top = new Plane(
				mat.M14 - mat.M12,
				mat.M24 - mat.M22,
				mat.M32 - mat.M32,
				mat.M44 - mat.M42
			).Normalize();
			frustum.near = new Plane(
				mat.M14 + mat.M13,
				mat.M24 + mat.M23,
				mat.M34 + mat.M33,
				mat.M44 + mat.M43
			).Normalize();
			frustum.far = new Plane(
				mat.M14 - mat.M13,
				mat.M24 - mat.M23,
				mat.M34 - mat.M33,
				mat.M44 - mat.M43
			).Normalize();

			//for (int i = 4; i--; ) left[i]      = mat[i][3] + mat[i][0];
			//for (int i = 4; i--; ) right[i]     = mat[i][3] - mat[i][0]; 
			//for (int i = 4; i--; ) bottom[i]    = mat[i][3] + mat[i][1];
			//for (int i = 4; i--; ) top[i]       = mat[i][3] - mat[i][1];
			//for (int i = 4; i--; ) near[i]      = mat[i][3] + mat[i][2];
			//for (int i = 4; i--; ) far[i]       = mat[i][3] - mat[i][2];
			return frustum;
		}
	}
}
