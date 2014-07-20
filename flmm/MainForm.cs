using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting;
using System.Text;
using System.Windows.Forms;
using Fomm.Commands;
using Fomm.Properties;

namespace Fomm
{
  public partial class MainForm : Form
  {
    private bool AlphaSortMode;
    private PluginFormat.PluginFormatterManager m_pfmPluginFormatManager = new PluginFormat.PluginFormatterManager();

    #region Properties

    /// <summary>
    ///   Gets whether or not to change the game mode.
    /// </summary>
    /// <value>Whether or not to change the game mode.</value>
    public bool ChangeGameMode { get; private set; }

    /// <summary>
    ///   Gets whether there are any open utility windows.
    /// </summary>
    /// <value>Whether there are any open utility windows.</value>
    public bool HasOpenUtilityWindows
    {
      get
      {
        var intIngoredWindowCount = 0;
        for (var i = Application.OpenForms.Count - 1; i >= 0; i--)
        {
          var frmForm = Application.OpenForms[i];
          if (frmForm.GetType().Namespace.StartsWith("ICSharp", StringComparison.InvariantCultureIgnoreCase))
          {
            intIngoredWindowCount++;
          }
        }
        return (Application.OpenForms.Count - intIngoredWindowCount > 1);
      }
    }

    /// <summary>
    ///   Gets a list of currently selected plugins.
    /// </summary>
    /// <value>A list of currently selected plugins.</value>
    public IList<string> SelectedPlugins
    {
      get
      {
        var lstSelectedPlugins = new List<string>();
        foreach (ListViewItem lviPlugin in lvEspList.SelectedItems)
        {
          lstSelectedPlugins.Add(lviPlugin.Text);
        }
        return lstSelectedPlugins;
      }
    }

    /// <summary>
    ///   Gets whether the package manager is open.
    /// </summary>
    /// <value>Whether the package manager is open.</value>
    public bool IsPackageManagerOpen
    {
      get
      {
        return PackageManagerForm != null;
      }
    }

    #endregion

    #region Constructors

    public MainForm(string fomod)
    {
      InitializeComponent();
      Icon = Resources.fomm02;
      Settings.Default.windowPositions.GetWindowPosition("MainForm", this);

      if (fomod != null)
      {
        bPackageManager_Click(null, null);
        if (fomod.Length > 0)
        {
          PackageManagerForm.AddNewFomod(fomod);
        }
      }

      var cmdGameLaunch = Program.GameMode.LaunchCommand;
      bLaunch.Text = cmdGameLaunch.Name;
      bLaunch.Tag = cmdGameLaunch;

      if (!Settings.Default.DisableIPC)
      {
        var newFommTimer = new Timer();
        try
        {
          newFommTimer.Interval = 1000;
          newFommTimer.Tick += newFommTimer_Tick;
          newFommTimer.Start();
          Messaging.ServerSetup(RecieveMessage);
        }
        catch (RemotingException)
        {
          newFommTimer.Stop();
          newFommTimer.Enabled = false;
          Settings.Default.DisableIPC = true;
          Settings.Default.Save();
        }
      }

      SetupTools();
      SetupPluginFormatProviders();
    }

    private void SetupPluginFormatProviders()
    {
      foreach (var pfpProvider in Program.GameMode.PluginFormatProviders)
      {
        m_pfmPluginFormatManager.RegisterProvider(pfpProvider);
      }
    }

    #endregion

    /// <summary>
    ///   Adds the game-specific tools to the Tool menu.
    /// </summary>
    protected void SetupTools()
    {
      foreach (var cmdTool in Program.GameMode.Tools)
      {
        var tsiMenuItem = new ToolStripMenuItem();
        new ToolStripMenuItemCommandBinding<MainForm>(tsiMenuItem, cmdTool, () => this);
        toolsToolStripMenuItem.DropDownItems.Add(tsiMenuItem);
      }

      foreach (var cmdTool in Program.GameMode.GameSettingsTools)
      {
        var tsiMenuItem = new ToolStripMenuItem();
        new ToolStripMenuItemCommandBinding<MainForm>(tsiMenuItem, cmdTool, () => this);
        gameSettingsToolStripMenuItem.DropDownItems.Add(tsiMenuItem);
      }

      foreach (var cmdTool in Program.GameMode.RightClickTools)
      {
        var tsiMenuItem = new ToolStripMenuItem();
        new ToolStripMenuItemCommandBinding<MainForm>(tsiMenuItem, cmdTool, () => this);
        cmsPlugins.Items.Add(tsiMenuItem);
      }

      foreach (var cmdTool in Program.GameMode.LoadOrderTools)
      {
        var tsiMenuItem = new ToolStripMenuItem();
        new ToolStripMenuItemCommandBinding<MainForm>(tsiMenuItem, cmdTool, () => this);
        loadOrderToolStripMenuItem.DropDownItems.Add(tsiMenuItem);
      }

      foreach (var cmdTool in Program.GameMode.GameLaunchCommands)
      {
        var tsiMenuItem = new ToolStripMenuItem();
        new ToolStripMenuItemCommandBinding<MainForm>(tsiMenuItem, cmdTool, () => this);
        launchGameToolStripMenuItem.DropDownItems.Add(tsiMenuItem);
      }
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
      var tmp = Settings.Default.MainFormPanelSplit;
      if (tmp > 0)
      {
        splitContainer1.SplitterDistance = Math.Max(splitContainer1.Panel1MinSize + 1,
                                                    Math.Min(
                                                      splitContainer1.Height - (splitContainer1.Panel2MinSize + 1), tmp));
      }

      Int32[] intColumnWidths = Settings.Default.MainFormColumnWidths;
      if (intColumnWidths != null)
      {
        for (var i = 0; i < intColumnWidths.Length; i++)
        {
          lvEspList.Columns[i].Width = intColumnWidths[i];
        }
      }

      Program.GameMode.BuildPluginList();
      RefreshPluginList();
      exportLoadOrder(Path.Combine(Program.GameMode.InstallInfoDirectory, "load order backup.txt"));
    }

    private void lvEspList_DragDrop(object sender, DragEventArgs e)
    {
      if (AlphaSortMode)
      {
        MessageBox.Show("Cannot change load order when sorting by file name", "Error");
      }
      else
      {
        for (var i = 0; i < lvEspList.Items.Count; i++)
        {
          Program.GameMode.PluginManager.SetLoadOrder(
            Path.Combine(Program.GameMode.PluginsPath, lvEspList.Items[i].Text), i);
        }

        RefreshIndexCounts();
      }
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      for (var i = Application.OpenForms.Count - 1; i >= 0; i--)
      {
        var frmForm = Application.OpenForms[i];
        if (frmForm.GetType().Namespace.StartsWith("ICSharp", StringComparison.InvariantCultureIgnoreCase))
        {
          frmForm.Close();
          frmForm.Dispose();
        }
      }
      if (Application.OpenForms.Count > 1)
      {
        MessageBox.Show("Please close all utility windows before closing the program.");
        e.Cancel = true;
        return;
      }

      Settings.Default.windowPositions.SetWindowPosition("MainForm", this);
      Settings.Default.MainFormPanelSplit = splitContainer1.SplitterDistance;

      var intColumnWidths = new Int32[lvEspList.Columns.Count];
      foreach (ColumnHeader chdHeader in lvEspList.Columns)
      {
        intColumnWidths[chdHeader.Index] = chdHeader.Width;
      }
      Settings.Default.MainFormColumnWidths = intColumnWidths;
      Settings.Default.Save();
    }

    public void LoadPluginInfo()
    {
      if (lvEspList.SelectedItems.Count == 1)
      {
        var strPluginName = lvEspList.SelectedItems[0].Text;
        var pifInfo =
          Program.GameMode.PluginManager.GetPluginInfo(Path.Combine(Program.GameMode.PluginsPath, strPluginName));
        var stbDescription = new StringBuilder(pifInfo.Description);

        var pftFormat = m_pfmPluginFormatManager.GetFormat(strPluginName);
        if (!String.IsNullOrEmpty(pftFormat.Message))
        {
          stbDescription.Append(@"\par ");
          stbDescription.Append(pftFormat.Message);
        }
        stbDescription.AppendLine().Append("}");
        rtbPluginInfo.Rtf = stbDescription.ToString();
        pictureBox1.Image = pifInfo.Picture;
      }
    }

    private void lvEspList_SelectedIndexChanged(object sender, EventArgs e)
    {
      LoadPluginInfo();
    }

    private PackageManager.PackageManager PackageManagerForm;

    private void bPackageManager_Click(object sender, EventArgs e)
    {
      if (PackageManagerForm != null)
      {
        PackageManagerForm.Focus();
      }
      else
      {
        PackageManagerForm = new PackageManager.PackageManager(this);
        PackageManagerForm.FormClosed += delegate
        {
          RefreshPluginList();
          PackageManagerForm = null;
        };
        PackageManagerForm.Show();
      }
    }

    private FileManager.FileManager m_fmgFileManagerForm;

    /// <summary>
    ///   Handles the <see cref="Button.Click" /> event of the file manager button.
    /// </summary>
    /// <remarks>
    ///   Displays the file manager.
    /// </remarks>
    /// <param name="sender">The object that trigger the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void butFileManager_Click(object sender, EventArgs e)
    {
      if (m_fmgFileManagerForm != null)
      {
        m_fmgFileManagerForm.Focus();
      }
      else
      {
        m_fmgFileManagerForm = new FileManager.FileManager();
        m_fmgFileManagerForm.FormClosed += delegate
        {
          m_fmgFileManagerForm = null;
        };
        m_fmgFileManagerForm.Show();
      }
    }

    private void RefreshIndexCounts()
    {
      if (lvEspList.Items.Count != 0)
      {
        var add = lvEspList.Items[0].SubItems.Count == 1;
        var strPlugins =
          Program.GameMode.PluginManager.SortPluginList(Program.GameMode.PluginManager.ActivePluginList);

        for (var i = 0; i < strPlugins.Length; i++)
        {
          strPlugins[i] = Path.GetFileName(strPlugins[i].Trim()).ToLowerInvariant();
        }

        foreach (ListViewItem lvi in lvEspList.Items)
        {
          var i = Array.IndexOf(strPlugins, lvi.Text.ToLowerInvariant());
          if (i != -1)
          {
            if (add)
            {
              lvi.Checked = true;
              lvi.SubItems.Add(i.ToString("X2"));
            }
            else
            {
              lvi.SubItems[1].Text = i.ToString("X2");
            }
          }
          else
          {
            if (add)
            {
              lvi.SubItems.Add("NA");
            }
            else
            {
              lvi.SubItems[1].Text = "NA";
            }
          }
        }
        Program.GameMode.BuildPluginList();
        rerunPluginFormatters();
      }
    }

    protected void rerunPluginFormatters()
    {
      lvEspList.BeginUpdate();

      foreach (ListViewItem lviPlugin in lvEspList.Items)
      {
        var pftFormat = m_pfmPluginFormatManager.GetFormat(Path.GetFileName(lviPlugin.Text));

        lviPlugin.Font = pftFormat.ResolveFont(lviPlugin.Font);
        if (pftFormat.Colour.HasValue)
        {
          lviPlugin.ForeColor = pftFormat.Colour.Value;
        }

        if (pftFormat.Highlight.HasValue)
        {
          lviPlugin.BackColor = pftFormat.Highlight.Value;
        }
      }
      lvEspList.EndUpdate();
    }

    public void RefreshPluginList()
    {
      Program.GameMode.BuildPluginList();

      RefreshingList = true;
      lvEspList.BeginUpdate();
      lvEspList.Items.Clear();

      var lstPluginFilenames = new List<string>(Program.GameMode.PluginManager.OrderedPluginList);
      if (AlphaSortMode)
      {
        lstPluginFilenames.Sort(delegate(string a, string b)
        {
          return Path.GetFileName(a).CompareTo(Path.GetFileName(b));
        });
      }

      var lstPluginViewItems = new List<ListViewItem>();
      foreach (var strPlugin in lstPluginFilenames)
      {
        var pftFormat = m_pfmPluginFormatManager.GetFormat(Path.GetFileName(strPlugin));
        var lviPlugin = new ListViewItem(Path.GetFileName(strPlugin));
        lviPlugin.Font = pftFormat.ResolveFont(lviPlugin.Font);
        if (pftFormat.Colour.HasValue)
        {
          lviPlugin.ForeColor = pftFormat.Colour.Value;
        }

        if (pftFormat.Highlight.HasValue)
        {
          lviPlugin.BackColor = pftFormat.Highlight.Value;
        }

        lstPluginViewItems.Add(lviPlugin);
      }

      lvEspList.Items.AddRange(lstPluginViewItems.ToArray());
      RefreshIndexCounts();
      lvEspList.EndUpdate();
      RefreshingList = false;
    }

    #region toolbuttons

    private void bLaunch_Click(object sender, EventArgs e)
    {
      if (Application.OpenForms.Count > 1)
      {
        MessageBox.Show("Please close all utility windows before launching fallout");
        return;
      }
      ((Command<MainForm>) ((Button) sender).Tag).Execute(this);
    }

    private void bHelp_Click(object sender, EventArgs e)
    {
      Process.Start(Path.Combine(Program.ProgrammeInfoDirectory, "fomm.chm"));
    }

    #endregion

    private void CommitLoadOrder(int position, int[] indicies)
    {
      if (AlphaSortMode)
      {
        MessageBox.Show("Cannot change load order when sorting by file name", "Error");
      }
      else
      {
        Array.Sort(indicies);
        var items = new List<ListViewItem>();
        RefreshingList = true;
        lvEspList.BeginUpdate();
        var intLoadOrder = 0;
        for (var i = 0; i < position; i++)
        {
          if (Array.BinarySearch(indicies, i) < 0)
          {
            Program.GameMode.PluginManager.SetLoadOrder(
              Path.Combine(Program.GameMode.PluginsPath, lvEspList.Items[i].Text), intLoadOrder++);
            items.Add(lvEspList.Items[i]);
            items[items.Count - 1].Selected = false;
          }
        }

        foreach (var index in indicies)
        {
          Program.GameMode.PluginManager.SetLoadOrder(
            Path.Combine(Program.GameMode.PluginsPath, lvEspList.Items[index].Text), intLoadOrder++);
          items.Add(lvEspList.Items[index]);
          items[items.Count - 1].Selected = true;
        }

        for (var i = position; i < lvEspList.Items.Count; i++)
        {
          if (Array.BinarySearch(indicies, i) < 0)
          {
            Program.GameMode.PluginManager.SetLoadOrder(
              Path.Combine(Program.GameMode.PluginsPath, lvEspList.Items[i].Text), intLoadOrder++);
            items.Add(lvEspList.Items[i]);
            items[items.Count - 1].Selected = false;
          }
        }
        lvEspList.Items.Clear();
        lvEspList.Items.AddRange(items.ToArray());
        RefreshIndexCounts();
        lvEspList.EndUpdate();
        RefreshingList = false;
        lvEspList.EnsureVisible(position == lvEspList.Items.Count ? position - 1 : position);
      }
    }

    private bool RefreshingList;

    private void lvEspList_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      if (!RefreshingList)
      {
        if (e.Item.Checked)
        {
          Program.GameMode.PluginManager.ActivatePlugin(e.Item.Text);
        }
        else
        {
          Program.GameMode.PluginManager.DeactivatePlugin(e.Item.Text);
        }
        RefreshIndexCounts();
      }
    }

    private void sendToTopToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (lvEspList.SelectedIndices.Count != 0)
      {
        var toswap = new int[lvEspList.SelectedIndices.Count];
        for (var i = 0; i < lvEspList.SelectedIndices.Count; i++)
        {
          toswap[i] = lvEspList.SelectedIndices[i];
        }
        Array.Sort(toswap);
        CommitLoadOrder(0, toswap);
      }
    }

    private void sendToBottomToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (lvEspList.SelectedIndices.Count != 0)
      {
        var toswap = new int[lvEspList.SelectedIndices.Count];
        for (var i = 0; i < lvEspList.SelectedIndices.Count; i++)
        {
          toswap[i] = lvEspList.SelectedIndices[i];
        }
        Array.Sort(toswap);
        CommitLoadOrder(lvEspList.Items.Count, toswap);
      }
    }

    private void copyLoadOrderToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var sb = new StringBuilder();
      var lvis = new ListViewItem[lvEspList.CheckedItems.Count];
      for (var i = 0; i < lvEspList.CheckedItems.Count; i++)
      {
        lvis[i] = lvEspList.CheckedItems[i];
      }

      Array.Sort(lvis, delegate(ListViewItem a, ListViewItem b)
      {
        return
          int.Parse(a.SubItems[1].Text, NumberStyles.AllowHexSpecifier)
             .CompareTo(int.Parse(b.SubItems[1].Text, NumberStyles.AllowHexSpecifier));
      });

      foreach (var item in lvis)
      {
        sb.AppendLine(item.Text);
      }

      sb.AppendLine();
      sb.AppendLine("Total active plugins: " + lvEspList.CheckedItems.Count);
      sb.AppendLine("Total plugins: " + lvEspList.Items.Count);
      Clipboard.SetText(sb.ToString());
    }

    private volatile string newFommMessage;

    private void newFommTimer_Tick(object sender, EventArgs e)
    {
      var tmp = newFommMessage;
      if (tmp != null)
      {
        newFommMessage = null;
        if (PackageManagerForm == null)
        {
          bPackageManager_Click(null, null);
        }
        PackageManagerForm.AddNewFomod(tmp);
      }
    }

    private void RecieveMessage(string msg)
    {
      newFommMessage = msg;
    }

    private void lvEspList_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Alt && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
      {
        e.Handled = true;
        if (lvEspList.SelectedItems.Count > 0)
        {
          var indicies = new int[lvEspList.SelectedIndices.Count];
          for (var i = 0; i < indicies.Length; i++)
          {
            indicies[i] = lvEspList.SelectedIndices[i];
          }

          Array.Sort(indicies);
          if (e.KeyCode == Keys.Up)
          {
            if (indicies[0] > 0)
            {
              CommitLoadOrder(indicies[0] - 1, indicies);
            }
          }
          else
          {
            if (indicies[indicies.Length - 1] < lvEspList.Items.Count - 1)
            {
              CommitLoadOrder(indicies[indicies.Length - 1] + 2, indicies);
            }
          }
        }
      }
      else if (e.KeyCode == Keys.Delete)
      {
        deleteToolStripMenuItem_Click(null, null);
        e.Handled = true;
      }
    }

    private void exportLoadOrder(string path)
    {
      var sw = new StreamWriter(path);
      for (var i = 0; i < lvEspList.Items.Count; i++)
      {
        sw.WriteLine("[" + (lvEspList.Items[i].Checked ? "X" : " ") + "] " + lvEspList.Items[i].Text);
      }
      sw.Close();
    }

    private void exportLoadOrderToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var ofd = new SaveFileDialog();
      ofd.Filter = "Text file (*.txt)|*.txt";
      ofd.AddExtension = true;
      ofd.RestoreDirectory = true;
      if (ofd.ShowDialog() == DialogResult.OK)
      {
        exportLoadOrder(ofd.FileName);
      }
    }

    private void importLoadOrderToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var ofd = new OpenFileDialog();
      ofd.Filter = "Text file (*.txt)|*.txt";
      ofd.AddExtension = true;
      ofd.RestoreDirectory = true;
      if (ofd.ShowDialog() == DialogResult.OK)
      {
        var lines = File.ReadAllLines(ofd.FileName);
        var active = new List<string>();
        for (var i = 0; i < lines.Length; i++)
        {
          if (lines[i].Length < 5 || lines[i][0] != '[' || lines[i][2] != ']' || lines[i][3] != ' ')
          {
            MessageBox.Show("File does not appear to be an exported load order list", "Error");
            return;
          }
          var bactive = lines[i][1] == 'X';
          lines[i] = lines[i].Substring(4).ToLowerInvariant();
          if (bactive)
          {
            active.Add(lines[i]);
          }
        }

        var order = new string[lvEspList.Items.Count];
        var upto = 0;
        foreach (var line in lines)
        {
          if (File.Exists(Path.Combine(Program.GameMode.PluginsPath, line)))
          {
            order[upto++] = line;
          }
        }

        for (var i = 0; i < lvEspList.Items.Count; i++)
        {
          if (Array.IndexOf(order, lvEspList.Items[i].Text.ToLowerInvariant()) == -1)
          {
            order[upto++] = lvEspList.Items[i].Text;
          }
        }

        for (var i = 0; i < order.Length; i++)
        {
          Program.GameMode.PluginManager.SetLoadOrder(Path.Combine(Program.GameMode.PluginsPath, order[i]), i);
        }

        RefreshPluginList();

        RefreshingList = true;
        for (var i = 0; i < lvEspList.Items.Count; i++)
        {
          lvEspList.Items[i].Checked = active.Contains(lvEspList.Items[i].Text.ToLowerInvariant());
          if (lvEspList.Items[i].Checked)
          {
            Program.GameMode.PluginManager.ActivatePlugin(lvEspList.Items[i].Text);
          }
          else
          {
            Program.GameMode.PluginManager.DeactivatePlugin(lvEspList.Items[i].Text);
          }
        }
        RefreshingList = false;
      }
    }

    private void uncheckAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      RefreshingList = true;
      for (var i = 0; i < lvEspList.Items.Count; i++)
      {
        lvEspList.Items[i].Checked = false;
        Program.GameMode.PluginManager.DeactivatePlugin(lvEspList.Items[i].Text);
      }
      RefreshIndexCounts();
      RefreshingList = false;
    }

    private void checkAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      RefreshingList = true;
      for (var i = 0; i < lvEspList.Items.Count; i++)
      {
        lvEspList.Items[i].Checked = true;
        Program.GameMode.PluginManager.ActivatePlugin(lvEspList.Items[i].Text);
      }
      RefreshIndexCounts();
      RefreshingList = false;
    }

    private void pictureBox1_Click(object sender, EventArgs e)
    {
      if (pictureBox1.Image != null &&
          (pictureBox1.Image.Size.Width > pictureBox1.Width || pictureBox1.Image.Size.Height > pictureBox1.Height))
      {
        (new ImageForm(pictureBox1.Image)).ShowDialog();
      }
    }

    private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (lvEspList.SelectedIndices.Count != 0)
      {
        var files = new ListViewItem[lvEspList.SelectedItems.Count];
        for (var i = 0; i < lvEspList.SelectedItems.Count; i++)
        {
          files[i] = lvEspList.SelectedItems[i];
          if (Program.GameMode.PluginManager.IsCriticalPlugin(Path.Combine(Program.GameMode.PluginsPath, files[i].Text)))
          {
            MessageBox.Show(this, "Cannot delete " + files[i].Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
          }
        }

        if (
          MessageBox.Show(this, "Are you sure you want to delete the selected plugins?", "Warning",
                          MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
          lvEspList.SelectedItems.Clear();
          foreach (var file in files)
          {
            File.Delete(Path.Combine(Program.GameMode.PluginsPath, file.Text));
            lvEspList.Items.Remove(file);
          }
          RefreshIndexCounts();
        }
      }
    }

    private void bSettings_Click(object sender, EventArgs e)
    {
      if (Application.OpenForms.Count == 1)
      {
        var sfmSettings = new SettingsForm();
        if (sfmSettings.ShowDialog(this) == DialogResult.OK)
        {
          RefreshPluginList();
        }
      }
      else
      {
        MessageBox.Show("Please close all utility windows before changing the settings");
      }
    }

    private void lvEspList_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      AlphaSortMode = e.Column == 0;
      lvEspList.AllowDrop = AlphaSortMode ? false : true;
      RefreshPluginList();
    }

    private void closeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Close();
    }

    /// <summary>
    ///   Handles the <see cref="RichTextBox.LinkClicked" /> event of the plugin info text box.
    /// </summary>
    /// <remarks>
    ///   Launches clicked links using the default browser.
    /// </remarks>
    /// <param name="sender">The object that trigger the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void rtbPluginInfo_LinkClicked(object sender, LinkClickedEventArgs e)
    {
      Process.Start(e.LinkText);
    }

    /// <summary>
    ///   Handles the <see cref="Control.Click" /> event of the change game mode menu item.
    /// </summary>
    /// <remarks>
    ///   Re-launched the mod manager and allows the selection of a new game mode.
    /// </remarks>
    /// <param name="sender">The object that trigger the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void changeGameToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ChangeGameMode = true;
      Close();
    }
  }
}