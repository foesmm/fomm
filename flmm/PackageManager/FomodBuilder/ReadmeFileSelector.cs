using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Fomm.PackageManager.FomodBuilder
{
  /// <summary>
  /// A control for selecting files to concatenate into one Readme file.
  /// </summary>
  public partial class ReadmeFileSelector : UserControl
  {
    /// <summary>
    /// The rich-text formated content of the help box.
    /// </summary>
    private const string HELP_STRING = @"{\rtf1\ansi\ansicpg1252\deff0\deflang4105{\fonttbl{\f0\fnil\fcharset0 Arial;}}
{\*\generator Msftedit 5.41.21.2509;}\viewkind4\uc1\pard{\pntext\f0 1.\tab}{\*\pn\pnlvlbody\pnf0\pnindent0\pnstart1\pndec{\pntxta.}}
\fi-360\li720\sl240\slmult1\lang9\fs18 If the \b Source Files\b0  box does not already contain all of the files you wish to use in the Readme, add them to the \b Source Files\b0  box. You can either drag and drop files and folders, or use the buttons.\par
{\pntext\f0 2.\tab}Browse the Source Files tree and drag the files you want to use to generate your Readme into the \b Readme Files\b0  box.\par
{\pntext\f0 3.\tab}Order the files in the \b Readme Files\b0  box so that they are listed in the order you would like them to appear in the generated Readme file.\par
{\pntext\f0 4.\tab}Select the format you would like to use for your Readme file.\par
}
 ";
    #region Properties

    /// <summary>
    /// Gets the list of files used to generate the readme.
    /// </summary>
    /// <value>The list of files used to generate the readme.</value>
    public List<string> ReadmeFileList
    {
      get
      {
        List<string> lstFiles = new List<string>();
        foreach (ListViewItem lviFile in lvwReadmeFiles.Items)
          lstFiles.Add(lviFile.Name);
        return lstFiles;
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
        return ((KeyValuePair<string, ReadmeFormat>)cbxFormat.SelectedItem).Value;
      }
    }

    /// <summary>
    /// Sets the source files of the selector.
    /// </summary>
    /// <value>The source files of the selector.</value>
    public string[] Sources
    {
      set
      {
        sftSources.Sources = value;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public ReadmeFileSelector()
    {
      InitializeComponent();
      rtbHelp.Rtf = HELP_STRING;

      rtbHelp.Visible = false;

      cbxFormat.Items.Add(new KeyValuePair<string, ReadmeFormat>("Plain Text", ReadmeFormat.PlainText));
      cbxFormat.Items.Add(new KeyValuePair<string, ReadmeFormat>("Rich Text", ReadmeFormat.RichText));
      cbxFormat.Items.Add(new KeyValuePair<string, ReadmeFormat>("HTML", ReadmeFormat.HTML));
      cbxFormat.ValueMember = "Value";
      cbxFormat.DisplayMember = "Key";
      cbxFormat.SelectedIndex = 0;
    }

    #endregion

    /// <summary>
    /// Handles the <see cref="Control.Click"/> event of the help link.
    /// </summary>
    /// <remarks>
    /// This shows/hides the help box as appropriate.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="LinkLabelLinkClickedEventArgs"/> describing the event arguments.</param>
    private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      rtbHelp.Visible = !rtbHelp.Visible;
      lnkHelp.Text = rtbHelp.Visible ? "Close Help" : "Open Help";
    }

    #region Drag and Drop

    /// <summary>
    /// Handles the <see cref="Control.DragOver"/> event of the readme file list.
    /// </summary>
    /// <remarks>
    /// This determines if the item being dragged can be dropped at the current location.
    /// </remarks>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">A <see cref="DragEventArgs"/> that describes the event arguments.</param>
    private void lvwReadmeFiles_DragOver(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(typeof(List<SourceFileTree.SourceFileSystemDragData>)))
        return;
      List<SourceFileTree.SourceFileSystemDragData> lstData = (List<SourceFileTree.SourceFileSystemDragData>)e.Data.GetData(typeof(List<SourceFileTree.SourceFileSystemDragData>));
      bool booFoundFile = false;
      foreach (SourceFileTree.SourceFileSystemDragData sddData in lstData)
        if (!sddData.IsDirectory)
        {
          booFoundFile = true;
          break;
        }
      if (!booFoundFile)
        return;
      e.Effect = DragDropEffects.Copy;
    }

    /// <summary>
    /// Handles the <see cref="Control.DragDrop"/> event of the readme file list.
    /// </summary>
    /// <remarks>
    /// This handles adding the dropped file to the fomod tree.
    /// </remarks>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">A <see cref="DragEventArgs"/> that describes the event arguments.</param>
    private void lvwReadmeFiles_DragDrop(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(typeof(List<SourceFileTree.SourceFileSystemDragData>)))
        return;
      List<SourceFileTree.SourceFileSystemDragData> lstData = (List<SourceFileTree.SourceFileSystemDragData>)e.Data.GetData(typeof(List<SourceFileTree.SourceFileSystemDragData>));
      foreach (SourceFileTree.SourceFileSystemDragData sddData in lstData)
        if (!sddData.IsDirectory)
          lvwReadmeFiles.Items.Add(sddData.Path, Path.GetFileName(sddData.Path), 0);
    }

    #endregion

    /// <summary>
    /// Generates a single readme by concatenating all of the selected files.
    /// </summary>
    /// <returns>A concatenation of all selected file.</returns>
    public string GenerateReadme()
    {
      StringBuilder stbReadme = new StringBuilder();
      string strFileName = null;
      foreach (ListViewItem lviFile in lvwReadmeFiles.Items)
      {
        strFileName = lviFile.Name;
        if (strFileName.StartsWith(Archive.ARCHIVE_PREFIX))
        {
          KeyValuePair<string, string> kvpPath = Archive.ParseArchivePath(strFileName);
          Archive arcArchive = new Archive(kvpPath.Key);
          string strFile = Encoding.UTF8.GetString(arcArchive.GetFileContents(kvpPath.Value));
          stbReadme.Append(strFile).AppendLine().AppendLine();
        }
        else
        {
          if (!File.Exists(strFileName))
            continue;
          stbReadme.Append(File.ReadAllText(strFileName)).AppendLine().AppendLine();
        }
      }
      return stbReadme.ToString();
    }
  }
}
