using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Core.ECS;
using Core.ECS.Components;
using Core.Graphics.VulkanBackend;
using Core.Profiling;
using Core.Shared;
using GlmSharp;
using Vulkan;

namespace Core.Graphics.RenderSystems
{
	public enum RenderStage
	{
		BeforeRender,
		RenderShadows,
		AfterRenderShadows,
		RenderOpaques,
		AfterRenderOpaques,
		RenderSkybox,
		AfterRenderSkybox,
		RenderTransparents,
		AfterRenderTransparents,
		RenderPostProcessing,
		AfterRenderPostProcessing,
		RenderUi,
		AfterRenderUi,
		AfterRender
	}

	struct RenderSystemHolder
	{
		public IRenderSystem system;
		public System.Type systemType;
		public System.Type renderBefore;
		public System.Type renderAfter;
	}



	[ECSSystem(UpdateEvent.Render)]
	public class RenderManagerSystem : ISystem
	{
		public bool Enabled { get; set; }

		private readonly Dictionary<System.Type, IRenderSystem> systems = new Dictionary<Type, IRenderSystem>();

		private readonly List<RenderSystemHolder> beforeRenderSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> renderShadowsSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> afterRenderShadowsSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> renderOpaquesSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> afterRenderOpaquesSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> renderSkyboxSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> afterRenderSkyboxSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> renderTransparentsSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> afterRenderTransparentsSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> renderPostProcessingSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> afterRenderPostProcessingSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> renderUiSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> afterRenderUiSystems = new List<RenderSystemHolder>();
		private readonly List<RenderSystemHolder> afterRenderSystems = new List<RenderSystemHolder>();

		private List<CommandBuffer> buffersToDispose = new List<CommandBuffer>();
		private List<CommandBuffer> secondaryBuffers = new List<CommandBuffer>();

		//public static Camera camera;
		//public static Vector3 cameraPosition;
		//public static Quaternion cameraRotation;
		private ComponentQuery cameraQuery;

		private void SortSystemList(List<RenderSystemHolder> list)
		{
			list.Sort((a, b) =>
			{
				if (a.renderBefore == b.systemType || b.renderAfter == a.systemType)
				{
					return -1;
				}

				if (a.renderAfter == b.systemType || b.renderBefore == a.systemType)
				{
					return 1;
				}

				return 0;
			});
		}

		private void RegisterSystemTo(List<RenderSystemHolder> list, IRenderSystem system, System.Type systemType, System.Type updateBefore, System.Type updateAfter)
		{
			if (list == null)
			{
				Console.WriteLine("List was null while registering a system.");
				return;
			}

			if (systems.ContainsKey(systemType))
			{
				Console.WriteLine($"System {systemType.Name} is already registered!");
				return;
			}

			systems.Add(systemType, system);

			list.Add(new RenderSystemHolder()
			{
				system = system,
				systemType = systemType,
				renderBefore = updateBefore,
				renderAfter = updateAfter
			});

			SortSystemList(list);
		}


		public void RegisterRenderSystem(IRenderSystem system, System.Type type, RenderStage renderStage, System.Type renderBefore = null,
			System.Type renderAfter = null) {

			switch (renderStage) {
				case RenderStage.BeforeRender:
					RegisterSystemTo(beforeRenderSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.RenderShadows:
					RegisterSystemTo(renderShadowsSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.AfterRenderShadows:
					RegisterSystemTo(afterRenderShadowsSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.RenderOpaques:
					RegisterSystemTo(renderOpaquesSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.AfterRenderOpaques:
					RegisterSystemTo(afterRenderOpaquesSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.RenderSkybox:
					RegisterSystemTo(renderSkyboxSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.AfterRenderSkybox:
					RegisterSystemTo(afterRenderSkyboxSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.RenderTransparents:
					RegisterSystemTo(renderTransparentsSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.AfterRenderTransparents:
					RegisterSystemTo(afterRenderTransparentsSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.RenderPostProcessing:
					RegisterSystemTo(renderPostProcessingSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.AfterRenderPostProcessing:
					RegisterSystemTo(afterRenderPostProcessingSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.RenderUi:
					RegisterSystemTo(renderUiSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.AfterRenderUi:
					RegisterSystemTo(afterRenderUiSystems, system, type, renderBefore, renderAfter);
					break;
				case RenderStage.AfterRender:
					RegisterSystemTo(afterRenderSystems, system, type, renderBefore, renderAfter);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(renderStage), renderStage, null);
			}
		}
		
		public void OnCreateSystem(ECSWorld world)
		{
			//camera = new Camera() {
			//	aspect = (float)Window.window.Width / (float)Window.window.Height,
			//	farPlane = 10000,
			//	fow = 60,
			//	nearPlane = 0.1f
			//};

			//cameraPosition = new Vector3(4 , 10, 20);
			//cameraRotation = MathHelper.LookAt(cameraPosition, Vector3.Zero, Vector3.UnitY);
			cameraQuery = new ComponentQuery();
			cameraQuery.IncludeShared<Camera>();
			cameraQuery.IncludeReadonly<Position>();
			cameraQuery.IncludeReadonly<Rotation>();

			foreach (var assembly in AssemblyHelper.GetAllUserAssemblies())
			{

				foreach (Type type in AssemblyHelper.GetTypesWithAttribute(assembly, typeof(RenderSystemAttribute)))
				{
					try
					{
						if (!type.GetInterfaces().Contains(typeof(IRenderSystem)))
						{
							continue;
						}

						RenderSystemAttribute attribute =
							type.GetCustomAttribute(typeof(RenderSystemAttribute)) as RenderSystemAttribute;
						IRenderSystem instance = Activator.CreateInstance(type) as IRenderSystem;

						RegisterRenderSystem(instance, type, attribute.renderStage, attribute.renderBefore,
							attribute.renderAfter);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"could not create an instance of type {type.Name}. Does it have an empty constructor? \n{ex.StackTrace}");
					}
				}
			}

			foreach (var renderSystem in systems)
			{
				renderSystem.Value.OnCreate(world);
			}
		}

		public void OnDestroySystem(ECSWorld world)
		{
			foreach (var renderSystem in systems) {
				renderSystem.Value.OnDestroy(world);
			}
		}

		private void UpdateRenderSystems(List<RenderSystemHolder> systemList, ECSWorld world, in RenderContext context) {
			foreach (RenderSystemHolder holder in systemList) {
				Profiler.StartMethod(holder.systemType.Name);
				try {
					holder.system.Render(world, context);
				}
				catch (Exception ex) {
					Console.WriteLine(ex);
				}
				finally {
					Profiler.EndMethod();
				}
			}
		}

		private float timer = 0;
		public void Update(float deltaTime, ECSWorld world)
		{
			if (!GraphicsContext.initialized) return;

			//Dispose commandbuffers from last frame before new frame
			Profiler.StartMethod("WaitDeviceIdle");
			GraphicsContext.graphicsDevice.WaitIdle();
			foreach (var buffer in buffersToDispose) {
				buffer.Dispose();
			}
			buffersToDispose.Clear();
			Profiler.EndMethod();

			//var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, deltaTime * 0.01f);
			//cameraRotation = Quaternion.Multiply(cameraRotation, rotation);
			
			timer += deltaTime;

			vec3 lightDir = new vec3(MathF.Cos(timer * 1), -0.3f, MathF.Sin(timer * 1)).Normalized;


			Camera camera = null;
			vec3 cameraPosition = vec3.Zero;
			quat cameraRotation = quat.Identity;

			var cameraBlock = world.ComponentManager.GetBlocks(cameraQuery);
			foreach (var accessor in cameraBlock) {
				camera = accessor.GetSharedComponentData<Camera>();
				cameraPosition = accessor.GetReadOnlyComponentData<Position>()[0].value;
				cameraRotation = accessor.GetReadOnlyComponentData<Rotation>()[0].value;
			}

			if(camera == null)
			{
				return;
			}

			
			Profiler.StartMethod("StartRender");
			GraphicsContext.graphicsDevice.StartFrame();
			Profiler.EndMethod();

			Profiler.StartMethod("SetUniformsAndRenderContext");
			UniformBufferObject ubo = new UniformBufferObject();
			ubo.cameraPos = new vec4(cameraPosition);
			ubo.lightDir = new vec4(lightDir, 0);
			ubo.projection = camera.ProjectionMatrixVulkanCorrected();
			ubo.view = camera.ViewMatrix(cameraPosition, cameraRotation);

			//Create context
			RenderContext context = new RenderContext();
			context.currentFrameBuffer = GraphicsContext.graphicsDevice.GetCurrentFrameBuffer();
			context.currentRenderPass = GraphicsContext.graphicsDevice.singlePass;
			context.currentSubPassIndex = 0;
			context.activeCamera = camera;
			context.cameraPosition = cameraPosition;
			context.cameraRotation = cameraRotation;
			context.secondaryBuffers = secondaryBuffers;
			context.ubo = ubo;

			context.SetUniformNow(ubo);
			Profiler.EndMethod();

			Profiler.StartMethod("RentPrimaryCmd");
			CommandBuffer primaryCmd =
				GraphicsContext.graphicsDevice.GetCommandPool().Rent(VkCommandBufferLevel.Primary);
			Profiler.EndMethod();

			context.frameCommands = primaryCmd;

			Profiler.StartMethod("BeginPrimaryCmd");
			primaryCmd.Begin();
			Profiler.EndMethod();

			Profiler.StartMethod("BeginRenderPass");
			//TODO: Clear color doesn't work
			primaryCmd.BeginRenderPassClearColorDepth(context.currentRenderPass, context.currentFrameBuffer,
				new VkClearColorValue(0, 40, 40),
				new VkClearDepthStencilValue(1, 0), 
				true);
			Profiler.EndMethod();
			

			//Update systems
			Profiler.StartMethod("BeforeRenderSystems");
			UpdateRenderSystems(beforeRenderSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("RenderShadowsSystems");
			UpdateRenderSystems(renderShadowsSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("AfterRenderShadowsSystems");
			UpdateRenderSystems(afterRenderShadowsSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("RenderOpaquesSystems");
			UpdateRenderSystems(renderOpaquesSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("AfterRenderOpaquesSystems");
			UpdateRenderSystems(afterRenderOpaquesSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("RenderSkyboxSystems");
			UpdateRenderSystems(renderSkyboxSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("AfterRenderSkyboxSystems");
			UpdateRenderSystems(afterRenderSkyboxSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("RenderTransparentsSystems");
			UpdateRenderSystems(renderTransparentsSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("AfterRenderTransparentsSystems");
			UpdateRenderSystems(afterRenderTransparentsSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("RenderPostProcessingSystems");
			UpdateRenderSystems(renderPostProcessingSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("AfterRenderPostProcessingSystems");
			UpdateRenderSystems(afterRenderPostProcessingSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("RenderUiSystems");
			UpdateRenderSystems(renderUiSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("AfterRenderUiSystems");
			UpdateRenderSystems(afterRenderUiSystems, world, context);
			Profiler.EndMethod();
			Profiler.StartMethod("AfterRenderSystems");
			UpdateRenderSystems(afterRenderSystems, world, context);
			Profiler.EndMethod();

			foreach (var buffer in context.secondaryBuffers) {
				primaryCmd.ExecuteSecondaryBuffer(buffer);
			}

			primaryCmd.End();
			
			Profiler.StartMethod("SubmitFrame");
			GraphicsContext.graphicsDevice.SubmitAndFinalizeFrame(primaryCmd);
			Profiler.EndMethod();
			//GraphicsContext.graphicsDevice.WaitIdle();
			foreach (CommandBuffer buffer in context.secondaryBuffers) {
				buffersToDispose.Add(buffer);
			}
			buffersToDispose.Add(primaryCmd);
			secondaryBuffers.Clear();
		}
	}
}
