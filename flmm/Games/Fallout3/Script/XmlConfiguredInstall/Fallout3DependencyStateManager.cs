using System;
using Fomm.PackageManager.XmlConfiguredInstall;
using Fomm.PackageManager;

namespace Fomm.Games.Fallout3.Script.XmlConfiguredInstall
{
  /// <summary>
  /// This class manages the state of the installation.
  /// </summary>
  public class Fallout3DependencyStateManager : DependencyStateManager
  {
    #region Properties

    /// <summary>
    /// Gets the installed version of the script extender.
    /// </summary>
    /// <remarks>
    /// <lang langref="null"/> is returned if the script extender is not installed.
    /// </remarks>
    /// <value>The installed version of the script extender.</value>
    public Version ScriptExtenderVersion
    {
      get
      {
        return ((Fallout3ModInstallScript) Script).GetScriptExtenderVersion();
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_misInstallScript">The install script.</param>
    public Fallout3DependencyStateManager(ModInstallScript p_misInstallScript)
      : base(p_misInstallScript)
    {
    }

    #endregion
  }
}