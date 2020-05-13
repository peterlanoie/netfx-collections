using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Collections.Queues
{
	/// <summary>
	/// Defines a shared queue.  Used in a shared queue set.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SharedQueue<T>
	{
		private Queue<T> _queue;
		private object _queueLock = new object();
		private SharedQueueSet<T> _queueSet;
		
		/// <summary>
		/// The unique queue Id.
		/// </summary>
		public Guid QueueId { get; private set; }

		/// <summary>
		/// The scope of the queue. This is used in conjunction with the scope of the queue set to determine where
		/// shared items will go.
		/// </summary>
		public string Scope { get; private set; }

		/// <summary>
		/// The last time an item was read from the queue.
		/// </summary>
		public DateTime LastReadTime { get; set; }

		/// <summary>
		/// The last time an item was written to the queue.
		/// </summary>
		public DateTime LastWriteTime { get; set; }

		/// <summary>
		/// The last time the queue was accessed for either a read or write operation.
		/// </summary>
		public DateTime LastAccessTime { get; set; }

		internal SharedQueue(string scope, SharedQueueSet<T> queueSet)
		{
			_queue = new Queue<T>();
			_queueSet = queueSet;
			QueueId = Guid.NewGuid();
			Scope = scope;
			LastAccessTime = LastWriteTime = LastReadTime = DateTime.Now;
		}

		/// <summary>
		/// Enqueues a single item to a single shared queue.
		/// User 
		/// </summary>
		/// <param name="item"></param>
		public void Enqueue(T item)
		{
			lock(_queueLock)
			{
				LastAccessTime = LastWriteTime = DateTime.Now;
				_queue.Enqueue(item);
			}
		}

		/// <summary>
		/// Enqueues a list of items to a single shared queue.
		/// </summary>
		/// <param name="items"></param>
		public void Enqueue(List<T> items)
		{
			lock(_queueLock)
			{
				LastAccessTime = LastWriteTime = DateTime.Now;
				items.ForEach(i => _queue.Enqueue(i));
			}
		}


		/// <summary>
		/// Get one item from the queue. If queue contains no items, the type default will be returned.
		/// </summary>
		/// <returns></returns>
		public T DequeueOne()
		{
			T item = default(T);
			lock(_queueLock)
			{
				LastAccessTime = LastReadTime = DateTime.Now;
				if(_queue.Count > 0)
				{
					item = _queue.Dequeue();
				}
			}
			return item;
		}

		/// <summary>
		/// Get all the items currently in the queue.
		/// </summary>
		/// <returns></returns>
		public List<T> DequeueAll()
		{
			List<T> list = new List<T>();
			lock(_queueLock)
			{
				LastAccessTime = LastReadTime = DateTime.Now;
				while(_queue.Count > 0)
				{
					list.Add(_queue.Dequeue());
				}
			}
			return list;
		}

		/// <summary>
		/// Removes all objects from the queue.
		/// </summary>
		public void Clear()
		{
			lock(_queueLock)
			{
				_queue.Clear();
			}
		}

		/// <summary>
		/// Returns the item at the beginning of the queue without removing it.
		/// </summary>
		/// <returns></returns>
		public T Peek()
		{
			return _queue.Peek();
		}

		/// <summary>
		/// The full scope of this queue as it relates to the shared queue set in which it is included.
		/// </summary>
		public string FullScope
		{
			get
			{
				return _queueSet.GetFullScope(Scope);
			}
		}

		/// <summary>
		/// Gets the number of items in the queue.
		/// </summary>
		public int Count { get { return _queue.Count; } }

		/// <summary>
		/// Returns the ID of the queue.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.QueueId.ToString();
		}

	}
}
