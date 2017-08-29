using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerLib
{
	public class PriorityQueueItem<TypeOfPriority,TypeOfValue>:QueueItem<TypeOfValue>
		where TypeOfPriority : IComparable<TypeOfPriority>
	{
		
		private TypeOfPriority priority;
		public TypeOfPriority Priority
		{
			get { return priority; }
		}

		public PriorityQueueItem(TypeOfPriority Priority,TypeOfValue Value)
			:base(Value)
		{
			this.priority = Priority;
		}

		public override string ToString()
		{
			return "["+priority.ToString() + " - "+Value.ToString()+"]";
		}
	}

}
