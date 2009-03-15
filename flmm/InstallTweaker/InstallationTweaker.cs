using System;
using System.Windows.Forms;
using System.IO;

namespace Fomm.InstallTweaker {
    delegate void ReportProgressDelegate(string msg);

    partial class InstallationTweaker : Form {
        private static readonly string BackupPath=Path.Combine(Program.exeDir, "fomm\\itBackup\\");
        private static readonly string esmBackup=Path.Combine(BackupPath, "fallout3.esm");
        private static readonly string bsaBackup=Path.Combine(BackupPath, "Fallout - Textures.bsa");
        private static readonly string xlivePath=Path.Combine(Program.exeDir, "fomm\\xlive.dll");

        private readonly bool Initing=true;

        public InstallationTweaker() {
            InitializeComponent();
            if(Directory.Exists(BackupPath)) {
                if(File.Exists("xlive.dll")) {
                    cbDisableLive.Checked=true;
                    bXliveSettings.Enabled=true;
                }
                if(File.Exists(bsaBackup)) cbShrinkTextures.Checked=true;
                bApply.Enabled=false;
            } else bReset.Enabled=false;
            Initing=false;
        }

        /*private void cbDisableLive_CheckedChanged(object sender, EventArgs e) {
            if(backgroundWorker1.IsBusy||Initing) return;
        }

        private void cbShrinkTextures_CheckedChanged(object sender, EventArgs e) {
            if(backgroundWorker1.IsBusy||Initing) return;
            
        }

        private void cbRemoveClutter_CheckedChanged(object sender, EventArgs e) {
            if(backgroundWorker1.IsBusy) return;
            tbDescription.Text="Removes all references to some types of useless clutter from fallout3.esm"+Environment.NewLine+
                "Improves loading times and fps in any affected cells";
        }

        private void cbStripGeck_CheckedChanged(object sender, EventArgs e) {
            if(backgroundWorker1.IsBusy) return;
            tbDescription.Text="Strips some data out of fallout3.esm that is only used by the geck"+Environment.NewLine+
                "Slightly improves startup time and loading times"+Environment.NewLine+
                "Do not check this option if you plan to use the geck or other tools to create your own mods"+Environment.NewLine+
                "Does not effect your use of any third party mods";
        }*/

        private void bCancel_Click(object sender, EventArgs e) {
            Close();
        }

        private void bApply_Click(object sender, EventArgs e) {
            if(cbDisableLive.Checked) bXliveSettings.Enabled=true;
            WorkerArgs args=new WorkerArgs();
            args.xlive=cbDisableLive.Checked;
            //args.stripedids=cbStripGeck.Checked;
            //args.striprefs=cbRemoveClutter.Checked;
            args.trimbsa=cbShrinkTextures.Checked;
            args.hwnd=this.Handle;
            tbDescription.Text="";
            bApply.Enabled=false;
            bReset.Enabled=true;
            lines=new string[70];
            for(int i=0;i<70;i++) lines[i]=string.Empty;
            LineCount=0;
            backgroundWorker1.RunWorkerAsync(args);
        }

        private void bReset_Click(object sender, EventArgs e) {
            if(backgroundWorker1.IsBusy) {
                MessageBox.Show("Cannot reset while the tweaker is still running.", "Error");
                return;
            }
            File.Delete("xlive.dll");
            File.Delete("xlive.ini");
            if(File.Exists(esmBackup)) {
                File.Delete("data\\fallout3.esm");
                File.Move(esmBackup, "data\\fallout3.esm");
            }
            if(File.Exists(bsaBackup)) {
                File.Delete("data\\Fallout - Textures.bsa");
                File.Move(bsaBackup, "data\\Fallout - Textures.bsa");
            }
            Directory.Delete(BackupPath, true);
            bReset.Enabled=false;
            bApply.Enabled=true;
            bXliveSettings.Enabled=false;
        }

        private struct WorkerArgs {
            public bool xlive;
            //public bool stripedids;
            //public bool striprefs;
            public bool trimbsa;
            public IntPtr hwnd;
        }

        private void ReportProgress(string msg) {
            backgroundWorker1.ReportProgress(0, msg);
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
            WorkerArgs args=(WorkerArgs)e.Argument;
            Directory.CreateDirectory(BackupPath);
            if(cbDisableLive.Checked) {
                backgroundWorker1.ReportProgress(0, "Copying fake xlive.dll");
                if(File.Exists("xlive.dll")) File.Delete("xlive.dll"); //In case people are using Quarn's mod
                File.Copy(xlivePath, "xlive.dll");
            }
            /*if(cbRemoveClutter.Checked||cbStripGeck.Checked) {
                backgroundWorker1.ReportProgress(0, "Parsing fallout3.esm");
                File.Move("data\\fallout3.esm", esmBackup);
                EsmTrimmer.Trim(cbStripGeck.Checked, cbRemoveClutter.Checked, esmBackup, "data\\fallout3.esm", ReportProgress);
            }*/
            if(cbShrinkTextures.Checked) {
                backgroundWorker1.ReportProgress(0, "Parsing Fallout - Textures.bsa");
                File.Move("data\\Fallout - Textures.bsa", bsaBackup);
                BsaTrimmer.Trim(args.hwnd, bsaBackup, "data\\Fallout - Textures.bsa", ReportProgress);
            }
            backgroundWorker1.ReportProgress(0, "Complete");
        }

        private int LineCount;
        private string[] lines;
        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e) {
            if(LineCount<70) lines[LineCount++]=(string)e.UserState;
            else {
                for(int i=0;i<69;i++) lines[i]=lines[i+1];
                lines[69]=(string)e.UserState;
            }
            tbDescription.Lines=lines;
            tbDescription.Select(tbDescription.TextLength-(70-LineCount)*Environment.NewLine.Length, 0);
            tbDescription.ScrollToCaret();
        }

        private void InstallationTweaker_FormClosing(object sender, FormClosingEventArgs e) {
            if(backgroundWorker1.IsBusy) {
                MessageBox.Show("Wait until tweaker has finished running before closing the form", "Error");
                e.Cancel=true;
            }
        }

        private void bXliveSettings_Click(object sender, EventArgs e) {
            (new xliveSettings()).ShowDialog();
        }

        private void cbDisableLive_MouseEnter(object sender, EventArgs e) {
            tbDescription.Text="Installs a fake version of the games for windows live dll"+Environment.NewLine+"Prevents fallout from loading the real xlive.dll at all, and enables some extra code patching options that require disabling g4wl's hash checking"+Environment.NewLine+
                "Improves program startup time, and possibly fps"+Environment.NewLine+"Do not use if you use DLC or want achievements"+Environment.NewLine+
                "Additional code patching options can be accessed by clicking 'settings'"+Environment.NewLine+
                "The save games associated with g4wl profiles can still be accessed by clicking settings and using an offline profile";
        }

        private void cbShrinkTextures_MouseEnter(object sender, EventArgs e) {
            tbDescription.Text="Repacks the textures bsa after stripping the top mipmap from all non-interface textures"+Environment.NewLine+
                "Improves loading times"+Environment.NewLine+"Do not use if you normally have texture size set to large"+Environment.NewLine+
                "After checking this, change textures to medium if you normally use small or large if you normally use medium to keep the same visual quality.";
        }
    }
}
