using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Collections.Generic;

namespace Common.Collections.Generic
{
	/// <summary>
	/// Defines extension methods for IEnumerable.
	/// </summary>
	public static class ExtIEnumerable
	{

		/// <summary>
		/// Returns the IEnumerable collection as a <see cref="Common.Collections.Generic.PagedList{T}"/>.
		/// </summary>
		/// <typeparam name="T">Type of item in the collection.</typeparam>
		/// <param name="list">Source list.</param>
		/// <param name="pageSize">Number of items per page.</param>
		/// <param name="currentPage">Page number to return.</param>
		/// <returns></returns>
		public static PagedList<T> AsPagedList<T>(this IEnumerable<T> list, int pageSize, int currentPage)
		{
			return new PagedList<T>(list, pageSize, currentPage);
		}

		/// <summary>
		/// Returns the IEnumerable collection as a <see cref="Common.Collections.Generic.PagedList{T}"/>.
		/// </summary>
		/// <typeparam name="T">Type of item in the collection.</typeparam>
		/// <param name="list">Source list.</param>
		/// <returns></returns>
		public static PagedList<T> AsPagedList<T>(this IEnumerable<T> list)
		{
			return new PagedList<T>(list);
		}

	}
}
