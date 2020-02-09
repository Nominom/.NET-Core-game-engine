using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Core.ECS;
using Core.Shared;
using GlmSharp;
using Plane = Core.Shared.Plane;

namespace Core.Graphics
{
	public class Camera : ISharedComponent
	{
		public float farPlane;
		public float nearPlane;
		public float fow;
		public float aspect;

		public mat4 ViewMatrix(vec3 position, quat rotation) {
			//return mat4.LookAt(new vec3(0, 0, -1), vec3.Zero, vec3.UnitY);
			//var cameraPosition = new vec3(4 , 10, 20);
			//return mat4.LookAt(cameraPosition, vec3.Zero, vec3.UnitY);

			vec3 right = rotation * -vec3.UnitX;
			vec3 up = rotation * vec3.UnitY;
			vec3 forward = rotation * -vec3.UnitZ;

			var translation = mat4.Translate(-position);

			var rotMat = new mat4(
				right.x, up.x, forward.x, 0,
				right.y, up.y, forward.y, 0,
				right.z, up.z, forward.z, 0,
				0, 0, 0, 1
				);

			return rotMat * translation;


			//var matrix = Matrix4x4.Identity;
			//matrix = Matrix4x4.Transform(matrix, rotation);
			//var res = Matrix4x4.Multiply(matrix, Matrix4x4.CreateTranslation(-pos));
			//Matrix4x4.Invert(res, out res);
			//return res;
			//return Matrix4x4.CreateLookAt(position, Vector3.Transform(Vector3.UnitZ, rotation),
			//	Vector3.Transform(Vector3.UnitY, rotation));

			//rotation = Quaternion.Inverse(rotation);


			//Vector3 zaxis = Vector3.Transform(Vector3.UnitZ, rotation);
			//Vector3 xaxis = Vector3.Transform(Vector3.UnitX, rotation);
			//Vector3 yaxis = Vector3.Transform(Vector3.UnitY, rotation);

			//Matrix4x4 result;

			//result.M11 = xaxis.X;
			//result.M12 = yaxis.X;
			//result.M13 = zaxis.X;
			//result.M14 = 0.0f;
			//result.M21 = xaxis.Y;
			//result.M22 = yaxis.Y;
			//result.M23 = zaxis.Y;
			//result.M24 = 0.0f;
			//result.M31 = xaxis.Z;
			//result.M32 = yaxis.Z;
			//result.M33 = zaxis.Z;
			//result.M34 = 0.0f;
			//result.M41 = -Vector3.Dot(xaxis, position);
			//result.M42 = -Vector3.Dot(yaxis, position);
			//result.M43 = -Vector3.Dot(zaxis, position);
			//result.M44 = 1.0f;

			//return result;
		}

		public mat4 ProjectionMatrix()
		{
			float fowRadians = (MathF.PI / 180) * fow;
			var proj = mat4.Perspective(fowRadians, aspect, nearPlane, farPlane);
			//var proj = Matrix4x4.CreatePerspectiveFieldOfView(fowRadians, aspect, nearPlane, farPlane);
			//Vulkan silliness
			//if (invertY)
			//{
			//	proj.M22 = -proj.M22;
			//}
			return proj;
		}

		public mat4 ProjectionMatrixVulkanCorrected()
		{
			var mat = ProjectionMatrix();
			var correctionMatrix = new mat4(
				1f, 0f, 0f, 0f,
				0f, -1f, 0f, 0f,
				0f, 0f, 0.5f, 0.5f,
				0f, 0f, 0f, 1f
			);
			return correctionMatrix * mat;
		}

		public Frustum GetFrustum(vec3 position, quat rotation)
		{
			mat4 mat =  ProjectionMatrixVulkanCorrected() * ViewMatrix(position, rotation);

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

			//frustum.left = new Plane(
			//	mat.M14 + mat.M11,
			//	mat.M24 + mat.M21,
			//	mat.M34 + mat.M31,
			//	mat.M44 + mat.M41
			//).Normalize();
			//frustum.right = new Plane(
			//	mat.M14 - mat.M11,
			//	mat.M24 - mat.M21,
			//	mat.M34 - mat.M31,
			//	mat.M44 - mat.M41
			//).Normalize();
			//frustum.bottom = new Plane(
			//	mat.M14 + mat.M12,
			//	mat.M24 + mat.M22,
			//	mat.M34 + mat.M32,
			//	mat.M44 + mat.M42
			//).Normalize();
			//frustum.top = new Plane(
			//	mat.M14 - mat.M12,
			//	mat.M24 - mat.M22,
			//	mat.M32 - mat.M32,
			//	mat.M44 - mat.M42
			//).Normalize();
			//frustum.near = new Plane(
			//	mat.M14 + mat.M13,
			//	mat.M24 + mat.M23,
			//	mat.M34 + mat.M33,
			//	mat.M44 + mat.M43
			//).Normalize();
			//frustum.far = new Plane(
			//	mat.M14 - mat.M13,
			//	mat.M24 - mat.M23,
			//	mat.M34 - mat.M33,
			//	mat.M44 - mat.M43
			//).Normalize();

			//frustum.left = new Plane(
			//	mat.m30 + mat.m00,
			//	mat.m31 + mat.m01,
			//	mat.m32 + mat.m02,
			//	mat.m33 + mat.m03
			//).Normalize();
			//frustum.right = new Plane(
			//	mat.m30 - mat.m00,
			//	mat.m31 - mat.m01,
			//	mat.m32 - mat.m02,
			//	mat.m33 - mat.m03
			//).Normalize();
			//frustum.bottom = new Plane(
			//	mat.m30 + mat.m10,
			//	mat.m31 + mat.m11,
			//	mat.m32 + mat.m12,
			//	mat.m33 + mat.m13
			//).Normalize();
			//frustum.top = new Plane(
			//	mat.m30 - mat.m10,
			//	mat.m31 - mat.m11,
			//	mat.m32 - mat.m12,
			//	mat.m33 - mat.m13
			//).Normalize();
			//frustum.near = new Plane(
			//	mat.m30 + mat.m20,
			//	mat.m31 + mat.m21,
			//	mat.m32 + mat.m22,
			//	mat.m33 + mat.m23
			//).Normalize();
			//frustum.far = new Plane(
			//	mat.m30 - mat.m20,
			//	mat.m31 - mat.m21,
			//	mat.m32 - mat.m22,
			//	mat.m33 - mat.m23
			//).Normalize();

			frustum.left = new Plane(
				mat.m03 + mat.m00,
				mat.m13 + mat.m10,
				mat.m23 + mat.m20,
				mat.m33 + mat.m30
			).Normalize();
			frustum.right = new Plane(
				mat.m03 - mat.m00,
				mat.m13 - mat.m10,
				mat.m23 - mat.m20,
				mat.m33 - mat.m30
			).Normalize();
			frustum.bottom = new Plane(
				mat.m03 + mat.m01,
				mat.m13 + mat.m11,
				mat.m23 + mat.m21,
				mat.m33 + mat.m31
			).Normalize();
			frustum.top = new Plane(
				mat.m03 - mat.m01,
				mat.m13 - mat.m11,
				mat.m23 - mat.m21,
				mat.m33 - mat.m31
			).Normalize();
			frustum.near = new Plane(
				mat.m03 + mat.m02,
				mat.m13 + mat.m12,
				mat.m23 + mat.m22,
				mat.m33 + mat.m32
			).Normalize();
			frustum.far = new Plane(
				mat.m03 - mat.m02,
				mat.m13 - mat.m12,
				mat.m23 - mat.m22,
				mat.m33 - mat.m32
			).Normalize();

			return frustum;
		}
	}
}
