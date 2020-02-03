using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Filters
{
	public class EntityAddedRemovedFilter : IComponentFilter
	{
		private readonly Dictionary<long, long> blockEntityVersions = new Dictionary<long, long>();

		public bool FilterBlock(BlockAccessor block) {
			long id = block.Id;
			long version = block.GetEntityVersion();
			if (!blockEntityVersions.TryGetValue(id, out long oldVersion)) {
				return false;
			}
			return oldVersion == version;
		}

		public void UpdateFilter(BlockAccessor block) {
			long id = block.Id;
			long version = block.GetEntityVersion();
			blockEntityVersions[id] = version;
		}
	}
}
