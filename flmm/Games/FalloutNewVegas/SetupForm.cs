using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Fomm.Controls;
using System.IO;

namespace Fomm.Games.FalloutNewVegas
{
  /// <summary>
  /// This is the setup form for the Fallout: New Vegas game mode.
  /// </summary>
  public partial class SetupForm : Form
  {
    #region Contructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public SetupForm()
    {
      InitializeComponent();
      this.Icon = Properties.Resources.fomm02;
      rdcDirectories.LoadSettings();
    }

    #endregion

    #region Navigation

    /// <summary>
    /// Handles the <see cref="WizardControl.SelectedTabPageChanged"/> event of the wizard control.
    /// </summary>
    /// <remarks>
    /// This validates each page as it is navigated away from.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A <see cref="VerticalTabControl.TabPageEventArgs"/> describing the event arguments.</param>
    private void wizSetup_SelectedTabPageChanged(object sender, VerticalTabControl.TabPageEventArgs e)
    {
      if (e.TabPage == vtpDirectories)
      {
        if (!rdcDirectories.ValidateSettings())
          wizSetup.SelectedTabPage = e.TabPage;
      }
    }

    /// <summary>
    /// Handles the <see cref="WizardControl.Cancelled"/> event of the wizard control.
    /// </summary>
    /// <remarks>
    /// This cancels the wizard.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void wizSetup_Cancelled(object sender, EventArgs e)
    {
      if (MessageBox.Show(this, "If you cancel the setup FOMM will close.", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
        DialogResult = DialogResult.Cancel;
    }

    /// <summary>
    /// Handles the <see cref="WizardControl.Finished"/> event of the wizard control.
    /// </summary>
    /// <remarks>
    /// This finishes the wizard and persists the selected values.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void wizSetup_Finished(object sender, EventArgs e)
    {
      if (rdcDirectories.ValidateSettings())
      {
        rdcDirectories.SaveSettings();
        Properties.Settings.Default.Save();
        DialogResult = DialogResult.OK;
      }
    }

    #endregion
  }
}
