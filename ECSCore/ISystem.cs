using System;
using System.Collections.Generic;
using System.Text;

namespace ECSCore {
	public interface ISystem {
		bool Enabled { get; set; }
		void OnCreateSystem(ECSWorld world);
		void OnDestroySystem(ECSWorld world);
		void OnEnableSystem(ECSWorld world);
		void OnDisableSystem(ECSWorld world);
		void Update(float deltaTime, ECSWorld world);
	}

	public enum UpdateEvent {
		/// <summary>
		/// Called every frame before rendering.
		/// </summary>
		Update,

		/// <summary>
		/// Called every frame before Update.
		/// </summary>
		EarlyUpdate,

		/// <summary>
		/// Called every frame after Update.
		/// </summary>
		LateUpdate,

		/// <summary>
		/// Called every <see cref="SystemManager.fixedUpdateStep"/>.
		/// </summary>
		FixedUpdate,

		/// <summary>
		/// Called just before rendering.
		/// </summary>
		BeforeRender,

		/// <summary>
		/// Called every frame when rendering.
		/// </summary>
		Render,

		/// <summary>
		/// Called every frame after rendering has finished.
		/// </summary>
		AfterRender
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class ECSSystemAttribute : Attribute {
		public readonly UpdateEvent updateEvent = UpdateEvent.Update;
		public readonly System.Type updateBefore;
		public readonly System.Type updateAfter;

		public ECSSystemAttribute(UpdateEvent updateEvent = UpdateEvent.Update, System.Type updateBefore = null,
			System.Type updateAfter = null) {
			this.updateEvent = updateEvent;
			this.updateBefore = updateBefore;
			this.updateAfter = updateAfter;
		}
	}

}