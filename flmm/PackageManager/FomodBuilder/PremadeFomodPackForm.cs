using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Fomm.PackageManager.FomodBuilder
{
  /// <summary>
  /// Gathers information required to make a FOMod from a Premade FOMod Pack (PFP).
  /// </summary>
  public partial class PremadeFomodPackForm : Form
  {
    /// <summary>
    /// The open PFP modes.
    /// </summary>
    public enum OpenPFPMode
    {
      /// <summary>
      /// Indicates the PFP is being opened to install it as a FOMod.
      /// </summary>
      Install,

      /// <summary>
      /// Indicates the PFP is being opened for editing.
      /// </summary>
      Edit
    }

    private OpenPFPMode m_omdMode = OpenPFPMode.Install;

    #region Properties

    /// <summary>
    /// Gets or sets the path to the selected PFP.
    /// </summary>
    /// <value>The path to the selected PFP.</value>
    public string PFPPath
    {
      get
      {
        return tbxPFP.Text;
      }
      set
      {
        tbxPFP.Text = value;
      }
    }

    /// <summary>
    /// Gets or sets the path to the directory containing the source files
    /// required for the PFP.
    /// </summary>
    /// <value>The path to the directory containing the source files
    /// required for the PFP.</value>
    public string SourcesPath
    {
      get
      {
        return tbxSources.Text;
      }
      set
      {
        tbxSources.Text = value;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public PremadeFomodPackForm(OpenPFPMode p_omdMode)
    {
      InitializeComponent();

      Icon = Fomm.Properties.Resources.fomm02;
      ofdPFP.InitialDirectory = Properties.Settings.Default.LastPFPPath;
      fbdSources.SelectedPath = Properties.Settings.Default.LastPFPSourcesPath;
      m_omdMode = p_omdMode;
    }

    #endregion

    /// <summary>
    /// Hanldes the <see cref="Control.Click"/> event of the select PFP
    /// button.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void butPFP_Click(object sender, EventArgs e)
    {
      if (ofdPFP.ShowDialog(this) == DialogResult.OK)
      {
        tbxPFP.Text = ofdPFP.FileName;
      }
    }

    /// <summary>
    /// Hanldes the <see cref="Control.Click"/> event of the select source folder
    /// button.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void butSources_Click(object sender, EventArgs e)
    {
      if (fbdSources.ShowDialog(this) == DialogResult.OK)
      {
        tbxSources.Text = fbdSources.SelectedPath;
      }
    }

    /// <summary>
    /// Validates the selected paths.
    /// </summary>
    /// <remarks>
    /// This makes sure the selected PFP file is a valid PFP, and that all required sources are present.
    /// </remarks>
    /// <returns><lang cref="true"/> if the selected paths are valid; <lang cref="false"/> otherwise.</returns>
    protected bool ValidateFiles()
    {
      erpError.Clear();
      if (String.IsNullOrEmpty(tbxPFP.Text))
      {
        erpError.SetError(butPFP, "You must specify a PFP.");
        return false;
      }
      if (!File.Exists(tbxPFP.Text))
      {
        erpError.SetError(butPFP, "File does not exist.");
        return false;
      }

      string strError = PremadeFomodPack.ValidatePFP(tbxPFP.Text);
      if (!String.IsNullOrEmpty(strError))
      {
        erpError.SetError(butPFP, "File is not a valid PFP: " + strError);
        return false;
      }
      PremadeFomodPack pfpPack = new PremadeFomodPack(tbxPFP.Text);
      List<SourceFile> lstSources = pfpPack.GetSources();
      if (lstSources.Count > 0)
      {
        if (String.IsNullOrEmpty(tbxSources.Text))
        {
          erpError.SetError(butSources, "You must specify a folder containing the required source files.");
          return false;
        }
        if (!Directory.Exists(tbxSources.Text))
        {
          erpError.SetError(butSources, "Folder does not exist.");
          return false;
        }
        List<SourceFile> lstMissingSources = new List<SourceFile>();
        List<SourceFile> lstMissingHiddenSources = new List<SourceFile>();
        bool booSourceMissing = false;
        foreach (SourceFile sflSource in lstSources)
        {
          booSourceMissing = !File.Exists(Path.Combine(tbxSources.Text, sflSource.Source)) &&
                             !Directory.Exists(Path.Combine(tbxSources.Text, sflSource.Source));
          if ((!sflSource.Hidden || sflSource.Generated) && booSourceMissing)
          {
            lstMissingSources.Add(sflSource);
          }
          if ((sflSource.Hidden && !sflSource.Generated) && booSourceMissing)
          {
            lstMissingHiddenSources.Add(sflSource);
          }
        }
        if ((lstMissingSources.Count > 0) || ((m_omdMode == OpenPFPMode.Edit) && (lstMissingHiddenSources.Count > 0)))
        {
          erpError.SetError(butSources, "Missing sources.");

          StringBuilder stbErrorHtml = new StringBuilder("<html><body bgcolor=\"");
          stbErrorHtml.AppendFormat("#{0:x6}", Color.FromKnownColor(KnownColor.Control).ToArgb() & 0x00ffffff);
          stbErrorHtml.Append("\">");
          bool booMissingGeneratedFiles = false;
          if (lstMissingSources.Count > 0)
          {
            stbErrorHtml.Append("The following sources are missing:<ul>");
            foreach (SourceFile sflSource in lstMissingSources)
            {
              if (!sflSource.Generated)
              {
                stbErrorHtml.AppendFormat("<li><a href=\"{0}\">{1}</a></li>", sflSource.URL, sflSource.SourceFileName)
                            .AppendLine();
              }
              else
              {
                booMissingGeneratedFiles = true;
                stbErrorHtml.AppendFormat("<li>{0}</li>", sflSource.SourceFileName).AppendLine();
              }
            }
            stbErrorHtml.AppendLine("</ul>");
          }
          if (lstMissingHiddenSources.Count > 0)
          {
            stbErrorHtml.Append("Some of the missing sources require the following files to be downloaded:<ul>");
            foreach (SourceFile sflSource in lstMissingHiddenSources)
            {
              stbErrorHtml.AppendFormat("<li><a href=\"{0}\">{1}</a></li>", sflSource.URL, sflSource.SourceFileName)
                          .AppendLine();
            }
            stbErrorHtml.AppendLine("</ul>");
          }
          if (booMissingGeneratedFiles)
          {
            stbErrorHtml.Append(
              "You are missing some files that need to be created before FOMM can open the Premade FOMod Pack (PFP). ");
            stbErrorHtml.Append("Please read the <b>howto.txt</b> file included in the PFP.");
          }

          stbErrorHtml.Append("</body></html>");
          ShowHTML(stbErrorHtml.ToString());
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Shows an HTML page.
    /// </summary>
    protected void ShowHTML(string p_strHTML)
    {
      Form frmHTMLPreview = new Form();
      frmHTMLPreview.Size = new Size(500, 500);
      frmHTMLPreview.Icon = Fomm.Properties.Resources.fomm02;
      frmHTMLPreview.ShowInTaskbar = false;
      frmHTMLPreview.StartPosition = FormStartPosition.CenterParent;
      WebBrowser wbrBrowser = new WebBrowser();
      frmHTMLPreview.Controls.Add(wbrBrowser);
      wbrBrowser.Dock = DockStyle.Fill;
      frmHTMLPreview.Text = "Missing Sources";
      wbrBrowser.WebBrowserShortcutsEnabled = false;
      wbrBrowser.AllowWebBrowserDrop = false;
      wbrBrowser.DocumentText = p_strHTML;
      wbrBrowser.Navigating += ((s, e) =>
      {
        e.Cancel = true;
        System.Diagnostics.Process.Start(e.Url.ToString());
      });
      frmHTMLPreview.ShowDialog(this);
    }

    private void butOK_Click(object sender, EventArgs e)
    {
      Properties.Settings.Default.LastPFPPath = Path.GetDirectoryName(tbxPFP.Text);
      if (!String.IsNullOrEmpty(tbxSources.Text))
      {
        Properties.Settings.Default.LastPFPSourcesPath = Path.GetDirectoryName(tbxSources.Text);
      }
      Properties.Settings.Default.Save();
      if (ValidateFiles())
      {
        DialogResult = DialogResult.OK;
      }
    }
  }
}