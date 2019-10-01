using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Core.Graphics
{
	internal static class BasicMeshConstructor
	{
		public static Mesh FullScreenQuad() {
			VertexData[] quadVertices =
			{
				new VertexData(new Vector3(-1f, 1f, 0), new Color(1, 0, 0)),
				new VertexData(new Vector3(1f, 1f, 0), new Color(0, 1, 0)),
				new VertexData(new Vector3(-1f, -1f, 0), new Color(0, 0, 1)),
				new VertexData(new Vector3(1f, -1f, 0), new Color(1, 1, 0))
			};

			ushort[] quadIndices = { 0, 1, 2, 2, 1, 3 };

			SubMesh mesh = new SubMesh(quadVertices, quadIndices);

			return new Mesh(mesh);
		}

		public static Mesh UnitCube()
		{
			VertexData[] cubeVertices =
			{
				new VertexData(new Vector3(-1.0f, -1.0f,  1.0f), new Color(1, 0,0)),
				new VertexData(new Vector3(1.0f, -1.0f,  1.0f), new Color(0, 1,0)),
				new VertexData(new Vector3(1.0f,  1.0f,  1.0f), new Color(0, 0,1)),
				new VertexData(new Vector3(-1.0f,  1.0f,  1.0f), new Color(1, 1,0)),

				new VertexData(new Vector3(-1.0f, -1.0f, -1.0f), new Color(1, 0,1)),
				new VertexData(new Vector3(1.0f, -1.0f, -1.0f), new Color(0, 1,1)),
				new VertexData(new Vector3(1.0f,  1.0f, -1.0f), new Color(1, 1,1)),
				new VertexData(new Vector3(-1.0f,  1.0f, -1.0f), new Color(0, 0,0))
			};

			/* //Counter clockwise winding
			ushort[] cubeIndices = { 
				//front
				0, 1, 2,
				2, 3, 0,
				// right
				1, 5, 6,
				6, 2, 1,
				// back
				7, 6, 5,
				5, 4, 7,
				// left
				4, 0, 3,
				3, 7, 4,
				// bottom
				4, 5, 1,
				1, 0, 4,
				// top
				3, 2, 6,
				6, 7, 3 };*/


			ushort[] cubeIndices = { 
				//front
				2, 1, 0,
				0, 3, 2,
				// right
				6, 5, 1,
				1, 2, 6,
				// back
				5, 6, 7,
				7, 4, 5,
				// left
				3, 0, 4,
				4, 7, 3,
				// bottom
				1, 5, 4,
				4, 0, 1,
				// top
				6, 2, 3,
				3, 7, 6 };

			SubMesh mesh = new SubMesh(cubeVertices, cubeIndices);

			return new Mesh(mesh);
		}
	}
}
