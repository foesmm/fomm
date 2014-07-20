using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Fomm.PackageManager.FomodBuilder;
using Fomm.PackageManager.Upgrade;
using Fomm.Properties;
using Fomm.Util;
using SevenZip;

namespace Fomm.PackageManager
{
  /// <summary>
  ///   The mod manager window.
  /// </summary>
  internal partial class PackageManager : Form
  {
    private readonly List<fomod> mods = new List<fomod>();
    private readonly List<string> groups;
    private readonly List<string> lgroups;
    private readonly MainForm mf;
    private BackgroundWorkerProgressDialog m_bwdProgress;
    private string m_strLastFromFolderPath;
    private Dictionary<string, string> m_dicWebVersions = new Dictionary<string, string>();

    public PackageManager(MainForm mf)
    {
      this.mf = mf;
      InitializeComponent();

      CheckFOModCache();

      Icon = Resources.fomm02;
      cmbSortOrder.ContextMenu = new ContextMenu();
      lvModList.ListViewItemSorter = new FomodSorter();
      Settings.Default.windowPositions.GetWindowPosition("PackageManager", this);
      sbtAddFomod.SelectedItemIndex = Settings.Default.SelectedAddFomodAction;
      m_strLastFromFolderPath = Settings.Default.LastBuildFOMODFromFolderPath;

      foreach (var modpath in Program.GetFiles(Program.GameMode.ModDirectory, "*.fomod.zip"))
      {
        if (!File.Exists(Path.ChangeExtension(modpath, null)))
        {
          File.Move(modpath, Path.ChangeExtension(modpath, null));
        }
      }

      string[] groups = Settings.Default.pluginGroups;
      this.groups = new List<string>(groups);
      lgroups = new List<string>(groups.Length);
      foreach (var group in groups)
      {
        lgroups.Add(group.ToLowerInvariant());
      }

      cbGroups.Checked = Settings.Default.PackageManagerShowsGroups;

      WebsiteLogin();

      foreach (var modpath in Program.GetFiles(Program.GameMode.ModDirectory, "*.fomod"))
      {
        AddFomod(modpath, false);
      }

      RebuildListView();
    }

    /// <summary>
    ///   This removes any old cache files.
    /// </summary>
    protected void CheckFOModCache()
    {
      var strCaches = Directory.GetFiles(Program.GameMode.ModInfoCacheDirectory);
      foreach (var strCache in strCaches)
      {
        var strFOModPath = Path.Combine(Program.GameMode.ModDirectory,
                                        Path.GetFileNameWithoutExtension(strCache) + ".fomod");
        if (!File.Exists(strFOModPath) || (File.GetLastWriteTimeUtc(strCache) < File.GetLastWriteTimeUtc(strFOModPath)))
        {
          FileUtil.ForceDelete(strCache);
        }
      }
    }

    private void AddFomodToList(fomod mod)
    {
      string strWebVersion;
      m_dicWebVersions.TryGetValue(mod.BaseName, out strWebVersion);

      if (lvModList.Items.ContainsKey(mod.BaseName))
      {
        lvModList.Items.RemoveByKey(mod.BaseName);
      }

      if (!cbGroups.Checked)
      {
        var lvi = new ListViewItem(new[]
        {
          mod.ModName, mod.HumanReadableVersion, strWebVersion, mod.Author
        });
        lvi.Tag = mod;
        lvi.Name = mod.BaseName;
        lvi.Checked = mod.IsActive;
        lvi.SubItems[2].Name = "WebVersion";
        lvModList.Items.Add(lvi);
        return;
      }
      var added = false;
      for (var i = 0; i < groups.Count; i++)
      {
        if (Array.IndexOf(mod.Groups, lgroups[i]) != -1)
        {
          added = true;
          var lvi = new ListViewItem(new[]
          {
            mod.ModName, mod.HumanReadableVersion, strWebVersion, mod.Author
          });
          lvi.Tag = mod;
          lvi.Name = mod.BaseName;
          lvi.Checked = mod.IsActive;
          lvi.SubItems[2].Name = "WebVersion";
          lvModList.Items.Add(lvi);
          lvModList.Groups[i + 1].Items.Add(lvi);
        }
      }
      if (!added)
      {
        var lvi = new ListViewItem(new[]
        {
          mod.ModName, mod.HumanReadableVersion, strWebVersion, mod.Author
        });
        lvi.Tag = mod;
        lvi.Name = mod.BaseName;
        lvi.Checked = mod.IsActive;
        lvi.SubItems[2].Name = "WebVersion";
        lvModList.Items.Add(lvi);
        lvModList.Groups[0].Items.Add(lvi);
      }
    }

    private void RebuildListView()
    {
      lvModList.SuspendLayout();
      lvModList.Clear();
      lvModList.Groups.Clear();

      if (!cbGroups.Checked)
      {
        lvModList.ShowGroups = false;
      }
      else
      {
        var lvg = new ListViewGroup("No group");
        lvModList.Groups.Add(lvg);

        foreach (var group in groups)
        {
          lvg = new ListViewGroup(group);
          lvModList.Groups.Add(lvg);
        }
        lvModList.ShowGroups = true;
      }

      if (lvModList.Columns.Count == 0)
      {
        lvModList.Columns.Add("Name");
        lvModList.Columns.Add("Version");
        lvModList.Columns.Add("Web Version");
        lvModList.Columns.Add("Author");
        Int32[] intColumnWidths = Settings.Default.PackageManagerColumnWidths;
        for (var i = 0; i < intColumnWidths.Length; i++)
        {
          lvModList.Columns[i].Width = intColumnWidths[i];
        }
      }

      foreach (var mod in mods)
      {
        AddFomodToList(mod);
      }

      lvModList.ResumeLayout();
    }

    private void ReaddFomodToList(fomod mod)
    {
      lvModList.SuspendLayout();
      for (var i = 0; i < lvModList.Items.Count; i++)
      {
        if (lvModList.Items[i].Tag == mod)
        {
          lvModList.Items.RemoveAt(i--);
        }
      }
      AddFomodToList(mod);
      lvModList.ResumeLayout();
    }

    private void AddFomod(string modpath, bool addToList)
    {
      fomod mod;
      try
      {
        mod = new fomod(modpath);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error loading '" + Path.GetFileName(modpath) + "'\n" + ex.Message);
        return;
      }
      for (var i = mods.Count - 1; i >= 0; i--)
      {
        if (mods[i].filepath.Equals(mod.filepath))
        {
          mods.RemoveAt(i);
          break;
        }
      }
      mods.Add(mod);
      if (addToList)
      {
        AddFomodToList(mod);
      }
    }

    /// <summary>
    ///   Logins into the websites.
    /// </summary>
    /// <remarks>
    ///   If needed, credentials are gathered from the user.
    /// </remarks>
    private void WebsiteLogin()
    {
      if (!Settings.Default.checkForNewModVersionsInitialized)
      {
        Settings.Default.checkForNewModVersions = false;
        Settings.Default.checkForNewModVersionsInitialized = true;
        Settings.Default.Save();
      }

      if (!Settings.Default.addMissingInfoToModsInitialized)
      {
        Settings.Default.addMissingInfoToMods = false;
        Settings.Default.addMissingInfoToModsInitialized = true;
        Settings.Default.Save();
      }
    }

    private void PackageManager_Load(object sender, EventArgs e)
    {
      var tmp = Settings.Default.PackageManagerPanelSplit;
      if (tmp > 0)
      {
        splitContainer1.SplitterDistance = Math.Max(splitContainer1.Panel1MinSize + 1,
                                                    Math.Min(
                                                      splitContainer1.Height - (splitContainer1.Panel2MinSize + 1), tmp));
      }
    }

    private void lvModList_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateModStateText();
    }

    /// <summary>
    ///   Updates the UI elements to reflect the current mod's state.
    /// </summary>
    /// <remarks>
    ///   This updates elements such as button text and the displayed description.
    /// </remarks>
    protected void UpdateModStateText()
    {
      if (lvModList.SelectedItems.Count == 0)
      {
        return;
      }
      var mod = (fomod) lvModList.SelectedItems[0].Tag;
      tbModInfo.Text = mod.HasInfo
        ? mod.Description
        : "No description is associaited with this fomod. Click 'edit info' if you want to add one.";

      butDeactivate.Enabled = mod.IsActive;
      butViewReadme.Enabled = mod.HasReadme;
      bActivate.Text = mod.IsActive ? "Reactivate" : "Activate";
      bEditScript.Text = mod.HasInstallScript ? "Edit script" : "Create script";
      pictureBox1.Image = mod.GetScreenshotImage();
    }

    private void lvModList_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      e.NewValue = ((fomod) lvModList.Items[e.Index].Tag).IsActive ? CheckState.Checked : CheckState.Unchecked;
    }

    private void bEditScript_Click(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      var mod = (fomod) lvModList.SelectedItems[0].Tag;
      var esfEditor = new EditScriptForm();
      esfEditor.Script = !mod.HasInstallScript
        ? new FomodScript(FomodScriptType.CSharp, Program.GameMode.DefaultCSharpScript)
        : mod.GetInstallScript();
      if (esfEditor.ShowDialog(this) == DialogResult.OK)
      {
        mod.SetScript(esfEditor.Script);
      }
      UpdateModStateText();
    }

    private void butViewReadme_Click(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      var mod = (fomod) lvModList.SelectedItems[0].Tag;
      if (mod.HasReadme)
      {
        var vrfEditor = new ViewReadmeForm(mod.GetReadme());
        vrfEditor.ShowDialog(this);
      }
    }

    private void PackageManager_FormClosing(object sender, FormClosingEventArgs e)
    {
      Settings.Default.windowPositions.SetWindowPosition("PackageManager", this);
      Settings.Default.SelectedAddFomodAction = sbtAddFomod.SelectedItemIndex;
      Settings.Default.PackageManagerPanelSplit = splitContainer1.SplitterDistance;
      var intColumnWidths = new Int32[lvModList.Columns.Count];
      for (var i = 0; i < lvModList.Columns.Count; i++)
      {
        intColumnWidths[i] = lvModList.Columns[i].Width;
      }
      Settings.Default.PackageManagerColumnWidths = intColumnWidths;
      Settings.Default.Save();

      foreach (ListViewItem lvi in lvModList.Items)
      {
        ((fomod) lvi.Tag).Dispose();
      }
    }

    private void bEditInfo_Click(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      var mod = (fomod) lvModList.SelectedItems[0].Tag;
      if ((new InfoEditor(mod)).ShowDialog() == DialogResult.OK)
      {
        if (cbGroups.Checked)
        {
          ReaddFomodToList(mod);
        }
        else
        {
          var lvi = lvModList.SelectedItems[0];
          lvi.SubItems[0].Text = mod.ModName;
          lvi.SubItems[1].Text = mod.HumanReadableVersion;
          lvi.SubItems[3].Text = mod.Author;
          tbModInfo.Text = mod.Description;
          pictureBox1.Image = mod.GetScreenshotImage();
        }
        UpdateModStateText();
      }
    }

    /// <summary>
    ///   Activates the given fomod.
    /// </summary>
    /// <remarks>
    ///   This method checks to see if the given fomod could be an upgrade for another fomod.
    /// </remarks>
    /// <param name="mod">The fomod to activate.</param>
    private void ActivateFomod(fomod mod)
    {
      var booFound = false;
      foreach (ListViewItem lviFomod in lvModList.Items)
      {
        var fomodMod = (fomod) lviFomod.Tag;
        if (fomodMod.ModName.Equals(mod.ModName) && fomodMod.IsActive && !fomodMod.BaseName.Equals(mod.BaseName))
        {
          //ask to do upgrade
          var strUpgradeMessage =
            "A different version of {0} has been detected. The installed version is {1}, the new version is {2}. Would you like to upgrade?" +
            Environment.NewLine + "Selecting No will install the new FOMod normally.";
          switch (
            MessageBox.Show(
              String.Format(strUpgradeMessage, fomodMod.ModName, fomodMod.HumanReadableVersion, mod.HumanReadableVersion),
              "Upgrade", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
          {
            case DialogResult.Yes:
              var mduUpgrader = new ModUpgrader(mod, fomodMod.BaseName);
              mduUpgrader.Upgrade();
              if (mod.IsActive)
              {
                fomodMod.IsActive = false;
                lviFomod.Checked = false;
              }
              return;
            case DialogResult.No:
              booFound = true;
              break;
          }
        }
        if (booFound)
        {
          break;
        }
      }
      var mdiInstaller = new ModInstaller(mod);
      mdiInstaller.Install();
    }

    /// <summary>
    ///   Activates, Reactivates, or Deactivates the selected mod as appropriate.
    /// </summary>
    /// <param name="mod">The mod to act upon.</param>
    /// <param name="p_booReactivate">If this is a reativation request.</param>
    private void ToggleActivation(fomod mod, bool p_booReactivate)
    {
      if (!mod.IsActive)
      {
        ActivateFomod(mod);
      }
      else if (p_booReactivate)
      {
        var mraReactivator = new ModReactivator(mod);
        mraReactivator.Upgrade();
      }
      else
      {
        var mduUninstaller = new ModUninstaller(mod);
        mduUninstaller.Uninstall();
      }
      if (cbGroups.Checked)
      {
        foreach (ListViewItem lvi in lvModList.Items)
        {
          if (lvi.Tag == mod)
          {
            lvi.Checked = mod.IsActive;
          }
        }
      }
      else
      {
        lvModList.SelectedItems[0].Checked = mod.IsActive;
      }
      butDeactivate.Enabled = mod.IsActive;
      bActivate.Text = !mod.IsActive ? "Activate" : "Reactivate";

      mf.RefreshPluginList();
    }

    private void bActivate_Click(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      var mod = (fomod) lvModList.SelectedItems[0].Tag;
      ToggleActivation(mod, mod.IsActive);
    }

    private void butDeactivate_Click(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      var mod = (fomod) lvModList.SelectedItems[0].Tag;
      ToggleActivation(mod, false);
    }

    /// <summary>
    ///   Creates a fomod from a source archive.
    /// </summary>
    /// <param name="p_strPath">The path to the archive from which to create the fomod.</param>
    public void AddNewFomod(string p_strPath)
    {
      var ffbBuilder = new FomodFromSourceBuilder();
      var lstFomodPaths = ffbBuilder.BuildFomodFromSource(p_strPath);
      foreach (var strFomodPath in lstFomodPaths)
      {
        AddFomod(strFomodPath, true);
      }
    }

    private void fomodContextMenu_Opening(object sender, CancelEventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        e.Cancel = true;
        return;
      }
      var mod = (fomod) lvModList.SelectedItems[0].Tag;
      emailAuthorToolStripMenuItem.Visible = mod.Email.Length != 0;
      visitWebsiteToolStripMenuItem.Visible = mod.Website.Length != 0;
    }

    private void visitWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      var mod = (fomod) lvModList.SelectedItems[0].Tag;
      try
      {
        Process.Start(mod.Website);
      }
      catch (Win32Exception ex)
      {
        MessageBox.Show(this, "Error launching site: " + mod.Website + "\n" + ex.Message, "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
      }
    }

    private void emailAuthorToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      var mod = (fomod) lvModList.SelectedItems[0].Tag;
      Process.Start("mailto://" + mod.Email, "");
    }

    private void cbGroups_CheckedChanged(object sender, EventArgs e)
    {
      RebuildListView();
      Settings.Default.PackageManagerShowsGroups = cbGroups.Checked;
      Settings.Default.Save();
      bActivateGroup.Enabled = cbGroups.Checked;
      bDeactivateGroup.Enabled = cbGroups.Checked;
      cmbSortOrder.Enabled = !cbGroups.Checked;
    }

    private void bEditGroups_Click(object sender, EventArgs e)
    {
      var f = new Form();
      Settings.Default.windowPositions.GetWindowPosition("GroupEditor", f);
      f.Text = "Groups";
      var tb = new TextBox();
      f.Controls.Add(tb);
      tb.Dock = DockStyle.Fill;
      tb.AcceptsReturn = true;
      tb.Multiline = true;
      tb.ScrollBars = ScrollBars.Vertical;
      tb.Text = string.Join(Environment.NewLine, groups.ToArray());
      tb.Select(0, 0);
      f.FormClosing += delegate
      {
        Settings.Default.windowPositions.SetWindowPosition("GroupEditor", f);
        Settings.Default.Save();
      };
      f.ShowDialog();
      groups.Clear();
      groups.AddRange(tb.Lines);
      for (var i = 0; i < groups.Count; i++)
      {
        if (groups[i].Length == 0)
        {
          groups.RemoveAt(i--);
        }
      }
      lgroups.Clear();
      foreach (var group in groups)
      {
        lgroups.Add(group.ToLowerInvariant());
      }
      RebuildListView();
      Settings.Default.pluginGroups = groups;
      Settings.Default.Save();
    }

    private void fomodStatusToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      var mod = (fomod) lvModList.SelectedItems[0].Tag;
      var f = new Form();
      Settings.Default.windowPositions.GetWindowPosition("FomodStatus", f);
      f.Text = "Fomod status";
      var tb = new TextBox();
      f.Controls.Add(tb);
      tb.Dock = DockStyle.Fill;
      tb.Multiline = true;
      tb.Text = mod.GetStatusString();
      tb.ReadOnly = true;
      tb.BackColor = SystemColors.Window;
      tb.Select(0, 0);
      tb.ScrollBars = ScrollBars.Vertical;
      f.ShowDialog();
      Settings.Default.windowPositions.SetWindowPosition("FomodStatus", f);
      Settings.Default.Save();
    }

    private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      var mod = (fomod) lvModList.SelectedItems[0].Tag;
      if (mod.IsActive)
      {
        MessageBox.Show("Cannot delete an active fomod");
        return;
      }
      for (var i = 0; i < lvModList.Items.Count; i++)
      {
        if (lvModList.Items[i].Tag == mod)
        {
          lvModList.Items.RemoveAt(i--);
        }
      }
      mod.Dispose();
      File.Delete(mod.filepath);
      File.Delete(mod.CachePath);
      mods.Remove(mod);
    }

    private void bActivateGroup_Click(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      if (!cbGroups.Checked)
      {
        return;
      }
      foreach (ListViewItem lvi in lvModList.SelectedItems[0].Group.Items)
      {
        var mod = (fomod) lvi.Tag;
        if (mod.IsActive)
        {
          continue;
        }
        ActivateFomod(mod);
        if (cbGroups.Checked)
        {
          foreach (ListViewItem lvi2 in lvModList.Items)
          {
            if (lvi2.Tag == mod)
            {
              lvi2.Checked = mod.IsActive;
            }
          }
        }
        else
        {
          lvModList.SelectedItems[0].Checked = mod.IsActive;
        }
      }

      mf.RefreshPluginList();
    }

    private void bDeactivateGroup_Click(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      if (!cbGroups.Checked)
      {
        return;
      }
      foreach (ListViewItem lvi in lvModList.SelectedItems[0].Group.Items)
      {
        var mod = (fomod) lvi.Tag;
        if (!mod.IsActive)
        {
          continue;
        }
        var mduUninstaller = new ModUninstaller(mod);
        mduUninstaller.Uninstall(true);
        if (cbGroups.Checked)
        {
          foreach (ListViewItem lvi2 in lvModList.Items)
          {
            if (lvi2.Tag == mod)
            {
              lvi2.Checked = mod.IsActive;
            }
          }
        }
        else
        {
          lvModList.SelectedItems[0].Checked = mod.IsActive;
        }
      }

      mf.RefreshPluginList();
    }

    private void bDeactivateAll_Click(object sender, EventArgs e)
    {
      if (
        MessageBox.Show("This will deactivate all fomods.\nAre you sure you want to continue?", "Warning",
                        MessageBoxButtons.YesNo) != DialogResult.Yes)
      {
        return;
      }
      foreach (ListViewItem lvi in lvModList.Items)
      {
        var mod = (fomod) lvi.Tag;
        if (!mod.IsActive)
        {
          continue;
        }
        var mduUninstaller = new ModUninstaller(mod);
        mduUninstaller.Uninstall(true);
      }

      foreach (ListViewItem lvi in lvModList.Items)
      {
        lvi.Checked = false;
      }

      mf.RefreshPluginList();
    }

    private class FomodSorter : IComparer
    {
      public static int Mode;

      public int Compare(object a, object b)
      {
        var m1 = (fomod) ((ListViewItem) a).Tag;
        var m2 = (fomod) ((ListViewItem) b).Tag;

        switch (Mode)
        {
          case 0:
            return 0;
          case 1:
            return m1.BaseName.CompareTo(m2.BaseName);
          case 2:
            return m1.ModName.CompareTo(m2.ModName);
          case 3:
            return m1.Author.CompareTo(m2.Author);
        }
        return 0;
      }
    }

    private void cmbSortOrder_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cmbSortOrder.SelectedIndex < 0)
      {
        return;
      }
      FomodSorter.Mode = cmbSortOrder.SelectedIndex + 1;
      lvModList.Sort();
      cmbSortOrder.Text = "Sort order";
    }

    private void cmbSortOrder_KeyPress(object sender, KeyPressEventArgs e)
    {
      e.Handled = true;
    }

    private void lvModList_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (cbGroups.Checked)
      {
        return;
      }
      switch (e.Column)
      {
        case 0:
          cmbSortOrder.SelectedIndex = 1;
          break;
        case 2:
          cmbSortOrder.SelectedIndex = 2;
          break;
      }
    }

    private void lvModList_ItemActivate(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      var mod = (fomod) lvModList.SelectedItems[0].Tag;
      ToggleActivation(mod, false);
    }

    private void pictureBox1_Click(object sender, EventArgs e)
    {
      if (pictureBox1.Image != null &&
          (pictureBox1.Image.Size.Width > pictureBox1.Width || pictureBox1.Image.Size.Height > pictureBox1.Height))
      {
        (new ImageForm(pictureBox1.Image)).ShowDialog();
      }
    }

    #region Fomod Extraction

    /// <summary>
    ///   Handles the <see cref="Button.Click" /> event of the extract button.
    /// </summary>
    /// <remarks>
    ///   This queries the user for the destinations directory, and then launches the
    ///   background process to extract the fomod.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void butExtractFomod_Click(object sender, EventArgs e)
    {
      if (lvModList.SelectedItems.Count != 1)
      {
        return;
      }
      var fomodMod = (fomod) lvModList.SelectedItems[0].Tag;

      if (fbdExtractFomod.ShowDialog(this) == DialogResult.OK)
      {
        using (m_bwdProgress = new BackgroundWorkerProgressDialog(UnpackFomod))
        {
          var strOutput = Path.Combine(fbdExtractFomod.SelectedPath,
                                       Path.GetFileNameWithoutExtension(fomodMod.filepath));
          if (!Directory.Exists(strOutput))
          {
            Directory.CreateDirectory(strOutput);
          }
          m_bwdProgress.WorkMethodArguments = new Pair<fomod, string>(fomodMod, strOutput);
          m_bwdProgress.ShowDialog();
        }
        m_bwdProgress = null;
      }
    }

    /// <summary>
    ///   Unpacks the given fomod to the specified directory.
    /// </summary>
    /// <remarks>
    ///   This method is used by the background worker.
    /// </remarks>
    /// <param name="p_objArgs">
    ///   A Pair{fomod, string} containing the fomod to extract
    ///   and the direcotry to which to extract it.
    /// </param>
    private void UnpackFomod(object p_objArgs)
    {
      if (!(p_objArgs is Pair<fomod, string>))
      {
        throw new ArgumentException("Given argument is not a Pair<fomod,string>.", "p_objArgs");
      }
      var fomodMod = ((Pair<fomod, string>) p_objArgs).Key;
      var strOutput = ((Pair<fomod, string>) p_objArgs).Value;
      var lstFiles = fomodMod.GetFileList();

      m_bwdProgress.ShowItemProgress = false;
      m_bwdProgress.OverallMessage = "Extracting Files...";
      m_bwdProgress.OverallProgressMaximum = lstFiles.Count;
      m_bwdProgress.OverallProgressStep = 1;

      using (var szeExtractor = new SevenZipExtractor(fomodMod.filepath))
      {
        szeExtractor.FileExtractionFinished += UnpackFomod_FileExtractionFinished;
        szeExtractor.FileExtractionStarted += UnpackFomod_FileExtractionStarted;
        szeExtractor.ExtractArchive(strOutput);
      }
    }

    /// <summary>
    ///   Called when a file has been extracted from a fomod.
    /// </summary>
    /// <remarks>
    ///   This steps the progress of the create fomod from archive progress dialog.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void UnpackFomod_FileExtractionFinished(object sender, FileInfoEventArgs e)
    {
      m_bwdProgress.StepItemProgress();
    }

    /// <summary>
    ///   Called when a file is about to be extracted from a fomod.
    /// </summary>
    /// <remarks>
    ///   This cancels the compression if the user has clicked the cancel button of the progress dialog.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A <see cref="FileNameEventArgs" /> describing the event arguments.</param>
    private void UnpackFomod_FileExtractionStarted(object sender, FileInfoEventArgs e)
    {
      e.Cancel = m_bwdProgress.Cancelled();
    }

    #endregion

    /// <summary>
    ///   Handles the <see cref="Control.Click" /> event of the add fomod button.
    /// </summary>
    /// <remarks>
    ///   Adds a fomod to the package manager.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void addFOMODToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (openFileDialog1.ShowDialog() != DialogResult.OK)
      {
        return;
      }
      AddNewFomod(openFileDialog1.FileName);
    }

    /// <summary>
    ///   Handles the <see cref="Control.Click" /> event of the create fomod from folder button.
    /// </summary>
    /// <remarks>
    ///   Creates a fomod from a folder.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void createFromFolderToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var fbd = new FolderBrowserDialog();
      fbd.SelectedPath = m_strLastFromFolderPath;
      fbd.ShowNewFolderButton = false;
      fbd.Description = "Pick a folder to convert to a fomod";
      if (fbd.ShowDialog() != DialogResult.OK)
      {
        return;
      }
      m_strLastFromFolderPath = fbd.SelectedPath;
      Settings.Default.LastBuildFOMODFromFolderPath = Path.GetDirectoryName(m_strLastFromFolderPath);
      Settings.Default.Save();
      AddNewFomod(fbd.SelectedPath);
    }

    /// <summary>
    ///   Handles the <see cref="Control.Click" /> event of the create fomod button.
    /// </summary>
    /// <remarks>
    ///   Creates a fomod using the fomod builder.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void createFOMODToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var fbfBuilder = new FomodBuilderForm();
      if (fbfBuilder.ShowDialog(this) == DialogResult.OK)
      {
        if (!String.IsNullOrEmpty(fbfBuilder.FomodPath))
        {
          AddFomod(fbfBuilder.FomodPath, true);
        }
      }
    }

    /// <summary>
    ///   Handles the <see cref="Control.Click" /> event of the add pfp button.
    /// </summary>
    /// <remarks>
    ///   Creates a FOMod from a PFP.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void addPFPToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var pkfPFPForm = new PremadeFomodPackForm(PremadeFomodPackForm.OpenPFPMode.Install);
      if (pkfPFPForm.ShowDialog(this) == DialogResult.Cancel)
      {
        return;
      }

      var pfpPack = new PremadeFomodPack(pkfPFPForm.PFPPath);
      var lstCopyInstructions = pfpPack.GetCopyInstructions(pkfPFPForm.SourcesPath);
      var strPremadeSource = Archive.GenerateArchivePath(pkfPFPForm.PFPPath, pfpPack.PremadePath);
      lstCopyInstructions.Add(new KeyValuePair<string, string>(strPremadeSource, "/"));

      var fgnGenerator = new NewFomodBuilder();
      var strNewFomodPath = fgnGenerator.BuildFomod(pfpPack.FomodName, lstCopyInstructions, null, null, false, null,
                                                    null);
      if (!String.IsNullOrEmpty(strNewFomodPath))
      {
        AddFomod(strNewFomodPath, true);
      }
    }

    /// <summary>
    ///   Handles the <see cref="Control.Click" /> event of the edit pfp button.
    /// </summary>
    /// <remarks>
    ///   Edits a PFP.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void editPFPToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var pkfPFPForm = new PremadeFomodPackForm(PremadeFomodPackForm.OpenPFPMode.Edit);
      if (pkfPFPForm.ShowDialog(this) == DialogResult.Cancel)
      {
        return;
      }

      var pfpPack = new PremadeFomodPack(pkfPFPForm.PFPPath);
      var fbfBuilder = new FomodBuilderForm(pfpPack, pkfPFPForm.SourcesPath);
      if (fbfBuilder.ShowDialog(this) == DialogResult.OK)
      {
        if (!String.IsNullOrEmpty(fbfBuilder.FomodPath))
        {
          AddFomod(fbfBuilder.FomodPath, true);
        }
      }
    }

    /// <summary>
    ///   Exports the list of mods being managed by FOMM.
    /// </summary>
    /// <remarks>
    ///   The list of mods is export to a file of the user's choosing. Optionally, only active mods
    ///   can be exported.
    /// </remarks>
    /// <param name="p_booActiveOnly">Whether only active mods should be exported.</param>
    protected void ExportModList(bool p_booActiveOnly)
    {
      var sfdModList = new SaveFileDialog();
      sfdModList.Filter = "Text file (*.txt)|*.txt";
      sfdModList.AddExtension = true;
      sfdModList.RestoreDirectory = true;
      if (sfdModList.ShowDialog() != DialogResult.OK)
      {
        return;
      }

      var intMaxNameLength = 0;
      var intMaxVersionLength = 0;
      for (var i = 0; i < lvModList.Items.Count; i++)
      {
        if (lvModList.Items[i].Checked || !p_booActiveOnly)
        {
          if (lvModList.Items[i].Text.Length > intMaxNameLength)
          {
            intMaxNameLength = lvModList.Items[i].Text.Length;
          }
          if (lvModList.Items[i].SubItems[1].Text.Length > intMaxVersionLength)
          {
            intMaxVersionLength = lvModList.Items[i].SubItems[1].Text.Length;
          }
        }
      }

      var swrModList = new StreamWriter(sfdModList.FileName);
      try
      {
        for (var i = 0; i < lvModList.Items.Count; i++)
        {
          if (lvModList.Items[i].Checked || !p_booActiveOnly)
          {
            swrModList.WriteLine("[{0}] {1,-" + intMaxNameLength + "}\t{2,-" + intMaxVersionLength + "}\t{3}",
                                 (lvModList.Items[i].Checked ? "X" : " "), lvModList.Items[i].Text,
                                 lvModList.Items[i].SubItems[1].Text, ((fomod) lvModList.Items[i].Tag).filepath);
          }
        }
      }
      finally
      {
        swrModList.Close();
      }
    }

    /// <summary>
    ///   Handles the <see cref="Control.Click" /> event of the Export Mod List menu item.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void exportModListToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ExportModList(false);
    }

    /// <summary>
    ///   Handles the <see cref="Control.Click" /> event of the Export Active Mod List menu item.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void exportActiveModListToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ExportModList(true);
    }
  }
}