using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.ECS.Events;

namespace Core.Physics
{
	[ECSSystem(updateEvent : UpdateEvent.FixedUpdate)]
	public class PhysicsBodyFinalizerSystem : ComponentSystem, IEventHandler<ComponentRemovedEvent<InternalRigidBodyHandle>>
	{
		public override ComponentQuery GetQuery() {
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<InternalRigidBodyHandle>();
			query.Exclude<RigidBody>();
			return query;
		}

		//Remove any leftover internal rigid bodies
		public override void ProcessBlock(float deltaTime, BlockAccessor block) {
			var entities = block.GetEntityData();
			for (int i = 0; i < block.length; i++) {
				entities[i].RemoveComponent<InternalRigidBodyHandle>(afterUpdateCommands);
			}
		}

		//When entity is deleted or internal rigidbody destroyed
		void IEventHandler<ComponentRemovedEvent<InternalRigidBodyHandle>>.ProcessEvents(ECSWorld world, ReadOnlySpan<ComponentRemovedEvent<InternalRigidBodyHandle>> events) {
			foreach (var @event in events) {
				int handle = @event.oldComponentData.rigidBodyHandle;
				PhysicsSystem.Simulation.Bodies.Remove(handle);
			}
		}
	}

	[ECSSystem(updateEvent : UpdateEvent.FixedUpdate)]
	public class StaticBodyFinalizerSystem : ComponentSystem, IEventHandler<ComponentRemovedEvent<InternalStaticBodyHandle>>
	{
		public override ComponentQuery GetQuery() {
			ComponentQuery query = new ComponentQuery();
			query.IncludeReadonly<InternalStaticBodyHandle>();
			query.Exclude<StaticRigidBody>();
			return query;
		}

		//Remove any leftover internal static bodies
		public override void ProcessBlock(float deltaTime, BlockAccessor block) {
			var entities = block.GetEntityData();
			for (int i = 0; i < block.length; i++) {
				entities[i].RemoveComponent<InternalStaticBodyHandle>(afterUpdateCommands);
			}
		}

		//When entity is deleted or internal static body destroyed
		void IEventHandler<ComponentRemovedEvent<InternalStaticBodyHandle>>.ProcessEvents(ECSWorld world, ReadOnlySpan<ComponentRemovedEvent<InternalStaticBodyHandle>> events) {
			foreach (var @event in events) {
				int handle = @event.oldComponentData.staticBodyHandle;
				PhysicsSystem.Simulation.Statics.Remove(handle);
			}
		}
	}
}
