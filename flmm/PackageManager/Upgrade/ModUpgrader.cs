using System;
using System.IO;
using System.Windows.Forms;
using Fomm.PackageManager.ModInstallLog;

namespace Fomm.PackageManager.Upgrade
{
  /// <summary>
  /// Performs an in-place upgrade of a <see cref="fomod"/>.
  /// </summary>
  /// <remarks>
  /// An in-place upgrade installs one fomod over top of another. This differs from deactivating the old
  /// fomod and activating the new fomod in that a deactivation/activation will cause the new fomod's
  /// files/changes to overwrite existing data, whereas an in-place upgrade puts the new fomod's data
  /// in same priority as that of the old fomod.
  /// 
  /// For example, assume Mod A (v1.0) installs File1, and Mod B overwrites File1 with a new version. If
  /// Mod A (v1.0) is upgraded with Mod A (v2.0), Mod A (v2.0)'s File1 will be place in the overwrite
  /// directory and Mod B's File1 will still be the version used.
  /// </remarks>
  public class ModUpgrader : ModInstaller
  {
    private fomod m_fomodOriginalMod;
    private BackgroundWorkerProgressDialog m_bwdProgress;

    #region Properties

    /// <seealso cref="ModInstallScript.ExceptionMessage"/>
    protected override string ExceptionMessage
    {
      get
      {
        return "A problem occurred during in-place upgrade: " + Environment.NewLine + "{0}" + Environment.NewLine +
               "The mod was not upgraded.";
      }
    }

    /// <seealso cref="ModInstallScript.SuccessMessage"/>
    protected override string SuccessMessage
    {
      get
      {
        return "The mod was successfully upgraded.";
      }
    }

    /// <seealso cref="ModInstallScript.FailMessage"/>
    protected override string FailMessage
    {
      get
      {
        return "The mod was not upgraded.";
      }
    }

    /// <summary>
    /// Gets the message to display inthe progress dialog.
    /// </summary>
    /// <value>The message to display inthe progress dialog.</value>
    protected virtual string ProgressMessage
    {
      get
      {
        return "Upgrading Fomod";
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object.
    /// </summary>
    /// <param name="p_fomodMod">The <see cref="fomod"/> to be upgraded.</param>
    internal ModUpgrader(fomod p_fomodMod)
      : this(p_fomodMod, p_fomodMod.BaseName)
    {
    }

    /// <summary>
    /// A simple constructor that initializes the object.
    /// </summary>
    /// <param name="p_fomodMod">The <see cref="fomod"/> to be upgraded.</param>
    internal ModUpgrader(fomod p_fomodMod, string p_strOldBaseName)
      : base(new UpgradeFomod(p_fomodMod.filepath))
    {
      m_fomodOriginalMod = p_fomodMod;
      ((UpgradeFomod) Fomod).SetBaseName(p_strOldBaseName);
    }

    #endregion

    #region Install Methods

    /// <summary>
    /// Indicates that this script's work has already been completed if
    /// the <see cref="fomod"/>'s installed version is equal to the
    /// current <see cref="Fomod"/>'s version.
    /// </summary>
    /// <returns><lang cref="true"/> if the <see cref="Fomod"/>'s installed version is equal to the
    /// current <see cref="Fomod"/>'s version; <lang cref="false"/> otherwise.</returns>
    /// <seealso cref="ModInstallScript.CheckAlreadyDone()"/>
    protected override bool CheckAlreadyDone()
    {
      FomodInfo fifInfo = InstallLog.Current.GetModInfo(Fomod.BaseName);
      string strCurrentVersion = (fifInfo == null) ? null : fifInfo.Version;
      return Fomod.HumanReadableVersion.Equals(strCurrentVersion);
    }

    /// <summary>
    /// Performs an in-place upgrade of the <see cref="fomod"/>.
    /// </summary>
    internal void Upgrade()
    {
      Run();
    }

    /// <summary>
    /// Performs an in-place upgrade of the <see cref="fomod"/>.
    /// </summary>
    protected override bool DoScript()
    {
      foreach (string strSettingsFile in Program.GameMode.SettingsFiles.Values)
      {
        TransactionalFileManager.Snapshot(strSettingsFile);
      }
      foreach (string strAdditionalFile in Program.GameMode.AdditionalPaths.Values)
      {
        if (File.Exists(strAdditionalFile))
        {
          TransactionalFileManager.Snapshot(strAdditionalFile);
        }
      }
      TransactionalFileManager.Snapshot(InstallLog.Current.InstallLogPath);

      bool booUpgraded = false;
      try
      {
        MergeModule = new InstallLogMergeModule();
        if (Fomod.HasInstallScript)
        {
          FomodScript fscInstallScript = Fomod.GetInstallScript();
          switch (fscInstallScript.Type)
          {
            case FomodScriptType.CSharp:
              booUpgraded = RunCustomInstallScript();
              break;
            case FomodScriptType.XMLConfig:
              booUpgraded = RunXmlInstallScript();
              break;
          }
        }
        else
        {
          booUpgraded = RunBasicInstallScript(ProgressMessage);
        }
        if (booUpgraded)
        {
          using (m_bwdProgress = new BackgroundWorkerProgressDialog(ReconcileDifferences))
          {
            m_bwdProgress.OverallMessage = "Finalizing Upgrade";
            m_bwdProgress.ItemProgressStep = 1;
            m_bwdProgress.OverallProgressStep = 1;
            if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
            {
              return false;
            }
          }
          string strOldBaseName = Fomod.BaseName;
          ((UpgradeFomod) Fomod).SetBaseName(((UpgradeFomod) Fomod).OriginalBaseName);
          InstallLog.Current.MergeUpgrade(Fomod, strOldBaseName, MergeModule);
          ((UpgradeFomod) Fomod).SetBaseName(strOldBaseName);
          Script.CommitActivePlugins();
        }
      }
      catch (Exception e)
      {
        booUpgraded = false;
        throw e;
      }
      m_fomodOriginalMod.IsActive = DetermineFomodActiveStatus(booUpgraded);
      return booUpgraded;
    }

    protected override ModInstallScript CreateInstallScript()
    {
      return Program.GameMode.CreateUpgradeScript(Fomod, this);
    }

    /// <summary>
    /// Determines whether or not the fomod should be activated, based on whether
    /// or not the script was successful.
    /// </summary>
    /// <param name="p_booSucceeded">Whether or not the script was successful.</param>
    /// <returns><lang cref="true"/> if the script was successful;
    /// <lang cref="false"/> otherwise.</returns>
    protected virtual bool DetermineFomodActiveStatus(bool p_booSucceeded)
    {
      return p_booSucceeded;
    }

    #endregion

    /// <summary>
    /// This undoes any changes that were made by the previous version of the fomod being upgraded, but
    /// were not made by the current version.
    /// </summary>
    /// <remarks>
    /// This method is used for the background worker.
    /// </remarks>
    private void ReconcileDifferences()
    {
      m_bwdProgress.OverallProgressMaximum = 3;
      m_bwdProgress.ItemMessage = "Synchronizing Files";
      InstallLogMergeModule ilmPreviousChanges = InstallLog.Current.GetMergeModule(Fomod.BaseName);
      m_bwdProgress.ItemProgressMaximum = ilmPreviousChanges.DataFiles.Count;
      foreach (string strFile in ilmPreviousChanges.DataFiles)
      {
        if (!MergeModule.ContainsFile(strFile))
        {
          Script.UninstallDataFile(strFile);
        }
        if (m_bwdProgress.Cancelled())
        {
          return;
        }
        m_bwdProgress.StepItemProgress();
      }
      m_bwdProgress.StepOverallProgress();

      m_bwdProgress.ItemMessage = "Synchronizing Ini Edits";
      m_bwdProgress.ItemProgressMaximum = ilmPreviousChanges.IniEdits.Count;
      foreach (InstallLogMergeModule.IniEdit iniEdit in ilmPreviousChanges.IniEdits)
      {
        if (!MergeModule.IniEdits.Contains(iniEdit))
        {
          Script.UneditIni(iniEdit.File, iniEdit.Section, iniEdit.Key);
        }
        if (m_bwdProgress.Cancelled())
        {
          return;
        }
        m_bwdProgress.StepItemProgress();
      }
      m_bwdProgress.StepOverallProgress();

      m_bwdProgress.ItemMessage = "Synchronizing Game Specific Value Edits";
      m_bwdProgress.ItemProgressMaximum = ilmPreviousChanges.GameSpecificValueEdits.Count;
      foreach (InstallLogMergeModule.GameSpecificValueEdit gsvEdit in ilmPreviousChanges.GameSpecificValueEdits)
      {
        if (!MergeModule.GameSpecificValueEdits.Contains(gsvEdit))
        {
          Script.UneditGameSpecificValue(gsvEdit.Key);
        }
        if (m_bwdProgress.Cancelled())
        {
          return;
        }
        m_bwdProgress.StepItemProgress();
      }
      m_bwdProgress.StepOverallProgress();
    }
  }
}