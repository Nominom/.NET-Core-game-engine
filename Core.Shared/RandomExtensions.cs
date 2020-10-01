using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;

namespace Core.Shared
{
	public static class RandomExtensions
	{
		public static float Range(this Random random, float min, float max)
		{
			return min + (float)random.NextDouble() * (max - min);
		}

		public static vec3 RandomInSphere(this Random random, float radius)
		{
			var r = (float)random.NextDouble() * radius;
			var polar = (float)random.NextDouble() * MathF.PI * 2;
			var alpha = (float)random.NextDouble() * MathF.PI * 2;
			var x = r * MathF.Sin(polar) * MathF.Cos(alpha);
			var y = r * MathF.Sin(polar) * MathF.Sin(alpha);
			var z = r * MathF.Cos(polar);
			return new vec3(x, y, z);
		}

		public static vec3 RandomOnSphere(this Random random, float radius)
		{
			var polar = (float)random.NextDouble() * MathF.PI * 2;
			var alpha = (float)random.NextDouble() * MathF.PI * 2;
			var x = radius * MathF.Sin(polar) * MathF.Cos(alpha);
			var y = radius * MathF.Sin(polar) * MathF.Sin(alpha);
			var z = radius * MathF.Cos(polar);
			return new vec3(x, y, z);
		}

		public static vec3 RandomInCircle(this Random random, float radius)
		{
			var r = (float)random.NextDouble() * radius;
			var alpha = (float)random.NextDouble() * MathF.PI * 2;
			var x = r * MathF.Cos(alpha);
			var y = r * MathF.Sin(alpha);
			return new vec3(x, y, 0);
		}

		public static vec3 RandomOnCircle(this Random random, float radius)
		{
			var alpha = (float)random.NextDouble() * MathF.PI * 2;
			var x = radius * MathF.Cos(alpha);
			var y = radius * MathF.Sin(alpha);
			return new vec3(x, y, 0);
		}
	}
}
