using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using Core.AssetSystem;
using Core.AssetSystem.Assets;
using Core.Graphics.VulkanBackend;

namespace Core.Graphics
{
	public class ShaderPipeline
	{
		public AssetReference<ShaderAsset> Fragment { get; }
		public AssetReference<ShaderAsset> Vertex { get; }

		public ShaderType ShaderType { get; }


		private ShaderPair _shaderPair;
		public ShaderPair ShaderPair
		{
			get
			{
				if (_shaderPair == null)
				{
					_shaderPair = ShaderPair.Load(GraphicsContext.graphicsDevice, Fragment, Vertex, ShaderType);
				}

				return _shaderPair;
			}
		}

		public ShaderPipeline(AssetReference<ShaderAsset> fragment, AssetReference<ShaderAsset> vertex, ShaderType shaderType)
		{
			Fragment = fragment;
			Vertex = vertex;
			ShaderType = shaderType;
		}
	}
}
