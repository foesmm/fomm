using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Fomm.Games.Fallout3.Tools;
using Fomm.Games.Fallout3.Tools.AutoSorter;
using Fomm.Games.Fallout3.Tools.BSA;
using Fomm.Games.Fallout3.Tools.TESsnip;
using Fomm.PackageManager;
using Fomm.PackageManager.ModInstallLog;
using ScriptCompiler = Fomm.Games.Fallout3.Tools.TESsnip.ScriptCompiler.ScriptCompiler;

namespace Fomm.Games.Fallout3.Script
{
  public class Fallout3ModInstallScript : ModInstallScript
  {
    #region Properties

    /// <summary>
    ///   Gets the <see cref="BsaManager" /> this script is using.
    /// </summary>
    /// <value>The <see cref="BsaManager" /> this script is using.</value>
    public BsaManager BsaManager { get; private set; }

    /// <summary>
    ///   Gets the <see cref="TextureManager" /> this script is using.
    /// </summary>
    /// <value>The <see cref="TextureManager" /> this script is using.</value>
    public TextureManager TextureManager { get; private set; }

    // extender name
    public override String ScriptExtenderName
    {
      get
      {
        return "fose_loader.exe";
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   A simple constructor that initializes the object.
    /// </summary>
    /// <param name="p_fomodMod">The <see cref="fomod" /> against which to run the script.</param>
    public Fallout3ModInstallScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller)
      : base(p_fomodMod, p_mibInstaller)
    {
      //m_misScript = new ModInstallScript(p_fomodMod);
      BsaManager = new BsaManager();
      TextureManager = new TextureManager();
    }

    #endregion

    #region FO3 Script Compilation

    /// <summary>
    ///   Sets up the script compiler for the given plugins.
    /// </summary>
    /// <param name="p_plgPlugins">The plugins for which to set up the script compiler.</param>
    public void SetupScriptCompiler(Plugin[] p_plgPlugins)
    {
      PermissionsManager.CurrentPermissions.Assert();
      ScriptCompiler.Setup(p_plgPlugins);
    }

    /// <summary>
    ///   Compiles the result script.
    /// </summary>
    public void CompileResultScript(SubRecord sr, out Record r2, out string msg)
    {
      ScriptCompiler.CompileResultScript(sr, out r2, out msg);
    }

    /// <summary>
    ///   Compiles a script.
    /// </summary>
    public void CompileScript(Record r2, out string msg)
    {
      ScriptCompiler.Compile(r2, out msg);
    }

    #endregion

    #region Version Checking

    /// <summary>
    ///   Gets the version of GECK that is installed.
    /// </summary>
    /// <returns>
    ///   The version of GECK, or <lang langref="null" /> if GECK
    ///   is not installed.
    /// </returns>
    public Version GetGeckVersion()
    {
      PermissionsManager.CurrentPermissions.Assert();
      if (!File.Exists("geck.exe"))
      {
        return null;
      }
      return new Version(FileVersionInfo.GetVersionInfo("geck.exe").FileVersion.Replace(", ", "."));
    }

    #endregion

    #region Load Order Management

    /// <summary>
    ///   Determines if the plugins have been auto-sorted.
    /// </summary>
    /// <returns>
    ///   <lang langref="true" /> if the plugins have been auto-sorted;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public bool IsLoadOrderAutoSorted()
    {
      PermissionsManager.CurrentPermissions.Assert();
      return new LoadOrderSorter().CheckList(GetAllPlugins());
    }

    /// <summary>
    ///   Determins where in the load order the specified plugin would be inserted
    ///   if the plugins were auto-sorted.
    /// </summary>
    /// <param name="p_strPlugin">
    ///   The name of the plugin whose auto-sort insertion
    ///   point is to be determined.
    /// </param>
    /// <returns>
    ///   The index where the specified plugin would be inserted were the
    ///   plugins to be auto-sorted.
    /// </returns>
    public int GetAutoInsertionPoint(string p_strPlugin)
    {
      PermissionsManager.CurrentPermissions.Assert();
      return new LoadOrderSorter().GetInsertionPos(GetAllPlugins(), p_strPlugin);
    }

    /// <summary>
    ///   Auto-sorts the specified plugins.
    /// </summary>
    /// <remarks>
    ///   This is, apparently, a beta function. Use with caution.
    /// </remarks>
    /// <param name="p_strPlugins">The list of plugins to auto-sort.</param>
    public void AutoSortPlugins(string[] p_strPlugins)
    {
      PermissionsManager.CurrentPermissions.Assert();
      new LoadOrderSorter().SortList(p_strPlugins);
    }

    #endregion

    #region Ini Management

    #region Ini File Value Retrieval

    /// <summary>
    ///   Retrieves the specified Fallout.ini value as a string.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as a string.</returns>
    public virtual string GetFalloutIniString(string p_strSection, string p_strKey)
    {
      return GetSettingsString(((Fallout3GameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).FOIniPath,
                               p_strSection, p_strKey);
    }

    /// <summary>
    ///   Retrieves the specified Fallout.ini value as an integer.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as an integer.</returns>
    public virtual int GetFalloutIniInt(string p_strSection, string p_strKey)
    {
      return GetSettingsInt(((Fallout3GameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).FOIniPath, p_strSection,
                            p_strKey);
    }

    /// <summary>
    ///   Retrieves the specified FalloutPrefs.ini value as a string.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as a string.</returns>
    public virtual string GetPrefsIniString(string p_strSection, string p_strKey)
    {
      return GetSettingsString(((Fallout3GameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).FOPrefsIniPath,
                               p_strSection, p_strKey);
    }

    /// <summary>
    ///   Retrieves the specified FalloutPrefs.ini value as an integer.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as an integer.</returns>
    public virtual int GetPrefsIniInt(string p_strSection, string p_strKey)
    {
      return GetSettingsInt(((Fallout3GameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).FOPrefsIniPath,
                            p_strSection, p_strKey);
    }

    /// <summary>
    ///   Retrieves the specified GECKCustom.ini value as a string.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as a string.</returns>
    public virtual string GetGeckIniString(string p_strSection, string p_strKey)
    {
      return GetSettingsString(((Fallout3GameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).GeckIniPath,
                               p_strSection, p_strKey);
    }

    /// <summary>
    ///   Retrieves the specified GECKCustom.ini value as an integer.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as an integer.</returns>
    public virtual int GetGeckIniInt(string p_strSection, string p_strKey)
    {
      return GetSettingsInt(((Fallout3GameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).GeckIniPath,
                            p_strSection, p_strKey);
    }

    /// <summary>
    ///   Retrieves the specified GECKPrefs.ini value as a string.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as a string.</returns>
    public virtual string GetGeckPrefsIniString(string p_strSection, string p_strKey)
    {
      return GetSettingsString(((Fallout3GameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).GeckPrefsIniPath,
                               p_strSection, p_strKey);
    }

    /// <summary>
    ///   Retrieves the specified GECKPrefs.ini value as an integer.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as an integer.</returns>
    public virtual int GetGeckPrefsIniInt(string p_strSection, string p_strKey)
    {
      return GetSettingsInt(((Fallout3GameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).GeckPrefsIniPath,
                            p_strSection, p_strKey);
    }

    #endregion

    #region Ini Editing

    /// <summary>
    ///   Sets the specified value in the Fallout.ini file to the given value.
    /// </summary>
    /// <param name="p_strSection">The section in the Ini file to edit.</param>
    /// <param name="p_strKey">The key in the Ini file to edit.</param>
    /// <param name="p_strValue">The value to which to set the key.</param>
    /// <param name="p_booSaveOld">Not used.</param>
    /// <returns>
    ///   <lang langref="true" /> if the value was set; <lang langref="false" />
    ///   if the user chose not to overwrite the existing value.
    /// </returns>
    public virtual bool EditFalloutINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
    {
      return EditINI(((Fallout3GameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).FOIniPath, p_strSection,
                     p_strKey, p_strValue);
    }

    /// <summary>
    ///   Sets the specified value in the FalloutPrefs.ini file to the given value.
    /// </summary>
    /// <param name="p_strSection">The section in the Ini file to edit.</param>
    /// <param name="p_strKey">The key in the Ini file to edit.</param>
    /// <param name="p_strValue">The value to which to set the key.</param>
    /// <param name="p_booSaveOld">Not used.</param>
    /// <returns>
    ///   <lang langref="true" /> if the value was set; <lang langref="false" />
    ///   if the user chose not to overwrite the existing value.
    /// </returns>
    public virtual bool EditPrefsINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
    {
      return EditINI(((Fallout3GameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).FOPrefsIniPath, p_strSection,
                     p_strKey, p_strValue);
    }

    /// <summary>
    ///   Sets the specified value in the GECKCustom.ini file to the given value.
    /// </summary>
    /// <param name="p_strSection">The section in the Ini file to edit.</param>
    /// <param name="p_strKey">The key in the Ini file to edit.</param>
    /// <param name="p_strValue">The value to which to set the key.</param>
    /// <param name="p_booSaveOld">Not used.</param>
    /// <returns>
    ///   <lang langref="true" /> if the value was set; <lang langref="false" />
    ///   if the user chose not to overwrite the existing value.
    /// </returns>
    public virtual bool EditGeckINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
    {
      return EditINI(((Fallout3GameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).GeckIniPath, p_strSection,
                     p_strKey, p_strValue);
    }

    /// <summary>
    ///   Sets the specified value in the GECKPrefs.ini file to the given value.
    /// </summary>
    /// <param name="p_strSection">The section in the Ini file to edit.</param>
    /// <param name="p_strKey">The key in the Ini file to edit.</param>
    /// <param name="p_strValue">The value to which to set the key.</param>
    /// <param name="p_booSaveOld">Not used.</param>
    /// <returns>
    ///   <lang langref="true" /> if the value was set; <lang langref="false" />
    ///   if the user chose not to overwrite the existing value.
    /// </returns>
    public virtual bool EditGeckPrefsINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
    {
      return EditINI(((Fallout3GameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).GeckPrefsIniPath, p_strSection,
                     p_strKey, p_strValue);
    }

    #endregion

    #endregion

    #region Game-Specific Value Management

    /// <summary>
    ///   Undoes the edit made to the spcified game-specific value.
    /// </summary>
    /// <param name="p_strFomodBaseName">
    ///   The base name of the <see cref="fomod" /> whose file
    ///   is being uninstalled.
    /// </param>
    /// <param name="p_strValueKey">The key of the game-specific value to unedit.</param>
    public override bool UneditGameSpecificValue(string p_strFomodBaseName, string p_strValueKey)
    {
      var strKey = p_strValueKey.Split(new[]
      {
        ':'
      }, 2);
      switch (strKey[0])
      {
        case "sdp":
          var strShaderInfo = strKey[1].Split('/');
          UneditShader(Int32.Parse(strShaderInfo[0]), strShaderInfo[1]);
          return true;
      }
      return false;
    }

    #endregion

    #region Shader Management

    #region Shader Editing

    /// <summary>
    ///   Edits the specified shader with the specified data.
    /// </summary>
    /// <param name="p_intPackage">The package containing the shader to edit.</param>
    /// <param name="p_strShaderName">The shader to edit.</param>
    /// <param name="p_bteData">The value to which to edit the shader.</param>
    /// <returns>
    ///   <lang langref="true" /> if the value was set; <lang langref="false" />
    ///   if the user chose not to overwrite the existing value.
    /// </returns>
    /// <exception cref="ShaderException">Thrown if the shader could not be edited.</exception>
    public virtual bool EditShader(int p_intPackage, string p_strShaderName, byte[] p_bteData)
    {
      var strShaderKey = String.Format("sdp:{0}/{1}", p_intPackage, p_strShaderName);
      var strOldMod = InstallLog.Current.GetCurrentGameSpecifcValueEditorModName(strShaderKey);
      if (strOldMod != null)
      {
        var strMessage = String.Format("Shader '{0}' in package '{1}' has already been overwritten by '{2}'\n" +
                                       "Overwrite the changes?", p_strShaderName, p_intPackage, strOldMod);
        if (
          System.Windows.Forms.MessageBox.Show(strMessage, "Confirm Overwrite", MessageBoxButtons.YesNo,
                                               MessageBoxIcon.Question) != DialogResult.Yes)
        {
          return false;
        }
      }

      PermissionsManager.CurrentPermissions.Assert();
      byte[] oldData;
      if (!SDPArchives.EditShader(p_intPackage, p_strShaderName, p_bteData, out oldData))
      {
        throw new ShaderException("Failed to edit the shader");
      }

      //if we are overwriting an original shader, back it up
      if ((strOldMod == null) || (oldData != null))
      {
        Installer.MergeModule.BackupOriginalGameSpecificValueEdit(strShaderKey, oldData);
      }

      Installer.MergeModule.AddGameSpecificValueEdit(strShaderKey, p_bteData);
      return true;
    }

    #endregion

    #region Shader Unediting

    /// <summary>
    ///   Undoes the edit made to the specified shader.
    /// </summary>
    /// <param name="p_intPackage">The package containing the shader to edit.</param>
    /// <param name="p_strShaderName">The shader to edit.</param>
    /// <exception cref="ShaderException">Thrown if the shader could not be unedited.</exception>
    protected void UneditShader(int p_intPackage, string p_strShaderName)
    {
      var strShaderKey = String.Format("sdp:{0}/{1}", p_intPackage, p_strShaderName);
      var strKey = InstallLog.Current.GetModKey(Fomod.BaseName);
      var strCurrentOwnerKey = InstallLog.Current.GetCurrentGameSpecifcValueEditorModKey(strShaderKey);
      //if we didn't edit the shader, then leave it alone
      if (!strKey.Equals(strCurrentOwnerKey))
      {
        return;
      }

      //if we did edit the shader, replace it with the shader we overwrote
      // if we didn't overwrite the shader, then just delete it
      var btePreviousData = InstallLog.Current.GetPreviousGameSpecifcValueData(strShaderKey);
      if (btePreviousData != null)
      {
        /*TODO: I'm not sure if this is the strictly correct way to unedit a shader
         * the original unedit code was:
         * 
         *  if (m_xelModInstallLogSdpEdits != null)
         *  {
         *    foreach (XmlNode node in m_xelModInstallLogSdpEdits.ChildNodes)
         *    {
         *      //TODO: Remove this workaround for the release version
         *      if (node.Attributes.GetNamedItem("crc") == null)
         *      {
         *        InstallLog.UndoShaderEdit(int.Parse(node.Attributes.GetNamedItem("package").Value), node.Attributes.GetNamedItem("shader").Value, 0);
         *      }
         *      else
         *      {
         *        InstallLog.UndoShaderEdit(int.Parse(node.Attributes.GetNamedItem("package").Value), node.Attributes.GetNamedItem("shader").Value,
         *          uint.Parse(node.Attributes.GetNamedItem("crc").Value));
         *      }
         *    }
         *  }
         *  
         * where InstallLog.UndoShaderEdit was:
         * 
         *  public void UndoShaderEdit(int package, string shader, uint crc)
         *  {
         *    XmlNode node = sdpEditsNode.SelectSingleNode("sdp[@package='" + package + "' and @shader='" + shader + "']");
         *    if (node == null) return;
         *    byte[] b = new byte[node.InnerText.Length / 2];
         *    for (int i = 0; i < b.Length; i++)
         *    {
         *      b[i] = byte.Parse("" + node.InnerText[i * 2] + node.InnerText[i * 2 + 1], System.Globalization.NumberStyles.AllowHexSpecifier);
         *    }
         *    if (SDPArchives.RestoreShader(package, shader, b, crc)) sdpEditsNode.RemoveChild(node);
         *  }
         *  
         * after looking at SDPArchives it is not clear to me why a crc was being used.
         * if ever it becomes evident that a crc is required, I will have to alter the log to store
         *  a crc and pass it to the RestoreShader method.
         */

        PermissionsManager.CurrentPermissions.Assert();
        if (!SDPArchives.RestoreShader(p_intPackage, p_strShaderName, btePreviousData, 0))
        {
          throw new ShaderException("Failed to unedit the shader");
        }
      }
      //TODO: how do we delete a shader? Right now, if there was no previous shader the current shader
      // remains
    }

    #endregion

    #endregion

    #region Misc Info

    /// <summary>
    ///   Gets the specified value from the RendererInfo.txt file.
    /// </summary>
    /// <param name="p_strValue">The value to retrieve from the file.</param>
    /// <returns>
    ///   The specified value from the RendererInfo.txt file, or
    ///   <lang langref="null" /> if the value is not found.
    /// </returns>
    public virtual string GetRendererInfo(string p_strValue)
    {
      PermissionsManager.CurrentPermissions.Assert();
      var strLines = File.ReadAllLines(((Fallout3GameMode) Program.GameMode).FORendererFile);
      for (var i = 1; i < strLines.Length; i++)
      {
        if (!strLines[i].Contains(":"))
        {
          continue;
        }
        var strCurrentValue = strLines[i].Remove(strLines[i].IndexOf(':')).Trim();
        if (strCurrentValue.Equals(p_strValue))
        {
          return strLines[i].Substring(strLines[i].IndexOf(':') + 1).Trim();
        }
      }
      return null;
    }

    /// <summary>
    ///   Determines if archive invalidation is active.
    /// </summary>
    /// <returns>
    ///   <lang langref="true" /> if archive invalidation is active;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public virtual bool IsAIActive()
    {
      return ArchiveInvalidation.IsActive();
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    ///   Cleans up used resources.
    /// </summary>
    public override void Dispose()
    {
      if (TextureManager != null)
      {
        TextureManager.Dispose();
      }
      if (BsaManager != null)
      {
        BsaManager.Dispose();
      }
      base.Dispose();
    }

    #endregion

    #region upgrade functions

    /// <summary>
    ///   Writes the file represented by the given byte array to the given path.
    /// </summary>
    /// <remarks>
    ///   This method writes the given data as a file at the given path, if it is owned
    ///   by the fomod being upgraded. If the specified data file is not owned by the fomod
    ///   being upgraded, the file is instead written to the overwrites directory.
    ///   If the file was not previously installed by the fomod, then the normal install rules apply,
    ///   including confirming overwrite if applicable.
    /// </remarks>
    /// <param name="p_strPath">The path where the file is to be created.</param>
    /// <param name="p_bteData">The data that is to make up the file.</param>
    /// <returns>
    ///   <lang langref="true" /> if the file was written; <lang langref="false" /> if the user chose
    ///   not to overwrite an existing file.
    /// </returns>
    /// <exception cref="IllegalFilePathException">
    ///   Thrown if <paramref name="p_strPath" /> is
    ///   not safe.
    /// </exception>
    public override bool GenerateDataFile(string p_strPath, byte[] p_bteData)
    {
      PermissionsManager.CurrentPermissions.Assert();
      FileManagement.AssertFilePathIsSafe(p_strPath);

      IList<string> lstInstallers = InstallLog.Current.GetInstallingMods(p_strPath);
      if (lstInstallers.Contains(Fomod.BaseName))
      {
        string strWritePath;
        if (!lstInstallers[lstInstallers.Count - 1].Equals(Fomod.BaseName))
        {
          var strDirectory = Path.GetDirectoryName(p_strPath);
          var strBackupPath = Path.Combine(Program.GameMode.OverwriteDirectory, strDirectory);
          var strOldModKey = InstallLog.Current.GetModKey(Fomod.BaseName);
          var strFile = strOldModKey + "_" + Path.GetFileName(p_strPath);
          strWritePath = Path.Combine(strBackupPath, strFile);
        }
        else
        {
          strWritePath = Path.Combine(Program.GameMode.PluginsPath, p_strPath);
        }
        Installer.TransactionalFileManager.WriteAllBytes(strWritePath, p_bteData);
        Installer.MergeModule.AddFile(p_strPath);
        return true;
      }

      return base.GenerateDataFile(p_strPath, p_bteData);
    }

    #endregion
  }
}