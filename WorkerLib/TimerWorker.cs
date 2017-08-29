using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LogUtils;

namespace WorkerLib
{
	public abstract class TimerWorker: ThreadWorker
	{
		public event EventTriggeredHandler<int> EventTriggered;

		private AutoResetEvent periodModifiedEvent;
		protected AutoResetEvent PeriodModifiedEvent
		{
			get { return periodModifiedEvent; }
		}

		private int period;
		public int Period
		{
			get { return period; }
			set 
			{
				if (period == value) return;
				period = value; periodModifiedEvent.Set(); 
			}
		}

		public TimerWorker(string Name,ThreadPriority Priority, int StopTimeout=5000, params object[] Parameters)
			: base(Name, Priority,StopTimeout,Parameters)
		{
			period=3000;
		}

		protected override void OnInitializeRessources(params object[] Parameters)
		{
			base.OnInitializeRessources(Parameters);

			WriteLog(LogUtils.LogLevels.Debug, "Create Period Modified Event");
			periodModifiedEvent = new AutoResetEvent(false);
		}

		protected override void OnDisposeRessources()
		{
			base.OnDisposeRessources();
			WriteLog(LogUtils.LogLevels.Debug, "Close Period Modified Event");
			periodModifiedEvent.Close();
		}


		protected virtual void OnEventTriggered(EventTriggeredEventArgs<int> e)
		{
			if (EventTriggered != null)
			{
				WriteLog(LogLevels.Debug, "Trigger event");
				EventTriggered(this, e);
				//EventTriggered.BeginInvoke(this, e, null, null);
			}
		}

		
				

		protected override void ThreadLoop()
		{
			int eventTick;
			WaitHandle result=null;

			do
			{
				#region wait for period
				result = periodModifiedEvent;
				while ((State == WorkerStates.Started) && (result==periodModifiedEvent))
				{
					WriteLog(LogLevels.Debug, "Wait for "+period.ToString()+" ms");
					result=WaitHandles(period, QuitEvent, periodModifiedEvent);
				}
				#endregion
								
				#region trigger event if needed
				if (State == WorkerStates.Started)
				{
					eventTick = Environment.TickCount;
					WriteLog(LogLevels.Information, "Call event procedure");
					OnEventTriggered(new EventTriggeredEventArgs<int>(DateTime.Now.Ticks, eventTick));
				}
				#endregion
			} while (State == WorkerStates.Started);

		}

		
		

	

	}
}
