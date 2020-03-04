using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Core.ECS.Filters
{
	public static class ComponentFilters {
		public static EmptyFilter Empty() => EmptyFilter.Instance;
		public static ChangedFilter<T> Changed<T>() where T : unmanaged , IComponent => new ChangedFilter<T>();

		public static EntityAddedRemovedFilter EntityChanged() => new EntityAddedRemovedFilter();

		public static CombinedFilterAny ChangedAny<T1, T2>()
			where T1 : unmanaged, IComponent
			where T2 : unmanaged, IComponent
			=> CombineAny(Changed<T1>(), Changed<T2>());

		public static CombinedFilterAny ChangedAny<T1, T2, T3>()
			where T1 : unmanaged, IComponent
			where T2 : unmanaged, IComponent
			where T3 : unmanaged, IComponent
			=> CombineAny(Changed<T1>(), Changed<T2>(), Changed<T3>());

		public static CombinedFilterAny ChangedAny<T1, T2, T3, T4>()
			where T1 : unmanaged, IComponent
			where T2 : unmanaged, IComponent
			where T3 : unmanaged, IComponent
			where T4 : unmanaged, IComponent
			=> CombineAny(Changed<T1>(), Changed<T2>(), Changed<T3>(), Changed<T4>());

		public static CombinedFilterAny ChangedAny<T1, T2, T3, T4, T5>()
			where T1 : unmanaged, IComponent
			where T2 : unmanaged, IComponent
			where T3 : unmanaged, IComponent
			where T4 : unmanaged, IComponent
			where T5 : unmanaged, IComponent
			=> CombineAny(Changed<T1>(), Changed<T2>(), Changed<T3>(), Changed<T4>(), Changed<T5>());

		public static CombinedFilterAny CombineAny(IComponentFilter filter1,IComponentFilter filter2) 
			=> new CombinedFilterAny(filter1, filter2);
		public static CombinedFilterAny CombineAny(IComponentFilter filter1,IComponentFilter filter2,IComponentFilter filter3) 
			=> new CombinedFilterAny(filter1, new CombinedFilterAny(filter2, filter3));
		public static CombinedFilterAny CombineAny(IComponentFilter filter1,IComponentFilter filter2,IComponentFilter filter3,IComponentFilter filter4) 
			=> new CombinedFilterAny(filter1, new CombinedFilterAny(filter2,  new CombinedFilterAny(filter3, filter4)));
		public static CombinedFilterAny CombineAny(IComponentFilter filter1,IComponentFilter filter2,IComponentFilter filter3,IComponentFilter filter4,IComponentFilter filter5) 
			=> new CombinedFilterAny(filter1, new CombinedFilterAny(filter2,  new CombinedFilterAny(filter3, new CombinedFilterAny(filter4, filter5))));

		public static CombinedFilterAll CombineAll(IComponentFilter filter1,IComponentFilter filter2) 
			=> new CombinedFilterAll(filter1, filter2);
		public static CombinedFilterAll CombineAll(IComponentFilter filter1,IComponentFilter filter2,IComponentFilter filter3) 
			=> new CombinedFilterAll(filter1, new CombinedFilterAll(filter2, filter3));
		public static CombinedFilterAll CombineAll(IComponentFilter filter1,IComponentFilter filter2,IComponentFilter filter3,IComponentFilter filter4) 
			=> new CombinedFilterAll(filter1, new CombinedFilterAll(filter2,  new CombinedFilterAll(filter3, filter4)));
		public static CombinedFilterAll CombineAll(IComponentFilter filter1,IComponentFilter filter2,IComponentFilter filter3,IComponentFilter filter4,IComponentFilter filter5) 
			=> new CombinedFilterAll(filter1, new CombinedFilterAll(filter2,  new CombinedFilterAll(filter3, new CombinedFilterAll(filter4, filter5))));
	}
}
