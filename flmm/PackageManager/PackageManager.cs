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
                lvModList.Items.Add(lvi);
            }
        }

        private void lvModList_SelectedIndexChanged(object sender, EventArgs e) {
            if(lvModList.SelectedItems.Count==0) return;
            tbModInfo.Text=((fomod)lvModList.SelectedItems[0].Tag).Description;
        }

        private void lvModList_ItemChecked(object sender, ItemCheckedEventArgs e) {

        }
    }
}