using System;
using LogUtils;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WorkerLib
{
	public abstract class Worker :  IDisposable
	{

		private static int idCounter=0;

		private int id;
		public int ID
		{
			get { return id; }
		}

		private string name;
		public string Name
		{
			get { return name; }
		}

	

		public Worker(string Name, params object[] Parameters)
		{
			idCounter++;
			this.id = idCounter;

			this.name = Name;
			WriteLog(LogLevels.Debug, LogActions.Enter);
			OnInitializeRessources(Parameters);
			WriteLog(LogLevels.Debug, LogActions.Quit);
		}


		protected virtual void OnInitializeRessources(params object[] Parameters)
		{
			WriteLog(LogLevels.Debug, LogActions.Enter);
		}

		public void Dispose()
		{
			WriteLog(LogLevels.Debug,LogActions.Enter);
			OnDisposeRessources();
		}
		protected virtual void OnDisposeRessources()
		{
			WriteLog(LogLevels.Debug,LogActions.Enter);
		}


		protected void WriteLog(LogLevels Level, LogActions Action, [CallerMemberName]string CallerMember = "CallerMember")
		{
			//string stackProc;
			//StackTrace stackTrace = new StackTrace();
			//stackProc = stackTrace.GetFrame(1).GetMethod().Name;
			Logger.WriteLog(Level, Name + ":" + CallerMember,id, Action.ToString());
		}
		protected void WriteLog(LogLevels Level, string Message, [CallerMemberName]string CallerMember = "CallerMember")
		{
			//string stackProc;
			//StackTrace stackTrace = new StackTrace();
			//stackProc = stackTrace.GetFrame(1).GetMethod().Name;
			Logger.WriteLog(Level, Name+":"+CallerMember,id,Message);
		}
		protected void WriteLog(Exception ex, [CallerMemberName]string CallerMember = "CallerMember")
		{
			string message;
			message = Logger.FormatException("An exception occured", ex);
			WriteLog(LogLevels.Fatal, message,CallerMember);
		}


	}
}
