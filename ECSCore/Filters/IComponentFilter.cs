using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Filters
{
	public interface IComponentFilter {
		/// <summary>
		/// Returns true if the block should be filtered out.
		/// </summary>
		bool FilterBlock(BlockAccessor block);
		/// <summary>
		/// Updates filter with new data
		/// </summary>
		void UpdateFilter(BlockAccessor block);
	}
}
