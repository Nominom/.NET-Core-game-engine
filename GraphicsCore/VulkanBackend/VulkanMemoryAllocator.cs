using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Core.Graphics.VulkanBackend.Utility;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Core.Graphics.VulkanBackend
{
	public unsafe class VulkanMemorySlice : IDisposable {
		internal VulkanMemoryPool originPool;
		internal bool disposed;
		internal readonly ulong alignOffset;

		public readonly uint memoryTypeIndex;
		public readonly VkMemoryPropertyFlags memoryPropertyFlags;
		public readonly  VkDeviceMemory vkDeviceMemory;
		public readonly ulong size;
		public readonly ulong offset;
		public readonly bool hostVisible;

		public void* Mapped {
			get {
				if (disposed) {
					throw new InvalidOperationException("This MemorySlice is already disposed.");
				}
				if ((memoryPropertyFlags & VkMemoryPropertyFlags.HostVisible) == 0) {
					throw new InvalidOperationException("Cannot map a non-host visible memory");
				}
				return (byte*) originPool.Mapped + offset;
			}
		}


		internal VulkanMemorySlice(VulkanMemoryPool originPool, ulong size, ulong offset, ulong alignOffset, VkDeviceMemory vkDeviceMemory, uint memoryTypeIndex, VkMemoryPropertyFlags memoryPropertyFlags) {
			this.originPool = originPool;
			this.memoryTypeIndex = memoryTypeIndex;
			this.memoryPropertyFlags = memoryPropertyFlags;
			this.vkDeviceMemory = vkDeviceMemory;
			this.size = size;
			this.offset = offset;
			this.alignOffset = alignOffset;
			disposed = false;
			hostVisible = (memoryPropertyFlags & VkMemoryPropertyFlags.HostVisible) != 0;
		}

		public void Dispose() {
			if (!disposed) {
				originPool.Return(this);
			}
		}
	}

	public unsafe class VulkanMemoryAllocator : IDisposable {
		//128 MB
		public const ulong PoolSize = 1024 * 1024 * 128;
		public GraphicsDevice device;
		private List<VulkanMemoryPool> memoryPools = new List<VulkanMemoryPool>();

		public VulkanMemoryAllocator(GraphicsDevice device) {
			this.device = device;
		}

		private uint GetPreferredMemoryType(uint typeBits, VkMemoryPropertyFlags required, VkMemoryPropertyFlags preferred)
		{
			var physicalMemoryProperties = device.DeviceMemoryProperties;

			//Match preferred and required
			for (uint i = 0; i < physicalMemoryProperties.memoryTypeCount; i++)
			{
				if ((typeBits & 1) == 1)
				{
					if ((physicalMemoryProperties.GetMemoryType(i).propertyFlags & required) != required)
					{
						continue;
					}
					if ((physicalMemoryProperties.GetMemoryType(i).propertyFlags & preferred) != preferred)
					{
						continue;
					}
					return i;
				}
				typeBits >>= 1;
			}
			//Match only required
			for (uint i = 0; i < physicalMemoryProperties.memoryTypeCount; i++)
			{
				if ((typeBits & 1) == 1)
				{
					if ((physicalMemoryProperties.GetMemoryType(i).propertyFlags & required) != required)
					{
						continue;
					}
					return i;
				}
				typeBits >>= 1;
			}

			
			throw new InvalidOperationException("Could not find a matching memory type");
		}

		public VulkanMemorySlice Allocate(ulong size, ulong alignment, uint memoryTypeBits, bool hostVisible, bool preferDeviceLocal = true) {
			VkMemoryPropertyFlags requiredMemProps = VkMemoryPropertyFlags.DeviceLocal;
			VkMemoryPropertyFlags preferredMemProps = preferDeviceLocal ? VkMemoryPropertyFlags.None : VkMemoryPropertyFlags.DeviceLocal;
			if (hostVisible) {
				requiredMemProps = VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent;
			}

			if (AllocateFromPools(size, alignment, memoryTypeBits, requiredMemProps, preferredMemProps, hostVisible, out VulkanMemorySlice allocation)) {
				return allocation;
			}

			uint memoryTypeIndex = GetPreferredMemoryType(memoryTypeBits, requiredMemProps, preferredMemProps);
			VkMemoryPropertyFlags memProps = device.DeviceMemoryProperties.GetMemoryType(memoryTypeIndex).propertyFlags;

			VulkanMemoryPool newMemoryPool = new VulkanMemoryPool(device, PoolSize, memoryTypeIndex, memProps, hostVisible);
			var result = newMemoryPool.AllocateDeviceMemory();

			if (result != VkResult.Success) {
				requiredMemProps = VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent;
				preferredMemProps = VkMemoryPropertyFlags.None;
				memoryTypeIndex = GetPreferredMemoryType(memoryTypeBits, requiredMemProps, preferredMemProps);
				memProps = device.DeviceMemoryProperties.GetMemoryType(memoryTypeIndex).propertyFlags;
				newMemoryPool = new VulkanMemoryPool(device, PoolSize, memoryTypeIndex, memProps, hostVisible);
				result = newMemoryPool.AllocateDeviceMemory();
				if (result != VkResult.Success) {
					throw new OutOfMemoryException(result.ToString());
				}
			}
			
			memoryPools.Add(newMemoryPool);
			result = newMemoryPool.Allocate(size, alignment, out allocation);

			if (result != VkResult.Success) {
				throw new OutOfMemoryException(result.ToString());
			}

			return allocation;
		}

		public VulkanMemorySlice Allocate(VkMemoryRequirements requirements, bool hostVisible)
			=> Allocate(requirements.size, requirements.alignment, requirements.memoryTypeBits, hostVisible);

		private bool AllocateFromPools(ulong size, ulong alignment, uint memoryTypeBits, VkMemoryPropertyFlags required, VkMemoryPropertyFlags preferred, bool hostVisible, out VulkanMemorySlice allocation) {

			var physicalMemoryProperties = device.DeviceMemoryProperties;

			//Match for preferred
			for (int i = 0; i < memoryPools.Count; i++) {
				VulkanMemoryPool pool = memoryPools[i];
				uint memoryTypeIndex = pool.memoryTypeIndex;

				if (pool.hostVisible != hostVisible) {
					continue;
				}

				//Match memoryTypeIndex of the pool
				if ((((int)memoryTypeBits >> (int)memoryTypeIndex) & 1) == 0)
				{
					continue;
				}

				// Match required memory properties
				VkMemoryPropertyFlags properties = physicalMemoryProperties.GetMemoryType(memoryTypeIndex).propertyFlags;
				if ( ( properties & required ) != required ) {
					continue;
				}
				// Match preferred
				if ( ( properties & preferred ) != preferred ) {
					continue;
				}

				var result = pool.Allocate(size, alignment, out allocation);
				if (result != VkResult.Success) {
					continue;
				}
				return true;
			}

			//Match only required
			for (int i = 0; i < memoryPools.Count; i++) {
				VulkanMemoryPool pool = memoryPools[i];
				uint memoryTypeIndex = pool.memoryTypeIndex;

				if (pool.hostVisible != hostVisible) {
					continue;
				}

				//Match memoryTypeIndex of the pool
				if ((((int)memoryTypeBits >> (int)memoryTypeIndex) & 1) == 0)
				{
					continue;
				}

				// Match required memory properties
				VkMemoryPropertyFlags properties = physicalMemoryProperties.GetMemoryType(memoryTypeIndex).propertyFlags;
				if (( properties & required ) != required ) {
					continue;
				}

				var result = pool.Allocate(size, alignment, out allocation);
				if (result != VkResult.Success) {
					continue;
				}
				return true;
			}


			allocation = null;
			return false;
		}

		public void Dispose() {
			foreach (VulkanMemoryPool memoryPool in memoryPools) {
				memoryPool.Dispose();
			}
			memoryPools.Clear();
		}
	}


	public unsafe class VulkanMemoryPool : IDisposable {
		public struct FreeSlice {
			public ulong startIndex;
			public ulong length;
		}

		public bool hostVisible;
		public GraphicsDevice device;
		public uint memoryTypeIndex;
		public VkMemoryPropertyFlags memoryPropertyFlags;
		public ulong max_size;
		public List<FreeSlice> freeSlices;
		public VulkanMemoryPool next;
		public void* Mapped { get; private set; } = null;


		private VkDeviceMemory vkDeviceMemory;

		public VulkanMemoryPool(GraphicsDevice device, ulong size, uint memoryTypeIndex, VkMemoryPropertyFlags memProps,bool hostVisible ) {
			this.device = device;
			this.max_size = size;
			this.memoryPropertyFlags = memProps;
			this.hostVisible = hostVisible;
			freeSlices = new List<FreeSlice>();
			freeSlices.Add(new FreeSlice(){length = max_size, startIndex = 0});
			this.memoryTypeIndex = memoryTypeIndex;

			
		}


		internal VkResult AllocateDeviceMemory() {
			VkMemoryAllocateInfo info = VkMemoryAllocateInfo.New();
			info.allocationSize = max_size;
			info.memoryTypeIndex = memoryTypeIndex;

			VkResult result = vkAllocateMemory(device.device, &info, null, out vkDeviceMemory);
			if (result != VkResult.Success) {
				return result;
			}

			if ((memoryPropertyFlags & VkMemoryPropertyFlags.HostVisible) != 0) {
				Map();
			}
			return result;
		}

		public VkResult Allocate(ulong size, ulong alignment, out VulkanMemorySlice outSlice) {
			if (size > max_size) {
				CreateNext(size);
				var allocResult = next.AllocateDeviceMemory();
				if (allocResult != VkResult.Success) {
					next = null;
					outSlice = null;
					return allocResult;
				}
				return next.Allocate(size, alignment, out outSlice);
			}

			VulkanMemorySlice returnSlice = null;
			for (int i = 0; i < freeSlices.Count; i++) {
				FreeSlice slice = freeSlices[i];

				//align the slice to alignment
				uint align = slice.startIndex % alignment == 0 ? 0 : ((uint)alignment - ((uint)slice.startIndex % (uint)alignment));
				ulong sizeWithAlign = size + align;

				if (slice.length >= sizeWithAlign) {
					returnSlice = new VulkanMemorySlice(this, size, slice.startIndex + align, align, vkDeviceMemory,
						memoryTypeIndex, memoryPropertyFlags);

					slice.length -= sizeWithAlign;
					slice.startIndex += sizeWithAlign;

					if (slice.length == 0) {
						freeSlices.RemoveAt(i);
					}
					else {
						freeSlices[i] = slice;
					}
					break;
				}
			}


			if (returnSlice != null) {
				outSlice = returnSlice;
				return VkResult.Success;
			}
			else {
				CreateNext(max_size);
				var allocResult = next.AllocateDeviceMemory();
				if (allocResult != VkResult.Success) {
					next = null;
					outSlice = null;
					return allocResult;
				}
				return next.Allocate(size, alignment, out outSlice);
			}
		}

		internal void Return(VulkanMemorySlice returnSlice) {
			DebugHelper.AssertThrow<InvalidOperationException>(returnSlice.vkDeviceMemory.Handle == vkDeviceMemory.Handle);

			//First try to merge with existing free slices
			for (int i = 0; i < freeSlices.Count; i++) {
				var slice = freeSlices[i];
				ulong sizeWithAlign = returnSlice.size + returnSlice.alignOffset;

				if (slice.startIndex == returnSlice.offset + sizeWithAlign) {
					slice.length += sizeWithAlign;
					slice.startIndex -= sizeWithAlign;
					freeSlices[i] = slice;
					return;
				}
			}
			//If not possible insert to the list
			FreeSlice newSlice = new FreeSlice(){length =  returnSlice.size + returnSlice.alignOffset, startIndex = returnSlice.offset - returnSlice.alignOffset};
			//freeSlices.Add(newSlice);

			//Add and sort in ascending order by length
			for (int i = 0; i < freeSlices.Count; i++) {
				if (freeSlices[i].length > newSlice.length) {
					freeSlices.Insert(i, newSlice);
					return;
				}
			}
			//If largest, add to the end 
			freeSlices.Add(newSlice);
		}

		private void Map() {
			if (Mapped != null) {
				return;
			}
			if ((memoryPropertyFlags & VkMemoryPropertyFlags.HostVisible) == 0) {
				return;
			}

			void* result;
			Util.CheckResult(vkMapMemory(device.device, vkDeviceMemory, 0, max_size, 0, &result));
			Mapped = result;
		}

		private void Unmap() {
			if(Mapped == null) {
				return;
			}
			if ((memoryPropertyFlags & VkMemoryPropertyFlags.HostVisible) == 0) {
				return;
			}

			vkUnmapMemory(device.device, vkDeviceMemory);
		}


		private void CreateNext(ulong newSize) {
			next = new VulkanMemoryPool(device, newSize, memoryTypeIndex, memoryPropertyFlags, hostVisible);
		}

		public void Dispose() {
			next?.Dispose();
			Unmap();
			vkFreeMemory(device.device, vkDeviceMemory, null);
		}
	}
}
