using System;
using System.Windows.Forms;

namespace fomm.PackageManager {
    partial class InfoEditor : Form {
        private readonly fomod mod;
        public InfoEditor(fomod mod) {
            this.mod=mod;
            InitializeComponent();
            tbName.Text=mod.Name;
            tbAuthor.Text=mod.Author;
            tbVersion.Text=mod.VersionS;
            tbMVersion.Text=mod.Version.ToString();
            tbDescription.Text=mod.Description;
            if(mod.MinFommVersion==new Version(0, 0, 0, 0)) tbMinFommVersion.Text="";
            else tbMinFommVersion.Text=mod.MinFommVersion.ToString();
        }

        private bool setScreenshot;
        private byte[] screenshot;


        private void bClearScreenshot_Click(object sender, EventArgs e) {
            setScreenshot=true;
            screenshot=null;
        }

        private void bScreenshot_Click(object sender, EventArgs e) {
            if(openFileDialog1.ShowDialog()!=DialogResult.OK) return;
            setScreenshot=true;
            screenshot=System.IO.File.ReadAllBytes(openFileDialog1.FileName);
        }

        private void bCancel_Click(object sender, EventArgs e) {
            Close();
        }

        private void bSave_Click(object sender, EventArgs e) {
            Version version=fomod.DefaultVersion;
            Version minFommVersion=fomod.DefaultMinFommVersion;
            if(tbVersion.Text!="") {
                try {
                    version=new Version(tbVersion.Text);
                } catch {
                    MessageBox.Show("Invalid version specified", "Error");
                    return;
                }
            }
            if(tbMinFommVersion.Text!="") {
                try {
                    minFommVersion=new Version(tbMinFommVersion.Text);
                } catch {
                    MessageBox.Show("Invalid minimum fomm version specified", "Error");
                    return;
                }
            }

            if(minFommVersion>Program.MVersion) {
                MessageBox.Show("Specified minimum fomm version is newer than this version of fomm", "Error");
                return;
            }

            mod.Name=tbName.Text;
            mod.Author=tbAuthor.Text;
            mod.VersionS=tbVersion.Text;
            mod.Description=tbDescription.Text;

            mod.CommitInfo(setScreenshot, screenshot);
            DialogResult=DialogResult.OK;
            Close();
        }
    }
}