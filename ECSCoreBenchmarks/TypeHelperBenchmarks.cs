using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Core.ECS;

namespace CoreBenchmarks
{
	[Config(typeof(NormalAsmConfig))]
	public class TypeHelperBenchmarks
	{

		public class TestComponentThing : IComponent { }

		[Benchmark]
		public int ComponentMaskIndex() {
			return ComponentMask.GetComponentIndex<TestComponentThing>();
		}

		[Benchmark]
		public int ComponentMaskIndexByType()
		{
			return ComponentMask.GetComponentIndex(typeof(TestComponentThing));
		}

		[Benchmark]
		public int TypeHelperIndex() {
			return TypeHelper.Component<TestComponentThing>.componentIndex;
		}
	}
}
