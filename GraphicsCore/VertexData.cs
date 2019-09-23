﻿using System;
using System.Collections.Generic;
using System.Text;
using ECSCore.Numerics;
using Veldrid;

namespace Core.Graphics
{
	public struct VertexData {
		public Vector3 position;
		public Color color;

		public VertexData(Vector3 position, Color color) {
			this.position = position;
			this.color = color;
		}
	}
}
