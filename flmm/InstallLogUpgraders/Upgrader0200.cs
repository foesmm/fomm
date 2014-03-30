using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
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
      XDocument doc       = null;
      XElement  root      = null;
      XElement  modlist   = null;
      XElement  datafiles = null;
      XElement  mod       = null;
      IList<string> lstMods;
      fomod fomodMod = null;
      string strModPath;

      // Load the document
      doc       = XDocument.Load(InstallLog.Current.InstallLogPath);
      root      = doc.Element("installLog");
      modlist   = root.Element("modList");
      datafiles = root.Element("dataFiles");

      // Set the declaration (missing in previous versions)
      doc.Declaration = new XDeclaration("1.0", "UTF-8", null);

      // Update file to new version
      root.SetAttributeValue("fileVersion", InstallLog.CURRENT_VERSION);

      // Upgrade datafile entries
      foreach (XElement el in datafiles.Descendants("file"))
      {
        el.SetAttributeValue("path", Path.Combine("Data", el.Attribute("path").Value));
      }

      // Upgrade mod entries
      lstMods = InstallLog.Current.GetModList();
      ProgressWorker.OverallProgressStep = 0;
      ProgressWorker.OverallProgressMaximum = lstMods.Count;
      ProgressWorker.ShowItemProgress = false;

      foreach (string strMod in lstMods)
      {
        ProgressWorker.StepOverallProgress();
        strModPath = Path.Combine(Program.GameMode.ModDirectory, strMod + ".fomod");
        if (File.Exists(strModPath))
        {
          fomodMod = new fomod(strModPath);
          // find the matching mod entry
          mod = modlist.XPathSelectElement("mod[@name='" + strMod + "']");

          // Add path attribute
          mod.SetAttributeValue("path", Path.GetFileName(fomodMod.filepath));

          // Remove name attribute
          mod.SetAttributeValue("name", null);

          // Add name element
          mod.Add(new XElement("name", strMod));

          // Add installdate element
          mod.Add(new XElement("installDate", System.IO.File.GetCreationTime(fomodMod.filepath).ToString("MM/dd/yyyy HH:mm:ss")));
        }
      }

      // Update ORIGNAL_VALUES
      mod = modlist.XPathSelectElement("mod[@name='" + InstallLog.ORIGINAL_VALUES + "']");
      if (mod != null)
      {
        mod.SetAttributeValue("path", "Dummy Mod: " + InstallLog.ORIGINAL_VALUES);
        mod.SetAttributeValue("name", null);
        mod.Add(new XElement("version", "0"));
        mod.Element("version").SetAttributeValue("machineVersion", mod.Element("version").Value);
        mod.Add(new XElement("name", InstallLog.ORIGINAL_VALUES));
        mod.Add(new XElement("installDate", DateTime.Today.ToString("MM/dd/yyyy HH:mm:ss")));
      }

      // Update ORIGNAL_VALUES
      mod = modlist.XPathSelectElement("mod[@name='" + InstallLog.FOMM + "']");
      if (mod != null)
      {
        mod.SetAttributeValue("path", "Dummy Mod: " + InstallLog.MOD_MANAGER_VALUE);
        mod.SetAttributeValue("name", null);
        mod.Add(new XElement("version", "0"));
        mod.Element("version").SetAttributeValue("machineVersion", mod.Element("version").Value);
        mod.Add(new XElement("name", InstallLog.MOD_MANAGER_VALUE));
        mod.Add(new XElement("installDate", DateTime.Today.ToString("MM/dd/yyyy HH:mm:ss")));
      }

//      doc.Save("C:\\games\\FalloutNV\\Install Info\\test.xml");
      doc.Save(InstallLog.Current.InstallLogPath);
    }
  }
}
