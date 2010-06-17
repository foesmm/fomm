using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.Drawing.Design;

namespace Fomm.Controls
{
	/// <summary>
	/// A tab control whose tabs are in a drop down box.
	/// </summary>
	[DefaultProperty("SelectedPage")]
	[DefaultEvent("SelectedIndexChanged")]
	[Designer(typeof(DropDownTabControlDesigner))]
	public class DropDownTabControl : ScrollableControl
	{
		/// <summary>
		/// Raised when the selected tab page index has changed.
		/// </summary>
		[Category("Action")]
		public event EventHandler SelectedIndexChanged
		{
			add
			{
				m_cbxSelector.SelectedIndexChanged += value;
			}
			remove
			{
				m_cbxSelector.SelectedIndexChanged -= value;
			}
		}

		/// <summary>
		/// The event arguments for when a tab page is added or removed from the control.
		/// </summary>
		public class TabPageEventArgs : EventArgs
		{
			private DropDownTabPage m_tpgPage = null;

			#region Properties

			/// <summary>
			/// Gets the tab page that was affected by the event.
			/// </summary>
			/// <value>The tab page that was affected by the event.</value>
			public DropDownTabPage TabPage
			{
				get
				{
					return m_tpgPage;
				}
			}

			#endregion

			#region Constructors

			/// <summary>
			/// A simple consturctor that initializes the object with the given values.
			/// </summary>
			/// <param name="p_tpgPage">The tab page that was affected by the event.</param>
			public TabPageEventArgs(DropDownTabPage p_tpgPage)
			{
				m_tpgPage = p_tpgPage;
			}

			#endregion
		}

		/// <summary>
		/// A collection of <see cref="DropDownTabPage"/>s.
		/// </summary>
		public class TabPageCollection : IList<DropDownTabPage>, ICollection<DropDownTabPage>, IEnumerable<DropDownTabPage>, ICollection, IList
		{
			#region Events

			/// <summary>
			/// Raised when a <see cref="DropDownTabPage"/> is added to the collection.
			/// </summary>
			public event EventHandler<TabPageEventArgs> TabPageAdded;

			/// <summary>
			/// Raised when a <see cref="DropDownTabPage"/> is removed from the collection.
			/// </summary>
			public event EventHandler<TabPageEventArgs> TabPageRemoved;

			#endregion

			private List<DropDownTabPage> m_lstPages = new List<DropDownTabPage>();

			#region Event Raising

			/// <summary>
			/// Raises the <see cref="TabPageAdded"/> event.
			/// </summary>
			/// <param name="p_tpgPage">The <see cref="DropDownTabPage"/> that was added.</param>
			protected void OnTabPageAdded(DropDownTabPage p_tpgPage)
			{
				if (TabPageAdded != null)
					TabPageAdded(this, new TabPageEventArgs(p_tpgPage));
			}

			/// <summary>
			/// Raises the <see cref="TabPageRemoved"/> event.
			/// </summary>
			/// <param name="p_tpgPage">The <see cref="DropDownTabPage"/> that was removed.</param>
			protected void OnTabPageRemoved(DropDownTabPage p_tpgPage)
			{
				if (TabPageRemoved != null)
					TabPageRemoved(this, new TabPageEventArgs(p_tpgPage));
			}

			#endregion

			#region IList<DropDownTabPage> Members

			/// <seealso cref="IList{DropDownTabPage}.IndexOf"/>
			public int IndexOf(DropDownTabPage item)
			{
				return m_lstPages.IndexOf(item);
			}

			/// <seealso cref="IList{DropDownTabPage}.Insert"/>
			public void Insert(int index, DropDownTabPage item)
			{
				m_lstPages.Insert(index, item);
				OnTabPageAdded(item);
			}

			/// <seealso cref="IList{DropDownTabPage}.RemoveAt"/>
			public void RemoveAt(int index)
			{
				DropDownTabPage tpgPage = m_lstPages[index];
				m_lstPages.RemoveAt(index);
				OnTabPageRemoved(tpgPage);
			}

			/// <seealso cref="IList{DropDownTabPage}.this"/>
			public DropDownTabPage this[int index]
			{
				get
				{
					return m_lstPages[index];
				}
				set
				{
					m_lstPages[index] = value;
				}
			}

			#endregion

			#region ICollection<DropDownTabPage> Members

			/// <seealso cref="ICollection{DropDownTabPage}.Add"/>
			public void Add(DropDownTabPage item)
			{
				m_lstPages.Add(item);
				OnTabPageAdded(item);
			}

			/// <seealso cref="ICollection{DropDownTabPage}.Clear"/>
			public void Clear()
			{
				DropDownTabPage tpgPage = null;
				for (Int32 i = m_lstPages.Count - 1; i >= 0; i--)
				{
					tpgPage = m_lstPages[i];
					RemoveAt(i);
				}
			}

			/// <seealso cref="ICollection{DropDownTabPage}.Contains"/>
			public bool Contains(DropDownTabPage item)
			{
				return m_lstPages.Contains(item);
			}

			/// <seealso cref="ICollection{DropDownTabPage}.CopyTo"/>
			public void CopyTo(DropDownTabPage[] array, int arrayIndex)
			{
				m_lstPages.CopyTo(array, arrayIndex);
			}

			/// <seealso cref="ICollection{DropDownTabPage}.Count"/>
			public int Count
			{
				get
				{
					return m_lstPages.Count;
				}
			}

			/// <seealso cref="ICollection{DropDownTabPage}.IsReadOnly"/>
			public bool IsReadOnly
			{
				get
				{
					return false;
				}
			}

			/// <seealso cref="ICollection{DropDownTabPage}.Remove"/>
			public bool Remove(DropDownTabPage item)
			{
				if (m_lstPages.Remove(item))
				{
					OnTabPageRemoved(item);
					return true;
				}
				return false;
			}

			#endregion

			#region IEnumerable<DropDownTabPage> Members

			/// <seealso cref="IEnumerator{DropDownTabPage}.GetEnumerator"/>
			public IEnumerator<DropDownTabPage> GetEnumerator()
			{
				return m_lstPages.GetEnumerator();
			}

			#endregion

			#region IEnumerable Members

			/// <seealso cref="IEnumerator.GetEnumerator"/>
			IEnumerator IEnumerable.GetEnumerator()
			{
				return m_lstPages.GetEnumerator();
			}

			#endregion

			#region ICollection Members

			/// <seealso cref="ICollection.CopyTo"/>
			public void CopyTo(Array array, int index)
			{
				if (array == null)
					throw new ArgumentNullException("array is null.");
				if (index < 0)
					throw new ArgumentOutOfRangeException("index", index, "index is less than 0.");
				if (array.Length - index < m_lstPages.Count)
					throw new ArgumentException("Insufficient space in target array.");
				for (Int32 i = index; i < m_lstPages.Count + index; i++)
					array.SetValue(m_lstPages[i - index], i);
			}

			/// <seealso cref="ICollection.IsSynchronized"/>
			public bool IsSynchronized
			{
				get
				{
					return false;
				}
			}

			/// <seealso cref="ICollection.SyncRoot"/>
			public object SyncRoot
			{
				get
				{
					return this;
				}
			}

			#endregion

			#region IList Members

			/// <seealso cref="IList.Add"/>
			public int Add(object value)
			{
				DropDownTabPage vtpPage = value as DropDownTabPage;
				if (vtpPage == null)
					throw new ArgumentException(String.Format("Cannot add item of type '{0}'. Expecting '{1}'.", value.GetType(), typeof(DropDownTabPage)), "value");
				Add(vtpPage);
				return Count - 1;
			}

			/// <seealso cref="IList.Contains"/>
			public bool Contains(object value)
			{
				DropDownTabPage vtpPage = value as DropDownTabPage;
				if (vtpPage == null)
					return false;
				return Contains(vtpPage);
			}

			/// <seealso cref="IList.IndexOf"/>
			public int IndexOf(object value)
			{
				DropDownTabPage vtpPage = value as DropDownTabPage;
				if (vtpPage == null)
					return -1;
				return IndexOf(vtpPage);
			}

			/// <seealso cref="IList.Insert"/>
			public void Insert(int index, object value)
			{
				DropDownTabPage vtpPage = value as DropDownTabPage;
				if (vtpPage == null)
					throw new ArgumentException(String.Format("Cannot insert item of type '{0}'. Expecting '{1}'.", value.GetType(), typeof(DropDownTabPage)), "value");
				Insert(index, vtpPage);
			}

			/// <seealso cref="IList.IsFixedSize"/>
			public bool IsFixedSize
			{
				get
				{
					return false;
				}
			}

			/// <seealso cref="IList.Remove"/>
			public void Remove(object value)
			{
				DropDownTabPage vtpPage = value as DropDownTabPage;
				if (vtpPage != null)
					Remove(vtpPage);
			}

			/// <seealso cref="IList.this"/>
			object IList.this[int index]
			{
				get
				{
					return this[index];
				}
				set
				{
					DropDownTabPage vtpPage = value as DropDownTabPage;
					if (vtpPage == null)
						throw new ArgumentException(String.Format("Cannot set item of type '{0}'. Expecting '{1}'.", value.GetType(), typeof(DropDownTabPage)), "value");
					this[index] = vtpPage;
				}
			}

			#endregion
		}

		private Panel m_pnlDropDownPanel = null;
		private ComboBox m_cbxSelector = null;
		private Label m_lblLabel = null;
		private TabPageCollection m_tpcPages = null;
		private DropDownTabPage m_tpgSelected = null;

		#region Properties

		/// <summary>
		/// Gets the tab selector combobox.
		/// </summary>
		/// <value>The tab selector combobox.</value>
		internal ComboBox TabSelector
		{
			get
			{
				return m_cbxSelector;
			}
		}

		/// <summary>
		/// Gets or sets the text of the tab selector.
		/// </summary>
		public override string Text
		{
			get
			{
				return m_lblLabel.Text;
			}
			set
			{
				m_lblLabel.Text = value;
			}
		}

		/// <summary>
		/// Gets the tab pages of this control.
		/// </summary>
		/// <value>The tab pages of this control.</value>
		[Editor(typeof(DropDownTabPageCollectionEditor), typeof(UITypeEditor))]
		public TabPageCollection TabPages
		{
			get
			{
				return m_tpcPages;
			}
		}

		/// <summary>
		/// Gets or sets the currently selected tab page.
		/// </summary>
		/// <value>The currently selected tab page.</value>
		[TypeConverter(typeof(SelectedDropDownTabPageConverter))]
		public DropDownTabPage SelectedTabPage
		{
			get
			{
				return m_tpgSelected;
			}
			set
			{
				if (m_tpgSelected == value)
					return;
				m_tpgSelected = value;
				if (m_tpgSelected != null)
				{
					m_tpgSelected.BringToFront();
					m_cbxSelector.SelectedItem = m_tpgSelected;
				}
			}
		}

		/// <summary>
		/// Gets or sets the index of the currently selected tab page.
		/// </summary>
		/// <value>The index of the currently selected tab page.</value>
		[Browsable(false)]
		public Int32 SelectedIndex
		{
			get
			{
				return this.TabPages.IndexOf(SelectedTabPage);
			}
			set
			{
				if (value == -1)
					SelectedTabPage = null;
				else
					SelectedTabPage = TabPages[value];
			}
		}

		/// <summary>
		/// Gets or sets the width of the tabs.
		/// </summary>
		/// <value>The width of the tabs.</value>
		[Category("Appearance")]
		[DefaultValue(150)]
		public Int32 TabWidth
		{
			get
			{
				return m_cbxSelector.Width;
			}
			set
			{
				m_cbxSelector.Width = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[DefaultValue(KnownColor.Window)]
		public override Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public DropDownTabControl()
		{
			m_tpcPages = new TabPageCollection();
			m_tpcPages.TabPageAdded += new EventHandler<TabPageEventArgs>(AddTabPage);
			m_tpcPages.TabPageRemoved += new EventHandler<TabPageEventArgs>(RemoveTabPage);

			m_pnlDropDownPanel = new Panel();
			m_pnlDropDownPanel.Dock = DockStyle.Top;
			m_pnlDropDownPanel.DataBindings.Add("BackColor", this, "BackColor");

			m_lblLabel = new Label();
			m_lblLabel.AutoSize = true;
			m_lblLabel.Text = Name;
			m_lblLabel.Location = new Point(3, 3);

			m_cbxSelector = new ComboBox();
			m_cbxSelector.Location = new Point(13, m_lblLabel.Top + 13 + 4);
			m_cbxSelector.DisplayMember = "Text";
			m_cbxSelector.SelectedIndexChanged += new EventHandler(TabSelected);

			m_pnlDropDownPanel.Height = m_cbxSelector.Location.Y + m_cbxSelector.Height + 4;
			m_pnlDropDownPanel.Controls.Add(m_lblLabel);
			m_pnlDropDownPanel.Controls.Add(m_cbxSelector);
			Controls.Add(m_pnlDropDownPanel);
		}

		#endregion

		/// <summary>
		/// Raises the <see cref="Control.CreateControl"/> event.
		/// </summary>
		/// <remarks>
		/// I can't get the <see cref="ComboBox"/> to be interactive in design mode when its
		/// style is set to <see cref="ComboBoxStyle.DropDownList"/>, so only set the style
		/// if we aren't designing.
		/// </remarks>
		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			if (!DesignMode)
				m_cbxSelector.DropDownStyle = ComboBoxStyle.DropDownList;
		}


		/// <summary>
		/// Handles the <see cref="DropDownTabButton.Selected"/> event of the tabs.
		/// </summary>
		/// <remarks>
		/// This sets the <see cref="DropDownTabButton"/> associated with the tab
		/// that was clicked as the <see cref="SelectedTabPage"/>.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		protected void TabSelected(object sender, EventArgs e)
		{

			SelectedTabPage = (DropDownTabPage)m_cbxSelector.SelectedItem;
		}

		/// <summary>
		/// Handles the <see cref="TabPageCollection.TabPageAdded"/> event of this
		/// control's collection of <see cref="DropDownTabPages"/>.
		/// </summary>
		/// <remarks>
		/// This wires the added tab page into the control, and adds it to the <see cref="Controls"/>
		/// collection.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="DropDownTabControl.TabPageEventArgs"/> describing the event arguments.</param>
		private void AddTabPage(object sender, DropDownTabControl.TabPageEventArgs e)
		{
			DropDownTabPage ctlPage = e.TabPage;
			if (ctlPage.PageIndex == -1)
				ctlPage.PageIndex = m_tpcPages.Count - 1;
			if (!m_tpcPages.Contains(ctlPage))
				m_tpcPages.Add(ctlPage);
			ctlPage.PageIndexChanged += new EventHandler(PageIndexChanged);
			ctlPage.TextChanged += new EventHandler(PageTextChanged);
			InsertTabPageInSelector(ctlPage);
			ctlPage.Dock = DockStyle.Fill;
			Controls.Add(e.TabPage);
			SelectedTabPage = ctlPage;
		}

		/// <summary>
		/// Handles the <see cref="TabPageCollection.TabPageRemoved"/> event of this
		/// control's collection of <see cref="DropDownTabPages"/>.
		/// </summary>
		/// <remarks>
		/// This unwires the tab page from the control, and removes it to the <see cref="Controls"/>
		/// collection.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="DropDownTabControl.TabPageEventArgs"/> describing the event arguments.</param>
		private void RemoveTabPage(object sender, DropDownTabControl.TabPageEventArgs e)
		{
			DropDownTabPage ctlPage = e.TabPage;
			ctlPage.PageIndexChanged -= new EventHandler(PageIndexChanged);
			ctlPage.TextChanged -= new EventHandler(PageTextChanged);
			m_cbxSelector.Items.Remove(ctlPage);
			for (Int32 i = 0; i < m_tpcPages.Count; i++)
				if (m_tpcPages[i].PageIndex > ctlPage.PageIndex)
					m_tpcPages[i].PageIndex--;
			if (SelectedTabPage == ctlPage)
			{
				if (m_tpcPages.Count == 0)
					SelectedTabPage = null;
				else if (SelectedIndex == m_tpcPages.Count)
					SelectedIndex--;
				else
					SelectedIndex++;
			}
			Controls.Remove(e.TabPage);
		}

		/// <summary>
		/// Raises the <see cref="Control.ControlAdded"/> event.
		/// </summary>
		/// <remarks>
		/// This ensures that any <see cref="DropDownTabPage"/>s added to this control are added
		/// from the <see cref="TabPages"/> collection.
		/// </remarks>
		/// <param name="e">A <see cref="ControlEventArgs"/> describing the event arguments.</param>
		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);
			if (e.Control is DropDownTabPage)
			{
				DropDownTabPage ctlPage = (DropDownTabPage)e.Control;
				if (!m_tpcPages.Contains(ctlPage))
					m_tpcPages.Add(ctlPage);
			}
		}

		/// <summary>
		/// Raises the <see cref="Control.ControlAdded"/> event.
		/// </summary>
		/// <remarks>
		/// This ensures that any <see cref="DropDownTabPage"/>s removed from this control are removed
		/// from the <see cref="TabPages"/> collection.
		/// </remarks>
		/// <param name="e">A <see cref="ControlEventArgs"/> describing the event arguments.</param>
		protected override void OnControlRemoved(ControlEventArgs e)
		{
			base.OnControlRemoved(e);
			if (e.Control is DropDownTabPage)
			{
				DropDownTabPage ctlPage = (DropDownTabPage)e.Control;
				m_tpcPages.Remove(ctlPage);
			}
		}

		/// <summary>
		/// Inserts the given <see cref="DropDownTabPage"/> into the selector drop box at the correct index based on the
		/// tab's <see cref="DropDownTabPage.PageIndex"/>.
		/// </summary>
		/// <param name="p_ddpPage">The <see cref="DropDownTabPage"/> to insert.</param>
		protected void InsertTabPageInSelector(DropDownTabPage p_ddpPage)
		{
			DropDownTabPage ddpCurrent = null;
			for (Int32 i = 0; i < m_cbxSelector.Items.Count; i++)
			{
				ddpCurrent = (DropDownTabPage)m_cbxSelector.Items[i];
				if (ddpCurrent.PageIndex > p_ddpPage.PageIndex)
				{
					m_cbxSelector.Items.Insert(i, p_ddpPage);
					return;
				}
			}
			m_cbxSelector.Items.Add(p_ddpPage);
		}

		/// <summary>
		/// This updates the items int he selector combo box to reflect changes in tab page
		/// properties.
		/// </summary>
		protected void UpdateSelector()
		{
			m_cbxSelector.BeginUpdate();
			m_cbxSelector.Items.Clear();
			foreach (DropDownTabPage ddpPage in m_tpcPages)
				InsertTabPageInSelector(ddpPage);
			m_cbxSelector.SelectedItem = SelectedTabPage;
			m_cbxSelector.EndUpdate();
		}

		/// <summary>
		/// Handles the <see cref="DropDownTabPage.PageIndexChanged"/>
		/// </summary>
		/// <remarks>
		/// This reorders the items in the selector combo box to match the new page order.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void PageIndexChanged(object sender, EventArgs e)
		{
			UpdateSelector();
		}

		/// <summary>
		/// Handles the <see cref="DropDownTabPage.PageIndexChanged"/>
		/// </summary>
		/// <remarks>
		/// This reorders the items in the selector combo box to match the new page order.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void PageTextChanged(object sender, EventArgs e)
		{
			UpdateSelector();
		}
	}
}
