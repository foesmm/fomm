using System;
using System.Windows.Forms;
using System.IO;
using Fomm.Properties;

namespace Fomm.Games
{
  /// <summary>
  /// Prompts the user to select the working directory.
  /// </summary>
  /// <remarks>
  /// This form also provides an auto-detect feature if the user is unsure of which folder to select.
  /// </remarks>
  public partial class WorkingDirectorySelectionForm : Form
  {
    private string[] m_strSearchFiles;
    private BackgroundWorkerProgressDialog m_bwdProgress;
    private string m_strFoundWorkingDirectory;

    #region Properties

    /// <summary>
    /// Gets the selected working directory.
    /// </summary>
    /// <value>The selected working directory.</value>
    public string WorkingDirectory
    {
      get
      {
        return tbxWorkingDirectory.Text;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_strMessage">The message to display in the window.</param>
    /// <param name="p_strLabel">The label of the working directory textbox.</param>
    /// <param name="p_strSearchFiles">The files to search for when auto-detecting.</param>
    public WorkingDirectorySelectionForm(string p_strMessage, string p_strLabel, string[] p_strSearchFiles)
    {
      m_strSearchFiles = p_strSearchFiles;
      InitializeComponent();
      Icon = Resources.fomm02;
      autosizeLabel1.Text = p_strMessage;
      label2.Text = p_strLabel;
    }

    #endregion

    /// <summary>
    /// Handles the <see cref="Control.Click"/> event of the elipses button next to
    /// the working directory textbox.
    /// </summary>
    /// <remarks>
    /// This opens the folder selection dialog so the use can select the working directory.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void butSelect_Click(object sender, EventArgs e)
    {
      fbdWorkingDirectory.SelectedPath = tbxWorkingDirectory.Text;
      if (fbdWorkingDirectory.ShowDialog() != DialogResult.Cancel)
      {
        tbxWorkingDirectory.Text = fbdWorkingDirectory.SelectedPath;
      }
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

    /// <summary>
    /// Handles the <see cref="Control.Click"/> event of the auto-detect button.
    /// </summary>
    /// <remarks>
    /// This launches the auto-detection algorithm on another process using the
    /// <see cref="BackgroundWorkerProgressDialog"/> class.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void butAutoDetect_Click(object sender, EventArgs e)
    {
      using (m_bwdProgress = new BackgroundWorkerProgressDialog(AutoDetectWokringDirectory))
      {
        m_bwdProgress.ShowItemProgress = false;
        m_bwdProgress.OverallProgressMarquee = true;
        m_strFoundWorkingDirectory = null;
        if (m_bwdProgress.ShowDialog(this) == DialogResult.Cancel)
        {
          m_strFoundWorkingDirectory = null;
        }
      }
      if (!String.IsNullOrEmpty(m_strFoundWorkingDirectory))
      {
        tbxWorkingDirectory.Text = m_strFoundWorkingDirectory;
      }
      else
      {
        MessageBox.Show(this, "Could not find Fallout 3.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    /// <summary>
    /// This searches for the working directory.
    /// </summary>
    protected void AutoDetectWokringDirectory()
    {
      var difDrives = DriveInfo.GetDrives();

      foreach (var difDrive in difDrives)
      {
        if (difDrive.DriveType == DriveType.CDRom)
        {
          continue;
        }
        var strFound = Search(difDrive.Name);
        if (!String.IsNullOrEmpty(strFound))
        {
          m_strFoundWorkingDirectory = strFound;
          return;
        }
      }
    }

    /// <summary>
    /// This recursively searches the specified directory for the search files.
    /// </summary>
    /// <param name="p_strPath">The path of the direcotry to recursively search.</param>
    protected string Search(string p_strPath)
    {
      m_bwdProgress.OverallMessage = p_strPath;
      foreach (var strSearchFile in m_strSearchFiles)
      {
        if (m_bwdProgress.Cancelled())
        {
          return null;
        }
        try
        {
          var strFoundFiles = Directory.GetFiles(p_strPath, strSearchFile, SearchOption.TopDirectoryOnly);
          foreach (var strFoundFile in strFoundFiles)
          {
            if (
              MessageBox.Show(m_bwdProgress,
                              "Found: " + Path.GetDirectoryName(strFoundFile) + Environment.NewLine + "Is this correct?",
                              "Found File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
              return Path.GetDirectoryName(strFoundFile);
            }
          }
        }
        catch (UnauthorizedAccessException)
        {
          //we don't have access to the path we are trying to search, so let's bail
          return null;
        }
      }
      var strDirectories = Directory.GetDirectories(p_strPath);
      foreach (var strDirectory in strDirectories)
      {
        if (m_bwdProgress.Cancelled())
        {
          return null;
        }
        if (Path.GetFileName(p_strPath).StartsWith("$"))
        {
          continue;
        }
        var strFound = Search(strDirectory);
        if (!String.IsNullOrEmpty(strFound))
        {
          return strFound;
        }
      }
      return null;
    }
  }
}