using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;

namespace CoreBenchmarks
{
	public class FullProfileConfig : ManualConfig
	{

		public FullProfileConfig()
		{
			var summaryStyle = new SummaryStyle(false, SizeUnit.KB, TimeUnit.Millisecond, true);

			this.SummaryStyle = summaryStyle;

			Add(
				DisassemblyDiagnoser.Create(
					new DisassemblyDiagnoserConfig(printAsm: true, printPrologAndEpilog: true, recursiveDepth: 3, printDiff: true)
				)
			);
			Add(MemoryDiagnoser.Default);
			Add(
				HardwareCounter.BranchInstructions,
				//HardwareCounter.CacheMisses,
				HardwareCounter.BranchMispredictions
			);
			Add(
				Job
					.Core
					.With(Runtime.Core)
					.WithWarmupCount(2)
					.WithIterationCount(10)
					.WithIterationTime(TimeInterval.FromMilliseconds(200))
					.WithLaunchCount(1)
					.With(new GcMode()
					{
						Force = false
					})
			);
		}
	}
}
