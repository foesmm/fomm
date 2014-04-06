using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;

namespace Fomm.Updater
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class UpdateForm : Form
  {
    private WebClient downloader;
    private string updateLocation;
    private ZipStorer updatePackage;
    private List<ZipStorer.ZipFileEntry> updatePackageCatalog;

    public UpdateForm()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();

      Text = String.Format("{0} Updater v{1}", Fomm.ProductInfo.ShortName, Fomm.ProductInfo.Version);

      label1.Text = "Checking for update";

      downloader = new WebClient();
      downloader.DownloadProgressChanged += (s, e) =>
      {
        progressBar1.Maximum = (int)e.TotalBytesToReceive;
        progressBar1.Value = (int)e.BytesReceived;
      };
      downloader.DownloadFileCompleted += updateFileDownloaded;

      updateLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.zip");

      Shown += UpdateForm_Shown;

      if (File.Exists(String.Format("{0}.tmp", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName)))) File.Delete(String.Format("{0}.tmp", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName)));

      //MessageBox.Show(String.Format("Is Elevated: {0}", UpdateHelper.IsProcessElevated));
      //MessageBox.Show(String.Format("{0}: {1}", AppDomain.CurrentDomain.BaseDirectory, UpdateHelper.HasWriteAccessToFolder(AppDomain.CurrentDomain.BaseDirectory).ToString()));
      //MessageBox.Show(release_notes);

    }

    void UpdateForm_Shown(object sender, EventArgs e)
    {
      Release remote = Release.GetLatest(new GitHub());
      richTextBox1.Text = String.Format("{0}\n\n{1}", remote.Name, remote.Notes);

      if (File.Exists(updateLocation))
      {
        try
        {
          PrepareUpdate();
          updateFileDownloaded(this, null);
        }
        catch (Exception exc)
        {
          progressBar1.Value = 0;
          DownloadUpdate(remote.GetUpdateFile());
        }
        
      }
      else
      {
        Debug.WriteLine(remote.Name);
        Debug.WriteLine(remote.Notes);
        Debug.WriteLine(remote.Version);
        Debug.WriteLine(remote.IsNewer() ? "Newer" : "Older");
        Debug.WriteLine(remote.GetUpdateFile().URL);

        if (remote.IsNewer())
        {
          DownloadUpdate(remote.GetUpdateFile());
        }
        else
        {
          label1.Text = "You have latest version";
        }
      }
    }

    void DownloadUpdate(Release.File updateFile)
    {
      label1.Text = String.Format("Downloading: {0}", updateFile.FileName);
      downloader.DownloadFileAsync(updateFile.URL, updateLocation);
    }

    void PrepareUpdate()
    {
      updatePackage = ZipStorer.Open(updateLocation, System.IO.FileAccess.Read);
      updatePackageCatalog = updatePackage.ReadCentralDir();
    }

    bool SelfUpdate()
    {
      label1.Text = "Updating updater =)";
      string oldUpdater = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);
      ZipStorer.ZipFileEntry updater;
      try
      {
        updater = (from entry in updatePackageCatalog where entry.FilenameInZip == AppDomain.CurrentDomain.FriendlyName select entry).Single();
      }
      catch (InvalidOperationException exc)
      {
        return false;
      }
      if (ZipStorer.CalculateCrc32(oldUpdater) != updater.Crc32)
      {
        File.Move(oldUpdater, String.Format("{0}.tmp", oldUpdater));
        return updatePackage.ExtractFile(updater, oldUpdater); ;
      }

      return false;
    }

    void ApplyUpdateAsync()
    {
      PrepareUpdate();
      if (SelfUpdate())
      {
        UpdateHelper.Restart();
      }

      progressBar1.Maximum = (int)updatePackage.CompressedCatalogSize;
      progressBar1.Value = 0;

      foreach (ZipStorer.ZipFileEntry entry in updatePackageCatalog)
      {
        if (entry.FilenameInZip == AppDomain.CurrentDomain.FriendlyName) continue;
        updatePackage.ExtractFile(entry, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, entry.FilenameInZip));
        progressBar1.Value += (int)entry.CompressedSize;
        label1.Text = String.Format("Extracting: {0}", entry.FilenameInZip);
      }

      CleanUp();
    }

    void CleanUp()
    {
      label1.Text = "Cleaning up";
      updatePackage.Close();
      File.Delete(updateLocation);
      UpdateRegistry();
    }

    void UpdateRegistry()
    {
      label1.Text = "Writting registry";
      UninstallInfo ui = new UninstallInfo(Fomm.ProductInfo.GUID);
      ui.SetVersion(Fomm.ProductInfo.Version);
      label1.Text = "All operations complete";
    }

    void updateFileDownloaded(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      progressBar1.Value = progressBar1.Maximum;
      button1.Enabled = true;
      label1.Text = "Update downloaded";
    }

    private void button1_Click(object sender, EventArgs e)
    {
      ApplyUpdateAsync();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }
  }
}
