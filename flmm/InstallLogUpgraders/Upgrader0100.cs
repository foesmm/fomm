using System;
using Fomm.PackageManager;
using System.Collections.Generic;
using System.IO;

namespace Fomm.InstallLogUpgraders
{
	/// <summary>
	/// Upgrades the Install Log to the current version from version 0.1.0.0.
	/// </summary>
	internal class Upgrader0100 : Upgrader
	{
		/// <summary>
		/// Upgrades the Install Log to the current version from version 0.1.0.0.
		/// </summary>
		/// <remarks>
		/// This method is called by a background worker to perform the actual upgrade.
		/// </remarks>
		protected override void DoUpgrade()
		{
			IList<string> lstMods = InstallLog.Current.GetModList();
			ProgressWorker.OverallProgressStep = 1;
			ProgressWorker.OverallProgressMaximum = lstMods.Count;
			ProgressWorker.ShowItemProgress = false;

			foreach (string strMod in lstMods)
			{
				ProgressWorker.StepOverallProgress();
				if (strMod.Equals(InstallLog.ORIGINAL_VALUES) || strMod.Equals(InstallLog.FOMM))
					continue;
				string strModPath = Path.Combine(Program.PackageDir, strMod + ".fomod");
				fomod fomodMod = new fomod(strModPath);
				InstallLog.Current.UpdateMod(fomodMod);
			}
			InstallLog.Current.SetInstallLogVersion(InstallLog.CURRENT_VERSION);
			InstallLog.Current.Save();
		}
	}
}
