using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using BepuUtilities.Memory;
using Core.ECS;
using Core.ECS.Events;
using Core.Shared;

namespace Core.Physics
{
	
	[ECSSystem(UpdateEvent.FixedUpdate)]
	public class PhysicsSystem : ISystem
	{
		public bool Enabled { get; set; }
		
		internal static ThreadLocal<List<CollisionEventData>> collisionsToFire = new ThreadLocal<List<CollisionEventData>>(() => new List<CollisionEventData>(), true);
		private static IList<List<CollisionEventData>> collisionsToFireCached = null;
		internal static ConcurrentMemoryPool<CollisionData> collisionDataPool = new ConcurrentMemoryPool<CollisionData>();
		internal static Dictionary<int, Entity> rigidBodyEntityDictionary = new Dictionary<int, Entity>();
		internal static Dictionary<int, Entity> staticBodyEntityDictionary = new Dictionary<int, Entity>();
		private static HashSet<EntityPair> lastFrameCollisions = new HashSet<EntityPair>();
		private static HashSet<EntityPair> thisFrameCollisions = new HashSet<EntityPair>();

		public static Simulation Simulation { get; private set; }

		public static BufferPool BufferPool { get; private set; }

		private SimpleThreadDispatcher threadDispatcher;

		public void OnCreateSystem(ECSWorld world) {
			BufferPool = new BufferPool();

			Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntergratorCallbacks(
				-Vector3.UnitY * 9));

			threadDispatcher = new SimpleThreadDispatcher(Environment.ProcessorCount);

		}

		public void OnDestroySystem(ECSWorld world) {
			Simulation?.Dispose();
			BufferPool?.Clear();
			threadDispatcher?.Dispose();
		}

		public void Update(float deltaTime, ECSWorld world) {
			collisionDataPool.FreeAll();

			Simulation.Timestep(deltaTime, threadDispatcher);

			if (collisionsToFireCached == null || collisionsToFireCached.Count < threadDispatcher.ThreadCount) {
				collisionsToFireCached = collisionsToFire.Values;
			}

			for(int i = 0; i < collisionsToFireCached.Count; i++) {
				var list = collisionsToFireCached[i];
				foreach (var ced in list) {
					Entity a = ced.pair.A.Mobility == CollidableMobility.Static
						? staticBodyEntityDictionary[ced.pair.A.Handle]
						: rigidBodyEntityDictionary[ced.pair.A.Handle];
					Entity b = ced.pair.B.Mobility == CollidableMobility.Static
						? staticBodyEntityDictionary[ced.pair.B.Handle]
						: rigidBodyEntityDictionary[ced.pair.B.Handle];

					new CollisionEvent() {
						A = a,
						B = b,
						collisions = ced.collisionMemory
					}.Fire(world);

					EntityPair key = new EntityPair(a, b);
					thisFrameCollisions.Add(key);
					if (!lastFrameCollisions.Contains(key)) {
						new CollisionEnterEvent() {
							A = a,
							B = b,
							collisions = ced.collisionMemory
						}.Fire(world);
					}
				}

				list.Clear();
			}

			foreach (EntityPair lastFrameCollision in lastFrameCollisions) {
				if (!thisFrameCollisions.Contains(lastFrameCollision)) {
					new CollisionExitEvent() {
						A = lastFrameCollision.A,
						B = lastFrameCollision.B
					}.Fire(world);
				}
			}

			var store = lastFrameCollisions;
			lastFrameCollisions = thisFrameCollisions;
			thisFrameCollisions = store;
			thisFrameCollisions.Clear();
		}
	}
}
