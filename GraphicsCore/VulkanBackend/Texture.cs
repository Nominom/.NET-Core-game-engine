using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Core.AssetSystem;
using Core.AssetSystem.Assets;
using Core.Graphics.VulkanBackend.Utility;
using Core.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Core.Graphics.VulkanBackend
{
	public abstract unsafe class Texture : IDisposable
	{
		public GraphicsDevice device;
		public VkImageView view;
		public VkImage image;
		public VkSampler sampler;
		public VkFormat format;
		public VulkanMemorySlice memory;
		public uint width;
		public uint height;
		public uint mipLevels;
		public VkImageLayout imageLayout;
		public VkDescriptorImageInfo descriptor;



		public virtual void Dispose() {
			memory.Dispose();
			//vkFreeMemory(device.device, deviceMemory, null); 
			vkDestroyImage(device.device, image, null);
			vkDestroySampler(device.device, sampler, null);
			vkDestroyImageView(device.device, view, null);
		}
	}

	public unsafe class Texture2D : Texture {

		public static Texture2D White { get; } = CreateWhite();

		private static Texture2D CreateWhite() {
			if (!GraphicsContext.initialized) {
				throw new InvalidOperationException();
			}
			byte[] whiteBytes = new byte[4];
			Array.Fill(whiteBytes, (byte)255);
			using var tex2D = Image.LoadPixelData<Rgba32>(whiteBytes, 1, 1);
			
			Texture2D white = new Texture2D();
			white.CreateFromImage(tex2D, GraphicsContext.graphicsDevice);
			return white;
		}

		/**
        * Load a 2D texture including all mip levels
        *
        * @param filename File to load (supports .ktx and .dds)
        * @param format Vulkan format of the image data stored in the file
        * @param device Vulkan device to create the texture on
        * @param copyQueue Queue used for the texture staging copy commands (must support transfer)
        * @param (Optional) imageUsageFlags Usage flags for the texture's image (defaults to VK_IMAGE_USAGE_SAMPLED_BIT)
        * @param (Optional) imageLayout Usage layout for the texture (defaults VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
        * @param (Optional) forceLinear Force linear tiling (not advised, defaults to false)
        *
        */
		public void FromTextureAsset(
			TextureAsset asset,
			GraphicsDevice device,
			VkImageUsageFlags imageUsageFlags = VkImageUsageFlags.Sampled,
			VkImageLayout imageLayout = VkImageLayout.ShaderReadOnlyOptimal) {


			VkFormat format = VkFormat.Undefined;

			KtxFile tex2D = asset.GetTexture();
			format = GlFormatToVulkanFormat.vkGetFormatFromOpenGLInternalFormat(tex2D.Header.GlInternalFormat, tex2D.Header.GlFormat, tex2D.Header.GlType);

			width = tex2D.Header.PixelWidth;
			height = tex2D.Header.PixelHeight;
			if (height == 0) height = width;
			mipLevels = tex2D.Header.NumberOfMipmapLevels;

			this.imageLayout = imageLayout;
			this.format = format;
			this.device = device;

			// Get device properites for the requested texture format
			VkFormatProperties formatProperties;
			vkGetPhysicalDeviceFormatProperties(device.physicalDevice, format, out formatProperties);

			VkMemoryAllocateInfo memAllocInfo = Initializers.memoryAllocateInfo();
			VkMemoryRequirements memReqs;

			// Create optimal tiled target image
			VkImageCreateInfo imageCreateInfo = Initializers.imageCreateInfo();
			imageCreateInfo.imageType = VkImageType.Image2D;
			imageCreateInfo.format = format;
			imageCreateInfo.mipLevels = mipLevels;
			imageCreateInfo.arrayLayers = 1;
			imageCreateInfo.samples = VkSampleCountFlags.Count1;
			imageCreateInfo.tiling = VkImageTiling.Optimal;
			imageCreateInfo.sharingMode = VkSharingMode.Exclusive;
			imageCreateInfo.initialLayout = VkImageLayout.Undefined;
			imageCreateInfo.extent = new VkExtent3D { width = width, height = height, depth = 1 };
			imageCreateInfo.usage = imageUsageFlags;
			// Ensure that the TRANSFER_DST bit is set for staging
			if ((imageCreateInfo.usage & VkImageUsageFlags.TransferDst) == 0)
			{
				imageCreateInfo.usage |= VkImageUsageFlags.TransferDst;
			}
			Util.CheckResult(vkCreateImage(device.device, &imageCreateInfo, null, out image));

			vkGetImageMemoryRequirements(device.device, image, &memReqs);

			memAllocInfo.allocationSize = memReqs.size;

			memory = device.memoryAllocator.Allocate(memReqs.size, memReqs.alignment, memReqs.memoryTypeBits, false);
			Util.CheckResult(vkBindImageMemory(device.device, image, memory.vkDeviceMemory, memory.offset));

			//memAllocInfo.memoryTypeIndex = device.vulkanDevice.getMemoryType(memReqs.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal);
			//Util.CheckResult(vkAllocateMemory(device.device, &memAllocInfo, null, out deviceMemory));
			//Util.CheckResult(vkBindImageMemory(device.device, image, deviceMemory, 0));

			TransferDataKtx(tex2D);

			CreateSampler();

			CreateView();

			// Update descriptor image info member that can be used for setting up descriptor sets
			UpdateDescriptor();
		}


		private void TransferDataKtx(KtxFile tex2D)
		{
			using CommandBuffer copyCmd = device.GetCommandPool().Rent();

			VkMemoryAllocateInfo memAllocInfo = Initializers.memoryAllocateInfo();
			VkMemoryRequirements memReqs;

			copyCmd.Begin();

			// Create a host-visible staging buffer that contains the raw image data
			VkBuffer stagingBuffer;
			VkDeviceMemory stagingMemory;

			VkBufferCreateInfo bufferCreateInfo = Initializers.bufferCreateInfo();
			bufferCreateInfo.size = tex2D.GetTotalSize();
			// This buffer is used as a transfer source for the buffer copy
			bufferCreateInfo.usage = VkBufferUsageFlags.TransferSrc;
			bufferCreateInfo.sharingMode = VkSharingMode.Exclusive;

			Util.CheckResult(vkCreateBuffer(device.device, &bufferCreateInfo, null, &stagingBuffer));

			// Get memory requirements for the staging buffer (alignment, memory type bits)
			vkGetBufferMemoryRequirements(device.device, stagingBuffer, &memReqs);

			memAllocInfo.allocationSize = memReqs.size;
			// Get memory type index for a host visible buffer
			memAllocInfo.memoryTypeIndex = device.vulkanDevice.getMemoryType(memReqs.memoryTypeBits, VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent);

			Util.CheckResult(vkAllocateMemory(device.device, &memAllocInfo, null, &stagingMemory));
			Util.CheckResult(vkBindBufferMemory(device.device, stagingBuffer, stagingMemory, 0));

			// Copy texture data into staging buffer
			byte* data;
			Util.CheckResult(vkMapMemory(device.device, stagingMemory, 0, memReqs.size, 0, (void**)&data));
			byte[] pixelData = tex2D.GetAllTextureData();
			fixed (byte* pixelDataPtr = &pixelData[0])
			{
				Unsafe.CopyBlock(data, pixelDataPtr, (uint)pixelData.Length);
			}
			vkUnmapMemory(device.device, stagingMemory);

			// Setup buffer copy regions for each mip level
			using NativeList<VkBufferImageCopy> bufferCopyRegions = new NativeList<VkBufferImageCopy>();
			uint offset = 0;

			for (uint i = 0; i < mipLevels; i++)
			{
				VkBufferImageCopy bufferCopyRegion = new VkBufferImageCopy();
				bufferCopyRegion.imageSubresource.aspectMask = VkImageAspectFlags.Color;
				bufferCopyRegion.imageSubresource.mipLevel = i;
				bufferCopyRegion.imageSubresource.baseArrayLayer = 0;
				bufferCopyRegion.imageSubresource.layerCount = 1;
				bufferCopyRegion.imageExtent.width = tex2D.Faces[0].Mipmaps[i].Width;
				bufferCopyRegion.imageExtent.height = tex2D.Faces[0].Mipmaps[i].Height;
				bufferCopyRegion.imageExtent.depth = 1;
				bufferCopyRegion.bufferOffset = offset;

				bufferCopyRegions.Add(bufferCopyRegion);

				offset += tex2D.Faces[0].Mipmaps[i].SizeInBytes;
			}

			VkImageSubresourceRange subresourceRange = new VkImageSubresourceRange();
			subresourceRange.aspectMask = VkImageAspectFlags.Color;
			subresourceRange.baseMipLevel = 0;
			subresourceRange.levelCount = mipLevels;
			subresourceRange.layerCount = 1;

			// Image barrier for optimal image (target)
			// Optimal image will be used as destination for the copy
			Tools.setImageLayout(
				copyCmd.vkCmd,
				image,
				VkImageAspectFlags.Color,
				VkImageLayout.Undefined,
				VkImageLayout.TransferDstOptimal,
				subresourceRange);

			// Copy mip levels from staging buffer
			vkCmdCopyBufferToImage(
				copyCmd.vkCmd,
				stagingBuffer,
				image,
				VkImageLayout.TransferDstOptimal,
				bufferCopyRegions.Count,
				bufferCopyRegions.Data);

			// Change texture image layout to shader read after all mip levels have been copied
			Tools.setImageLayout(
				copyCmd.vkCmd,
				image,
				VkImageAspectFlags.Color,
				VkImageLayout.TransferDstOptimal,
				imageLayout,
				subresourceRange);

			copyCmd.End();
			device.FlushCommandBuffer(copyCmd);

			//device.flushCommandBuffer(copyCmd, copyQueue);

			// Clean up staging resources
			vkFreeMemory(device.device, stagingMemory, null);
			vkDestroyBuffer(device.device, stagingBuffer, null);
		}

		private void CreateFromImage(
			Image<Rgba32> tex2D,
			GraphicsDevice device,
			VkImageUsageFlags imageUsageFlags = VkImageUsageFlags.Sampled,
			VkImageLayout imageLayout = VkImageLayout.ShaderReadOnlyOptimal) {

			width = (uint)tex2D.Width;
			height = (uint)tex2D.Height;
			if (height == 0) height = width;
			mipLevels = 1;

			this.imageLayout = imageLayout;
			this.format = VkFormat.R8g8b8a8Unorm;
			this.device = device;

			// Get device properites for the requested texture format
			VkFormatProperties formatProperties;
			vkGetPhysicalDeviceFormatProperties(device.physicalDevice, format, out formatProperties);

			VkMemoryAllocateInfo memAllocInfo = Initializers.memoryAllocateInfo();
			VkMemoryRequirements memReqs;

			// Create optimal tiled target image
			VkImageCreateInfo imageCreateInfo = Initializers.imageCreateInfo();
			imageCreateInfo.imageType = VkImageType.Image2D;
			imageCreateInfo.format = format;
			imageCreateInfo.mipLevels = mipLevels;
			imageCreateInfo.arrayLayers = 1;
			imageCreateInfo.samples = VkSampleCountFlags.Count1;
			imageCreateInfo.tiling = VkImageTiling.Optimal;
			imageCreateInfo.sharingMode = VkSharingMode.Exclusive;
			imageCreateInfo.initialLayout = VkImageLayout.Undefined;
			imageCreateInfo.extent = new VkExtent3D { width = width, height = height, depth = 1 };
			imageCreateInfo.usage = imageUsageFlags;
			// Ensure that the TRANSFER_DST bit is set for staging
			if ((imageCreateInfo.usage & VkImageUsageFlags.TransferDst) == 0)
			{
				imageCreateInfo.usage |= VkImageUsageFlags.TransferDst;
			}
			Util.CheckResult(vkCreateImage(device.device, &imageCreateInfo, null, out image));

			vkGetImageMemoryRequirements(device.device, image, &memReqs);

			memAllocInfo.allocationSize = memReqs.size;

			memory = device.memoryAllocator.Allocate(memReqs.size, memReqs.alignment, memReqs.memoryTypeBits, false);
			Util.CheckResult(vkBindImageMemory(device.device, image, memory.vkDeviceMemory, memory.offset));

			//memAllocInfo.memoryTypeIndex = device.vulkanDevice.getMemoryType(memReqs.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal);
			//Util.CheckResult(vkAllocateMemory(device.device, &memAllocInfo, null, out deviceMemory));
			//Util.CheckResult(vkBindImageMemory(device.device, image, deviceMemory, 0));

			TransferData(tex2D);

			CreateSampler();

			CreateView();

			// Update descriptor image info member that can be used for setting up descriptor sets
			UpdateDescriptor();
		}


		private void TransferData(Image<Rgba32> tex2D)
		{
			using CommandBuffer copyCmd = device.GetCommandPool().Rent();

			VkMemoryAllocateInfo memAllocInfo = Initializers.memoryAllocateInfo();
			VkMemoryRequirements memReqs;

			copyCmd.Begin();

			// Create a host-visible staging buffer that contains the raw image data
			VkBuffer stagingBuffer;
			VkDeviceMemory stagingMemory;


			var pixels = tex2D.GetPixelSpan();
			var byteCount = (ulong)(pixels.Length * Marshal.SizeOf<Rgba32>());

			VkBufferCreateInfo bufferCreateInfo = Initializers.bufferCreateInfo();
			bufferCreateInfo.size = byteCount;
			// This buffer is used as a transfer source for the buffer copy
			bufferCreateInfo.usage = VkBufferUsageFlags.TransferSrc;
			bufferCreateInfo.sharingMode = VkSharingMode.Exclusive;

			Util.CheckResult(vkCreateBuffer(device.device, &bufferCreateInfo, null, &stagingBuffer));

			// Get memory requirements for the staging buffer (alignment, memory type bits)
			vkGetBufferMemoryRequirements(device.device, stagingBuffer, &memReqs);

			memAllocInfo.allocationSize = memReqs.size;
			// Get memory type index for a host visible buffer
			memAllocInfo.memoryTypeIndex = device.vulkanDevice.getMemoryType(memReqs.memoryTypeBits, VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent);

			Util.CheckResult(vkAllocateMemory(device.device, &memAllocInfo, null, &stagingMemory));
			Util.CheckResult(vkBindBufferMemory(device.device, stagingBuffer, stagingMemory, 0));

			// Copy texture data into staging buffer
			byte* data;
			Util.CheckResult(vkMapMemory(device.device, stagingMemory, 0, memReqs.size, 0, (void**)&data));

			fixed (Rgba32* pixelDataPtr = &pixels[0])
			{
				Unsafe.CopyBlock(data, pixelDataPtr, (uint)byteCount);
			}
			vkUnmapMemory(device.device, stagingMemory);

			// Setup buffer copy regions for each mip level
			using NativeList<VkBufferImageCopy> bufferCopyRegions = new NativeList<VkBufferImageCopy>();
			uint offset = 0;

			VkBufferImageCopy bufferCopyRegion = new VkBufferImageCopy();
			bufferCopyRegion.imageSubresource.aspectMask = VkImageAspectFlags.Color;
			bufferCopyRegion.imageSubresource.mipLevel = 0;
			bufferCopyRegion.imageSubresource.baseArrayLayer = 0;
			bufferCopyRegion.imageSubresource.layerCount = 1;
			bufferCopyRegion.imageExtent.width = (uint)tex2D.Width;
			bufferCopyRegion.imageExtent.height = (uint)tex2D.Height;
			bufferCopyRegion.imageExtent.depth = 1;
			bufferCopyRegion.bufferOffset = offset;

			bufferCopyRegions.Add(bufferCopyRegion);

			VkImageSubresourceRange subresourceRange = new VkImageSubresourceRange();
			subresourceRange.aspectMask = VkImageAspectFlags.Color;
			subresourceRange.baseMipLevel = 0;
			subresourceRange.levelCount = mipLevels;
			subresourceRange.layerCount = 1;

			// Image barrier for optimal image (target)
			// Optimal image will be used as destination for the copy
			Tools.setImageLayout(
				copyCmd.vkCmd,
				image,
				VkImageAspectFlags.Color,
				VkImageLayout.Undefined,
				VkImageLayout.TransferDstOptimal,
				subresourceRange);

			// Copy mip levels from staging buffer
			vkCmdCopyBufferToImage(
				copyCmd.vkCmd,
				stagingBuffer,
				image,
				VkImageLayout.TransferDstOptimal,
				bufferCopyRegions.Count,
				bufferCopyRegions.Data);

			// Change texture image layout to shader read after all mip levels have been copied
			Tools.setImageLayout(
				copyCmd.vkCmd,
				image,
				VkImageAspectFlags.Color,
				VkImageLayout.TransferDstOptimal,
				imageLayout,
				subresourceRange);

			copyCmd.End();
			device.FlushCommandBuffer(copyCmd);

			//device.flushCommandBuffer(copyCmd, copyQueue);

			// Clean up staging resources
			vkFreeMemory(device.device, stagingMemory, null);
			vkDestroyBuffer(device.device, stagingBuffer, null);
		}




		private void CreateSampler() {
			// Create a defaultsampler
			VkSamplerCreateInfo samplerCreateInfo = VkSamplerCreateInfo.New();
			samplerCreateInfo.magFilter = VkFilter.Linear;
			samplerCreateInfo.minFilter = VkFilter.Linear;
			samplerCreateInfo.mipmapMode = VkSamplerMipmapMode.Linear;
			samplerCreateInfo.addressModeU = VkSamplerAddressMode.Repeat;
			samplerCreateInfo.addressModeV = VkSamplerAddressMode.Repeat;
			samplerCreateInfo.addressModeW = VkSamplerAddressMode.Repeat;
			samplerCreateInfo.mipLodBias = 0.0f;
			samplerCreateInfo.compareOp = VkCompareOp.Never;
			samplerCreateInfo.minLod = 0.0f;
			// Max level-of-detail should match mip level count
			samplerCreateInfo.maxLod = mipLevels;
			// Enable anisotropic filtering
			samplerCreateInfo.maxAnisotropy = 8;
			samplerCreateInfo.anisotropyEnable = True;
			samplerCreateInfo.borderColor = VkBorderColor.FloatOpaqueWhite;
			Util.CheckResult(vkCreateSampler(device.device, &samplerCreateInfo, null, out sampler));
		}

		private void CreateView() {
			// Create image view
			// Textures are not directly accessed by the shaders and
			// are abstracted by image views containing additional
			// information and sub resource ranges
			VkImageViewCreateInfo viewCreateInfo = VkImageViewCreateInfo.New();
			viewCreateInfo.viewType = VkImageViewType.Image2D;
			viewCreateInfo.format = format;
			viewCreateInfo.components = new VkComponentMapping { r = VkComponentSwizzle.R, g = VkComponentSwizzle.G, b = VkComponentSwizzle.B, a = VkComponentSwizzle.A };
			viewCreateInfo.subresourceRange = new VkImageSubresourceRange { aspectMask = VkImageAspectFlags.Color, baseMipLevel = 0, levelCount = 1, baseArrayLayer = 0, layerCount = 1 };
			// Linear tiling usually won't support mip maps
			// Only set mip map count if optimal tiling is used
			viewCreateInfo.subresourceRange.levelCount = mipLevels;
			viewCreateInfo.image = image;
			Util.CheckResult(vkCreateImageView(device.device, &viewCreateInfo, null, out view));

		}
		void UpdateDescriptor()
		{
			descriptor.sampler = sampler;
			descriptor.imageView = view;
			descriptor.imageLayout = imageLayout;
		}


		public static Texture2D Create(AssetReference<TextureAsset> asset) {
			if (!asset.IsLoaded)
			{
				asset.LoadNow();
			}
			Texture2D texture = new Texture2D();
			texture.FromTextureAsset(asset.Get(), GraphicsContext.graphicsDevice);
			return texture;
		}

	}


}
