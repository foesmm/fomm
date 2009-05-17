using System;
using System.Windows.Forms;
using File=System.IO.File;

namespace Fomm.InstallTweaker {
    public partial class xliveSettings : Form {
        public xliveSettings() {
            InitializeComponent();
            this.Icon=Fomm.Properties.Resources.fomm02;
            if(!File.Exists("xlive.ini")) {
                File.WriteAllLines("xlive.ini", new string[] { "[d3dx]", "sse=0", "", "[xlive]", "profile=" });
            }
            switch(NativeMethods.GetPrivateProfileIntA("d3dx", "sse", 0, ".\\xlive.ini")) {
            case 2:
                rbSse2.Checked=true;
                break;
            case 3:
                rbSse3.Checked=true;
                break;
            case 4:
                rbSse4.Checked=true;
                break;
            default:
                rbSse0.Checked=true;
                break;
            }
            tbProfile.Text=NativeMethods.GetPrivateProfileString("xlive", "profile", "", ".\\xlive.ini");
        }

        private void bSseHelp_Click(object sender, EventArgs e) {
            MessageBox.Show("If set to something other than 'off', the fake xlive dll will patch several pieces of fallouts code with faster versions\n"+
                "Setting this to an sse instruction set unsupported by your processor will cause fallout to crash\n"+
                "This doesn't make any permenent changes; the improved functions will only be in effect while the fake xlive dll is in place\n"+
                "Requires a supported version of fallout.exe to work. (Currently only 1.4.0.6)\n"+
                "This setting will be ignored if the fallout exe is not supported",
                "Help");
        }

        private void bProfileHelp_Click(object sender, EventArgs e) {
            MessageBox.Show("If you want to use an offline xlive profile, enter its name here\n"+
                "Leave blank if you don't want to use a profile", "Help");
        }

        private void xliveSettings_FormClosing(object sender, FormClosingEventArgs e) {
            int sse;
            if(rbSse4.Checked) sse=4;
            else if(rbSse3.Checked) sse=3;
            else if(rbSse2.Checked) sse=2;
            else sse=0;
            NativeMethods.WritePrivateProfileIntA("d3dx", "sse", sse, ".\\xlive.ini");
            NativeMethods.WritePrivateProfileStringA("xlive", "profile", tbProfile.Text, ".\\xlive.ini");
        }

    }
}