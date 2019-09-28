﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Core.ECS;
using Veldrid;
using System.Linq;
using System.Numerics;

namespace Core.Graphics
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

		public void OnEnableSystem(ECSWorld world)
		{
		}

		public void OnDisableSystem(ECSWorld world)
		{
		}

		private void UpdateRenderSystems(List<RenderSystemHolder> systemList, ECSWorld world, in RenderContext context) {
			foreach (RenderSystemHolder holder in systemList) {
				try {
					holder.system.Render(world, context);
				}catch(Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

		public void Update(float deltaTime, ECSWorld world)
		{
			if (!GraphicsContext.initialized) return;

			Camera camera = new Camera() {
				aspect = (float)Window.window.Width / (float)Window.window.Height,
				farPlane = 100,
				fow = 60,
				nearPlane = 0.1f
			};

			//Create context
			RenderContext context = new RenderContext();
			context.mainFrameBuffer = GraphicsContext._graphicsDevice.SwapchainFramebuffer;

			CommandList cmd = context.CreateCommandList();
			cmd.Begin();
			cmd.SetFramebuffer(context.mainFrameBuffer);
			cmd.ClearColorTarget(0, RgbaFloat.Black);

			cmd.UpdateBuffer(GraphicsContext._cameraProjViewBuffer, 0, 
				Matrix4x4.CreateLookAt(new Vector3(4, 10, -20),Vector3.Zero, Vector3.UnitY )
				);
			cmd.UpdateBuffer(GraphicsContext._cameraProjViewBuffer, 64, camera.ProjectionMatrix());

			cmd.End();
			context.SubmitCommands(cmd);

			//Update systems
			UpdateRenderSystems(beforeRenderSystems, world, context);
			UpdateRenderSystems(renderShadowsSystems, world, context);
			UpdateRenderSystems(afterRenderShadowsSystems, world, context);
			UpdateRenderSystems(renderOpaquesSystems, world, context);
			UpdateRenderSystems(afterRenderOpaquesSystems, world, context);
			UpdateRenderSystems(renderSkyboxSystems, world, context);
			UpdateRenderSystems(afterRenderSkyboxSystems, world, context);
			UpdateRenderSystems(renderTransparentsSystems, world, context);
			UpdateRenderSystems(afterRenderTransparentsSystems, world, context);
			UpdateRenderSystems(renderPostProcessingSystems, world, context);
			UpdateRenderSystems(afterRenderPostProcessingSystems, world, context);
			UpdateRenderSystems(renderUiSystems, world, context);
			UpdateRenderSystems(afterRenderUiSystems, world, context);
			UpdateRenderSystems(afterRenderSystems, world, context);

			GraphicsContext._graphicsDevice.WaitForIdle();
			GraphicsContext._graphicsDevice.SwapBuffers();
		}
	}
}