using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Collections.Generic
{
	/// <summary>
	/// Defines a collection if items to be returned in limited size page chunks.
	/// This class includes additional properties that define the scope of the page within the set as a whole.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class PagedList<T>
	{
		/// <summary>
		/// Number of items in the source list.
		/// </summary>
		public int TotalItemCount { get; private set; }

		/// <summary>
		/// Number of total pages.
		/// </summary>
		public int TotalPageCount { get; private set; }

		/// <summary>
		/// Current page number.
		/// </summary>
		public int CurrentPage { get; private set; }

		/// <summary>
		/// List of the items in the current page.
		/// </summary>
		public List<T> CurrentPageItems { get; private set; }

		/// <summary>
		/// Current number of items per page.
		/// </summary>
		public int PageSize { get; private set; }

		/// <summary>
		/// The index of the first item in the entire set (outside of this 'page's scope).
		/// </summary>
		public int FirstItemIndex { get; private set; }

		/// <summary>
		/// The index of the last item in the entire set (outside of this 'page's scope).
		/// </summary>
		public int LastItemIndex { get; private set; }

		/// <summary>
		/// The index of the first page in the entire set.
		/// </summary>
		public int FirstPageIndex { get; private set; }

		/// <summary>
		/// The index of the last page in the entire set.
		/// </summary>
		public int LastPageIndex { get; private set; }

		/// <summary>
		/// Creates a new instance with one "page" containing all the items from the <paramref name="fullList"/>.
		/// </summary>
		/// <param name="fullList">The complete list of items from which to extract one "page".</param>
		public PagedList(IEnumerable<T> fullList) : this(fullList, 0, 0)
		{
		}

		private int _intSkip;

		private PagedList(int totalItemCount, int pageSize, int currentPage)
		{
			if(pageSize < 0)
			{
				throw new ArgumentOutOfRangeException("pageSize", "Value for pageSize must be 0 or higher.");
			}
			currentPage = currentPage > 0 ? currentPage : 1;

			TotalItemCount = totalItemCount;
			PageSize = pageSize == 0 ? TotalItemCount : pageSize;

			if(PageSize > 0)
			{
				TotalPageCount = (TotalItemCount / PageSize) + ((TotalItemCount % PageSize) > 0 ? 1 : 0);
			}
			CurrentPage = currentPage > TotalPageCount ? TotalPageCount : currentPage;

			_intSkip = PageSize * (CurrentPage > 0 ? CurrentPage - 1 : 0);
			FirstItemIndex = _intSkip + 1;
			LastItemIndex = Math.Min(_intSkip + PageSize, TotalItemCount);

			// Initially set the first and last page indexes without using calculations
			FirstPageIndex = 1;
			LastPageIndex = TotalPageCount;
		}

		/// <summary>
		/// Creates a new instance with one "page" of items from the <paramref name="fullList"/> using the specified <paramref name="pageSize"/> and <paramref name="currentPage"/>.
		/// </summary>
		/// <param name="fullList">The complete list of items from which to extract one "page".</param>
		/// <param name="pageSize">The number of items to include on one "page". Can be 0 to return all items.</param>
		/// <param name="currentPage">The number of the "page" to retrieve. Ignored when pageSize = 0.</param>
		public PagedList(IEnumerable<T> fullList, int pageSize, int currentPage) : this(fullList.Count(), pageSize, currentPage)
		{
			CurrentPageItems = fullList.Skip(_intSkip).Take(PageSize).ToList();
		}

		/// <summary>
		/// Creates a new list from a partial list.  This performs the necessary page calculations, but does not select the page subset of items.
		/// Use this constructor for data that is paged at its source (e.g. database).
		/// </summary>
		/// <param name="singlePageItemList">A single page of items.</param>
		/// <param name="pageSize">The number of items in a single page.</param>
		/// <param name="currentPage">The current page of the full list.</param>
		/// <param name="totalItemCount">The total number of items in the full list.</param>
		public PagedList(IEnumerable<T> singlePageItemList, int pageSize, int currentPage, int totalItemCount) : this(totalItemCount, pageSize, currentPage)
		{
			CurrentPageItems = singlePageItemList.ToList();
		}

		/// <summary>
		/// Calculates the start and end page index values for "pager" development
		/// </summary>
		/// <param name="max_displayable_pages">Maximum Displayable Pages</param>
		public void RecalculatePagerIndexes(int max_displayable_pages = 0)
		{
			// Initially set the first and last page indexes without using calculation
			FirstPageIndex = 1;
			LastPageIndex = TotalPageCount;

			// If there are maximum displayable pages and the total page count for this
			// result set is greater than the maximum displayable pages
			if (max_displayable_pages > 0 && TotalPageCount > max_displayable_pages)
			{
				// Set the first available pager index
				FirstPageIndex = (((int)Math.Floor(((decimal)(CurrentPage - 1) / (decimal)max_displayable_pages)) * max_displayable_pages) + 1);

				// Calculate the last available pager index
				LastPageIndex = FirstPageIndex + (max_displayable_pages - 1);

				// If the calculated LastPageIndex is greater than the total page count
				// then set the LastPageIndex to the total page count
				if (LastPageIndex > TotalPageCount)
				{
					LastPageIndex = TotalPageCount;
				}
			}

		}

	}

}
