using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Core;

namespace TestApp
{
	public class Program
	{
		static void Main(string[] args)
		{
			CoreEngine.Initialize();
			CoreEngine.Run();
		}
	}
}
