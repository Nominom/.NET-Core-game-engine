using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Core.Graphics.VulkanBackend;
using Core.Graphics.VulkanBackend.Utility;
using Veldrid;
using Vulkan;

namespace Core.Graphics
{
	public struct RenderContext {
		public Camera activeCamera;
		public Vector3 cameraPosition;
		public Quaternion cameraRotation;
		public RenderPass currentRenderPass;
		public FrameBuffer currentFrameBuffer;
		public uint currentSubPassIndex;
		public CommandBuffer frameCommands;
		public List<CommandBuffer> secondaryBuffers;
		public UniformBufferObject ubo;

		public readonly CommandBuffer GetCommandBuffer() {
			return GraphicsContext.graphicsDevice.GetCommandPool().Rent(VkCommandBufferLevel.Secondary);
		}

		public readonly void SubmitCommands(CommandBuffer cmd) {
			if (cmd.level == VkCommandBufferLevel.Primary) {
				GraphicsContext.graphicsDevice.SubmitCommandBuffer(cmd);
			}
			else {
				secondaryBuffers.Add(cmd);
			}
		}

		public readonly void SetUniformNow(UniformBufferObject uniform) {
			GraphicsContext.graphicsDevice.WaitIdle();
			GraphicsContext.uniform0.SetData(uniform);
		}

		public readonly void SetUniformCmd(CommandBuffer cmd, UniformBufferObject uniform) {
			cmd.UpdateBuffer(uniform, GraphicsContext.uniform0, 0);
		}
	}
}
