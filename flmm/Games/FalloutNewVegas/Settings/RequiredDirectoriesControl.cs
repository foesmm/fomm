using System;
using System.Windows.Forms;
using System.IO;

namespace Fomm.Games.FalloutNewVegas.Settings
{
  /// <summary>
  /// A control that encapsulates the management of the critical directory settings.
  /// </summary>
  public partial class RequiredDirectoriesControl : UserControl
  {
    #region

    /// <summary>
    /// The default constructor.
    /// </summary>
    public RequiredDirectoriesControl()
    {
      InitializeComponent();
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validates the specified directory.
    /// </summary>
    /// <returns><lang langref="true"/> if the specified directory is valid;
    /// <lang langref="false"/> otherwise.</returns>
    protected bool ValidateDirectory(string p_strPath, string p_strPathName, Control p_ctlErrorControl)
    {
      erpValidator.SetError(p_ctlErrorControl, null);
      if (String.IsNullOrEmpty(p_strPath))
      {
        erpValidator.SetError(p_ctlErrorControl, String.Format("You must select a {0}.", p_strPathName));
        return false;
      }
      if (!Directory.Exists(p_strPath))
      {
        if (
          MessageBox.Show(this,
                          String.Format("The selected {0} does not exist.{1}Would you like to create it?", p_strPathName,
                                        Environment.NewLine), "Missing Directory", MessageBoxButtons.YesNo,
                          MessageBoxIcon.Question) == DialogResult.Yes)
        {
          Directory.CreateDirectory(p_strPath);
          return true;
        }
        erpValidator.SetError(p_ctlErrorControl, String.Format("{0} does not exist.", p_strPathName));
        return false;
      }
      return true;
    }

    /// <summary>
    /// Validates the selected mod directory.
    /// </summary>
    /// <returns><lang langref="true"/> if the selected mod directory is valid;
    /// <lang langref="false"/> otherwise.</returns>
    protected bool ValidateModDirectory()
    {
      return ValidateDirectory(tbxModDirectory.Text, "Mod Directory", butSelectModDirectory);
    }

    /// <summary>
    /// Validates the selected install info directory.
    /// </summary>
    /// <returns><lang langref="true"/> if the selected install info directory is valid;
    /// <lang langref="false"/> otherwise.</returns>
    protected bool ValidateInstallInfoDirectory()
    {
      return ValidateDirectory(tbxInstallInfo.Text, "Install Info Directory", butSelectInfoDirectory);
    }

    /// <summary>
    /// Validates the settings on this control.
    /// </summary>
    /// <returns><lang langref="true"/> if the settings are valid;
    /// <lang langref="false"/> otherwise.</returns>
    public bool ValidateSettings()
    {
      return ValidateModDirectory() && ValidateInstallInfoDirectory();
    }

    #endregion

    /// <summary>
    /// Loads the user's settings into the control.
    /// </summary>
    public void LoadSettings()
    {
      tbxModDirectory.Text = Properties.Settings.Default.falloutNewVegasModDirectory;
      if (String.IsNullOrEmpty(tbxModDirectory.Text))
      {
        var strDefault = Path.Combine(Path.GetDirectoryName(Program.GameMode.PluginsPath), "mods");
        if (strDefault.StartsWith(Path.Combine(Path.GetPathRoot(strDefault), "Program Files"),
                                  StringComparison.InvariantCultureIgnoreCase))
        {
          strDefault = Path.Combine(Path.GetPathRoot(Program.GameMode.PluginsPath), "Games\\FalloutNV\\mods");
        }
        tbxModDirectory.Text = strDefault;
      }
      tbxInstallInfo.Text = Properties.Settings.Default.falloutNewVegasInstallInfoDirectory;
      if (String.IsNullOrEmpty(tbxInstallInfo.Text))
      {
        var strDefault = Path.Combine(Path.GetDirectoryName(Program.GameMode.PluginsPath), "Install Info");
        if (strDefault.StartsWith(Path.Combine(Path.GetPathRoot(strDefault), "Program Files"),
                                  StringComparison.InvariantCultureIgnoreCase))
        {
          strDefault = Path.Combine(Path.GetPathRoot(Program.GameMode.PluginsPath), "Games\\FalloutNV\\Install Info");
        }
        tbxInstallInfo.Text = strDefault;
      }
    }

    /// <summary>
    /// Persists the settings on this control.
    /// </summary>
    public void SaveSettings()
    {
      Properties.Settings.Default.falloutNewVegasModDirectory = tbxModDirectory.Text;
      Properties.Settings.Default.falloutNewVegasInstallInfoDirectory = tbxInstallInfo.Text;
    }

    /// <summary>
    /// Handles the <see cref="Control.Click"/> event of the select mod directory button.
    /// </summary>
    /// <remarks>
    /// This opens the folder selection dialog for the mod directory.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void butSelectModDirectory_Click(object sender, EventArgs e)
    {
      fbdDirectory.SelectedPath = tbxModDirectory.Text;
      if (fbdDirectory.ShowDialog(this) == DialogResult.OK)
      {
        tbxModDirectory.Text = fbdDirectory.SelectedPath;
      }
    }

    /// <summary>
    /// Handles the <see cref="Control.Click"/> event of the select install info button.
    /// </summary>
    /// <remarks>
    /// This opens the folder selection dialog for the install info directory.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void butSelectInfoDirectory_Click(object sender, EventArgs e)
    {
      fbdDirectory.SelectedPath = tbxInstallInfo.Text;
      if (fbdDirectory.ShowDialog(this) == DialogResult.OK)
      {
        tbxInstallInfo.Text = fbdDirectory.SelectedPath;
      }
    }
  }
}