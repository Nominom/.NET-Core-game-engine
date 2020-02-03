using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Filters
{
	public class CombinedFilterAny : IComponentFilter {
		private readonly IComponentFilter filter1;
		private readonly IComponentFilter filter2;

		public CombinedFilterAny(IComponentFilter filter1, IComponentFilter filter2) {
			this.filter1 = filter1;
			this.filter2 = filter2;
		}

		public bool FilterBlock(BlockAccessor block) {
			return filter1.FilterBlock(block) && filter2.FilterBlock(block);
		}

		public void UpdateFilter(BlockAccessor block) {
			filter1.UpdateFilter(block);
			filter2.UpdateFilter(block);
		}
	}
}
