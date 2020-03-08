using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS
{
	public static class EntityExtensions
	{
		public static bool IsValid(this Entity entity, ECSWorld world)
		{
			return world.ComponentManager.IsEntityValid(entity);
		}

		public static void Destroy(this Entity entity, ECSWorld world)
		{
			world.EntityManager.DestroyEntity(entity);
		}

		public static void AddComponent<T>(this Entity entity, ECSWorld world, in T component) where T : unmanaged, IComponent
		{
			world.ComponentManager.AddComponent(entity, component);
		}

		public static void SetComponent<T>(this Entity entity, ECSWorld world, in T component) where T : unmanaged, IComponent
		{
			world.ComponentManager.SetComponent(entity, component);
		}

		public static void RemoveComponent<T>(this Entity entity, ECSWorld world) where T : unmanaged, IComponent
		{
			world.ComponentManager.RemoveComponent<T>(entity);
		}

		public static bool HasComponent<T>(this Entity entity, ECSWorld world) where T : unmanaged, IComponent
		{
			return world.ComponentManager.HasComponent<T>(entity);
		}

		public static ref T GetComponent<T>(this Entity entity, ECSWorld world) where T : unmanaged, IComponent
		{
			return ref world.ComponentManager.GetComponent<T>(entity);
		}

		public static bool TryGetComponent<T>(this Entity entity, ECSWorld world, out T component) where T : unmanaged, IComponent
		{
			return world.ComponentManager.TryGetComponent<T>(entity, out component);
		}

		public static void AddSharedComponent<T>(this Entity entity, ECSWorld world, in T component) where T : class, ISharedComponent
		{
			world.ComponentManager.AddSharedComponent(entity, component);
		}
		public static void RemoveSharedComponent<T>(this Entity entity, ECSWorld world) where T : class, ISharedComponent
		{
			world.ComponentManager.RemoveSharedComponent<T>(entity);
		}
		public static T GetSharedComponent<T>(this Entity entity, ECSWorld world) where T : class, ISharedComponent
		{
			return world.ComponentManager.GetSharedComponent<T>(entity);
		}
		public static bool HasSharedComponent<T>(this Entity entity, ECSWorld world) where T : class, ISharedComponent
		{
			return world.ComponentManager.HasSharedComponent<T>(entity);
		}
		public static bool TryGetSharedComponent<T>(this Entity entity, ECSWorld world, out T component) where T : class, ISharedComponent
		{
			return world.ComponentManager.TryGetSharedComponent<T>(entity, out component);
		}




		public static void Destroy(this Entity entity, IEntityCommandBuffer commandBuffer)
		{
			commandBuffer.DestroyEntity(entity);
		}

		public static void AddComponent<T>(this Entity entity, IEntityCommandBuffer commandBuffer, in T component) where T : unmanaged, IComponent
		{
			commandBuffer.AddComponent(entity, component);
		}

		public static void SetComponent<T>(this Entity entity, IEntityCommandBuffer commandBuffer, in T component) where T : unmanaged, IComponent
		{
			commandBuffer.SetComponent(entity, component);
		}

		public static void RemoveComponent<T>(this Entity entity, IEntityCommandBuffer commandBuffer) where T : unmanaged, IComponent
		{
			commandBuffer.RemoveComponent<T>(entity);
		}

		public static void AddSharedComponent<T>(this Entity entity, IEntityCommandBuffer commandBuffer, in T component) where T : class, ISharedComponent
		{
			commandBuffer.AddSharedComponent(entity, component);
		}
		public static void RemoveSharedComponent<T>(this Entity entity, IEntityCommandBuffer commandBuffer) where T : class, ISharedComponent
		{
			commandBuffer.RemoveSharedComponent<T>(entity);
		}
	}
}
