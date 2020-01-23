using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ECS.Jobs
{
	public interface IJob {
		void DoJob();
	}
	public interface IShortJob : IJob { }
	public interface ILongJob : IJob { }
}
