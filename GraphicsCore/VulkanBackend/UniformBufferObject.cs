using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using GlmSharp;

namespace Core.Graphics.VulkanBackend
{
	public struct UniformBufferObject
	{
		public mat4 projection;
		public mat4 view;
		public vec4 lightDir;
		public vec4 cameraPos;
	}
}
