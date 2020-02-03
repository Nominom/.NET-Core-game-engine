using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Filters
{
	public class EmptyFilter : IComponentFilter {
		public static EmptyFilter Instance { get; } = new EmptyFilter();

		public bool FilterBlock(BlockAccessor block) {
			return false;
		}

		public void UpdateFilter(BlockAccessor block) { }
	}
}
