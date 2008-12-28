using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace fomm.PackageManager {
    partial class InfoEditor : Form {
        private readonly fomod mod;
        private readonly Regex email=new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.Singleline|RegexOptions.CultureInvariant);

        public InfoEditor(fomod mod) {
            this.mod=mod;
            InitializeComponent();
            Settings.GetWindowPosition("InfoEditor", this);
            tbName.Text=mod.Name;
            tbAuthor.Text=mod.Author;
            tbVersion.Text=mod.VersionS;
            tbMVersion.Text=mod.Version.ToString();
            tbDescription.Text=mod.Description;
            tbWebsite.Text=mod.website;
            tbEmail.Text=mod.email;
            if(mod.MinFommVersion==new Version(0, 0, 0, 0)) tbMinFommVersion.Text="";
            else tbMinFommVersion.Text=mod.MinFommVersion.ToString();

            string[] groups=Settings.GetStringArray("fomodGroups");
            clbGroups.SuspendLayout();
            for(int i=0;i<groups.Length;i++) {
                clbGroups.Items.Add(groups[i], Array.IndexOf<string>(mod.groups, groups[i].ToLowerInvariant())!=-1);
            }
            clbGroups.ResumeLayout();
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

            if(tbEmail.Text!=""&&!email.IsMatch(tbEmail.Text)) {
                MessageBox.Show("Invalid email address specified");
                return;
            }
            if(tbWebsite.Text!="") {
                Uri uri;
                if(!Uri.TryCreate(tbWebsite.Text, UriKind.Absolute, out uri)||uri.IsFile||(uri.Scheme!="http"&&uri.Scheme!="https")) {
                    MessageBox.Show("Invalid web address specified.\nDid you miss the 'http://'?)");
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
            mod.website=tbWebsite.Text;
            mod.email=tbEmail.Text;

            mod.groups=new string[clbGroups.CheckedItems.Count];
            for(int i=0;i<mod.groups.Length;i++) mod.groups[i]=((string)clbGroups.CheckedItems[i]).ToLowerInvariant();

            mod.CommitInfo(setScreenshot, screenshot);
            DialogResult=DialogResult.OK;
            Close();
        }

        private void InfoEditor_FormClosing(object sender, FormClosingEventArgs e) {
            Settings.SetWindowPosition("InfoEditor", this);
        }

        private void clbGroups_ItemCheck(object sender, ItemCheckEventArgs e) {

        }
    }
}