using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace Fomm.Controls
{
  /// <summary>
  /// A treeview that allows the selection of multiple nodes.
  /// </summary>
  public class MultiSelectTreeView : TreeView
  {
    /// <summary>
    /// A collections of <see cref="TreeNode"/>s.
    /// </summary>
    /// <remarks>
    /// This collection has events that can notify listeners of changes.
    /// </remarks>
    public class TreeNodeCollection : IList<TreeNode>
    {
      /// <summary>
      /// Describes the arguments of event that affect a <see cref="TreeNode"/>.
      /// </summary>
      public class TreeNodeEventArgs : EventArgs
      {
        private TreeNode m_tndNode = null;

        #region Properties

        /// <summary>
        /// Gets the <see cref="TreeNode"/> affected by the event.
        /// </summary>
        /// <value>The <see cref="TreeNode"/> affected by the event.</value>
        public TreeNode TreeNode
        {
          get
          {
            return m_tndNode;
          }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// A simple constructor that initializes the object with the given values.
        /// </summary>
        /// <param name="p_tndNode">The <see cref="TreeNode"/> affected by the event.</param>
        public TreeNodeEventArgs(TreeNode p_tndNode)
        {
          m_tndNode = p_tndNode;
        }

        #endregion
      }

      private LinkedList<TreeNode> m_lklNodes = new LinkedList<TreeNode>();

      #region Events

      public event EventHandler<TreeNodeEventArgs> ItemRemoved = delegate { };
      public event EventHandler<TreeNodeEventArgs> ItemAdded = delegate { };

      /// <summary>
      /// Raises the <see cref="ItemRemoved"/> event.
      /// </summary>
      /// <param name="p_tndNode">The <see cref="TreeNode"/> that was removed.</param>
      protected void OnItemRemoved(TreeNode p_tndNode)
      {

        ItemRemoved(this, new TreeNodeEventArgs(p_tndNode));
      }

      /// <summary>
      /// Raises the <see cref="OnItemAdded"/> event.
      /// </summary>
      /// <param name="p_tndNode">The <see cref="TreeNode"/> that was added.</param>
      protected void OnItemAdded(TreeNode p_tndNode)
      {
        ItemAdded(this, new TreeNodeEventArgs(p_tndNode));
      }

      #endregion

      /// <summary>
      /// Adds all of the given <see cref="TreeNode"/>s to the collection.
      /// </summary>
      /// <param name="p_enmNodes">The <see cref="TreeNode"/>s to add to the collection.</param>
      public void AddRange(IEnumerable<TreeNode> p_enmNodes)
      {
        foreach (TreeNode tndNode in p_enmNodes)
        {
          m_lklNodes.AddLast(tndNode);
          OnItemAdded(tndNode);
        }
      }

      #region IList<TreeNode> Members

      /// <summary>
      /// Gets the index of the given item in the collection.
      /// </summary>
      /// <param name="item">The item whose index is to be determined.</param>
      /// <returns>The index of the given item in the collection if it is in the collection;
      /// -1 otherwise.</returns>
      public int IndexOf(TreeNode item)
      {
        LinkedListNode<TreeNode> lndNode = m_lklNodes.First;
        for (Int32 i = 0; lndNode != null; i++, lndNode = lndNode.Next)
          if (lndNode.Value == item)
            return i;
        return -1;
      }

      /// <summary>
      /// Inserts the given item into the collection at the given index.
      /// </summary>
      /// <param name="index">The index at which to insert the item.</param>
      /// <param name="item">The item to insert.</param>
      /// <exception cref="IndexOutOfRangeException">Thrown if the given index is less than 0
      /// or greater than <see cref="Count"/>.</exception>
      public void Insert(int index, TreeNode item)
      {
        if ((index < 0) || (index > m_lklNodes.Count))
          throw new IndexOutOfRangeException("Index " + index + " is out of range.");
        LinkedListNode<TreeNode> lndNode = m_lklNodes.First;
        for (Int32 i = 0; lndNode != null && i < index; i++, lndNode = lndNode.Next) ;
        if (lndNode == null)
          m_lklNodes.AddLast(item);
        else
          m_lklNodes.AddBefore(lndNode, item);
        OnItemAdded(item);
      }

      /// <summary>
      /// Removes the item at the given index.
      /// </summary>
      /// <param name="index">The index of the item to remove.</param>
      /// <exception cref="IndexOutOfRangeException">Thrown if the given index is less than 0
      /// or greater than or equal to <see cref="Count"/>.</exception>
      public void RemoveAt(int index)
      {
        if ((index < 0) || (index >= m_lklNodes.Count))
          throw new IndexOutOfRangeException("Index " + index + " is out of range.");
        LinkedListNode<TreeNode> lndNode = m_lklNodes.First;
        for (Int32 i = 0; lndNode != null && i < index; i++, lndNode = lndNode.Next) ;
        if (lndNode != null)
        {
          m_lklNodes.Remove(lndNode);
          OnItemRemoved(lndNode.Value);
        }
      }

      /// <summary>
      /// Gets or sets the item at the given index.
      /// </summary>
      /// <param name="index">The index of the item to get or set.</param>
      /// <returns>The index of the item at the specified index.</returns>
      /// <exception cref="IndexOutOfRangeException">Thrown if the given index is less than 0
      /// or greater than or equal to <see cref="Count"/>.</exception>
      public TreeNode this[int index]
      {
        get
        {
          if ((index < 0) || (index >= m_lklNodes.Count))
            throw new IndexOutOfRangeException("Index " + index + " is out of range.");
          LinkedListNode<TreeNode> lndNode = m_lklNodes.First;
          for (Int32 i = 0; lndNode != null && i < index; i++, lndNode = lndNode.Next) ;
          return lndNode.Value;
        }
        set
        {

          if ((index < 0) || (index >= m_lklNodes.Count))
            throw new IndexOutOfRangeException("Index " + index + " is out of range.");
          LinkedListNode<TreeNode> lndNode = m_lklNodes.First;
          for (Int32 i = 0; lndNode != null && i < index; i++, lndNode = lndNode.Next) ;
          if (value == null)
            OnItemRemoved(lndNode.Value);
          lndNode.Value = value;
          if (value != null)
            OnItemAdded(value);
        }
      }

      #endregion

      #region ICollection<TreeNode> Members

      /// <summary>
      /// Adds the given item to the end of the collection.
      /// </summary>
      /// <param name="item">The item to add.</param>
      public void Add(TreeNode item)
      {
        m_lklNodes.AddLast(item);
        OnItemAdded(item);
      }

      /// <summary>
      /// Empties the collection.
      /// </summary>
      public void Clear()
      {
        TreeNode tndNode = null;
        while (m_lklNodes.First != null)
        {
          tndNode = m_lklNodes.First.Value;
          m_lklNodes.RemoveFirst();
          OnItemRemoved(tndNode);
        }
      }

      /// <summary>
      /// Determines if the given item is in the collection.
      /// </summary>
      /// <param name="item">The item whose presence in the collection is to be determined.</param>
      /// <returns><lang cref="true"/> if the item is in the collection;
      /// <lang cref="false"/> otherwise.</returns>
      public bool Contains(TreeNode item)
      {
        return (IndexOf(item) > -1);
      }

      /// <summary>
      /// Copies the contents of this collection to the given array starting at the specified index.
      /// </summary>
      /// <param name="array">The array into which to copy the contents of this collection.</param>
      /// <param name="arrayIndex">The index in the given array at which to begin copying.</param>
      /// <exception cref="ArgumentException">Thrown if the number of elements in the collection
      /// is greater than the available space from <paramref name="arrayIndex"/> to the end of
      /// the destination array.</exception>
      public void CopyTo(TreeNode[] array, int arrayIndex)
      {
        if (arrayIndex + m_lklNodes.Count > array.Length)
          throw new ArgumentException("Given array is too small");
        LinkedListNode<TreeNode> lndNode = m_lklNodes.First;
        for (Int32 i = arrayIndex; lndNode != null; i++, lndNode = lndNode.Next)
          array[i] = lndNode.Value;
      }

      /// <summary>
      /// Gets the number of items in the collection.
      /// </summary>
      /// <value>The number of items in the collection.</value>
      public int Count
      {
        get
        {
          return m_lklNodes.Count;
        }
      }

      /// <summary>
      /// Gets whether the collection is readonly.
      /// </summary>
      /// <value>Whether the collection is readonly.</value>
      public bool IsReadOnly
      {
        get
        {
          return false;
        }
      }

      /// <summary>
      /// Removes the given item from the collection.
      /// </summary>
      /// <param name="item">The item to remove from the collection.</param>
      /// <returns><lang cref="true"/> if the item was removed from the collection;
      /// <lang cref="false"/> if the item couldn't be removed because it was not in the collection.</returns>
      public bool Remove(TreeNode item)
      {
        LinkedListNode<TreeNode> lndNode = m_lklNodes.First;
        for (Int32 i = 0; lndNode != null; i++, lndNode = lndNode.Next)
          if (lndNode.Value == item)
            break;
        if (lndNode != null)
        {
          m_lklNodes.Remove(lndNode);
          OnItemRemoved(lndNode.Value);
        }
        return (lndNode != null);
      }

      #endregion

      #region IEnumerable<TreeNode> Members

      public IEnumerator<TreeNode> GetEnumerator()
      {
        return m_lklNodes.GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return m_lklNodes.GetEnumerator();
      }

      #endregion
    }

    private TreeNodeCollection m_tncSelectedNodes = null;
    private TreeNode m_tndFirst = null;

    #region Properties

    /// <summary>
    /// Gets the selected <see cref="TreeNode"/>s.
    /// </summary>
    /// <value>The selected <see cref="TreeNode"/>s.</value>
    public TreeNodeCollection SelectedNodes
    {
      get
      {
        return m_tncSelectedNodes;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public MultiSelectTreeView()
    {
      DoubleBuffered = true;
      m_tncSelectedNodes = new TreeNodeCollection();
      m_tncSelectedNodes.ItemAdded += new EventHandler<TreeNodeCollection.TreeNodeEventArgs>(m_tncSelectedNodes_ItemAdded);
      m_tncSelectedNodes.ItemRemoved += new EventHandler<TreeNodeCollection.TreeNodeEventArgs>(m_tncSelectedNodes_ItemRemoved);
    }

    #endregion

    /// <summary>
    /// Handles the <see cref="TreeNodeCollection.ItemRemoved"/> event of the selected nodes collection.
    /// </summary>
    /// <remarks>
    /// This unhighlights the <see cref="TreeNode"/> that was removed from the selected nodes collection.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A <see cref="TreeNodeCollection.TreeNodeEventArgs"/> describing the event arguments.</param>
    private void m_tncSelectedNodes_ItemRemoved(object sender, MultiSelectTreeView.TreeNodeCollection.TreeNodeEventArgs e)
    {
      e.TreeNode.BackColor = Color.Empty;
      e.TreeNode.ForeColor = Color.Empty;
    }

    /// <summary>
    /// Handles the <see cref="TreeNodeCollection.ItemAdded"/> event of the selected nodes collection.
    /// </summary>
    /// <remarks>
    /// This highlights the <see cref="TreeNode"/> that was added to the selected nodes collection.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A <see cref="TreeNodeCollection.TreeNodeEventArgs"/> describing the event arguments.</param>
    private void m_tncSelectedNodes_ItemAdded(object sender, MultiSelectTreeView.TreeNodeCollection.TreeNodeEventArgs e)
    {
      e.TreeNode.BackColor = SystemColors.Highlight;
      e.TreeNode.ForeColor = SystemColors.HighlightText;
    }

    /// <summary>
    /// Raises the <see cref="Control.MouseDown"/> event.
    /// </summary>
    /// <remarks>
    /// This prevents the base <see cref="TreeView"/> from doing any node highlighting.
    /// </remarks>
    /// <param name="e">A <see cref="MouseEventArgs"/> describing the event arguments.</param>
    protected override void OnMouseDown(MouseEventArgs e)
    {
      base.SelectedNode = null;
      base.OnMouseDown(e);
    }

    /// <summary>
    /// Raises the <see cref="TreeView.BeforeSelect"/> event.
    /// </summary>
    /// <remarks>
    /// This handles unselecting nodes as required if the Ctrl key is being pushed, as well
    /// as trcking the beginning of Shft-Click selections.
    /// </remarks>
    /// <param name="e">A <see cref="TreeViewCancelEventArgs"/> describing the event arguments.</param>
    protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
    {
      base.OnBeforeSelect(e);
      if (((ModifierKeys & Keys.Control) > 0) && m_tncSelectedNodes.Contains(e.Node))
      {
        SelectedNodes.Remove(e.Node);
        e.Cancel = true;
      }

      if (ModifierKeys != Keys.Shift)
        m_tndFirst = e.Node;
    }

    /// <summary>
    /// Raises the <see cref="TreeView.AfterSelect"/> event.
    /// </summary>
    /// <remarks>
    /// This handles the selection and unselection of nodes.
    /// </remarks>
    /// <param name="e">A <see cref="TreeViewEventArgs"/> describing the event arguments.</param>
    protected override void OnAfterSelect(TreeViewEventArgs e)
    {
      if (((ModifierKeys & Keys.Control) > 0))
      {
        if (!SelectedNodes.Contains(e.Node))
          SelectedNodes.Add(e.Node);
        else
          SelectedNodes.Remove(e.Node);
      }
      else
      {
        if (((ModifierKeys & Keys.Shift) > 0) && (m_tndFirst != null))
        {
          SelectedNodes.Clear();
          SelectedNodes.AddRange(FindPath(m_tndFirst, e.Node));
        }
        else
        {
          if (SelectedNodes.Count > 0)
            SelectedNodes.Clear();
          SelectedNodes.Add(e.Node);
        }
      }
      base.OnAfterSelect(e);
    }

    /// <summary>
    /// This finds the list of nodes visible between the two given node.
    /// </summary>
    /// <param name="p_tndStart">The node at the start of the path.</param>
    /// <param name="p_tndEnd">The node at the end of the path.</param>
    /// <returns>The list of nodes visible between the two given node.</returns>
    protected List<TreeNode> FindPath(TreeNode p_tndStart, TreeNode p_tndEnd)
    {
      List<TreeNode> lstPath = new List<TreeNode>();
      TreeNode tndPathNode = p_tndStart;
      while ((tndPathNode != null) && (tndPathNode != p_tndEnd))
      {
        lstPath.Add(tndPathNode);
        tndPathNode = tndPathNode.NextVisibleNode;
      }
      if (tndPathNode == null)
      {
        lstPath.Clear();
        tndPathNode = p_tndStart;
        while ((tndPathNode != null) && (tndPathNode != p_tndEnd))
        {
          lstPath.Add(tndPathNode);
          tndPathNode = tndPathNode.PrevVisibleNode;
        }
      }
      lstPath.Add(p_tndEnd);
      return lstPath;
    }
  }
}