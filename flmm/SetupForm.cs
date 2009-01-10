using System;
using System.Windows.Forms;

namespace fomm {
    public partial class SetupForm : Form {
        public SetupForm() {
            InitializeComponent();
            string tmp=Settings.GetString("FomodDir");
            if(tmp!=null) {
                cbFomod.Checked=true;
                tbFomod.Text=tmp;
            }
            tmp=Settings.GetString("FalloutDir");
            if(tmp!=null) {
                cbFallout.Checked=true;
                tbFallout.Text=tmp;
            }
            tmp=Settings.GetString("LaunchCommand");
            if(tmp!=null) {
                cbLaunch.Checked=true;
                tbLaunch.Text=tmp;
            }
        }

        private void SetupForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(cbFomod.Checked) Settings.SetString("FomodDir", tbFomod.Text);
            else Settings.RemoveString("FomodDir");
            if(cbFallout.Checked) Settings.SetString("FalloutDir", tbFallout.Text);
            else Settings.RemoveString("FalloutDir");
            if(cbLaunch.Checked) Settings.SetString("LaunchCommand", tbLaunch.Text);
            else Settings.RemoveString("LaunchCommand");
        }

        private void cbFomod_CheckedChanged(object sender, EventArgs e) {
            tbFomod.ReadOnly=!cbFomod.Checked;
            bBrowseFomod.Enabled=cbFomod.Checked;
        }

        private void cbFallout_CheckedChanged(object sender, EventArgs e) {
            tbFallout.ReadOnly=!cbFallout.Checked;
            bBrowseFallout.Enabled=cbFallout.Checked;
        }

        private void cbLaunch_CheckedChanged(object sender, EventArgs e) {
            tbLaunch.ReadOnly=!cbLaunch.Checked;
        }

        private void bBrowseFomod_Click(object sender, EventArgs e) {
            if(folderBrowserDialog1.ShowDialog()!=DialogResult.OK) return;
            tbFomod.Text=folderBrowserDialog1.SelectedPath;
        }

        private void bBrowseFallout_Click(object sender, EventArgs e) {
            if(folderBrowserDialog1.ShowDialog()!=DialogResult.OK) return;
            tbFallout.Text=folderBrowserDialog1.SelectedPath;
        }
    }
}