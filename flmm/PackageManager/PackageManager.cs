using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace fomm.PackageManager {
    public partial class PackageManager : Form {

        private readonly List<fomod> mods=new List<fomod>();
        private bool AllowCheckedChanges;

        private void AddFomod(string modpath) {
            fomod mod;
            try {
                mod=new fomod(modpath);
            } catch(Exception ex) {
                MessageBox.Show("Error loading '"+Path.GetFileName(modpath)+"'\n"+ex.Message);
                return;
            }
            mods.Add(mod);
            ListViewItem lvi=new ListViewItem(new string[] { mod.Name, mod.VersionS, mod.Author });
            lvi.Tag=mod;
            lvi.Checked=mod.IsActive;
            lvModList.Items.Add(lvi);
        }
        public PackageManager() {
            InitializeComponent();
            Settings.GetWindowPosition("PackageManager", this);
            foreach(string modpath in Directory.GetFiles(Program.PackageDir, "*.fomod.zip")) {
                if(!File.Exists(Path.ChangeExtension(modpath, null))) File.Move(modpath, Path.ChangeExtension(modpath, null));
            }
            AllowCheckedChanges=true;
            foreach(string modpath in Directory.GetFiles(Program.PackageDir, "*.fomod")) {
                AddFomod(modpath);
            }
            AllowCheckedChanges=false;
        }

        private void lvModList_SelectedIndexChanged(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count==0) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            if(mod.HasInfo) tbModInfo.Text=mod.Description;
            else tbModInfo.Text="Warning: info.xml is missing from this fomod.";

            if(!mod.IsActive) bActivate.Text="Activate";
            else bActivate.Text="Deactivate";

            if(mod.HasScript) bEditScript.Text="Edit script";
            else bEditScript.Text="Create script";

            pictureBox1.Image=mod.GetScreenshot();
        }

        private void lvModList_ItemCheck(object sender, ItemCheckEventArgs e) {
            if(!AllowCheckedChanges) e.NewValue=e.CurrentValue;
        }

        private void bEditScript_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            string result=ScriptEditor.ShowEditor(mod.GetScript());
            if(result!=null) mod.SetScript(result);
        }

        private void bEditReadme_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            string result=null;
            if(!mod.HasReadme) {
                result=TextEditor.ShowEditor("", false);
            } else {
                string readme=mod.GetReadme();
                switch(mod.ReadmeExt) {
                case ".txt":
                    result=TextEditor.ShowEditor(readme, false);
                    break;
                case ".rtf":
                    result=TextEditor.ShowEditor(readme, true);
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
                ListViewItem lvi=lvModList.SelectedItems[0];
                lvi.SubItems[0].Text=mod.Name;
                lvi.SubItems[1].Text=mod.VersionS;
                lvi.SubItems[2].Text=mod.Author;
                tbModInfo.Text=mod.Description;
                pictureBox1.Image=mod.GetScreenshot();
            }
        }

        private void bActivate_Click(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count!=1) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            if(!mod.IsActive) mod.Activate();
            else mod.Deactivate();
            AllowCheckedChanges=true;
            lvModList.SelectedItems[0].Checked=mod.IsActive;
            AllowCheckedChanges=false;
            if(!mod.IsActive) bActivate.Text="Activate";
            else bActivate.Text="Deactivate";
        }

        private void bAddNew_Click(object sender, EventArgs e) {
            if(openFileDialog1.ShowDialog()!=DialogResult.OK) return;
            string oldpath=openFileDialog1.FileName, newpath;
            if(oldpath.EndsWith(".fomod", StringComparison.InvariantCultureIgnoreCase)) {
                newpath=Path.Combine(Program.PackageDir, Path.GetFileName(oldpath));
            } else if(oldpath.EndsWith(".fomod.zip", StringComparison.InvariantCultureIgnoreCase)) {
                newpath=Path.Combine(Program.PackageDir, Path.GetFileNameWithoutExtension(oldpath));
            } else if(oldpath.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase)) {
                //Insert checks that this is a valid fomod here
                newpath=Path.Combine(Program.PackageDir, Path.ChangeExtension(Path.GetFileName(oldpath), ".fomod"));
            } else {
                MessageBox.Show("Unknown file type", "Error");
                return;
            }
            if(File.Exists(newpath)) {
                MessageBox.Show("A fomod with the same name is already installed", "Error");
                return;
            }
            if(MessageBox.Show("Make a copy of the original file?", "", MessageBoxButtons.YesNo)!=DialogResult.Yes) {
                File.Move(oldpath, newpath);
            } else {
                File.Copy(oldpath, newpath);
            }
            AddFomod(newpath);
        }
    }
}