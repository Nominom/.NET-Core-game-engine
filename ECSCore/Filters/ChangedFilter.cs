using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Filters
{
	public class ChangedFilter<T> : IComponentFilter where T : unmanaged, IComponent {

		private readonly Dictionary<long, long> blockComponentVersions = new Dictionary<long, long>();

		public bool FilterBlock(BlockAccessor block) {
			long id = block.Id;
			long version = block.GetComponentVersion<T>();
			if (!blockComponentVersions.TryGetValue(id, out long oldVersion)) {
				return false;
			}
			return oldVersion == version;
		}

		public void UpdateFilter(BlockAccessor block) {
			long id = block.Id;
			long version = block.GetComponentVersion<T>();
			blockComponentVersions[id] = version;
		}
	}
}
