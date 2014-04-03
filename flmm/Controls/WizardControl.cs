using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Fomm.Controls
{
  /// <summary>
  /// A wizard control.
  /// </summary>
  [DefaultProperty("SelectedPage"), DefaultEvent("SelectedTabPageChanged"), Designer(typeof (WizardControlDesigner))]
  public class WizardControl : VerticalTabControl
  {
    /// <summary>
    /// Raised when the finish button is clicked.
    /// </summary>
    [Category("Action")]
    public event EventHandler Finished = delegate
    {
    };

    /// <summary>
    /// Raised when the cancel button is clicked.
    /// </summary>
    [Category("Action")]
    public event EventHandler Cancelled = delegate
    {
    };

    private Button m_butPrevious;
    private Button m_butNext;

    #region Properties

    /// <summary>
    /// Gets the wizard's previous button.
    /// </summary>
    /// <value>The wizard's previous button.</value>
    [Browsable(false)]
    public Button PreviousButton
    {
      get
      {
        return m_butPrevious;
      }
    }

    /// <summary>
    /// Gets the wizard's next button.
    /// </summary>
    /// <value>The wizard's next button.</value>
    [Browsable(false)]
    public Button NextButton
    {
      get
      {
        return m_butNext;
      }
    }

    /// <summary>
    /// Gets or sets whether the tabs are visible.
    /// </summary>
    /// <value>Whether the tabs are visible.</value>
    [Category("Appearance"), DefaultValue(false)]
    public override bool TabsVisible
    {
      get
      {
        return base.TabsVisible;
      }
      set
      {
        base.TabsVisible = value;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public WizardControl()
    {
      TabsVisible = false;
      BackColor = Color.FromKnownColor(KnownColor.Control);

      Panel mPnlNavigation = new Panel();
      mPnlNavigation.Dock = DockStyle.Bottom;
      mPnlNavigation.Height = 23 + 2*12;
      mPnlNavigation.DataBindings.Add("BackColor", this, "BackColor");

      Panel mPnlNavigationLight = new Panel();
      mPnlNavigationLight.BackColor = SystemColors.ControlLightLight;
      mPnlNavigationLight.Dock = DockStyle.Top;
      mPnlNavigationLight.Location = new Point(0, 1);
      mPnlNavigationLight.Size = new Size(444, 1);
      mPnlNavigationLight.TabIndex = 1;

      Panel mPnlNavigationShadow = new Panel();
      mPnlNavigationShadow.BackColor = SystemColors.ControlDark;
      mPnlNavigationShadow.Dock = DockStyle.Top;
      mPnlNavigationShadow.Location = new Point(0, 0);
      mPnlNavigationShadow.Size = new Size(444, 1);
      mPnlNavigationShadow.TabIndex = 2;

      mPnlNavigation.Controls.Add(mPnlNavigationLight);
      mPnlNavigation.Controls.Add(mPnlNavigationShadow);

      Controls.Add(mPnlNavigation);

      Button mButCancel = new Button();
      mButCancel.Text = "Cancel";
      mButCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
      mButCancel.Size = new Size(75, 23);
      mButCancel.Location = new Point(mPnlNavigation.Width - 12 - mButCancel.Width, 12);
      mButCancel.Click += Cancel_Click;
      mPnlNavigation.Controls.Add(mButCancel);

      m_butNext = new Button();
      m_butNext.Text = "Next >>";
      m_butNext.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
      m_butNext.Size = new Size(75, 23);
      m_butNext.Location = new Point(mButCancel.Left - 12 - m_butNext.Width, 12);
      m_butNext.Click += Next_Click;
      mPnlNavigation.Controls.Add(m_butNext);

      m_butPrevious = new Button();
      m_butPrevious.Text = "<< Back";
      m_butPrevious.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
      m_butPrevious.Size = new Size(75, 23);
      m_butPrevious.Location = new Point(m_butNext.Left - 6 - m_butPrevious.Width, 12);
      m_butPrevious.Click += Previous_Click;
      mPnlNavigation.Controls.Add(m_butPrevious);

      Dock = DockStyle.Fill;
    }

    #endregion

    /// <summary>
    /// Raises the <see cref="Control.ControlAdded"/> event.
    /// </summary>
    /// <remarks>
    /// This ensures that the wizard buttons are enabled/disabled correctly.
    /// </remarks>
    /// <param name="e">A <see cref="ControlEventArgs"/> describing the event arguments.</param>
    protected override void OnControlAdded(ControlEventArgs e)
    {
      base.OnControlAdded(e);
      if (e.Control is VerticalTabPage)
      {
        MovePage(0);
      }
    }

    /// <summary>
    /// Raises the <see cref="Control.ControlRemoved"/> event.
    /// </summary>
    /// <remarks>
    /// This ensures that the wizard buttons are enabled/disabled correctly.
    /// </remarks>
    /// <param name="e">A <see cref="ControlEventArgs"/> describing the event arguments.</param>
    protected override void OnControlRemoved(ControlEventArgs e)
    {
      base.OnControlRemoved(e);
      if (e.Control is VerticalTabPage)
      {
        MovePage(0);
      }
    }

    #region Page Navigation

    /// <summary>
    /// Handles the <see cref="Control.Click"/> event of the cancel button.
    /// </summary>
    /// <remarks>
    /// This raises the <see cref="Cancelled"/> event.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void Cancel_Click(object sender, EventArgs e)
    {
      Cancelled(this, new EventArgs());
    }

    /// <summary>
    /// This navigates to the page whose index is <see cref="p_intJumpSize"/>
    /// away from the current page's index.
    /// </summary>
    /// <remarks>
    /// This makes sure that the selected index resulting from the jump is never 
    /// out of bounds. It also enables/disables buttons and changes button text as
    /// appropriate.
    /// </remarks>
    /// <param name="p_intJumpSize">The number of pages to jump.</param>
    protected void MovePage(Int32 p_intJumpSize)
    {
      var intNewIndex = SelectedIndex + p_intJumpSize;
      if (intNewIndex < 0)
      {
        intNewIndex = 0;
      }
      else if (intNewIndex >= TabPages.Count)
      {
        intNewIndex = TabPages.Count - 1;
      }

      m_butPrevious.Enabled = (intNewIndex > 0);
      m_butNext.Text = intNewIndex == TabPages.Count - 1 ? "Finish" : "Next >>";
      SelectedIndex = intNewIndex;
    }

    /// <summary>
    /// Handles the <see cref="Control.Click"/> event of the previous button.
    /// </summary>
    /// <remarks>
    /// This navigates to the previous page, if there is one.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void Previous_Click(object sender, EventArgs e)
    {
      MovePage(-1);
    }

    /// <summary>
    /// Handles the <see cref="Control.Click"/> event of the next button.
    /// </summary>
    /// <remarks>
    /// This navigates to the next page, if there is one.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void Next_Click(object sender, EventArgs e)
    {
      if (m_butNext.Text.Equals("Finish"))
      {
        Finished(this, new EventArgs());
      }
      else
      {
        MovePage(1);
      }
    }

    #endregion
  }
}