using System;
using System.Diagnostics;
using System.IO;
using Fomm.Games.Fallout3.Script;
using Fomm.PackageManager;

namespace Fomm.Games.FalloutNewVegas.Script
{
  public class FalloutNewVegasModInstallScript : Fallout3ModInstallScript
  {
    #region Properties

    // extender name
    public override String ScriptExtenderName
    {
      get
      {
        return "nvse_loader.exe";
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   A simple constructor that initializes the object.
    /// </summary>
    /// <param name="p_fomodMod">The <see cref="fomod" /> against which to run the script.</param>
    public FalloutNewVegasModInstallScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller)
      : base(p_fomodMod, p_mibInstaller) {}

    #endregion

    #region Ini Management

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
    public override bool EditFalloutINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
    {
      EditINI(((FalloutNewVegasGameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).FODefaultIniPath, p_strSection,
              p_strKey, p_strValue);
      return EditINI(((FalloutNewVegasGameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).FOIniPath, p_strSection,
                     p_strKey, p_strValue);
    }

    #endregion

    #endregion

    #region IDisposable Members

    #endregion
  }
}