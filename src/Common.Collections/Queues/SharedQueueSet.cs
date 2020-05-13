using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Common.Collections.Queues
{
	/// <summary>
	/// Defines a set of shared queues.  Handles the queueing of items into one or more of the queues based on parameters 
	/// as well as cleanup of stale queues.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SharedQueueSet<T>
	{
		private static List<SharedQueue<T>> _queues = new List<SharedQueue<T>>();
		private static object _queuesLock = new object();
		private static Timer _queueCleanupTimer;

		private int _staleTime;
		private string _setScope;

		/// <summary>
		/// Event raised when a queue is added to the SharedQueueSet.
		/// </summary>
		public event QueueEventHandler<T> QueueAdded;

		/// <summary>
		/// Event raised when a queue has been removed (either due to expiration or an explicit call) from the SharedQueueSet.
		/// </summary>
		public event QueueRemovedEventHandler<T> QueueRemoved;

		/// <summary>
		/// The scope of the queue set.
		/// </summary>
		public string Scope
		{
			get { return _setScope; }
		}

		/// <summary>
		/// Creates a new shared queue set with the specified scope.
		/// </summary>
		/// <param name="setScope">The scope identifier of the queue set.  This is used to identify target queues for items.</param>
		public SharedQueueSet(string setScope) : this(setScope, 60)
		{
		}
		
		/// <summary>
		/// Creates a new shared queue set with the specified queue stale time.
		/// </summary>
		/// <param name="staleTime">How many seconds before a queue is considered stale (e.g. last accessed) and will be removed.  Pass 0 for no cleanup.</param>
		public SharedQueueSet(int staleTime) : this("", staleTime)
		{
		}

		/// <summary>
		/// Creates a new shared queue set.
		/// </summary>
		/// <param name="setScope">The scope identifier of the queue set.  This is used to identify target queues for items.</param>
		/// <param name="staleTime">How many seconds before a queue is considered stale (e.g. last accessed) and will be removed.  Pass 0 for no cleanup.</param>
		public SharedQueueSet(string setScope, int staleTime)
		{
			_setScope = setScope;
			_staleTime = staleTime;
			if(_staleTime > 0)
			{
				StartQueueCleanup();
			}
		}

		private void StartQueueCleanup()
		{
			_queueCleanupTimer = new Timer(1000 * _staleTime);
			_queueCleanupTimer.AutoReset = false;
			_queueCleanupTimer.Elapsed += new ElapsedEventHandler(_queueCleanupTimer_Elapsed);
			_queueCleanupTimer.Start();
		}

		private void _queueCleanupTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			DateTime checkTime = DateTime.Now.AddSeconds(-1 * _staleTime);
			int i;
			SharedQueue<T> queue;

			lock(_queuesLock)
			{
				i = 0;
				while(i < _queues.Count)
				{
					queue = _queues[i];
					if(queue.LastReadTime < checkTime)
					{
						Remove(queue, true);
					}
					else
					{
						i++;
					}
				}
			}
			_queueCleanupTimer.Start();
		}

		/// <summary>
		/// Create a new queue for this set with the specified sub scope.  
		/// The resulting queue scope will be a combination of the set scope and individual queue scope.
		/// </summary>
		/// <param name="subScope">The new queue's sub scope specifier.</param>
		/// <returns></returns>
		public SharedQueue<T> NewQueue(string subScope)
		{
			SharedQueue<T> queue;
			lock(_queuesLock)
			{
				queue = new SharedQueue<T>(subScope, this);
				_queues.Add(queue);
			}
			if(QueueAdded != null)
			{
				QueueAdded(this, new SharedQueueEventArgs<T>() { Queue = queue });
			}

			return queue;
		}

		/// <summary>
		/// Enqueues an item to all the queues in the set.
		/// </summary>
		/// <param name="item">The item to queue.</param>
		public void Enqueue(T item)
		{
			Enqueue(item, _setScope);
		}

		/// <summary>
		/// Enqueues an item to all queues that match the specified scope.
		/// </summary>
		/// <param name="item">The item to queue.</param>
		/// <param name="scope">The target queue scope.</param>
		public void Enqueue(T item, string scope)
		{
			Enqueue(item, q => q.FullScope.StartsWith(scope));
		}

		/// <summary>
		/// Enqueues an item to all queues that match the specified <paramref name="queueId"/> (normally just one).
		/// </summary>
		/// <param name="item">The item to queue.</param>
		/// <param name="queueId">A queue Id.</param>
		public void Enqueue(T item, Guid queueId)
		{
			Enqueue(item, q => q.QueueId.Equals(queueId));
		}

		/// <summary>
		/// Enqueues an item to all queue EXCEPT those matching the specified <paramref name="excludeQueueId"/>s.
		/// </summary>
		/// <param name="item">The item to queue.</param>
		/// <param name="excludeQueueId">One or more queue ids.</param>
		public void EnqueueExclude(T item, params Guid[] excludeQueueId)
		{
			Enqueue(item, q => !excludeQueueId.Contains(q.QueueId));
		}

		/// <summary>
		/// Enqueues an item to all queue EXCEPT those matching the specified <paramref name="excludeQueue"/>s.
		/// </summary>
		/// <param name="item">The item to queue.</param>
		/// <param name="excludeQueue">One or more queues.</param>
		public void EnqueueExclude(T item, params SharedQueue<T>[] excludeQueue)
		{
			Enqueue(item, q => !excludeQueue.Contains(q));
		}

		private void Enqueue(T item, Func<SharedQueue<T>, bool> predicate)
		{
			IEnumerable<SharedQueue<T>> toQueues;
			lock(_queuesLock)
			{
				toQueues = _queues.Where(predicate);
				toQueues.ToList().ForEach(mq => mq.Enqueue(item));
			}
		}

		/// <summary>
		/// Gets a full scope string of a shared queue owned by this queue set for the provided sub scope.
		/// </summary>
		/// <param name="subScope">The scope of an individual shared queue.</param>
		/// <returns></returns>
		public string GetFullScope(string subScope)
		{
			return string.Format("{0}.{1}", _setScope, subScope);
		}

		/// <summary>
		/// Gets the queue in the set that matches the <paramref name="queueId"/>.
		/// Otherwise, returns null.
		/// </summary>
		/// <param name="queueId"></param>
		/// <returns></returns>
		public SharedQueue<T> GetQueue(Guid queueId)
		{
			return _queues.SingleOrDefault(q => q.QueueId.Equals(queueId));
		}

		/// <summary>
		/// Gets all the queued messages for the specified <paramref name="queueId"/>.
		/// If no queue matching the queue id is in the set, an empty list will be returned.
		/// </summary>
		/// <param name="queueId">Id of the queue from which items will be retrieved.</param>
		/// <returns></returns>
		public List<T> GetQueueItems(Guid queueId)
		{
			SharedQueue<T> queue = GetQueue(queueId);
			if(queue != null)
			{
				return queue.DequeueAll();
			}
			return new List<T>();
		}

		/// <summary>
		/// Removes the queue matching the specified queueId.
		/// </summary>
		/// <param name="queueId"></param>
		/// <returns></returns>
		public bool RemoveQueue(Guid queueId)
		{
			lock(_queuesLock)
			{
				SharedQueue<T> queue = GetQueue(queueId);
				if(queue != null)
				{
					Remove(queue, false);
					return true;
				}
			}
			return false;
		}

		private void Remove(SharedQueue<T> queue, bool expired)
		{
			_queues.Remove(queue);
			if(QueueRemoved != null)
			{
				QueueRemoved(this, new SharedQueueRemovedEventArgs<T>() { Queue = queue, QueueExpired = expired});
			}
		}

	}


}
