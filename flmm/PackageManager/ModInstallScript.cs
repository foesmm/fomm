using System;
using ChinhDo.Transactions;
using System.IO;
using System.Collections.Generic;
using fomm.Transactions;
using System.Windows.Forms;
using System.Text;
using Fomm.PackageManager.ModInstallLog;
using System.ComponentModel;
using System.Drawing;
using Fomm.Util;

namespace Fomm.PackageManager
{
  /// <summary>
  /// the base script for scripts that install or uninstall mods.
  /// </summary>
  public abstract class ModInstallScript : IDisposable
  {
    private Set<string> m_setActivePlugins = null;
    private fomod m_fomodMod = null;
    private ModInstallerBase m_mibInstaller = null;
    private List<string> m_lstOverwriteFolders = new List<string>();
    private List<string> m_lstDontOverwriteFolders = new List<string>();
    private bool m_booDontOverwriteAll = false;
    private bool m_booOverwriteAll = false;
    private List<string> m_lstOverwriteMods = new List<string>();
    private List<string> m_lstDontOverwriteMods = new List<string>();
    private bool m_booDontOverwriteAllIni = false;
    private bool m_booOverwriteAllIni = false;

    #region Properties

    /// <summary>
    /// Gets the list of active plugins.
    /// </summary>
    /// <value>The list of active plugins.</value>
    protected Set<string> ActivePlugins
    {
      get
      {
        if (m_setActivePlugins == null)
        {
          PermissionsManager.CurrentPermissions.Assert();
          m_setActivePlugins = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
          m_setActivePlugins.AddRange(Program.GameMode.PluginManager.ActivePluginList);
        }
        return m_setActivePlugins;
      }
    }

    /// <summary>
    /// Gets the mod that is being scripted against.
    /// </summary>
    /// <value>The mod that is being scripted against.</value>
    public fomod Fomod
    {
      get
      {
        return m_fomodMod;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to overwrite
    /// all Ini values.
    /// </summary>
    /// <value>A value indicating whether to overwrite
    /// all Ini values.</value>
    public bool OverwriteAllIni
    {
      get
      {
        return m_booOverwriteAllIni;
      }
      set
      {
        m_booOverwriteAllIni = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to overwrite
    /// all files.
    /// </summary>
    /// <value>A value indicating whether to overwrite
    /// all files.</value>
    protected bool OverwriteAllFiles
    {
      get
      {
        return m_booOverwriteAll;
      }
      set
      {
        m_booOverwriteAll = value;
      }
    }

    protected ModInstallerBase Installer
    {
      get
      {
        return m_mibInstaller;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object.
    /// </summary>
    /// <param name="p_fomodMod">The <see cref="fomod"/> to be installed or uninstalled.</param>
    public ModInstallScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller)
    {
      m_fomodMod = p_fomodMod;

      //make sure the permissions manager is initialized.
      // static members are (usually) only loaded upon first access.
      // this can cause a problem for our permissions manager as if
      // the first time it is called is in a domain with limited access
      // to the machine then the initialization will fail.
      // to prevent this, we call it now to make sure it is ready when we need it.
      object objIgnore = PermissionsManager.CurrentPermissions;

      m_mibInstaller = p_mibInstaller;
      //m_tfmFileManager = new TxFileManager();
    }

    #endregion

    #region UI

    #region MessageBox

    /// <summary>
    /// Shows a message box with the given message.
    /// </summary>
    /// <param name="p_strMessage">The message to display in the message box.</param>
    public void MessageBox(string p_strMessage)
    {
      PermissionsManager.CurrentPermissions.Assert();
      System.Windows.Forms.MessageBox.Show(p_strMessage);
    }

    /// <summary>
    /// Shows a message box with the given message and title.
    /// </summary>
    /// <param name="p_strMessage">The message to display in the message box.</param>
    /// <param name="p_strTitle">The message box's title, display in the title bar.</param>
    public void MessageBox(string p_strMessage, string p_strTitle)
    {
      PermissionsManager.CurrentPermissions.Assert();
      System.Windows.Forms.MessageBox.Show(p_strMessage, p_strTitle);
    }

    /// <summary>
    /// Shows a message box with the given message, title, and buttons.
    /// </summary>
    /// <param name="p_strMessage">The message to display in the message box.</param>
    /// <param name="p_strTitle">The message box's title, display in the title bar.</param>
    /// <param name="p_mbbButtons">The buttons to show in the message box.</param>
    public DialogResult MessageBox(string p_strMessage, string p_strTitle, MessageBoxButtons p_mbbButtons)
    {
      PermissionsManager.CurrentPermissions.Assert();
      return System.Windows.Forms.MessageBox.Show(p_strMessage, p_strTitle, p_mbbButtons);
    }

    #endregion

    /// <summary>
    /// Displays a selection form to the user.
    /// </summary>
    /// <remarks>
    /// The items, previews, and descriptions are repectively ordered. In other words,
    /// the i-th item in <paramref name="p_strItems"/> uses the i-th preview in
    /// <paramref name="p_strPreviews"/> and the i-th description in <paramref name="p_strDescriptions"/>.
    /// 
    /// Similarly, the idices return as results correspond to the indices of the items in
    /// <paramref name="p_strItems"/>.
    /// </remarks>
    /// <param name="p_strItems">The items from which to select.</param>
    /// <param name="p_strPreviews">The preview image file names for the items.</param>
    /// <param name="p_strDescriptions">The descriptions of the items.</param>
    /// <param name="p_strTitle">The title of the selection form.</param>
    /// <param name="p_booSelectMany">Whether more than one item can be selected.</param>
    /// <returns>The indices of the selected items.</returns>
    public int[] Select(string[] p_strItems, string[] p_strPreviews, string[] p_strDescriptions, string p_strTitle,
                        bool p_booSelectMany)
    {
      PermissionsManager.CurrentPermissions.Assert();
      Image[] imgPreviews = null;
      if (p_strPreviews != null)
      {
        imgPreviews = new Image[p_strPreviews.Length];
        int intMissingImages = 0;
        for (int i = 0; i < p_strPreviews.Length; i++)
        {
          if (p_strPreviews[i] == null)
          {
            continue;
          }
          try
          {
            imgPreviews[i] = Fomod.GetImage(p_strPreviews[i]);
          }
          catch (Exception e)
          {
            if ((e is FileNotFoundException) || (e is DecompressionException))
            {
              intMissingImages++;
            }
            else
            {
              throw e;
            }
          }
        }
        //for now I don't think the user needs to be able to detect this.
        // i don't think it is severe enough to be an exception, as it may be
        // intentional, and if it is a bug it should be readily apparent
        // during testing.
        /*if (intMissingImages > 0)
        {
          m_strLastError = "There were " + intMissingImages + " filenames specified for preview images which could not be loaded";
        }*/
      }
      SelectForm sfmSelectForm = new SelectForm(p_strItems, p_strTitle, p_booSelectMany, imgPreviews, p_strDescriptions);
      sfmSelectForm.ShowDialog();
      int[] intResults = new int[sfmSelectForm.SelectedIndex.Length];
      for (int i = 0; i < sfmSelectForm.SelectedIndex.Length; i++)
      {
        intResults[i] = sfmSelectForm.SelectedIndex[i];
      }
      return intResults;
    }

    /// <summary>
    /// Creates a form that can be used in custom mod scripts.
    /// </summary>
    /// <returns>A form that can be used in custom mod scripts.</returns>
    public Form CreateCustomForm()
    {
      PermissionsManager.CurrentPermissions.Assert();
      return new Form();
    }

    #endregion

    #region Version Checking

    /// <summary>
    /// Gets the version of FOMM.
    /// </summary>
    /// <returns>The version of FOMM.</returns>
    public Version GetFommVersion()
    {
      return Program.MVersion;
    }

    /// <summary>
    /// Gets the version of Fallout that is installed.
    /// </summary>
    /// <returns>The version of Fallout, or <lang cref="null"/> if Fallout
    /// is not installed.</returns>
    public Version GetGameVersion()
    {
      PermissionsManager.CurrentPermissions.Assert();
      return Program.GameMode.GameVersion;
    }

    #endregion

    #region Plugin Management

    /// <summary>
    /// Gets a list of all installed plugins.
    /// </summary>
    /// <returns>A list of all installed plugins.</returns>
    public string[] GetAllPlugins()
    {
      PermissionsManager.CurrentPermissions.Assert();
      string[] strPlugins = Program.GameMode.PluginManager.OrderedPluginList;
      Int32 intTrimLength =
        Program.GameMode.PluginsPath.Trim(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Length + 1;
      for (int i = 0; i < strPlugins.Length; i++)
      {
        strPlugins[i] = strPlugins[i].Remove(0, intTrimLength);
      }
      return strPlugins;
    }

    #region Plugin Activation Info

    /// <summary>
    /// Retrieves a list of currently active plugins.
    /// </summary>
    /// <returns>A list of currently active plugins.</returns>
    public string[] GetActivePlugins()
    {
      PermissionsManager.CurrentPermissions.Assert();
      string[] strPlugins = Program.GameMode.PluginManager.SortPluginList(ActivePlugins.ToArray());
      Int32 intTrimLength =
        Program.GameMode.PluginsPath.Trim(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Length + 1;
      for (int i = 0; i < strPlugins.Length; i++)
      {
        strPlugins[i] = strPlugins[i].Remove(0, intTrimLength);
      }
      return strPlugins;
    }

    #endregion

    #region Load Order Management

    /// <summary>
    /// Sets the load order of the plugins.
    /// </summary>
    /// <remarks>
    /// Each plugin will be moved from its current index to its indice's position
    /// in <paramref name="p_intPlugins"/>.
    /// </remarks>
    /// <param name="p_intPlugins">The new load order of the plugins. Each entry in this array
    /// contains the current index of a plugin. This array must contain all current indices.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="p_intPlugins"/> does not
    /// contain all current plugins.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if an index in <paramref name="p_intPlugins"/>
    /// is outside the range of current plugins. In other words, it is thrown if an entry in
    /// <paramref name="p_intPlugins"/> refers to a non-existant plugin.</exception>
    public void SetLoadOrder(int[] p_intPlugins)
    {
      string[] strPluginNames = GetAllPlugins();
      if (p_intPlugins.Length != strPluginNames.Length)
      {
        throw new ArgumentException("Length of new load order array was different to the total number of plugins");
      }

      for (int i = 0; i < p_intPlugins.Length; i++)
      {
        if (p_intPlugins[i] < 0 || p_intPlugins[i] >= p_intPlugins.Length)
        {
          throw new IndexOutOfRangeException("A plugin index was out of range");
        }
      }

      PermissionsManager.CurrentPermissions.Assert();
      for (int i = 0; i < strPluginNames.Length; i++)
      {
        Program.GameMode.PluginManager.SetLoadOrder(
          Path.Combine(Program.GameMode.PluginsPath, strPluginNames[p_intPlugins[i]]), i);
      }
    }

    /// <summary>
    /// Moves the specified plugins to the given position in the load order.
    /// </summary>
    /// <remarks>
    /// Note that the order of the given list of plugins is not maintained. They are re-ordered
    /// to be in the same order as they are in the before-operation load order. This, I think,
    /// is somewhat counter-intuitive and may change, though likely not so as to not break
    /// backwards compatibility.
    /// </remarks>
    /// <param name="p_intPlugins">The list of plugins to move to the given position in the
    /// load order. Each entry in this array contains the current index of a plugin.</param>
    /// <param name="p_intPosition">The position in the load order to which to move the specified
    /// plugins.</param>
    public void SetLoadOrder(int[] p_intPlugins, int p_intPosition)
    {
      string[] strPluginNames = GetAllPlugins();
      PermissionsManager.CurrentPermissions.Assert();
      Array.Sort<int>(p_intPlugins);

      Int32 intLoadOrder = 0;
      for (int i = 0; i < p_intPosition; i++)
      {
        if (Array.BinarySearch<int>(p_intPlugins, i) >= 0)
        {
          continue;
        }
        Program.GameMode.PluginManager.SetLoadOrder(Path.Combine(Program.GameMode.PluginsPath, strPluginNames[i]),
                                                    intLoadOrder++);
      }
      for (int i = 0; i < p_intPlugins.Length; i++)
      {
        Program.GameMode.PluginManager.SetLoadOrder(
          Path.Combine(Program.GameMode.PluginsPath, strPluginNames[p_intPlugins[i]]), intLoadOrder++);
      }
      for (int i = p_intPosition; i < strPluginNames.Length; i++)
      {
        if (Array.BinarySearch<int>(p_intPlugins, i) >= 0)
        {
          continue;
        }
        Program.GameMode.PluginManager.SetLoadOrder(Path.Combine(Program.GameMode.PluginsPath, strPluginNames[i]),
                                                    intLoadOrder++);
      }
    }

    #endregion

    #region Plugin Activation

    /// <summary>
    /// Sets the activated status of a plugin (i.e., and esp or esm file).
    /// </summary>
    /// <param name="p_strName">The name of the plugin to activate or deactivate.</param>
    /// <param name="p_booActivate">Whether to activate the plugin.</param>
    /// <exception cref="IllegalFilePathException">Thrown if the given path is not safe.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the given plugin name
    /// is invalid or does not exist.</exception>
    public void SetPluginActivation(string p_strName, bool p_booActivate)
    {
      if (p_strName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
      {
        throw new IllegalFilePathException(p_strName);
      }
      PermissionsManager.CurrentPermissions.Assert();
      string strFullPath = Path.Combine(Program.GameMode.PluginsPath, p_strName);
      if (!File.Exists(strFullPath))
      {
        throw new FileNotFoundException("Plugin does not exist", p_strName);
      }

      if (p_booActivate)
      {
        ActivePlugins.Add(strFullPath);
      }
      else
      {
        ActivePlugins.Remove(strFullPath);
      }
    }

    /// <summary>
    /// Commits the list of active plugins.
    /// </summary>
    public void CommitActivePlugins()
    {
      if (m_setActivePlugins != null)
      {
        Program.GameMode.PluginManager.SetActivePlugins(m_setActivePlugins);
      }
    }

    #endregion

    #endregion

    #region File Management

    #region File Creation

    /// <summary>
    /// Verifies if the given file can be written.
    /// </summary>
    /// <remarks>
    /// This method checks if the given path is valid. If so, and the file does not
    /// exist, the file can be written. If the file does exist, than the user is
    /// asked to overwrite the file.
    /// </remarks>
    /// <param name="p_strPath">The file path, relative to the Data folder, whose writability is to be verified.</param>
    /// <returns><lang cref="true"/> if the location specified by <paramref name="p_strPath"/>
    /// can be written; <lang cref="false"/> otherwise.</returns>
    protected bool TestDoOverwrite(string p_strPath)
    {
      string strDataPath = Path.Combine(Program.GameMode.PluginsPath, p_strPath);
      string strOldMod = InstallLog.Current.GetCurrentFileOwnerName(p_strPath);

      if (!File.Exists(strDataPath))
      {
        return true;
      }
      string strLoweredPath = strDataPath.ToLowerInvariant();
      if (m_lstOverwriteFolders.Contains(Path.GetDirectoryName(strLoweredPath)))
      {
        return true;
      }
      if (m_lstDontOverwriteFolders.Contains(Path.GetDirectoryName(strLoweredPath)))
      {
        return false;
      }
      if (m_booOverwriteAll)
      {
        return true;
      }
      if (m_booDontOverwriteAll)
      {
        return false;
      }
      if (m_lstOverwriteMods.Contains(strOldMod))
      {
        return true;
      }
      if (m_lstDontOverwriteMods.Contains(strOldMod))
      {
        return false;
      }

      string strMessage = null;
      if (strOldMod != null)
      {
        strMessage = String.Format("Data file '{{0}}' has already been installed by '{0}'" + Environment.NewLine +
                                   "Overwrite with this mod's file?", strOldMod);
      }
      else
      {
        strMessage = "Data file '{0}' already exists." + Environment.NewLine +
                     "Overwrite with this mod's file?";
      }
      switch (Overwriteform.ShowDialog(String.Format(strMessage, p_strPath), true, (strOldMod != null)))
      {
        case OverwriteResult.Yes:
          return true;
        case OverwriteResult.No:
          return false;
        case OverwriteResult.NoToAll:
          m_booDontOverwriteAll = true;
          return false;
        case OverwriteResult.YesToAll:
          m_booOverwriteAll = true;
          return true;
        case OverwriteResult.NoToMod:
          m_lstDontOverwriteMods.Add(strOldMod);
          return false;
        case OverwriteResult.YesToMod:
          m_lstOverwriteMods.Add(strOldMod);
          return true;
        case OverwriteResult.NoToFolder:
          Queue<string> folders = new Queue<string>();
          folders.Enqueue(Path.GetDirectoryName(strLoweredPath));
          while (folders.Count > 0)
          {
            strLoweredPath = folders.Dequeue();
            if (!m_lstOverwriteFolders.Contains(strLoweredPath))
            {
              m_lstDontOverwriteFolders.Add(strLoweredPath);
              foreach (string s in Directory.GetDirectories(strLoweredPath))
              {
                folders.Enqueue(s.ToLowerInvariant());
              }
            }
          }
          return false;
        case OverwriteResult.YesToFolder:
          folders = new Queue<string>();
          folders.Enqueue(Path.GetDirectoryName(strLoweredPath));
          while (folders.Count > 0)
          {
            strLoweredPath = folders.Dequeue();
            if (!m_lstDontOverwriteFolders.Contains(strLoweredPath))
            {
              m_lstOverwriteFolders.Add(strLoweredPath);
              foreach (string s in Directory.GetDirectories(strLoweredPath))
              {
                folders.Enqueue(s.ToLowerInvariant());
              }
            }
          }
          return true;
        default:
          throw new Exception(
            "Sanity check failed: OverwriteDialog returned a value not present in the OverwriteResult enum");
      }
    }

    public bool InstallFile(string fnFrom, string fnTo = "")
    {
      bool ret = false;
      string tmpFN;
      string strDataPath;

      /*
       * There were two differences between InstallFileFromFomod and CopyDataFile
       * 1. IFFF checked permissions, CDF did not.
       * 2. IFFF used the same src and dst filenames, CDF used different names.
       */
      PermissionsManager.CurrentPermissions.Assert();

      if (fnTo == "")
      {
        fnTo = fnFrom;
      }

      FileManagement.AssertFilePathIsSafe(fnTo);

      #region refactor me

      #region original code

      //      byte[] bteFomodFile = Fomod.GetFile(fnFrom);
      //      ret = GenerateDataFile(fnTo, bteFomodFile);

      #endregion

      #region newcode

      /*
       * New code
       * 1. Extract the file from the archive to the temp file
       * 2. Copy the temp file to the destination
       * 3. Delete the temp file
       */

      tmpFN = Fomod.ExtractToTemp(fnFrom);
      if (GenerateDataFilePrep(fnTo, out strDataPath))
      {
        Installer.TransactionalFileManager.Copy(tmpFN, strDataPath, true);
        Installer.MergeModule.AddFile(fnTo);
        ret = true;
      }

      #endregion

      #endregion

      return ret;
    }

    /// <summary>
    /// Installs the speified file from the FOMod to the file system.
    /// </summary>
    /// <param name="p_strFile">The path of the file to install.</param>
    /// <returns><lang cref="true"/> if the file was written; <lang cref="false"/> if the user chose
    /// not to overwrite an existing file.</returns>
    public bool InstallFileFromFomod(string p_strFile)
    {
      return InstallFile(p_strFile);
    }

    /// <summary>
    /// Installs the speified file from the FOMod to the specified location on the file system.
    /// </summary>
    /// <param name="p_strFrom">The path of the file in the FOMod to install.</param>
    /// <param name="p_strTo">The path on the file system where the file is to be created.</param>
    /// <returns><lang cref="true"/> if the file was written; <lang cref="false"/> if the user chose
    /// not to overwrite an existing file.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file referenced by
    /// <paramref name="p_strFrom"/> is not in the FOMod.</exception>
    /// <exception cref="IllegalFilePathException">Thrown if <paramref name="p_strTo"/> is
    /// not safe.</exception>
    public bool CopyDataFile(string p_strFrom, string p_strTo)
    {
      return InstallFile(p_strFrom, p_strTo);
    }

    /// <summary>
    /// Writes the file represented by the given byte array to the given path.
    /// </summary>
    /// <remarks>
    /// This method writes the given data as a file at the given path. If the file
    /// already exists the user is prompted to overwrite the file.
    /// </remarks>
    /// <param name="p_strPath">The path where the file is to be created.</param>
    /// <param name="p_bteData">The data that is to make up the file.</param>
    /// <returns><lang cref="true"/> if the file was written; <lang cref="false"/> if the user chose
    /// not to overwrite an existing file.</returns>
    /// <exception cref="IllegalFilePathException">Thrown if <paramref name="p_strPath"/> is
    /// not safe.</exception>
    public virtual bool GenerateDataFile(string p_strPath, byte[] p_bteData)
    {
      PermissionsManager.CurrentPermissions.Assert();
      FileManagement.AssertFilePathIsSafe(p_strPath);
      string strDataPath;
      bool ret = false;

      if (GenerateDataFilePrep(p_strPath, out strDataPath))
      {
        Installer.TransactionalFileManager.WriteAllBytes(strDataPath, p_bteData);
        Installer.MergeModule.AddFile(p_strPath);
        ret = true;
      }

      return ret;
    }

    private bool GenerateDataFilePrep(string p_strPath, out string strDataPath)
    {
      strDataPath = Path.Combine(Program.GameMode.PluginsPath, p_strPath);
      if (!Directory.Exists(Path.GetDirectoryName(strDataPath)))
      {
        Installer.TransactionalFileManager.CreateDirectory(Path.GetDirectoryName(strDataPath));
      }
      else
      {
        if (!TestDoOverwrite(p_strPath))
        {
          return false;
        }

        if (File.Exists(strDataPath))
        {
          string strDirectory = Path.GetDirectoryName(p_strPath);
          string strBackupPath = Path.Combine(Program.GameMode.OverwriteDirectory, strDirectory);
          string strOldModKey = InstallLog.Current.GetCurrentFileOwnerKey(p_strPath);
          //if this mod installed a file, and now we are overwriting itm
          // the install log will tell us no one owns the file, or the wrong mod owns the
          // file. so, if this mod has installed this file already just replace it, don't
          // back it up.
          if (!Installer.MergeModule.ContainsFile(p_strPath))
          {
            if (!Directory.Exists(strBackupPath))
            {
              Installer.TransactionalFileManager.CreateDirectory(strBackupPath);
            }

            //if we are overwriting an original value, back it up
            if (strOldModKey == null)
            {
              Installer.MergeModule.BackupOriginalDataFile(p_strPath);
              strOldModKey = InstallLog.Current.OriginalValuesKey;
            }
            string strFile =
              Path.GetFileName(Directory.GetFiles(Path.GetDirectoryName(strDataPath), Path.GetFileName(strDataPath))[0]);
            strFile = strOldModKey + "_" + strFile;

            strBackupPath = Path.Combine(strBackupPath, strFile);
            Installer.TransactionalFileManager.Copy(strDataPath, strBackupPath, true);
          }
          Installer.TransactionalFileManager.Delete(strDataPath);
        }
      }
      return true;
    }

    #endregion

    #region File Removal

    /// <summary>
    /// Uninstalls the specified file.
    /// </summary>
    /// <remarks>
    /// If the mod we are uninstalling doesn't own the file, then its version is removed
    /// from the overwrites directory. If the mod we are uninstalling overwrote a file when it
    /// installed the specified file, then the overwritten file is restored. Otherwise
    /// the file is deleted.
    /// </remarks>
    /// <param name="p_strPath">The path to the file that is to be uninstalled.</param>
    /// <seealso cref="UninstallDataFile(string p_strFomodBaseName, string p_strFile)"/>
    public void UninstallDataFile(string p_strFile)
    {
      UninstallDataFile(Fomod.BaseName, p_strFile);
    }

    /// <summary>
    /// Uninstalls the specified file.
    /// </summary>
    /// <remarks>
    /// If the mod we are uninstalling doesn't own the file, then its version is removed
    /// from the overwrites directory. If the mod we are uninstalling overwrote a file when it
    /// installed the specified file, then the overwritten file is restored. Otherwise
    /// the file is deleted.
    /// 
    /// This variant of <see cref="UninstallDataFile"/> is for use when uninstalling a file
    /// for a mod whose FOMod is missing.
    /// </remarks>
    /// <param name="p_strFomodBaseName">The base name of the <see cref="fomod"/> whose file
    /// is being uninstalled.</param>
    /// <param name="p_strPath">The path to the file that is to be uninstalled.</param>
    /// <seealso cref="UninstallDataFile(string p_strFile)"/>
    public void UninstallDataFile(string p_strFomodBaseName, string p_strFile)
    {
      PermissionsManager.CurrentPermissions.Assert();
      FileManagement.AssertFilePathIsSafe(p_strFile);
      string strDataPath = Path.Combine(Program.GameMode.PluginsPath, p_strFile);
      string strKey = InstallLog.Current.GetModKey(p_strFomodBaseName);
      string strDirectory = Path.GetDirectoryName(p_strFile);
      string strBackupDirectory = Path.Combine(Program.GameMode.OverwriteDirectory, strDirectory);
      if (File.Exists(strDataPath))
      {
        string strCurrentOwnerKey = InstallLog.Current.GetCurrentFileOwnerKey(p_strFile);
        //if we didn't install the file, then leave it alone
        if (strKey.Equals(strCurrentOwnerKey))
        {
          //if we did install the file, replace it with the file we overwrote
          // if we didn't overwrite a file, then just delete it
          Installer.TransactionalFileManager.Delete(strDataPath);

          string strPreviousOwnerKey = InstallLog.Current.GetPreviousFileOwnerKey(p_strFile);
          if (strPreviousOwnerKey != null)
          {
            string strFile = strPreviousOwnerKey + "_" + Path.GetFileName(p_strFile);
            string strRestoreFromPath = Path.Combine(strBackupDirectory, strFile);
            if (File.Exists(strRestoreFromPath))
            {
              string strBackupFileName =
                Path.GetFileName(
                  Directory.GetFiles(Path.GetDirectoryName(strRestoreFromPath), Path.GetFileName(strRestoreFromPath))[0]);
              string strCasedFileName = strBackupFileName.Substring(strBackupFileName.IndexOf('_') + 1);
              string strNewDataPath = Path.Combine(Path.GetDirectoryName(strDataPath), strCasedFileName);
              Installer.TransactionalFileManager.Copy(strRestoreFromPath, strNewDataPath, true);
              Installer.TransactionalFileManager.Delete(strRestoreFromPath);
            }

            //remove anny empty directories from the overwrite folder we may have created
            string strStopDirectory = Program.GameMode.OverwriteDirectory;
            strStopDirectory = strStopDirectory.Remove(0, strStopDirectory.LastIndexOfAny(new char[]
            {
              Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar
            }));
            TrimEmptyDirectories(strRestoreFromPath, strStopDirectory);
          }
          else
          {
            //remove any empty directories from the data folder we may have created
            string strStopDirectory = Program.GameMode.PluginsPath;
            strStopDirectory = strStopDirectory.Remove(0, strStopDirectory.LastIndexOfAny(new char[]
            {
              Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar
            }));
            TrimEmptyDirectories(strDataPath, strStopDirectory);
          }
        }
      }

      //remove our version of the file from the backup directory
      string strOverwriteFile = strKey + "_" + Path.GetFileName(p_strFile);
      string strOverwritePath = Path.Combine(strBackupDirectory, strOverwriteFile);
      if (File.Exists(strOverwritePath))
      {
        Installer.TransactionalFileManager.Delete(strOverwritePath);
        //remove anny empty directories from the overwrite folder we may have created
        TrimEmptyDirectories(strOverwritePath, Program.GameMode.OverwriteDirectory);
      }
    }

    /// <summary>
    /// Deletes any empty directories found between the start path and the end directory.
    /// </summary>
    /// <param name="p_strStartPath">The path from which to start looking for empty directories.</param>
    /// <param name="p_strStopDirectory">The directory at which to stop looking.</param>
    protected void TrimEmptyDirectories(string p_strStartPath, string p_strStopDirectory)
    {
      string strEmptyDirectory = Path.GetDirectoryName(p_strStartPath).ToLowerInvariant();
      if (!Directory.Exists(strEmptyDirectory))
      {
        return;
      }
      while (true)
      {
        if ((Directory.GetFiles(strEmptyDirectory).Length + Directory.GetDirectories(strEmptyDirectory).Length == 0) &&
            !strEmptyDirectory.EndsWith(p_strStopDirectory.ToLowerInvariant()))
        {
          Directory.Delete(strEmptyDirectory);
        }
        else
        {
          break;
        }
        strEmptyDirectory = Path.GetDirectoryName(strEmptyDirectory);
      }
    }

    #endregion

    #endregion

    #region Ini Management

    #region Ini File Value Retrieval

    /// <summary>
    /// Retrieves the specified settings value as a string.
    /// </summary>
    /// <param name="p_strSettingsFileName">The name of the settings file from which to retrieve the value.</param>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as a string.</returns>
    protected string GetSettingsString(string p_strSettingsFileName, string p_strSection, string p_strKey)
    {
      PermissionsManager.CurrentPermissions.Assert();
      return NativeMethods.GetPrivateProfileString(p_strSection, p_strKey, null, p_strSettingsFileName);
    }

    /// <summary>
    /// Retrieves the specified settings value as an integer.
    /// </summary>
    /// <param name="p_strSettingsFileName">The name of the settings file from which to retrieve the value.</param>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as an integer.</returns>
    protected Int32 GetSettingsInt(string p_strSettingsFileName, string p_strSection, string p_strKey)
    {
      PermissionsManager.CurrentPermissions.Assert();
      return NativeMethods.GetPrivateProfileIntA(p_strSection, p_strKey, 0, p_strSettingsFileName);
    }

    #endregion

    #region Ini Editing

    /// <summary>
    /// Sets the specified value in the specified Ini file to the given value.
    /// </summary>
    /// <param name="p_strSettingsFileName">The name of the settings file to edit.</param>
    /// <param name="p_strSection">The section in the Ini file to edit.</param>
    /// <param name="p_strKey">The key in the Ini file to edit.</param>
    /// <param name="p_strValue">The value to which to set the key.</param>
    /// <returns><lang cref="true"/> if the value was set; <lang cref="false"/>
    /// if the user chose not to overwrite the existing value.</returns>
    protected virtual bool EditINI(string p_strSettingsFileName, string p_strSection, string p_strKey, string p_strValue)
    {
      string strFile = p_strSettingsFileName;
      if (m_booDontOverwriteAllIni)
      {
        return false;
      }

      PermissionsManager.CurrentPermissions.Assert();
      string strLoweredFile = strFile.ToLowerInvariant();
      string strLoweredSection = p_strSection.ToLowerInvariant();
      string strLoweredKey = p_strKey.ToLowerInvariant();
      string strOldMod = InstallLog.Current.GetCurrentIniEditorModName(strFile, p_strSection, p_strKey);
      string strOldValue = NativeMethods.GetPrivateProfileString(p_strSection, p_strKey, null, strFile);
      if (!m_booOverwriteAllIni)
      {
        string strMessage = null;
        if (strOldMod != null)
        {
          strMessage =
            String.Format("Key '{{0}}' in section '{{1}}' of {{2}}} has already been overwritten by '{0}'\n" +
                          "Overwrite again with this mod?\n" +
                          "Current value '{{3}}', new value '{{4}}'", strOldMod);
        }
        else
        {
          strMessage = "The mod wants to modify key '{0}' in section '{1}' of {2}.\n" +
                       "Allow the change?\n" +
                       "Current value '{3}', new value '{4}'";
        }
        switch (
          Overwriteform.ShowDialog(String.Format(strMessage, p_strKey, p_strSection, strFile, strOldValue, p_strValue),
                                   false, (strOldMod != null)))
        {
          case OverwriteResult.YesToAll:
            m_booOverwriteAllIni = true;
            break;
          case OverwriteResult.NoToAll:
            m_booDontOverwriteAllIni = true;
            break;
          case OverwriteResult.Yes:
            break;
          default:
            return false;
        }
      }

      //if we are overwriting an original value, back it up
      if ((strOldMod == null) || (strOldValue != null))
      {
        Installer.MergeModule.BackupOriginalIniValue(strLoweredFile, strLoweredSection, strLoweredKey, strOldValue);
      }

      NativeMethods.WritePrivateProfileStringA(strLoweredSection, strLoweredKey, p_strValue, strLoweredFile);
      Installer.MergeModule.AddIniEdit(strLoweredFile, strLoweredSection, strLoweredKey, p_strValue);
      return true;
    }

    #endregion

    #region Ini Unediting

    /// <summary>
    /// Undoes the edit made to the spcified key.
    /// </summary>
    /// <param name="p_strSettingsFileName">The name of the settings file to unedit.</param>
    /// <param name="p_strSection">The section in the Ini file to unedit.</param>
    /// <param name="p_strKey">The key in the Ini file to unedit.</param>
    /// <seealso cref="UneditIni(string p_strFomodBaseName, string p_strSettingsFileName, string p_strSection, string p_strKey)"/>
    public void UneditIni(string p_strSettingsFileName, string p_strSection, string p_strKey)
    {
      UneditIni(Fomod.BaseName, p_strSettingsFileName, p_strSection, p_strKey);
    }

    /// <summary>
    /// Undoes the edit made to the spcified key.
    /// </summary>
    /// <remarks>
    ///  This variant of <see cref="UneditIni"/> is for use when uninstalling a file
    /// for a mod whose FOMod is missing.
    /// </remarks>
    /// <param name="p_strFomodBaseName">The base name of the <see cref="fomod"/> whose file
    /// is being uninstalled.</param>
    /// <param name="p_strSettingsFileName">The name of the settings file to unedit.</param>
    /// <param name="p_strSection">The section in the Ini file to unedit.</param>
    /// <param name="p_strKey">The key in the Ini file to unedit.</param>
    /// <seealso cref="UneditIni(string p_strSettingsFileName, string p_strSection, string p_strKey)"/>
    public void UneditIni(string p_strFomodBaseName, string p_strSettingsFileName, string p_strSection, string p_strKey)
    {
      string strLoweredFile = p_strSettingsFileName.ToLowerInvariant();
      string strLoweredSection = p_strSection.ToLowerInvariant();
      string strLoweredKey = p_strKey.ToLowerInvariant();

      string strKey = InstallLog.Current.GetModKey(p_strFomodBaseName);
      string strCurrentOwnerKey = InstallLog.Current.GetCurrentIniEditorModKey(strLoweredFile, strLoweredSection,
                                                                               strLoweredKey);
      //if we didn't edit the value, then leave it alone
      if (!strKey.Equals(strCurrentOwnerKey))
      {
        return;
      }

      //if we did edit the value, replace if with the value we overwrote
      // if we didn't overwrite a value, then just delete it
      string strPreviousValue = InstallLog.Current.GetPreviousIniValue(strLoweredFile, strLoweredSection, strLoweredKey);
      if (strPreviousValue != null)
      {
        PermissionsManager.CurrentPermissions.Assert();
        NativeMethods.WritePrivateProfileStringA(p_strSection, p_strKey, strPreviousValue, strLoweredFile);
      }
      //TODO: how do we remove an Ini key? Right now, if there was no previous value the current value
      // remains
    }

    #endregion

    #endregion

    #region Game-Specific Value Management

    /// <summary>
    /// Undoes the edit made to the spcified game-specific value.
    /// </summary>
    /// <param name="p_strValueKey">The key of the game-specific value to unedit.</param>
    /// <seealso cref="UneditGameSpecificValue(string p_strFomodBaseName, string p_strValueKey)"/>
    public bool UneditGameSpecificValue(string p_strValueKey)
    {
      return UneditGameSpecificValue(Fomod.BaseName, p_strValueKey);
    }

    /// <summary>
    /// Undoes the edit made to the spcified game-specific value.
    /// </summary>
    /// <remarks>
    ///  This variant of <see cref="UneditGameSpecificValue"/> is for use when uninstalling a file
    /// for a mod whose FOMod is missing.
    /// </remarks>
    /// <param name="p_strFomodBaseName">The base name of the <see cref="fomod"/> whose file
    /// is being uninstalled.</param>
    /// <param name="p_strValueKey">The key of the game-specific value to unedit.</param>
    /// <seealso cref="UneditGameSpecificValue(string p_strValueKey)"/>
    public abstract bool UneditGameSpecificValue(string p_strFomodBaseName, string p_strValueKey);

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Cleans up used resources.
    /// </summary>
    public virtual void Dispose()
    {
      m_lstOverwriteFolders.Clear();
      m_lstDontOverwriteFolders.Clear();
      m_booDontOverwriteAll = false;
      m_booOverwriteAll = false;
      m_booDontOverwriteAllIni = false;
      m_booOverwriteAllIni = false;
    }

    #endregion
  }
}