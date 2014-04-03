using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Fomm.Properties;
using Fomm.Util;

namespace Fomm.Controls
{
  /// <summary>
  /// This is a tool strip control.
  /// </summary>
  /// <remarks>
  /// It is implemented as a simple panel to allow the use of any control as
  /// tool strip "buttons."
  /// </remarks>
  public class PanelToolStrip : Panel
  {
    /// <summary>
    /// The directions the toolstrip can be oriented.
    /// </summary>
    public enum Orientation
    {
      Horizontal,
      Vertical
    }

    #region Region Internal ToolStripPanel Class

    /// <summary>
    /// This is the inside panel that actually contains the toolstrip items.
    /// </summary>
    protected class ToolStripPanel : Panel
    {
      #region ItemComparer Class

      /// <summary>
      /// A comparer that orders controls base on their oreder in a given list.
      /// </summary>
      protected class ItemComparer : IComparer<Control>
      {
        private List<Control> m_lstOrderAdded;

        /// <summary>
        /// A simple contructor.
        /// </summary>
        /// <param name="p_lstOrderAdded">The list dictating the order of the controls.</param>
        public ItemComparer(List<Control> p_lstOrderAdded)
        {
          m_lstOrderAdded = p_lstOrderAdded;
        }

        #region IComparer<Control> Members

        /// <seealso cref="IComparer{T}.Compare"/>
        public int Compare(Control x, Control y)
        {
          if (x == null)
          {
            if (y == null)
            {
              return 0;
            }
            return -1;
          }
          if (y == null)
          {
            return 1;
          }
          return m_lstOrderAdded.IndexOf(x).CompareTo(m_lstOrderAdded.IndexOf(y));
        }

        #endregion
      }

      #endregion

      private Orientation m_otnDirection = Orientation.Vertical;
      private List<Control> m_lstOrderAdded = new List<Control>();
      private Int32 m_intScrollAmount = 5;
      private FlatStyle m_fstFlatStyle = FlatStyle.Flat;

      #region Properties

      /// <summary>
      /// Gets or sets the scrollAmount of the ToolStripPanel.
      /// </summary>
      /// <value>The scrollAmount of the ToolStripPanel.</value>
      public Int32 ScrollAmount
      {
        get
        {
          return m_intScrollAmount;
        }
        set
        {
          m_intScrollAmount = value;
        }
      }

      /// <summary>
      /// Gets or sets the direction of the ToolStripPanel.
      /// </summary>
      /// <value>The direction of the ToolStripPanel.</value>
      public Orientation Direction
      {
        get
        {
          return m_otnDirection;
        }
        set
        {
          m_otnDirection = value;
        }
      }

      /// <summary>
      /// Gets whether an up scroll control is needed.
      /// </summary>
      /// <value>Whether an up scroll control is needed.</value>
      public bool NeedScroll { get; private set; }

      /// <summary>
      /// Gets whether the up scroll should be enabled.
      /// </summary>
      /// <value>Whether the up scroll should be enabled.</value>
      public bool EnableUpScroll { get; private set; }

      /// <summary>
      /// Gets whether the down scroll should be enabled.
      /// </summary>
      /// <value>Whether the down scroll should be enabled.</value>
      public bool EnableDownScroll { get; private set; }

      /// <summary>
      /// Gets or sets the flatStyle of the ToolStripItems.
      /// </summary>
      /// <value>The flatStyle of the ToolStripItems.</value>
      public FlatStyle ButtonFlatStyle
      {
        get
        {
          return m_fstFlatStyle;
        }
        set
        {
          m_fstFlatStyle = value;
        }
      }

      /// <summary>
      /// Gets or sets the BorderWidth of the ToolStripItems.
      /// </summary>
      /// <value>The BorderWidth of the ToolStripItems.</value>
      public int ButtonBorderWidth { get; set; }

      #endregion

      #region Constructors

      /// <summary>
      /// The default constructor.
      /// </summary>
      public ToolStripPanel()
      {
        AutoScroll = true;
      }

      #endregion

      protected void SortToolStripItems()
      {
        var sltItems = new SortedList<Int32, SortedList<Control>>();

        int intIndex;
        var icpComparer = new ItemComparer(m_lstOrderAdded);
        for (var i = Controls.Count - 1; i >= 0; i--)
        {
          var ctlControl = Controls[i];

          intIndex = ((PanelToolStripItem) ctlControl.Tag).Index;
          if (!sltItems.ContainsKey(intIndex))
          {
            sltItems[intIndex] = new SortedList<Control>(icpComparer);
          }
          sltItems[intIndex].Add(ctlControl);
        }

        //the lower the index, the higher up/further to the left
        // so index 0 is at the top/left
        // (top or left depending on orientation)
        intIndex = 0;
        for (var i = sltItems.Values.Count - 1; i >= 0; i--)
          //for (Int32 i = 0; i < sltItems.Values.Count; i++)
        {
          var lstButtons = sltItems.Values[i];
          for (var j = lstButtons.Count - 1; j >= 0; j--)
          {
            var ctlButton = lstButtons[j];
            Controls.SetChildIndex(ctlButton, intIndex++);

            if ((i == sltItems.Values.Count - 1) && (j == lstButtons.Count - 1))
            {
              if (m_otnDirection == Orientation.Vertical)
              {
                NeedScroll = (NeedScroll || (ctlButton.Bounds.Bottom > Height));
                EnableDownScroll = NeedScroll;
              }
              else
              {
                NeedScroll = (NeedScroll || (ctlButton.Bounds.Right > Width));
                EnableUpScroll = NeedScroll;
              }
            }
          }
        }
      }

      #region Control Addition/Removal

      /// <summary>
      /// Controls the addition of controls to the panel.
      /// </summary>
      /// <remarks>
      /// This makes sure the added toolstrip items are sized, positioned, and ordered correctly.
      /// </remarks>
      /// <param name="e">A <see cref="ControlEventArgs"/> describing the event arguments.</param>
      protected override void OnControlAdded(ControlEventArgs e)
      {
        var ctlButton = e.Control;

        if (ctlButton.Tag is PanelToolStripItem)
        {
          var tsiStripItem = (PanelToolStripItem) ctlButton.Tag;
          m_lstOrderAdded.Add(ctlButton);
          ((PanelToolStripItem) ctlButton.Tag).IndexChanged += ToolStripPanel_IndexChanged;

          ctlButton.Dock = (m_otnDirection == Orientation.Horizontal) ? DockStyle.Left : DockStyle.Top;
          tsiStripItem.SetUnselected();

          SortToolStripItems();
        }
        base.OnControlAdded(e);
      }

      protected override void OnControlRemoved(ControlEventArgs e)
      {
        var ctlButton = e.Control;
        if (ctlButton.Tag is PanelToolStripItem)
        {
          ((PanelToolStripItem) ctlButton.Tag).IndexChanged -= ToolStripPanel_IndexChanged;
        }
        base.OnControlRemoved(e);
      }

      private void ToolStripPanel_IndexChanged(object sender, EventArgs e)
      {
        SortToolStripItems();
      }

      /// <summary>
      /// Adds a <see cref="PanelToolStripItem"/> to the panel.
      /// </summary>
      /// <param name="p_pdiItem">The <see cref="PanelToolStripItem"/> to add.</param>
      public void addToolStripItem(PanelToolStripItem p_pdiItem)
      {
        p_pdiItem.Selected += psiButton_Selected;
        Controls.Add(p_pdiItem.Button);
      }

      /// <summary>
      /// Removes a <see cref="PanelToolStripItem"/> to the panel.
      /// </summary>
      /// <param name="p_pdiItem">The <see cref="PanelToolStripItem"/> to remove.</param>
      public void removeToolStripItem(PanelToolStripItem p_pdiItem)
      {
        p_pdiItem.Selected -= psiButton_Selected;
        Controls.Remove(p_pdiItem.Button);
      }

      #endregion

      #region Scrolling

      /// <summary>
      /// Scrolls the toolstrip items up by one unit of <see cref="ScrollAmount"/> pixels.
      /// </summary>
      public void scrollUp()
      {
        var intNewX = DisplayRectangle.X;
        var intNewY = DisplayRectangle.Y;
        if (m_otnDirection == Orientation.Horizontal)
        {
          intNewX -= m_intScrollAmount;
          if (DisplayRectangle.Right < Width)
          {
            intNewX += Width - DisplayRectangle.Right + 1;
          }
        }
        else
        {
          intNewY += m_intScrollAmount;
          if (intNewY > 0)
          {
            intNewY = 0;
          }
        }
        SetDisplayRectLocation(intNewX, intNewY);

        EnableDownScroll = true;
        checkScrollUp();
      }

      /// <summary>
      /// Scrolls the toolstrip items down by one unit of <see cref="ScrollAmount"/> pixels.
      /// </summary>
      public void scrollDown()
      {
        var intNewX = DisplayRectangle.X;
        var intNewY = DisplayRectangle.Y;
        if (m_otnDirection == Orientation.Horizontal)
        {
          intNewX += m_intScrollAmount;
          if (intNewX > 0)
          {
            intNewX = 0;
          }
        }
        else
        {
          intNewY -= m_intScrollAmount;
          if (DisplayRectangle.Bottom < Height)
          {
            intNewY += Height - DisplayRectangle.Bottom + 1;
          }
        }
        SetDisplayRectLocation(intNewX, intNewY);

        EnableUpScroll = true;
        checkScrollDown();
      }

      /// <summary>
      /// This checks to see if the scroll up button needs to be enabled.
      /// </summary>
      protected void checkScrollUp()
      {
        Control ctlButton;
        if (m_otnDirection == Orientation.Vertical)
        {
          ctlButton = Controls[Controls.Count - 1];
          EnableUpScroll = (ctlButton.Bounds.Top < 0);
        }
        else
        {
          ctlButton = Controls[0];
          EnableUpScroll = (ctlButton.Bounds.Right > Width);
        }
      }

      /// <summary>
      /// This checks to see if the scroll down button needs to be enabled.
      /// </summary>
      protected void checkScrollDown()
      {
        Control ctlButton;
        if (m_otnDirection == Orientation.Vertical)
        {
          ctlButton = Controls[0];
          EnableDownScroll = (ctlButton.Bounds.Bottom > Height);
        }
        else
        {
          ctlButton = Controls[Controls.Count - 1];
          EnableDownScroll = (ctlButton.Bounds.Left < 0);
        }
      }

      /// <summary>
      /// This checks to see if we need scroll buttons, and, if so,
      /// which buttons need to be enabled.
      /// </summary>
      public void checkScroll()
      {
        if (DesignMode)
        {
          NeedScroll = true;
          return;
        }

        if (Controls.Count == 0)
        {
          return;
        }

        Control ctlButton;
        if (m_otnDirection == Orientation.Vertical)
        {
          ctlButton = Controls[0];
          NeedScroll = (ctlButton.Bounds.Bottom > Height);

          ctlButton = Controls[Controls.Count - 1];
          NeedScroll = (NeedScroll || (ctlButton.Bounds.Top < 0));
        }
        else
        {
          ctlButton = Controls[0];
          NeedScroll = (ctlButton.Bounds.Right > Width);

          ctlButton = Controls[Controls.Count - 1];
          NeedScroll = (NeedScroll || (ctlButton.Bounds.Left < 0));
        }

        if (NeedScroll)
        {
          checkScrollUp();
          checkScrollDown();
        }
      }

      #endregion

      /// <summary>
      /// Handles the colour change of the toolstrip items when selected/unselected.
      /// </summary>
      /// <param name="sender">The object that triggered the event.</param>
      /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
      private void psiButton_Selected(object sender, EventArgs e)
      {
        var ctlButton = ((PanelToolStripItem) sender).Button;
        for (var i = Controls.Count - 1; i >= 0; i--)
        {
          var ctlOther = Controls[i];
          if ((ctlOther != ctlButton) && (ctlOther.Tag is PanelToolStripItem))
          {
            ((PanelToolStripItem) ctlOther.Tag).SetUnselected();
          }
        }
      }
    }

    #endregion

    private ToolStripPanel m_pnlToolStrip = new ToolStripPanel();
    private Timer m_tmrScrollTimer = new Timer();
    private Int32 m_intScrollTimerInterval = 25;
    private Button m_butDown;
    private Button m_butUp;
    private Int32 m_intMinScrollButtonWidth = 20;

    #region Properties

    /// <summary>
    /// Gets or sets how many pixels the tool strip scrolls per tick.
    /// </summary>
    /// <value>How many pixels the tool strip scrolls per tick.</value>
    [Category("Behavior"), DefaultValue(5)]
    public Int32 ScrollAmount
    {
      get
      {
        return m_pnlToolStrip.ScrollAmount;
      }
      set
      {
        var intValue = (value == 0) ? 1 : value;
        m_pnlToolStrip.ScrollAmount = intValue;
      }
    }

    /// <summary>
    /// Gets or sets the timerScrollAmountRatio of the PanelToolStrip.
    /// </summary>
    /// <remarks>
    /// The suggested value for this property is 5 times the ScrollAmount.
    /// </remarks>
    /// <value>The timerScrollAmountRatio of the PanelToolStrip.</value>
    [Category("Behavior"), DefaultValue(25)]
    public Int32 ScrollInterval
    {
      get
      {
        return m_intScrollTimerInterval;
      }
      set
      {
        m_intScrollTimerInterval = value;
        m_tmrScrollTimer.Interval = m_intScrollTimerInterval;
      }
    }

    /// <summary>
    /// Gets or sets the direction the tool strip is oriented.
    /// </summary>
    /// <value>
    /// The direction the tool strip is oriented.
    /// </value>
    [Category("Appearance"), DefaultValue(Orientation.Vertical)]
    public Orientation Direction
    {
      get
      {
        return m_pnlToolStrip.Direction;
      }
      set
      {
        m_pnlToolStrip.Direction = value;
        m_butDown.Dock = (m_pnlToolStrip.Direction == Orientation.Horizontal) ? DockStyle.Left : DockStyle.Bottom;
        m_butUp.Dock = (m_pnlToolStrip.Direction == Orientation.Horizontal) ? DockStyle.Right : DockStyle.Top;
        m_butDown.MinimumSize = (m_pnlToolStrip.Direction == Orientation.Horizontal)
          ? new Size(m_intMinScrollButtonWidth, 0)
          : new Size(0, m_intMinScrollButtonWidth);
        m_butDown.MinimumSize = (m_pnlToolStrip.Direction == Orientation.Horizontal)
          ? new Size(m_intMinScrollButtonWidth, 0)
          : new Size(0, m_intMinScrollButtonWidth);
      }
    }

    /// <summary>
    /// Gest or sets the minimum width or height of the scroll buttons.
    /// </summary>
    /// <value>
    /// The minimum width or height of the scroll buttons.
    /// </value>
    [Category("Appearance"), DefaultValue(20)]
    public Int32 MinimumScrollButtonWidth
    {
      get
      {
        return m_intMinScrollButtonWidth;
      }
      set
      {
        m_intMinScrollButtonWidth = value;
      }
    }

    /// <summary>
    /// Gets or sets the background colour of the conrol.
    /// </summary>
    /// <value>The background colour of the conrol.</value>
    public override Color BackColor
    {
      get
      {
        return m_pnlToolStrip.BackColor;
      }
      set
      {
        m_pnlToolStrip.BackColor = value;
      }
    }

    /// <summary>
    /// Gets or sets the image displayed on the scroll down button.
    /// </summary>
    /// <value>The image displayed on the scroll down button.</value>
    [Category("Appearance")]
    public Image ScrollDownImage
    {
      get
      {
        return m_butDown.Image;
      }
      set
      {
        m_butDown.Image = value;
      }
    }

    /// <summary>
    /// Gets or sets the scroll down button image alignment.
    /// </summary>
    /// <value>The scroll down button image alignment.</value>
    [Category("Appearance"), DefaultValue(ContentAlignment.MiddleCenter)]
    public ContentAlignment ScrollDownImageAlign
    {
      get
      {
        return m_butDown.ImageAlign;
      }
      set
      {
        m_butDown.ImageAlign = value;
      }
    }

    /// <summary>
    /// Gets or sets the scroll down button text.
    /// </summary>
    /// <value>The scroll down button text.</value>
    [Category("Appearance"), DefaultValue("")]
    public string ScrollDownText
    {
      get
      {
        return m_butDown.Text;
      }
      set
      {
        m_butDown.Text = value;
      }
    }

    /// <summary>
    /// Gets or sets the scroll down button text alignment.
    /// </summary>
    /// <value>The scroll down button text alignment.</value>
    [Category("Appearance"), DefaultValue(ContentAlignment.MiddleCenter)]
    public ContentAlignment ScrollDownTextAlign
    {
      get
      {
        return m_butDown.TextAlign;
      }
      set
      {
        m_butDown.TextAlign = value;
      }
    }

    /// <summary>
    /// Gets or sets the position of the text and image relative to each other on the scroll bown button.
    /// </summary>
    /// <value>The position of the text and image relative to each other on the scroll bown button.</value>
    [Category("Appearance"), DefaultValue(TextImageRelation.ImageAboveText)]
    public TextImageRelation ScrollDownTextImageRelation
    {
      get
      {
        return m_butDown.TextImageRelation;
      }
      set
      {
        m_butDown.TextImageRelation = value;
      }
    }

    /// <summary>
    /// Gets or sets the image displayed on the scroll Up button.
    /// </summary>
    /// <value>The image displayed on the scroll Up button.</value>
    [Category("Appearance")]
    public Image ScrollUpImage
    {
      get
      {
        return m_butUp.Image;
      }
      set
      {
        m_butUp.Image = value;
      }
    }

    /// <summary>
    /// Gets or sets the scroll Up button image alignment.
    /// </summary>
    /// <value>The scroll Up button image alignment.</value>
    [Category("Appearance"), DefaultValue(ContentAlignment.MiddleCenter)]
    public ContentAlignment ScrollUpImageAlign
    {
      get
      {
        return m_butUp.ImageAlign;
      }
      set
      {
        m_butUp.ImageAlign = value;
      }
    }

    /// <summary>
    /// Gets or sets the scroll Up button text.
    /// </summary>
    /// <value>The scroll Up button text.</value>
    [Category("Appearance"), DefaultValue("")]
    public string ScrollUpText
    {
      get
      {
        return m_butUp.Text;
      }
      set
      {
        m_butUp.Text = value;
      }
    }

    /// <summary>
    /// Gets or sets the scroll Up button text alignment.
    /// </summary>
    /// <value>The scroll Up button text alignment.</value>
    [Category("Appearance"), DefaultValue(ContentAlignment.MiddleCenter)]
    public ContentAlignment ScrollUpTextAlign
    {
      get
      {
        return m_butUp.TextAlign;
      }
      set
      {
        m_butUp.TextAlign = value;
      }
    }

    /// <summary>
    /// Gets or sets the position of the text and image relative to each other on the scroll bown button.
    /// </summary>
    /// <value>The position of the text and image relative to each other on the scroll bown button.</value>
    [Category("Appearance"), DefaultValue(TextImageRelation.ImageAboveText)]
    public TextImageRelation ScrollUpTextImageRelation
    {
      get
      {
        return m_butUp.TextImageRelation;
      }
      set
      {
        m_butUp.TextImageRelation = value;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public PanelToolStrip()
    {
      BackColor = m_pnlToolStrip.BackColor;
      Controls.Add(m_pnlToolStrip);
      m_pnlToolStrip.ControlAdded += m_pnlToolStrip_ControlAdded;
      m_pnlToolStrip.Width = Width;
      m_pnlToolStrip.Height = Height;

      m_butDown = new Button();
      m_butDown.Text = "";
      m_butDown.Image = Resources.arrow_down_black_small;
      m_butDown.TextImageRelation = TextImageRelation.ImageAboveText;
      m_butDown.FlatStyle = m_pnlToolStrip.ButtonFlatStyle;
      m_butDown.AutoSize = true;
      m_butDown.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      m_butDown.MinimumSize = (m_pnlToolStrip.Direction == Orientation.Horizontal)
        ? new Size(m_intMinScrollButtonWidth, 0)
        : new Size(0, m_intMinScrollButtonWidth);
      m_butDown.MouseEnter += scrollStart;
      m_butDown.MouseLeave += scrollStop;
      Controls.Add(m_butDown);

      m_butUp = new Button();
      m_butUp.Text = "";
      m_butUp.Image = Resources.arrow_up_black_small;
      m_butUp.TextImageRelation = TextImageRelation.ImageAboveText;
      m_butUp.FlatStyle = m_pnlToolStrip.ButtonFlatStyle;
      m_butUp.AutoSize = true;
      m_butUp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      m_butUp.MinimumSize = (m_pnlToolStrip.Direction == Orientation.Horizontal)
        ? new Size(m_intMinScrollButtonWidth, 0)
        : new Size(0, m_intMinScrollButtonWidth);
      m_butUp.MouseEnter += scrollStart;
      m_butUp.MouseLeave += scrollStop;
      Controls.Add(m_butUp);

      m_butDown.Dock = (m_pnlToolStrip.Direction == Orientation.Horizontal) ? DockStyle.Left : DockStyle.Bottom;
      m_butUp.Dock = (m_pnlToolStrip.Direction == Orientation.Horizontal) ? DockStyle.Right : DockStyle.Top;

      m_tmrScrollTimer.Interval = m_intScrollTimerInterval;
      m_tmrScrollTimer.Tick += m_tmrScrollTimer_Tick;
    }

    #endregion

    #region Control Addition/Removal

    /// <summary>
    /// Adds a <see cref="PanelToolStripItem"/>.
    /// </summary>
    /// <param name="p_pdiItem">The <see cref="PanelToolStripItem"/> to add.</param>
    public void addToolStripItem(PanelToolStripItem p_pdiItem)
    {
      if (m_pnlToolStrip.Direction == Orientation.Horizontal)
      {
        m_pnlToolStrip.Height = Height + SystemInformation.HorizontalScrollBarHeight;
      }
      else
      {
        m_pnlToolStrip.Width = Width + SystemInformation.VerticalScrollBarWidth;
      }
      m_pnlToolStrip.addToolStripItem(p_pdiItem);
    }

    /// <summary>
    /// Adds a <see cref="Control"/> to the toolstrip.
    /// </summary>
    /// <remarks>
    /// This method creates a <see cref="PanelToolStripItem"/> base on the given
    /// values and adds it to the toolstrip.
    /// </remarks>
    /// <param name="p_ctlButton">The <see cref="Control"/> to add.</param>
    /// <param name="p_strEventName">Event Name</param>
    /// <param name="p_intIndex">The index at which to insert the added item.</param>
    /// <param name="p_tdsDisplayStyle">The <see cref="ToolStripItemDisplayStyle"/> indicating how text and
    /// images are displayed on the added item.</param>
    public void addToolStripItem(Control p_ctlButton, string p_strEventName, Int32 p_intIndex,
                                 ToolStripItemDisplayStyle p_tdsDisplayStyle)
    {
      addToolStripItem(new PanelToolStripItem(p_ctlButton, p_strEventName, p_intIndex, p_tdsDisplayStyle));
    }

    /// <summary>
    /// Removes a <see cref="PanelToolStripItem"/>.
    /// </summary>
    /// <param name="p_pdiItem">The <see cref="PanelToolStripItem"/> to remove.</param>
    public void removeToolStripItem(PanelToolStripItem p_pdiItem)
    {
      m_pnlToolStrip.removeToolStripItem(p_pdiItem);
    }

    /// <summary>
    /// This ensures that all the scrolling controls are properly positioned whenever a new control is added.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">A <see cref="ControlEventArgs"/> describing the event arguments.</param>
    private void m_pnlToolStrip_ControlAdded(object sender, ControlEventArgs e)
    {
      checkScroll();
    }

    #endregion

    #region ToolStripPanel Positioning

    /// <summary>
    /// This positions the contained <see cref="ToolStripPanel"/> so that is isn't hidden
    /// by the scroll controls, and so its scrollbar is not visible.
    /// </summary>
    protected void positionToolStripPanel()
    {
      SuspendLayout();

      var intButtonSpace = 0;
      if (m_pnlToolStrip.Direction == Orientation.Horizontal)
      {
        if (m_butDown.Visible)
        {
          intButtonSpace += m_butDown.Width;
          m_pnlToolStrip.Left = m_butDown.Width;
        }
        else
        {
          m_pnlToolStrip.Left = 0;
        }

        if (m_butUp.Visible)
        {
          intButtonSpace += m_butUp.Width;
        }

        m_pnlToolStrip.Height = (m_pnlToolStrip.NeedScroll)
          ? Height + SystemInformation.HorizontalScrollBarHeight
          : Height;
        m_pnlToolStrip.Width = Width - intButtonSpace;
        m_pnlToolStrip.Top = 0;
      }
      else
      {
        if (m_butUp.Visible)
        {
          intButtonSpace += m_butUp.Height;
          m_pnlToolStrip.Top = m_butUp.Height;
        }
        else
        {
          m_pnlToolStrip.Top = 0;
        }

        if (m_butDown.Visible)
        {
          intButtonSpace += m_butDown.Height;
        }

        m_pnlToolStrip.Width = (m_pnlToolStrip.NeedScroll)
          ? Width + SystemInformation.VerticalScrollBarWidth
          : Width;
        m_pnlToolStrip.Height = Height - intButtonSpace;
        m_pnlToolStrip.Left = 0;
      }

      ResumeLayout();
    }

    #endregion

    #region Scrolling

    /// <summary>
    /// Raises the resize event.
    /// </summary>
    /// <remarks>
    /// This redraws the contain toolstrip panel to conform to our new dimensions.
    /// </remarks>
    /// <param name="eventargs">An <see cref="EventArgs"/> describing the event arguments.</param>
    protected override void OnResize(EventArgs eventargs)
    {
      base.OnResize(eventargs);

      SuspendLayout();

      if (m_pnlToolStrip.Direction == Orientation.Horizontal)
      {
        m_pnlToolStrip.Height = Height + SystemInformation.HorizontalScrollBarHeight;
        m_pnlToolStrip.Width = Width;
      }
      else
      {
        m_pnlToolStrip.Width = Width + SystemInformation.VerticalScrollBarWidth;
        m_pnlToolStrip.Height = Height;
      }

      m_pnlToolStrip.checkScroll();
      checkScroll();

      ResumeLayout();
    }

    /// <summary>
    /// This checks to see if the scroll contrls should be visible.
    /// </summary>
    /// <remarks>
    /// This makes the scroll controls visible or not as required, and ensures that
    /// the contained <see cref="ToolStripPanel"/> is correctly positioned.
    /// </remarks>
    protected void checkScroll()
    {
      m_butDown.Visible = m_pnlToolStrip.NeedScroll;
      m_butDown.Enabled = m_pnlToolStrip.EnableDownScroll;
      m_butUp.Visible = m_pnlToolStrip.NeedScroll;
      m_butUp.Enabled = m_pnlToolStrip.EnableUpScroll;

      positionToolStripPanel();
    }

    /// <summary>
    /// This starts the scrolling whenever the mouse is over a scroll control.
    /// </summary>
    /// <param name="sender">The scroll control that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    protected void scrollStart(object sender, EventArgs e)
    {
      m_tmrScrollTimer.Tag = sender;
      m_tmrScrollTimer.Start();
    }

    /// <summary>
    /// This stops the scrolling whenever the mouse leaves a scroll control.
    /// </summary>
    /// <param name="sender">The scroll control that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    protected void scrollStop(object sender, EventArgs e)
    {
      m_tmrScrollTimer.Stop();
    }

    /// <summary>
    /// This scrolls the toolstrip while the mouse is over a scroll control.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void m_tmrScrollTimer_Tick(object sender, EventArgs e)
    {
      if (m_tmrScrollTimer.Tag == m_butUp)
      {
        m_pnlToolStrip.scrollUp();
      }
      else
      {
        m_pnlToolStrip.scrollDown();
      }
      checkScroll();
    }

    #endregion
  }
}