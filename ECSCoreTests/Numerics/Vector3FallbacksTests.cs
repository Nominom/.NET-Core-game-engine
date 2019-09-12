using System;
using System.Collections.Generic;
using System.Text;
using ECSCore.Numerics;
using Xunit;

namespace ECSCoreTests.Numerics
{
	public class Vector3FallbacksTests {
		private const int arrLength = 10;

		Vector3[] vec1 = new Vector3[arrLength];
		Vector3[] vec2 = new Vector3[arrLength];
		float[] inputF = new float[arrLength];
		Vector3[] result = new Vector3[arrLength];
		float[] resultF = new float[arrLength];

		private void initializeArrays() {
			vec1 = new Vector3[arrLength];
			vec2 = new Vector3[arrLength];
			inputF = new float[arrLength];
			result = new Vector3[arrLength];
			resultF = new float[arrLength];

			for (int i = 0; i < arrLength; i++)
			{
				vec1[i] = new Vector3(i, i * 2, i * 3);
				vec2[i] = new Vector3(i * 4, i * 5, i * 6);
				inputF[i] = i;
			}
		}

		[Fact]
		public void Add()
		{
			initializeArrays();

			Vector3Fallbacks.Add(vec1, vec2, result);

			for (int i = 0; i < arrLength; i++) {
				Assert.True(result[i].ApproximatelyEquals(vec1[i] + vec2[i]));
			}
		}

		[Fact]
		public void Subtract()
		{
			initializeArrays();

			Vector3Fallbacks.Subtract(vec1, vec2, result);

			for (int i = 0; i < arrLength; i++)
			{
				Assert.True(result[i].ApproximatelyEquals(vec1[i] - vec2[i]));
			}
		}

		[Fact]
		public void Multiply()
		{
			initializeArrays();

			Vector3Fallbacks.Multiply(vec1, vec2, result);

			for (int i = 0; i < arrLength; i++)
			{
				Assert.True(result[i].ApproximatelyEquals(vec1[i] * vec2[i]));
			}
		}

		[Fact]
		public void MultiplyAllScalar()
		{
			initializeArrays();
			const float scalar = 5f;

			Vector3Fallbacks.MultiplyAllScalar(vec1, scalar, result);

			for (int i = 0; i < arrLength; i++)
			{
				Assert.True(result[i].ApproximatelyEquals(vec1[i] * scalar));
			}
		}

		[Fact]
		public void MultiplyScalar()
		{
			initializeArrays();
			const float scalar = 5f;

			Vector3Fallbacks.MultiplyScalar(vec1, inputF, result);

			for (int i = 0; i < arrLength; i++)
			{
				Assert.True(result[i].ApproximatelyEquals(vec1[i] * inputF[i]));
			}
		}

		[Fact]
		public void Normalize()
		{
			initializeArrays();

			Vector3Fallbacks.Normalize(vec1, result);

			for (int i = 0; i < arrLength; i++)
			{
				Assert.True(result[i].ApproximatelyEquals(vec1[i].Normalized()));
			}
		}

		[Fact]
		public void Dot()
		{
			initializeArrays();

			Vector3Fallbacks.Dot(vec1, vec2, resultF);

			for (int i = 0; i < arrLength; i++) {
				float actual = vec1[i].Dot(vec2[i]);
				Assert.InRange(resultF[i], actual - float.Epsilon, actual + float.Epsilon);
			}
		}

		[Fact]
		public void Cross()
		{
			initializeArrays();

			Vector3Fallbacks.Cross(vec1, vec2, result);

			for (int i = 0; i < arrLength; i++)
			{
				Assert.True(result[i].ApproximatelyEquals(vec1[i].Cross(vec2[i])));
			}
		}

		[Fact]
		public void Reflect()
		{
			initializeArrays();

			Vector3Fallbacks.Reflect(vec1, vec2, result);

			for (int i = 0; i < arrLength; i++)
			{
				Assert.True(result[i].ApproximatelyEquals(vec1[i].Reflect(vec2[i])));
			}
		}

		[Fact]
		public void Length()
		{
			initializeArrays();
			const float scalar = 5f;

			Vector3Fallbacks.Length(vec1, resultF);

			for (int i = 0; i < arrLength; i++) {
				float actual = vec1[i].Length();
				Assert.InRange(resultF[i], actual - float.Epsilon, actual + float.Epsilon);
			}
		}
	}
}
