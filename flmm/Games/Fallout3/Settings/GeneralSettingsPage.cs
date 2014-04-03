using System;
using System.Windows.Forms;
using Fomm.Controls;

namespace Fomm.Games.Fallout3.Settings
{
  /// <summary>
  /// The page of general settings for the Fallout 3 game mode.
  /// </summary>
  public partial class GeneralSettingsPage : SettingsPage
  {
    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public GeneralSettingsPage()
    {
      InitializeComponent();
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validates the entered settings values on this page.
    /// </summary>
    /// <returns><lang langref="true"/> if the entered settings values are valid;
    /// <lang langref="false"/> otherwise.</returns>
    protected bool ValidateSettings()
    {
      var booValid = rdcDirectories.ValidateSettings();
      booValid &= ValidateWorkingDirectory();
      return booValid;
    }

    /// <summary>
    /// Validates the selected working directory.
    /// </summary>
    /// <returns><lang langref="true"/> if the selected working directory is valid;
    /// <lang langref="false"/> otherwise.</returns>
    protected bool ValidateWorkingDirectory()
    {
      erpErrors.SetError(butSelectWorkingDirectory, null);
      if (!((Fallout3GameMode) Program.GameMode).VerifyWorkingDirectory(tbxWorkingDirectory.Text))
      {
        erpErrors.SetError(butSelectWorkingDirectory, "Invalid working directory. Could not find Fallout 3.");
        return false;
      }
      return true;
    }

    #endregion

    #region Settings Management

    /// <summary>
    /// Loads the settings into the page's controls.
    /// </summary>
    public override void LoadSettings()
    {
      tbxWorkingDirectory.Text = Properties.Settings.Default.fallout3WorkingDirectory;
      tbxCommand.Text = Properties.Settings.Default.fallout3LaunchCommand;
      tbxCommandArguments.Text = Properties.Settings.Default.fallout3LaunchCommandArgs;
      cbxBoldifyESMs.Checked = Properties.Settings.Default.fallout3BoldifyESMs;
      rdcDirectories.LoadSettings();
    }

    /// <summary>
    /// Persists the settings from the page's controls.
    /// </summary>
    /// <returns><lang langref="true"/> if ettings were saved;
    /// <lang langref="false"/> otherwise.</returns>
    public override bool SaveSettings()
    {
      if (ValidateSettings())
      {
        rdcDirectories.SaveSettings();
        Properties.Settings.Default.fallout3LaunchCommand = tbxCommand.Text;
        Properties.Settings.Default.fallout3LaunchCommandArgs = tbxCommandArguments.Text;
        Properties.Settings.Default.fallout3WorkingDirectory = tbxWorkingDirectory.Text;
        Properties.Settings.Default.fallout3BoldifyESMs = cbxBoldifyESMs.Checked;
        return true;
      }
      return false;
    }

    #endregion

    /// <summary>
    /// Handles the <see cref="Control.Click"/> event of the select working directory button.
    /// </summary>
    /// <remarks>
    /// This opens the folder selection dialog for the selection of the working directory.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void butSelectWorkingDirectory_Click(object sender, EventArgs e)
    {
      fbdWorkingDirectory.SelectedPath = tbxWorkingDirectory.Text;
      if (fbdWorkingDirectory.ShowDialog(FindForm()) == DialogResult.OK)
      {
        tbxWorkingDirectory.Text = fbdWorkingDirectory.SelectedPath;
      }
    }
  }
}