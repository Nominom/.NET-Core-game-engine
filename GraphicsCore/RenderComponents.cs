using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.Graphics.VulkanBackend;
using Newtonsoft.Json;

namespace Core.Graphics
{
	public class MeshRenderer : ISharedComponent {
		public Mesh mesh;
		public Material[] materials = Array.Empty<Material>();

		public MeshRenderer(){}
		public MeshRenderer(Mesh mesh, params Material[] materials) {
			this.mesh = mesh;
			this.materials = materials;
		}
	}
}
