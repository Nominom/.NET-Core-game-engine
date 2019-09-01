using System;
using System.Collections.Generic;
using System.Text;

namespace ECSCore {
	public class ECSWorld {
		public EntityManager entityManager { get; }
		public ComponentManager componentManager { get; }
		public SystemManager systemManager { get; }

		public ECSWorld() {
			componentManager = new ComponentManager();
			entityManager = new EntityManager(componentManager);
			systemManager = new SystemManager();
		}
	}
}
