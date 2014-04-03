using System;
using System.Collections.Generic;
using Fomm.PackageManager;
using System.IO;
using Fomm.PackageManager.ModInstallLog;

namespace Fomm.FileManager
{
  public class ModInstallReorderer : ModInstallerBase
  {
    private string m_strFailMessage;
    private string m_strFile;
    private IList<string> m_lstOrderedMods;

    #region Properties

    /// <seealso cref="ModInstallScript.ExceptionMessage"/>
    protected override string ExceptionMessage
    {
      get
      {
        return "A problem occurred during reorder: " + Environment.NewLine + "{0}";
      }
    }

    /// <seealso cref="ModInstallScript.SuccessMessage"/>
    protected override string SuccessMessage
    {
      get
      {
        return "The mod was successfully installed.";
      }
    }

    /// <seealso cref="ModInstallScript.FailMessage"/>
    protected override string FailMessage
    {
      get
      {
        return m_strFailMessage;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object.
    /// </summary>
    internal ModInstallReorderer()
      : base(null)
    {
    }

    #endregion

    /// <summary>
    /// Checks to see if the script work has already been done.
    /// </summary>
    /// <remarks>
    /// This always returns <lang langref="false"/>.
    /// </remarks>
    /// <returns><lang langref="true"/> if the script work has already been done and the script
    /// doesn't need to execute; <lang langref="false"/> otherwise.</returns>
    protected override bool CheckAlreadyDone()
    {
      return false;
    }

    /// <summary>
    /// Reorders the installers of the specified file.
    /// </summary>
    /// <remarks>
    /// This changes the version of the specified file that is in the user's data directory.
    /// </remarks>
    /// <param name="p_strFile">The file whose installers are to be reordered.</param>
    /// <param name="p_lstOrderedMods">The new order of the file's installers.</param>
    /// <returns><lang langref="true"/> if the file installers were reordered;
    /// <lang langref="false"/> otherwise.</returns>
    internal bool ReorderFileInstallers(string p_strFile, List<string> p_lstOrderedMods)
    {
      m_strFile = p_strFile;
      m_lstOrderedMods = p_lstOrderedMods;
      return Run(true, true);
    }

    /// <summary>
    /// This does the moving of files and log alteration.
    /// </summary>
    /// <returns><lang langref="true"/> if the script work was completed successfully and needs to
    /// be committed; <lang langref="false"/> otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown if m_strFile or m_lstOrderedMods are
    /// <lang langref="null"/>.</exception>
    /// <seealso cref="ModInstallScript.DoScript"/>
    protected override bool DoScript()
    {
      if ((m_strFile == null) || (m_lstOrderedMods == null))
      {
        throw new InvalidOperationException(
          "The File and OrderedMods properties must be set before calling Run(); or Run(string, IList<string>) can be used instead.");
      }

      TransactionalFileManager.Snapshot(InstallLog.Current.InstallLogPath);

      var strOldOwner = InstallLog.Current.GetCurrentFileOwnerKey(m_strFile);
      InstallLog.Current.SetInstallingModsOrder(m_strFile, m_lstOrderedMods);
      var strNewOwner = InstallLog.Current.GetCurrentFileOwnerKey(m_strFile);

      if (!strNewOwner.Equals(strOldOwner))
      {
        var strDataPath = Path.Combine(Program.GameMode.PluginsPath, m_strFile);
        strDataPath = Directory.GetFiles(Path.GetDirectoryName(strDataPath), Path.GetFileName(strDataPath))[0];

        var strDirectory = Path.GetDirectoryName(m_strFile);
        var strBackupPath = Path.Combine(Program.GameMode.OverwriteDirectory, strDirectory);
        //the old backup file is becoming the new file
        var strOldBackupFile = strNewOwner + "_" + Path.GetFileName(strDataPath);
        //the old owner is becoming the new backup file
        var strNewBackupFile = strOldOwner + "_" + Path.GetFileName(strDataPath);

        var strNewBackupPath = Path.Combine(strBackupPath, strNewBackupFile);
        var strOldBackupPath = Path.Combine(strBackupPath, strOldBackupFile);
        if (!TransactionalFileManager.FileExists(strOldBackupPath))
        {
          m_strFailMessage = "The version of the file for " + InstallLog.Current.GetModName(strNewOwner) +
                             " does not exist. This is likely because files in the data folder have been altered manually.";
          return false;
        }
        TransactionalFileManager.Copy(strDataPath, strNewBackupPath, true);
        var strOldBackupFileName = Path.GetFileName(Directory.GetFiles(strBackupPath, strOldBackupFile)[0]);
        var strCasedFileName = strOldBackupFileName.Substring(strOldBackupFileName.IndexOf('_') + 1);
        var strNewDataPath = Path.Combine(Path.GetDirectoryName(strDataPath), strCasedFileName);
        TransactionalFileManager.Delete(strNewDataPath);
        TransactionalFileManager.Move(strOldBackupPath, strNewDataPath);
      }
      return true;
    }
  }
}