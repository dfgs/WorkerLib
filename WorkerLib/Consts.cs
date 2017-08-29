using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkerLib
{
	public enum LogActions { Enter, Quit };
	public enum WorkerStates { Stopping, Stopped, Starting, Started, Error, Inactive };
}
