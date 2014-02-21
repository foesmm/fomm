
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace Fomm.Updater
{
    /// <summary>
    /// Description of InstallForm.
    /// </summary>
    public partial class InstallForm : Form
    {
        private bool bLegacyBoxShown = false;
        private bool bInstalled = false;
        
        public InstallForm()
        {
            InitializeComponent();
            
            Text = String.Format("{0} v{1} Installation", Fomm.ProductInfo.FullName, Fomm.ProductInfo.Version);
        }
        
        void InstallFormShown(object sender, EventArgs e)
        {
            if (!bLegacyBoxShown && UpdateHelper.IsLegacyFommInstalled)
            {
                // @fixme temporary disabled
                //string strLegacyFommUninstall = String.Format("{0}\nRun uninstall program?", "");
                //if (DialogResult.Yes == MessageBox.Show(strLegacyFommUninstall, "Legacy FOMM is installed", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)) {
                //    foreach(string id in UpdateHelper.LegacyIDs) {
                //        UninstallInfo uiLegacy = new UninstallInfo(id);
                //        if (uiLegacy.IsValid) {
                //            Process uninstall = new Process();
                //            uninstall.StartInfo.FileName = uiLegacy.UninstallString;
                //            //uninstall.Start();
                //            //uninstall.WaitForExit();
                //            Debug.WriteLine(uninstall.ExitCode.ToString());
                //        }
                //    }
                //  UpdateHelper.UninstallLegacyFomm();
            }
        }
    }
}
