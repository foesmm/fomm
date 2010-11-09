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
		private IComparer<T> m_cmpComparer = null;

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public Set()
		{
		}

		/// <summary>
		/// A constructor that allows the specification of a custom comparer.
		/// </summary>
		/// <param name="p_cmpComparer">The comparer to use when determining if an item is already in the set.</param>
		public Set(IComparer<T> p_cmpComparer)
		{
			m_cmpComparer = p_cmpComparer;
		}

		/// <summary>
		/// The copy constructor.
		/// </summary>
		/// <param name="p_setCopy">The set to copy.</param>
		public Set(Set<T> p_setCopy)
			: base(p_setCopy)
		{
			m_cmpComparer = p_setCopy.m_cmpComparer;
		}

		#endregion

		/// <summary>
		/// Determines the first index of the specified item.
		/// </summary>
		/// <param name="p_tItem">The item whose index in the set is to be found.</param>
		/// <returns>The first index of the specified item, or -1 if the item is not in the set.</returns>
		public new Int32 IndexOf(T p_tItem)
		{
			return IndexOf(p_tItem, 0);
		}

		/// <summary>
		/// Determines the first index of the specified item.
		/// </summary>
		/// <param name="p_tItem">The item whose index in the set is to be found.</param>
		/// <param name="p_intStartIndex">The zero-based index where to start the search.</param>
		/// <returns>The first index of the specified item, or -1 if the item is not in the set.</returns>
		public new Int32 IndexOf(T p_tItem, Int32 p_intStartIndex)
		{
			if (m_cmpComparer != null)
			{
				for (Int32 i = p_intStartIndex; i < this.Count; i++)
					if (m_cmpComparer.Compare(this[i], p_tItem) == 0)
						return i;
				return -1;
			}
			return base.IndexOf(p_tItem, p_intStartIndex);
		}

		/// <summary>
		/// Determines the last index of the specified item.
		/// </summary>
		/// <param name="p_tItem">The item whose index in the set is to be found.</param>
		/// <returns>The last index of the specified item, or -1 if the item is not in the set.</returns>
		public new Int32 LastIndexOf(T p_tItem)
		{
			return LastIndexOf(p_tItem, Count - 1);
		}

		/// <summary>
		/// Determines the last index of the specified item.
		/// </summary>
		/// <param name="p_tItem">The item whose index in the set is to be found.</param>
		/// <param name="p_intStartIndex">The zero-based index where to start the search.</param>
		/// <returns>The last index of the specified item, or -1 if the item is not in the set.</returns>
		public new Int32 LastIndexOf(T p_tItem, Int32 p_intStartIndex)
		{
			if (m_cmpComparer != null)
			{
				for (Int32 i = p_intStartIndex; i > 0; i++)
					if (m_cmpComparer.Compare(this[i], p_tItem) == 0)
						return i;
				return -1;
			}
			return base.LastIndexOf(p_tItem, p_intStartIndex);
		}

		/// <summary>
		/// Sorts the set.
		/// </summary>
		public new void Sort()
		{
			if (m_cmpComparer != null)
				this.Sort(m_cmpComparer);
			else
				base.Sort();
		}

		/// <summary>
		/// Determines if the given item is in the set.
		/// </summary>
		public new bool Contains(T p_tItem)
		{
			return IndexOf(p_tItem) > -1;
		}

		/// <summary>
		/// Removes the given item from the set.
		/// </summary>
		public new void Remove(T p_tItem)
		{
			if (m_cmpComparer != null)
			{
				for (Int32 i = this.Count - 1; i > 0; i--)
					if (m_cmpComparer.Compare(this[i], p_tItem) == 0)
					{
						RemoveAt(i);
						return;
					}
			}
			else
				base.Remove(p_tItem);
		}

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
