using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using Fomm.PackageManager.Upgrade;
using SevenZip;
using NexusAPI;

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
		private Dictionary<fomod, string> m_dicWebVersions = new Dictionary<fomod, string>();

		private void AddFomodToList(fomod mod)
		{
			string strWebVersion = "NA";
			m_dicWebVersions.TryGetValue(mod, out strWebVersion);

			if (!cbGroups.Checked)
			{
				ListViewItem lvi = new ListViewItem(new string[] { mod.Name, mod.VersionS, strWebVersion, mod.Author });
				lvi.Tag = mod;
				lvi.Checked = mod.IsActive;
				lvModList.Items.Add(lvi);
				return;
			}
			bool added = false;
			for (int i = 0; i < groups.Count; i++)
			{
				if (Array.IndexOf<string>(mod.groups, lgroups[i]) != -1)
				{
					added = true;
					ListViewItem lvi = new ListViewItem(new string[] { mod.Name, mod.VersionS, strWebVersion, mod.Author });
					lvi.Tag = mod;
					lvi.Checked = mod.IsActive;
					lvModList.Items.Add(lvi);
					lvModList.Groups[i + 1].Items.Add(lvi);
				}
			}
			if (!added)
			{
				ListViewItem lvi = new ListViewItem(new string[] { mod.Name, mod.VersionS, strWebVersion, mod.Author });
				lvi.Tag = mod;
				lvi.Checked = mod.IsActive;
				lvModList.Items.Add(lvi);
				lvModList.Groups[0].Items.Add(lvi);
			}
		}
		private void RebuildListView()
		{
			lvModList.SuspendLayout();

			Int32[] intColumnWidths = {200, 100, 100, 100};
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
			mods.Add(mod);
			if (addToList) AddFomodToList(mod);
		}
		public PackageManager(MainForm mf)
		{
			this.mf = mf;
			InitializeComponent();
			this.Icon = Fomm.Properties.Resources.fomm02;
			cmbSortOrder.ContextMenu = new ContextMenu();
			lvModList.ListViewItemSorter = new FomodSorter();
			Settings.GetWindowPosition("PackageManager", this);
			m_strLastFromFolderPath = Settings.GetString("LastBuildFOMODFromFolderPath");

			foreach (string modpath in Program.GetFiles(Program.PackageDir, "*.fomod.zip"))
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
			foreach (string modpath in Program.GetFiles(Program.PackageDir, "*.fomod"))
			{
				AddFomod(modpath, false);
			}

			Timer tmrG = new Timer();
			tmrG.Tick += new EventHandler(tmrG_Tick);
			NexusAPI.NexusAPI

			RebuildListView();
		}

		int inte = 0;
		void tmrG_Tick(object sender, EventArgs e)
		{
			m_dicWebVersions[(fomod)lvModList.Items[inte].Tag] = "3.0";
		}

		private void PackageManager_Load(object sender, EventArgs e)
		{
			string tmp = Settings.GetString("PackageManagerPanelSplit");
			if (tmp != null) splitContainer1.SplitterDistance = Math.Max(splitContainer1.Panel1MinSize + 1, Math.Min(splitContainer1.Height - (splitContainer1.Panel2MinSize + 1), int.Parse(tmp))); ;
		}

		private void lvModList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count == 0) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			if (mod.HasInfo) tbModInfo.Text = mod.Description;
			else tbModInfo.Text = "No description is associaited with this fomod. Click 'edit info' if you want to add one.";

			butDeactivate.Enabled = mod.IsActive;
			if (!mod.IsActive) bActivate.Text = "Activate";
			else bActivate.Text = "Reactivate";

			if (mod.HasInstallScript) bEditScript.Text = "Edit script";
			else bEditScript.Text = "Create script";

			pictureBox1.Image = mod.GetScreenshot();
		}

		private void lvModList_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if (((fomod)lvModList.Items[e.Index].Tag).IsActive) e.NewValue = CheckState.Checked;
			else e.NewValue = CheckState.Unchecked;
		}

		private void bEditScript_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			string result = TextEditor.ShowEditor(mod.GetInstallScript(), TextEditorType.Script, true);
			if (result != null) mod.SetScript(result);
		}

		private void bEditReadme_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			string result = null;
			if (!mod.HasReadme)
			{
				result = TextEditor.ShowEditor("", TextEditorType.Text, true);
			}
			else
			{
				string readme = mod.GetReadme();
				switch (mod.ReadmeExt)
				{
					case ".txt":
						result = TextEditor.ShowEditor(readme, TextEditorType.Text, true);
						break;
					case ".rtf":
						result = TextEditor.ShowEditor(readme, TextEditorType.Rtf, true);
						break;
					case ".htm":
					case ".html":
						Form f = new Form();
						WebBrowser wb = new WebBrowser();
						f.Controls.Add(wb);
						wb.Dock = DockStyle.Fill;
						wb.DocumentCompleted += delegate(object unused1, WebBrowserDocumentCompletedEventArgs unused2)
						{
							if (!string.IsNullOrEmpty(wb.DocumentTitle)) f.Text = wb.DocumentTitle;
							else f.Text = "Readme";
						};
						wb.WebBrowserShortcutsEnabled = false;
						wb.AllowWebBrowserDrop = false;
						wb.AllowNavigation = false;
						wb.DocumentText = readme;
						f.ShowDialog();
						break;
					default:
						MessageBox.Show("fomod had an unrecognised readme type", "Error");
						return;
				}
			}

			if (result != null) mod.SetReadme(result);
		}

		private void PackageManager_FormClosing(object sender, FormClosingEventArgs e)
		{
			Settings.SetWindowPosition("PackageManager", this);
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
					lvi.SubItems[0].Text = mod.Name;
					lvi.SubItems[1].Text = mod.VersionS;
					lvi.SubItems[2].Text = mod.Author;
					tbModInfo.Text = mod.Description;
					pictureBox1.Image = mod.GetScreenshot();
				}
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
				if (fomodMod.Name.Equals(mod.Name) && fomodMod.IsActive && !fomodMod.BaseName.Equals(mod.BaseName))
				{
					//ask to do upgrade
					string strUpgradeMessage = "A different verion of {0} has been detected. The installed verion is {1}, the new verion is {2}. Would you like to upgrade?" + Environment.NewLine + "Selecting No will install the new FOMOD normally.";
					switch (MessageBox.Show(String.Format(strUpgradeMessage, fomodMod.Name, fomodMod.VersionS, mod.VersionS), "Upgrade", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
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

			mf.RefreshEspList();
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

		private void CheckFomodFolder(ref string path, string tesnexusext, string fomodname)
		{
			foreach (string aifile in Program.GetFiles(path, "ArchiveInvalidation.txt", SearchOption.AllDirectories)) File.Delete(aifile);
			foreach (string aifile in Program.GetFiles(path, "thumbs.db", SearchOption.AllDirectories)) File.Delete(aifile);
			foreach (string aifile in Program.GetFiles(path, "desktop.ini", SearchOption.AllDirectories)) File.Delete(aifile);

			//this code removes any top-level folders until it finds esp/esm/bsa, or the top-level folder
			// is a fomod/textures/meshes/music/shaders/video/facegen/menus/lodsettings/lsdata/sound folder.
			string[] directories = Directory.GetDirectories(path);
			while (directories.Length == 1 && Program.GetFiles(path, "*.esp").Length == 0 && Program.GetFiles(path, "*.esm").Length == 0 && Program.GetFiles(path, "*.bsa").Length == 0)
			{
				directories = directories[0].Split(Path.DirectorySeparatorChar);
				string name = directories[directories.Length - 1].ToLowerInvariant();
				if (name != "fomod" && name != "textures" && name != "meshes" && name != "music" && name != "shaders" && name != "video" && name != "facegen" && name != "menus" && name != "lodsettings" && name != "lsdata" && name != "sound")
				{
					foreach (string file in Directory.GetFiles(path))
					{
						string newpath2 = Path.Combine(Path.Combine(Path.GetDirectoryName(file), name), Path.GetFileName(file));
						if (!File.Exists(newpath2)) File.Move(file, newpath2);
					}
					path = Path.Combine(path, name);
					directories = Directory.GetDirectories(path);
				}
				else break;
			}

			string[] readme = Directory.GetFiles(path, "readme - " + fomodname + ".*", SearchOption.TopDirectoryOnly);
			if (readme.Length == 0)
			{
				readme = Directory.GetFiles(path, "*readme*.*", SearchOption.AllDirectories);
				if (readme.Length == 0) readme = Program.GetFiles(path, "*.rtf", SearchOption.AllDirectories);
				if (readme.Length == 0) readme = Program.GetFiles(path, "*.txt", SearchOption.AllDirectories);
				if (readme.Length == 0) readme = Program.GetFiles(path, "*.html", SearchOption.AllDirectories);
				if (readme.Length > 0)
				{
					if (Settings.GetBool("UseDocsFolder"))
					{
						Directory.CreateDirectory(Path.Combine(path, "docs"));
						File.Move(readme[0], Path.Combine(path, "docs\\Readme - " + fomodname + Path.GetExtension(readme[0])));
					}
					else
					{
						File.Move(readme[0], Path.Combine(path, "Readme - " + fomodname + Path.GetExtension(readme[0])));
					}
				}
			}
			if (tesnexusext != null)
			{
				if (!Directory.Exists(Path.Combine(path, "fomod"))) Directory.CreateDirectory(Path.Combine(path, "fomod"));
				if (!File.Exists(Path.Combine(path, "fomod\\info.xml")))
				{
					System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
					xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-16", null));
					System.Xml.XmlElement el, el2;
					xmlDoc.AppendChild(el = xmlDoc.CreateElement("fomod"));
					el.AppendChild(el2 = xmlDoc.CreateElement("Website"));
					el2.InnerText = tesnexusext;
					xmlDoc.Save(Path.Combine(path, "fomod\\info.xml"));
				}
			}
			if (Program.GetFiles(path, "*.esp", SearchOption.AllDirectories).Length + Program.GetFiles(path, "*.esm", SearchOption.AllDirectories).Length >
					Program.GetFiles(path, "*.esp", SearchOption.TopDirectoryOnly).Length + Program.GetFiles(path, "*.esm", SearchOption.TopDirectoryOnly).Length)
			{
				if (!File.Exists(Path.Combine(path, "fomod\\script.cs")) && !File.Exists(Path.Combine(path, "fomod\\ModuleConfig.xml")))
				{
					MessageBox.Show("This archive contains plugins in subdirectories, and will need a script attached for fomm to install it correctly.", "Warning");
				}
			}
		}

		private bool CheckFomodName(ref string newpath)
		{
			if (File.Exists(newpath))
			{
				string newpath2 = null;
				bool match = false;
				for (int i = 2; i < 999; i++)
				{
					newpath2 = Path.ChangeExtension(newpath, null) + "(" + i + ").fomod";
					if (!File.Exists(newpath2))
					{
						match = true;
						break;
					}
				}
				if (!match)
				{
					MessageBox.Show("File '" + newpath + "' already exists.", "Error");
					return false;
				}
				if (MessageBox.Show("File '" + newpath + "' already exists. Continue anyway?\n" +
					"A new file named '" + newpath2 + "' will be created", "Warning", MessageBoxButtons.YesNo) != DialogResult.Yes) return false;
				newpath = newpath2;
			}
			return true;
		}

		#region Create fomod from Folder

		/// <summary>
		/// Creates a fomod from a source folder.
		/// </summary>
		/// <param name="p_strPath">The path to the folder from which to create the fomod.</param>
		private void BuildFomodFromFolder(string p_strPath)
		{
			p_strPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string strName = Path.GetFileName(p_strPath);
			string strFomodPath = Path.Combine(Program.PackageDir, strName + ".fomod");
			CheckFomodFolder(ref p_strPath, null, strName);
			if (!CheckFomodName(ref strFomodPath))
				return;

			using (m_bwdProgress = new BackgroundWorkerProgressDialog(CompressFomodFromFolder))
			{
				m_bwdProgress.OverallMessage = "Creating Fomod...";
				m_bwdProgress.ShowItemProgress = false;
				m_bwdProgress.OverallProgressMaximum = Directory.GetFiles(p_strPath, "*", SearchOption.AllDirectories).Length;
				m_bwdProgress.OverallProgressStep = 1;
				m_bwdProgress.WorkMethodArguments = new string[] { p_strPath, strFomodPath };
				if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
				{
					if (File.Exists(strFomodPath))
						File.Delete(strFomodPath);
					return;
				}
			}
			AddFomod(strFomodPath, true);
		}

		/// <summary>
		/// Compress a folder to a fomod.
		/// </summary>
		/// <remarks>
		/// This method is called by a <see cref="BackgroundWorkerProgressDialog"/> and so displays progress.
		/// </remarks>
		/// <param name="p_objArgs">An array of strings. Index 0 is the path to the folder to compress. Index 1 is
		/// the path to the new fomod.</param>
		protected void CompressFomodFromFolder(object p_objArgs)
		{
			string strFolderPath = ((string[])p_objArgs)[0];
			string strFomodPath = ((string[])p_objArgs)[1];

			SevenZipCompressor szcCompressor = new SevenZipCompressor();
			szcCompressor.CompressionLevel = CompressionLevel.Ultra;
			szcCompressor.CompressionMethod = CompressionMethod.Default;
			szcCompressor.ArchiveFormat = OutArchiveFormat.Zip;
			szcCompressor.FileCompressionStarted += new EventHandler<FileNameEventArgs>(CompressFomodFromFolder_FileCompressionStarted);
			szcCompressor.FileCompressionFinished += new EventHandler(CompressFomodFromFolder_FileCompressionFinished);
			szcCompressor.CompressDirectory(strFolderPath, strFomodPath);
		}

		/// <summary>
		/// Called when a file has been added to a new fomod.
		/// </summary>
		/// <remarks>
		/// This steps the progress of the create fomod from folder progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void CompressFomodFromFolder_FileCompressionFinished(object sender, EventArgs e)
		{
			m_bwdProgress.StepOverallProgress();
		}

		/// <summary>
		/// Called when a file is about to be added to a new fomod.
		/// </summary>
		/// <remarks>
		/// This cancels the compression if the user has clicked the cancel button of the progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="FileNameEventArgs"/> describing the event arguments.</param>
		private void CompressFomodFromFolder_FileCompressionStarted(object sender, FileNameEventArgs e)
		{
			e.Cancel = m_bwdProgress.Cancelled();
		}

		#endregion

		#region Repacking Archive to Fomod

		/// <summary>
		/// Repacks an archive into a fomod.
		/// </summary>
		/// <remarks>
		/// This method is called by a <see cref="BackgroundWorkerProgressDialog"/> and so displays progress.
		/// </remarks>
		/// <param name="p_objArgs">An array of strings. Index 0 is the path to the archive to repack. Index 1 is
		/// the path to the new fomod. Index 2 is the URL to the mod on tes nexus.</param>
		protected void RepackToFomod(object p_objArgs)
		{
			string strArchivePath = (string)((object[])p_objArgs)[0];
			string strFomodPath = (string)((object[])p_objArgs)[1];
			string strTesNexusUrl = (string)((object[])p_objArgs)[2];
			string strTmpPath = Program.CreateTempDirectory();

			SevenZipExtractor szeExtractor = new SevenZipExtractor(strArchivePath);
			szeExtractor.FileExtractionFinished += new EventHandler(RepackToFomod_FileExtractionFinished);
			szeExtractor.FileExtractionStarted += new EventHandler<FileInfoEventArgs>(RepackToFomod_FileExtractionStarted);
			m_bwdProgress.ItemProgressMaximum = (Int32)szeExtractor.FilesCount;
			m_bwdProgress.ItemProgressStep = 1;
			m_bwdProgress.ItemMessage = "Extracting Files...";
			szeExtractor.ExtractArchive(strTmpPath);

			m_bwdProgress.StepOverallProgress();

			//Check for packing errors here
			CheckFomodFolder(ref strTmpPath, strTesNexusUrl, Path.GetFileNameWithoutExtension(strFomodPath));

			m_bwdProgress.StepOverallProgress();

			SevenZipCompressor szcCompressor = new SevenZipCompressor();
			szcCompressor.CompressionLevel = CompressionLevel.Ultra;
			szcCompressor.CompressionMethod = CompressionMethod.Default;
			szcCompressor.ArchiveFormat = OutArchiveFormat.Zip;
			szcCompressor.FileCompressionStarted += new EventHandler<FileNameEventArgs>(RepackToFomod_FileCompressionStarted);
			szcCompressor.FileCompressionFinished += new EventHandler(RepackToFomod_FileCompressionFinished);
			m_bwdProgress.ItemProgress = 0;
			m_bwdProgress.ItemMessage = "Compressing Files...";
			szcCompressor.CompressDirectory(strTmpPath, strFomodPath);

			m_bwdProgress.StepOverallProgress();
		}

		/// <summary>
		/// Called when a file has been added to a new fomod.
		/// </summary>
		/// <remarks>
		/// This steps the progress of the create fomod from archive progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		void RepackToFomod_FileCompressionFinished(object sender, EventArgs e)
		{
			m_bwdProgress.StepItemProgress();
		}

		/// <summary>
		/// Called when a file is about to be added to a new fomod.
		/// </summary>
		/// <remarks>
		/// This cancels the compression if the user has clicked the cancel button of the progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="FileNameEventArgs"/> describing the event arguments.</param>
		void RepackToFomod_FileCompressionStarted(object sender, FileNameEventArgs e)
		{
			e.Cancel = m_bwdProgress.Cancelled();
		}

		/// <summary>
		/// Called when a file has been extracted from a source archive.
		/// </summary>
		/// <remarks>
		/// This steps the progress of the create fomod from archive progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		void RepackToFomod_FileExtractionFinished(object sender, EventArgs e)
		{
			m_bwdProgress.StepItemProgress();
		}

		/// <summary>
		/// Called when a file is about to be extracted from a source archive.
		/// </summary>
		/// <remarks>
		/// This cancels the compression if the user has clicked the cancel button of the progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="FileNameEventArgs"/> describing the event arguments.</param>
		void RepackToFomod_FileExtractionStarted(object sender, FileInfoEventArgs e)
		{
			e.Cancel = m_bwdProgress.Cancelled();
		}

		/// <summary>
		/// Creates a fomod from a source archive.
		/// </summary>
		/// <param name="p_strPath">The path to the archive from which to create the fomod.</param>
		public void AddNewFomod(string p_strPath)
		{
			bool booRepack = false;
			string strFomodPath = null;
			if (p_strPath.EndsWith(".fomod", StringComparison.OrdinalIgnoreCase))
				strFomodPath = Path.Combine(Program.PackageDir, Path.GetFileName(p_strPath));
			else if (p_strPath.EndsWith(".fomod.zip", StringComparison.OrdinalIgnoreCase))
				strFomodPath = Path.Combine(Program.PackageDir, Path.GetFileNameWithoutExtension(p_strPath));
			else if (p_strPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) && p_strPath.Contains("-fomod-"))
			{
				string tmppath2 = Path.GetFileName(p_strPath);
				strFomodPath = Path.Combine(Program.PackageDir, Path.GetFileName(tmppath2.Substring(0, tmppath2.IndexOf("-fomod-")))) + ".fomod";
			}
			else
			{
				strFomodPath = Path.Combine(Program.PackageDir, Path.ChangeExtension(Path.GetFileName(p_strPath), ".fomod"));
				booRepack = true;
			}

			string strTesNexusUrl = Path.GetFileNameWithoutExtension(strFomodPath);
			Int32 intFileId;
			if (strTesNexusUrl.Contains("-") && int.TryParse(strTesNexusUrl.Substring(strTesNexusUrl.LastIndexOf('-') + 1), out intFileId))
			{
				strFomodPath = Path.Combine(Path.GetDirectoryName(strFomodPath), strTesNexusUrl.Remove(strTesNexusUrl.LastIndexOf('-'))) + Path.GetExtension(strFomodPath);
				strTesNexusUrl = @"http://www.fallout3nexus.com/downloads/file.php?id=" + intFileId;
			}
			else
				strTesNexusUrl = null;
			if (!CheckFomodName(ref strFomodPath))
				return;
			if (booRepack)
			{
				try
				{
					using (m_bwdProgress = new BackgroundWorkerProgressDialog(RepackToFomod))
					{
						m_bwdProgress.OverallMessage = "Creating Fomod...";
						m_bwdProgress.OverallProgressMaximum = 3;
						m_bwdProgress.OverallProgressStep = 1;
						m_bwdProgress.WorkMethodArguments = new object[] { p_strPath, strFomodPath, strTesNexusUrl };
						if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
						{
							if (File.Exists(strFomodPath))
								File.Delete(strFomodPath);
							return;
						}
					}
				}
				catch
				{
					MessageBox.Show("Unknown file type, or corrupt archive.", "Error");
					return;
				}
			}
			else
			{
				if (MessageBox.Show("Make a copy of the original file?", "", MessageBoxButtons.YesNo) != DialogResult.Yes)
					File.Move(p_strPath, strFomodPath);
				else
					File.Copy(p_strPath, strFomodPath);
			}
			AddFomod(strFomodPath, true);
		}

		#endregion

		private void bAddNew_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
			AddNewFomod(openFileDialog1.FileName);
		}

		private void bFomodFromFolder_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.SelectedPath = m_strLastFromFolderPath;
			fbd.ShowNewFolderButton = false;
			fbd.Description = "Pick a folder to convert to a fomod";
			if (fbd.ShowDialog() != DialogResult.OK) return;
			m_strLastFromFolderPath = fbd.SelectedPath;
			Settings.SetString("LastBuildFOMODFromFolderPath", Path.GetDirectoryName(m_strLastFromFolderPath));
			BuildFomodFromFolder(fbd.SelectedPath);
		}

		private void fomodContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1)
			{
				e.Cancel = true;
				return;
			}
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			if (mod.email.Length == 0) emailAuthorToolStripMenuItem.Visible = false;
			else emailAuthorToolStripMenuItem.Visible = true;
			if (mod.website.Length == 0) visitWebsiteToolStripMenuItem.Visible = false;
			else visitWebsiteToolStripMenuItem.Visible = true;
		}

		private void visitWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			System.Diagnostics.Process.Start(mod.website, "");
		}

		private void emailAuthorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			System.Diagnostics.Process.Start("mailto://" + mod.email, "");
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

			mf.RefreshEspList();
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

			mf.RefreshEspList();
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

			mf.RefreshEspList();
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
						return m1.Name.CompareTo(m2.Name);
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

			SevenZipExtractor szeExtractor = new SevenZipExtractor(fomodMod.filepath);
			szeExtractor.FileExtractionFinished += new EventHandler(UnpackFomod_FileExtractionFinished);
			szeExtractor.FileExtractionStarted += new EventHandler<FileInfoEventArgs>(UnpackFomod_FileExtractionStarted);
			szeExtractor.ExtractArchive(strOutput);
		}

		/// <summary>
		/// Called when a file has been extracted from a fomod.
		/// </summary>
		/// <remarks>
		/// This steps the progress of the create fomod from archive progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		void UnpackFomod_FileExtractionFinished(object sender, EventArgs e)
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
	}
}