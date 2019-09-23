using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using CoreBenchmarks;
using ECSCore;


namespace ECSCoreBenchmarks {
	class Program {

		public struct TestComponentWithInt : IComponent {
			public int someInt;
		}

		static void TestArr(int len) {
			using (PooledArray<int> arr = PooledArray<int>.Rent(len)) {
				//Console.WriteLine("Asked for: " + len);
				int i = arr.array.Length;
				//Console.WriteLine("Got: " + arr.array.Length);
			}
		}

		static void Main (string[] args) {
			/*
			Stopwatch sw = new Stopwatch();
			sw.Start();
			for (int i = 0; i < 10000000; i++) {
				TestArr(10);
				TestArr(20);
				TestArr(100);
				TestArr(1000);
				TestArr(10000);
				TestArr(100000);
				TestArr(1000000);
				TestArr(10000000);
			}
			

			sw.Stop();
			Console.WriteLine(sw.ElapsedMilliseconds + "ms");*/

			Console.WriteLine("SSE supported: " + Sse.IsSupported);
			Console.WriteLine("SSE2 supported: " + Sse2.IsSupported);
			Console.WriteLine("AVX supported: " + Avx.IsSupported);
			Console.WriteLine("AVX2 supported: " + Avx2.IsSupported);

			BenchmarkRunner.Run<Matrix4x4TransposeBenchmarks>();

			/*
			Stopwatch sw = new Stopwatch();
			ComponentManager cm = new ComponentManager();
			EntityManager em = new EntityManager(cm);

			for (int j = 0; j < 100; j++) {
				sw.Restart();
				for (int i = 0; i < 100_000; i++) {
					Entity e = em.CreateEntity();
					cm.AddComponent(e, new TestComponentWithInt { someInt = 10 });
					TestComponentWithInt test = cm.GetComponent<TestComponentWithInt>(e);
					cm.GetComponent<TestComponentWithInt>(e).someInt = 12;

					em.DestroyEntity(e);
				}

				sw.Stop();

				Console.WriteLine(sw.ElapsedMilliseconds + "ms");
			}
			*/

			//BenchmarkRunner.Run<ComponentBenchmarks>();
			/*
			Stopwatch watch = new Stopwatch();
			EntityArchetype archetype = new EntityArchetype().Add<TestComponent>();

			using (var block = new ComponentMemoryBlock(archetype)) {
				Span<TestComponent> span = block.GetComponentData<TestComponent>();
				Console.WriteLine("Length: " + span.Length);


				for (int i = 0; i < span.Length; i++) {
					span[i].x = i;
				}
				Span<TestComponent> span2 = block.GetComponentData<TestComponent>();
				for (int i = 0; i < span2.Length; i++) {
					if (span2[i].x != i) {
						Console.WriteLine("eror: "+i);
					}
				}
			}

			watch.Start();
			List<Task> tasks = new List<Task>();
			for (int t = 0; t < 10; t++) {
				Task t2 = Task.Run(() => {
					using (var block = new ComponentMemoryBlock(archetype)) {
						for (int i = 0; i < 1_000_000; i++) {
							Span<TestComponent> span = block.GetComponentData<TestComponent>();

							for (int j = 0; j < span.Length; j++) {
								span[j].x = i;
								span[j].y = j;
								span[j].z = span[j].x + span[j].y;
							}

							Span<TestComponent> span2 = block.GetComponentData<TestComponent>();

							for (int j = 0; j < span2.Length; j++) {
								int a = span2[j].z;
							}
						}
					}
				});
				tasks.Add(t2);
			}

			Task.WaitAll(tasks.ToArray());

			watch.Stop();
			Console.WriteLine(watch.ElapsedMilliseconds + "ms");




			Dictionary<Type, int> dict1 = new Dictionary<Type, int>();
			dict1.Add(typeof(TestComponent), 100);

			watch.Restart();
			for (int i = 0; i < 10_000_000; i++) {
				Type type = TypeHelper<TestComponent>.type;
				int val = dict1[type];
			}
			watch.Stop();
			Console.WriteLine(watch.ElapsedMilliseconds + "ms");

			Dictionary<int, int> dict2 = new Dictionary<int, int>();
			dict2.Add(TypeHelper<TestComponent>.hashCode, 100);

			watch.Restart();
			for (int i = 0; i < 10_000_000; i++) {
				int hash = TypeHelper<TestComponent>.hashCode;
				int val = dict2[hash];
				//int val = dict2[hash];
			}
			watch.Stop();
			Console.WriteLine(watch.ElapsedMilliseconds + "ms");
			*/

			Console.ReadKey();
		}
	}
}
