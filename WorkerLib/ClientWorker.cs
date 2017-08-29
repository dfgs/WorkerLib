using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LogUtils;
using System.Net.Sockets;

namespace WorkerLib
{

	//delegate void OnDataReceivedDelegate(byte[] Data);

	public abstract class ClientWorker<ClientType>:ThreadWorker
	{
		//public event DataReceivedEventHandler<ClientWorker<ClientType>,ClientType> DataReceived;

		

		private ClientType client;
		public ClientType Client
		{
			get { return client; }
		}

		public ClientWorker(string Name,ClientType Client,int StopTimeout= 5000)
			:base(Name,ThreadPriority.Normal,StopTimeout, Client)
		{
		}



		protected override void OnInitializeRessources(params object[] Parameters)
		{
			base.OnInitializeRessources(Parameters);
			client = (ClientType)Parameters[0];
		}

		protected void CloseClient()
		{
			WriteLog(LogLevels.Debug, "Close client");
			OnCloseClient();

		}

		protected override bool OnStopProcessing()
		{
			CloseClient();
			return base.OnStopProcessing();
		}

		protected abstract void OnCloseClient();

		protected abstract void OnReceiveData();


		protected override void ThreadLoop()
		{

			WriteLog(LogLevels.Debug, "Start to handle client messages");
			while (State == WorkerStates.Started)
			{
				WriteLog(LogLevels.Debug, "Wait for data");
				try
				{
					OnReceiveData();
				}
				catch (Exception ex)
				{
					if (State != WorkerStates.Stopping)
					{
						WriteLog(LogLevels.Warning, Logger.FormatException("An expected error occured, client may disconnected", ex));
					}
					break;
				}
			}

		}

	}
}
