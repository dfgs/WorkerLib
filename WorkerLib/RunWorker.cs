using System;
using LogUtils;
using System.Diagnostics;


namespace WorkerLib
{
	public abstract class RunWorker :  Worker
	{
		public event EventHandler Terminated;
		public event EventHandler Started;
		public event EventHandler Stopped;

	
		private WorkerStates state;
		public WorkerStates State
		{
			get { return state; }
			protected set { state = value; }
		}

	

		public RunWorker(string Name, params object[] Parameters)
			:base(Name,Parameters)
		{
			state = WorkerStates.Stopped;
		}

				
		protected override void OnDisposeRessources()
		{
			WriteLog(LogLevels.Debug,LogActions.Enter);
			Stop();
		}


		public bool Start()
		{
			WriteLog(LogLevels.Debug,LogActions.Enter);
			bool result=false;

			if (state != WorkerStates.Stopped)
			{
				WriteLog(LogLevels.Information,  "Current state is " + state.ToString() + ", exiting");
			}
			else
			{
				state = WorkerStates.Starting;
				result = OnStartProcessing();
				if (!result)
				{
					WriteLog(LogLevels.Error, "Failed to start");
					state=WorkerStates.Error;
				}
				else 
				{
					state = WorkerStates.Started;
					OnStarted();
				}
			}
			WriteLog(LogLevels.Debug, LogActions.Quit);
			//OnPropertyChanged("State");
			return result;
		}

		protected virtual bool OnStartProcessing()
		{
			WriteLog(LogLevels.Debug, LogActions.Enter);
			//OnPropertyChanged("State");
			return true;
		}

		public bool Stop()
		{
			bool result = false;
			WriteLog(LogLevels.Debug,LogActions.Enter);
			
			if ((state != WorkerStates.Started) && (state!=WorkerStates.Inactive))
			{
				WriteLog(LogLevels.Information, "Current state is " + state.ToString() + ", exiting");
			}
			else
			{
				state = WorkerStates.Stopping;
				result = OnStopProcessing();
				if (!result)
				{
					WriteLog(LogLevels.Error, "Failed to stop");
					state = WorkerStates.Error;
				}
				else
				{
					state = WorkerStates.Stopped;
					OnStopped();
				}
			}
						
			WriteLog(LogLevels.Debug, LogActions.Quit);
			//OnPropertyChanged("State");
			return result;
		}

		protected virtual bool OnStopProcessing()
		{
			WriteLog(LogLevels.Debug, LogActions.Enter);
			//OnPropertyChanged("State");
			return true;
		}


		protected virtual void OnStarted()
		{
			if (Started != null) Started(this, EventArgs.Empty);
		}
		protected virtual void OnStopped()
		{
			if (Stopped != null) Stopped(this, EventArgs.Empty);
		}
		protected virtual void OnTerminated()
		{
			if (Terminated != null) Terminated(this, EventArgs.Empty);
		}

		
	}
}
