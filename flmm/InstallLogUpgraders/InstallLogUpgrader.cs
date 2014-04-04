using System;
using System.Collections.Generic;
using System.Xml;
using Fomm.PackageManager.ModInstallLog;

namespace Fomm.InstallLogUpgraders
{
  /// <summary>
  ///   Reverts the InstallLog to 2.0.0.0.
  /// </summary>
  internal class InstallLogUpgrader
  {
    private Dictionary<Version, Upgrader> m_dicUpgraders;

    /// <summary>
    ///   The default constructor.
    /// </summary>
    internal InstallLogUpgrader()
    {
      m_dicUpgraders = new Dictionary<Version, Upgrader>();
      m_dicUpgraders[new Version("0.0.0.0")] = new Upgrader0000();
      m_dicUpgraders[new Version("0.1.0.0")] = new Upgrader0100();
      m_dicUpgraders[new Version("0.1.1.0")] = new Upgrader0110();
      m_dicUpgraders[new Version("0.5.0.0")] = new Downgrader0500();
    }

    /// <summary>
    ///   Upgrades the install log.
    /// </summary>
    /// <remarks>
    ///   This creates a <see cref="BackgroundWorkerProgressDialog" /> to do the work
    ///   and display progress.
    /// </remarks>
    /// <returns><lang langref="false" /> if the user cancelled the upgrade; <lang langref="true" /> otherwise.</returns>
    public bool UpgradeInstallLog()
    {
      //this is to handle the few people who already installed a version that used
      // the new-style install log, but before it had a version
      if (InstallLog.Current.GetInstallLogVersion().ToString().Equals("0.0.0.0"))
      {
        var xmlOldInstallLog = new XmlDocument();
        xmlOldInstallLog.Load(InstallLog.Current.InstallLogPath);
        if (xmlOldInstallLog.SelectNodes("descendant::installingMods").Count > 0)
        {
          InstallLog.Current.SetInstallLogVersion(new Version("0.1.0.0"));
          InstallLog.Current.Save();
        }
      }

      var verOldVersion = InstallLog.Current.GetInstallLogVersion();
      if (verOldVersion == InstallLog.CURRENT_VERSION)
      {
        return true;
      }

      //we only want one upgrade at a time happening to minimize the chances of
      // messed up install logs.
      bool booUpgraded;
      //lock (m_objLock)
      {
        InstallLog.Current.EnableLogFileRefresh = false;
        if (!m_dicUpgraders.ContainsKey(verOldVersion))
        {
          throw new InvalidOperationException("No upgrade or downgrade available for Install Log Version " +
                                              verOldVersion + ".");
        }

        booUpgraded = m_dicUpgraders[verOldVersion].PerformUpgrade();
        InstallLog.Current.EnableLogFileRefresh = true;
      }
      return booUpgraded;
    }
  }
}