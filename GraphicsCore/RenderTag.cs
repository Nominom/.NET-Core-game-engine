using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;

namespace Core.Graphics
{
	public static class RenderTag
	{
		public static OpaqueRenderTag Opaque { get; } = OpaqueRenderTag.Instance;
		public static TransparentRenderTag Transparent { get; } = TransparentRenderTag.Instance;
	}



	public class OpaqueRenderTag : ISharedComponent {
		public static OpaqueRenderTag Instance { get; } = new OpaqueRenderTag();
	}

	public class TransparentRenderTag : ISharedComponent {
		public static TransparentRenderTag Instance { get; } = new TransparentRenderTag();
	}

}
