using System;
using System.Collections.Generic;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  ///   This class manages the state of the installation.
  /// </summary>
  public abstract class DependencyStateManager
  {
    /// <summary>
    ///   Describe the owner and value of a condition flag.
    /// </summary>
    private class FlagValue
    {
      /// <summary>
      ///   The value of the flag.
      /// </summary>
      public string Value;

      /// <summary>
      ///   The owner of the flag.
      /// </summary>
      public PluginInfo Owner;
    }

    private Dictionary<string, FlagValue> m_dicFlags = new Dictionary<string, FlagValue>();

    #region Properties

    /// <summary>
    ///   Gets the install script being used to perform the install.
    /// </summary>
    /// <value>The install script being used to perform the install.</value>
    protected ModInstallScript Script { get; private set; }

    /// <summary>
    ///   A dictionary listed all installed plugins, and indicating which are active.
    /// </summary>
    public Dictionary<string, bool> InstalledPlugins { get; protected set; }

    /// <summary>
    ///   Gets the current values of the flags that have been set.
    /// </summary>
    /// <value>The current values of the flags that have been set.</value>
    public Dictionary<string, string> FlagValues
    {
      get
      {
        var dicValues = new Dictionary<string, string>();
        foreach (var kvpValue in m_dicFlags)
        {
          dicValues[kvpValue.Key] = kvpValue.Value.Value;
        }
        return dicValues;
      }
    }

    /// <summary>
    ///   Gets the installed version of the current game.
    /// </summary>
    /// <remarks>
    ///   <lang langref="null" /> is returned if the game is not installed.
    /// </remarks>
    /// <value>The installed version of the current game.</value>
    public Version GameVersion
    {
      get
      {
        return Script.GetGameVersion();
      }
    }

    /// <summary>
    ///   Gets the installed version of FOMM.
    /// </summary>
    /// <remarks>
    ///   <lang langref="null" /> is returned if FOMM is not installed.
    /// </remarks>
    /// <value>The installed version of FOMM.</value>
    public Version FommVersion
    {
      get
      {
        return Script.GetFommVersion();
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_misInstallScript">The install script.</param>
    public DependencyStateManager(ModInstallScript p_misInstallScript)
    {
      Script = p_misInstallScript;

      var dicPlugins = new Dictionary<string, bool>();
      var strPlugins = Script.GetAllPlugins();
      foreach (var strPlugin in strPlugins)
      {
        dicPlugins.Add(strPlugin.ToLowerInvariant(), IsPluginActive(strPlugin));
      }
      InstalledPlugins = dicPlugins;
    }

    #endregion

    /// <summary>
    ///   Determins if the specified plugin is active.
    /// </summary>
    /// <param name="p_strFile">The plugin whose state is to be dtermined.</param>
    /// <returns>true if the specified plugin is active; false otherwise.</returns>
    protected bool IsPluginActive(string p_strFile)
    {
      var strAtiveInstalledPlugins = GetActiveInstalledPlugins();
      foreach (var strActivePlugin in strAtiveInstalledPlugins)
      {
        if (strActivePlugin.Equals(p_strFile.ToLowerInvariant()))
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    ///   Gets a list of all active installed plugins.
    /// </summary>
    /// <returns>A list of all active installed plugins.</returns>
    protected string[] GetActiveInstalledPlugins()
    {
      if (m_strActiveInstalledPlugins == null)
      {
        var strActivePlugins = Script.GetActivePlugins();
        var lstActiveInstalled = new List<string>();
        foreach (var strActivePlugin in strActivePlugins)
        {
          if (FileManagement.DataFileExists(strActivePlugin))
          {
            lstActiveInstalled.Add(strActivePlugin.ToLowerInvariant());
          }
        }
        m_strActiveInstalledPlugins = lstActiveInstalled.ToArray();
      }
      return m_strActiveInstalledPlugins;
    }

    private string[] m_strActiveInstalledPlugins;

    /// <summary>
    ///   Sets the value of a conditional flag.
    /// </summary>
    /// <param name="p_strFlagName">The name of the falg whose value is to be set.</param>
    /// <param name="p_strValue">The value to which to set the flag.</param>
    /// <param name="p_pifPlugin">The plugin that is responsible for setting the flag's value.</param>
    public void SetFlagValue(string p_strFlagName, string p_strValue, PluginInfo p_pifPlugin)
    {
      if (!m_dicFlags.ContainsKey(p_strFlagName))
      {
        m_dicFlags[p_strFlagName] = new FlagValue();
      }
      m_dicFlags[p_strFlagName].Value = p_strValue;
      m_dicFlags[p_strFlagName].Owner = p_pifPlugin;
    }

    /// <summary>
    ///   Removes the specified flag if the given plugin is the owner of the current value.
    /// </summary>
    /// <param name="p_strFlagName">The name of the flag to remove.</param>
    /// <param name="p_pifPlugin">The owner of the flag to remove.</param>
    public void RemoveFlags(PluginInfo p_pifPlugin)
    {
      var lstFlags = new List<string>(m_dicFlags.Keys);
      foreach (var strFlag in lstFlags)
      {
        if (m_dicFlags[strFlag].Owner == p_pifPlugin)
        {
          m_dicFlags.Remove(strFlag);
        }
      }
    }
  }
}