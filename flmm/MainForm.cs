using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Remoting;
using Fomm.PackageManager;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using Fomm.PackageManager.ModInstallLog;
using Fomm.Games;
#if TRACE
using System.Diagnostics;
#endif

namespace Fomm
{
	public partial class MainForm : Form
	{
#if TRACE
		private bool m_booListedPlugins = false;
#endif
		private bool AlphaSortMode = false;
		private List<string> m_lstIgnoreReadOnly = new List<string>();
		private Dictionary<string, Dictionary<string, List<string>>> m_dicExtraInfo = new Dictionary<string, Dictionary<string, List<string>>>();

		#region Properties

		/// <summary>
		/// Gets whether there are any open utility windows.
		/// </summary>
		/// <value>Whether there are any open utility windows.</value>
		public bool HasOpenUtilityWindows
		{
			get
			{
				Int32 intIngoredWindowCount = 0;
				for (Int32 i = Application.OpenForms.Count - 1; i >= 0; i--)
				{
					Form frmForm = Application.OpenForms[i];
					if (frmForm.GetType().Namespace.StartsWith("ICSharp", StringComparison.InvariantCultureIgnoreCase))
						intIngoredWindowCount++;
				}
				return (Application.OpenForms.Count - intIngoredWindowCount > 1);
			}
		}

		/// <summary>
		/// Gets the list view items representing the currently installed plugins.
		/// </summary>
		/// <value>The list view items representing the currently installed plugins.</value>
		public ListView.ListViewItemCollection PluginsListViewItems
		{
			get
			{
				return lvEspList.Items;
			}
		}

		/// <summary>
		/// Gets a list of currently selected plugins.
		/// </summary>
		/// <value>A list of currently selected plugins.</value>
		public IList<string> SelectedPlugins
		{
			get
			{
				List<string> lstSelectedPlugins = new List<string>();
				foreach (ListViewItem lviPlugin in lvEspList.SelectedItems)
					lstSelectedPlugins.Add(lviPlugin.Text);
				return lstSelectedPlugins;
			}
		}

		/// <summary>
		/// Gets whether the package manager is open.
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
			this.Icon = Fomm.Properties.Resources.fomm02;
			Properties.Settings.Default.windowPositions.GetWindowPosition("MainForm", this);

			Text += " (" + Program.Version + ")";
#if TRACE
			Text += " TRACE";
#endif

			if (fomod != null)
			{
				bPackageManager_Click(null, null);
				if (fomod.Length > 0) PackageManagerForm.AddNewFomod(fomod);
			}

			GameTool gtlGameLaunch = Program.GameMode.LaunchCommand;
			bLaunch.Text = gtlGameLaunch.Name;
			bLaunch.Tag = gtlGameLaunch.Command;

			if (!Properties.Settings.Default.DisableIPC)
			{
				Timer newFommTimer = new Timer();
				try
				{
					newFommTimer.Interval = 1000;
					newFommTimer.Tick += new EventHandler(newFommTimer_Tick);
					newFommTimer.Start();
					Messaging.ServerSetup(RecieveMessage);
				}
				catch (RemotingException)
				{
					newFommTimer.Stop();
					newFommTimer.Enabled = false;
					Properties.Settings.Default.DisableIPC = true;
					Properties.Settings.Default.Save();
				}
			}

			SetupTools();
		}

		#endregion

		#region Extra Plugin Info

		/// <summary>
		/// Clears all current extra plugin info for the specified info provider.
		/// </summary>
		/// <param name="p_strInfoKey">The key of the info provider whose extra info should be cleared.</param>
		public void ClearExtraInfo(string p_strInfoKey)
		{
			if (m_dicExtraInfo.ContainsKey(p_strInfoKey))
				m_dicExtraInfo.Remove(p_strInfoKey);
		}

		/// <summary>
		/// Adds extra info to be displayed in the description box for the specified plugin, from the specified provider.
		/// </summary>
		/// <param name="p_strInfoKey">The key of the info provider providing the extra info.</param>
		/// <param name="p_strPluginName">The plugin for which the info is being provided.</param>
		/// <param name="p_strMessage">The extra info about the plugin.</param>
		public void AddExtraInfo(string p_strInfoKey, string p_strPluginName, string p_strMessage)
		{
			if (!m_dicExtraInfo.ContainsKey(p_strInfoKey))
				m_dicExtraInfo[p_strInfoKey] = new Dictionary<string, List<string>>();
			if (!m_dicExtraInfo[p_strInfoKey].ContainsKey(p_strPluginName))
				m_dicExtraInfo[p_strInfoKey][p_strPluginName] = new List<string>();
			m_dicExtraInfo[p_strInfoKey][p_strPluginName].Add(p_strMessage);
		}

		#endregion

		/// <summary>
		/// Adds the game-specific tools to the Tool menu.
		/// </summary>
		protected void SetupTools()
		{
			foreach (GameTool gtlTool in Program.GameMode.Tools)
			{
				ToolStripItem tsiMenuItem = toolsToolStripMenuItem.DropDownItems.Add(gtlTool.Name, null, (s, a) => { ((GameTool.LaunchToolMethod)((ToolStripItem)s).Tag)(this); });
				tsiMenuItem.Tag = gtlTool.Command;
			}

			foreach (GameTool gtlTool in Program.GameMode.GameSettingsTools)
			{
				ToolStripItem tsiMenuItem = gameSettingsToolStripMenuItem.DropDownItems.Add(gtlTool.Name, null, (s, a) => { ((GameTool.LaunchToolMethod)((ToolStripItem)s).Tag)(this); });
				tsiMenuItem.Tag = gtlTool.Command;
			}

			foreach (GameTool gtlTool in Program.GameMode.RightClickTools)
			{
				ToolStripItem tsiMenuItem = cmsPlugins.Items.Add(gtlTool.Name, null, (s, a) => { ((GameTool.LaunchToolMethod)((ToolStripItem)s).Tag)(this); });
				tsiMenuItem.Tag = gtlTool.Command;
			}

			foreach (GameTool gtlTool in Program.GameMode.LoadOrderTools)
			{
				ToolStripItem tsiMenuItem = loadOrderToolStripMenuItem.DropDownItems.Add(gtlTool.Name, null, (s, a) => { ((GameTool.LaunchToolMethod)((ToolStripItem)s).Tag)(this); });
				tsiMenuItem.Tag = gtlTool.Command;
			}

			foreach (GameTool gtlTool in Program.GameMode.GameLaunchCommands)
			{
				ToolStripItem tsiMenuItem = launchGameToolStripMenuItem.DropDownItems.Add(gtlTool.Name, null, (s, a) => { ((GameTool.LaunchToolMethod)((ToolStripItem)s).Tag)(this); });
				tsiMenuItem.Tag = gtlTool.Command;
			}
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			Int32 tmp = Properties.Settings.Default.MainFormPanelSplit;
			if (tmp > 0)
				splitContainer1.SplitterDistance = Math.Max(splitContainer1.Panel1MinSize + 1, Math.Min(splitContainer1.Height - (splitContainer1.Panel2MinSize + 1), tmp));

			Int32[] intColumnWidths = Properties.Settings.Default.MainFormColumnWidths;
			if (intColumnWidths != null)
				for (Int32 i = 0; i < intColumnWidths.Length; i++)
					lvEspList.Columns[i].Width = intColumnWidths[i];
			RefreshPluginList();
			exportLoadOrder(Path.Combine(Program.fommDir, "load order backup.txt"));

			if (!File.Exists(Program.GameMode.SettingsFiles[Fomm.Games.Fallout3.Fallout3GameMode.SettingsFile.FOIniPath]))
			{
				MessageBox.Show("You have no Fallout INI file. Please run Fallout 3 to initialize the file before installing any mods or turning on Archive Invalidation.", "Missing INI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
		}

		private void lvEspList_DragDrop(object sender, DragEventArgs e)
		{
			if (AlphaSortMode)
			{
				MessageBox.Show("Cannot change load order when sorting by file name", "Error");
				return;
			}
			for (int i = 0; i < lvEspList.Items.Count; i++)
				Program.GameMode.PluginManager.SetLoadOrder(Path.Combine(Program.GameMode.PluginsPath, lvEspList.Items[i].Text), i);

			RefreshIndexCounts();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			for (Int32 i = Application.OpenForms.Count - 1; i >= 0; i--)
			{
				Form frmForm = Application.OpenForms[i];
				if (frmForm.GetType().Namespace.StartsWith("ICSharp", StringComparison.InvariantCultureIgnoreCase))
				{
					frmForm.Close();
					frmForm.Dispose();
				}
			}
			if (Application.OpenForms.Count > 1)
			{
				MessageBox.Show("Please close all utility windows before closing fomm");
				e.Cancel = true;
				return;
			}

			Properties.Settings.Default.windowPositions.SetWindowPosition("MainForm", this);
			Properties.Settings.Default.MainFormPanelSplit = splitContainer1.SplitterDistance;

			Int32[] intColumnWidths = new Int32[lvEspList.Columns.Count];
			foreach (ColumnHeader chdHeader in lvEspList.Columns)
				intColumnWidths[chdHeader.Index] = chdHeader.Width;
			Properties.Settings.Default.MainFormColumnWidths = intColumnWidths;

			Properties.Settings.Default.Save();
		}

		public void LoadPluginInfo()
		{
			if (lvEspList.SelectedItems.Count != 1)
				return;

			PluginInfo pifInfo = Program.GameMode.PluginManager.GetPluginInfo(Path.Combine(Program.GameMode.PluginsPath, lvEspList.SelectedItems[0].Text));
			StringBuilder stbDescription = new StringBuilder(pifInfo.Description);
			foreach (string strInfoOwner in m_dicExtraInfo.Keys)
			{
				if (m_dicExtraInfo[strInfoOwner].ContainsKey(lvEspList.SelectedItems[0].Text))
				{
					foreach (string strExtraInfo in m_dicExtraInfo[strInfoOwner][lvEspList.SelectedItems[0].Text])
					{
						stbDescription.Append(@"\par ");
						stbDescription.Append(strExtraInfo);
					}
				}
			}
			stbDescription.AppendLine().Append("}");
			rtbPluginInfo.Rtf = stbDescription.ToString();
			pictureBox1.Image = pifInfo.Picture;
		}

		private void lvEspList_SelectedIndexChanged(object sender, EventArgs e)
		{
			LoadPluginInfo();
		}

		private PackageManager.PackageManager PackageManagerForm;
		private void bPackageManager_Click(object sender, EventArgs e)
		{
			if (PackageManagerForm != null) PackageManagerForm.Focus();
			else
			{
				PackageManagerForm = new Fomm.PackageManager.PackageManager(this);
				PackageManagerForm.FormClosed += delegate(object sender2, FormClosedEventArgs e2)
				{
					RefreshPluginList();
					PackageManagerForm = null;
				};
				PackageManagerForm.Show();
			}
		}

		private FileManager.FileManager m_fmgFileManagerForm = null;
		/// <summary>
		/// Handles the <see cref="Button.Click"/> event of the file manager button.
		/// </summary>
		/// <remarks>
		/// Displays the file manager.
		/// </remarks>
		/// <param name="sender">The object that trigger the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butFileManager_Click(object sender, EventArgs e)
		{
			if (m_fmgFileManagerForm != null)
				m_fmgFileManagerForm.Focus();
			else
			{
				m_fmgFileManagerForm = new Fomm.FileManager.FileManager();
				m_fmgFileManagerForm.FormClosed += delegate(object sender2, FormClosedEventArgs e2)
				{
					m_fmgFileManagerForm = null;
				};
				m_fmgFileManagerForm.Show();
			}
		}

		private void RefreshIndexCounts()
		{
			if (lvEspList.Items.Count == 0) return;
			bool add = lvEspList.Items[0].SubItems.Count == 1;
			bool boldify = false;

			if (add)
			{
				boldify = Settings.GetBool("ShowEsmInBold");
			}

			string[] strPlugins = Program.GameMode.PluginManager.SortPluginList(Program.GameMode.PluginManager.ActivePluginList);

			for (int i = 0; i < strPlugins.Length; i++)
				strPlugins[i] = Path.GetFileName(strPlugins[i].Trim()).ToLowerInvariant();
			foreach (ListViewItem lvi in lvEspList.Items)
			{
				int i = Array.IndexOf<string>(strPlugins, lvi.Text.ToLowerInvariant());
				if (i != -1)
				{
					if (add)
					{
						lvi.Checked = true;
						lvi.SubItems.Add(i.ToString("X2"));
					}
					else
						lvi.SubItems[1].Text = i.ToString("X2");
				}
				else
				{
					if (add)
						lvi.SubItems.Add("NA");
					else
						lvi.SubItems[1].Text = "NA";
				}
			}
		}

		public void RefreshPluginList()
		{
#if TRACE
			if (!m_booListedPlugins)
			{
				Trace.WriteLine("");
				Trace.WriteLine("Refreshing Plugin List: ");
				Trace.Indent();
			}
#endif
			RefreshingList = true;
			lvEspList.BeginUpdate();
			lvEspList.Items.Clear();

			List<string> lstPluginFilenames = new List<string>(Program.GameMode.PluginManager.OrderedPluginList);
			if (AlphaSortMode)
			{
				lstPluginFilenames.Sort(delegate(string a, string b)
				{
					return Path.GetFileName(a).CompareTo(Path.GetFileName(b));
				});
			}

			List<ListViewItem> lstPluginViewItems = new List<ListViewItem>();
			foreach (string strPlugin in lstPluginFilenames)
			{
#if TRACE
				if (!m_booListedPlugins)
					Trace.WriteLine(strPlugin);
#endif
				lstPluginViewItems.Add(new ListViewItem(Path.GetFileName(strPlugin)));
			}

			lvEspList.Items.AddRange(lstPluginViewItems.ToArray());
			RefreshIndexCounts();
			lvEspList.EndUpdate();
			RefreshingList = false;
#if TRACE
			if (!m_booListedPlugins)
			{
				m_booListedPlugins = true;
				Trace.Unindent();
				Trace.Flush();
			}
#endif
		}

		#region toolbuttons

		private void bLaunch_Click(object sender, EventArgs e)
		{
			if (Application.OpenForms.Count > 1)
			{
				MessageBox.Show("Please close all utility windows before launching fallout");
				return;
			}
			((GameTool.LaunchToolMethod)((Button)sender).Tag)(this);
			Close();
		}

		private void bSaveGames_Click(object sender, EventArgs e)
		{
			string[] active = new string[lvEspList.CheckedItems.Count];
			for (int i = 0; i < active.Length; i++) active[i] = lvEspList.CheckedItems[i].Text;
			//ok, so this includes active mods as well as inactive ones, but it still works the same
			string[] inactive = new string[lvEspList.Items.Count];
			for (int i = 0; i < inactive.Length; i++) inactive[i] = lvEspList.Items[i].Text;
			(new SaveForm(active, inactive)).Show();
		}

		private void bHelp_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start(Path.Combine(Program.fommDir, "fomm.chm"));
			//System.Diagnostics.Process.Start(@"http://fomm.wiki.sourceforge.net/");
		}
		#endregion

		private void CommitLoadOrder(int position, int[] indicies)
		{
			if (AlphaSortMode)
			{
				MessageBox.Show("Cannot change load order when sorting by file name", "Error");
				return;
			}
			Array.Sort<int>(indicies);
			List<ListViewItem> items = new List<ListViewItem>();
			RefreshingList = true;
			lvEspList.BeginUpdate();
			Int32 intLoadOrder = 0;
			for (int i = 0; i < position; i++)
			{
				if (Array.BinarySearch<int>(indicies, i) >= 0)
					continue;
				Program.GameMode.PluginManager.SetLoadOrder(Path.Combine(Program.GameMode.PluginsPath, lvEspList.Items[i].Text), intLoadOrder++);
				items.Add(lvEspList.Items[i]);
				items[items.Count - 1].Selected = false;
			}
			for (int i = 0; i < indicies.Length; i++)
			{
				Program.GameMode.PluginManager.SetLoadOrder(Path.Combine(Program.GameMode.PluginsPath, lvEspList.Items[indicies[i]].Text), intLoadOrder++);
				items.Add(lvEspList.Items[indicies[i]]);
				items[items.Count - 1].Selected = true;
			}
			for (int i = position; i < lvEspList.Items.Count; i++)
			{
				if (Array.BinarySearch<int>(indicies, i) >= 0)
					continue;
				Program.GameMode.PluginManager.SetLoadOrder(Path.Combine(Program.GameMode.PluginsPath, lvEspList.Items[i].Text), intLoadOrder++);
				items.Add(lvEspList.Items[i]);
				items[items.Count - 1].Selected = false;
			}
			lvEspList.Items.Clear();
			lvEspList.Items.AddRange(items.ToArray());
			RefreshIndexCounts();
			lvEspList.EndUpdate();
			RefreshingList = false;
			lvEspList.EnsureVisible(position == lvEspList.Items.Count ? position - 1 : position);
		}

		private bool RefreshingList;
		private void lvEspList_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			if (RefreshingList) return;
			if (e.Item.Checked)
				Program.GameMode.PluginManager.ActivatePlugin(e.Item.Text);
			else
				Program.GameMode.PluginManager.DeactivatePlugin(e.Item.Text);
			RefreshIndexCounts();
		}

		private void sendToTopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (lvEspList.SelectedIndices.Count == 0) return;
			int[] toswap = new int[lvEspList.SelectedIndices.Count];
			for (int i = 0; i < lvEspList.SelectedIndices.Count; i++) toswap[i] = lvEspList.SelectedIndices[i];
			Array.Sort<int>(toswap);
			CommitLoadOrder(0, toswap);
		}

		private void sendToBottomToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (lvEspList.SelectedIndices.Count == 0) return;
			int[] toswap = new int[lvEspList.SelectedIndices.Count];
			for (int i = 0; i < lvEspList.SelectedIndices.Count; i++) toswap[i] = lvEspList.SelectedIndices[i];
			Array.Sort<int>(toswap);
			CommitLoadOrder(lvEspList.Items.Count, toswap);
		}

		private void copyLoadOrderToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			ListViewItem[] lvis = new ListViewItem[lvEspList.CheckedItems.Count];
			for (int i = 0; i < lvEspList.CheckedItems.Count; i++) lvis[i] = lvEspList.CheckedItems[i];
			Array.Sort<ListViewItem>(lvis, delegate(ListViewItem a, ListViewItem b)
			{
				return int.Parse(a.SubItems[1].Text, System.Globalization.NumberStyles.AllowHexSpecifier).CompareTo(int.Parse(b.SubItems[1].Text, System.Globalization.NumberStyles.AllowHexSpecifier));
			});
			for (int i = 0; i < lvis.Length; i++) sb.AppendLine(lvis[i].Text);
			sb.AppendLine();
			sb.AppendLine("Total active plugins: " + lvEspList.CheckedItems.Count);
			sb.AppendLine("Total plugins: " + lvEspList.Items.Count);
			Clipboard.SetText(sb.ToString());
		}

		private volatile string newFommMessage;

		private void newFommTimer_Tick(object sender, EventArgs e)
		{
			string tmp = newFommMessage;
			if (tmp == null) return;
			newFommMessage = null;
			if (PackageManagerForm == null) bPackageManager_Click(null, null);
			PackageManagerForm.AddNewFomod(tmp);
		}

		private void RecieveMessage(string msg) { newFommMessage = msg; }

		private void lvEspList_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Alt && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
			{
				e.Handled = true;
				if (lvEspList.SelectedItems.Count > 0)
				{
					int[] indicies = new int[lvEspList.SelectedIndices.Count];
					for (int i = 0; i < indicies.Length; i++) indicies[i] = lvEspList.SelectedIndices[i];
					Array.Sort<int>(indicies);
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
			StreamWriter sw = new StreamWriter(path);
			for (int i = 0; i < lvEspList.Items.Count; i++)
			{
				sw.WriteLine("[" + (lvEspList.Items[i].Checked ? "X" : " ") + "] " + lvEspList.Items[i].Text);
			}
			sw.Close();
		}

		private void exportLoadOrderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog ofd = new SaveFileDialog();
			ofd.Filter = "Text file (*.txt)|*.txt";
			ofd.AddExtension = true;
			ofd.RestoreDirectory = true;
			if (ofd.ShowDialog() != DialogResult.OK) return;
			exportLoadOrder(ofd.FileName);
		}

		private void importLoadOrderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Text file (*.txt)|*.txt";
			ofd.AddExtension = true;
			ofd.RestoreDirectory = true;
			if (ofd.ShowDialog() != DialogResult.OK) return;
			string[] lines = File.ReadAllLines(ofd.FileName);
			List<string> active = new List<string>();
			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].Length < 5 || lines[i][0] != '[' || lines[i][2] != ']' || lines[i][3] != ' ')
				{
					MessageBox.Show("File does not appear to be an exported load order list", "Error");
					return;
				}
				bool bactive = lines[i][1] == 'X';
				lines[i] = lines[i].Substring(4).ToLowerInvariant();
				if (bactive) active.Add(lines[i]);
			}

			string[] order = new string[lvEspList.Items.Count];
			int upto = 0;
			for (int i = 0; i < lines.Length; i++)
			{
				if (File.Exists(Path.Combine(Program.GameMode.PluginsPath, lines[i]))) order[upto++] = lines[i];
			}
			for (int i = 0; i < lvEspList.Items.Count; i++)
			{
				if (Array.IndexOf<string>(order, lvEspList.Items[i].Text.ToLowerInvariant()) == -1) order[upto++] = lvEspList.Items[i].Text;
			}
			for (int i = 0; i < order.Length; i++)
				Program.GameMode.PluginManager.SetLoadOrder(Path.Combine(Program.GameMode.PluginsPath, order[i]), i);

			RefreshPluginList();

			RefreshingList = true;
			for (int i = 0; i < lvEspList.Items.Count; i++)
				lvEspList.Items[i].Checked = active.Contains(lvEspList.Items[i].Text.ToLowerInvariant());
			RefreshingList = false;
			lvEspList_ItemChecked(null, null);
		}

		private void uncheckAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RefreshingList = true;
			for (int i = 0; i < lvEspList.Items.Count; i++) lvEspList.Items[i].Checked = false;
			RefreshingList = false;
			lvEspList_ItemChecked(null, null);
		}

		private void checkAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RefreshingList = true;
			for (int i = 0; i < lvEspList.Items.Count; i++) lvEspList.Items[i].Checked = true;
			RefreshingList = false;
			lvEspList_ItemChecked(null, null);
		}

		private void pictureBox1_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null && (pictureBox1.Image.Size.Width > pictureBox1.Width || pictureBox1.Image.Size.Height > pictureBox1.Height))
			{
				(new ImageForm(pictureBox1.Image)).ShowDialog();
			}
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (lvEspList.SelectedIndices.Count == 0) return;
			ListViewItem[] files = new ListViewItem[lvEspList.SelectedItems.Count];
			for (int i = 0; i < lvEspList.SelectedItems.Count; i++)
			{
				files[i] = lvEspList.SelectedItems[i];
				if (Program.GameMode.PluginManager.IsCriticalPlugin(Path.Combine(Program.GameMode.PluginsPath, files[i].Text)))
				{
					MessageBox.Show(this, "Cannot delete " + files[i].Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}
			if (MessageBox.Show(this, "Are you sure you want to delete the selected plugins?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
				return;
			lvEspList.SelectedItems.Clear();
			for (int i = 0; i < files.Length; i++)
			{
				File.Delete(Path.Combine(Program.GameMode.PluginsPath, files[i].Text));
				lvEspList.Items.Remove(files[i]);
			}
			RefreshIndexCounts();
		}

		private void bSettings_Click(object sender, EventArgs e)
		{
			if (Application.OpenForms.Count > 1)
			{
				MessageBox.Show("Please close all utility windows before changing the settings");
				return;
			}
			(new SettingsForm(true)).ShowDialog();
			RefreshPluginList();
		}

		private void lvEspList_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			AlphaSortMode = e.Column == 0;
			if (AlphaSortMode) lvEspList.AllowDrop = false;
			else lvEspList.AllowDrop = true;
			RefreshPluginList();
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void visitForumsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://sourceforge.net/projects/fomm/forums");
		}

		private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
		{
			bool booWasUpdate = false;
			DateTime dteLastUpdateCheck = Properties.Settings.Default.LastUpdateCheck;
			if (dteLastUpdateCheck + TimeSpan.FromHours(2) > DateTime.Now)
			{
				MessageBox.Show("No newer updates available");
				return;
			}

			//check for new FOMM
			Regex rgxVersion = new Regex(@"Download Now!</strong>\s+fomm([\d\.]+)\.exe", System.Text.RegularExpressions.RegexOptions.Singleline);
			string strVersionPage = null;
			using (System.Net.WebClient wclGetter = new System.Net.WebClient())
			{
				strVersionPage = wclGetter.DownloadString("http://sf.net/projects/fomm");
			}
			string strWebVersion = rgxVersion.Match(strVersionPage).Groups[1].Value.Trim();
			if (new Version(strWebVersion + ".0") > Program.MVersion)
			{
				if (MessageBox.Show("A new version of fomm is available: " + strWebVersion +
					"\nDo you wish to download?", "Message", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					System.Diagnostics.Process.Start("http://sf.net/projects/fomm");
				}
				booWasUpdate = true;
			}

			booWasUpdate = booWasUpdate || Program.GameMode.CheckForUpdates();

			if (!booWasUpdate)
			{
				MessageBox.Show("No newer updates available");
				Properties.Settings.Default.LastUpdateCheck = DateTime.Now;
				Properties.Settings.Default.Save();
			}
		}

		/// <summary>
		/// Handles the <see cref="RichTextBox.LinkClicked"/> event of the plugin info text box.
		/// </summary>
		/// <remarks>
		/// Launches clicked links using the default browser.
		/// </remarks>
		/// <param name="sender">The object that trigger the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void rtbPluginInfo_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(e.LinkText);
		}
	}
}