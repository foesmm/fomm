using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Fomm.Controls
{
  /// <summary>
  /// A split button control.
  /// </summary>
  public class SplitButton : Button
  {
    private const int PUSH_BUTTON_WIDTH = 14;

    private static readonly int m_intBorderSize = SystemInformation.Border3DSize.Width*1;
    private PushButtonState m_pbsState;
    private bool m_booSkipNextOpen;
    private Rectangle m_rctDropDownRectangle;
    private bool m_booShowSplit = true;
    private ToolStripItem m_tsiLastPressedButton;
    private Int32 m_intSelectedItemIndex;

    #region Properties

    /// <summary>
    /// Sets whether or not to display the split.
    /// </summary>
    /// <value>Whether or not to display the split.</value>
    [DefaultValue(true)]
    public bool ShowSplit
    {
      set
      {
        if (value != m_booShowSplit)
        {
          m_booShowSplit = value;
          Invalidate();
          if (Parent != null)
          {
            Parent.PerformLayout();
          }
        }
      }
    }

    /// <summary>
    /// Gets or sets the pushed state of the button.
    /// </summary>
    /// <value>The pushed state of the button.</value>
    private PushButtonState State
    {
      get
      {
        return m_pbsState;
      }
      set
      {
        if (!m_pbsState.Equals(value))
        {
          m_pbsState = value;
          Invalidate();
        }
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="ContextMenuStrip"/> for this control.
    /// </summary>
    /// <remarks>
    /// This context menu is used to display the options when the arrow of the split button is clicked.
    /// </remarks>
    /// <value>The <see cref="ContextMenuStrip"/> for this control.</value>
    public override ContextMenuStrip ContextMenuStrip
    {
      get
      {
        return base.ContextMenuStrip;
      }
      set
      {
        if (base.ContextMenuStrip != value)
        {
          if (base.ContextMenuStrip != null)
          {
            base.ContextMenuStrip.ItemClicked -= ContextMenuStrip_ItemClicked;
          }
          base.ContextMenuStrip = value;
          if (base.ContextMenuStrip != null)
          {
            base.ContextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked;
            if (m_intSelectedItemIndex >= base.ContextMenuStrip.Items.Count)
            {
              m_intSelectedItemIndex = 0;
            }
            m_tsiLastPressedButton = ContextMenuStrip.Items[m_intSelectedItemIndex];
          }
        }
      }
    }

    /// <summary>
    /// Gets or sets the button text.
    /// </summary>
    /// <remarks>
    /// The button text is the text of the last selected item. If there are no items, then it is the button
    /// text.
    /// </remarks>
    /// <value>The button text.</value>
    public override string Text
    {
      get
      {
        return (m_tsiLastPressedButton == null) ? base.Text : m_tsiLastPressedButton.Text;
      }
      set
      {
        base.Text = value;
      }
    }

    /// <summary>
    /// Gets or sets the index of the selected drop down item.
    /// </summary>
    /// <value>The index of the selected drop down item.</value>
    [Category("Behavior"), DefaultValue(0)]
    public Int32 SelectedItemIndex
    {
      get
      {
        if (m_tsiLastPressedButton == null)
        {
          return 0;
        }
        return ContextMenuStrip.Items.IndexOf(m_tsiLastPressedButton);
      }
      set
      {
        if (value >= ContextMenuStrip.Items.Count)
        {
          m_intSelectedItemIndex = ContextMenuStrip.Items.Count;
        }
        else if (value < 0)
        {
          m_intSelectedItemIndex = 0;
        }
        else
        {
          m_intSelectedItemIndex = value;
        }
        if (ContextMenuStrip != null)
        {
          m_tsiLastPressedButton = ContextMenuStrip.Items[m_intSelectedItemIndex];
        }
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public SplitButton()
    {
      AutoSize = true;
    }

    #endregion

    /// <summary>
    /// Gets the preferred size of the control.
    /// </summary>
    /// <param name="proposedSize">The desired size.</param>
    /// <returns>The preferred size of the control.</returns>
    public override Size GetPreferredSize(Size proposedSize)
    {
      Size szePreferredSize = base.GetPreferredSize(proposedSize);
      if (m_booShowSplit && !string.IsNullOrEmpty(Text) &&
          TextRenderer.MeasureText(Text, Font).Width + PUSH_BUTTON_WIDTH > szePreferredSize.Width)
      {
        return szePreferredSize + new Size(PUSH_BUTTON_WIDTH + m_intBorderSize*2, 0);
      }
      return szePreferredSize;
    }

    /// <summary>
    /// Determines if the given key is considered an input key by the control.
    /// </summary>
    /// <remarks>
    /// The we are showing the split, then the down arrow is an input key.
    /// </remarks>
    /// <param name="keyData">The key for which it is to be determined if it is an input key.</param>
    /// <returns><lang cref="true"/> if the given key is an input key; <lang cref="false"/> otherwise.</returns>
    protected override bool IsInputKey(Keys keyData)
    {
      if (keyData.Equals(Keys.Down) && m_booShowSplit)
      {
        return true;
      }
      return base.IsInputKey(keyData);
    }

    /// <summary>
    /// Raises the <see cref="Control.GotFocus"/> event.
    /// </summary>
    /// <remarks>
    /// This changes the state of the control based on whether or not it has focus.
    /// </remarks>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    protected override void OnGotFocus(EventArgs e)
    {
      if (!m_booShowSplit)
      {
        base.OnGotFocus(e);
        return;
      }

      if (!State.Equals(PushButtonState.Pressed) && !State.Equals(PushButtonState.Disabled))
      {
        State = PushButtonState.Default;
      }
    }

    /// <summary>
    /// Raises the <see cref="Control.KeyDown"/> event.
    /// </summary>
    /// <remarks>
    /// This displays the context menu if the down key is pressed.
    /// </remarks>
    /// <param name="e">A <see cref="KeyEventArgs"/> describing the event arguments.</param>
    protected override void OnKeyDown(KeyEventArgs kevent)
    {
      if (m_booShowSplit)
      {
        if (kevent.KeyCode.Equals(Keys.Down))
        {
          ShowContextMenuStrip();
        }
        else if (kevent.KeyCode.Equals(Keys.Space) && kevent.Modifiers == Keys.None)
        {
          State = PushButtonState.Pressed;
        }
      }

      base.OnKeyDown(kevent);
    }

    /// <summary>
    /// Raises the <see cref="Control.KeyUp"/> event.
    /// </summary>
    /// <remarks>
    /// This changes the state as appropriate.
    /// </remarks>
    /// <param name="e">A <see cref="KeyEventArgs"/> describing the event arguments.</param>
    protected override void OnKeyUp(KeyEventArgs kevent)
    {
      if (kevent.KeyCode.Equals(Keys.Space) && (MouseButtons == MouseButtons.None))
      {
        State = PushButtonState.Normal;
      }
      base.OnKeyUp(kevent);
    }

    /// <summary>
    /// Raises the <see cref="Control.LostFocus"/> event.
    /// </summary>
    /// <remarks>
    /// This changes the state of the control based on whether or not it has focus.
    /// </remarks>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    protected override void OnLostFocus(EventArgs e)
    {
      if (!m_booShowSplit)
      {
        base.OnLostFocus(e);
        return;
      }
      if (!State.Equals(PushButtonState.Pressed) && !State.Equals(PushButtonState.Disabled))
      {
        State = PushButtonState.Normal;
      }
    }

    /// <summary>
    /// Raises the <see cref="Control.MouseDown"/> event.
    /// </summary>
    /// <remarks>
    /// This displays the context menu if the arrow portion of the button is pressed.
    /// </remarks>
    /// <param name="e">A <see cref="MouseEventArgs"/> describing the event arguments.</param>
    protected override void OnMouseDown(MouseEventArgs e)
    {
      if (!m_booShowSplit)
      {
        base.OnMouseDown(e);
        return;
      }

      if (m_rctDropDownRectangle.Contains(e.Location))
      {
        ShowContextMenuStrip();
      }
      else
      {
        State = PushButtonState.Pressed;
      }
    }

    /// <summary>
    /// Raises the <see cref="Control.MouseEnter"/> event.
    /// </summary>
    /// <remarks>
    /// This updates the state as appropriate.
    /// </remarks>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    protected override void OnMouseEnter(EventArgs e)
    {
      if (!m_booShowSplit)
      {
        base.OnMouseEnter(e);
        return;
      }

      if (!State.Equals(PushButtonState.Pressed) && !State.Equals(PushButtonState.Disabled))
      {
        State = PushButtonState.Hot;
      }
    }

    /// <summary>
    /// Raises the <see cref="Control.MouseLeave"/> event.
    /// </summary>
    /// <remarks>
    /// This updates the state as appropriate.
    /// </remarks>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    protected override void OnMouseLeave(EventArgs e)
    {
      if (!m_booShowSplit)
      {
        base.OnMouseLeave(e);
        return;
      }

      if (!State.Equals(PushButtonState.Pressed) && !State.Equals(PushButtonState.Disabled))
      {
        State = Focused ? PushButtonState.Default : PushButtonState.Normal;
      }
    }

    /// <summary>
    /// Raises the <see cref="Control.MouseUp"/> event.
    /// </summary>
    /// <remarks>
    /// This updates the state as appropriate.
    /// </remarks>
    /// <param name="e">A <see cref="MouseEventArgs"/> describing the event arguments.</param>
    protected override void OnMouseUp(MouseEventArgs mevent)
    {
      if (!m_booShowSplit)
      {
        base.OnMouseUp(mevent);
        return;
      }

      if (ContextMenuStrip == null || !ContextMenuStrip.Visible)
      {
        SetButtonDrawState();
        if (Bounds.Contains(Parent.PointToClient(Cursor.Position)) && !m_rctDropDownRectangle.Contains(mevent.Location))
        {
          OnClick(new EventArgs());
        }
      }
    }

    /// <summary>
    /// Raises the <see cref="Control.MouseDown"/> event.
    /// </summary>
    /// <remarks>
    /// This activates the last clicked menu item.
    /// </remarks>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    protected override void OnClick(EventArgs e)
    {
      if (!m_booShowSplit)
      {
        base.OnClick(e);
        return;
      }

      if (m_tsiLastPressedButton != null)
      {
        m_tsiLastPressedButton.PerformClick();
      }
    }

    /// <summary>
    /// Raises the <see cref="Control.Paint"/> event.
    /// </summary>
    /// <remarks>
    /// This paints the button to look split.
    /// </remarks>
    /// <param name="e">A <see cref="PaintEventArgs"/> describing the event arguments.</param>
    protected override void OnPaint(PaintEventArgs pevent)
    {
      base.OnPaint(pevent);

      if (!m_booShowSplit)
      {
        return;
      }

      Graphics gphGraphics = pevent.Graphics;
      Rectangle rctBounds = ClientRectangle;

      // draw the button background as according to the current state.
      if (State != PushButtonState.Pressed && IsDefault && !Application.RenderWithVisualStyles)
      {
        Rectangle backgroundBounds = rctBounds;
        backgroundBounds.Inflate(-1, -1);
        ButtonRenderer.DrawButton(gphGraphics, backgroundBounds, State);

        // button renderer doesnt draw the black frame when themes are off =(
        gphGraphics.DrawRectangle(SystemPens.WindowFrame, 0, 0, rctBounds.Width - 1, rctBounds.Height - 1);
      }
      else
      {
        ButtonRenderer.DrawButton(gphGraphics, rctBounds, State);
      }

      // calculate the current dropdown rectangle.
      m_rctDropDownRectangle = new Rectangle(rctBounds.Right - PUSH_BUTTON_WIDTH - 1, m_intBorderSize, PUSH_BUTTON_WIDTH,
                                             rctBounds.Height - m_intBorderSize*2);

      Int32 intInternalBorder = m_intBorderSize;
      Rectangle rctFocusRect = new Rectangle(intInternalBorder,
                                             intInternalBorder,
                                             rctBounds.Width - m_rctDropDownRectangle.Width - intInternalBorder,
                                             rctBounds.Height - (intInternalBorder*2));

      bool booDrawSplitLine = (State == PushButtonState.Hot || State == PushButtonState.Pressed ||
                               !Application.RenderWithVisualStyles);
      if (RightToLeft == RightToLeft.Yes)
      {
        m_rctDropDownRectangle.X = rctBounds.Left + 1;
        rctFocusRect.X = m_rctDropDownRectangle.Right;
        if (booDrawSplitLine)
        {
          // draw two lines at the edge of the dropdown button
          gphGraphics.DrawLine(SystemPens.ButtonShadow, rctBounds.Left + PUSH_BUTTON_WIDTH, m_intBorderSize,
                               rctBounds.Left + PUSH_BUTTON_WIDTH, rctBounds.Bottom - m_intBorderSize);
          gphGraphics.DrawLine(SystemPens.ButtonFace, rctBounds.Left + PUSH_BUTTON_WIDTH + 1, m_intBorderSize,
                               rctBounds.Left + PUSH_BUTTON_WIDTH + 1, rctBounds.Bottom - m_intBorderSize);
        }
      }
      else
      {
        if (booDrawSplitLine)
        {
          // draw two lines at the edge of the dropdown button
          gphGraphics.DrawLine(SystemPens.ButtonShadow, rctBounds.Right - PUSH_BUTTON_WIDTH, m_intBorderSize,
                               rctBounds.Right - PUSH_BUTTON_WIDTH, rctBounds.Bottom - m_intBorderSize);
          gphGraphics.DrawLine(SystemPens.ButtonFace, rctBounds.Right - PUSH_BUTTON_WIDTH - 1, m_intBorderSize,
                               rctBounds.Right - PUSH_BUTTON_WIDTH - 1, rctBounds.Bottom - m_intBorderSize);
        }
      }

      // Draw an arrow in the correct location 
      PaintArrow(gphGraphics, m_rctDropDownRectangle);

      // Figure out how to draw the text
      TextFormatFlags tffFormatFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;

      // If we dont' use mnemonic, set formatFlag to NoPrefix as this will show ampersand.
      if (!UseMnemonic)
      {
        tffFormatFlags = tffFormatFlags | TextFormatFlags.NoPrefix;
      }
      else if (!ShowKeyboardCues)
      {
        tffFormatFlags = tffFormatFlags | TextFormatFlags.HidePrefix;
      }

      if (!string.IsNullOrEmpty(Text))
      {
        TextRenderer.DrawText(gphGraphics, Text, Font, rctFocusRect, SystemColors.ControlText, tffFormatFlags);
      }

      // draw the focus rectangle.
      if (State != PushButtonState.Pressed && Focused)
      {
        ControlPaint.DrawFocusRectangle(gphGraphics, rctFocusRect);
      }
    }

    /// <summary>
    /// Paints the drop down arrow.
    /// </summary>
    /// <param name="p_gphGraphics">The graphics object to use to paint the arrow.</param>
    /// <param name="p_rctDropDownRect">The rectangle in which to paint the arrow.</param>
    private void PaintArrow(Graphics p_gphGraphics, Rectangle p_rctDropDownRect)
    {
      Point pntMiddle = new Point(Convert.ToInt32(p_rctDropDownRect.Left + p_rctDropDownRect.Width/2),
                                  Convert.ToInt32(p_rctDropDownRect.Top + p_rctDropDownRect.Height/2));

      //if the width is odd - favor pushing it over one pixel right.
      pntMiddle.X += (p_rctDropDownRect.Width%2);

      Point[] pntArrowPoints =
      {
        new Point(pntMiddle.X - 2, pntMiddle.Y - 1), new Point(pntMiddle.X + 3, pntMiddle.Y - 1),
        new Point(pntMiddle.X, pntMiddle.Y + 2)
      };

      p_gphGraphics.FillPolygon(SystemBrushes.ControlText, pntArrowPoints);
    }

    /// <summary>
    /// Displays the context menu strip.
    /// </summary>
    private void ShowContextMenuStrip()
    {
      if (m_booSkipNextOpen)
      {
        // we were called because we're closing the context menu strip
        // when clicking the dropdown button.
        m_booSkipNextOpen = false;
        return;
      }
      State = PushButtonState.Pressed;

      if (ContextMenuStrip != null)
      {
        ContextMenuStrip.Closing += ContextMenuStrip_Closing;
        ContextMenuStrip.Show(this, new Point(0, Height), ToolStripDropDownDirection.BelowRight);
      }
    }

    /// <summary>
    /// Handles the <see cref="ContextMenuStrip.ItemClicked"/> event.
    /// </summary>
    /// <remarks>
    /// This remembers the last clicked item so that when the non-drop down part of the split
    /// button is clicked the correct drop down item can be activated.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A <see cref="ToolStripItemClickedEventArgs"/> describing the event arguments.</param>
    private void ContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
      m_tsiLastPressedButton = e.ClickedItem;
    }

    /// <summary>
    /// Handles the <see cref="Control.Closing"/> event of the drop down items content menu.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A <see cref="ToolStripDropDownClosingEventArgs"/> describing the event arguments.</param>
    private void ContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
    {
      ContextMenuStrip cmsDropDownItems = sender as ContextMenuStrip;
      if (cmsDropDownItems != null)
      {
        cmsDropDownItems.Closing -= ContextMenuStrip_Closing;
      }

      SetButtonDrawState();
      if (e.CloseReason == ToolStripDropDownCloseReason.AppClicked)
      {
        m_booSkipNextOpen = (m_rctDropDownRectangle.Contains(PointToClient(Cursor.Position)));
      }
    }

    /// <summary>
    /// Sets the button's state as appropriate based on the mouse cursor position.
    /// </summary>
    private void SetButtonDrawState()
    {
      if (Bounds.Contains(Parent.PointToClient(Cursor.Position)))
      {
        State = PushButtonState.Hot;
      }
      else if (Focused)
      {
        State = PushButtonState.Default;
      }
      else
      {
        State = PushButtonState.Normal;
      }
    }
  }
}