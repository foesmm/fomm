using System;
using System.Windows.Forms;

namespace fomm.PackageManager {
    enum OverwriteResult { YesToAll=1, YesToFolder=2, Yes=3, NoToAll=4, NoToFolder=5, No=6 }

    partial class Overwriteform : Form {
        private Overwriteform(string msg, bool allowFolder) {
            InitializeComponent();
            Settings.GetWindowPosition("OverwriteForm", this);
            label1.Text=msg;
            if(!allowFolder) {
                bYesToFolder.Enabled=false;
                bNoToFolder.Enabled=false;
            }
        }

        private OverwriteResult result;

        public static OverwriteResult ShowDialog(string msg, bool allowFolder) {
            Overwriteform of=new Overwriteform(msg, allowFolder);
            of.ShowDialog();
            return of.result;
        }

        private void bYesToAll_Click(object sender, EventArgs e) {
            result=OverwriteResult.YesToAll;
            Close();
        }

        private void bYesToFolder_Click(object sender, EventArgs e) {
            result=OverwriteResult.YesToFolder;
            Close();
        }

        private void bYes_Click(object sender, EventArgs e) {
            result=OverwriteResult.Yes;
            Close();
        }

        private void bNoToAll_Click(object sender, EventArgs e) {
            result=OverwriteResult.NoToAll;
            Close();
        }

        private void bNoToFolder_Click(object sender, EventArgs e) {
            result=OverwriteResult.NoToFolder;
            Close();
        }

        private void bNo_Click(object sender, EventArgs e) {
            result=OverwriteResult.No;
            Close();
        }

        private void Overwriteform_FormClosing(object sender, FormClosingEventArgs e) {
            Settings.SetWindowPosition("OverwriteForm", this);
        }
    }
}