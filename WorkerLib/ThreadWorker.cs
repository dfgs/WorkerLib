using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LogUtils;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace WorkerLib
{
	public abstract class ThreadWorker:RunWorker
	{


		private static long percentScale = 100 / Environment.ProcessorCount;
		private ProcessThread processThread;

		[DllImport("Kernel32", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
		public static extern Int32 GetCurrentWin32ThreadId();

		private long previousTicks;
		private long previousCPUTime;
		private long previousCPUTime2;
		private float cpuUsage;
		private static long oneSecondTicks = TimeSpan.FromSeconds(1).Ticks;
		private int stopTimeout;
		protected int StopTimeout
		{
			get { return stopTimeout; }
		}

		private ManualResetEvent exitEvent;

		private Thread thread;
		protected Thread Thread
		{
			get { return thread; }
		}

		private ManualResetEvent quitEvent;
		protected ManualResetEvent QuitEvent
		{
			get { return quitEvent; }
		}

		private ThreadPriority priority;
		public ThreadPriority Priority
		{
			get { return priority; }
		}

		public ThreadWorker(string Name,ThreadPriority Priority,int StopTimeout=5000, params object[] Parameters)
			: base(Name, Parameters)
		{
			this.priority = Priority;
			this.stopTimeout = StopTimeout;
		}


		protected override void OnInitializeRessources(params object[] Parameters)
		{
			base.OnInitializeRessources(Parameters);
			WriteLog(LogLevels.Debug, "Create exit events");
			exitEvent = new ManualResetEvent(false);
			quitEvent = new ManualResetEvent(false);
		}

		protected override void OnDisposeRessources()
		{
			base.OnDisposeRessources();
			WriteLog(LogLevels.Debug, "Dispose events");
			exitEvent.Close();
			quitEvent.Close();
			
		}

		protected override  bool OnStartProcessing()
		{
			base.OnStartProcessing();
			WriteLog(LogLevels.Debug, "Reset exit event");
			exitEvent.Reset();
			WriteLog(LogLevels.Debug, "Reset quit event");
			quitEvent.Reset();

			#region try to create thread
			WriteLog(LogLevels.Debug, "Create thread");
			try
			{
				thread = new Thread(new ThreadStart(ThreadStart));
				thread.Priority = priority;
				thread.Name = Name;
			}
			catch (Exception e)
			{
				WriteLog(LogLevels.Error, Logger.FormatException("Failed to create thread", e));
				return false;
			}
			#endregion

			#region try to start thread
			WriteLog(LogLevels.Debug, "Start thread");
			try
			{
				thread.Start();
			}
			catch (Exception e)
			{
				WriteLog(LogLevels.Error, Logger.FormatException("Failed to start thread", e));
				return false;
			}
			#endregion

			return true;
		}

		protected override  bool OnStopProcessing()
		{
			base.OnStopProcessing();
			WriteLog(LogLevels.Debug, "Trigger quit event");
			quitEvent.Set();

			//WriteLog(LogLevels.Debug, "Allow 5 secs to thread to stop gracefully");
			if (exitEvent.WaitOne(stopTimeout))
			{
				WriteLog(LogLevels.Debug, "Thread stopped gracefully");
				return true;
			}
			else
			{
				WriteLog(LogLevels.Warning, "Thread didn't stop gracefully");
				return false;
			}
		}

		private void ThreadStart()
		{
			int threadId;

			WriteLog(LogLevels.Debug, LogActions.Enter);
			while (State != WorkerStates.Started)
			{
				WriteLog(LogLevels.Debug, "Wait 100 ms, state need to be started");
				Thread.Sleep(100);
			}


			WriteLog(LogLevels.Debug, "Getting current process thread from system");
			threadId = GetCurrentWin32ThreadId();
			foreach (ProcessThread pc in Process.GetCurrentProcess().Threads)
			{
				if (pc.Id == threadId) this.processThread = pc;
			}

			previousTicks = processThread.StartTime.Ticks;
			previousCPUTime = processThread.TotalProcessorTime.Ticks;
			previousCPUTime2 = previousCPUTime;

			WriteLog(LogLevels.Debug, "Call ThreadLoop");
			ThreadLoop();

			OnTerminated();
			WriteLog(LogLevels.Debug, "ThreadLoop terminated, trigger Terminated Event");
			exitEvent.Set();
			
			State = WorkerStates.Inactive;
			WriteLog(LogLevels.Debug, LogActions.Quit);

		}

		protected abstract void ThreadLoop();

		public void Join()
		{
			if (thread == null) return;
			thread.Join();
		}

		protected void LimitCPU(int Percent)
		{
			long cpuTime;
			long currentCPUTime;
			long idleTime;

			currentCPUTime = processThread.TotalProcessorTime.Ticks;
			cpuTime = currentCPUTime - previousCPUTime2;

			idleTime = (percentScale * cpuTime) / Percent - cpuTime;
			Thread.Sleep(TimeSpan.FromTicks(idleTime));

			previousCPUTime2 = currentCPUTime;
		}

		private float GetCPUUsage()
		{
			long currentTicks;
			long currentCPUTime;
			
			currentTicks = DateTime.Now.Ticks;
			if (currentTicks < previousTicks + oneSecondTicks) return cpuUsage;

			currentCPUTime = processThread.TotalProcessorTime.Ticks;

			cpuUsage = (currentCPUTime - previousCPUTime) * percentScale / (currentTicks - previousTicks);

			//WriteLog(LogLevels.Debug, "CPU usage is " + usage.ToString() + "%");

			previousCPUTime = currentCPUTime;
			previousTicks = currentTicks;

			return cpuUsage;
		}

		protected WaitHandle WaitHandles( int Milliseconds,params WaitHandle[] Handles)
		{
			int result;

			WriteLog(LogLevels.Debug, "Sleep for " + Milliseconds.ToString() + " milliseconds...");
			result = WaitHandle.WaitAny(Handles, Milliseconds);
			WriteLog(LogLevels.Debug,"Wait handle returned result " + result.ToString());

			if (result == WaitHandle.WaitTimeout) return null;
			else return Handles[result];
		}


	}
}
