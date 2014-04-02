using System;
using System.Collections.Generic;
using System.Collections;

namespace Fomm.Util
{
  public class SortedList<T> : IList<T>
  {
    private List<T> m_lstItems = new List<T>();
    private IComparer<T> m_cmpComparer;

    public SortedList()
    {
      if (!typeof (IComparable<T>).IsAssignableFrom(typeof (T)))
      {
        throw new ArgumentException("Type " + typeof (T).Name +
                                    " is not IComparable. Use SortedList(IComparer) to supply a comparer.");
      }
    }

    public SortedList(IComparer<T> p_cmpComparer)
    {
      m_cmpComparer = p_cmpComparer;
    }

    #region IList<T> Members

    public int IndexOf(T item)
    {
      if (m_cmpComparer == null)
      {
        return m_lstItems.BinarySearch(item);
      }
      return m_lstItems.BinarySearch(item, m_cmpComparer);
    }

    public void Insert(int index, T item)
    {
      throw new InvalidOperationException("The method or operation cannot be performed on an Ordered List.");
    }

    public void RemoveAt(int index)
    {
      m_lstItems.RemoveAt(index);
    }

    public T this[int index]
    {
      get
      {
        return m_lstItems[index];
      }
      set
      {
        throw new InvalidOperationException("The method or operation cannot be performed on an Ordered List.");
      }
    }

    #endregion

    #region ICollection<T> Members

    public void Add(T item)
    {
      if (m_cmpComparer == null)
      {
        m_lstItems.BinarySearch(item);
      }
      int intIndex = m_lstItems.BinarySearch(item, m_cmpComparer);

      if (intIndex < 0)
      {
        intIndex = ~intIndex;
      }

      m_lstItems.Insert(intIndex, item);
    }

    public void Clear()
    {
      m_lstItems.Clear();
    }

    public bool Contains(T item)
    {
      if (m_cmpComparer == null)
      {
        m_lstItems.BinarySearch(item);
      }
      int intIndex = m_lstItems.BinarySearch(item, m_cmpComparer);
      return intIndex > -1;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      m_lstItems.CopyTo(array, arrayIndex);
    }

    public int Count
    {
      get
      {
        return m_lstItems.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public bool Remove(T item)
    {
      if (m_cmpComparer == null)
      {
        m_lstItems.BinarySearch(item);
      }
      int intIndex = m_lstItems.BinarySearch(item, m_cmpComparer);
      if (intIndex > -1)
      {
        m_lstItems.RemoveAt(intIndex);
        return true;
      }
      return false;
    }

    #endregion

    #region IEnumerable<T> Members

    public IEnumerator<T> GetEnumerator()
    {
      return m_lstItems.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable) m_lstItems).GetEnumerator();
    }

    #endregion
  }
}