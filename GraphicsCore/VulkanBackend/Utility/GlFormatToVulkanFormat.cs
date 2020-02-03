using System;
using System.Collections.Generic;
using System.Text;
using Vulkan;

namespace Core.Graphics.VulkanBackend.Utility
{
	public static class GlFormatToVulkanFormat
	{
		public enum GlInternalFormat : uint
		{
			GL_COMPRESSED_RGB_S3TC_DXT1_EXT = 0x83F0,
			GL_COMPRESSED_RGBA_S3TC_DXT1_EXT = 0x83F1,
			GL_COMPRESSED_RGBA_S3TC_DXT3_EXT = 0x83F2,
			GL_COMPRESSED_RGBA_S3TC_DXT5_EXT = 0x83F3,

			GL_ETC1_RGB8_OES = 0x8D64,

			GL_COMPRESSED_R11_EAC = 0x9270,
			GL_COMPRESSED_SIGNED_R11_EAC = 0x9271,
			GL_COMPRESSED_RG11_EAC = 0x9272,
			GL_COMPRESSED_SIGNED_RG11_EAC = 0x9273,

			GL_COMPRESSED_RGB8_ETC2 = 0x9274,
			GL_COMPRESSED_SRGB8_ETC2 = 0x9275,
			GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 0x9276,
			GL_COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 0x9277,
			GL_COMPRESSED_RGBA8_ETC2_EAC = 0x9278,
			GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC = 0x9279,

			GL_COMPRESSED_RGBA_ASTC_4x4_KHR = 0x93B0,
			GL_COMPRESSED_RGBA_ASTC_5x4_KHR = 0x93B1,
			GL_COMPRESSED_RGBA_ASTC_5x5_KHR = 0x93B2,
			GL_COMPRESSED_RGBA_ASTC_6x5_KHR = 0x93B3,
			GL_COMPRESSED_RGBA_ASTC_6x6_KHR = 0x93B4,
			GL_COMPRESSED_RGBA_ASTC_8x5_KHR = 0x93B5,
			GL_COMPRESSED_RGBA_ASTC_8x6_KHR = 0x93B6,
			GL_COMPRESSED_RGBA_ASTC_8x8_KHR = 0x93B7,
			GL_COMPRESSED_RGBA_ASTC_10x5_KHR = 0x93B8,
			GL_COMPRESSED_RGBA_ASTC_10x6_KHR = 0x93B9,
			GL_COMPRESSED_RGBA_ASTC_10x8_KHR = 0x93BA,
			GL_COMPRESSED_RGBA_ASTC_10x10_KHR = 0x93BB,
			GL_COMPRESSED_RGBA_ASTC_12x10_KHR = 0x93BC,
			GL_COMPRESSED_RGBA_ASTC_12x12_KHR = 0x93BD,

			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_4x4_KHR = 0x93D0,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_5x4_KHR = 0x93D1,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_5x5_KHR = 0x93D2,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_6x5_KHR = 0x93D3,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_6x6_KHR = 0x93D4,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x5_KHR = 0x93D5,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x6_KHR = 0x93D6,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x8_KHR = 0x93D7,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x5_KHR = 0x93D8,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x6_KHR = 0x93D9,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x8_KHR = 0x93DA,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x10_KHR = 0x93DB,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_12x10_KHR = 0x93DC,
			GL_COMPRESSED_SRGB8_ALPHA8_ASTC_12x12_KHR = 0x93DD
		}

		public static VkFormat vkGetFormatFromOpenGLInternalFormat(GlInternalFormat internalFormat)
		{
			switch (internalFormat)
			{

				//
				// S3TC/DXT/BC
				//

				case GlInternalFormat.GL_COMPRESSED_RGB_S3TC_DXT1_EXT:
					return VkFormat.Bc1RgbUnormBlock;
				//return VK_FORMAT_BC1_RGB_UNORM_BLOCK; // line through 3D space, 4x4 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT:
					return VkFormat.Bc1RgbaUnormBlock;
				//returnVK_FORMAT_BC1_RGBA_UNORM_BLOCK; // line through 3D space plus 1-bit alpha, 4x4 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT:
					return VkFormat.Bc2UnormBlock;
				//return VK_FORMAT_BC2_UNORM_BLOCK; // line through 3D space plus line through 1D space, 4x4 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT:
					return VkFormat.Bc3UnormBlock;
				//return VK_FORMAT_BC3_UNORM_BLOCK; // line through 3D space plus 4-bit alpha, 4x4 blocks, unsigned normalized

				//
				// ETC
				//
				case GlInternalFormat.GL_ETC1_RGB8_OES:
					return VkFormat.Etc2R8g8b8UnormBlock;
				//return VK_FORMAT_ETC2_R8G8B8_UNORM_BLOCK; // 3-component ETC1, 4x4 blocks, unsigned normalized

				case GlInternalFormat.GL_COMPRESSED_RGB8_ETC2:
					return VkFormat.Etc2R8g8b8UnormBlock;
				//return VK_FORMAT_ETC2_R8G8B8_UNORM_BLOCK; // 3-component ETC2, 4x4 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2:
					return VkFormat.Etc2R8g8b8a1UnormBlock;
				//return VK_FORMAT_ETC2_R8G8B8A1_UNORM_BLOCK; // 4-component ETC2 with 1-bit alpha, 4x4 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA8_ETC2_EAC:
					return VkFormat.Etc2R8g8b8a8UnormBlock;
				//return VK_FORMAT_ETC2_R8G8B8A8_UNORM_BLOCK; // 4-component ETC2, 4x4 blocks, unsigned normalized

				case GlInternalFormat.GL_COMPRESSED_SRGB8_ETC2:

					return VkFormat.Etc2R8g8b8SrgbBlock;
				//return VK_FORMAT_ETC2_R8G8B8_SRGB_BLOCK; // 3-component ETC2, 4x4 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2:
					return VkFormat.Etc2R8g8b8a1SrgbBlock;
				//return VK_FORMAT_ETC2_R8G8B8A1_SRGB_BLOCK; // 4-component ETC2 with 1-bit alpha, 4x4 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC:
					return VkFormat.Etc2R8g8b8a8SrgbBlock;
				//return VK_FORMAT_ETC2_R8G8B8A8_SRGB_BLOCK; // 4-component ETC2, 4x4 blocks, sRGB

				case GlInternalFormat.GL_COMPRESSED_R11_EAC:
					return VkFormat.EacR11UnormBlock;
				//return VK_FORMAT_EAC_R11_UNORM_BLOCK; // 1-component ETC, 4x4 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RG11_EAC:
					return VkFormat.EacR11g11UnormBlock;
				//return VK_FORMAT_EAC_R11G11_UNORM_BLOCK; // 2-component ETC, 4x4 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_SIGNED_R11_EAC:
					return VkFormat.EacR11SnormBlock;
				//return VK_FORMAT_EAC_R11_SNORM_BLOCK; // 1-component ETC, 4x4 blocks, signed normalized
				case GlInternalFormat.GL_COMPRESSED_SIGNED_RG11_EAC:
					return VkFormat.EacR11g11SnormBlock;
				//return VK_FORMAT_EAC_R11G11_SNORM_BLOCK; // 2-component ETC, 4x4 blocks, signed normalized

				//
				// ASTC
				//
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_4x4_KHR:
					return VkFormat.Astc4x4UnormBlock;
				//return VK_FORMAT_ASTC_4x4_UNORM_BLOCK; // 4-component ASTC, 4x4 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_5x4_KHR:
					return VkFormat.Astc5x4UnormBlock;
				//return VK_FORMAT_ASTC_5x4_UNORM_BLOCK; // 4-component ASTC, 5x4 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_5x5_KHR:
					return VkFormat.Astc5x5UnormBlock;
				//return VK_FORMAT_ASTC_5x5_UNORM_BLOCK; // 4-component ASTC, 5x5 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_6x5_KHR:
					return VkFormat.Astc6x5UnormBlock;
				//return VK_FORMAT_ASTC_6x5_UNORM_BLOCK; // 4-component ASTC, 6x5 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_6x6_KHR:
					return VkFormat.Astc6x6UnormBlock;
				//return VK_FORMAT_ASTC_6x6_UNORM_BLOCK; // 4-component ASTC, 6x6 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_8x5_KHR:
					return VkFormat.Astc8x5UnormBlock;
				//return VK_FORMAT_ASTC_8x5_UNORM_BLOCK; // 4-component ASTC, 8x5 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_8x6_KHR:
					return VkFormat.Astc8x6UnormBlock;
				//return VK_FORMAT_ASTC_8x6_UNORM_BLOCK; // 4-component ASTC, 8x6 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_8x8_KHR:
					return VkFormat.Astc8x8UnormBlock;
				//return VK_FORMAT_ASTC_8x8_UNORM_BLOCK; // 4-component ASTC, 8x8 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_10x5_KHR:
					return VkFormat.Astc10x5UnormBlock;
				//return VK_FORMAT_ASTC_10x5_UNORM_BLOCK; // 4-component ASTC, 10x5 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_10x6_KHR:
					return VkFormat.Astc10x6UnormBlock;
				//return VK_FORMAT_ASTC_10x6_UNORM_BLOCK; // 4-component ASTC, 10x6 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_10x8_KHR:
					return VkFormat.Astc10x8UnormBlock;
				//return VK_FORMAT_ASTC_10x8_UNORM_BLOCK; // 4-component ASTC, 10x8 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_10x10_KHR:
					return VkFormat.Astc10x10UnormBlock;
				//return VK_FORMAT_ASTC_10x10_UNORM_BLOCK; // 4-component ASTC, 10x10 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_12x10_KHR:
					return VkFormat.Astc12x10UnormBlock;
				//return VK_FORMAT_ASTC_12x10_UNORM_BLOCK; // 4-component ASTC, 12x10 blocks, unsigned normalized
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_12x12_KHR:
					return VkFormat.Astc12x12UnormBlock;
				//return VK_FORMAT_ASTC_12x12_UNORM_BLOCK; // 4-component ASTC, 12x12 blocks, unsigned normalized

				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_4x4_KHR:
					return VkFormat.Astc4x4SrgbBlock;
				//return VK_FORMAT_ASTC_4x4_SRGB_BLOCK; // 4-component ASTC, 4x4 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_5x4_KHR:
					return VkFormat.Astc5x4SrgbBlock;
				//return VK_FORMAT_ASTC_5x4_SRGB_BLOCK; // 4-component ASTC, 5x4 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_5x5_KHR:
					return VkFormat.Astc5x5SrgbBlock;
				//return VK_FORMAT_ASTC_5x5_SRGB_BLOCK; // 4-component ASTC, 5x5 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_6x5_KHR:
					return VkFormat.Astc6x5SrgbBlock;
				//return VK_FORMAT_ASTC_6x5_SRGB_BLOCK; // 4-component ASTC, 6x5 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_6x6_KHR:
					return VkFormat.Astc6x6SrgbBlock;
				//return VK_FORMAT_ASTC_6x6_SRGB_BLOCK; // 4-component ASTC, 6x6 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x5_KHR:
					return VkFormat.Astc8x5SrgbBlock;
				//return VK_FORMAT_ASTC_8x5_SRGB_BLOCK; // 4-component ASTC, 8x5 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x6_KHR:
					return VkFormat.Astc8x6SrgbBlock;
				//return VK_FORMAT_ASTC_8x6_SRGB_BLOCK; // 4-component ASTC, 8x6 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x8_KHR:
					return VkFormat.Astc8x8SrgbBlock;
				//return VK_FORMAT_ASTC_8x8_SRGB_BLOCK; // 4-component ASTC, 8x8 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x5_KHR:
					return VkFormat.Astc10x5SrgbBlock;
				//return VK_FORMAT_ASTC_10x5_SRGB_BLOCK; // 4-component ASTC, 10x5 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x6_KHR:
					return VkFormat.Astc10x6SrgbBlock;
				//return VK_FORMAT_ASTC_10x6_SRGB_BLOCK; // 4-component ASTC, 10x6 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x8_KHR:
					return VkFormat.Astc10x8SrgbBlock;
				//return VK_FORMAT_ASTC_10x8_SRGB_BLOCK; // 4-component ASTC, 10x8 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x10_KHR:
					return VkFormat.Astc10x10SrgbBlock;
				//return VK_FORMAT_ASTC_10x10_SRGB_BLOCK; // 4-component ASTC, 10x10 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_12x10_KHR:
					return VkFormat.Astc12x10SrgbBlock;
				//return VK_FORMAT_ASTC_12x10_SRGB_BLOCK; // 4-component ASTC, 12x10 blocks, sRGB
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_12x12_KHR:
					return VkFormat.Astc12x12SrgbBlock;
				//return VK_FORMAT_ASTC_12x12_SRGB_BLOCK; // 4-component ASTC, 12x12 blocks, sRGB

				default: return VkFormat.Undefined;
			}
		}
	}
}
