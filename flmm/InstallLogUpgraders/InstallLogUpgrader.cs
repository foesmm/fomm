using System;
using System.IO;
using System.Xml;
using ICSharpCode.SharpZipLib.Checksums;
using ChinhDo.Transactions;
using System.Collections.Generic;
using System.Windows.Forms;
using fomm.Transactions;
using System.ComponentModel;
using Fomm.PackageManager;

namespace Fomm.InstallLogUpgraders
{
	/// <summary>
	/// Upgrades the install log.
	/// </summary>
	internal class InstallLogUpgrader
	{
		private Dictionary<Version, Upgrader> m_dicUpgraders = null;

		/// <summary>
		/// The default constructor.
		/// </summary>
		internal InstallLogUpgrader()
		{
			m_dicUpgraders = new Dictionary<Version, Upgrader>();
			m_dicUpgraders[new Version("0.0.0.0")] = new Upgrader0000();
			m_dicUpgraders[new Version("0.1.0.0")] = new Upgrader0100();
		}

		/// <summary>
		/// Upgrades the install log.
		/// </summary>
		/// <remarks>
		/// This creates a <see cref="BackgroundWorkerProgressDialog"/> to do the work
		/// and display progress.
		/// </remarks>
		/// <returns><lang cref="false"/> if the user cancelled the upgrade; <lang cref="true"/> otherwise.</returns>
		public bool UpgradeInstallLog()
		{
			//this is to handle the few people who already installed a version that used
			// the new-style install log, but before it had a version
			if (InstallLog.Current.GetInstallLogVersion().ToString().Equals("0.0.0.0"))
			{
				XmlDocument xmlOldInstallLog = new XmlDocument();
				xmlOldInstallLog.Load(InstallLog.Current.InstallLogPath);
				if (xmlOldInstallLog.SelectNodes("descendant::installingMods").Count > 0)
				{
					InstallLog.Current.SetInstallLogVersion(new Version("0.1.0.0"));
					InstallLog.Current.Save();
				}
			}

			Version verOldVersion = InstallLog.Current.GetInstallLogVersion();
			if (verOldVersion == InstallLog.CURRENT_VERSION)
				return true;
		
			//we only want one upgrade at a time happening to minimize the chances of
			// messed up install logs.
			bool booUpgraded = false;
			//lock (m_objLock)
			{
				InstallLog.Current.EnableLogFileRefresh = false;
				if (!m_dicUpgraders.ContainsKey(verOldVersion))
					throw new InvalidOperationException("No upgrader for Install Log Version " + verOldVersion + ".");

				booUpgraded = m_dicUpgraders[verOldVersion].PerformUpgrade();
				InstallLog.Current.EnableLogFileRefresh = true;
			}
			return booUpgraded;
		}
	}
}
