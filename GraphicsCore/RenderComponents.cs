using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.Graphics.VulkanBackend;

namespace Core.Graphics
{
	public class MeshRenderer : ISharedComponent {
		public Mesh mesh;
		public Material[] materials = Array.Empty<Material>();
	}
}
