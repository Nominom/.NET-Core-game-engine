using System;
using System.Collections.Generic;
using System.Text;
using Core.Shared;
using Vulkan;

namespace Core.Graphics.VulkanBackend.Utility
{
	public static class GlFormatToVulkanFormat
	{
		

		public static VkFormat vkGetFormatFromOpenGLInternalFormat(GlInternalFormat internalFormat, GLFormat format, GLType glType)
		{
			switch (internalFormat)
			{
				//
				// Packed formats
				//

				case GlInternalFormat.GL_RGBA4:
					if (format == GLFormat.GL_RGBA) 
						return VkFormat.R4g4b4a4UnormPack16;
					else 
						return VkFormat.B4g4r4a4UnormPack16;
				case GlInternalFormat.GL_RGB565:
					if (glType == GLType.GL_UNSIGNED_SHORT_5_6_5) 
						return VkFormat.R5g6b5UnormPack16;
					else 
						return VkFormat.B5g6r5UnormPack16;
				case GlInternalFormat.GL_RGB5_A1:
					if (format == GLFormat.GL_RGBA)
						return VkFormat.R5g5b5a1UnormPack16;
					else if (glType == GLType.GL_UNSIGNED_SHORT_5_5_5_1)
						return VkFormat.B5g5r5a1UnormPack16;
					else {
						return VkFormat.A1r5g5b5UnormPack16;
					}
				
				//
				// Raw formats 8 bits
				//
				case GlInternalFormat.GL_R8:
					return VkFormat.R8Unorm;
				case GlInternalFormat.GL_R8_SNORM:
					return VkFormat.R8Snorm;
				case GlInternalFormat.GL_R8UI:
					return VkFormat.R8Uint;
				case GlInternalFormat.GL_R8I:
					return VkFormat.R8Sint;

				case GlInternalFormat.GL_RG8:
					return VkFormat.R8g8Unorm;
				case GlInternalFormat.GL_RG8_SNORM:
					return VkFormat.R8g8Snorm;
				case GlInternalFormat.GL_RG8UI:
					return VkFormat.R8g8Uint;
				case GlInternalFormat.GL_RG8I:
					return VkFormat.R8g8Sint;

				case GlInternalFormat.GL_RGB8:
					if (format == GLFormat.GL_RGB)
						return VkFormat.R8g8b8Unorm;
					else
						return VkFormat.B8g8r8Unorm;
				case GlInternalFormat.GL_RGB8_SNORM:
					if (format == GLFormat.GL_RGB)
						return VkFormat.R8g8b8Snorm;
					else
						return VkFormat.B8g8r8Snorm;
				case GlInternalFormat.GL_RGB8UI:
					if (format == GLFormat.GL_RGB)
						return VkFormat.R8g8b8Uint;
					else
						return VkFormat.B8g8r8Uint;
				case GlInternalFormat.GL_RGB8I:
					if (format == GLFormat.GL_RGB)
						return VkFormat.R8g8b8Sint;
					else
						return VkFormat.B8g8r8Sint;

				case GlInternalFormat.GL_RGBA8:
					if (format == GLFormat.GL_RGBA)
						return VkFormat.R8g8b8a8Unorm;
					else
						return VkFormat.B8g8r8a8Unorm;
				case GlInternalFormat.GL_RGBA8_SNORM:
					if (format == GLFormat.GL_RGBA)
						return VkFormat.R8g8b8a8Snorm;
					else
						return VkFormat.B8g8r8a8Snorm;
				case GlInternalFormat.GL_RGBA8UI:
					if (format == GLFormat.GL_RGBA)
						return VkFormat.R8g8b8a8Uint;
					else
						return VkFormat.B8g8r8a8Uint;
				case GlInternalFormat.GL_RGBA8I:
					if (format == GLFormat.GL_RGBA)
						return VkFormat.R8g8b8a8Sint;
					else
						return VkFormat.B8g8r8a8Sint;

				//
				// 10 bit color 2 alpha packed formats
				//
				case GlInternalFormat.GL_RGB10_A2:
					if (format == GLFormat.GL_RGBA)
						return VkFormat.A2r10g10b10SnormPack32;
					else 
						return VkFormat.A2b10g10r10SnormPack32;
				case GlInternalFormat.GL_RGB10_A2UI:
					if (format == GLFormat.GL_RGBA)
						return VkFormat.A2r10g10b10UintPack32;
					else 
						return VkFormat.A2b10g10r10UintPack32;

				//
				// Raw formats 16 bits
				//

				case GlInternalFormat.GL_R16:
					return VkFormat.R16Unorm;
				case GlInternalFormat.GL_R16_SNORM:
					return VkFormat.R16Snorm;
				case GlInternalFormat.GL_R16UI:
					return VkFormat.R16Uint;
				case GlInternalFormat.GL_R16I:
					return VkFormat.R16Sint;
				case GlInternalFormat.GL_R16F:
					return VkFormat.R16Sfloat;

				case GlInternalFormat.GL_RG16:
					return VkFormat.R16g16Unorm;
				case GlInternalFormat.GL_RG16_SNORM:
					return VkFormat.R16g16Snorm;
				case GlInternalFormat.GL_RG16UI:
					return VkFormat.R16g16Uint;
				case GlInternalFormat.GL_RG16I:
					return VkFormat.R16g16Sint;
				case GlInternalFormat.GL_RG16F:
					return VkFormat.R16g16Sfloat;

				case GlInternalFormat.GL_RGB16:
					return VkFormat.R16g16b16Unorm;
				case GlInternalFormat.GL_RGB16_SNORM:
					return VkFormat.R16g16b16Snorm;
				case GlInternalFormat.GL_RGB16UI:
					return VkFormat.R16g16b16Uint;
				case GlInternalFormat.GL_RGB16I:
					return VkFormat.R16g16b16Sint;
				case GlInternalFormat.GL_RGB16F:
					return VkFormat.R16g16b16Sfloat;

				case GlInternalFormat.GL_RGBA16:
					return VkFormat.R16g16b16a16Unorm;
				case GlInternalFormat.GL_RGBA16_SNORM:
					return VkFormat.R16g16b16a16Snorm;
				case GlInternalFormat.GL_RGBA16UI:
					return VkFormat.R16g16b16a16Uint;
				case GlInternalFormat.GL_RGBA16I:
					return VkFormat.R16g16b16a16Sint;
				case GlInternalFormat.GL_RGBA16F:
					return VkFormat.R16g16b16a16Sfloat;

				//
				// Raw formats 32 bits
				//

				case GlInternalFormat.GL_R32UI:
					return VkFormat.R32Uint;
				case GlInternalFormat.GL_R32I:
					return VkFormat.R32Sint;
				case GlInternalFormat.GL_R32F:
					return VkFormat.R32Sfloat;

				case GlInternalFormat.GL_RG32UI:
					return VkFormat.R32g32Uint;
				case GlInternalFormat.GL_RG32I:
					return VkFormat.R32g32Sint;
				case GlInternalFormat.GL_RG32F:
					return VkFormat.R32g32Sfloat;

				case GlInternalFormat.GL_RGB32UI:
					return VkFormat.R32g32b32Uint;
				case GlInternalFormat.GL_RGB32I:
					return VkFormat.R32g32b32Sint;
				case GlInternalFormat.GL_RGB32F:
					return VkFormat.R32g32b32Sfloat;

				case GlInternalFormat.GL_RGBA32UI:
					return VkFormat.R32g32b32a32Uint;
				case GlInternalFormat.GL_RGBA32I:
					return VkFormat.R32g32b32a32Sint;
				case GlInternalFormat.GL_RGBA32F:
					return VkFormat.R32g32b32a32Sfloat;

				//
				// Depth formats
				//

				case GlInternalFormat.GL_DEPTH_COMPONENT16:
					return VkFormat.D16Unorm;
				case GlInternalFormat.GL_DEPTH_COMPONENT24:
					return VkFormat.X8D24UnormPack32;
				case GlInternalFormat.GL_DEPTH_COMPONENT32F:
					return VkFormat.D32Sfloat;

				case GlInternalFormat.GL_STENCIL_INDEX8:
					return VkFormat.S8Uint;

				case GlInternalFormat.GL_DEPTH24_STENCIL8:
					return VkFormat.D24UnormS8Uint;
				case GlInternalFormat.GL_DEPTH32F_STENCIL8:
					return VkFormat.D32SfloatS8Uint;

				//
				// S3TC/DXT/BC
				//
				case GlInternalFormat.GL_COMPRESSED_RGB_S3TC_DXT1_EXT:
					return VkFormat.Bc1RgbUnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT:
					return VkFormat.Bc1RgbaUnormBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB_S3TC_DXT1_EXT:
					return VkFormat.Bc1RgbSrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT1_EXT:
					return VkFormat.Bc1RgbaSrgbBlock;

				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT:
					return VkFormat.Bc2UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT3_EXT:
					return VkFormat.Bc2SrgbBlock;


				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT:
					return VkFormat.Bc3UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT5_EXT:
					return VkFormat.Bc3SrgbBlock;

				case  GlInternalFormat.GL_COMPRESSED_RED_RGTC1_EXT:
					return VkFormat.Bc4UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_SIGNED_RED_RGTC1_EXT:
					return VkFormat.Bc4SnormBlock;

				case GlInternalFormat.GL_COMPRESSED_RED_GREEN_RGTC2_EXT:
					return VkFormat.Bc5UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_SIGNED_RED_GREEN_RGTC2_EXT:
					return VkFormat.Bc5SnormBlock;

				case GlInternalFormat.GL_COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT_ARB:
					return VkFormat.Bc6hUfloatBlock;
				case GlInternalFormat.GL_COMPRESSED_RGB_BPTC_SIGNED_FLOAT_ARB:
					return VkFormat.Bc6hSfloatBlock;

				case GlInternalFormat.GL_COMPRESSED_RGBA_BPTC_UNORM_ARB:
					return VkFormat.Bc7UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB_ALPHA_BPTC_UNORM_ARB:
					return VkFormat.Bc7SrgbBlock;

				//
				// ETC
				//
				case GlInternalFormat.GL_ETC1_RGB8_OES:
					return VkFormat.Etc2R8g8b8UnormBlock;
				
				case GlInternalFormat.GL_COMPRESSED_RGB8_ETC2:
					return VkFormat.Etc2R8g8b8UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2:
					return VkFormat.Etc2R8g8b8a1UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA8_ETC2_EAC:
					return VkFormat.Etc2R8g8b8a8UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ETC2:
					return VkFormat.Etc2R8g8b8SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2:
					return VkFormat.Etc2R8g8b8a1SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC:
					return VkFormat.Etc2R8g8b8a8SrgbBlock;

				case GlInternalFormat.GL_COMPRESSED_R11_EAC:
					return VkFormat.EacR11UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RG11_EAC:
					return VkFormat.EacR11g11UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_SIGNED_R11_EAC:
					return VkFormat.EacR11SnormBlock;
				case GlInternalFormat.GL_COMPRESSED_SIGNED_RG11_EAC:
					return VkFormat.EacR11g11SnormBlock;

				//
				// ASTC
				//
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_4x4_KHR:
					return VkFormat.Astc4x4UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_5x4_KHR:
					return VkFormat.Astc5x4UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_5x5_KHR:
					return VkFormat.Astc5x5UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_6x5_KHR:
					return VkFormat.Astc6x5UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_6x6_KHR:
					return VkFormat.Astc6x6UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_8x5_KHR:
					return VkFormat.Astc8x5UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_8x6_KHR:
					return VkFormat.Astc8x6UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_8x8_KHR:
					return VkFormat.Astc8x8UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_10x5_KHR:
					return VkFormat.Astc10x5UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_10x6_KHR:
					return VkFormat.Astc10x6UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_10x8_KHR:
					return VkFormat.Astc10x8UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_10x10_KHR:
					return VkFormat.Astc10x10UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_12x10_KHR:
					return VkFormat.Astc12x10UnormBlock;
				case GlInternalFormat.GL_COMPRESSED_RGBA_ASTC_12x12_KHR:
					return VkFormat.Astc12x12UnormBlock;

				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_4x4_KHR:
					return VkFormat.Astc4x4SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_5x4_KHR:
					return VkFormat.Astc5x4SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_5x5_KHR:
					return VkFormat.Astc5x5SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_6x5_KHR:
					return VkFormat.Astc6x5SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_6x6_KHR:
					return VkFormat.Astc6x6SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x5_KHR:
					return VkFormat.Astc8x5SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x6_KHR:
					return VkFormat.Astc8x6SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x8_KHR:
					return VkFormat.Astc8x8SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x5_KHR:
					return VkFormat.Astc10x5SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x6_KHR:
					return VkFormat.Astc10x6SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x8_KHR:
					return VkFormat.Astc10x8SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x10_KHR:
					return VkFormat.Astc10x10SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_12x10_KHR:
					return VkFormat.Astc12x10SrgbBlock;
				case GlInternalFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_12x12_KHR:
					return VkFormat.Astc12x12SrgbBlock;

				default: return VkFormat.Undefined;
			}
		}
	}
}
