﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Core.ECS;

namespace Core.Graphics
{
	public class Camera {
		public float farPlane;
		public float nearPlane;
		public float fow;
		public float aspect;

		public Matrix4x4 ViewMatrix(Vector3 position, Quaternion rotation) {
			return Matrix4x4.CreateLookAt(position, Vector3.Transform(Vector3.UnitZ, rotation),
				Vector3.Transform(Vector3.UnitY, rotation));
		}

		public Matrix4x4 ProjectionMatrix() {
			float fowRadians = (MathF.PI / 180) * fow;
			return Matrix4x4.CreatePerspectiveFieldOfView(fowRadians, aspect, nearPlane, farPlane);
		}
	}
}