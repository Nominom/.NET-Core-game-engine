using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using ECSCore;

namespace ECSCoreBenchmarks {
	[Config(typeof(Config))]

	public class DictionaryBenchmarks {
		private class Config : ManualConfig {

			public Config () {
				Add(MemoryDiagnoser.Default);
				Add(
					Job
						.Default
						.WithWarmupCount(2)
						.WithIterationCount(25)
						.WithIterationTime(TimeInterval.FromMilliseconds(100))
						.With(new GcMode() {
							Force = false
						})
						.WithLaunchCount(1)
				);
			}
		}

		private System.Type[] types = new Type[] {
			typeof(BlockAllocator),
			typeof(ComponentManager),
			typeof(ComponentMemoryBlock),
			typeof(Entity),
			typeof(ArrayPool<Entity>),
			typeof(ISharedComponent),
			typeof(ISystem),
			typeof(EntityManager),
			typeof(EntityArchetype),
			typeof(byte),
			typeof(int),
			typeof(double),
			typeof(ComponentQuery)
		};

		private Dictionary<System.Type, int> normalDictionary;
		private Dictionary<int, int> intDictionary;

	
		public DictionaryBenchmarks () {
			normalDictionary = new Dictionary<System.Type, int>();
			intDictionary = new Dictionary<int, int>();


			foreach (var type in types) {
				normalDictionary.Add(type, type.GetHashCode());
				intDictionary.Add(type.GetHashCode(), type.GetHashCode());
			}
		}


		[Params(10, 100, 1_000, 10_000, 100_000)]
		public int IterCount { get; set; }

		[Benchmark]
		public int IntDictionaryAdd () {
			Dictionary<int, int> intDict = new Dictionary<int, int>();

			for (int i = 0; i < IterCount; i++) {
				intDict.TryAdd(TypeHelper<BlockAllocator>.hashCode, 0);
				intDict.TryAdd(TypeHelper<ComponentManager>.hashCode, 0);
				intDict.TryAdd(TypeHelper<ComponentMemoryBlock>.hashCode, 0);
				intDict.TryAdd(TypeHelper<Entity>.hashCode, 0);
				intDict.TryAdd(TypeHelper<ArrayPool<Entity>>.hashCode, 0);
				intDict.TryAdd(TypeHelper<ISharedComponent>.hashCode, 0);
				intDict.TryAdd(TypeHelper<ISystem>.hashCode, 0);
				intDict.TryAdd(TypeHelper<EntityManager>.hashCode, 0);
				intDict.TryAdd(TypeHelper<EntityArchetype>.hashCode, 0);
				intDict.TryAdd(TypeHelper<byte>.hashCode, 0);
				intDict.TryAdd(TypeHelper<int>.hashCode, 0);
				intDict.TryAdd(TypeHelper<double>.hashCode, 0);
			}

			return intDict.Count;
		}

		[Benchmark]
		public int DictionaryAdd () {
			Dictionary<System.Type, int> dict = new Dictionary<System.Type, int>();

			for (int i = 0; i < IterCount; i++) {
				dict.TryAdd(typeof(BlockAllocator), 0);
				dict.TryAdd(typeof(ComponentManager), 0);
				dict.TryAdd(typeof(ComponentMemoryBlock), 0);
				dict.TryAdd(typeof(Entity), 0);
				dict.TryAdd(typeof(ArrayPool<Entity>), 0);
				dict.TryAdd(typeof(ISharedComponent), 0);
				dict.TryAdd(typeof(ISystem), 0);
				dict.TryAdd(typeof(EntityManager), 0);
				dict.TryAdd(typeof(EntityArchetype), 0);
				dict.TryAdd(typeof(byte), 0);
				dict.TryAdd(typeof(int), 0);
				dict.TryAdd(typeof(double), 0);
			}

			return dict.Count;
		}

		[Benchmark]
		public int IntDictionaryGet () {
			int v = 0;
			for (int i = 0; i < IterCount; i++) {
				intDictionary.TryGetValue(TypeHelper<BlockAllocator>.hashCode, out v);
				intDictionary.TryGetValue(TypeHelper<ComponentManager>.hashCode, out v);
				intDictionary.TryGetValue(TypeHelper<ComponentMemoryBlock>.hashCode, out v);
				intDictionary.TryGetValue(TypeHelper<Entity>.hashCode, out v);
				intDictionary.TryGetValue(TypeHelper<ArrayPool<Entity>>.hashCode, out v);
				intDictionary.TryGetValue(TypeHelper<ISharedComponent>.hashCode, out v);
				intDictionary.TryGetValue(TypeHelper<ISystem>.hashCode, out v);
				intDictionary.TryGetValue(TypeHelper<EntityManager>.hashCode, out v);
				intDictionary.TryGetValue(TypeHelper<EntityArchetype>.hashCode, out v);
				intDictionary.TryGetValue(TypeHelper<byte>.hashCode, out v);
				intDictionary.TryGetValue(TypeHelper<int>.hashCode, out v);
				intDictionary.TryGetValue(TypeHelper<double>.hashCode, out v);
			}

			return v;
		}

		[Benchmark]
		public int DictionaryGet () {
			int v = 0;
			for (int i = 0; i < IterCount; i++) {
				normalDictionary.TryGetValue(typeof(BlockAllocator), out v);
				normalDictionary.TryGetValue(typeof(ComponentManager), out v);
				normalDictionary.TryGetValue(typeof(ComponentMemoryBlock), out v);
				normalDictionary.TryGetValue(typeof(Entity), out v);
				normalDictionary.TryGetValue(typeof(ArrayPool<Entity>), out v);
				normalDictionary.TryGetValue(typeof(ISharedComponent), out v);
				normalDictionary.TryGetValue(typeof(ISystem), out v);
				normalDictionary.TryGetValue(typeof(EntityManager), out v);
				normalDictionary.TryGetValue(typeof(EntityArchetype), out v);
				normalDictionary.TryGetValue(typeof(byte), out v);
				normalDictionary.TryGetValue(typeof(int), out v);
				normalDictionary.TryGetValue(typeof(double), out v);
			}

			return v;
		}

	}

}
