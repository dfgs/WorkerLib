using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LogUtils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WorkerLib
{
	public abstract class AsyncWorker:ThreadWorker
	{
		
		

		public AsyncWorker(string Name,ThreadPriority Priority, int StopTimeout=5000, params object[] Parameters)
			: base(Name, Priority, StopTimeout, Parameters)
		{
			
		}

		protected sealed override void ThreadLoop()
		{
			TaskLoopAsync().Wait();
		}

		protected abstract Task TaskLoopAsync();



	}
}
