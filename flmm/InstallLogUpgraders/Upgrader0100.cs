using System;
using System.Collections.Generic;
using System.IO;
using Fomm.PackageManager.ModInstallLog;
using Fomm.PackageManager;
using System.Xml;

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
      var xmlInstallLog = new XmlDocument();
      xmlInstallLog.LoadXml(InstallLog.Current.InstallLogPath);

      var xndRoot = xmlInstallLog.SelectSingleNode("installLog");
      var xndSdpEdits = xndRoot.SelectSingleNode("sdpEdits");
      var lstMods = InstallLog.Current.GetModList();

      ProgressWorker.OverallProgressStep = 1;
      ProgressWorker.OverallProgressMaximum = lstMods.Count + xndSdpEdits.ChildNodes.Count;
      ProgressWorker.ShowItemProgress = false;

      //remove the sdp edit node...
      xndSdpEdits.ParentNode.RemoveChild(xndSdpEdits);
      //...and replace it with the game-specific edits node
      var xndGameSpecificsValueEdits = xndRoot.AppendChild(xmlInstallLog.CreateElement("gameSpecificEdits"));
      foreach (XmlNode xndSdpEdit in xndSdpEdits.ChildNodes)
      {
        ProgressWorker.StepOverallProgress();
        var xndGameSpecificsValueEdit = xndGameSpecificsValueEdits.AppendChild(xmlInstallLog.CreateElement("edit"));
        var strValueKey = String.Format("sdp:{0}/{1}", xndGameSpecificsValueEdits.Attributes["package"].Value,
                                           xndGameSpecificsValueEdits.Attributes["shader"].Value);
        xndGameSpecificsValueEdit.Attributes.Append(xmlInstallLog.CreateAttribute("key")).Value = strValueKey;
        xndGameSpecificsValueEdit.AppendChild(xndSdpEdit.FirstChild.Clone());
      }
      xmlInstallLog.Save(InstallLog.Current.InstallLogPath);

      //now update the mod info
      foreach (var strMod in lstMods)
      {
        ProgressWorker.StepOverallProgress();
        if (strMod.Equals(InstallLog.ORIGINAL_VALUES) || strMod.Equals(InstallLog.FOMM))
        {
          continue;
        }
        var strModPath = Path.Combine(Program.GameMode.ModDirectory, strMod + ".fomod");
        var fomodMod = new fomod(strModPath);
        InstallLog.Current.UpdateMod(fomodMod);
      }

      InstallLog.Current.SetInstallLogVersion(InstallLog.CURRENT_VERSION);
      InstallLog.Current.Save();
    }
  }
}