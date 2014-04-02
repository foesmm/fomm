using System;
using System.Windows.Forms;
using Fomm.Controls;

namespace Fomm.Games.FalloutNewVegas.Settings
{
  /// <summary>
  /// The page of general settings for the Fallout: New Vegas game mode.
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
      Text = "Fallout: New Vegas";
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validates the entered settings values on this page.
    /// </summary>
    /// <returns><lang cref="true"/> if the entered settings values are valid;
    /// <lang cref="false"/> otherwise.</returns>
    protected bool ValidateSettings()
    {
      var booValid = rdcDirectories.ValidateSettings();
      booValid &= ValidateWorkingDirectory();
      return booValid;
    }

    /// <summary>
    /// Validates the selected working directory.
    /// </summary>
    /// <returns><lang cref="true"/> if the selected working directory is valid;
    /// <lang cref="false"/> otherwise.</returns>
    protected bool ValidateWorkingDirectory()
    {
      erpErrors.SetError(butSelectWorkingDirectory, null);
      if (!((FalloutNewVegasGameMode) Program.GameMode).VerifyWorkingDirectory(tbxWorkingDirectory.Text))
      {
        erpErrors.SetError(butSelectWorkingDirectory, "Invalid working directory. Could not find Fallout: New Vegas.");
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
      tbxWorkingDirectory.Text = Properties.Settings.Default.falloutNewVegasWorkingDirectory;
      tbxCommand.Text = Properties.Settings.Default.falloutNewVegasLaunchCommand;
      tbxCommandArguments.Text = Properties.Settings.Default.falloutNewVegasLaunchCommandArgs;
      cbxBoldifyESMs.Checked = Properties.Settings.Default.falloutNewVegasBoldifyESMs;
      rdcDirectories.LoadSettings();
    }

    /// <summary>
    /// Persists the settings from the page's controls.
    /// </summary>
    /// <returns><lang cref="true"/> if ettings were saved;
    /// <lang cref="false"/> otherwise.</returns>
    public override bool SaveSettings()
    {
      if (ValidateSettings())
      {
        rdcDirectories.SaveSettings();
        Properties.Settings.Default.falloutNewVegasLaunchCommand = tbxCommand.Text;
        Properties.Settings.Default.falloutNewVegasLaunchCommandArgs = tbxCommandArguments.Text;
        Properties.Settings.Default.falloutNewVegasWorkingDirectory = tbxWorkingDirectory.Text;
        Properties.Settings.Default.falloutNewVegasBoldifyESMs = cbxBoldifyESMs.Checked;
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