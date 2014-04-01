﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using Fomm.Properties;

namespace Fomm.PackageManager
{
  public partial class EditScriptForm : Form
  {
    #region Properties

    /// <summary>
    /// Gets or sets the <see cref="FomodScript"/> being edited.
    /// </summary>
    /// <value>The <see cref="FomodScript"/> being edited.</value>
    public FomodScript Script
    {
      get
      {
        return fseScriptEditor.Script;
      }
      set
      {
        fseScriptEditor.Script = value;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public EditScriptForm()
    {
      InitializeComponent();

      Icon = Resources.fomm02;
      Settings.Default.windowPositions.GetWindowPosition("EditScriptForm", this);
    }

    #endregion

    /// <summary>
    /// Raises the <see cref="Form.Closing"/> event.
    /// </summary>
    /// <remarks>
    /// Saves the window's position.
    /// </remarks>
    /// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
    protected override void OnClosing(CancelEventArgs e)
    {
      Settings.Default.windowPositions.SetWindowPosition("EditScriptForm", this);
      Settings.Default.Save();
      base.OnClosing(e);
    }

    /// <summary>
    /// Handles the <see cref="Control.Click"/> event of the OK button.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void butOK_Click(object sender, EventArgs e)
    {
      if (fseScriptEditor.IsValid ||
          (MessageBox.Show(this, "The script is not valid." + Environment.NewLine + "Are you sure you want to save?",
                           "Invalid Script", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK))
      {
        DialogResult = DialogResult.OK;
      }
    }
  }
}