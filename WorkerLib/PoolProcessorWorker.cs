using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LogUtils;

namespace WorkerLib
{

	public abstract class PoolProcessorWorker<TypeOfChildWorker,TypeOfValue> : QueueWorker<QueueItem<TypeOfValue>,TypeOfValue>
		where TypeOfChildWorker:RunWorker
	{
		public event EventHandler ChildWorkerTerminated;

		private int maxChildWorkerCount;
		public int MaxChildWorkerCount
		{
			get { return maxChildWorkerCount; }
		}

		public int ChildWorkerCount
		{
			get
			{
				lock(childWorkers)
				{
					return childWorkers.Count;
				}
			}
		}
		private List<TypeOfChildWorker> childWorkers;
		
		

		private Queue<QueueItem<TypeOfValue>> items;


		public PoolProcessorWorker(string Name,ThreadPriority ThreadPriority, int MaxChildWorkerCount)
			: base(Name, ThreadPriority,MaxChildWorkerCount)
		{
		
		}

		protected override void OnInitializeRessources(params object[] Parameters)
		{
			base.OnInitializeRessources(Parameters);

			this.maxChildWorkerCount = (int)Parameters[0];
			items = new Queue<QueueItem<TypeOfValue>>();

			childWorkers = new List<TypeOfChildWorker>();
	
		}

		protected override void OnDisposeRessources()
		{
			base.OnDisposeRessources();

			childWorkers = null;			
		}

			

		protected override bool OnStopProcessing()
		{
			bool result;

			result = true;
			foreach (TypeOfChildWorker childWorker in childWorkers)
			{
				result &= childWorker.Stop();
				childWorker.Dispose();
			}
			result &= base.OnStopProcessing();
			return result;
		}

		protected abstract TypeOfChildWorker OnCreateChildWorker(TypeOfValue Value);

		protected override void OnEventTriggered(EventTriggeredEventArgs<QueueItem<TypeOfValue>> e)
		{
			base.OnEventTriggered(e);
			TypeOfChildWorker childWorker;

			childWorker = OnCreateChildWorker(e.Item.Value);
			childWorker.Terminated += childWorker_Terminated;
			lock (childWorkers)
			{
				childWorkers.Add(childWorker);
			}
			childWorker.Start();
		}

		private void childWorker_Terminated(object sender, EventArgs e)
		{
			lock(childWorkers)
			{
				childWorkers.Remove((TypeOfChildWorker)sender);
			}
			if (ChildWorkerTerminated != null) ChildWorkerTerminated(this, EventArgs.Empty);
		}

		
		protected override int OnGetQueueLength()
		{
			lock (items)
			{
				return items.Count;
			}
		}

		protected override QueueItem<TypeOfValue> OnDequeueItem()
		{
			lock(items)
			{
				lock (childWorkers)
				{
					if (childWorkers.Count == maxChildWorkerCount) return null;
					else return items.Dequeue();
				}
			}
		}

		protected override void OnEnqueueItem(QueueItem<TypeOfValue> Item)
		{
			lock(items)
			{
				items.Enqueue(Item);
			}
		}

		protected override void OnClearQueue()
		{
			lock (items)
			{
				items.Clear();
			}
		}

		public void Enqueue(TypeOfValue Value)
		{
			Enqueue(new QueueItem<TypeOfValue>(Value));
		}
		public void Enqueue(IEnumerable<TypeOfValue> Values)
		{
			foreach (TypeOfValue value in Values)
			{
				Enqueue(new QueueItem<TypeOfValue>(value));
			}
		}


	}
}
