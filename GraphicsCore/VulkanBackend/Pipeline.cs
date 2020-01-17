using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Core.Graphics.VulkanBackend.Utility;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Core.Graphics.VulkanBackend
{
	public unsafe class Pipeline : IDisposable {
		public const uint VERTEX_DATA_BUFFER_BIND_ID = 0;
		public const uint VERTEX_INSTANCE_BUFFER_BIND_ID = 1;

		public VkPipeline vkPipeline;
		public VkPipelineLayout pipelineLayout;
		public VkDescriptorSet descriptorSet;
		public VkDescriptorSetLayout descriptorSetLayout;
		public VkDescriptorPool descriptorPool;
		public bool instanced;

		//protected NativeList<VkShaderModule> shaderModules = new NativeList<VkShaderModule>();

		protected GraphicsDevice device;


		//protected RenderPass renderPass;
		//protected PipelineCache pipelineCache;

		//vksTexture2D textures_colorMap = new vksTexture2D();
		//vksBuffer uniformBuffers_scene = new vksBuffer();
		//private string fragShader;
		//private string vertShader;

		VkPipelineVertexInputStateCreateInfo vertices_inputState;
		NativeList<VkVertexInputBindingDescription> vertices_bindingDescriptions = new NativeList<VkVertexInputBindingDescription>();
		NativeList<VkVertexInputAttributeDescription> vertices_attributeDescriptions = new NativeList<VkVertexInputAttributeDescription>();


		public Pipeline(GraphicsDevice device) {
			this.device = device;
		}

		public void Dispose() {
			vkDestroyDescriptorSetLayout(device.device, descriptorSetLayout, null);
			vkDestroyDescriptorPool(device.device, descriptorPool, null);
			vkDestroyPipelineLayout(device.device, pipelineLayout, null);
			vkDestroyPipeline(device.device, vkPipeline, null);
		}


		/*
		protected void Setup() {
			setupVertexDescriptions();
			prepareUniformBuffers();
			setupDescriptorSetLayout();
			preparePipelines();
			setupDescriptorPool();
			setupDescriptorSet();
			//buildCommandBuffers();
		}

		protected virtual void setupVertexDescriptions()
        {
            // Binding description
            vertices_bindingDescriptions.Count = 1;
            vertices_bindingDescriptions[0] =
                Initializers.vertexInputBindingDescription(
	                VERTEX_BUFFER_BIND_ID,
                    (uint)sizeof(Vertex),
                    VkVertexInputRate.Vertex);

            // Attribute descriptions
            // Describes memory layout and shader positions
            vertices_attributeDescriptions.Count = 4;
            // Location 0 : Position
            vertices_attributeDescriptions[0] =
                Initializers.vertexInputAttributeDescription(
	                VERTEX_BUFFER_BIND_ID,
                    0,
                    VkFormat.R32g32b32Sfloat,
                    0);
            // Location 1 : Normal
            vertices_attributeDescriptions[1] =
                Initializers.vertexInputAttributeDescription(
	                VERTEX_BUFFER_BIND_ID,
                    1,
                    VkFormat.R32g32b32Sfloat,
                    12);
            // Location 2 : Texture coordinates
            vertices_attributeDescriptions[2] =
                Initializers.vertexInputAttributeDescription(
	                VERTEX_BUFFER_BIND_ID,
                    2,
                    VkFormat.R32g32Sfloat,
                    24);
            // Location 3 : Color
            vertices_attributeDescriptions[3] =
                Initializers.vertexInputAttributeDescription(
	                VERTEX_BUFFER_BIND_ID,
                    3,
                    VkFormat.R32g32b32Sfloat,
                    32);


            vertices_inputState = Initializers.pipelineVertexInputStateCreateInfo();
            vertices_inputState.vertexBindingDescriptionCount = (vertices_bindingDescriptions.Count);
            vertices_inputState.pVertexBindingDescriptions = (VkVertexInputBindingDescription*)vertices_bindingDescriptions.Data;
            vertices_inputState.vertexAttributeDescriptionCount = (vertices_attributeDescriptions.Count);
            vertices_inputState.pVertexAttributeDescriptions = (VkVertexInputAttributeDescription*)vertices_attributeDescriptions.Data;
        }


		

		protected virtual void setupDescriptorPool()
        {
            // Example uses one ubo and one combined image sampler
            FixedArray2<VkDescriptorPoolSize> poolSizes = new FixedArray2<VkDescriptorPoolSize>(
                Initializers.descriptorPoolSize(VkDescriptorType.UniformBuffer, 1),
                Initializers.descriptorPoolSize(VkDescriptorType.CombinedImageSampler, 1));

            VkDescriptorPoolCreateInfo descriptorPoolInfo =
                Initializers.descriptorPoolCreateInfo(
                    poolSizes.Count,
                    &poolSizes.First,
                    1);

            Util.CheckResult(vkCreateDescriptorPool(device.device, &descriptorPoolInfo, null, out descriptorPool));
        }

        void setupDescriptorSetLayout()
        {
            FixedArray2<VkDescriptorSetLayoutBinding> setLayoutBindings = new FixedArray2<VkDescriptorSetLayoutBinding>(
                // Binding 0 : Vertex shader uniform buffer
                Initializers.descriptorSetLayoutBinding(
                    VkDescriptorType.UniformBuffer,
                    VkShaderStageFlags.Vertex,
                    0),
                // Binding 1 : Fragment shader combined sampler
                Initializers.descriptorSetLayoutBinding(
                    VkDescriptorType.CombinedImageSampler,
                    VkShaderStageFlags.Fragment,
                    1));

            VkDescriptorSetLayoutCreateInfo descriptorLayout =
                Initializers.descriptorSetLayoutCreateInfo(
                    &setLayoutBindings.First,
                    setLayoutBindings.Count);

            Util.CheckResult(vkCreateDescriptorSetLayout(device.device, &descriptorLayout, null, out descriptorSetLayout));

            var dsl = descriptorSetLayout;
            VkPipelineLayoutCreateInfo pPipelineLayoutCreateInfo =
                Initializers.pipelineLayoutCreateInfo(
                    &dsl,
                    1);

            Util.CheckResult(vkCreatePipelineLayout(device.device, &pPipelineLayoutCreateInfo, null, out pipelineLayout));
        }

        void setupDescriptorSet()
        {
            var dsl = descriptorSetLayout;
            VkDescriptorSetAllocateInfo allocInfo =
                Initializers.descriptorSetAllocateInfo(
                    descriptorPool,
                    &dsl,
                    1);

            Util.CheckResult(vkAllocateDescriptorSets(device.device, &allocInfo, out descriptorSet));

            VkDescriptorImageInfo texDescriptor =
                Initializers.descriptorImageInfo(
                    textures_colorMap.sampler,
                    textures_colorMap.view,
                    VkImageLayout.General);

            var temp = uniformBuffers_scene.descriptor;
            FixedArray2<VkWriteDescriptorSet> writeDescriptorSets = new FixedArray2<VkWriteDescriptorSet>(
                // Binding 0 : Vertex shader uniform buffer
                Initializers.writeDescriptorSet(
                    descriptorSet,
                    VkDescriptorType.UniformBuffer,
                    0,
                    &temp),
                // Binding 1 : Color map 
                Initializers.writeDescriptorSet(
                    descriptorSet,
                    VkDescriptorType.CombinedImageSampler,
                    1,
                    &texDescriptor));

            vkUpdateDescriptorSets(device.device, (writeDescriptorSets.Count), ref writeDescriptorSets.First, 0, null);
        }

        void preparePipelines()
        {
            VkPipelineInputAssemblyStateCreateInfo inputAssemblyState =
                Initializers.pipelineInputAssemblyStateCreateInfo(
                    VkPrimitiveTopology.TriangleList,
                    0,
                    False);

            VkPipelineRasterizationStateCreateInfo rasterizationState =
                Initializers.pipelineRasterizationStateCreateInfo(
                    VkPolygonMode.Fill,
                    VkCullModeFlags.Back,
                    VkFrontFace.Clockwise,
                    0);

            VkPipelineColorBlendAttachmentState blendAttachmentState =
                Initializers.pipelineColorBlendAttachmentState(
                    0xf,
                    False);

            VkPipelineColorBlendStateCreateInfo colorBlendState =
                Initializers.pipelineColorBlendStateCreateInfo(
                    1,
                    &blendAttachmentState);

            VkPipelineDepthStencilStateCreateInfo depthStencilState =
                Initializers.pipelineDepthStencilStateCreateInfo(
                    True,
                    True,
                     VkCompareOp.LessOrEqual);

            VkPipelineViewportStateCreateInfo viewportState =
                Initializers.pipelineViewportStateCreateInfo(1, 1, 0);

            VkPipelineMultisampleStateCreateInfo multisampleState =
                Initializers.pipelineMultisampleStateCreateInfo(
                    VkSampleCountFlags.Count1,
                    0);

            FixedArray2<VkDynamicState> dynamicStateEnables = new FixedArray2<VkDynamicState>(
                 VkDynamicState.Viewport,
                 VkDynamicState.Scissor);
            VkPipelineDynamicStateCreateInfo dynamicState =
                Initializers.pipelineDynamicStateCreateInfo(
                    &dynamicStateEnables.First,
                    dynamicStateEnables.Count,
                    0);

            // Solid rendering pipeline
            // Load shaders
            FixedArray2<VkPipelineShaderStageCreateInfo> shaderStages = new FixedArray2<VkPipelineShaderStageCreateInfo>(
                loadShader(getAssetPath() + "shaders/mesh/mesh.vert.spv", VkShaderStageFlags.Vertex),
                loadShader(getAssetPath() + "shaders/mesh/mesh.frag.spv", VkShaderStageFlags.Fragment));

            VkGraphicsPipelineCreateInfo pipelineCreateInfo =
                Initializers.pipelineCreateInfo(
                    pipelineLayout,
                    renderPass.vkRenderPass,
                    0);

            var via = new VkPipelineVertexInputStateCreateInfo();
            pipelineCreateInfo.pVertexInputState = &via;
            pipelineCreateInfo.pInputAssemblyState = &inputAssemblyState;
            pipelineCreateInfo.pRasterizationState = &rasterizationState;
            pipelineCreateInfo.pColorBlendState = &colorBlendState;
            pipelineCreateInfo.pMultisampleState = &multisampleState;
            pipelineCreateInfo.pViewportState = &viewportState;
            pipelineCreateInfo.pDepthStencilState = &depthStencilState;
            pipelineCreateInfo.pDynamicState = &dynamicState;
            pipelineCreateInfo.stageCount = shaderStages.Count;
            pipelineCreateInfo.pStages = &shaderStages.First;

            Util.CheckResult(vkCreateGraphicsPipelines(device.device, pipelineCache.vkPipelineCache, 1, &pipelineCreateInfo, null, out vkPipeline));

            // Wire frame rendering pipeline
            //if (device.DeviceFeatures.fillModeNonSolid == 1)
            //{
            //    rasterizationState.polygonMode = VkPolygonMode.Line;
            //    rasterizationState.lineWidth = 1.0f;
            //    Util.CheckResult(vkCreateGraphicsPipelines(device.device, pipelineCache.vkPipelineCache, 1, &pipelineCreateInfo, null, out pipelines_wireframe));
            //}
        }

        // Prepare and initialize uniform buffer containing shader uniforms
        void prepareUniformBuffers()
        {
            // Vertex shader uniform buffer block
            Util.CheckResult(device.vulkanDevice.createBuffer(
                VkBufferUsageFlags.UniformBuffer,
                VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent,
                uniformBuffers_scene,
                (uint)sizeof(UniformBufferObject)));

            // Map persistent
            Util.CheckResult(uniformBuffers_scene.map());
        }

        public void updateUniformBuffers(UniformBufferObject ubo)
        {
	        //uboVS.projection = System.Numerics.Matrix4x4.CreatePerspectiveFieldOfView(Util.DegreesToRadians(60.0f), (float)width / (float)height, 0.1f, 256.0f);
	        //System.Numerics.Matrix4x4 viewMatrix = System.Numerics.Matrix4x4.CreateTranslation(0.0f, 0.0f, zoom);

	        //uboVS.model = viewMatrix * System.Numerics.Matrix4x4.CreateTranslation(cameraPos);
	        //uboVS.model = System.Numerics.Matrix4x4.CreateRotationX(Util.DegreesToRadians(rotation.X)) * uboVS.model;
	        //uboVS.model = System.Numerics.Matrix4x4.CreateRotationY(Util.DegreesToRadians(rotation.Y)) * uboVS.model;
	        //uboVS.model = System.Numerics.Matrix4x4.CreateRotationZ(Util.DegreesToRadians(rotation.Z)) * uboVS.model;

	        Unsafe.Copy(uniformBuffers_scene.mapped, ref ubo);
        }


        protected VkPipelineShaderStageCreateInfo loadShader(string fileName, VkShaderStageFlags stage)
        {
	        VkPipelineShaderStageCreateInfo shaderStage = VkPipelineShaderStageCreateInfo.New();
	        shaderStage.stage = stage;
	        shaderStage.module = Tools.loadShader(fileName, device.device, stage);
	        shaderStage.pName = Strings.main; // todo : make param
	        Debug.Assert(shaderStage.module.Handle != 0);
	        shaderModules.Add(shaderStage.module);
	        return shaderStage;
        }

        protected string getAssetPath()
        {
	        return Path.Combine(AppContext.BaseDirectory, "data/");
        }
		*/
	}
}
