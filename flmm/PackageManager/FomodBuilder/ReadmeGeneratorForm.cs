using System;
using System.ComponentModel;
using System.Windows.Forms;
using Fomm.Properties;

namespace Fomm.PackageManager.FomodBuilder
{
  /// <summary>
  /// A form that generates a readme by concatenating several selected files.
  /// </summary>
  public partial class ReadmeGeneratorForm : Form
  {
    #region Properties

    /// <summary>
    /// Sets the source files of the generator.
    /// </summary>
    /// <value>The source files of the generator.</value>
    public string[] Sources
    {
      set
      {
        rfsSelector.Sources = value;
      }
    }

    /// <summary>
    /// Gets the selected format for the readme.
    /// </summary>
    /// <value>The selected format for the readme.</value>
    public ReadmeFormat Format
    {
      get
      {
        return rfsSelector.Format;
      }
    }

    /// <summary>
    /// Gets the generated readme.
    /// </summary>
    /// <value>The generated readme.</value>
    public string GeneratedReadme
    {
      get
      {
        return rfsSelector.GenerateReadme();
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public ReadmeGeneratorForm()
    {
      InitializeComponent();

      Icon = Resources.fomm02;
      Settings.Default.windowPositions.GetWindowPosition("ReadmeGeneratorForm", this);
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
      Settings.Default.windowPositions.SetWindowPosition("ReadmeGeneratorForm", this);
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
      DialogResult = DialogResult.OK;
    }
  }
}