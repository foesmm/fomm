using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace fomm.PackageManager {
    internal partial class PackageManager : Form {

        private readonly List<fomod> mods=new List<fomod>();
        private readonly List<string> groups;
        private readonly List<string> lgroups;
        private readonly MainForm mf;

        private void AddFomodToList(fomod mod) {
            if(!cbGroups.Checked) {
                ListViewItem lvi=new ListViewItem(new string[] { mod.Name, mod.VersionS, mod.Author });
                lvi.Tag=mod;
                lvi.Checked=mod.IsActive;
                lvModList.Items.Add(lvi);
                return;
            }
            bool added=false;
            for(int i=0;i<groups.Count;i++) {
                if(Array.IndexOf<string>(mod.groups, lgroups[i])!=-1) {
                    added=true;
                    ListViewItem lvi=new ListViewItem(new string[] { mod.Name, mod.VersionS, mod.Author });
                    lvi.Tag=mod;
                    lvi.Checked=mod.IsActive;
                    lvModList.Items.Add(lvi);
                    lvModList.Groups[i+1].Items.Add(lvi);
                }
            }
            if(!added) {
                ListViewItem lvi=new ListViewItem(new string[] { mod.Name, mod.VersionS, mod.Author });
                lvi.Tag=mod;
                lvi.Checked=mod.IsActive;
                lvModList.Items.Add(lvi);
                lvModList.Groups[0].Items.Add(lvi);
            }
        }
        private void RebuildListView() {
            lvModList.SuspendLayout();

            lvModList.Clear();
            lvModList.Groups.Clear();

            if(!cbGroups.Checked) {
                lvModList.ShowGroups=false;
            } else {
                ListViewGroup lvg=new ListViewGroup("No group");
                lvModList.Groups.Add(lvg);

                for(int i=0;i<groups.Count;i++) {
                    lvg=new ListViewGroup(groups[i]);
                    lvModList.Groups.Add(lvg);
                }
                lvModList.ShowGroups=true;
            }

            if(lvModList.Columns.Count==0) {
                lvModList.Columns.Add("Name");
                lvModList.Columns[0].Width=200;
                lvModList.Columns.Add("Version");
                lvModList.Columns.Add("Author");
            }

            foreach(fomod mod in mods) AddFomodToList(mod);

            lvModList.ResumeLayout();
        }
        private void ReaddFomodToList(fomod mod) {
            lvModList.SuspendLayout();
            for(int i=0;i<lvModList.Items.Count;i++) if(lvModList.Items[i].Tag==mod) lvModList.Items.RemoveAt(i--);
            AddFomodToList(mod);
            lvModList.ResumeLayout();
        }

        private void AddFomod(string modpath, bool addToList) {
            fomod mod;
            try {
                mod=new fomod(modpath);
            } catch(Exception ex) {
                MessageBox.Show("Error loading '"+Path.GetFileName(modpath)+"'\n"+ex.Message);
                return;
            }
            mods.Add(mod);
            if(addToList) AddFomodToList(mod);
        }
        public PackageManager(MainForm mf) {
            this.mf=mf;
            InitializeComponent();
            cmbSortOrder.ContextMenu=new ContextMenu();
            lvModList.ListViewItemSorter=new FomodSorter();
            Settings.GetWindowPosition("PackageManager", this);
            foreach(string modpath in Directory.GetFiles(Program.PackageDir, "*.fomod.zip")) {
                if(!File.Exists(Path.ChangeExtension(modpath, null))) File.Move(modpath, Path.ChangeExtension(modpath, null));
            }

            string[] groups=Settings.GetStringArray("fomodGroups");
            if(groups==null) {
                groups=new string[] {
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
            this.groups=new List<string>(groups);
            this.lgroups=new List<string>(groups.Length);
            for(int i=0;i<groups.Length;i++) lgroups.Add(groups[i].ToLowerInvariant());

            if(Settings.GetBool("PackageManagerShowsGroups")) {
                cbGroups.Checked=true;
            }
            foreach(string modpath in Directory.GetFiles(Program.PackageDir, "*.fomod")) {
                AddFomod(modpath, false);
            }

            RebuildListView();
        }

        private void lvModList_SelectedIndexChanged(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count==0) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            if(mod.HasInfo) tbModInfo.Text=mod.Description;
            else tbModInfo.Text="No description is associaited with this fomod. Click 'edit info' if you want to add one.";

            if(!mod.IsActive) bActivate.Text="Activate";
            else bActivate.Text="Deactivate";

            if(mod.HasScript) bEditScript.Text="Edit script";
            else bEditScript.Text="Create script";

            pictureBox1.Image=mod.GetScreenshot();
        }

        private void lvModList_ItemCheck(object sender, ItemCheckEventArgs e) {
            if(((fomod)lvModList.Items[e.Index].Tag).IsActive) e.NewValue=CheckState.Checked;
            else e.NewValue=CheckState.Unchecked;
        }

        private void bEditScript_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            string result=TextEditor.ShowEditor(mod.GetScript(), TextEditorType.Script);
            if(result!=null) mod.SetScript(result);
        }

        private void bEditReadme_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            string result=null;
            if(!mod.HasReadme) {
                result=TextEditor.ShowEditor("", TextEditorType.Text);
            } else {
                string readme=mod.GetReadme();
                switch(mod.ReadmeExt) {
                case ".txt":
                    result=TextEditor.ShowEditor(readme, TextEditorType.Text);
                    break;
                case ".rtf":
                    result=TextEditor.ShowEditor(readme, TextEditorType.Rtf);
                    break;
                case ".htm":
                case ".html":
                    Form f=new Form();
                    WebBrowser wb=new WebBrowser();
                    f.Controls.Add(wb);
                    wb.Dock=DockStyle.Fill;
                    wb.DocumentCompleted+=delegate(object unused1, WebBrowserDocumentCompletedEventArgs unused2)
                    {
                        if(wb.DocumentTitle!=null&&wb.DocumentTitle!="") f.Text=wb.DocumentTitle;
                        else f.Text="Readme";
                    };
                    wb.WebBrowserShortcutsEnabled=false;
                    wb.AllowWebBrowserDrop=false;
                    wb.AllowNavigation=false;
                    wb.DocumentText=readme;
                    f.ShowDialog();
                    break;
                default:
                    MessageBox.Show("fomod had an unrecognised readme type", "Error");
                    return;
                }
            }
           
            if(result!=null) mod.SetReadme(result);
        }

        private void PackageManager_FormClosing(object sender, FormClosingEventArgs e) {
            Settings.SetWindowPosition("PackageManager", this);
            foreach(ListViewItem lvi in lvModList.Items) {
                ((fomod)lvi.Tag).Dispose();
            }
        }

        private void bEditInfo_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            if((new InfoEditor(mod)).ShowDialog()==DialogResult.OK) {
                if(cbGroups.Checked) ReaddFomodToList(mod);
                else {
                    ListViewItem lvi=lvModList.SelectedItems[0];
                    lvi.SubItems[0].Text=mod.Name;
                    lvi.SubItems[1].Text=mod.VersionS;
                    lvi.SubItems[2].Text=mod.Author;
                    tbModInfo.Text=mod.Description;
                    pictureBox1.Image=mod.GetScreenshot();
                }
            }
            
        }

        private void bActivate_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            if(!mod.IsActive) mod.Activate();
            else mod.Deactivate();
            if(cbGroups.Checked) {
                foreach(ListViewItem lvi in lvModList.Items) {
                    if(lvi.Tag==mod) lvi.Checked=mod.IsActive;
                }
            } else {
                lvModList.SelectedItems[0].Checked=mod.IsActive;
            }
            if(!mod.IsActive) bActivate.Text="Activate";
            else bActivate.Text="Deactivate";

            mf.RefreshEspList();
        }

        public void AddNewFomod(string oldpath) {
            bool Repack=false;
            string newpath, tmppath=null;
            if(oldpath.EndsWith(".fomod", StringComparison.InvariantCultureIgnoreCase)) {
                newpath=Path.Combine(Program.PackageDir, Path.GetFileName(oldpath));
            } else if(oldpath.EndsWith(".fomod.zip", StringComparison.InvariantCultureIgnoreCase)) {
                newpath=Path.Combine(Program.PackageDir, Path.GetFileNameWithoutExtension(oldpath));
            } else if(oldpath.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase)) {
                tmppath=Program.CreateTempDirectory();
                ICSharpCode.SharpZipLib.Zip.FastZip fastZip=new ICSharpCode.SharpZipLib.Zip.FastZip();
                fastZip.ExtractZip(oldpath, tmppath, null);
                Repack=true;
                newpath=Path.Combine(Program.PackageDir, Path.ChangeExtension(Path.GetFileName(oldpath), ".fomod"));
            } else if(oldpath.EndsWith(".rar", StringComparison.InvariantCultureIgnoreCase)) {
                tmppath=Program.CreateTempDirectory();
                Unrar unrar=null;
                try {
                    unrar=new Unrar(oldpath);
                    unrar.Open(Unrar.OpenMode.Extract);
                    while(unrar.ReadHeader()) unrar.ExtractToDirectory(tmppath);
                } catch {
                    MessageBox.Show("The file was password protected, or was not a valid rar file.", "Error");
                    return;
                } finally {
                    if(unrar!=null) unrar.Close();
                }
                Repack=true;
                newpath=Path.Combine(Program.PackageDir, Path.ChangeExtension(Path.GetFileName(oldpath), ".fomod"));
            } else if(oldpath.EndsWith(".7z", StringComparison.InvariantCultureIgnoreCase)) {
                tmppath=Program.CreateTempDirectory();
                System.Diagnostics.ProcessStartInfo psi=new System.Diagnostics.ProcessStartInfo(@"fomm\7za.exe",
                    "x \""+oldpath+"\" * -o\""+tmppath+"\" -aos -y  -r");
                psi.CreateNoWindow=true;
                psi.UseShellExecute=false;
                System.Diagnostics.Process p=System.Diagnostics.Process.Start(psi);
                p.WaitForExit();
                if(Directory.GetFileSystemEntries(tmppath).Length==0) {
                    MessageBox.Show("Failed to extract anything from 7-zip archive", "Error");
                    return;
                }
                Repack=true;
                newpath=Path.Combine(Program.PackageDir, Path.ChangeExtension(Path.GetFileName(oldpath), ".fomod"));
            } else {
                MessageBox.Show("Unknown file type", "Error");
                return;
            }
            if(File.Exists(newpath)) {
                MessageBox.Show("A fomod with the same name is already installed", "Error");
                return;
            }
            string texnexusext=Path.GetFileNameWithoutExtension(newpath);
            int unused;
            if(texnexusext.Contains("-")&&int.TryParse(texnexusext.Substring(texnexusext.LastIndexOf('-'))+1, out unused)) {
                newpath=Path.Combine(Path.GetDirectoryName(newpath), texnexusext.Remove(texnexusext.LastIndexOf('-')))+Path.GetExtension(newpath);
            }
            if(Repack) {
                //Check for packing errors here
                foreach(string aifile in Directory.GetFiles(tmppath, "ArchiveInvalidation.txt", SearchOption.AllDirectories)) File.Delete(aifile);
                string[] directories=Directory.GetDirectories(tmppath);
                if(directories.Length==1&&Directory.GetFiles(tmppath, "*.esp").Length==0&&Directory.GetFiles(tmppath, "*.esm").Length==0&&Directory.GetFiles(tmppath, "*.bsa").Length==0) {
                    directories=directories[0].Split(Path.DirectorySeparatorChar);
                    string name=directories[directories.Length-1].ToLowerInvariant();
                    if(name!="textures"&&name!="meshes"&&name!="music"&&name!="shaders"&&name!="video"&&name!="facegen"&&name!="menus"&&name!="lodsettings"&&name!="lsdata") {
                        foreach(string file in Directory.GetFiles(tmppath)) {
                            string newpath2=Path.Combine(Path.Combine(Path.GetDirectoryName(file), name), Path.GetFileName(file));
                            if(!File.Exists(newpath2)) File.Move(file, newpath2);
                        }
                        tmppath=Path.Combine(tmppath, name);
                    }
                }
                string[] readme=Directory.GetFiles(tmppath, "readme - "+Path.GetFileNameWithoutExtension(newpath)+".*", SearchOption.TopDirectoryOnly);
                if(readme.Length==0) {
                    readme=Directory.GetFiles(tmppath, "*readme*.*", SearchOption.AllDirectories);
                    if(readme.Length==0) readme=Directory.GetFiles(tmppath, "*.rtf", SearchOption.AllDirectories);
                    if(readme.Length==0) readme=Directory.GetFiles(tmppath, "*.txt", SearchOption.AllDirectories);
                    if(readme.Length==0) readme=Directory.GetFiles(tmppath, "*.html", SearchOption.AllDirectories);
                    if(readme.Length>0) {
                        File.Move(readme[0], Path.Combine(tmppath, "Readme - "+Path.GetFileNameWithoutExtension(newpath)+Path.GetExtension(readme[0])));
                    }
                }
                if(Directory.GetFiles(tmppath, "*.esp", SearchOption.AllDirectories).Length+Directory.GetFiles(tmppath, "*.esm", SearchOption.AllDirectories).Length>
                    Directory.GetFiles(tmppath, "*.esp", SearchOption.TopDirectoryOnly).Length+Directory.GetFiles(tmppath, "*.esm", SearchOption.TopDirectoryOnly).Length) {
                    if(!File.Exists(Path.Combine(tmppath, "fomod\\script.cs"))) {
                        MessageBox.Show("This archive contains plugins in subdirectories, and will need a script attached for fomm to install it correctly.", "Warning");
                    }
                }
                ICSharpCode.SharpZipLib.Zip.FastZip fastZip=new ICSharpCode.SharpZipLib.Zip.FastZip();
                fastZip.CreateZip(newpath, tmppath, true, null);
            } else {
                if(MessageBox.Show("Make a copy of the original file?", "", MessageBoxButtons.YesNo)!=DialogResult.Yes) {
                    File.Move(oldpath, newpath);
                } else {
                    File.Copy(oldpath, newpath);
                }
            }
            AddFomod(newpath, true);
        }

        private void bAddNew_Click(object sender, EventArgs e) {
            if(openFileDialog1.ShowDialog()!=DialogResult.OK) return;
            AddNewFomod(openFileDialog1.FileName);
        }

        private void fomodContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
            if(lvModList.SelectedItems.Count!=1) {
                e.Cancel=true;
                return;
            }
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            if(mod.email=="") emailAuthorToolStripMenuItem.Visible=false;
            else emailAuthorToolStripMenuItem.Visible=true;
            if(mod.website=="") visitWebsiteToolStripMenuItem.Visible=false;
            else visitWebsiteToolStripMenuItem.Visible=true;
        }

        private void visitWebsiteToolStripMenuItem_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            System.Diagnostics.Process.Start(mod.website, "");
        }

        private void emailAuthorToolStripMenuItem_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            System.Diagnostics.Process.Start("mailto://"+mod.email, "");
        }

        private void cbGroups_CheckedChanged(object sender, EventArgs e) {
            RebuildListView();
            Settings.SetBool("PackageManagerShowsGroups", cbGroups.Checked);
            bActivateGroup.Enabled=cbGroups.Checked;
            bDeactivateGroup.Enabled=cbGroups.Checked;
            cmbSortOrder.Enabled=!cbGroups.Checked;
        }

        private void bEditGroups_Click(object sender, EventArgs e) {
            Form f=new Form();
            Settings.GetWindowPosition("GroupEditor", f);
            f.Text="Groups";
            TextBox tb=new TextBox();
            f.Controls.Add(tb);
            tb.Dock=DockStyle.Fill;
            tb.AcceptsReturn=true;
            tb.Multiline=true;
            tb.ScrollBars=ScrollBars.Vertical;
            tb.Text=string.Join(Environment.NewLine, groups.ToArray());
            tb.Select(0, 0);
            f.FormClosing+=delegate(object sender2, FormClosingEventArgs args2)
            {
                Settings.SetWindowPosition("GroupEditor", f);
            };
            f.ShowDialog();
            groups.Clear();
            groups.AddRange(tb.Lines);
            for(int i=0;i<groups.Count;i++) {
                if(groups[i]=="") groups.RemoveAt(i--);
            }
            lgroups.Clear();
            for(int i=0;i<groups.Count;i++) lgroups.Add(groups[i].ToLowerInvariant());
            RebuildListView();
            Settings.SetStringArray("fomodGroups", groups.ToArray());
        }

        private void fomodStatusToolStripMenuItem_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            Form f=new Form();
            Settings.GetWindowPosition("FomodStatus", f);
            f.Text="Fomod status";
            TextBox tb=new TextBox();
            f.Controls.Add(tb);
            tb.Dock=DockStyle.Fill;
            tb.Multiline=true;
            tb.Text=mod.GetStatusString();
            tb.ReadOnly=true;
            tb.BackColor=System.Drawing.SystemColors.Window;
            tb.Select(0, 0);
            tb.ScrollBars=ScrollBars.Vertical;
            f.ShowDialog();
            Settings.SetWindowPosition("FomodStatus", f);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            if(mod.IsActive) {
                MessageBox.Show("Cannot delete an active fomod");
                return;
            }
            for(int i=0;i<lvModList.Items.Count;i++) if(lvModList.Items[i].Tag==mod) lvModList.Items.RemoveAt(i--);
            mod.Dispose();
            File.Delete(mod.filepath);
            mods.Remove(mod);
        }

        private void bActivateGroup_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            if(!cbGroups.Checked) return;
            foreach(ListViewItem lvi in lvModList.SelectedItems[0].Group.Items) {
                fomod mod=(fomod)lvi.Tag;
                if(mod.IsActive) continue;
                mod.Activate();
                if(cbGroups.Checked) {
                    foreach(ListViewItem lvi2 in lvModList.Items) {
                        if(lvi2.Tag==mod) lvi2.Checked=mod.IsActive;
                    }
                } else {
                    lvModList.SelectedItems[0].Checked=mod.IsActive;
                }
            }

            mf.RefreshEspList();
        }

        private void bDeactivateGroup_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            if(!cbGroups.Checked) return;
            foreach(ListViewItem lvi in lvModList.SelectedItems[0].Group.Items) {
                fomod mod=(fomod)lvi.Tag;
                if(!mod.IsActive) continue;
                mod.Deactivate();
                if(cbGroups.Checked) {
                    foreach(ListViewItem lvi2 in lvModList.Items) {
                        if(lvi2.Tag==mod) lvi2.Checked=mod.IsActive;
                    }
                } else {
                    lvModList.SelectedItems[0].Checked=mod.IsActive;
                }
            }

            mf.RefreshEspList();
        }

        private void bDeactivateAll_Click(object sender, EventArgs e) {
            if(MessageBox.Show("This will deactivate all fomods.\nAre you sure you want to continue?", "Warning", MessageBoxButtons.YesNo)!=DialogResult.Yes) return;
            foreach(ListViewItem lvi in lvModList.Items) {
                fomod mod=(fomod)lvi.Tag;
                if(!mod.IsActive) continue;
                else mod.Deactivate();
            }
            foreach(ListViewItem lvi in lvModList.Items) lvi.Checked=false;

            mf.RefreshEspList();
        }

        private class FomodSorter : System.Collections.IComparer {
            public static int Mode=0;
            public int Compare(object a, object b) {
                fomod m1=(fomod)((ListViewItem)a).Tag;
                fomod m2=(fomod)((ListViewItem)b).Tag;

                switch(Mode) {
                case 0:
                    return 0;
                case 1:
                    return m1.baseName.CompareTo(m2.baseName);
                case 2:
                    return m1.Name.CompareTo(m2.Name);
                case 3:
                    return m1.Author.CompareTo(m2.Author);
                }
                return 0;
            }
        }

        private void cmbSortOrder_SelectedIndexChanged(object sender, EventArgs e) {
            if(cmbSortOrder.SelectedIndex<0) return;
            FomodSorter.Mode=cmbSortOrder.SelectedIndex+1;
            lvModList.Sort();
            cmbSortOrder.Text="Sort order";
        }

        private void cmbSortOrder_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled=true;
        }

        private void lvModList_ColumnClick(object sender, ColumnClickEventArgs e) {
            if(cbGroups.Checked) return;
            switch(e.Column) {
            case 0:
                cmbSortOrder.SelectedIndex=1;
                break;
            case 2:
                cmbSortOrder.SelectedIndex=2;
                break;
            }
        }

        private void lvModList_ItemActivate(object sender, EventArgs e) {
            bActivate_Click(null, null);
        }
    }
}