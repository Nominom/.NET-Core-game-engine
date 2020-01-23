using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Core.ECS.Components;
using Core.Graphics.VulkanBackend.Utility;
using Core.Shared;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Core.Graphics.VulkanBackend
{
	public unsafe class PipelineCreator : IDisposable {

		VkPipelineVertexInputStateCreateInfo vertices_inputState;
		NativeList<VkVertexInputBindingDescription> vertices_bindingDescriptions = new NativeList<VkVertexInputBindingDescription>();
		NativeList<VkVertexInputAttributeDescription> vertices_attributeDescriptions = new NativeList<VkVertexInputAttributeDescription>();


		public Pipeline CreatePipeline(GraphicsDevice device, RenderPass renderPass, Material material, UniformBuffer uniformBuffer) {
			Pipeline pipeline = new Pipeline(device);

			if (material.shaderPair.shaderType == ShaderType.Normal) {
				SetupNormalVertexDescriptions();
				pipeline.instanced = false;
			}
			else {
				SetupInstancedVertexDescriptions();
				pipeline.instanced = true;
			}

			SetupDescriptorSetLayout(pipeline, device);
			PreparePipelines(pipeline, device, renderPass, material);
			SetupDescriptorPool(pipeline, device);
			if(material.mainTexture != null)
			{
				SetupDescriptorSet(pipeline, device, uniformBuffer, material.mainTexture);
			}
			return pipeline;
		}



		protected virtual void SetupNormalVertexDescriptions()
        {
            // Binding description
            vertices_bindingDescriptions.Count = 1;
            vertices_bindingDescriptions[0] =
                Initializers.vertexInputBindingDescription(
	                Pipeline.VERTEX_DATA_BUFFER_BIND_ID,
                    (uint)sizeof(Vertex),
                    VkVertexInputRate.Vertex);

            // Attribute descriptions
            // Describes memory layout and shader positions
            vertices_attributeDescriptions.Count = 3;
            // Location 0 : Position
            vertices_attributeDescriptions[0] =
                Initializers.vertexInputAttributeDescription(
	                Pipeline.VERTEX_DATA_BUFFER_BIND_ID,
                    0,
                    VkFormat.R32g32b32Sfloat,
                    0);
            // Location 1 : Normal
            vertices_attributeDescriptions[1] =
                Initializers.vertexInputAttributeDescription(
	                Pipeline.VERTEX_DATA_BUFFER_BIND_ID,
                    1,
                    VkFormat.R32g32b32Sfloat,
                    12);
            // Location 2 : Texture coordinates
            vertices_attributeDescriptions[2] =
                Initializers.vertexInputAttributeDescription(
	                Pipeline.VERTEX_DATA_BUFFER_BIND_ID,
                    2,
                    VkFormat.R32g32Sfloat,
                    24);
            // Location 3 : Color
            //vertices_attributeDescriptions[3] =
            //    Initializers.vertexInputAttributeDescription(
	           //     Pipeline.VERTEX_DATA_BUFFER_BIND_ID,
            //        3,
            //        VkFormat.R32g32b32Sfloat,
            //        32);


            vertices_inputState = Initializers.pipelineVertexInputStateCreateInfo();
            vertices_inputState.vertexBindingDescriptionCount = (vertices_bindingDescriptions.Count);
            vertices_inputState.pVertexBindingDescriptions = (VkVertexInputBindingDescription*)vertices_bindingDescriptions.Data;
            vertices_inputState.vertexAttributeDescriptionCount = (vertices_attributeDescriptions.Count);
            vertices_inputState.pVertexAttributeDescriptions = (VkVertexInputAttributeDescription*)vertices_attributeDescriptions.Data;
        }

		protected virtual void SetupInstancedVertexDescriptions()
        {
            // Binding description
            vertices_bindingDescriptions.Count = 2;
            vertices_bindingDescriptions[0] =
                Initializers.vertexInputBindingDescription(
	                Pipeline.VERTEX_DATA_BUFFER_BIND_ID,
                    (uint)sizeof(Vertex),
                    VkVertexInputRate.Vertex);

            vertices_bindingDescriptions[1] =
	            Initializers.vertexInputBindingDescription(
		            Pipeline.VERTEX_INSTANCE_BUFFER_BIND_ID,
		            (uint)sizeof(ObjectToWorld),
		            VkVertexInputRate.Instance);

            // Attribute descriptions
            // Describes memory layout and shader positions
            vertices_attributeDescriptions.Count = 10;
            // Location 0 : Position
            vertices_attributeDescriptions[0] =
                Initializers.vertexInputAttributeDescription(
	                Pipeline.VERTEX_DATA_BUFFER_BIND_ID,
                    0,
                    VkFormat.R32g32b32Sfloat,
                    0);
            // Location 1 : Normal
            vertices_attributeDescriptions[1] =
                Initializers.vertexInputAttributeDescription(
	                Pipeline.VERTEX_DATA_BUFFER_BIND_ID,
                    1,
                    VkFormat.R32g32b32Sfloat,
                    12);
            // Location 2 : Texture coordinates
            vertices_attributeDescriptions[2] =
                Initializers.vertexInputAttributeDescription(
	                Pipeline.VERTEX_DATA_BUFFER_BIND_ID,
                    2,
                    VkFormat.R32g32Sfloat,
                    24);
            // Location 3 : Color
            //vertices_attributeDescriptions[3] =
            //    Initializers.vertexInputAttributeDescription(
	           //     Pipeline.VERTEX_DATA_BUFFER_BIND_ID,
            //        3,
            //        VkFormat.R32g32b32Sfloat,
            //        32);



			//Location 3-9 Instance model matrix and normal matrix
            vertices_attributeDescriptions[3] =
	            Initializers.vertexInputAttributeDescription(
		            Pipeline.VERTEX_INSTANCE_BUFFER_BIND_ID,
		            3,
		            VkFormat.R32g32b32a32Sfloat,
		            0);
            vertices_attributeDescriptions[4] =
	            Initializers.vertexInputAttributeDescription(
		            Pipeline.VERTEX_INSTANCE_BUFFER_BIND_ID,
		            4,
		            VkFormat.R32g32b32a32Sfloat,
		            16);
            vertices_attributeDescriptions[5] =
	            Initializers.vertexInputAttributeDescription(
		            Pipeline.VERTEX_INSTANCE_BUFFER_BIND_ID,
		            5,
		            VkFormat.R32g32b32a32Sfloat,
		            32);
            vertices_attributeDescriptions[6] =
	            Initializers.vertexInputAttributeDescription(
		            Pipeline.VERTEX_INSTANCE_BUFFER_BIND_ID,
		            6,
		            VkFormat.R32g32b32a32Sfloat,
		            48);
            vertices_attributeDescriptions[7] =
	            Initializers.vertexInputAttributeDescription(
		            Pipeline.VERTEX_INSTANCE_BUFFER_BIND_ID,
		            7,
		            VkFormat.R32g32b32Sfloat,
		            64);
            vertices_attributeDescriptions[8] =
	            Initializers.vertexInputAttributeDescription(
		            Pipeline.VERTEX_INSTANCE_BUFFER_BIND_ID,
		            8,
		            VkFormat.R32g32b32Sfloat,
		            76);
            vertices_attributeDescriptions[9] =
	            Initializers.vertexInputAttributeDescription(
		            Pipeline.VERTEX_INSTANCE_BUFFER_BIND_ID,
		            9,
		            VkFormat.R32g32b32Sfloat,
		            88);


            vertices_inputState = Initializers.pipelineVertexInputStateCreateInfo();
            vertices_inputState.vertexBindingDescriptionCount = (vertices_bindingDescriptions.Count);
            vertices_inputState.pVertexBindingDescriptions = (VkVertexInputBindingDescription*)vertices_bindingDescriptions.Data;
            vertices_inputState.vertexAttributeDescriptionCount = (vertices_attributeDescriptions.Count);
            vertices_inputState.pVertexAttributeDescriptions = (VkVertexInputAttributeDescription*)vertices_attributeDescriptions.Data;
        }



		void SetupDescriptorSetLayout(Pipeline pipeline, GraphicsDevice device)
		{
			FixedArray2<VkDescriptorSetLayoutBinding> setLayoutBindings = new FixedArray2<VkDescriptorSetLayoutBinding>(
				// Binding 0 : Vertex shader uniform buffer
				Initializers.descriptorSetLayoutBinding(
					VkDescriptorType.UniformBuffer,
					VkShaderStageFlags.Vertex |VkShaderStageFlags.Fragment,
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

			Util.CheckResult(vkCreateDescriptorSetLayout(device.device, &descriptorLayout, null, out pipeline.descriptorSetLayout));

			var dsl = pipeline.descriptorSetLayout;
			VkPipelineLayoutCreateInfo pPipelineLayoutCreateInfo =
				Initializers.pipelineLayoutCreateInfo(
					&dsl,
					1);

			Util.CheckResult(vkCreatePipelineLayout(device.device, &pPipelineLayoutCreateInfo, null, out pipeline.pipelineLayout));
		}


		void PreparePipelines(Pipeline pipeline, GraphicsDevice device, RenderPass renderPass, Material material)
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

            if (material.wireframe && device.DeviceFeatures.fillModeNonSolid == 1)
            {
	            rasterizationState.polygonMode = VkPolygonMode.Line;
	            rasterizationState.lineWidth = 1.0f;
            }

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
                material.shaderPair.GetVertPipeline(),
                material.shaderPair.GetFragPipeline());

            VkGraphicsPipelineCreateInfo pipelineCreateInfo =
                Initializers.pipelineCreateInfo(
                    pipeline.pipelineLayout,
                    renderPass.vkRenderPass,
                    0);

            var via = vertices_inputState;
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


            Util.CheckResult(vkCreateGraphicsPipelines(device.device, device.pipelineCache.vkPipelineCache, 1, &pipelineCreateInfo, null, out pipeline.vkPipeline));
        }


		protected virtual void SetupDescriptorPool(Pipeline pipeline, GraphicsDevice device)
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

			Util.CheckResult(vkCreateDescriptorPool(device.device, &descriptorPoolInfo, null, out pipeline.descriptorPool));
		}

		void SetupDescriptorSet(Pipeline pipeline, GraphicsDevice device, UniformBuffer uniformBuffer, Texture2D texture_colorMap)
		{
			var dsl = pipeline.descriptorSetLayout;
			VkDescriptorSetAllocateInfo allocInfo =
				Initializers.descriptorSetAllocateInfo(
					pipeline.descriptorPool,
					&dsl,
					1);

			Util.CheckResult(vkAllocateDescriptorSets(device.device, &allocInfo, out pipeline.descriptorSet));

			VkDescriptorImageInfo texDescriptor =
				Initializers.descriptorImageInfo(
					texture_colorMap.sampler,
					texture_colorMap.view,
					VkImageLayout.General);

			VkDescriptorBufferInfo temp = uniformBuffer.GetVkDescriptor();

			FixedArray2<VkWriteDescriptorSet> writeDescriptorSets = new FixedArray2<VkWriteDescriptorSet>(
				// Binding 0 : Vertex shader uniform buffer
				Initializers.writeDescriptorSet(
					pipeline.descriptorSet,
					VkDescriptorType.UniformBuffer,
					uniformBuffer.location,
					&temp),
				// Binding 1 : Color map 
				Initializers.writeDescriptorSet(
					pipeline.descriptorSet,
					VkDescriptorType.CombinedImageSampler,
					1,
					&texDescriptor));

			vkUpdateDescriptorSets(device.device, (writeDescriptorSets.Count), ref writeDescriptorSets.First, 0, null);
		}



		public void Dispose() {
			vertices_bindingDescriptions?.Dispose();
			vertices_attributeDescriptions?.Dispose();
		}
	}
}
