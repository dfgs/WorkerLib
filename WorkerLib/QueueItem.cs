using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkerLib
{
	public class QueueItem<TypeOfValue>:IDisposable
	{
		private bool isCanceled;
		public bool IsCanceled
		{
			get { return isCanceled; }
		}

		private TypeOfValue value;
		public TypeOfValue Value
		{
			get { return value; }
		}

		public QueueItem(TypeOfValue Value)
		{
			this.value = Value;
		}

		public void Cancel()
		{
			this.isCanceled = true;
		}

		public virtual void Dispose()
		{
		}

	
	}
}
