using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Fomm.PackageManager.ModInstallLog;

namespace Fomm.PackageManager.Upgrade
{
  /// <summary>
  /// Checks to see if any fomods' versions have changed.
  /// </summary>
  public class UpgradeScanner
  {
    protected static readonly string m_strUpgradeMessage =
      "A different version of {0} has been detected. The installed version is {1}, the new version is {2}. Would you like to upgrade?" +
      Environment.NewLine + "Selecting No will replace the FOMod in FOMM's plugin list, but won't change any files.";

    /// <summary>
    /// Scans the mods folder for fomods that have versions that differ from their versions in the install log.
    /// </summary>
    /// <remarks>
    /// If fomods with versions that differ from those in the install log are found, the use is asked whether
    /// to replace or upgrade the fomod. Replacing the fomod merely changes the version in the install log,
    /// but makes no system changes. Upgrading the fomod performs an in-place upgrade.
    /// </remarks>
    public void Scan()
    {
      var lstMods = InstallLog.Current.GetVersionedModList();
      var lstModsToUpgrade = new List<fomod>();
      var lstModsToReplace = new List<fomod>();
      foreach (var fifMod in lstMods)
      {
        var fomodMod = new fomod(Path.Combine(Program.GameMode.ModDirectory, fifMod.BaseName + ".fomod"));
        if (!fomodMod.HumanReadableVersion.Equals(fifMod.Version))
        {
          switch (
            MessageBox.Show(
              String.Format(m_strUpgradeMessage, fomodMod.ModName, fifMod.Version, fomodMod.HumanReadableVersion),
              "Upgrade", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
          {
            case DialogResult.Yes:
              lstModsToUpgrade.Add(fomodMod);
              break;
            case DialogResult.No:
              lstModsToReplace.Add(fomodMod);
              break;
          }
        }
      }

      Replace(lstModsToReplace);
      Upgrade(lstModsToUpgrade);
    }

    /// <summary>
    /// Upgrades the given fomods.
    /// </summary>
    /// <param name="p_lstModsToUpgrade">The list of fomods to upgrade.</param>
    private void Upgrade(IList<fomod> p_lstModsToUpgrade)
    {
      foreach (var fomodMod in p_lstModsToUpgrade)
      {
        var mduUpgrader = new ModUpgrader(fomodMod);
        mduUpgrader.Upgrade();
      }
    }

    /// <summary>
    /// Replaces the given fomods in the install log.
    /// </summary>
    /// <param name="p_lstModsToReplace">The list of fomods to replace.</param>
    protected void Replace(IList<fomod> p_lstModsToReplace)
    {
      if (p_lstModsToReplace.Count > 0)
      {
        foreach (var fomodMod in p_lstModsToReplace)
        {
          InstallLog.Current.UpdateMod(fomodMod);
        }
        InstallLog.Current.Save();
      }
    }
  }
}