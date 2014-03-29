using System;
using ChinhDo.Transactions;
using System.Windows.Forms;
using System.IO;
using fomm.Transactions;
using System.Collections.Generic;
using Fomm.PackageManager.ModInstallLog;

namespace Fomm.PackageManager
{
  class ModUninstaller : ModInstallerBase
  {
    private BackgroundWorkerProgressDialog m_bwdProgress = null;
    private string m_strBaseName = null;

    #region Properties

    /// <seealso cref="ModInstallScript.ExceptionMessage"/>
    protected override string ExceptionMessage
    {
      get
      {
        return "A problem occurred during uninstall: " + Environment.NewLine + "{0}" + Environment.NewLine + "The mod was not uninstalled.";
      }
    }

    /// <seealso cref="ModInstallScript.SuccessMessage"/>
    protected override string SuccessMessage
    {
      get
      {
        return "The mod was successfully uninstalled.";
      }
    }

    /// <seealso cref="ModInstallScript.FailMessage"/>
    protected override string FailMessage
    {
      get
      {
        return "The mod was not uninstalled.";
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object.
    /// </summary>
    /// <param name="p_fomodMod">The <see cref="fomod"/> to be uninstalled.</param>
    public ModUninstaller(fomod p_fomodMod)
      : base(p_fomodMod)
    {
    }

    /// <summary>
    /// A simple constructor that initializes the object.
    /// </summary>
    /// <param name="p_strFomodBaseName">The base name of the <see cref="fomod"/> to be uninstalled.</param>
    public ModUninstaller(string p_strFomodBaseName)
      : base(null)
    {
      m_strBaseName = p_strFomodBaseName.ToLowerInvariant();
    }

    #endregion

    #region Uninstall Methods

    /// <summary>
    /// Indicates that this script's work has already been completed if
    /// the <see cref="Fomod"/> is already not active.
    /// </summary>
    /// <returns><lang cref="true"/> if the <see cref="Fomod"/> is not active;
    /// <lang cref="false"/> otherwise.</returns>
    /// <seealso cref="ModInstallScript.CheckAlreadyDone()"/>
    protected override bool CheckAlreadyDone()
    {
      if (Fomod == null)
        return false;
      return !Fomod.IsActive;
    }

    /// <summary>
    /// Uninstalls the mod and deactivates it.
    /// </summary>
    /// <seealso cref="Uninstall(bool p_booSuppressSuccessMessage)"/>
    public void Uninstall()
    {
      Uninstall(false);
    }

    /// <summary>
    /// Uninstalls the mod and deactivates it.
    /// </summary>
    /// <param name="p_booSuppressSuccessMessage">Indicates whether to
    /// supress the success message. This is useful for batch uninstalls.</param>
    /// <seealso cref="Uninstall()"/>
    public void Uninstall(bool p_booSuppressSuccessMessage)
    {
      Run(p_booSuppressSuccessMessage, false);
    }

    /// <summary>
    /// This does the actual uninstallation; it removes the files and undoes any edits the
    /// fomod made.
    /// </summary>
    /// <returns><lang cref="true"/> if the script work was completed successfully and needs to
    /// be committed; <lang cref="false"/> otherwise.</returns>
    /// <seealso cref="ModInstallScript.DoScript"/>
    protected override bool DoScript()
    {
      foreach (string strSettingsFile in Program.GameMode.SettingsFiles.Values)
        TransactionalFileManager.Snapshot(strSettingsFile);
      foreach (string strAdditionalFile in Program.GameMode.AdditionalPaths.Values)
        if (File.Exists(strAdditionalFile))
          TransactionalFileManager.Snapshot(strAdditionalFile);
      TransactionalFileManager.Snapshot(InstallLog.Current.InstallLogPath);

      bool booIsActive = true;
      try
      {
        if (Fomod != null)
        {
          MergeModule = InstallLog.Current.GetMergeModule(Fomod.BaseName);
          if (Fomod.HasUninstallScript)
            Fomod.IsActive = !RunCustomUninstallScript();
          else
            Fomod.IsActive = !RunBasicUninstallScript();
          if (!Fomod.IsActive)
            InstallLog.Current.UnmergeModule(Fomod.BaseName);
          booIsActive = Fomod.IsActive;
        }
        else
        {
          MergeModule = InstallLog.Current.GetMergeModule(m_strBaseName);
          booIsActive = !RunBasicUninstallScript();
          InstallLog.Current.UnmergeModule(m_strBaseName);
        }
      }
      catch (Exception e)
      {
        if (Fomod != null)
          Fomod.IsActive = true;
        throw e;
      }
      if (booIsActive)
        return false;
      return true;
    }

    /// <summary>
    /// Runs the custom uninstall script included in the fomod.
    /// </summary>
    /// <returns><lang cref="true"/> if the uninstallation was successful;
    /// <lang cref="false"/> otherwise.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the uninstall script
    /// cannot be found.</exception>
    protected bool RunCustomUninstallScript()
    {
      string strScript = Fomod.GetUninstallScript();
      if (strScript == null)
        throw new FileNotFoundException("No uninstall script found, even though fomod claimed to have one.");
      return false;
    }

    /// <summary>
    /// Runs the basic uninstall script.
    /// </summary>
    /// <returns><lang cref="true"/> if the installation was successful;
    /// <lang cref="false"/> otherwise.</returns>
    protected bool RunBasicUninstallScript()
    {
      try
      {
        using (m_bwdProgress = new BackgroundWorkerProgressDialog(PerformBasicUninstall))
        {
          m_bwdProgress.OverallMessage = "Uninstalling Fomod";
          m_bwdProgress.ItemProgressStep = 1;
          m_bwdProgress.OverallProgressStep = 1;
          if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
            return false;
        }
      }
      finally
      {
        m_bwdProgress = null;
      }
      return true;
    }

    /// <summary>
    /// Performs a basic uninstall of the mod.
    /// </summary>
    /// <remarks>
    /// A basic uninstall removes all of the files that were installed by the mod,
    /// and undos all of the edits the mod made during install.
    /// </remarks>
    protected void PerformBasicUninstall()
    {
      List<string> lstFiles = MergeModule.DataFiles;
      List<InstallLogMergeModule.IniEdit> lstIniEdits = MergeModule.IniEdits;
      List<InstallLogMergeModule.GameSpecificValueEdit> lstGameSpecificValueEdits = MergeModule.GameSpecificValueEdits;
      m_bwdProgress.OverallProgressMaximum = lstFiles.Count + lstIniEdits.Count + lstGameSpecificValueEdits.Count;

      m_bwdProgress.ItemProgressMaximum = lstFiles.Count;
      m_bwdProgress.ItemMessage = "Uninstalling Files";
      foreach (string strFile in lstFiles)
      {
        if (m_bwdProgress.Cancelled())
          return;
        if (Fomod == null)
          Script.UninstallDataFile(m_strBaseName, strFile);
        else
          Script.UninstallDataFile(strFile);
        m_bwdProgress.StepItemProgress();
        m_bwdProgress.StepOverallProgress();
      }

      m_bwdProgress.ItemProgressMaximum = lstIniEdits.Count;
      m_bwdProgress.ItemMessage = "Undoing Ini Edits";
      foreach (InstallLogMergeModule.IniEdit iniEdit in lstIniEdits)
      {
        if (m_bwdProgress.Cancelled())
          return;
        if (Fomod == null)
          Script.UneditIni(m_strBaseName, iniEdit.File, iniEdit.Section, iniEdit.Key);
        else
          Script.UneditIni(iniEdit.File, iniEdit.Section, iniEdit.Key);
        m_bwdProgress.StepItemProgress();
        m_bwdProgress.StepOverallProgress();
      }

      m_bwdProgress.ItemProgressMaximum = lstGameSpecificValueEdits.Count;
      m_bwdProgress.ItemMessage = "Undoing Game Specific Value Edits";
      foreach (InstallLogMergeModule.GameSpecificValueEdit gsvEdit in lstGameSpecificValueEdits)
      {
        if (m_bwdProgress.Cancelled())
          return;
        if (Fomod == null)
          Script.UneditGameSpecificValue(m_strBaseName, gsvEdit.Key);
        else
          Script.UneditGameSpecificValue(gsvEdit.Key);
        m_bwdProgress.StepItemProgress();
        m_bwdProgress.StepOverallProgress();
      }
    }

    #endregion
  }
}
