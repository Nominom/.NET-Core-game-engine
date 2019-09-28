using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;

namespace Core.Graphics
{
	public interface IRenderSystem {
		void OnCreate(ECSWorld world);

		void OnDestroy(ECSWorld world);

		void Render(ECSWorld world, in RenderContext context);
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class RenderSystemAttribute : Attribute {
		public readonly RenderStage renderStage;
		public readonly System.Type renderBefore;
		public readonly System.Type renderAfter;

		public RenderSystemAttribute(RenderStage renderStage = RenderStage.RenderOpaques, System.Type renderBefore = null,
			System.Type renderAfter = null)
		{
			this.renderStage = renderStage;
			this.renderBefore = renderBefore;
			this.renderAfter = renderAfter;
		}
	}
}
