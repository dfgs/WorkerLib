using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace WorkerLib
{
	public class PriorityQueue<TypeOfPriority, TypeOfValue> : IDisposable, IEnumerable<PriorityQueueItem<TypeOfPriority, TypeOfValue>>
		where TypeOfPriority:IComparable<TypeOfPriority>
	{
		private int capacity;
		public int Capacity
		{
			get { return capacity; }
		}

		private int count;
		public int Count
		{
			get { return count; }
		}

		private PriorityQueueItem<TypeOfPriority, TypeOfValue>[] items;

		public PriorityQueue(int Capacity)
		{
			this.capacity = Capacity;
			count = 0;
			items = new PriorityQueueItem<TypeOfPriority, TypeOfValue>[capacity];
		}

		public void Dispose()
		{
			count = 0;
			items = null;
		}

		

		public PriorityQueueItem<TypeOfPriority, TypeOfValue> Peek()
		{
			return items[0];
		}

		public int Enqueue(TypeOfPriority Priority, TypeOfValue Value)
		{
			return Enqueue(new PriorityQueueItem<TypeOfPriority, TypeOfValue>(Priority, Value));
		}
		public int Enqueue(PriorityQueueItem<TypeOfPriority,TypeOfValue> Item)
		{
			if (count == capacity) return -1;

			int itemIndex = count;
			items[itemIndex] = Item;
			count++;

			while (BubbleUp(ref itemIndex))
			{
			}

			return itemIndex;
		}
		
		public PriorityQueueItem<TypeOfPriority, TypeOfValue> Dequeue()
		{
			if (count == 0) throw (new Exception("Queue is empty"));

			PriorityQueueItem<TypeOfPriority, TypeOfValue> firstItem;

			firstItem = items[0];
			items[0] = items[count - 1];
			items[count - 1] = null;
			count--;

			int itemIndex = 0;
			while (BubbleDown(ref itemIndex))
			{
			}

			return firstItem;
		}

		public void Clear()
		{
			count = 0;
		
		}
	

		private bool BubbleUp(ref int Index)
		{
			PriorityQueueItem<TypeOfPriority, TypeOfValue> parentItem;
			PriorityQueueItem<TypeOfPriority, TypeOfValue> item;
			int parentIndex;

			if (Index <= 0) return false;
			
			parentIndex = (Index - 1) / 2;
			parentItem = items[parentIndex];
			item = items[Index];

			if (parentItem.Priority.CompareTo(item.Priority) > 0)
			{
				items[parentIndex] = item;
				items[Index] = parentItem;
				Index = parentIndex;
				return true;
			}
			
			return false;
		}

		private bool BubbleDown(ref int Index)
		{
			PriorityQueueItem<TypeOfPriority, TypeOfValue> childItem;
			PriorityQueueItem<TypeOfPriority, TypeOfValue> item;
			int childIndex;


			childIndex = GetSmallestChildIndex(Index);
			if (childIndex == -1) return false;

			childItem=items[childIndex];
			item=items[Index];

			if (item.Priority.CompareTo(childItem.Priority) >= 0)
			{
				items[childIndex] = item;
				items[Index] = childItem;
				Index = childIndex;

				return true;
			}

			return false;		
				
		}

		private int GetSmallestChildIndex(int Index)
		{
			int childIndex1 = Index * 2 + 1;
			int childIndex2 = childIndex1 + 1;

			if (childIndex1 >= count) return -1;

			if (childIndex2 >= count) return childIndex1;

			if (items[childIndex1].Priority.CompareTo(items[childIndex2].Priority) < 0) return childIndex1;
			else return childIndex2;
		}

		public bool ContainsValue(TypeOfValue Value)
		{
			for (int t = 0; t < count; t++)
			{
				if (items[t].Value.Equals(Value)) return true;
			}

			return false;
		}


		#region IEnumerable<PriorityQueueItem<TypeOfPriority, TypeOfValue>> Membres

		public IEnumerator<PriorityQueueItem<TypeOfPriority, TypeOfValue>> GetEnumerator()
		{
			foreach (PriorityQueueItem<TypeOfPriority, TypeOfValue> item in items)
			{
				yield return item;
			}

		}

		#endregion

		#region IEnumerable Membres

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (PriorityQueueItem<TypeOfPriority, TypeOfValue> item in items)
			{
				yield return item;
			}
		}

		#endregion




	}
}
