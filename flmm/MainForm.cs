using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using Fomm.Games.Fallout3.Tools.TESsnip;
using System.Runtime.Remoting;
using Fomm.PackageManager;
using System.Drawing;
using System.Text;
using Fomm.Games.Fallout3.Tools.CriticalRecords;
using Fomm.AutoSorter;
using System.Text.RegularExpressions;
using Fomm.PackageManager.ModInstallLog;
#if TRACE
using System.Diagnostics;
using Fomm.Games;
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
		private BackgroundWorkerProgressDialog m_bwdProgress = null;

		#region Properties

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
			Settings.GetWindowPosition("MainForm", this);

			Text += " (" + Program.Version + ")";
#if TRACE
			Text += " TRACE";
#endif

			if (fomod != null)
			{
				bPackageManager_Click(null, null);
				if (fomod.Length > 0) PackageManagerForm.AddNewFomod(fomod);
			}

			if (Settings.GetString("LaunchCommand") == null && File.Exists("fose_loader.exe")) bLaunch.Text = "Launch FOSE";

			if (!Settings.GetBool("DisableIPC"))
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
					Settings.SetBool("DisableIPC", true);
				}
			}

			SetupTools();
		}

		#endregion

		#region Extra Plugin Info

		public void ClearExtraInfo(string p_strInfoKey)
		{
			if (m_dicExtraInfo.ContainsKey(p_strInfoKey))
				m_dicExtraInfo.Remove(p_strInfoKey);
		}

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
				toolsToolStripMenuItem.DropDownItems.Add(gtlTool.Name, null, (s, a) => { ExecuteGameToolCommand(gtlTool); });

			foreach (GameTool gtlTool in Program.GameMode.GameSettingsTools)
				gameSettingsToolStripMenuItem.DropDownItems.Add(gtlTool.Name, null, (s, a) => { ExecuteGameToolCommand(gtlTool); });

			foreach (GameTool gtlTool in Program.GameMode.RightClickTools)
				cmsPlugins.Items.Add(gtlTool.Name, null, (s, a) => { ExecuteGameToolCommand(gtlTool); });
		}

		private void ExecuteGameToolCommand(GameTool p_gtlTool)
		{
			if (p_gtlTool.Command != null)
				p_gtlTool.Command(this);
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			string tmp = Settings.GetString("MainFormPanelSplit");
			if (tmp != null)
			{
				try
				{
					splitContainer1.SplitterDistance = Math.Max(splitContainer1.Panel1MinSize + 1, Math.Min(splitContainer1.Height - (splitContainer1.Panel2MinSize + 1), int.Parse(tmp)));
				}
				catch { }
			}
			tmp = Settings.GetString("MainFormCol1Width");
			if (tmp != null) lvEspList.Columns[0].Width = int.Parse(tmp);
			tmp = Settings.GetString("MainFormCol2Width");
			if (tmp != null) lvEspList.Columns[1].Width = int.Parse(tmp);
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
			DateTime timestamp = new DateTime(2008, 1, 1);
			TimeSpan twomins = TimeSpan.FromMinutes(2);

			for (int i = 0; i < lvEspList.Items.Count; i++)
			{
				File.SetLastWriteTime(Path.Combine("data", lvEspList.Items[i].Text), timestamp);
				timestamp += twomins;
			}

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

			Settings.SetWindowPosition("MainForm", this);
			Settings.SetString("MainFormPanelSplit", splitContainer1.SplitterDistance.ToString());
			Settings.SetString("MainFormCol1Width", lvEspList.Columns[0].Width.ToString());
			Settings.SetString("MainFormCol2Width", lvEspList.Columns[1].Width.ToString());
		}

		public void LoadPluginInfo()
		{
			if (lvEspList.SelectedItems.Count != 1) return;
			Plugin plgPlugin;
			try
			{
				plgPlugin = new Plugin(Path.Combine("data", lvEspList.SelectedItems[0].Text), true);
			}
			catch
			{
				plgPlugin = null;
			}
			if (plgPlugin == null || plgPlugin.Records.Count == 0 || plgPlugin.Records[0].Name != "TES4")
			{
				rtbPluginInfo.Text = lvEspList.SelectedItems[0].Text + Environment.NewLine + "Warning: Plugin appears corrupt";
				pictureBox1.Image = null;
				return;
			}

			StringBuilder stbDescription = new StringBuilder(@"{\rtf1\ansi\ansicpg1252\deff0\deflang4105{\fonttbl{\f0\fnil\fcharset0 MS Sans Serif;}{\f1\fnil\fcharset2 Symbol;}}");
			stbDescription.AppendLine().AppendLine(@"{\colortbl ;\red255\green0\blue0;\red255\green215\blue0;\red51\green153\blue255;}");
			stbDescription.AppendLine().Append(@"{\*\generator Msftedit 5.41.21.2509;}\viewkind4\uc1\pard\sl240\slmult1\lang9\f0\fs17 ");
			string name = null;
			string desc = null;
			byte[] pic = null;
			List<string> masters = new List<string>();
			foreach (SubRecord sr in ((Record)plgPlugin.Records[0]).SubRecords)
			{
				switch (sr.Name)
				{
					case "CNAM":
						name = sr.GetStrData();
						break;
					case "SNAM":
						desc = sr.GetStrData();
						break;
					case "MAST":
						masters.Add(sr.GetStrData());
						break;
					case "SCRN":
						pic = sr.GetData();
						break;
				}
			}
			if (pic != null)
			{
				pictureBox1.Image = System.Drawing.Bitmap.FromStream(new MemoryStream(pic));
			}
			else pictureBox1.Image = null;
			if ((Path.GetExtension(lvEspList.SelectedItems[0].Text).CompareTo(".esp") == 0) != ((((Record)plgPlugin.Records[0]).Flags1 & 1) == 0))
			{
				if ((((Record)plgPlugin.Records[0]).Flags1 & 1) == 0)
					stbDescription.Append(@"\cf1 \b WARNING: This plugin has the file extension .esm, but its file header marks it as an esp! \b0 \cf0 \line \line ");
				else
					stbDescription.Append(@"\cf1 \b WARNING: This plugin has the file extension .esp, but its file header marks it as an esm! \b0 \cf0 \line \line ");
			}
			stbDescription.AppendFormat(@"\b \ul {0} \ulnone \b0 \line ", lvEspList.SelectedItems[0].Text);
			if (name != null)
				stbDescription.AppendFormat(@"\b Author: \b0 {0} \line ", name);
			if (desc != null)
			{
				desc = desc.Replace("\r\n", "\n").Replace("\n\r", "\n").Replace("\n", "\\line ");
				stbDescription.AppendFormat(@"\b Description: \b0 \line {0} \line ", desc);
			}
			if (masters.Count > 0)
			{
				stbDescription.Append(@"\b Masters: \b0 \par \pard{\*\pn\pnlvlblt\pnf1\pnindent0{\pntxtb\'B7}}\fi-360\li720\sl240\slmult1 ");
				for (int i = 0; i < masters.Count; i++)
				{
					stbDescription.AppendFormat("{{\\pntext\\f1\\'B7\\tab}}{0}\\par ", masters[i]);
					stbDescription.AppendLine();
				}
				stbDescription.Append(@"\pard\sl240\slmult1 ");
			}
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
				lstPluginViewItems.Add(new ListViewItem(strPlugin));
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
			string command = Settings.GetString("LaunchCommand");
			string args = Settings.GetString("LaunchCommandArgs");
			if (command == null)
			{
				if (File.Exists("fose_loader.exe")) command = "fose_loader.exe";
				else if (File.Exists("fallout3.exe")) command = "fallout3.exe";
				else command = "fallout3ng.exe";
				args = null;
			}
			try
			{
				System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
				psi.Arguments = args;
				psi.FileName = command;
				psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(command));
				if (System.Diagnostics.Process.Start(psi) == null)
				{
					MessageBox.Show("Failed to launch '" + command + "'");
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to launch '" + command + "'\n" + ex.Message);
				return;
			}
			Close();
		}

		private void runFalloutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (Application.OpenForms.Count > 1)
			{
				MessageBox.Show("Please close all utility windows before launching fallout");
				return;
			}
			string command;
			if (File.Exists("fallout3.exe")) command = "fallout3.exe";
			else command = "fallout3ng.exe";
			try
			{
				System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
				psi.FileName = command;
				psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(command));
				if (System.Diagnostics.Process.Start(psi) == null)
				{
					MessageBox.Show("Failed to launch '" + command + "'");
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to launch '" + command + "'\n" + ex.Message);
				return;
			}
			Close();
		}

		private void runFoseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!File.Exists("fose_loader.exe"))
			{
				MessageBox.Show("fose does not appear to be installed");
				return;
			}
			if (Application.OpenForms.Count > 1)
			{
				MessageBox.Show("Please close all utility windows before launching fallout");
				return;
			}
			try
			{
				System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
				psi.FileName = "fose_loader.exe";
				psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath("fose_loader.exe"));
				if (System.Diagnostics.Process.Start(psi) == null)
				{
					MessageBox.Show("Failed to launch 'fose_loader.exe'");
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to launch 'fose_loader.exe'\n" + ex.Message);
				return;
			}
			Close();
		}

		private void runCustomToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (Application.OpenForms.Count > 1)
			{
				MessageBox.Show("Please close all utility windows before launching fallout");
				return;
			}
			string command = Settings.GetString("LaunchCommand");
			string args = Settings.GetString("LaunchCommandArgs");
			if (command == null)
			{
				MessageBox.Show("No custom launch command has been set", "Error");
				return;
			}
			try
			{
				System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
				psi.Arguments = args;
				psi.FileName = command;
				psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(command));
				if (System.Diagnostics.Process.Start(psi) == null)
				{
					MessageBox.Show("Failed to launch '" + command + "'");
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to launch '" + command + "'\n" + ex.Message);
				return;
			}
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
			DateTime timestamp = new DateTime(2008, 1, 1);
			TimeSpan twomins = TimeSpan.FromMinutes(2);
			List<ListViewItem> items = new List<ListViewItem>();
			RefreshingList = true;
			lvEspList.BeginUpdate();
			for (int i = 0; i < position; i++)
			{
				if (Array.BinarySearch<int>(indicies, i) >= 0) continue;
				File.SetLastWriteTime(Path.Combine("data", lvEspList.Items[i].Text), timestamp);
				timestamp += twomins;
				items.Add(lvEspList.Items[i]);
				items[items.Count - 1].Selected = false;
			}
			for (int i = 0; i < indicies.Length; i++)
			{
				File.SetLastWriteTime(Path.Combine("data", lvEspList.Items[indicies[i]].Text), timestamp);
				timestamp += twomins;
				items.Add(lvEspList.Items[indicies[i]]);
				items[items.Count - 1].Selected = true;
			}
			for (int i = position; i < lvEspList.Items.Count; i++)
			{
				if (Array.BinarySearch<int>(indicies, i) >= 0) continue;
				File.SetLastWriteTime(Path.Combine("data", lvEspList.Items[i].Text), timestamp);
				timestamp += twomins;
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
				if (File.Exists(Path.Combine("data", lines[i]))) order[upto++] = lines[i];
			}
			for (int i = 0; i < lvEspList.Items.Count; i++)
			{
				if (Array.IndexOf<string>(order, lvEspList.Items[i].Text.ToLowerInvariant()) == -1) order[upto++] = lvEspList.Items[i].Text;
			}
			DateTime timestamp = new DateTime(2008, 1, 1);
			TimeSpan twomins = TimeSpan.FromMinutes(2);
			for (int i = 0; i < order.Length; i++)
			{
				File.SetLastWriteTime(Path.Combine("data", order[i]), timestamp);
				timestamp += twomins;
			}

			RefreshPluginList();

			RefreshingList = true;
			for (int i = 0; i < lvEspList.Items.Count; i++) lvEspList.Items[i].Checked = active.Contains(lvEspList.Items[i].Text.ToLowerInvariant());
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
				if (files[i].Text.Equals("fallout3.esm", StringComparison.OrdinalIgnoreCase))
				{
					MessageBox.Show("Cannot delete Fallout3.esm", "Error");
					return;
				}
			}
			if (MessageBox.Show("Are you sure you want to delete the selected plugins?", "Warning", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
			lvEspList.SelectedItems.Clear();
			for (int i = 0; i < files.Length; i++)
			{
				File.Delete(Path.Combine("data", files[i].Text));
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

		private void bSort_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("This is currently a beta feature, and the load order template may not be optimal.\n" +
				"Ensure you have a backup of your load order before running this tool.\n" +
				"War you sure you wish to continue?", "Warning", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
			string[] plugins = new string[lvEspList.Items.Count];
			for (int i = 0; i < plugins.Length; i++) plugins[i] = lvEspList.Items[i].Text;
			LoadOrderSorter.SortList(plugins);
			DateTime timestamp = new DateTime(2008, 1, 1);
			TimeSpan twomins = TimeSpan.FromMinutes(2);
			for (int i = 0; i < plugins.Length; i++)
			{
				File.SetLastWriteTime(Path.Combine("data", plugins[i]), timestamp);
				timestamp += twomins;
			}
			RefreshPluginList();
		}

		private void bReport_Click(object sender, EventArgs e)
		{
			string[] plugins = new string[lvEspList.Items.Count];
			bool[] active = new bool[lvEspList.Items.Count];
			bool[] corrupt = new bool[lvEspList.Items.Count];
			string[][] masters = new string[lvEspList.Items.Count][];
			Plugin p;
			List<string> mlist = new List<string>();
			for (int i = 0; i < plugins.Length; i++)
			{
				plugins[i] = lvEspList.Items[i].Text;
				active[i] = lvEspList.Items[i].Checked;
				try
				{
					p = new Plugin(Path.Combine("data", plugins[i]), true);
				}
				catch
				{
					p = null;
					corrupt[i] = true;
				}
				if (p != null)
				{
					foreach (SubRecord sr in ((Record)p.Records[0]).SubRecords)
					{
						if (sr.Name != "MAST") continue;
						mlist.Add(sr.GetStrData().ToLowerInvariant());
					}
					if (mlist.Count > 0)
					{
						masters[i] = mlist.ToArray();
						mlist.Clear();
					}
				}
			}
			string s = LoadOrderSorter.GenerateReport(plugins, active, corrupt, masters);
			PackageManager.TextEditor.ShowEditor(s, Fomm.PackageManager.TextEditorType.Text, false);
		}

		private void visitForumsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://sourceforge.net/projects/fomm/forums");
		}

		private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
		{
			bool booWasUpdate = false;
			string strLastUpdateCheck = Settings.GetString("LastUpdateCheck");
			if (strLastUpdateCheck != null)
			{
				DateTime dteLastUpdateCheck;
				if (DateTime.TryParse(strLastUpdateCheck, out dteLastUpdateCheck))
				{
					if (dteLastUpdateCheck + TimeSpan.FromHours(2) > DateTime.Now)
					{
						MessageBox.Show("No newer updates available");
						return;
					}
				}
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

			//check for new load order tepmlate
			Int32 intLOVersion = BOSSUpdater.GetMasterlistVersion();
			if (intLOVersion > LoadOrderSorter.GetFileVersion())
			{
				if (MessageBox.Show("A new version of the load order template is available: Release " + intLOVersion +
					"\nDo you wish to download?", "Message", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					BOSSUpdater.UpdateMasterlist(LoadOrderSorter.LoadOrderTemplatePath);
					MessageBox.Show(this, "The load order template was updated.", "Update Complete.", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				booWasUpdate = true;
			}
			if (!booWasUpdate)
			{
				MessageBox.Show("No newer updates available");
				Settings.SetString("LastUpdateCheck", DateTime.Now.ToString());
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