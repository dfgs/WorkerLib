using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LogUtils;

namespace WorkerLib
{
	//public delegate void OnEventTriggeredDelegate<TypeOfQueueItem>(EventTriggeredEventArgs<TypeOfQueueItem> e);

	public abstract class QueueWorker<TypeOfQueueItem,TypeOfValue> : ThreadWorker
		where TypeOfQueueItem:QueueItem<TypeOfValue>
	{
		public event EventHandler QueueChanged;

		private AutoResetEvent queueModifiedEvent;
		protected AutoResetEvent QueueModifiedEvent
		{
			get { return queueModifiedEvent; }
		}

		private TypeOfQueueItem currentQueueItem;
		protected TypeOfQueueItem CurrentQueueItem
		{
			get { return currentQueueItem; }
		}


		public event EventTriggeredHandler<TypeOfQueueItem> EventTriggered;

		private object queueLock;

		public int EventCount
		{
			get { return OnGetQueueLength(); }
		}


		public QueueWorker(string Name,ThreadPriority Priority,int StopTimeout=5000, params object[] Parameters)
			: base(Name, Priority,StopTimeout,Parameters)
		{
		}

		protected override void OnInitializeRessources(params object[] Parameters)
		{
			base.OnInitializeRessources(Parameters);

			queueLock = new List<string>();		// dummy lock
			WriteLog(LogUtils.LogLevels.Debug, "Create Queue Modified Event");
			queueModifiedEvent = new AutoResetEvent(false);
		}

		protected override void OnDisposeRessources()
		{
			base.OnDisposeRessources();
			WriteLog(LogUtils.LogLevels.Debug, "Close Queue Modified Event");
			queueModifiedEvent.Close();
		}


		protected virtual void OnEventTriggered(EventTriggeredEventArgs<TypeOfQueueItem> e)
		{
			if (EventTriggered != null)
			{
				WriteLog(LogLevels.Debug, "Trigger event");
				EventTriggered(this, e);
				//EventTriggered.BeginInvoke(this, e, null, null);
			}
		}

		

		protected abstract int OnGetQueueLength();

		protected abstract TypeOfQueueItem OnDequeueItem();

		protected abstract void OnEnqueueItem(TypeOfQueueItem Item);

		protected abstract void OnClearQueue();

		protected virtual void OnQueueChanged()
		{
			if (QueueChanged != null) QueueChanged(this, EventArgs.Empty);
		}

		public virtual void Clear()
		{
			lock (queueLock)
			{
				OnClearQueue();
			}
			OnTriggerQueueEvent();
		}

		protected virtual void Cancel(TypeOfQueueItem Item)
		{
			Item.Cancel();
			OnTriggerQueueEvent();
		}

		protected virtual void Enqueue(TypeOfQueueItem Item)
		{
			lock(queueLock)
			{
				OnEnqueueItem(Item);
			}
			OnTriggerQueueEvent();
		}

		protected virtual TypeOfQueueItem Dequeue()
		{
			TypeOfQueueItem item;
			item = OnDequeueItem();
			return item;
		}

		protected void OnTriggerQueueEvent()
		{
			queueModifiedEvent.Set();
			OnQueueChanged();
		}

		protected override void ThreadLoop()
		{
			
			//OnEventTriggeredDelegate<TypeOfQueueItem> eventDelegate;

			//eventDelegate = new OnEventTriggeredDelegate<TypeOfQueueItem>(OnEventTriggered);
			do
			{
				#region wait for queue to be filled
				while ((OnGetQueueLength() == 0) && (State == WorkerStates.Started))
				{
					WriteLog(LogLevels.Debug, "Queue is empty, sleep until new item is added");
					WaitHandles(-1, QuitEvent, queueModifiedEvent);
				}
				#endregion

				#region get next event
				if (State == WorkerStates.Started)	// loop while queue is modified
				{
					WriteLog(LogLevels.Debug, "Process queue item");
					currentQueueItem = Dequeue();
				}
				#endregion
				
				#region trigger event if needed
				if ((State == WorkerStates.Started) && (currentQueueItem!=null) && (!currentQueueItem.IsCanceled))
				{
					WriteLog(LogLevels.Information, "Call event procedure");
					OnEventTriggered(new EventTriggeredEventArgs<TypeOfQueueItem>(DateTime.Now.Ticks, currentQueueItem));
					//OnInvokeEventDelegate(eventDelegate, );
				}
				#endregion
			} while (State == WorkerStates.Started);

		}

		
		

	



	}
}
