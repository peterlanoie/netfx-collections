using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Collections.Generic;

namespace Common.Collections.Tests
{
	[TestClass]
	public class PagedListTests
	{

		[TestMethod]
		public void EvenListComputedValues()
		{
			PagedList<int> list = new PagedList<int>(GenerateTestList(100), 10, 1);
			Assert.IsTrue(list.CurrentPage == 1);
			Assert.IsTrue(list.PageSize == 10);
			Assert.IsTrue(list.TotalItemCount == 100);
			Assert.IsTrue(list.TotalPageCount == 10);
		}

		[TestMethod]
		public void UnevenListComputedValues()
		{
			PagedList<int> list = new PagedList<int>(GenerateTestList(105), 10, 5);
			Assert.IsTrue(list.CurrentPage == 5);
			Assert.IsTrue(list.PageSize == 10);
			Assert.IsTrue(list.TotalItemCount == 105);
			Assert.IsTrue(list.TotalPageCount == 11);
		}

		[TestMethod]
		public void EvenListContent()
		{
			PagedList<int> list = new PagedList<int>(GenerateTestList(100), 10, 1);
			Assert.IsTrue(list.CurrentPageItems.Count == 10);
			Assert.AreEqual<int>(1, list.CurrentPageItems.First());
			Assert.AreEqual<int>(10, list.CurrentPageItems.Last());
		}

		[TestMethod]
		public void UnevenListContent()
		{
			PagedList<int> list = new PagedList<int>(GenerateTestList(105), 10, 11);
			Assert.IsTrue(list.CurrentPageItems.Count == 5);
			Assert.AreEqual<int>(101, list.CurrentPageItems.First());
			Assert.AreEqual<int>(105, list.CurrentPageItems.Last());
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void BadPageSize()
		{
			PagedList<int> list = new PagedList<int>(GenerateTestList(105), -1, 11);
		}

		[TestMethod]
		public void LastPageRecomputation()
		{
			PagedList<int> list = new PagedList<int>(GenerateTestList(100), 10, 11);
			Assert.AreEqual<int>(10, list.CurrentPage);
			Assert.AreEqual<int>(91, list.CurrentPageItems.First());
			Assert.AreEqual<int>(100, list.CurrentPageItems.Last());
		}

		[TestMethod]
		public void CurrentPageRecomputation()
		{
			PagedList<int> list = new PagedList<int>(GenerateTestList(100), 10, -1);
			Assert.AreEqual<int>(1, list.CurrentPage);
			Assert.AreEqual<int>(1, list.CurrentPageItems.First());
			Assert.AreEqual<int>(10, list.CurrentPageItems.Last());
		}


		[TestMethod]
		public void AllItemsReturn()
		{
			PagedList<int> list = new PagedList<int>(GenerateTestList(100), 0, 1);
			Assert.IsTrue(list.CurrentPage == 1);
			Assert.IsTrue(list.PageSize == 100);
			Assert.IsTrue(list.TotalItemCount == 100);
			Assert.IsTrue(list.TotalPageCount == 1);
			Assert.AreEqual<int>(1, list.CurrentPageItems.First());
			Assert.AreEqual<int>(100, list.CurrentPageItems.Last());
		}


		[TestMethod]
		public void ExtensionMethod()
		{
			object test = GenerateTestList(105).AsPagedList(10, 1);
			Assert.IsInstanceOfType(test, typeof(PagedList<int>));
		}

		[TestMethod]
		public void CreatePageForPartialList()
		{
			PagedList<int> list = new PagedList<int>(GenerateTestList(15, 16), 15, 2, 15000);
			Assert.IsTrue(list.CurrentPage == 2);
			Assert.IsTrue(list.CurrentPageItems.Count == 15);
			Assert.IsTrue(list.FirstItemIndex == 16);
			Assert.IsTrue(list.LastItemIndex == 30);
			Assert.IsTrue(list.PageSize == 15);
			Assert.IsTrue(list.TotalItemCount == 15000);
			Assert.IsTrue(list.TotalPageCount == 1000);
		}

		private List<int> GenerateTestList(int count, int startIndex = 1)
		{
			List<int> list = new List<int>();

			for(int i = 0; i < count; i++)
			{
				list.Add(i + startIndex);
			}

			return list;
		}

	}
}
