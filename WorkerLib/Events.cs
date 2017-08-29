using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkerLib
{
	public class EventTriggeredEventArgs<TypeofItem> : EventArgs
	{
		private long eventTick;
		public long EventTick
		{
			get { return eventTick; }
		}
			

		private TypeofItem item;
		public TypeofItem Item
		{
			get { return item; }
		}

		public EventTriggeredEventArgs(long EventTick, TypeofItem Item)
		{
			this.eventTick = EventTick;
			this.item = Item;
		}
	}

	public delegate void EventTriggeredHandler<TypeOfQueueItem>(object Sender, EventTriggeredEventArgs<TypeOfQueueItem> e);



	public class DataReceivedEventArgs<ClientType> : EventArgs
	{
		private byte[] data;
		public byte[] Data
		{
			get { return data; }
		}

		private ClientWorker<ClientType> clientWorker;
		public ClientWorker<ClientType> ClientWorker
		{
			get { return clientWorker; }
		}

		public DataReceivedEventArgs(ClientWorker<ClientType> ClientWorker, byte[] Data)
		{
			this.data = Data;
			this.clientWorker = ClientWorker;
		}
	}

	public delegate void DataReceivedEventHandler<ClientWorkerType, ClientType>(object Sender, DataReceivedEventArgs< ClientType> e);

}
