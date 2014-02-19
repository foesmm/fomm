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
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (UpdateHelper.IsFommInstalled) {
				if (UpdateHelper.HasWriteAccessToFolder(AppDomain.CurrentDomain.BaseDirectory)) {
					Application.Run(new UpdateForm());
				} else if (!UpdateHelper.IsProcessElevated) {
					UpdateHelper.RunElevated();
				} else {
					string strMessage = String.Format("Must have write access to directory: {0}", AppDomain.CurrentDomain.BaseDirectory);
					MessageBox.Show(strMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			} else {
				if (!UpdateHelper.IsProcessElevated) {
					UpdateHelper.RunElevated();
				} else {
					Application.Run(new InstallForm());
				}
			}
		}
		
	}
}
