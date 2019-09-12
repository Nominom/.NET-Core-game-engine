using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;

namespace ECSCoreBenchmarks
{
	public class NormalAsmConfig : ManualConfig
	{
		public NormalAsmConfig()
		{
			var summaryStyle = new SummaryStyle(false, SizeUnit.KB, TimeUnit.Nanosecond, true);

			this.SummaryStyle = summaryStyle;

			Add(
				DisassemblyDiagnoser.Create(
					new DisassemblyDiagnoserConfig(printAsm: true, printPrologAndEpilog: true, recursiveDepth: 3, printDiff: true)
				)
			);
			Add(
				HardwareCounter.BranchInstructions,
				//HardwareCounter.CacheMisses,
				HardwareCounter.BranchMispredictions
			);
			Add(
				Job
					.Default
					.With(Runtime.Core)
					.WithWarmupCount(2)
					.WithIterationCount(20)
					.WithIterationTime(TimeInterval.FromMilliseconds(100))
					.WithLaunchCount(1)
			);
		}
	}
}
