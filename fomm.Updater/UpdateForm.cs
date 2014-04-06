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

      downloader = new WebClient();
      downloader.DownloadProgressChanged += updateFileDownloadProgressChanged;
      downloader.DownloadFileCompleted += updateFileDownloaded;

      Shown += UpdateForm_Shown;

      if (File.Exists(String.Format("{0}.tmp", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName)))) File.Delete(String.Format("{0}.tmp", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName)));

      //MessageBox.Show(String.Format("Is Elevated: {0}", UpdateHelper.IsProcessElevated));
      //MessageBox.Show(String.Format("{0}: {1}", AppDomain.CurrentDomain.BaseDirectory, UpdateHelper.HasWriteAccessToFolder(AppDomain.CurrentDomain.BaseDirectory).ToString()));
      //MessageBox.Show(release_notes);

    }

    void UpdateForm_Shown(object sender, EventArgs e)
    {
      updateLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.zip");
      if (File.Exists(updateLocation))
      {
        ApplyUpdate();
      }
      else
      {
        Release remote = Release.GetLatest(new GitHub());
        Debug.WriteLine(remote.Name);
        Debug.WriteLine(remote.Notes);
        Debug.WriteLine(remote.Version);
        Debug.WriteLine(remote.IsNewer() ? "Newer" : "Older");
        Debug.WriteLine(remote.GetUpdateFile().URL);

        if (true || remote.IsNewer())
        {
          DownloadUpdate(remote.GetUpdateFile());
        }
      }
    }

    void DownloadUpdate(Release.File updateFile)
    {
      downloader.DownloadFileAsync(updateFile.URL, updateLocation);
    }

    void PrepareUpdate()
    {
      updatePackage = ZipStorer.Open(updateLocation, System.IO.FileAccess.Read);
      updatePackageCatalog = updatePackage.ReadCentralDir();
    }

    bool SelfUpdate()
    {
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

    void ApplyUpdate()
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
        Text = entry.FilenameInZip;
      }

      CleanUp();
    }

    void CleanUp()
    {
      UpdateRegistry();
    }

    void UpdateRegistry()
    {
      UninstallInfo ui = new UninstallInfo(Fomm.ProductInfo.GUID);
      ui.SetVersion(Fomm.ProductInfo.Version);
    }

    void updateFileDownloaded(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      if (!e.Cancelled && e.Error == null)
      {
        ApplyUpdate();
      }
    }

    void updateFileDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      progressBar1.Value = e.ProgressPercentage;
    }

  }
}
