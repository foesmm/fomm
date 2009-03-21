using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Fomm {
    partial class SetupForm : Form {
        private bool FinishedSetup;
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
            cbEsmShow.Checked=Settings.GetBool("ShowEsmInBold");
            string key=Registry.GetValue(@"HKEY_CLASSES_ROOT\.bsa", null, null) as string;
            switch(key) {
            case "BethesdaSoftworks_Archive":
                cbAssociateBsa.Checked=true;
                break;
            case null:
                break;
            default:
                cbAssociateBsa.Enabled=false;
                break;
            }
            key=Registry.GetValue(@"HKEY_CLASSES_ROOT\.sdp", null, null) as string;
            switch(key) {
            case "BethesdaSoftworks_ShaderPackage":
                cbAssociateSdp.Checked=true;
                break;
            case null:
                break;
            default:
                cbAssociateSdp.Enabled=false;
                break;
            }
            key=Registry.GetValue(@"HKEY_CLASSES_ROOT\.fomod", null, null) as string;
            switch(key) {
            case "FOMM_Mod_Archive":
                cbAssociateFomod.Checked=true;
                break;
            case null:
                break;
            default:
                cbAssociateFomod.Enabled=false;
                break;
            }
            key=Registry.GetValue(@"HKEY_CLASSES_ROOT\.zip", null, null) as string;
            if(key==null) cbShellExtensions.Enabled=false;
            else {
                if(Registry.GetValue("HKEY_CLASSES_ROOT\\"+key+"\\Shell\\Convert_to_fomod\\command", null, null)!=null) cbShellExtensions.Checked=true;
            }

            FinishedSetup=true;
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

        private void cbAssociateFomod_CheckedChanged(object sender, EventArgs e) {
            if(!FinishedSetup) return;
            if(!cbAssociateFomod.Checked) {
                Registry.ClassesRoot.DeleteSubKeyTree("FOMM_Mod_Archive");
                Registry.ClassesRoot.DeleteSubKeyTree(".fomod");
            } else {
                Registry.SetValue(@"HKEY_CLASSES_ROOT\.fomod", null, "FOMM_Mod_Archive");
                Registry.SetValue(@"HKEY_CLASSES_ROOT\FOMM_Mod_Archive", null, "Fallout Mod Manager Archive", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\FOMM_Mod_Archive\DefaultIcon", null, Application.ExecutablePath+",0", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\FOMM_Mod_Archive\shell\open\command", null, "\""+Application.ExecutablePath+"\" \"%1\"", RegistryValueKind.String);
            }
        }

        private void cbAssociateBsa_CheckedChanged(object sender, EventArgs e) {
            if(!FinishedSetup) return;
            if(!cbAssociateBsa.Checked) {
                Registry.ClassesRoot.DeleteSubKeyTree("BethesdaSoftworks_Archive");
                Registry.ClassesRoot.DeleteSubKeyTree(".bsa");
            } else {
                Registry.SetValue(@"HKEY_CLASSES_ROOT\.bsa", null, "BethesdaSoftworks_Archive");
                Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_Archive", null, "Bethesda File Archive", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_Archive\DefaultIcon", null, Application.ExecutablePath+",0", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_Archive\shell\open\command", null, "\""+Application.ExecutablePath+"\" \"%1\"", RegistryValueKind.String);
            }
        }

        private void cbAssociateSdp_CheckedChanged(object sender, EventArgs e) {
            if(!FinishedSetup) return;
            if(!cbAssociateSdp.Checked) {
                Registry.ClassesRoot.DeleteSubKeyTree("BethesdaSoftworks_ShaderPackage");
                Registry.ClassesRoot.DeleteSubKeyTree(".sdp");
            } else {
                Registry.SetValue(@"HKEY_CLASSES_ROOT\.sdp", null, "BethesdaSoftworks_ShaderPackage");
                Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_ShaderPackage", null, "Bethesda Shader Package", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_ShaderPackage\DefaultIcon", null, Application.ExecutablePath+",0", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_ShaderPackage\shell\open\command", null, "\""+Application.ExecutablePath+"\" \"%1\"", RegistryValueKind.String);
            }
        }

        private static void AddShellExtension(string key) {
            if(key==null) return;
            Registry.SetValue("HKEY_CLASSES_ROOT\\"+key+"\\Shell\\Convert_to_fomod", null, "Convert to fomod");
            Registry.SetValue("HKEY_CLASSES_ROOT\\"+key+"\\Shell\\Convert_to_fomod\\command", null, "\""+Application.ExecutablePath+"\" \"%1\"", RegistryValueKind.String);
        }
        private static void RemoveShellExtension(string key) {
            if(key==null) return;
            RegistryKey rk=Registry.ClassesRoot.OpenSubKey(key+"\\Shell", true);
            if(Array.IndexOf<string>(rk.GetSubKeyNames(), "Convert_to_fomod")!=-1) rk.DeleteSubKeyTree("Convert_to_fomod");
            rk.Close();
        }
        private void cbShellExtensions_CheckedChanged(object sender, EventArgs e) {
            if(!FinishedSetup) return;
            if(cbShellExtensions.Checked) {
                AddShellExtension(Registry.GetValue(@"HKEY_CLASSES_ROOT\.zip", null, null) as string);
                AddShellExtension(Registry.GetValue(@"HKEY_CLASSES_ROOT\.rar", null, null) as string);
                AddShellExtension(Registry.GetValue(@"HKEY_CLASSES_ROOT\.7z", null, null) as string);
            } else {
                RemoveShellExtension(Registry.GetValue(@"HKEY_CLASSES_ROOT\.zip", null, null) as string);
                RemoveShellExtension(Registry.GetValue(@"HKEY_CLASSES_ROOT\.rar", null, null) as string);
                RemoveShellExtension(Registry.GetValue(@"HKEY_CLASSES_ROOT\.7z", null, null) as string);
            }
        }

        private void bEsmShow_CheckedChanged(object sender, EventArgs e) {
            Settings.SetBool("ShowEsmInBold", cbEsmShow.Checked);
        }
    }
}