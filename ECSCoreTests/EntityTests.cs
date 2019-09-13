using System;
using Xunit;
using ECSCore;

namespace ECSCoreTests {
	public class EntityTests {
		[Fact]
		public void Size () {
			unsafe { 
				Assert.Equal(sizeof(Entity), 8);
			}
		}

		[Fact]
		public void CreateEntity() {
			ECSWorld world = new ECSWorld();
			ComponentManager cm = world.ComponentManager;
			EntityManager em = world.EntityManager;

			Entity e1 = em.CreateEntity();
			Entity e2 = em.CreateEntity();

			Assert.NotEqual(e1.id, e2.id);

			em.DestroyEntity(e1);

			Entity e3 = em.CreateEntity();

			Assert.Equal(e1.id, e3.id);
			Assert.NotEqual(e1.version, e3.version);
			Assert.NotEqual(e1, e3);
		}
	}
}
