using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LogUtils;

namespace WorkerLib
{

	public class QueueProcessorWorker<TypeOfValue> : QueueWorker<QueueItem<TypeOfValue>,TypeOfValue>
	{
		private Queue<QueueItem<TypeOfValue>> queue;

		public QueueProcessorWorker(string Name,ThreadPriority Priority,int StopTimeout=5000, params object[] Parameters)
			: base(Name,Priority,StopTimeout, Parameters)
		{
		}

		protected override void OnInitializeRessources( params object[] Parameters)
		{
			base.OnInitializeRessources(Parameters);
			WriteLog(LogLevels.Debug, "Create queue");
			queue = new Queue<QueueItem<TypeOfValue>>(100);
		}

		protected override void OnDisposeRessources()
		{
			base.OnDisposeRessources();
			WriteLog(LogLevels.Debug, "Clear queue");
			queue.Clear();
		}

		protected override int OnGetQueueLength()
		{
			lock (queue)
			{
				return queue.Count;
			}
		}

		protected override void OnClearQueue()
		{
			lock (queue)
			{
				queue.Clear();
			}
		}
		protected override QueueItem<TypeOfValue> OnDequeueItem()
		{
			QueueItem<TypeOfValue> item;
			lock (queue)
			{
				item = queue.Dequeue();
			}
			return item;
		}

		protected override void OnEnqueueItem(QueueItem<TypeOfValue> Item)
		{
			lock (queue)
			{
				queue.Enqueue(Item);
			}
		}

		public void Enqueue(TypeOfValue Value)
		{
			lock (queue)
			{
				Enqueue(new QueueItem<TypeOfValue>(Value));
			}
		}

		public void Cancel(TypeOfValue Value)
		{

			if ((CurrentQueueItem.Value!=null) && (CurrentQueueItem.Value.Equals(Value))) Cancel(CurrentQueueItem);
			
			lock(queue)
			{
				foreach (QueueItem<TypeOfValue> item in queue)
				{
					if ((item.Value!=null) && (item.Value.Equals(Value))) Cancel(item);
				}
			}
		}

		/*protected override void OnInvokeEventDelegate(OnEventTriggeredDelegate<QueueItem<TypeOfValue>> EventDelegate, EventTriggeredEventArgs<QueueItem<TypeOfValue>> e)
		{
			EventDelegate.Invoke(e);
		}*/

	}
}
