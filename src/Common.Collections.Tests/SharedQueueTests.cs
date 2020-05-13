using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Collections.Queues;
using System.Threading;

namespace Common.Collections.Tests
{
	[TestClass]
	public class SharedQueueTests
	{
		SharedQueueSet<int> _queueSet;
		SharedQueue<int> _queue1, _queue2, _queue3, _queue4;

		[TestInitialize]
		public void Init()
		{
			MakeQueueSet(0);
		}

		private void MakeQueueSet(int staleTime)
		{
			_queueSet = new SharedQueueSet<int>("QueueSet", staleTime);
			_queue1 = _queueSet.NewQueue("Queue1");
			_queue2 = _queueSet.NewQueue("Queue2");
			_queue3 = _queueSet.NewQueue("Queue3/4");
			_queue4 = _queueSet.NewQueue("Queue3/4");
		}

		[TestMethod]
		public void BroadcastItemDefault()
		{
			_queueSet.Enqueue(100);
			Assert.AreEqual<int>(100, _queue1.DequeueOne());
			Assert.AreEqual<int>(100, _queue2.DequeueOne());
			Assert.AreEqual<int>(100, _queue3.DequeueOne());
			Assert.AreEqual<int>(100, _queue4.DequeueOne());
		}

		[TestMethod]
		public void BroadcastItemBySetScope()
		{
			_queueSet.Enqueue(110, _queueSet.Scope);
			Assert.AreEqual<int>(110, _queue1.DequeueOne());
			Assert.AreEqual<int>(110, _queue2.DequeueOne());
			Assert.AreEqual<int>(110, _queue3.DequeueOne());
			Assert.AreEqual<int>(110, _queue4.DequeueOne());
		}

		[TestMethod]
		public void BroadcastItemByQueueScope()
		{
			_queueSet.Enqueue(120, _queue3.FullScope);
			Assert.AreEqual(0, _queue1.DequeueOne());
			Assert.AreEqual(0, _queue2.DequeueOne());
			Assert.AreEqual<int>(120, _queue3.DequeueOne());
			Assert.AreEqual<int>(120, _queue4.DequeueOne());
		}

		[TestMethod]
		public void PrivateItemByQueueId()
		{
			_queueSet.Enqueue(200, _queue1.QueueId);
			Assert.AreEqual<int>(200, _queue1.DequeueOne());
			Assert.AreEqual(0, _queue2.DequeueOne());
		}

		[TestMethod]
		public void MultiItemDequeue()
		{
			_queueSet.Enqueue(300);
			_queueSet.Enqueue(301);
			_queueSet.Enqueue(302);
			_queueSet.Enqueue(303);
			_queueSet.Enqueue(304);
			List<int> list = _queue1.DequeueAll();
			Assert.AreEqual(0, _queue1.DequeueOne());
			Assert.AreEqual<int>(5, list.Count);
			Assert.AreEqual<int>(300, list[0]);
			Assert.AreEqual<int>(301, list[1]);
			Assert.AreEqual<int>(302, list[2]);
			Assert.AreEqual<int>(303, list[3]);
			Assert.AreEqual<int>(304, list[4]);
		}

		[TestMethod]
		public void QueueCleanup()
		{
			MakeQueueSet(2);
			_queueSet.Enqueue(400); // queue first Item
			Thread.Sleep(3000); //wait until 1 second past queue expiration
			_queueSet.Enqueue(401); // queue second Item
			Assert.AreEqual<int>(400, _queue1.DequeueOne()); // 1st Item should be there
			Assert.AreEqual(0, _queue1.DequeueOne()); // 2nd Item should NOT be there (queue destroyed)
		}

		[TestMethod]
		public void QueueNoCleanup()
		{
			_queueSet.Enqueue(400); // queue first Item
			Thread.Sleep(2000); //wait 
			_queueSet.Enqueue(401); // queue second Item
			Assert.AreEqual<int>(400, _queue1.DequeueOne());
			Assert.AreEqual<int>(401, _queue1.DequeueOne());
		}

		[TestMethod]
		public void QueueCount()
		{
			_queueSet.Enqueue(500);
			_queueSet.Enqueue(501);
			_queueSet.Enqueue(502);
			_queueSet.Enqueue(503);
			_queueSet.Enqueue(504);
			Assert.AreEqual<int>(5, _queue1.Count);
		}

		[TestMethod]
		public void ExcludedQueueBroadcastById()
		{
		    // This tests the scenario where you want to send a shared item to all queues EXCEPT those identified.
		    _queueSet.EnqueueExclude(600, _queue1.QueueId, _queue2.QueueId);
			Assert.AreEqual<int>(0, _queue1.Count);
			Assert.AreEqual<int>(0, _queue2.Count);
			Assert.AreEqual<int>(1, _queue3.Count);
			Assert.AreEqual<int>(1, _queue4.Count);
		}


		[TestMethod]
		public void ExcludedQueueBroadcastByQueue()
		{
			// This tests the scenario where you want to send a shared item to all queues EXCEPT those identified.
			_queueSet.EnqueueExclude(601, _queue3, _queue4);
			Assert.AreEqual<int>(1, _queue1.Count);
			Assert.AreEqual<int>(1, _queue2.Count);
			Assert.AreEqual<int>(0, _queue3.Count);
			Assert.AreEqual<int>(0, _queue4.Count);
		}

	}
}
