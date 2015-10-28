using System;
using Fomm.Games.Fallout3.Script;
using Fomm.Games.Fallout3.Tools.TESsnip;
using System.Drawing;
using Fomm.PackageManager;

namespace fomm.Scripting
{
  /// <summary>
  /// The base class for custom Fallout3 install scripts.
  /// </summary>
  public abstract class Fallout3BaseScript : GenericBaseScript
  {
    internal static Fallout3ModInstallScript Script
    {
      get
      {
        return (Fallout3ModInstallScript) Installer.Script;
      }
    }

    #region Version Checking

    /// <summary>
    /// Indicates whether or not FOSE is present.
    /// </summary>
    /// <returns><see langword="true"/> if FOSE is installed; <see langword="false"/> otherwise.</returns>
    /// <seealso cref="ModScript.ScriptExtenderPresent()"/>
    public static bool ScriptExtenderPresent()
    {
      return (bool) (ExecuteMethod(() => Script.ScriptExtenderPresent()) ?? false);
    }

    /// <summary>
    /// Gets the version of FOSE that is installed.
    /// </summary>
    /// <returns>The <see cref="Version"/> of FOSE that is installed, or <lang langword="null"/> if FOSE
    /// is not installed.</returns>
    /// <seealso cref="ModScript.GetFoseVersion()"/>
    public static Version GetFoseVersion()
    {
      return (Version) ExecuteMethod(() => Script.GetScriptExtenderVersion());
    }

    /// <summary>
    /// Gets the version of Fallout that is installed.
    /// </summary>
    /// <returns>The version of Fallout, or <lang langword="null"/> if Fallout
    /// is not installed.</returns>
    /// <seealso cref="ModScript.GetFalloutVersion()"/>
    public static Version GetFalloutVersion()
    {
      return GetGameVersion();
    }

    /// <summary>
    /// Gets the version of GECK that is installed.
    /// </summary>
    /// <returns>The version of GECK, or <lang langword="null"/> if GECK
    /// is not installed.</returns>
    /// <seealso cref="ModScript.GetGeckVersion()"/>
    public static Version GetGeckVersion()
    {
      return (Version) ExecuteMethod(() => Script.GetGeckVersion());
    }

    #endregion

    #region BSA Management

    /// <summary>
    /// Retrieves the list of files in the specified BSA.
    /// </summary>
    /// <param name="p_strBsa">The BSA whose file listing is requested.</param>
    /// <returns>The list of files contained in the specified BSA.</returns>
    /// <seealso cref="BsaManager.GetBSAFileList(string)"/>
    public static string[] GetBSAFileList(string p_strBsa)
    {
      return (string[]) ExecuteMethod(() => Script.BsaManager.GetBSAFileList(p_strBsa));
    }

    /// <summary>
    /// Gets the specified file from the specified BSA.
    /// </summary>
    /// <param name="p_strBsa">The BSA from which to extract the specified file.</param>
    /// <param name="p_strFile">The files to extract form the specified BSA.</param>
    /// <returns>The data of the specified file.</returns>
    /// <seealso cref="BsaManager.GetDataFileFromBSA(string, string)"/>
    public static byte[] GetDataFileFromBSA(string p_strBsa, string p_strFile)
    {
      return (byte[]) ExecuteMethod(() => Script.BsaManager.GetDataFileFromBSA(p_strBsa, p_strFile));
    }

    #endregion

    #region Ini File Value Management

    #region Ini File Value Retrieval

    /// <summary>
    /// Retrieves the specified Fallout.ini value as a string.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as a string.</returns>
    /// <seealso cref="ModScript.GetFalloutIniString(string, string)"/>
    public static string GetFalloutIniString(string p_strSection, string p_strKey)
    {
      return (string) ExecuteMethod(() => Script.GetFalloutIniString(p_strSection, p_strKey));
    }

    /// <summary>
    /// Retrieves the specified Fallout.ini value as an integer.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <seealso cref="ModScript.GetFalloutIniInt(string, string)"/>
    public static int GetFalloutIniInt(string p_strSection, string p_strKey)
    {
      return (int) ExecuteMethod(() => Script.GetFalloutIniInt(p_strSection, p_strKey));
    }

    /// <summary>
    /// Retrieves the specified FalloutPrefs.ini value as a string.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as a string.</returns>
    /// <seealso cref="ModScript.GetPrefsIniString(string, string)"/>
    public static string GetPrefsIniString(string p_strSection, string p_strKey)
    {
      return (string) ExecuteMethod(() => Script.GetPrefsIniString(p_strSection, p_strKey));
    }

    /// <summary>
    /// Retrieves the specified FalloutPrefs.ini value as an integer.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as an integer.</returns>
    /// <seealso cref="ModScript.GetPrefsIniInt(string, string)"/>
    public static int GetPrefsIniInt(string p_strSection, string p_strKey)
    {
      return (int) ExecuteMethod(() => Script.GetPrefsIniInt(p_strSection, p_strKey));
    }

    /// <summary>
    /// Retrieves the specified GECKCustom.ini value as a string.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as a string.</returns>
    /// <seealso cref="ModScript.GetGeckIniString(string, string)"/>
    public static string GetGeckIniString(string p_strSection, string p_strKey)
    {
      return (string) ExecuteMethod(() => Script.GetGeckIniString(p_strSection, p_strKey));
    }

    /// <summary>
    /// Retrieves the specified GECKCustom.ini value as an integer.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as an integer.</returns>
    /// <seealso cref="ModScript.GetGeckIniInt(string, string)"/>
    public static int GetGeckIniInt(string p_strSection, string p_strKey)
    {
      return (int) ExecuteMethod(() => Script.GetGeckIniInt(p_strSection, p_strKey));
    }

    /// <summary>
    /// Retrieves the specified GECKPrefs.ini value as a string.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as a string.</returns>
    /// <seealso cref="ModScript.GetGeckPrefsIniString(string, string)"/>
    public static string GetGeckPrefsIniString(string p_strSection, string p_strKey)
    {
      return (string) ExecuteMethod(() => Script.GetGeckPrefsIniString(p_strSection, p_strKey));
    }

    /// <summary>
    /// Retrieves the specified GECKPrefs.ini value as an integer.
    /// </summary>
    /// <param name="p_strSection">The section containing the value to retrieve.</param>
    /// <param name="p_strKey">The key of the value to retrieve.</param>
    /// <returns>The specified value as an integer.</returns>
    /// <seealso cref="ModScript.GetGeckPrefsIniInt(string, string)"/>
    public static int GetGeckPrefsIniInt(string p_strSection, string p_strKey)
    {
      return (int) ExecuteMethod(() => Script.GetGeckPrefsIniInt(p_strSection, p_strKey));
    }

    #endregion

    #region Ini Editing

    /// <summary>
    /// Sets the specified value in the Fallout.ini file to the given value. 
    /// </summary>
    /// <param name="p_strSection">The section in the Ini file to edit.</param>
    /// <param name="p_strKey">The key in the Ini file to edit.</param>
    /// <param name="p_strValue">The value to which to set the key.</param>
    /// <param name="p_booSaveOld">Not used.</param>
    /// <returns><lang langref="true"/> if the value was set; <lang langref="false"/>
    /// if the user chose not to overwrite the existing value.</returns>
    /// <seealso cref="ModInstaller.EditFalloutINI(string, string, string, bool)"/>
    public static bool EditFalloutINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
    {
      return
        (bool) (ExecuteMethod(() => Script.EditFalloutINI(p_strSection, p_strKey, p_strValue, p_booSaveOld)) ?? false);
    }

    /// <summary>
    /// Sets the specified value in the FalloutPrefs.ini file to the given value. 
    /// </summary>
    /// <param name="p_strSection">The section in the Ini file to edit.</param>
    /// <param name="p_strKey">The key in the Ini file to edit.</param>
    /// <param name="p_strValue">The value to which to set the key.</param>
    /// <param name="p_booSaveOld">Not used.</param>
    /// <returns><lang langref="true"/> if the value was set; <lang langref="false"/>
    /// if the user chose not to overwrite the existing value.</returns>
    /// <seealso cref="ModInstaller.EditPrefsINI(string, string, string, bool)"/>
    public static bool EditPrefsINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
    {
      return
        (bool) (ExecuteMethod(() => Script.EditPrefsINI(p_strSection, p_strKey, p_strValue, p_booSaveOld)) ?? false);
    }

    /// <summary>
    /// Sets the specified value in the GECKCustom.ini file to the given value. 
    /// </summary>
    /// <param name="p_strSection">The section in the Ini file to edit.</param>
    /// <param name="p_strKey">The key in the Ini file to edit.</param>
    /// <param name="p_strValue">The value to which to set the key.</param>
    /// <param name="p_booSaveOld">Not used.</param>
    /// <returns><lang langref="true"/> if the value was set; <lang langref="false"/>
    /// if the user chose not to overwrite the existing value.</returns>
    /// <seealso cref="ModInstaller.EditGeckINI(string, string, string, bool)"/>
    public static bool EditGeckINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
    {
      return (bool) (ExecuteMethod(() => Script.EditGeckINI(p_strSection, p_strKey, p_strValue, p_booSaveOld)) ?? false);
    }

    /// <summary>
    /// Sets the specified value in the GECKPrefs.ini file to the given value. 
    /// </summary>
    /// <param name="p_strSection">The section in the Ini file to edit.</param>
    /// <param name="p_strKey">The key in the Ini file to edit.</param>
    /// <param name="p_strValue">The value to which to set the key.</param>
    /// <param name="p_booSaveOld">Not used.</param>
    /// <returns><lang langref="true"/> if the value was set; <lang langref="false"/>
    /// if the user chose not to overwrite the existing value.</returns>
    /// <seealso cref="ModInstaller.EditGeckPrefsINI(string, string, string, bool)"/>
    public static bool EditGeckPrefsINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
    {
      return
        (bool) (ExecuteMethod(() => Script.EditGeckPrefsINI(p_strSection, p_strKey, p_strValue, p_booSaveOld)) ?? false);
    }

    #endregion

    #endregion

    #region Misc Info

    /// <summary>
    /// Gets the specified value from the RendererInfo.txt file.
    /// </summary>
    /// <param name="p_strValue">The value to retrieve from the file.</param>
    /// <returns>The specified value from the RendererInfo.txt file, or
    /// <lang langref="null"/> if the value is not found.</returns>
    /// <seealso cref="ModScript.GetRendererInfo(string)"/>
    public static string GetRendererInfo(string p_strValue)
    {
      return (string) ExecuteMethod(() => Script.GetRendererInfo(p_strValue));
    }

    /// <summary>
    /// Determines if archive invalidation is active.
    /// </summary>
    /// <returns><lang langref="true"/> if archive invalidation is active;
    /// <lang langref="false"/> otherwise.</returns>
    /// <seealso cref="ModScript.IsAIActive()"/>
    public static bool IsAIActive()
    {
      return (bool) (ExecuteMethod(() => Script.IsAIActive()) ?? false);
    }

    #endregion

    #region Shader Editing

    /// <summary>
    /// Edits the specified shader with the specified data.
    /// </summary>
    /// <param name="p_intPackage">The package containing the shader to edit.</param>
    /// <param name="p_strShaderName">The shader to edit.</param>
    /// <param name="p_bteData">The value to which to edit the shader.</param>
    /// <returns><lang langref="true"/> if the value was set; <lang langref="false"/>
    /// if the user chose not to overwrite the existing value.</returns>
    /// <seealso cref="ModInstaller.EditShader(int, string, byte[])"/>
    public static bool EditShader(int p_intPackage, string p_strShaderName, byte[] p_bteData)
    {
      return (bool) (ExecuteMethod(() => Script.EditShader(p_intPackage, p_strShaderName, p_bteData)) ?? false);
    }

    #endregion

    #region Script Compilation

    /// <summary>
    /// Sets up the script compiler for the given plugins.
    /// </summary>
    /// <param name="p_plgPlugins">The plugins for which to set up the script compiler.</param>
    /// <seealso cref="ModScript.SetupScriptCompiler(Fomm.TESsnip.Plugin[])"/>
    public static void SetupScriptCompiler(Plugin[] p_plgPlugins)
    {
      var tspPlugins = new Plugin[p_plgPlugins.Length];
      for (var i = 0; i < p_plgPlugins.Length; i++)
      {
        tspPlugins[i] = p_plgPlugins[i];
      }
      ExecuteMethod(() => Script.SetupScriptCompiler(tspPlugins));
    }

    /// <summary>
    /// Compiles the result script.
    /// </summary>
    /// <seealso cref="ModScript.CompileResultScript(Fomm.TESsnip.SubRecord, out Fomm.TESsnip.Record, out string)"/>
    public static void CompileResultScript(SubRecord sr, out Record r2, out string msg)
    {
      Record r;
      try
      {
        Script.CompileResultScript(sr, out r, out msg);
      }
      catch (Exception e)
      {
        LastError = e.Message;
        r = null;
        msg = null;
      }
      if (r != null)
      {
        r2 = (Record) r.Clone();
      }
      else
      {
        r2 = null;
      }
    }

    /// <summary>
    /// Compiles a script.
    /// </summary>
    /// <seealso cref="ModScript.CompileScript(Fomm.TESsnip.Record, out string)"/>
    public static void CompileScript(Record r2, out string msg)
    {
      try
      {
        Script.CompileScript(r2, out msg);
        r2.SubRecords.Clear();
        for (var i = 0; i < r2.SubRecords.Count; i++)
        {
          r2.SubRecords.Add((SubRecord) r2.SubRecords[i].Clone());
        }
      }
      catch (Exception e)
      {
        LastError = e.Message;
        msg = null;
      }
    }

    #endregion

    #region Texture Management

    /// <summary>
    /// Loads the given texture.
    /// </summary>
    /// <param name="p_bteTexture">The texture to load.</param>
    /// <returns>A pointer to the loaded texture.</returns>
    /// <seealso cref="TextureManager.LoadTexture(byte[])"/>
    public static IntPtr LoadTexture(byte[] p_bteTexture)
    {
      return (IntPtr) ExecuteMethod(() => Script.TextureManager.LoadTexture(p_bteTexture));
    }

    /// <summary>
    /// Creates a texture with the given dimensions.
    /// </summary>
    /// <param name="p_intWidth">The width of the texture.</param>
    /// <param name="p_intHeight">The height of the texture.</param>
    /// <returns>A pointer to the new texture.</returns>
    /// <seealso cref="TextureManager.CreateTexture(int, int)"/>
    public static IntPtr CreateTexture(int p_intWidth, int p_intHeight)
    {
      return (IntPtr) ExecuteMethod(() => Script.TextureManager.CreateTexture(p_intWidth, p_intHeight));
    }

    /// <summary>
    /// Saves the specified texture.
    /// </summary>
    /// <param name="p_ptrTexture">The pointer to the texture to save.</param>
    /// <param name="p_intFormat">The format in which to save the texture.</param>
    /// <param name="p_booMipmaps">Whether or not to create mipmaps (or maybe whether or
    /// not the given texture contains mipmaps?).</param>
    /// <returns>The saved texture.</returns>
    /// <seealso cref="TextureManager.SaveTexture(IntPtr, int, bool)"/>
    public static byte[] SaveTexture(IntPtr p_ptrTexture, int p_intFormat, bool p_booMipmaps)
    {
      return (byte[]) ExecuteMethod(() => Script.TextureManager.SaveTexture(p_ptrTexture, p_intFormat, p_booMipmaps));
    }

    /// <summary>
    /// Copies part of one texture to another.
    /// </summary>
    /// <param name="p_ptrSource">A pointer to the texture from which to make the copy.</param>
    /// <param name="p_rctSourceRect">The area of the source texture from which to make the copy.</param>
    /// <param name="p_ptrDestination">A pointer to the texture to which to make the copy.</param>
    /// <param name="p_rctDestinationRect">The area of the destination texture to which to make the copy.</param>
    /// <seealso cref="TextureManager.CopyTexture(IntPtr, Rectangle, IntPtr, Rectangle)"/>
    public static void CopyTexture(IntPtr p_ptrSource, Rectangle p_rctSourceRect, IntPtr p_ptrDestination,
                                   Rectangle p_rctDestinationRect)
    {
      ExecuteMethod(
        () => Script.TextureManager.CopyTexture(p_ptrSource, p_rctSourceRect, p_ptrDestination, p_rctDestinationRect));
    }

    /// <summary>
    /// Gets the dimensions of the specified texture.
    /// </summary>
    /// <param name="p_ptrTexture">A pointer to the texture whose dimensions are to be determined.</param>
    /// <param name="p_intWidth">The out parameter that will contain the width of the texture.</param>
    /// <param name="p_intHeight">The out parameter that will contain the height of the texture.</param>
    /// <seealso cref="TextureManager.GetTextureSize(IntPtr, out int, out int)"/>
    public static void GetTextureSize(IntPtr p_ptrTexture, out int p_intWidth, out int p_intHeight)
    {
      try
      {
        Script.TextureManager.GetTextureSize(p_ptrTexture, out p_intWidth, out p_intHeight);
      }
      catch (Exception e)
      {
        LastError = e.Message;
        p_intWidth = -1;
        p_intHeight = -1;
      }
    }

    /// <summary>
    /// Retrieves the texture data for the specified texture.
    /// </summary>
    /// <param name="p_ptrTexture">A pointer to the texture whose data is to be retrieved.</param>
    /// <param name="p_intPitch">The out parameter that will contain the texture's pitch.</param>
    /// <returns>The texture data.</returns>
    /// <seealso cref="TextureManager.GetTextureData(IntPtr, out int)"/>
    public static byte[] GetTextureData(IntPtr p_ptrTexture, out int p_intPitch)
    {
      try
      {
        return Script.TextureManager.GetTextureData(p_ptrTexture, out p_intPitch);
      }
      catch (Exception e)
      {
        LastError = e.Message;
        p_intPitch = -1;
      }
      return null;
    }

    /// <summary>
    /// Sets the data for the specified texture.
    /// </summary>
    /// <param name="p_ptrTexture">A pointer to the texture whose data is to be set.</param>
    /// <param name="p_bteData">The data to which to set the texture.</param>
    /// <seealso cref="TextureManager.SetTextureData(IntPtr, byte[])"/>
    public static void SetTextureData(IntPtr p_ptrTexture, byte[] p_bteData)
    {
      ExecuteMethod(() => Script.TextureManager.SetTextureData(p_ptrTexture, p_bteData));
    }

    /// <summary>
    /// Releases the specified texture.
    /// </summary>
    /// <param name="p_ptrTexture">A pointer to the texture to release.</param>
    /// <seealso cref="TextureManager.ReleaseTexture(IntPtr)"/>
    public static void ReleaseTexture(IntPtr p_ptrTexture)
    {
      ExecuteMethod(() => Script.TextureManager.ReleaseTexture(p_ptrTexture));
    }

    #endregion

    #region Plugin Management

    #region Load Order Management

    /// <summary>
    /// Determines if the plugins have been auto-sorted.
    /// </summary>
    /// <returns><lang langref="true"/> if the plugins have been auto-sorted;
    /// <lang langref="false"/> otherwise.</returns>
    /// <seealso cref="ModScript.IsLoadOrderAutoSorted()"/>
    public static bool IsLoadOrderAutoSorted()
    {
      return (bool) (ExecuteMethod(() => Script.IsLoadOrderAutoSorted()) ?? false);
    }

    /// <summary>
    /// Determins where in the load order the specified plugin would be inserted
    /// if the plugins were auto-sorted.
    /// </summary>
    /// <param name="p_strPlugin">The name of the plugin whose auto-sort insertion
    /// point is to be determined.</param>
    /// <returns>The index where the specified plugin would be inserted were the
    /// plugins to be auto-sorted.</returns>
    /// <seealso cref="ModScript.GetAutoInsertionPoint(string)"/>
    public static int GetAutoInsertionPoint(string p_strPlugin)
    {
      return (int) ExecuteMethod(() => Script.GetAutoInsertionPoint(p_strPlugin));
    }

    /// <summary>
    /// Auto-sorts the specified plugins.
    /// </summary>
    /// <remarks>
    /// This is, apparently, a beta function. Use with caution.
    /// </remarks>
    /// <param name="p_strPlugins">The list of plugins to auto-sort.</param>
    /// <seealso cref="ModScript.AutoSortPlugins(string[])"/>
    public static void AutoSortPlugins(string[] p_strPlugins)
    {
      ExecuteMethod(() => Script.AutoSortPlugins(p_strPlugins));
    }

    #endregion

    #endregion
  }
}