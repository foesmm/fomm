using System;
using System.Collections.Generic;

namespace Fomm.Util
{
	/// <summary>
	/// A Set implementation.
	/// </summary>
	/// <typeparam name="T">The type of objects in the Set.</typeparam>
	public class Set<T> : List<T>
	{
		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public Set()
		{
		}

		/// <summary>
		/// The copy constructor.
		/// </summary>
		/// <param name="p_setCopy">The set to copy.</param>
		public Set(Set<T> p_setCopy)
			: base(p_setCopy)
		{
		}

		#endregion

		/// <summary>
		/// Adds the given item to the set.
		/// </summary>
		/// <param name="p_tItem">the item to add.</param>
		public new void Add(T p_tItem)
		{
			if (!Contains(p_tItem))
				base.Add(p_tItem);
		}
	}
}
