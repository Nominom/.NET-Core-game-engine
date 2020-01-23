using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Core.Graphics.VulkanBackend.Utility;
using Vulkan;
using static Vulkan.VulkanNative;
using Version = Core.Graphics.VulkanBackend.Utility.Version;

namespace Core.Graphics.VulkanBackend
{
	public unsafe class GraphicsDevice : IDisposable
	{
		public readonly VkDevice device;
		public VkInstance Instance { get; protected set; }
		public GraphicsDeviceSettings Settings { get; } = new GraphicsDeviceSettings();
		public VkPhysicalDevice physicalDevice { get; protected set; }
		public vksVulkanDevice vulkanDevice { get; protected set; }
		public VkPhysicalDeviceFeatures enabledFeatures { get; protected set; }
		public VkPhysicalDeviceMemoryProperties DeviceMemoryProperties { get; set; }
		public VkPhysicalDeviceProperties DeviceProperties { get; protected set; }
		public VkPhysicalDeviceFeatures DeviceFeatures { get; protected set; }
		public NativeList<IntPtr> EnabledExtensions { get; } = new NativeList<IntPtr>();
		public VkFormat DepthFormat { get; protected set; }
		public VkQueue queue { get; protected set; }
		public NativeList<Semaphores> semaphores = new NativeList<Semaphores>(1, 1);
		public Semaphores* GetSemaphoresPtr() => (Semaphores*) semaphores.GetAddress(0);

		public VkSubmitInfo submitInfo;
		public NativeList<VkPipelineStageFlags> submitPipelineStages = CreateSubmitPipelineStages();
		private static NativeList<VkPipelineStageFlags> CreateSubmitPipelineStages()
			=> new NativeList<VkPipelineStageFlags>() { VkPipelineStageFlags.ColorAttachmentOutput };


		private ThreadLocal<CommandPool> threadCommandPools  = new ThreadLocal<CommandPool>(true);

		public Swapchain mainSwapchain { get; protected set; }
		public DepthStencil depthStencil { get; protected set; }
		public RenderPass startOfFrameRenderPass { get; protected set; }
		public RenderPass renderPass { get; protected set; }
		public RenderPass presentRenderPass { get; protected set; }
		public RenderPass singlePass { get; protected set; }
		public FrameBuffer[] frameBuffers { get; protected set; }
		public PipelineCache pipelineCache { get; protected set; }
		public VulkanMemoryAllocator memoryAllocator { get; protected set; }


		private uint currentBuffer = 0;

		public GraphicsDevice(bool enableValidation) {
			VkResult err;
            err = CreateInstance("VulkanDevice", enableValidation);
            if (err != VkResult.Success)
            {
                throw new InvalidOperationException("Could not create Vulkan instance. Error: " + err);
            }
            if (Settings.Validation)
            {

            }

            // Physical Device
            uint gpuCount = 0;
            Util.CheckResult(vkEnumeratePhysicalDevices(Instance, &gpuCount, null));
            Debug.Assert(gpuCount > 0);

            // Enumerate devices
            IntPtr* physicalDevices = stackalloc IntPtr[(int)gpuCount];
            err = vkEnumeratePhysicalDevices(Instance, &gpuCount, (VkPhysicalDevice*)physicalDevices);
            if (err != VkResult.Success)
            {
                throw new InvalidOperationException("Could not enumerate physical devices.");
            }

            // GPU selection

            uint selectedDevice = 0;
            VkPhysicalDeviceType bestType = VkPhysicalDeviceType.Other;
            ulong bestDeviceLocalMemSize = 0;
            for (uint i = 0; i < gpuCount; i++) {
	            VkPhysicalDevice device = ((VkPhysicalDevice*)physicalDevices)[i];
	            VkPhysicalDeviceProperties devProps;
	            vkGetPhysicalDeviceProperties(device, &devProps);


	            if (devProps.deviceType == VkPhysicalDeviceType.IntegratedGpu 
	                && bestType != VkPhysicalDeviceType.IntegratedGpu
	                && bestType != VkPhysicalDeviceType.DiscreteGpu) {

		            selectedDevice = i;
		            bestType = VkPhysicalDeviceType.IntegratedGpu;
		            bestDeviceLocalMemSize = GetDeviceLocalMemorySize(device);
	            }
				else if (devProps.deviceType == VkPhysicalDeviceType.DiscreteGpu
				         && bestType != VkPhysicalDeviceType.DiscreteGpu) {
					//Found a discrete GPU, we can choose it.
		            selectedDevice = i;
		            bestType = VkPhysicalDeviceType.DiscreteGpu;
		            bestDeviceLocalMemSize = GetDeviceLocalMemorySize(device);
	            }
				else if (devProps.deviceType == VkPhysicalDeviceType.DiscreteGpu
				         && bestType == VkPhysicalDeviceType.DiscreteGpu) {
		            ulong memSize = GetDeviceLocalMemorySize(device);
		            if (memSize > bestDeviceLocalMemSize) {

		            }selectedDevice = i;
		            bestType = VkPhysicalDeviceType.DiscreteGpu;
		            bestDeviceLocalMemSize = memSize;
	            }
            }

			Console.WriteLine($"Found a vulkan capable GPU of type {bestType} at location {selectedDevice}. With {bestDeviceLocalMemSize / (1024 * 1024)}MiB of memory.");
            // Select physical Device to be used for the Vulkan example
            // Defaults to the first Device unless specified by command line

            
            // TODO: Implement arg parsing, etc.



            physicalDevice = ((VkPhysicalDevice*)physicalDevices)[selectedDevice];

            Console.WriteLine(PrintDeviceMemoryData(physicalDevice));


            // Store properties (including limits) and features of the phyiscal Device
            // So examples can check against them and see if a feature is actually supported
            VkPhysicalDeviceProperties deviceProperties;
            vkGetPhysicalDeviceProperties(physicalDevice, &deviceProperties);
            DeviceProperties = deviceProperties;

            VkPhysicalDeviceFeatures deviceFeatures;
            vkGetPhysicalDeviceFeatures(physicalDevice, &deviceFeatures);
            DeviceFeatures = deviceFeatures;

            // Gather physical Device memory properties
            VkPhysicalDeviceMemoryProperties deviceMemoryProperties;
            vkGetPhysicalDeviceMemoryProperties(physicalDevice, &deviceMemoryProperties);
            DeviceMemoryProperties = deviceMemoryProperties;

            // Derived examples can override this to set actual features (based on above readings) to enable for logical device creation
            //getEnabledFeatures();

            // Vulkan Device creation
            // This is handled by a separate class that gets a logical Device representation
            // and encapsulates functions related to a Device
            vulkanDevice = new vksVulkanDevice(physicalDevice);
            VkResult res = vulkanDevice.CreateLogicalDevice(enabledFeatures, EnabledExtensions);
            if (res != VkResult.Success)
            {
                throw new InvalidOperationException("Could not create Vulkan Device.");
            }
            device = vulkanDevice.LogicalDevice;

            // Get a graphics queue from the Device
            VkQueue queue;
            vkGetDeviceQueue(device, vulkanDevice.QFIndices.Graphics, 0, &queue);
            this.queue = queue;

            // Find a suitable depth format
            VkFormat depthFormat;
            uint validDepthFormat = Tools.GetSupportedDepthFormat(physicalDevice, &depthFormat);
            Debug.Assert(validDepthFormat == True);
            DepthFormat = depthFormat;

			mainSwapchain = new Swapchain(this);
            mainSwapchain.Connect(Instance, physicalDevice, device);

            // Create synchronization objects
            VkSemaphoreCreateInfo semaphoreCreateInfo = Initializers.semaphoreCreateInfo();
            // Create a semaphore used to synchronize image presentation
            // Ensures that the image is displayed before we start submitting new commands to the queu
            Util.CheckResult(vkCreateSemaphore(device, &semaphoreCreateInfo, null, &GetSemaphoresPtr()->PresentComplete));
            // Create a semaphore used to synchronize command submission
            // Ensures that the image is not presented until all commands have been sumbitted and executed
            Util.CheckResult(vkCreateSemaphore(device, &semaphoreCreateInfo, null, &GetSemaphoresPtr()->RenderComplete));
            // Create a semaphore used to synchronize command submission
            // Ensures that the image is not presented until all commands for the text overlay have been sumbitted and executed
            // Will be inserted after the render complete semaphore if the text overlay is enabled
            Util.CheckResult(vkCreateSemaphore(device, &semaphoreCreateInfo, null, &GetSemaphoresPtr()->TextOverlayComplete));

            // Set up submit info structure
            // Semaphores will stay the same during application lifetime
            // Command buffer submission info is set by each example
            submitInfo = Initializers.SubmitInfo();
            submitInfo.pWaitDstStageMask = (VkPipelineStageFlags*)submitPipelineStages.Data;
            submitInfo.waitSemaphoreCount = 1;
            submitInfo.pWaitSemaphores = &GetSemaphoresPtr()->PresentComplete;
            submitInfo.signalSemaphoreCount = 1;
            submitInfo.pSignalSemaphores = &GetSemaphoresPtr()->RenderComplete;

            memoryAllocator = new VulkanMemoryAllocator(this);
		}

		public void ConnectToWindow(IntPtr windowHandle) {
			this.mainSwapchain.InitSurface(windowHandle);
			this.mainSwapchain.RecreateSwapchain();
		}

		public void SetupFrameBuffersAndRenderPass() {
			depthStencil = new DepthStencil(this, mainSwapchain);
			startOfFrameRenderPass = RenderPass.ClearTargetsPass(this, mainSwapchain);
			renderPass = RenderPass.Default(this, mainSwapchain);
			presentRenderPass = RenderPass.Present(this, mainSwapchain);
			singlePass = RenderPass.Single(this, mainSwapchain);

			pipelineCache = new PipelineCache(this);

			uint buffercount = mainSwapchain.vkSwapchain.Buffers.Count;
			frameBuffers = new FrameBuffer[buffercount];
			for (uint i = 0; i < buffercount; i++)
			{
				frameBuffers[i] = new FrameBuffer(this, startOfFrameRenderPass, depthStencil, mainSwapchain, i);
			}
		}

		public void FlushCommandBuffer(CommandBuffer cmd) {
			if (cmd.vkCmd == NullHandle)
			{
				return;
			}
			if (!cmd.ended) {
				cmd.End();
			}

			var vkCmd = cmd.vkCmd;

			VkSubmitInfo submitInfo = VkSubmitInfo.New();
			submitInfo.commandBufferCount = 1;
			submitInfo.pCommandBuffers = &vkCmd;

			VkFenceCreateInfo fenceInfo = VkFenceCreateInfo.New();
			Util.CheckResult(vkCreateFence(device, &fenceInfo, null, out VkFence fence));

			Util.CheckResult(vkQueueSubmit(queue, 1, &submitInfo, fence));

			//Wait for CommandBuffer to finish
			Util.CheckResult(vkWaitForFences(device, 1, ref fence, true, 10000000000));

			vkDestroyFence(device, fence, null);
		}

		public void SubmitCommandBuffer(CommandBuffer cmd) {
			if (cmd.vkCmd == NullHandle)
			{
				return;
			}

			if (!cmd.ended) {
				cmd.End();
			}

			var vkCmd = cmd.vkCmd;

			VkSubmitInfo submitInfo = VkSubmitInfo.New();
			submitInfo.commandBufferCount = 1;
			submitInfo.pCommandBuffers = &vkCmd;

			Util.CheckResult(vkQueueSubmit(queue, 1, &submitInfo, VkFence.Null));
		}

		public void StartFrame() {
			Util.CheckResult(mainSwapchain.vkSwapchain.AcquireNextImage(semaphores[0].PresentComplete, ref currentBuffer));
		}

		public void FinalizeFrame() {
			using CommandBuffer buffer = GetCommandPool().Rent();
			buffer.Begin();
			buffer.BeginRenderPass(presentRenderPass, GetCurrentFrameBuffer(), false);
			buffer.EndRenderPass();
			buffer.End();

			var vkCmd = buffer.vkCmd;

			submitInfo.commandBufferCount = 1;
			submitInfo.pCommandBuffers = &vkCmd;

			Util.CheckResult(vkQueueSubmit(queue, 1, ref submitInfo, VkFence.Null));

			Util.CheckResult(mainSwapchain.vkSwapchain.QueuePresent(queue, currentBuffer, semaphores[0].RenderComplete));
			
			//WaitQueueIdle();
		}

		public void SubmitAndFinalizeFrame(CommandBuffer commandBuffer) {
			if (!commandBuffer.ended) {
				commandBuffer.End();
			}

			var vkCmd = commandBuffer.vkCmd;

			submitInfo.commandBufferCount = 1;
			submitInfo.pCommandBuffers = &vkCmd;

			Util.CheckResult(vkQueueSubmit(queue, 1, ref submitInfo, VkFence.Null));

			Util.CheckResult(mainSwapchain.vkSwapchain.QueuePresent(queue, currentBuffer, semaphores[0].RenderComplete));
			
			//WaitQueueIdle();
		}

		public FrameBuffer GetCurrentFrameBuffer() {
			return frameBuffers[currentBuffer];
		}

		/// <summary>
		/// Gets the command pool for the current thread.
		/// If one is not found. One is created.
		/// </summary>
		public CommandPool GetCommandPool() {
			if (!threadCommandPools.IsValueCreated) {
				threadCommandPools.Value = new CommandPool(this, mainSwapchain);
			}
			return threadCommandPools.Value;
		}

		public void WaitIdle() {
			Util.CheckResult(vkDeviceWaitIdle(device));
		}

		public void WaitQueueIdle() {
			Util.CheckResult(vkQueueWaitIdle(queue));
		}

		private VkResult CreateInstance(FixedUtf8String name, bool enableValidation)
        {
            Settings.Validation = enableValidation;

            VkApplicationInfo appInfo = new VkApplicationInfo()
            {
                sType = VkStructureType.ApplicationInfo,
                apiVersion = new Version(1, 0, 0),
                pApplicationName = name,
                pEngineName = name,
            };

            using NativeList<IntPtr> instanceExtensions = new NativeList<IntPtr>(2);
            instanceExtensions.Add(new FixedUtf8String(Strings.VK_KHR_SURFACE_EXTENSION_NAME));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                instanceExtensions.Add(new FixedUtf8String(Strings.VK_KHR_WIN32_SURFACE_EXTENSION_NAME));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                instanceExtensions.Add(new FixedUtf8String(Strings.VK_KHR_XLIB_SURFACE_EXTENSION_NAME));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            VkInstanceCreateInfo instanceCreateInfo = VkInstanceCreateInfo.New();
            instanceCreateInfo.pApplicationInfo = &appInfo;

            if (instanceExtensions.Count > 0)
            {
                if (enableValidation)
                {
                    instanceExtensions.Add(new FixedUtf8String(Strings.VK_EXT_DEBUG_REPORT_EXTENSION_NAME));
                }
                instanceCreateInfo.enabledExtensionCount = instanceExtensions.Count;
                instanceCreateInfo.ppEnabledExtensionNames = (byte**)instanceExtensions.Data;
            }

            using NativeList<IntPtr> enabledLayerNames = new NativeList<IntPtr>(0);
            if (enableValidation) {
	            enabledLayerNames.Add(new FixedUtf8String(Strings.StandardValidationLayerName));
                instanceCreateInfo.enabledLayerCount = enabledLayerNames.Count;
                instanceCreateInfo.ppEnabledLayerNames = (byte**)enabledLayerNames.Data;
            }

            VkInstance instance;
            VkResult result = vkCreateInstance(&instanceCreateInfo, null, &instance);
            Instance = instance;

            return result;
        }

		public void Dispose() {
			semaphores?.Dispose();
			submitPipelineStages?.Dispose();
			threadCommandPools?.Dispose();
			EnabledExtensions?.Dispose();
			depthStencil?.Dispose();
			startOfFrameRenderPass.Dispose();
			renderPass.Dispose();
			presentRenderPass.Dispose();
			singlePass.Dispose();

			for (uint i = 0; i < frameBuffers.Length; i++)
			{
				frameBuffers[i].Dispose();
			}

			memoryAllocator?.Dispose();
		}

		public void RecreateSwapchainAndFrameBuffers() {
			WaitIdle();
			mainSwapchain.RecreateSwapchain();
			depthStencil.Recreate();
			for (uint i = 0; i < frameBuffers.Length; i++)
			{
				frameBuffers[i].Recreate();
			}
			WaitIdle();
		}

		private static ulong GetDeviceLocalMemorySize(VkPhysicalDevice device) {
			VkPhysicalDeviceMemoryProperties memProps;
			vkGetPhysicalDeviceMemoryProperties(device, out memProps);
			ulong heapSize = 0;

			for (uint i = 0; i < memProps.memoryHeapCount; i++) {
				var heap = memProps.GetMemoryHeap(i);
				if ((heap.flags & VkMemoryHeapFlags.DeviceLocal) != 0) {
					if (heap.size > heapSize) {
						heapSize = heap.size;
					}
				}
			}

			return heapSize;
		}

		private static string PrintDeviceMemoryData(VkPhysicalDevice device) {
			StringBuilder sb = new StringBuilder();
			VkPhysicalDeviceMemoryProperties memProps;
			vkGetPhysicalDeviceMemoryProperties(device, out memProps);

			sb.AppendLine("Heaps: \n");
			for (uint i = 0; i < memProps.memoryHeapCount; i++) {
				var heap = memProps.GetMemoryHeap(i);
				sb.AppendFormat("Heap {0}:\n", i.ToString());
				sb.Append("Size: ");
				sb.AppendLine(heap.size.ToString());

				sb.Append("Flags: ");
				if ((heap.flags & VkMemoryHeapFlags.DeviceLocal) != 0) {
					sb.Append("DeviceLocal ");
				}

				if ((heap.flags & VkMemoryHeapFlags.MultiInstanceKHX) != 0) {
					sb.Append("MultiInstanceKHX ");
				}

				if (heap.flags == VkMemoryHeapFlags.None) {
					sb.Append("None");
				}

				sb.Append("\n\n");
			}

			sb.Append("\n\n");

			sb.AppendLine("MemoryTypes: \n");

			for (uint i = 0; i < memProps.memoryTypeCount; i++) {
				var memType = memProps.GetMemoryType(i);
				sb.AppendFormat("Type {0}\n", i.ToString());
				sb.Append("Heap: ");
				sb.AppendLine(memType.heapIndex.ToString());

				sb.Append("Flags: ");
				if ((memType.propertyFlags & VkMemoryPropertyFlags.DeviceLocal) != 0) {
					sb.Append("DeviceLocal ");
				}

				if ((memType.propertyFlags & VkMemoryPropertyFlags.HostVisible) != 0) {
					sb.Append("HostVisible ");
				}

				if ((memType.propertyFlags & VkMemoryPropertyFlags.HostCoherent) != 0) {
					sb.Append("HostCoherent ");
				}

				if ((memType.propertyFlags & VkMemoryPropertyFlags.HostCached) != 0) {
					sb.Append("HostCached ");
				}

				if ((memType.propertyFlags & VkMemoryPropertyFlags.LazilyAllocated) != 0) {
					sb.Append("LazilyAllocated ");
				}

				if (memType.propertyFlags == VkMemoryPropertyFlags.None) {
					sb.Append("None");
				}

				sb.Append("\n\n");
			}

			return sb.ToString();
		}
	}

	public struct Semaphores
	{
		public VkSemaphore PresentComplete;
		public VkSemaphore RenderComplete;
		public VkSemaphore TextOverlayComplete;
	}

	public class GraphicsDeviceSettings
	{
		public bool Validation { get; set; } = false;
		public bool Fullscreen { get; set; } = false;
		public bool VSync { get; set; } = false;
	}
}
