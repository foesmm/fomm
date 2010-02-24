using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using Fomm.PackageManager.Upgrade;

namespace Fomm.PackageManager
{
	internal partial class PackageManager : Form
	{

		private readonly List<fomod> mods = new List<fomod>();
		private readonly List<string> groups;
		private readonly List<string> lgroups;
		private readonly MainForm mf;

		private void AddFomodToList(fomod mod)
		{
			if (!cbGroups.Checked)
			{
				ListViewItem lvi = new ListViewItem(new string[] { mod.Name, mod.VersionS, mod.Author });
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
					ListViewItem lvi = new ListViewItem(new string[] { mod.Name, mod.VersionS, mod.Author });
					lvi.Tag = mod;
					lvi.Checked = mod.IsActive;
					lvModList.Items.Add(lvi);
					lvModList.Groups[i + 1].Items.Add(lvi);
				}
			}
			if (!added)
			{
				ListViewItem lvi = new ListViewItem(new string[] { mod.Name, mod.VersionS, mod.Author });
				lvi.Tag = mod;
				lvi.Checked = mod.IsActive;
				lvModList.Items.Add(lvi);
				lvModList.Groups[0].Items.Add(lvi);
			}
		}
		private void RebuildListView()
		{
			lvModList.SuspendLayout();

			int w1, w2, w3;
			if (lvModList.Columns.Count == 0)
			{
				string tmp = Settings.GetString("PackageManagerCol1Width");
				if (tmp != null) w1 = int.Parse(tmp); else w1 = 200;
				tmp = Settings.GetString("PackageManagerCol2Width");
				if (tmp != null) w2 = int.Parse(tmp); else w2 = 100;
				tmp = Settings.GetString("PackageManagerCol3Width");
				if (tmp != null) w3 = int.Parse(tmp); else w3 = 100;
			}
			else
			{
				w1 = lvModList.Columns[0].Width;
				w2 = lvModList.Columns[1].Width;
				w3 = lvModList.Columns[2].Width;
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
				lvModList.Columns.Add("Author");
				lvModList.Columns[0].Width = w1;
				lvModList.Columns[1].Width = w2;
				lvModList.Columns[2].Width = w3;
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

			RebuildListView();
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

			if (!mod.IsActive) bActivate.Text = "Activate";
			else bActivate.Text = "Deactivate";

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
			Settings.SetString("PackageManagerCol1Width", lvModList.Columns[0].Width.ToString());
			Settings.SetString("PackageManagerCol2Width", lvModList.Columns[1].Width.ToString());
			Settings.SetString("PackageManagerCol3Width", lvModList.Columns[2].Width.ToString());

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

		private void ActivateFomod(fomod mod)
		{
			bool booFound = false;
			fomod fomodMod = null;
			foreach (ListViewItem lviFomod in lvModList.Items)
			//foreach (fomod fomodMod in mods)
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

		private void bActivate_Click(object sender, EventArgs e)
		{
			if (lvModList.SelectedItems.Count != 1) return;
			fomod mod = (fomod)lvModList.SelectedItems[0].Tag;
			if (!mod.IsActive)
			{
				ActivateFomod(mod);
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
			if (!mod.IsActive) bActivate.Text = "Activate";
			else bActivate.Text = "Deactivate";

			mf.RefreshEspList();
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
				if (!File.Exists(Path.Combine(path, "fomod\\script.cs")))
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

		private void BuildFomodFromFolder(string path)
		{
			path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string name = Path.GetFileName(path);
			string newpath = Path.Combine(Program.PackageDir, name + ".fomod");
			CheckFomodFolder(ref path, null, name);
			if (!CheckFomodName(ref newpath)) return;
			ICSharpCode.SharpZipLib.Zip.FastZip fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
			fastZip.CreateZip(newpath, path, true, null);
			AddFomod(newpath, true);
		}

		public void AddNewFomod(string oldpath)
		{
			bool Repack = false;
			string newpath, tmppath = null;
			if (oldpath.EndsWith(".fomod", StringComparison.OrdinalIgnoreCase))
			{
				newpath = Path.Combine(Program.PackageDir, Path.GetFileName(oldpath));
			}
			else if (oldpath.EndsWith(".fomod.zip", StringComparison.OrdinalIgnoreCase))
			{
				newpath = Path.Combine(Program.PackageDir, Path.GetFileNameWithoutExtension(oldpath));
			}
			else if (oldpath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) && oldpath.Contains("-fomod-"))
			{
				string tmppath2 = Path.GetFileName(oldpath);
				newpath = Path.Combine(Program.PackageDir, Path.GetFileName(tmppath2.Substring(0, tmppath2.IndexOf("-fomod-")))) + ".fomod";
			}
			else if (oldpath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
			{
				tmppath = Program.CreateTempDirectory();
				ICSharpCode.SharpZipLib.Zip.FastZip fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
				fastZip.ExtractZip(oldpath, tmppath, null);
				Repack = true;
				newpath = Path.Combine(Program.PackageDir, Path.ChangeExtension(Path.GetFileName(oldpath), ".fomod"));
			}
			else if (oldpath.EndsWith(".rar", StringComparison.OrdinalIgnoreCase))
			{
				tmppath = Program.CreateTempDirectory();
				Unrar unrar = null;
				try
				{
					unrar = new Unrar(oldpath);
					unrar.Open(Unrar.OpenMode.Extract);
					while (unrar.ReadHeader()) unrar.ExtractToDirectory(tmppath);
				}
				catch
				{
					MessageBox.Show("The file was password protected, or was not a valid rar file.", "Error");
					return;
				}
				finally
				{
					if (unrar != null) unrar.Close();
				}
				Repack = true;
				newpath = Path.Combine(Program.PackageDir, Path.ChangeExtension(Path.GetFileName(oldpath), ".fomod"));
			}
			else if (oldpath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
			{
				tmppath = Program.CreateTempDirectory();
				System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(@"fomm\7za.exe",
					"x \"" + oldpath + "\" * -o\"" + tmppath + "\" -aos -y  -r");
				psi.CreateNoWindow = true;
				psi.UseShellExecute = false;
				System.Diagnostics.Process p = System.Diagnostics.Process.Start(psi);
				p.WaitForExit();
				if (Directory.GetFileSystemEntries(tmppath).Length == 0)
				{
					MessageBox.Show("Failed to extract anything from 7-zip archive", "Error");
					return;
				}
				Repack = true;
				newpath = Path.Combine(Program.PackageDir, Path.ChangeExtension(Path.GetFileName(oldpath), ".fomod"));
			}
			else
			{
				MessageBox.Show("Unknown file type", "Error");
				return;
			}
			string tesnexusext = Path.GetFileNameWithoutExtension(newpath);
			int id;
			if (tesnexusext.Contains("-") && int.TryParse(tesnexusext.Substring(tesnexusext.LastIndexOf('-') + 1), out id))
			{
				newpath = Path.Combine(Path.GetDirectoryName(newpath), tesnexusext.Remove(tesnexusext.LastIndexOf('-'))) + Path.GetExtension(newpath);
				tesnexusext = @"http://www.fallout3nexus.com/downloads/file.php?id=" + id;
			}
			else tesnexusext = null;
			if (!CheckFomodName(ref newpath)) return;
			if (Repack)
			{
				//Check for packing errors here
				CheckFomodFolder(ref tmppath, tesnexusext, Path.GetFileNameWithoutExtension(newpath));
				ICSharpCode.SharpZipLib.Zip.FastZip fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
				fastZip.CreateZip(newpath, tmppath, true, null);
			}
			else
			{
				if (MessageBox.Show("Make a copy of the original file?", "", MessageBoxButtons.YesNo) != DialogResult.Yes)
				{
					File.Move(oldpath, newpath);
				}
				else
				{
					File.Copy(oldpath, newpath);
				}
			}
			AddFomod(newpath, true);
		}

		private void bAddNew_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
			AddNewFomod(openFileDialog1.FileName);
		}

		private void bFomodFromFolder_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.ShowNewFolderButton = false;
			fbd.Description = "Pick a folder to convert to a fomod";
			if (fbd.ShowDialog() != DialogResult.OK) return;
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
			bActivate_Click(null, null);
		}

		private void pictureBox1_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null && (pictureBox1.Image.Size.Width > pictureBox1.Width || pictureBox1.Image.Size.Height > pictureBox1.Height))
			{
				(new ImageForm(pictureBox1.Image)).ShowDialog();
			}
		}
	}
}