using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using Fomm.PackageManager.Upgrade;
using SevenZip;
using WebsiteAPIs;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Diagnostics;
using Fomm.PackageManager.FomodBuilder;
using Fomm.PackageManager.Controls;
using Fomm.Util;

namespace Fomm.PackageManager
{
	internal partial class PackageManager : Form
	{
		private readonly List<fomod> mods = new List<fomod>();
		private readonly List<string> groups;
		private readonly List<string> lgroups;
		private readonly MainForm mf;
		private BackgroundWorkerProgressDialog m_bwdProgress = null;
		private string m_strLastFromFolderPath = null;
		private NexusAPI m_nxaNexus = null;
		private Regex m_rgxNexusFileId = new Regex(@"fallout3nexus\.com/downloads/file\.php\?id=(\d+)");
		private Dictionary<string, string> m_dicWebVersions = new Dictionary<string, string>();

		public PackageManager(MainForm mf)
		{
			this.mf = mf;
			InitializeComponent();

			CheckFOModCache();

			this.Icon = Fomm.Properties.Resources.fomm02;
			cmbSortOrder.ContextMenu = new ContextMenu();
			lvModList.ListViewItemSorter = new FomodSorter();
			Settings.GetWindowPosition("PackageManager", this);
			sbtAddFomod.SelectedItemIndex = Settings.GetInt("SelectedAddFomodAction", 0);
			m_strLastFromFolderPath = Settings.GetString("LastBuildFOMODFromFolderPath");

			foreach (string modpath in Program.GetFiles(Program.GameMode.ModDirectory, "*.fomod.zip"))
			{
				if (!File.Exists(Path.ChangeExtension(modpath, null))) File.Move(modpath, Path.ChangeExtension(modpath, null));
			}

			string[] groups = Settings.GetStringArray("fomodGroups");
			if (groups == null)
			{
				groups = new string[] {
                    "Items",
                    "Items/Guns",
                    "Items/Armor",
                    "Items/Misc",
                    "Locations",
                    "Locations/Houses",
                    "Locations/Interiors",
                    "Locations/Exteriors",
                    "Gameplay",
                    "Gameplay/Perks",
                    "Gameplay/Realism",
                    "Gameplay/Combat",
                    "Gameplay/Loot",
                    "Gameplay/Enemies",
                    "Quests",
                    "Companions",
                    "ModResource",
                    "UI",
                    "Music",
                    "Replacers",
                    "Replacers/Meshes",
                    "Replacers/Textures",
                    "Replacers/Sounds",
                    "Replacers/Shaders",
                    "Tweaks",
                    "Fixes",
                    "Cosmetic",
                    "Cosmetic/Races",
                    "Cosmetic/Eyes",
                    "Cosmetic/Hair"
                };
				Settings.SetStringArray("fomodGroups", groups);
			}
			this.groups = new List<string>(groups);
			this.lgroups = new List<string>(groups.Length);
			for (int i = 0; i < groups.Length; i++) lgroups.Add(groups[i].ToLowerInvariant());

			if (Settings.GetBool("PackageManagerShowsGroups"))
			{
				cbGroups.Checked = true;
			}

			WebsiteLogin();

			foreach (string modpath in Program.GetFiles(Program.GameMode.ModDirectory, "*.fomod"))
			{
				AddFomod(modpath, false);
			}

			RebuildListView();
		}

		/// <summary>
		/// This removes any old cache files.
		/// </summary>
		protected void CheckFOModCache()
		{
			string[] strCaches = Directory.GetFiles(Program.GameMode.ModInfoCacheDirectory);
			foreach (string strCache in strCaches)
			{
				string strFOModPath = Path.Combine(Program.GameMode.ModDirectory, Path.GetFileNameWithoutExtension(strCache) + ".fomod");
				if (!File.Exists(strFOModPath) || (File.GetLastWriteTimeUtc(strCache) < File.GetLastWriteTimeUtc(strFOModPath)))
					FileUtil.ForceDelete(strCache);
			}
		}

		/// <summary>
		/// The callback method for the call to retrieve a mod version from the Nexus website.
		/// </summary>
		/// <param name="p_objState">The base name of the mod whose version has been retrieved.</param>
		/// <param name="p_strWebVersion">The version of the mod on the Nexus website.</param>
		private void Nexus_GotFileVersion(object p_objState, string p_strWebVersion)
		{
			if (!this.Visible)
				return;
			m_dicWebVersions[(string)p_objState] = p_strWebVersion;
			ListViewItem lviMod = lvModList.Items[(string)p_objState];
			lviMod.UseItemStyleForSubItems = false;
			if (!String.IsNullOrEmpty(p_strWebVersion) && !p_strWebVersion.Equals(lviMod.SubItems["WebVersion"].Text))
			{
				lviMod.SubItems["WebVersion"].Text = p_strWebVersion;
				string strWebVersion = p_strWebVersion;
				string strVersion = ((fomod)lviMod.Tag).HumanReadableVersion;
				if (!strWebVersion.Equals(strVersion) && !strWebVersion.Equals(strVersion.Replace(".", "")))
				{
					if (strVersion.StartsWith("0.") && !strWebVersion.StartsWith("0."))
						strVersion = strVersion.Substring(2);
					if (strWebVersion.StartsWith("0.") && !strVersion.StartsWith("0."))
						strWebVersion = strWebVersion.Substring(2);
					if (strVersion.EndsWith(".0") && !strWebVersion.EndsWith(".0"))
						strVersion = strVersion.Substring(0, strVersion.Length - 2);
					if (strWebVersion.EndsWith(".0") && !strVersion.EndsWith(".0"))
						strWebVersion = strWebVersion.Substring(0, strWebVersion.Length - 2);

					if (!strWebVersion.Equals(strVersion))
						lviMod.SubItems["WebVersion"].BackColor = Color.LightSalmon;

				}
			}
		}

		private void AddFomodToList(fomod mod)
		{
			if ((m_nxaNexus != null) && !String.IsNullOrEmpty(mod.Website) && m_rgxNexusFileId.IsMatch(mod.Website))
			{
				if (!m_dicWebVersions.ContainsKey(mod.BaseName))
				{
					string strFileId = m_rgxNexusFileId.Match(mod.Website).Groups[1].Value.Trim();
					Int32 intFileId = -1;
					try
					{
						if (Int32.TryParse(strFileId, out intFileId))
							m_nxaNexus.GetFileVersionAsync(intFileId, Nexus_GotFileVersion, mod.BaseName);
					}
					catch (Exception e)
					{
#if TRACE
						Trace.WriteLine("Couldn't get version from " + mod.Website);
						Program.TraceException(e);
						Trace.Flush();
#endif
					}
				}
			}

			string strWebVersion = "NA";
			m_dicWebVersions.TryGetValue(mod.BaseName, out strWebVersion);

			if (lvModList.Items.ContainsKey(mod.BaseName))
				lvModList.Items.RemoveByKey(mod.BaseName);

			if (!cbGroups.Checked)
			{
				ListViewItem lvi = new ListViewItem(new string[] { mod.ModName, mod.HumanReadableVersion, strWebVersion, mod.Author });
				lvi.Tag = mod;
				lvi.Name = mod.BaseName;
				lvi.Checked = mod.IsActive;
				lvi.SubItems[2].Name = "WebVersion";
				lvModList.Items.Add(lvi);
				return;
			}
			bool added = false;
			for (int i = 0; i < groups.Count; i++)
			{
				if (Array.IndexOf<string>(mod.Groups, lgroups[i]) != -1)
				{
					added = true;
					ListViewItem lvi = new ListViewItem(new string[] { mod.ModName, mod.HumanReadableVersion, strWebVersion, mod.Author });
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
				ListViewItem lvi = new ListViewItem(new string[] { mod.ModName, mod.HumanReadableVersion, strWebVersion, mod.Author });
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

			Int32[] intColumnWidths = { 200, 100, 100, 100 };
			for (Int32 i = 0; i < intColumnWidths.Length; i++)
			{
				Int32 intWidth = -1;
				string strWidth = Settings.GetString("PackageManagerCol" + i + "Width");
				if (Int32.TryParse(strWidth, out intWidth))
					intColumnWidths[i] = intWidth;
			}

			lvModList.Clear();
			lvModList.Groups.Clear();

			if (!cbGroups.Checked)
			{
				lvModList.ShowGroups = false;
			}
			else
			{
				ListViewGroup lvg = new ListViewGroup("No group");
				lvModList.Groups.Add(lvg);

				for (int i = 0; i < groups.Count; i++)
				{
					lvg = new ListViewGroup(groups[i]);
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
				for (Int32 i = 0; i < intColumnWidths.Length; i++)
					lvModList.Columns[i].Width = intColumnWidths[i];
			}

			foreach (fomod mod in mods) AddFomodToList(mod);

			lvModList.ResumeLayout();
		}
		private void ReaddFomodToList(fomod mod)
		{
			lvModList.SuspendLayout();
			for (int i = 0; i < lvModList.Items.Count; i++) if (lvModList.Items[i].Tag == mod) lvModList.Items.RemoveAt(i--);
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
			for (Int32 i = mods.Count - 1; i >= 0; i--)
				if (mods[i].filepath.Equals(mod.filepath))
				{
					mods.RemoveAt(i);
					break;
				}
			mods.Add(mod);
			if (addToList) AddFomodToList(mod);
		}

		/// <summary>
		/// Logins into the websites.
		/// </summary>
		/// <remarks>
		/// If needed, credentials are gathered from the user.
		/// </remarks>
		private void WebsiteLogin()
		{
			if (!Settings.HasSetting("checkForNewModVersions"))
			{
				string strMessage = "Would you like FOMM to check for new versions of your mods?" +
									Environment.NewLine +
									"This may require you to login to some mod sites. You can change this " +
									"setting in the Settings window.";
				switch (MessageBox.Show(this, strMessage, "Check For New Mods", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				{
					case DialogResult.Yes:
						Settings.SetBool("checkForNewModVersions", true);
						break;
					case DialogResult.No:
						Settings.SetBool("checkForNewModVersions", false);
						break;
				}
			}
			if (Settings.GetBool("checkForNewModVersions"))
			{
				string strNexusLoginKey = Settings.GetString("nexusLoginKey");
				if (String.IsNullOrEmpty(strNexusLoginKey))
				{
					string strMessage = "You must log into the Nexus website.";
					LoginForm lgfLogin = new LoginForm(strMessage, Settings.GetString("nexusUsername"));
					while (lgfLogin.ShowDialog() != DialogResult.Cancel)
					{
						Settings.SetString("nexusUsername", lgfLogin.Username);
						m_nxaNexus = new NexusAPI(NexusSite.Fallout3, lgfLogin.Username, lgfLogin.Password);
						if (!m_nxaNexus.Login())
						{
							lgfLogin.ErrorMessage = "The given login information is invalid.";
							continue;
						}
						if (lgfLogin.StayLoggedIn)
						{
							strNexusLoginKey = m_nxaNexus.LoginKey;
							Settings.SetString("nexusLoginKey", strNexusLoginKey);
						}
						break;
					}
					return;
				}
				m_nxaNexus = new NexusAPI(NexusSite.Fallout3, strNexusLoginKey);
			}
		}

		private void PackageManager_Load(object sender, EventArgs e)
		{
			string tmp = Settings.GetString("PackageManagerPanelSplit");
			if (tmp != null) splitContainer1.SplitterDistance = Math.Max(splitContainer1.Panel1MinSize + 1, Math.Min(splitContainer1.Height - (splitContainer1.Panel2MinSize + 1), int.Parse(tmp))); ;
		}

		private void lvModList_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateModStateText();
		}

		/// <summary>
		/// Updates the UI elements to reflect the current mod's state.
		/// </summary>
		/// <remarks>
		/// This updates elements such as button text and the displayed description.
		/// </remarks>
		protected void UpdateModStateText()
		{
			if (lvModList.SelectedItems.Count == 0)
				return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			tbModInfo.Text = mod.HasInfo ? mod.Description : "No description is associaited with this fomod. Click 'edit info' if you want to add one.";

			butDeactivate.Enabled = mod.IsActive;
			butViewReadme.Enabled = mod.HasReadme;
			bActivate.Text = mod.IsActive ? "Reactivate" : "Activate";
			bEditScript.Text = mod.HasInstallScript ? "Edit script" : "Create script";
			pictureBox1.Image = mod.GetScreenshotImage();
		}

		private void lvModList_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if (((fomod)lvModList.Items[e.Index].Tag).IsActive) e.NewValue = CheckState.Checked;
			else e.NewValue = CheckState.Unchecked;
		}

		private void bEditScript_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1)
				return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			EditScriptForm esfEditor = new EditScriptForm();
			if (!mod.HasInstallScript)
				esfEditor.Script = new FomodScript(FomodScriptType.CSharp, FomodScriptEditor.DEFAULT_CSHARP_SCRIPT);
			else
				esfEditor.Script = mod.GetInstallScript();
			if (esfEditor.ShowDialog(this) == DialogResult.OK)
				mod.SetScript(esfEditor.Script);
			UpdateModStateText();
		}

		private void butViewReadme_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1)
				return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			if (mod.HasReadme)
			{
				ViewReadmeForm vrfEditor = new ViewReadmeForm(mod.GetReadme());
				vrfEditor.ShowDialog(this);
			}
		}

		private void PackageManager_FormClosing(object sender, FormClosingEventArgs e)
		{
			Settings.SetWindowPosition("PackageManager", this);
			Settings.SetInt("SelectedAddFomodAction", sbtAddFomod.SelectedItemIndex);
			Settings.SetString("PackageManagerPanelSplit", splitContainer1.SplitterDistance.ToString());
			for (Int32 i = 0; i < lvModList.Columns.Count; i++)
				Settings.SetString("PackageManagerCol" + i + "Width", lvModList.Columns[i].Width.ToString());

			foreach (ListViewItem lvi in lvModList.Items)
			{
				((fomod)lvi.Tag).Dispose();
			}
		}

		private void bEditInfo_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			if ((new InfoEditor(mod)).ShowDialog() == DialogResult.OK)
			{
				if (cbGroups.Checked) ReaddFomodToList(mod);
				else
				{
					ListViewItem lvi = lvModList.SelectedItems[0];
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
		/// Activates the given fomod.
		/// </summary>
		/// <remarks>
		/// This method checks to see if the given fomod could be an upgrade for another fomod.
		/// </remarks>
		/// <param name="mod">The fomod to activate.</param>
		private void ActivateFomod(fomod mod)
		{
			bool booFound = false;
			fomod fomodMod = null;
			foreach (ListViewItem lviFomod in lvModList.Items)
			{
				fomodMod = (fomod)lviFomod.Tag;
				if (fomodMod.ModName.Equals(mod.ModName) && fomodMod.IsActive && !fomodMod.BaseName.Equals(mod.BaseName))
				{
					//ask to do upgrade
					string strUpgradeMessage = "A different verion of {0} has been detected. The installed verion is {1}, the new verion is {2}. Would you like to upgrade?" + Environment.NewLine + "Selecting No will install the new FOMod normally.";
					switch (MessageBox.Show(String.Format(strUpgradeMessage, fomodMod.ModName, fomodMod.HumanReadableVersion, mod.HumanReadableVersion), "Upgrade", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					{
						case DialogResult.Yes:
							ModUpgrader mduUpgrader = new ModUpgrader(mod, fomodMod.BaseName);
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
					break;
			}
			ModInstaller mdiInstaller = new ModInstaller(mod);
			mdiInstaller.Install();
		}

		/// <summary>
		/// Activates, Reactivates, or Deactivates the selected mod as appropriate.
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
				ModReactivator mraReactivator = new ModReactivator(mod);
				mraReactivator.Upgrade();
			}
			else
			{
				ModUninstaller mduUninstaller = new ModUninstaller(mod);
				mduUninstaller.Uninstall();
			}
			if (cbGroups.Checked)
			{
				foreach (ListViewItem lvi in lvModList.Items)
				{
					if (lvi.Tag == mod) lvi.Checked = mod.IsActive;
				}
			}
			else
			{
				lvModList.SelectedItems[0].Checked = mod.IsActive;
			}
			butDeactivate.Enabled = mod.IsActive;
			if (!mod.IsActive) bActivate.Text = "Activate";
			else bActivate.Text = "Reactivate";

			mf.RefreshPluginList();
		}

		private void bActivate_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			if (!mod.IsActive)
				ToggleActivation(mod, false);
			else
				ToggleActivation(mod, true);
		}

		private void butDeactivate_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			ToggleActivation(mod, false);
		}

		/// <summary>
		/// Creates a fomod from a source archive.
		/// </summary>
		/// <param name="p_strPath">The path to the archive from which to create the fomod.</param>
		public void AddNewFomod(string p_strPath)
		{
			FomodFromSourceBuilder ffbBuilder = new FomodFromSourceBuilder();
			IList<string> lstFomodPaths = ffbBuilder.BuildFomodFromSource(p_strPath);
			foreach (string strFomodPath in lstFomodPaths)
				AddFomod(strFomodPath, true);
		}

		private void fomodContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1)
			{
				e.Cancel = true;
				return;
			}
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			if (mod.Email.Length == 0) emailAuthorToolStripMenuItem.Visible = false;
			else emailAuthorToolStripMenuItem.Visible = true;
			if (mod.Website.Length == 0) visitWebsiteToolStripMenuItem.Visible = false;
			else visitWebsiteToolStripMenuItem.Visible = true;
		}

		private void visitWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			System.Diagnostics.Process.Start(mod.Website, "");
		}

		private void emailAuthorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			System.Diagnostics.Process.Start("mailto://" + mod.Email, "");
		}

		private void cbGroups_CheckedChanged(object sender, EventArgs e)
		{
			RebuildListView();
			Settings.SetBool("PackageManagerShowsGroups", cbGroups.Checked);
			bActivateGroup.Enabled = cbGroups.Checked;
			bDeactivateGroup.Enabled = cbGroups.Checked;
			cmbSortOrder.Enabled = !cbGroups.Checked;
		}

		private void bEditGroups_Click(object sender, EventArgs e)
		{
			Form f = new Form();
			Settings.GetWindowPosition("GroupEditor", f);
			f.Text = "Groups";
			TextBox tb = new TextBox();
			f.Controls.Add(tb);
			tb.Dock = DockStyle.Fill;
			tb.AcceptsReturn = true;
			tb.Multiline = true;
			tb.ScrollBars = ScrollBars.Vertical;
			tb.Text = string.Join(Environment.NewLine, groups.ToArray());
			tb.Select(0, 0);
			f.FormClosing += delegate(object sender2, FormClosingEventArgs args2)
			{
				Settings.SetWindowPosition("GroupEditor", f);
			};
			f.ShowDialog();
			groups.Clear();
			groups.AddRange(tb.Lines);
			for (int i = 0; i < groups.Count; i++)
			{
				if (groups[i].Length == 0) groups.RemoveAt(i--);
			}
			lgroups.Clear();
			for (int i = 0; i < groups.Count; i++) lgroups.Add(groups[i].ToLowerInvariant());
			RebuildListView();
			Settings.SetStringArray("fomodGroups", groups.ToArray());
		}

		private void fomodStatusToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			Form f = new Form();
			Settings.GetWindowPosition("FomodStatus", f);
			f.Text = "Fomod status";
			TextBox tb = new TextBox();
			f.Controls.Add(tb);
			tb.Dock = DockStyle.Fill;
			tb.Multiline = true;
			tb.Text = mod.GetStatusString();
			tb.ReadOnly = true;
			tb.BackColor = System.Drawing.SystemColors.Window;
			tb.Select(0, 0);
			tb.ScrollBars = ScrollBars.Vertical;
			f.ShowDialog();
			Settings.SetWindowPosition("FomodStatus", f);
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			if (mod.IsActive)
			{
				MessageBox.Show("Cannot delete an active fomod");
				return;
			}
			for (int i = 0; i < lvModList.Items.Count; i++) if (lvModList.Items[i].Tag == mod) lvModList.Items.RemoveAt(i--);
			mod.Dispose();
			File.Delete(mod.filepath);
			mods.Remove(mod);
		}

		private void bActivateGroup_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			if (!cbGroups.Checked) return;
			foreach (ListViewItem lvi in lvModList.SelectedItems[0].Group.Items)
			{
				fomod mod = (fomod)lvi.Tag;
				if (mod.IsActive) continue;
				ActivateFomod(mod);
				if (cbGroups.Checked)
				{
					foreach (ListViewItem lvi2 in lvModList.Items)
					{
						if (lvi2.Tag == mod) lvi2.Checked = mod.IsActive;
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
			if (lvModList.SelectedItems.Count != 1) return;
			if (!cbGroups.Checked) return;
			foreach (ListViewItem lvi in lvModList.SelectedItems[0].Group.Items)
			{
				fomod mod = (fomod)lvi.Tag;
				if (!mod.IsActive) continue;
				ModUninstaller mduUninstaller = new ModUninstaller(mod);
				mduUninstaller.Uninstall(true);
				if (cbGroups.Checked)
				{
					foreach (ListViewItem lvi2 in lvModList.Items)
					{
						if (lvi2.Tag == mod) lvi2.Checked = mod.IsActive;
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
			if (MessageBox.Show("This will deactivate all fomods.\nAre you sure you want to continue?", "Warning", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
			foreach (ListViewItem lvi in lvModList.Items)
			{
				fomod mod = (fomod)lvi.Tag;
				if (!mod.IsActive)
					continue;
				ModUninstaller mduUninstaller = new ModUninstaller(mod);
				mduUninstaller.Uninstall(true);
			}
			foreach (ListViewItem lvi in lvModList.Items) lvi.Checked = false;

			mf.RefreshPluginList();
		}

		private class FomodSorter : System.Collections.IComparer
		{
			public static int Mode;
			public int Compare(object a, object b)
			{
				fomod m1 = (fomod)((ListViewItem)a).Tag;
				fomod m2 = (fomod)((ListViewItem)b).Tag;

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
			if (cmbSortOrder.SelectedIndex < 0) return;
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
			if (cbGroups.Checked) return;
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
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			ToggleActivation(mod, false);
		}

		private void pictureBox1_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null && (pictureBox1.Image.Size.Width > pictureBox1.Width || pictureBox1.Image.Size.Height > pictureBox1.Height))
			{
				(new ImageForm(pictureBox1.Image)).ShowDialog();
			}
		}

		#region Fomod Extraction

		/// <summary>
		/// Handles the <see cref="Button.Click"/> event of the extract button.
		/// </summary>
		/// <remarks>
		/// This queries the user for the destinations directory, and then launches the
		/// background process to extract the fomod.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butExtractFomod_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1)
				return;
			fomod fomodMod = (fomod)lvModList.SelectedItems[0].Tag;

			if (fbdExtractFomod.ShowDialog(this) == DialogResult.OK)
			{
				using (m_bwdProgress = new BackgroundWorkerProgressDialog(UnpackFomod))
				{
					string strOutput = Path.Combine(fbdExtractFomod.SelectedPath, Path.GetFileNameWithoutExtension(fomodMod.filepath));
					if (!Directory.Exists(strOutput))
						Directory.CreateDirectory(strOutput);
					m_bwdProgress.WorkMethodArguments = new Pair<fomod, string>(fomodMod, strOutput);
					m_bwdProgress.ShowDialog();
				}
				m_bwdProgress = null;
			}
		}

		/// <summary>
		/// Unpacks the given fomod to the specified directory.
		/// </summary>
		/// <remarks>
		/// This method is used by the background worker.
		/// </remarks>
		/// <param name="p_objArgs">A <see cref="Pair{fomod, string}"/> containing the fomod to extract
		/// and the direcotry to which to extract it.</param>
		private void UnpackFomod(object p_objArgs)
		{
			if (!(p_objArgs is Pair<fomod, string>))
				throw new ArgumentException("Given argument is not a Pair<fomod,string>.", "p_objArgs");
			fomod fomodMod = ((Pair<fomod, string>)p_objArgs).Key;
			string strOutput = ((Pair<fomod, string>)p_objArgs).Value;
			List<string> lstFiles = fomodMod.GetFileList();

			m_bwdProgress.ShowItemProgress = false;
			m_bwdProgress.OverallMessage = "Extracting Files...";
			m_bwdProgress.OverallProgressMaximum = lstFiles.Count;
			m_bwdProgress.OverallProgressStep = 1;

			using (SevenZipExtractor szeExtractor = new SevenZipExtractor(fomodMod.filepath))
			{
				szeExtractor.FileExtractionFinished += new EventHandler<FileInfoEventArgs>(UnpackFomod_FileExtractionFinished);
				szeExtractor.FileExtractionStarted += new EventHandler<FileInfoEventArgs>(UnpackFomod_FileExtractionStarted);
				szeExtractor.ExtractArchive(strOutput);
			}
		}

		/// <summary>
		/// Called when a file has been extracted from a fomod.
		/// </summary>
		/// <remarks>
		/// This steps the progress of the create fomod from archive progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		void UnpackFomod_FileExtractionFinished(object sender, FileInfoEventArgs e)
		{
			m_bwdProgress.StepItemProgress();
		}

		/// <summary>
		/// Called when a file is about to be extracted from a fomod.
		/// </summary>
		/// <remarks>
		/// This cancels the compression if the user has clicked the cancel button of the progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="FileNameEventArgs"/> describing the event arguments.</param>
		void UnpackFomod_FileExtractionStarted(object sender, FileInfoEventArgs e)
		{
			e.Cancel = m_bwdProgress.Cancelled();
		}

		#endregion

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the add fomod button.
		/// </summary>
		/// <remarks>
		/// Adds a fomod to the package manager.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void addFOMODToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
			AddNewFomod(openFileDialog1.FileName);
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the create fomod from folder button.
		/// </summary>
		/// <remarks>
		/// Creates a fomod from a folder.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void createFromFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.SelectedPath = m_strLastFromFolderPath;
			fbd.ShowNewFolderButton = false;
			fbd.Description = "Pick a folder to convert to a fomod";
			if (fbd.ShowDialog() != DialogResult.OK) return;
			m_strLastFromFolderPath = fbd.SelectedPath;
			Settings.SetString("LastBuildFOMODFromFolderPath", Path.GetDirectoryName(m_strLastFromFolderPath));
			AddNewFomod(fbd.SelectedPath);
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the create fomod button.
		/// </summary>
		/// <remarks>
		/// Creates a fomod using the fomod builder.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void createFOMODToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FomodBuilderForm fbfBuilder = new FomodBuilderForm();
			if (fbfBuilder.ShowDialog(this) == DialogResult.OK)
			{
				if (!String.IsNullOrEmpty(fbfBuilder.FomodPath))
					AddFomod(fbfBuilder.FomodPath, true);
			}
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the add pfp button.
		/// </summary>
		/// <remarks>
		/// Creates a FOMod from a PFP.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void addPFPToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PremadeFomodPackForm pkfPFPForm = new PremadeFomodPackForm(PremadeFomodPackForm.OpenPFPMode.Install);
			if (pkfPFPForm.ShowDialog(this) == DialogResult.Cancel)
				return;

			PremadeFomodPack pfpPack = new PremadeFomodPack(pkfPFPForm.PFPPath);
			List<KeyValuePair<string, string>> lstCopyInstructions = pfpPack.GetCopyInstructions(pkfPFPForm.SourcesPath);
			string strPremadeSource = Archive.GenerateArchivePath(pkfPFPForm.PFPPath, pfpPack.PremadePath);
			lstCopyInstructions.Add(new KeyValuePair<string, string>(strPremadeSource, "/"));

			NewFomodBuilder fgnGenerator = new NewFomodBuilder();
			string strNewFomodPath = fgnGenerator.BuildFomod(pfpPack.FomodName, lstCopyInstructions, null, null, false, null, null);
			if (!String.IsNullOrEmpty(strNewFomodPath))
				AddFomod(strNewFomodPath, true);
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the edit pfp button.
		/// </summary>
		/// <remarks>
		/// Edits a PFP.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void editPFPToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PremadeFomodPackForm pkfPFPForm = new PremadeFomodPackForm(PremadeFomodPackForm.OpenPFPMode.Edit);
			if (pkfPFPForm.ShowDialog(this) == DialogResult.Cancel)
				return;

			PremadeFomodPack pfpPack = new PremadeFomodPack(pkfPFPForm.PFPPath);
			FomodBuilderForm fbfBuilder = new FomodBuilderForm(pfpPack, pkfPFPForm.SourcesPath);
			if (fbfBuilder.ShowDialog(this) == DialogResult.OK)
			{
				if (!String.IsNullOrEmpty(fbfBuilder.FomodPath))
					AddFomod(fbfBuilder.FomodPath, true);
			}
		}

		/// <summary>
		/// Exports the list of mods being managed by FOMM.
		/// </summary>
		/// <remarks>
		/// The list of mods is export to a file of the user's choosing. Optionally, only active mods
		/// can be exported.
		/// </remarks>
		/// <param name="p_booActiveOnly">Whether only active mods should be exported.</param>
		protected void ExportModList(bool p_booActiveOnly)
		{
			SaveFileDialog sfdModList = new SaveFileDialog();
			sfdModList.Filter = "Text file (*.txt)|*.txt";
			sfdModList.AddExtension = true;
			sfdModList.RestoreDirectory = true;
			if (sfdModList.ShowDialog() != DialogResult.OK)
				return;

			Int32 intMaxNameLength = 0;
			Int32 intMaxVersionLength = 0;
			for (int i = 0; i < lvModList.Items.Count; i++)
				if (lvModList.Items[i].Checked || !p_booActiveOnly)
				{
					if (lvModList.Items[i].Text.Length > intMaxNameLength)
						intMaxNameLength = lvModList.Items[i].Text.Length;
					if (lvModList.Items[i].SubItems[1].Text.Length > intMaxVersionLength)
						intMaxVersionLength = lvModList.Items[i].SubItems[1].Text.Length;
				}
		
			StreamWriter swrModList = new StreamWriter(sfdModList.FileName);
			try
			{
				for (int i = 0; i < lvModList.Items.Count; i++)
					if (lvModList.Items[i].Checked || !p_booActiveOnly)
						swrModList.WriteLine(String.Format("[{0}] {1,-" + intMaxNameLength + "}\t{2,-" + intMaxVersionLength + "}\t{3}", (lvModList.Items[i].Checked ? "X" : " "), lvModList.Items[i].Text, lvModList.Items[i].SubItems[1].Text, ((fomod)lvModList.Items[i].Tag).filepath));
			}
			finally
			{
				swrModList.Close();
			}
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the Export Mod List menu item.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void exportModListToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExportModList(false);
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the Export Active Mod List menu item.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void exportActiveModListToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExportModList(true);
		}
	}
}