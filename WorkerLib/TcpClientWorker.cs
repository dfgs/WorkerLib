using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using LogUtils;
using System.IO;

namespace WorkerLib
{
	public abstract class TcpClientWorker:ClientWorker<TcpClient>
	{
		private Stream stream;
		
		private BinaryReader reader;
		protected BinaryReader Reader
		{
			get { return reader; }
		}

		private BinaryWriter writer;
		protected BinaryWriter Writer
		{
			get { return writer; }
		}

		public TcpClientWorker(string Name,TcpClient Client,Stream Stream)
			: base(Name,Client)
		{
			this.stream = Stream;
			WriteLog(LogLevels.Debug, "Create reader and writer");
			reader = new BinaryReader(stream);
			writer = new BinaryWriter(stream);
		}

		


		protected override void OnCloseClient()
		{
			Client.Close();
		}


	}
}
