using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Core.Shared;
using GlmSharp;

namespace Core.Graphics {
	internal static class BasicMeshConstructor {
		public static MeshData FullScreenQuad() {
			Vertex[] quadVertices = {
				new Vertex(new vec3(-1f, 1f, 0), new vec3(0, 0, 1), new vec2(0, 1)),
				new Vertex(new vec3(1f, 1f, 0), new vec3(0, 0, 1), new vec2(1, 1)),
				new Vertex(new vec3(-1f, -1f, 0), new vec3(0, 0, 1), new vec2(0, 0)),
				new Vertex(new vec3(1f, -1f, 0), new vec3(0, 0, 1), new vec2(1, 0))
			};

			UInt32[] quadIndices = {0, 1, 2, 2, 1, 3};

			SubMeshData subMesh = new SubMeshData(quadVertices, quadIndices);
			MeshData meshData = new MeshData();
			meshData.subMeshes = new[] {subMesh};

			return meshData;
		}

		public static MeshData UnitCube() {
			Vertex[] cubeVertices = {
				//front
				/*0-0*/new Vertex(new vec3(-0.5f, -0.5f, 0.5f), new vec3(0, 0, 1), new vec2(0, 1)),
				/*1-1*/new Vertex(new vec3(0.5f, -0.5f, 0.5f), new vec3(0, 0, 1), new vec2(0, 1)),
				/*2-2*/new Vertex(new vec3(0.5f, 0.5f, 0.5f), new vec3(0, 0, 1), new vec2(0, 1)),
				/*3-3*/new Vertex(new vec3(-0.5f, 0.5f, 0.5f), new vec3(0, 0, 1), new vec2(0, 1)),

				//right
				/*6-4*/new Vertex(new vec3(0.5f, 0.5f, -0.5f), new vec3(1, 0, 0), new vec2(0, 1)),
				/*5-5*/new Vertex(new vec3(0.5f, -0.5f, -0.5f), new vec3(1, 0, 0), new vec2(0, 1)),
				/*1-6*/new Vertex(new vec3(0.5f, -0.5f, 0.5f), new vec3(1, 0, 0), new vec2(0, 1)),
				/*2-7*/new Vertex(new vec3(0.5f, 0.5f, 0.5f), new vec3(1, 0, 0), new vec2(0, 1)),
				
				//back
				/*5-8*/new Vertex(new vec3(0.5f, -0.5f, -0.5f), new vec3(0, 0, -1), new vec2(0, 1)),
				/*6-9*/new Vertex(new vec3(0.5f, 0.5f, -0.5f), new vec3(0, 0, -1), new vec2(0, 1)),
				/*7-10*/new Vertex(new vec3(-0.5f, 0.5f, -0.5f), new vec3(0, 0, -1), new vec2(0, 1)),
				/*4-11*/new Vertex(new vec3(-0.5f, -0.5f, -0.5f), new vec3(0, 0, -1), new vec2(0, 1)),
				
				//left
				/*3-12*/new Vertex(new vec3(-0.5f, 0.5f, 0.5f), new vec3(-1, 0, 0), new vec2(0, 1)),
				/*0-13*/new Vertex(new vec3(-0.5f, -0.5f, 0.5f), new vec3(-1, 0, 0), new vec2(0, 1)),
				/*4-14*/new Vertex(new vec3(-0.5f, -0.5f, -0.5f), new vec3(-1, 0, 0), new vec2(0, 1)),
				/*7-15*/new Vertex(new vec3(-0.5f, 0.5f, -0.5f), new vec3(-1, 0, 0), new vec2(0, 1)),
				
				//bottom
				/*1-16*/new Vertex(new vec3(0.5f, -0.5f, 0.5f), new vec3(0, -1, 0), new vec2(0, 1)),
				/*5-17*/new Vertex(new vec3(0.5f, -0.5f, -0.5f), new vec3(0, -1, 0), new vec2(0, 1)),
				/*4-18*/new Vertex(new vec3(-0.5f, -0.5f, -0.5f), new vec3(0, -1, 0), new vec2(0, 1)),
				/*0-19*/new Vertex(new vec3(-0.5f, -0.5f, 0.5f), new vec3(0, -1, 0), new vec2(0, 1)),
				
				//top
				/*6-20*/new Vertex(new vec3(0.5f, 0.5f, -0.5f), new vec3(0, 1, 0), new vec2(0, 1)),
				/*2-21*/new Vertex(new vec3(0.5f, 0.5f, 0.5f), new vec3(0, 1, 0), new vec2(0, 1)),
				/*3-22*/new Vertex(new vec3(-0.5f, 0.5f, 0.5f), new vec3(0, 1, 0), new vec2(0, 1)),
				/*7-23*/new Vertex(new vec3(-0.5f, 0.5f, -0.5f), new vec3(0, 1, 0), new vec2(0, 1)),
				
			};

			UInt32[] cubeIndices = {
				//front
				2, 1, 0,
				0, 3, 2,
				// right
				4, 5, 6,
				6, 7, 4,
				// back
				8, 9, 10,
				10, 11, 8,
				// left
				12, 13, 14,
				14, 15, 12,
				// bottom
				16, 17, 18,
				18, 19, 16,
				// top
				20, 21, 22,
				22, 23, 20
			};

			//Vertex[] cubeVertices = {
			//	/*0*/ new Vertex(new vec3(-1.0f, -1.0f, 1.0f), new vec3(0, 0, 0), new vec2(0, 1)),
			//	/*1*/ new Vertex(new vec3(1.0f, -1.0f, 1.0f), new vec3(0, 0, 0), new vec2(0, 1)),
			//	/*2*/ new Vertex(new vec3(1.0f, 1.0f, 1.0f), new vec3(0, 0, 0), new vec2(0, 1)),
			//	/*3*/ new Vertex(new vec3(-1.0f, 1.0f, 1.0f), new vec3(0, 0, 0), new vec2(0, 1)),

			//	/*4*/ new Vertex(new vec3(-1.0f, -1.0f, -1.0f), new vec3(0, 0, 0), new vec2(0, 1)),
			//	/*5*/ new Vertex(new vec3(1.0f, -1.0f, -1.0f), new vec3(0, 0, 0), new vec2(0, 1)),
			//	/*6*/ new Vertex(new vec3(1.0f, 1.0f, -1.0f), new vec3(0, 0, 0), new vec2(0, 1)),
			//	/*7*/ new Vertex(new vec3(-1.0f, 1.0f, -1.0f), new vec3(0, 0, 0), new vec2(0, 1))
			//};

			//ushort[] cubeIndices = {
			//	//front
			//	2, 1, 0,
			//	0, 3, 2,
			//	// right
			//	6, 5, 1,
			//	1, 2, 6,
			//	// back
			//	5, 6, 7,
			//	7, 4, 5,
			//	// left
			//	3, 0, 4,
			//	4, 7, 3,
			//	// bottom
			//	1, 5, 4,
			//	4, 0, 1,
			//	// top
			//	6, 2, 3,
			//	3, 7, 6
			//};

			/*
			//Counter clockwise winding
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

			
			

			SubMeshData subMesh = new SubMeshData(cubeVertices, cubeIndices);
			MeshData meshData = new MeshData();
			meshData.subMeshes = new[] {subMesh};

			return meshData;
		}
	}
}
