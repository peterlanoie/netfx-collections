using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable CheckNamespace
namespace System.Collections.Specialized
// ReSharper restore CheckNamespace
{
	/// <summary>
	/// Provides extension methods for an IDictionary.
	/// </summary>
	public static class ExtIDictionary
	{
		/// <summary>
		/// Add a range of dictionary items from the source.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="source"></param>
		public static void AddRange(this IDictionary target, IDictionary source)
		{
			foreach (var key in source.Keys)
			{
				target.Add(key, source[key]);
			}
		}
	}
}
