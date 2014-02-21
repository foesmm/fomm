using System;
using System.Windows.Forms;

namespace Fomm.Updater
{
    /// <summary>
    /// Class with program entry point.
    /// </summary>
    internal sealed class Program
    {
        /// <summary>
        /// Program entry point.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (UpdateHelper.FommUninstallInfo != null)
                {
                    if (UpdateHelper.HasWriteAccessToFolder(UpdateHelper.FommUninstallInfo.InstallLocation))
                    {
                        Application.Run(new UpdateForm(/*UpdateHelper.FommUninstallInfo*/));
                    }
                    else if (!UpdateHelper.IsProcessElevated)
                    {
                        UpdateHelper.RunElevated();
                    }
                    else
                    {
                        string strMessage = String.Format("Must have write access to directory: {0}", UpdateHelper.FommUninstallInfo.InstallLocation);
                        MessageBox.Show(strMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    if (!UpdateHelper.IsProcessElevated)
                    {
                        UpdateHelper.RunElevated();
                    }
                    else
                    {
                        Application.Run(new InstallForm());
                    }
                }
            }
            else
            {
                return; // @fixme temporary disabled
                switch (args[0])
                {
                    case "--update-associations":
                        MessageBox.Show(String.Join(" ", args));
                        if (!UpdateHelper.IsProcessElevated)
                        {
                            UpdateHelper.RunElevated(args);
                        }
                        else
                        {
                            string[] assoc = new string[args.Length - 1];
                            Array.Copy(args, 1, assoc, 0, args.Length - 1);
                            UpdateHelper.Associate(assoc);
                        }
                    break;
                }
            }
        }
    }
}
