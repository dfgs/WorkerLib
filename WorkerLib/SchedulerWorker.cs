using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LogUtils;

namespace WorkerLib
{

	public class SchedulerWorker<TypeOfValue>:QueueWorker<PriorityQueueItem<long, TypeOfValue>,TypeOfValue>
	{
		private PriorityQueue<long, TypeOfValue> priorityQueue;
	

	
		public SchedulerWorker(string Name, int StopTimeout=5000, params object[] Parameters)
			:base(Name,ThreadPriority.Normal,StopTimeout,Parameters)
		{
		}

		protected override void OnInitializeRessources( params object[] Parameters)
		{
			base.OnInitializeRessources(Parameters);
			WriteLog(LogLevels.Debug, "Create queue");
			priorityQueue = new PriorityQueue<long, TypeOfValue>(100);
		}

		protected override void OnDisposeRessources()
		{
			base.OnDisposeRessources();
			WriteLog(LogUtils.LogLevels.Debug, "Clear queue");
			priorityQueue.Dispose();
		}




		protected override int OnGetQueueLength()
		{
			return priorityQueue.Count;
		}

		protected override void OnClearQueue()
		{
			lock (priorityQueue)
			{
				priorityQueue.Clear();
			}
		}
		protected override PriorityQueueItem<long, TypeOfValue> OnDequeueItem()
		{
			PriorityQueueItem<long, TypeOfValue> item=null;
			long nowTick,eventTick;
			int waitTime;
			WaitHandle result;

			while (State == WorkerStates.Started)	// loop while queue is modified
			{
				WriteLog(LogLevels.Debug, "Lock priority queue and peek event's tick");
				lock (priorityQueue)
				{
					item = priorityQueue.Peek();
				}
					
				nowTick = DateTime.Now.Ticks;
				eventTick = item.Priority;
				waitTime = (int)((eventTick - nowTick)/10000);

				if (waitTime < 0)
				{
					WriteLog(LogLevels.Warning, "Event tick is onpast (delta is "+waitTime.ToString()+" ticks)");
					item = priorityQueue.Dequeue();
					break;
				}

				WriteLog(LogLevels.Debug, "Wait while " + waitTime.ToString() + " milliseconds before event");
				result = WaitHandles(waitTime, QuitEvent, QueueModifiedEvent);
				if (result == null)
				{
					item=priorityQueue.Dequeue();
					break;
				}
			}

			return item;
		}

		protected override void OnEnqueueItem(PriorityQueueItem<long, TypeOfValue> Item)
		{
			priorityQueue.Enqueue(Item);
		}

		public void Enqueue(long EventTick, TypeOfValue Value)
		{
			Enqueue(new PriorityQueueItem<long, TypeOfValue>(EventTick, Value));
		}

		/*protected override void OnInvokeEventDelegate(OnEventTriggeredDelegate<PriorityQueueItem<long, TypeOfValue>> EventDelegate, EventTriggeredEventArgs<PriorityQueueItem<long, TypeOfValue>> e)
		{
			EventDelegate.BeginInvoke(e, null, null);
		}*/


	}
}
