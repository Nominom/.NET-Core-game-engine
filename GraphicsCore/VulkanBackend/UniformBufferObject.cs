using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Core.Graphics.VulkanBackend
{
	public struct UniformBufferObject
	{
		public System.Numerics.Matrix4x4 projection;
		public System.Numerics.Matrix4x4 view;
		public Vector4 lightDir;
		public Vector4 cameraPos;
	}
}
