using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using LogUtils;
using System.IO;

namespace WorkerLib
{
	public abstract class TcpListenerWorker<ClientWorkerType> : ListenerWorker<TcpClient, ClientWorkerType>
		where ClientWorkerType : TcpClientWorker
	{
		private TcpListener listener;
		


		public TcpListenerWorker(string Name, IPEndPoint LocalEndPoint)
			: base(Name, LocalEndPoint)
		{
		
		}

		protected override void OnInitializeRessources(params object[] Parameters)
		{
			base.OnInitializeRessources(Parameters);
			try
			{
				WriteLog(LogLevels.Debug, "Initialize listener");
				listener = new TcpListener(LocalEndPoint);
			}
			catch (Exception ex)
			{
				WriteLog(LogLevels.Fatal, Logger.FormatException("failed to create listener", ex));
			}
		}

		protected override bool OnStartListener()
		{
			if (listener == null)
			{
				WriteLog(LogLevels.Fatal, "Listener is not initialized");
				return false;
			}
			try
			{
				WriteLog(LogLevels.Debug, "Try to start listener");
				listener.Start();
				this.LocalEndPoint = (IPEndPoint)listener.LocalEndpoint;
				return true;
			}
			catch (Exception ex)
			{
				WriteLog(LogLevels.Fatal, Logger.FormatException("Failed to start listener", ex));
				return false;
			}
		}

		protected override void OnStopListener()
		{
			if (listener == null)
			{
				WriteLog(LogLevels.Fatal, "Listener is not initialized");
			}
			else
			{
				listener.Stop();
			}
		}

		protected override TcpClient OnListenForClient()
		{
			return listener.AcceptTcpClient();
			
		}


		protected override string OnGetClientDescription(TcpClient Client)
		{
			return Client.Client.RemoteEndPoint.ToString();
		}

		protected override Stream OnCreateStream(TcpClient Client)
		{
			try
			{
				return Client.GetStream();
			}
			catch (Exception ex)
			{
				WriteLog(LogLevels.Error, Logger.FormatException("Cannot create stream from client", ex));
				return null;
			}
		}


	}
}
