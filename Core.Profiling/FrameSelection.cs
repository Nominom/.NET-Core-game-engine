using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Profiling
{
	public enum FrameSelection
	{
		Median,
		Longest,
		Percentile95,
		Shortest,
		Latest,
		All
	}
}
