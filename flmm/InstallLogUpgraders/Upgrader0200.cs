using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Fomm.PackageManager;
using Fomm.PackageManager.ModInstallLog;

namespace Fomm.InstallLogUpgraders
{
  /// <summary>
  /// Upgrades the Install Log to the current version from version 0.2.0.0.
  /// </summary>
  internal class Upgrader0200 : Upgrader
  {
    protected override void DoUpgrade()
    {
      fomod fomodMod = null;
      XmlDocument xmlInstallLog;
      XmlNode decNode;
      XmlNode xndData;
      IList<string> lstMods;

      // Load the install log
      xmlInstallLog = new XmlDocument();
      xmlInstallLog.Load(InstallLog.Current.InstallLogPath);

      // Add a declaration if there isn't one.
      if (xmlInstallLog.FirstChild.NodeType != XmlNodeType.XmlDeclaration)
      {
        decNode = xmlInstallLog.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlInstallLog.PrependChild(decNode);
      }

      // Fixup datafiles paths
      xndData = xmlInstallLog.SelectSingleNode("descendant::dataFiles");
      foreach (XmlNode tmpNode in xndData.SelectNodes("descendant::file"))
      {
        tmpNode.Attributes["path"].InnerText = Path.Combine("Data", tmpNode.Attributes["path"].InnerText);
      }

      // Save the document with declaration
      xmlInstallLog.Save(InstallLog.Current.InstallLogPath);
      InstallLog.Reload();
      
      lstMods = InstallLog.Current.GetModList();
      
      ProgressWorker.OverallProgressStep = 1;
      ProgressWorker.OverallProgressMaximum = lstMods.Count;
      ProgressWorker.ShowItemProgress = false;

      // now update the mod info
      foreach (string strMod in lstMods)
      {
        ProgressWorker.StepOverallProgress();

        string strModPath = Path.Combine(Program.GameMode.ModDirectory, strMod + ".fomod");
        if (File.Exists(strModPath))
        {
          fomodMod = new fomod(strModPath);
          InstallLog.Current.UpdateMod(fomodMod);
        }
        else
        {
          InstallLog.Current.UpdateMod(strMod);
        }
      }

      // Update the installlog version and save
      InstallLog.Current.SetInstallLogVersion(InstallLog.CURRENT_VERSION);
      InstallLog.Current.Save();
    }
  }
}
