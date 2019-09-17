using System;
using System.Collections.Generic;
using System.Text;

namespace ECSCore
{

	public sealed class ModificationCommandBuffer
	{
		private ECSWorld world;

		//TODO

		public void CreateEntity(EntityArchetype archetype)
		{

		}

		public void SetComponent<T>(T component) where T : unmanaged, IComponent
		{

		}

		public void AddComponent<T>(T component) where T : unmanaged, IComponent
		{

		}

		public void AddSharedComponent<T>(T component) where T : class, ISharedComponent
		{

		}
	}
}
