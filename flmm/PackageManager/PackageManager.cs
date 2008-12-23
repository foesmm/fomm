using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace fomm.PackageManager {
    public partial class PackageManager : Form {

        private readonly List<fomod> mods=new List<fomod>();

        public PackageManager() {
            InitializeComponent();
            foreach(string modpath in Directory.GetFiles(Program.PackageDir, "*.fomod")) {
                fomod mod;
                try {
                    mod=new fomod(modpath);
                } catch(Exception ex) {
                    MessageBox.Show("Error loading '"+Path.GetFileName(modpath)+"'\n"+ex.Message);
                    continue;
                }
                mods.Add(mod);
                ListViewItem lvi=new ListViewItem(new string[] { mod.Name, mod.VersionS, mod.Author });
                lvi.Tag=mod;
                lvi.Checked=mod.IsActive;
                lvModList.Items.Add(lvi);
            }
        }

        private void lvModList_SelectedIndexChanged(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count==0) return;
            fomod mod=(fomod)lvModList.SelectedItems[0].Tag;
            if(mod.HasInfo) tbModInfo.Text=mod.Description;
            else tbModInfo.Text="Warning: info.xml is missing from this fomod.";

            if(mod.HasScript) bEditScript.Text="Edit script";
            else bEditScript.Text="Create script";

            pictureBox1.Image=mod.GetScreenshot();
        }

        private void lvModList_ItemChecked(object sender, ItemCheckedEventArgs e) {
            if(e.Item.Checked) ((fomod)e.Item.Tag).Activate();
            else ((fomod)e.Item.Tag).Deactivate();
        }

        private void lvModList_ItemCheck(object sender, ItemCheckEventArgs e) {
            if(e.NewValue==CheckState.Checked) {

            } else {

            }
        }

        private void PackageManager_Shown(object sender, EventArgs e) {
            lvModList.ItemCheck+=new ItemCheckEventHandler(lvModList_ItemCheck);
            lvModList.ItemChecked+=new ItemCheckedEventHandler(lvModList_ItemChecked);
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
    }
}