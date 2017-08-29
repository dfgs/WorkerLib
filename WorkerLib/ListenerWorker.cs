using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LogUtils;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace WorkerLib
{


	public abstract class ListenerWorker<ClientType,ClientWorkerType>:ThreadWorker
		where ClientWorkerType:ClientWorker<ClientType>
	{
		//public event DataReceivedEventHandler DataReceived;

	
		private List<ClientWorkerType> clientWorkers;

		public event EventHandler ListenerUp;
		public event EventHandler ListenerDown;
		public event EventHandler ClientConnected;

		private IPEndPoint localEndPoint;
		public IPEndPoint LocalEndPoint
		{
			get { return localEndPoint; }
			protected set { localEndPoint = value; }
		}


		/*private bool ready;
		public bool Ready
		{
			get { return ready; }
		}*/

		public ListenerWorker(string Name,IPEndPoint LocalEndPoint,int StopTimeout=5000)
			: base(Name,ThreadPriority.Normal,StopTimeout, LocalEndPoint)
		{
			clientWorkers = new List<ClientWorkerType>();	
		}

		protected override void OnInitializeRessources(params object[] Parameters)
		{
			base.OnInitializeRessources(Parameters);
			localEndPoint = (IPEndPoint)Parameters[0];
		}
		
		protected override bool OnStopProcessing()
		{
			bool result=true;

			WriteLog(LogLevels.Debug, "Close Listener");
			OnStopListener();
			WriteLog(LogLevels.Debug, "Stop client workers");
			lock (clientWorkers)
			{
				foreach (ClientWorkerType clientWorker in clientWorkers)
				{
					result &=clientWorker.Stop();
				}
				WriteLog(LogLevels.Debug, "Clear client workers");
				clientWorkers.Clear();
			}
			return result & base.OnStopProcessing();
		}

		protected abstract bool OnStartListener();
		protected abstract void OnStopListener();
		protected abstract ClientType OnListenForClient();
		protected abstract ClientWorkerType OnCreateClientWorker(ClientType Client,Stream Stream);
		protected abstract Stream OnCreateStream(ClientType Client);
		protected abstract string OnGetClientDescription(ClientType Client);


		protected virtual void OnClientConnected(ClientType Client)
		{
			WriteLog(LogLevels.Debug, "New client connected from " + OnGetClientDescription(Client));
			if (ClientConnected != null) ClientConnected.BeginInvoke(this, EventArgs.Empty, null, null);
		}
		
		protected virtual void OnListenerUp()
		{
			if (ListenerUp != null) ListenerUp.BeginInvoke(this, EventArgs.Empty,null,null);
		}
		protected virtual void OnListenerDown()
		{
			if (ListenerDown != null) ListenerDown.BeginInvoke(this, EventArgs.Empty, null, null);
		}


		
		protected override void ThreadLoop()
		{
			Stream stream;
			ClientType client;
			ClientWorkerType clientWorker;

			do
			{
				#region start listener
				while (State == WorkerStates.Started)
				{
					WriteLog(LogLevels.Information,"Try to start listener");
					if (OnStartListener())
					{
						OnListenerUp();
						break;
					}
					WriteLog(LogLevels.Warning, "Failed to start listener, wait for 5 seconds");
					WaitHandles(5000, QuitEvent);
				}
				#endregion

				#region wait for client
				while (State==WorkerStates.Started)
				{
					WriteLog(LogLevels.Debug, "Waiting for new client");
					#region wait for client
					try
					{
						client = OnListenForClient();
					}
					catch (SocketException socketException)
					{
						if (socketException.SocketErrorCode == SocketError.Interrupted)
						{
							WriteLog(LogLevels.Information, "Listener was manually interrupted");
						}
						else
						{
							WriteLog(LogLevels.Error, Logger.FormatException("Listener was abnormally interrupted", socketException));
						}
						break;
					}
					catch (Exception otherException)
					{
						WriteLog(LogLevels.Error, Logger.FormatException("Listener was abnormally interrupted", otherException));
						break;
					}
					#endregion

					OnClientConnected(client);
					WriteLog(LogLevels.Debug, "Try to create stream from client");
					stream = OnCreateStream(client);
					if (stream == null) continue;

					WriteLog(LogLevels.Debug, "Create client worker");
					clientWorker = OnCreateClientWorker(client,stream);
						
					lock (clientWorkers)
					{
						clientWorkers.Add(clientWorker);
					} 
					clientWorker.Start();
					

				
				}
				OnListenerDown();
				#endregion

			} while (State == WorkerStates.Started);

		}

		


	
	}
}
