using System;
using System.Globalization;
using Fomm.Games.Fallout3.Tools.BSA;
using Fomm.PackageManager;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Fomm.SharpZipLib.Checksums;
using Fomm.PackageManager.ModInstallLog;

namespace Fomm.InstallLogUpgraders
{
  /// <summary>
  /// Upgrades the Install Log to the current version from version 0.0.0.0.
  /// </summary>
  internal class Upgrader0000 : Upgrader
  {
    private Dictionary<string, string> m_dicDefaultFileOwners;
    private XmlDocument m_xmlOldInstallLog;

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public Upgrader0000()
    {
      m_xmlOldInstallLog = new XmlDocument();
      m_xmlOldInstallLog.Load(InstallLog.Current.InstallLogPath);
    }

    #endregion

    /// <summary>
    /// Upgrades the Install Log to the current version from version 0.0.0.0.
    /// </summary>
    /// <remarks>
    /// This method is called by a background worker to perform the actual upgrade.
    /// </remarks>
    protected override void DoUpgrade()
    {
      InstallLog.Current.Reset();

      var strModInstallFiles = Directory.GetFiles(Program.GameMode.ModDirectory, "*.XMl",
                                                       SearchOption.TopDirectoryOnly);
      ProgressWorker.OverallProgressStep = 1;
      ProgressWorker.OverallProgressMaximum = strModInstallFiles.Length;
      ProgressWorker.ItemProgressStep = 1;

      foreach (var strModInstallLog in strModInstallFiles)
      {
        if (ProgressWorker.Cancelled())
        {
          return;
        }

        var strFomodPath = Path.ChangeExtension(strModInstallLog, ".fomod");
        if (File.Exists(strFomodPath))
        {
          var xmlModInstallLog = new XmlDocument();
          xmlModInstallLog.Load(strModInstallLog);
          //figure out how much work we need to do for this mod
          var xnlFiles = xmlModInstallLog.SelectNodes("descendant::installedFiles/*");
          var xnlIniEdits = xmlModInstallLog.SelectNodes("descendant::iniEdits/*");
          var xnlSdpEdits = xmlModInstallLog.SelectNodes("descendant::sdpEdits/*");
          var intItemCount = xnlFiles.Count + xnlIniEdits.Count + xnlSdpEdits.Count;
          ProgressWorker.ItemMessage = Path.GetFileNameWithoutExtension(strModInstallLog);
          ProgressWorker.ItemProgress = 0;
          ProgressWorker.ItemProgressMaximum = intItemCount;

          var fomodMod = new fomod(strFomodPath);
          var strModBaseName = fomodMod.BaseName;
          InstallLog.Current.AddMod(fomodMod);

          m_dicDefaultFileOwners = new Dictionary<string, string>();
          UpgradeInstalledFiles(xmlModInstallLog, fomodMod, strModBaseName);
          //we now have to tell all the remaining default owners that are are indeed
          // the owners
          foreach (var kvpOwner in m_dicDefaultFileOwners)
          {
            MakeOverwrittenModOwner(kvpOwner.Value, kvpOwner.Key);
          }
          if (ProgressWorker.Cancelled())
          {
            return;
          }

          UpgradeIniEdits(xmlModInstallLog, strModBaseName);
          if (ProgressWorker.Cancelled())
          {
            return;
          }

          UpgradeSdpEdits(xmlModInstallLog, strModBaseName);
          if (ProgressWorker.Cancelled())
          {
            return;
          }

          if (File.Exists(strModInstallLog + ".bak"))
          {
            FileManager.Delete(strModInstallLog + ".bak");
          }
          FileManager.Move(strModInstallLog, strModInstallLog + ".bak");
        }
        ProgressWorker.StepOverallProgress();
      }
      InstallLog.Current.SetInstallLogVersion(InstallLog.CURRENT_VERSION);
      InstallLog.Current.Save();
    }

    #region Sdp Edits Upgrade

    private byte[] GetOldSdpValue(Int32 p_intPackage, string p_strShader)
    {
      var node =
        m_xmlOldInstallLog.SelectSingleNode("descendant::sdp[@package=\"" + p_intPackage + "\" and @shader=\"" +
                                            p_strShader + "\"]");
      if (node == null)
      {
        return null;
      }
      var b = new byte[node.InnerText.Length/2];
      for (var i = 0; i < b.Length; i++)
      {
        b[i] = byte.Parse("" + node.InnerText[i*2] + node.InnerText[i*2 + 1],
                          NumberStyles.AllowHexSpecifier);
      }
      return b;
    }

    private List<string> m_lstSeenShader = new List<string>();

    /// <summary>
    /// Upgrades the sdp edits log entries.
    /// </summary>
    /// <remarks>
    /// This analyses the mods and determines, as best as possible, who edited which shaders, and attempts
    /// to reconstruct the install order. The resulting information is then put in the new install log.
    /// </remarks>
    /// <param name="p_xmlModInstallLog">The current mod install log we are parsing to upgrade.</param>
    /// <param name="p_strModBaseName">The base name of the mod whose install log is being parsed.</param>
    private void UpgradeSdpEdits(XmlDocument p_xmlModInstallLog, string p_strModBaseName)
    {
      var xnlSdpEdits = p_xmlModInstallLog.SelectNodes("descendant::sdpEdits/*");
      foreach (XmlNode xndSdpEdit in xnlSdpEdits)
      {
        var intPackage = Int32.Parse(xndSdpEdit.Attributes.GetNamedItem("package").Value);
        var strShader = xndSdpEdit.Attributes.GetNamedItem("shader").Value;
        var strShaderKey = String.Format("sdp:{0}/{1}", intPackage, strShader);
        var bteOldValue = GetOldSdpValue(intPackage, strShader);
        //we have no way of knowing who last edited the shader - that information
        // was not tracked
        // so, let's just do first come first serve 
        if (!m_lstSeenShader.Contains(intPackage + "~" + strShader.ToLowerInvariant()))
        {
          //this is the first mod we have encountered that edited this shader,
          // so let's assume it is the lastest mod to have made the edit...
          InstallLog.Current.AddGameSpecificValueEdit(p_strModBaseName, strShaderKey,
                                                      SDPArchives.GetShader(intPackage,
                                                                                                          strShader));
          //...and backup the old value as the original value
          InstallLog.Current.PrependAfterOriginalGameSpecificValueEdit(InstallLog.ORIGINAL_VALUES, strShaderKey,
                                                                       bteOldValue);
          m_lstSeenShader.Add(intPackage + "~" + strShader.ToLowerInvariant());
        }
        else
        {
          //someone else made the shader edit
          // we don't know what value was overwritten, so we will just use what we have
          // which is the old value
          InstallLog.Current.PrependAfterOriginalGameSpecificValueEdit(p_strModBaseName, strShaderKey, bteOldValue);
        }

        if (ProgressWorker.Cancelled())
        {
          return;
        }
        ProgressWorker.StepItemProgress();
      }
    }

    #endregion

    #region Ini Edits Upgrade

    private string GetOldIniValue(string p_strFile, string p_strSection, string p_strKey, out string p_strModName)
    {
      p_strModName = null;
      var node =
        m_xmlOldInstallLog.SelectSingleNode("descendant::ini[@file=\"" + p_strFile + "\" and @section=\"" + p_strSection +
                                            "\" and @key=\"" + p_strKey + "\"]");
      if (node == null)
      {
        return null;
      }
      var modnode = node.Attributes.GetNamedItem("mod");
      if (modnode != null)
      {
        p_strModName = modnode.Value;
      }
      return node.InnerText;
    }

    /// <summary>
    /// Upgrades the ini edits log entries.
    /// </summary>
    /// <remarks>
    /// This analyses the mods and determines, as best as possible, who edited which keys, and attempts
    /// to reconstruct the install order. The resulting information is then put in the new install log.
    /// </remarks>
    /// <param name="p_xmlModInstallLog">The current mod install log we are parsing to upgrade.</param>
    /// <param name="p_strModBaseName">The base name of the mod whose install log is being parsed.</param>
    private void UpgradeIniEdits(XmlDocument p_xmlModInstallLog, string p_strModBaseName)
    {
      var xnlIniEdits = p_xmlModInstallLog.SelectNodes("descendant::iniEdits/*");
      foreach (XmlNode xndIniEdit in xnlIniEdits)
      {
        var strFile = xndIniEdit.Attributes.GetNamedItem("file").Value;
        var strSection = xndIniEdit.Attributes.GetNamedItem("section").Value;
        var strKey = xndIniEdit.Attributes.GetNamedItem("key").Value;
        string strOldIniEditor;
        var strOldValue = GetOldIniValue(strFile, strSection, strKey, out strOldIniEditor);
        if (p_strModBaseName.Equals(strOldIniEditor))
        {
          //this mod owns the ini edit, so append it to the list of editing mods...
          InstallLog.Current.AddIniEdit(strFile, strSection, strKey, p_strModBaseName,
                                        NativeMethods.GetPrivateProfileString(strSection, strKey, "", strFile));
          //...and backup the old value as the original value
          InstallLog.Current.PrependAfterOriginalIniEdit(strFile, strSection, strKey, InstallLog.ORIGINAL_VALUES,
                                                         strOldValue);
        }
        else
        {
          //someone else made the ini edit
          // we don't know what value was overwritten, so we will just use what we have
          // which is the old value stored in the old install log
          InstallLog.Current.PrependAfterOriginalIniEdit(strFile, strSection, strKey, p_strModBaseName, strOldValue);
        }

        if (ProgressWorker.Cancelled())
        {
          return;
        }
        ProgressWorker.StepItemProgress();
      }
    }

    #endregion

    #region Installed Files Upgrade

    /// <summary>
    /// Makes the specified mod the owner of the specified file.
    /// </summary>
    /// <remarks>
    /// Moves the node representing that the specified mod installed the specified file to the end,
    /// indicating in was the last mod to install the file. It also deletes the mod's backup of the file
    /// from the overwrites folder.
    /// </remarks>
    /// <param name="p_strModName">The base name of the mod that is being made the file owner.</param>
    /// <param name="p_strDataRealtivePath">The path of the file whose owner is changing..</param>
    private void MakeOverwrittenModOwner(string p_strMadBaseName, string p_strDataRealtivePath)
    {
      var strModKey = InstallLog.Current.GetModKey(p_strMadBaseName);
      var strDirectory = Path.GetDirectoryName(p_strDataRealtivePath);
      var strBackupPath = Path.Combine(Program.GameMode.OverwriteDirectory, strDirectory);
      strBackupPath = Path.Combine(strBackupPath, strModKey + "_" + Path.GetFileName(p_strDataRealtivePath));
      FileManager.Delete(strBackupPath);
      InstallLog.Current.AddDataFile(p_strMadBaseName, p_strDataRealtivePath);
    }

    /// <summary>
    /// Determines if we already know who owns the specified file.
    /// </summary>
    /// <remarks>
    /// We know who owns a specified file if the file has at least one installing mod, and
    /// the last installing mod doesn't have a corresponding file in the overwrites folder.
    /// </remarks>
    /// <param name="p_strDataRelativePath">The file for which it is to be determined if the owner is known.</param>
    /// <returns><lang cref="true"/> if the owner is known; <lang cref="false"/> otherwise.</returns>
    private bool FileOwnerIsKnown(string p_strDataRelativePath)
    {
      var strModKey = InstallLog.Current.GetCurrentFileOwnerKey(p_strDataRelativePath);
      if (strModKey == null)
      {
        return false;
      }
      var strDirectory = Path.GetDirectoryName(p_strDataRelativePath);
      var strBackupPath = Path.Combine(Program.GameMode.OverwriteDirectory, strDirectory);
      strBackupPath = Path.Combine(strBackupPath, strModKey + "_" + Path.GetFileName(p_strDataRelativePath));
      return !FileManager.FileExists(strBackupPath);
    }

    /// <summary>
    /// Upgrades the installed files log entries.
    /// </summary>
    /// <remarks>
    /// This analyses the mods and determines, as best as possible, who owns which files, and attempts
    /// to reconstruct the install order. It populates the overwrites folder with the files that, as far
    /// as can be determined, belong there. This resulting information is then put in the new install log.
    /// </remarks>
    /// <param name="p_xmlModInstallLog">The current mod install log we are parsing to upgrade.</param>
    /// <param name="p_strModInstallLogPath">The path to the current mod install log.</param>
    /// <param name="p_strModBaseName">The base name of the mod whose install log is being parsed.</param>
    private void UpgradeInstalledFiles(XmlDocument p_xmlModInstallLog, fomod p_fomodMod, string p_strModBaseName)
    {
      var intDataPathStartPos =
        Program.GameMode.PluginsPath.Trim(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Length + 1;
      var xnlFiles = p_xmlModInstallLog.SelectNodes("descendant::installedFiles/*");
      foreach (XmlNode xndFile in xnlFiles)
      {
        var strFile = xndFile.InnerText;
        if (!File.Exists(strFile))
        {
          continue;
        }
        var strDataRelativePath = strFile.Substring(intDataPathStartPos);

        var crcDiskFile = new Crc32();
        var crcFomodFile = new Crc32();
        crcDiskFile.Update(File.ReadAllBytes(strFile));
        if (!p_fomodMod.ContainsFile(strDataRelativePath))
        {
          //we don't know if this mod owns the file, so let's assume
          // it doesn't
          //put this mod's file into the overwrites directory.
          // we can't get the original file from the fomod,
          // so we'll use the existing file instead. this isn't
          // strictly correct, but it is inline with the behaviour
          // of the fomm version we are upgrading from
          var strDirectory = Path.GetDirectoryName(strDataRelativePath);
          var strBackupPath = Path.Combine(Program.GameMode.OverwriteDirectory, strDirectory);
          var strModKey = InstallLog.Current.GetModKey(p_strModBaseName);
          if (!Directory.Exists(strBackupPath))
          {
            FileManager.CreateDirectory(strBackupPath);
          }
          strBackupPath = Path.Combine(strBackupPath, strModKey + "_" + Path.GetFileName(strDataRelativePath));
          FileManager.Copy(Path.Combine(Program.GameMode.PluginsPath, strDataRelativePath), strBackupPath, true);
          InstallLog.Current.PrependDataFile(p_strModBaseName, strDataRelativePath);

          //however, it may own the file, so let's make it the default owner for now
          // unless we already know who the owner is
          if (!FileOwnerIsKnown(strDataRelativePath))
          {
            m_dicDefaultFileOwners[strDataRelativePath] = p_strModBaseName;
          }
          continue;
        }
        var bteFomodFile = p_fomodMod.GetFileContents(strDataRelativePath);
        crcFomodFile.Update(bteFomodFile);
        if (!crcDiskFile.Value.Equals(crcFomodFile.Value) || FileOwnerIsKnown(strDataRelativePath))
        {
          //either:
          // 1) another mod owns the file
          // 2) according to the crc we own this file, however we have already found
          //    an owner. this could happen beacue two mods use the same version
          //    of a file, or there is a crc collision.
          //either way, put this mod's file into
          // the overwrites directory
          var strDirectory = Path.GetDirectoryName(strDataRelativePath);
          var strBackupPath = Path.Combine(Program.GameMode.OverwriteDirectory, strDirectory);
          var strModKey = InstallLog.Current.GetModKey(p_strModBaseName);
          if (!Directory.Exists(strBackupPath))
          {
            FileManager.CreateDirectory(strBackupPath);
          }
          strBackupPath = Path.Combine(strBackupPath, strModKey + "_" + Path.GetFileName(strDataRelativePath));
          FileManager.WriteAllBytes(strBackupPath, bteFomodFile);
          InstallLog.Current.PrependDataFile(p_strModBaseName, strDataRelativePath);
        }
        else
        {
          //this mod owns the file, so append it to the list of installing mods
          InstallLog.Current.AddDataFile(p_strModBaseName, strDataRelativePath);

          //we also have to displace the mod that is currently the default owner
          if (m_dicDefaultFileOwners.ContainsKey(strDataRelativePath))
          {
            m_dicDefaultFileOwners.Remove(strDataRelativePath);
          }
        }

        if (ProgressWorker.Cancelled())
        {
          return;
        }
        ProgressWorker.StepItemProgress();
      }
    }

    #endregion
  }
}