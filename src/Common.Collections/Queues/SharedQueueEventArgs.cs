using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Collections.Queues
{
	/// <summary>
	/// Defines a simple event argument for a shared queue
	/// </summary>
	public class SharedQueueEventArgs<T> : EventArgs
	{
		/// <summary>
		/// The id of the shared queue.
		/// </summary>
		public SharedQueue<T> Queue { get; set; }
	}

	/// <summary>
	/// Defines the arguments for the queue removed event.
	/// </summary>
	public class SharedQueueRemovedEventArgs<T> : SharedQueueEventArgs<T>
	{
		/// <summary>
		/// Whether the queue was removed due to expiration.
		/// </summary>
		public bool QueueExpired { get; set; }
	}

	/// <summary>
	/// Defines an event handler for shared queue events.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void QueueEventHandler<T>(SharedQueueSet<T> sender, SharedQueueEventArgs<T> e);

	/// <summary>
	/// Defines an event handler for shared queue events.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void QueueRemovedEventHandler<T>(SharedQueueSet<T> sender, SharedQueueRemovedEventArgs<T> e);

}
