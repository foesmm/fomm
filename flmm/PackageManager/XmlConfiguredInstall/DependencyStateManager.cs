using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  /// This class manages the state of the installation.
  /// </summary>
  public abstract class DependencyStateManager
  {
    /// <summary>
    /// Describe the owner and value of a condition flag.
    /// </summary>
    private class FlagValue
    {
      /// <summary>
      /// The value of the flag.
      /// </summary>
      public string Value;

      /// <summary>
      /// The owner of the flag.
      /// </summary>
      public PluginInfo Owner;
    }

    private ModInstallScript m_misInstallScript = null;
    private Dictionary<string, FlagValue> m_dicFlags = new Dictionary<string, FlagValue>();
    private Dictionary<string, bool> m_dicInstalledPlugins = null;

    #region Properties

    /// <summary>
    /// Gets the install script being used to perform the install.
    /// </summary>
    /// <value>The install script being used to perform the install.</value>
    protected ModInstallScript Script
    {
      get
      {
        return m_misInstallScript;
      }
    }

    /// <summary>
    /// A dictionary listed all installed plugins, and indicating which are active.
    /// </summary>
    public Dictionary<string, bool> InstalledPlugins
    {
      get
      {
        return m_dicInstalledPlugins;
      }
      protected set
      {
        m_dicInstalledPlugins = value;
      }
    }

    /// <summary>
    /// Gets the current values of the flags that have been set.
    /// </summary>
    /// <value>The current values of the flags that have been set.</value>
    public Dictionary<string, string> FlagValues
    {
      get
      {
        Dictionary<string, string> dicValues = new Dictionary<string, string>();
        foreach (KeyValuePair<string, FlagValue> kvpValue in m_dicFlags)
          dicValues[kvpValue.Key] = kvpValue.Value.Value;
        return dicValues;
      }
    }

    /// <summary>
    /// Gets the installed version of the current game.
    /// </summary>
    /// <remarks>
    /// <lang cref="null"/> is returned if the game is not installed.
    /// </remarks>
    /// <value>The installed version of the current game.</value>
    public Version GameVersion
    {
      get
      {
        return m_misInstallScript.GetGameVersion();
      }
    }

    /// <summary>
    /// Gets the installed version of FOMM.
    /// </summary>
    /// <remarks>
    /// <lang cref="null"/> is returned if FOMM is not installed.
    /// </remarks>
    /// <value>The installed version of FOMM.</value>
    public Version FommVersion
    {
      get
      {
        return m_misInstallScript.GetFommVersion();
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_misInstallScript">The install script.</param>
    public DependencyStateManager(ModInstallScript p_misInstallScript)
    {
      m_misInstallScript = p_misInstallScript;

      Dictionary<string, bool> dicPlugins = new Dictionary<string, bool>();
      string[] strPlugins = m_misInstallScript.GetAllPlugins();
      foreach (string strPlugin in strPlugins)
        dicPlugins.Add(strPlugin.ToLowerInvariant(), IsPluginActive(strPlugin));
      InstalledPlugins = dicPlugins;
    }

    #endregion

    /// <summary>
    /// Determins if the specified plugin is active.
    /// </summary>
    /// <param name="p_strFile">The plugin whose state is to be dtermined.</param>
    /// <returns>true if the specified plugin is active; false otherwise.</returns>
    protected bool IsPluginActive(string p_strFile)
    {
      string[] strAtiveInstalledPlugins = GetActiveInstalledPlugins();
      foreach (string strActivePlugin in strAtiveInstalledPlugins)
        if (strActivePlugin.Equals(p_strFile.ToLowerInvariant()))
          return true;
      return false;
    }

    /// <summary>
    /// Gets a list of all active installed plugins.
    /// </summary>
    /// <returns>A list of all active installed plugins.</returns>
    protected string[] GetActiveInstalledPlugins()
    {
      if (m_strActiveInstalledPlugins == null)
      {
        string[] strActivePlugins = m_misInstallScript.GetActivePlugins();
        List<string> lstActiveInstalled = new List<string>();
        foreach (string strActivePlugin in strActivePlugins)
          if (FileManagement.DataFileExists(strActivePlugin))
            lstActiveInstalled.Add(strActivePlugin.ToLowerInvariant());
        m_strActiveInstalledPlugins = lstActiveInstalled.ToArray();
      }
      return m_strActiveInstalledPlugins;
    }
    string[] m_strActiveInstalledPlugins = null;

    /// <summary>
    /// Sets the value of a conditional flag.
    /// </summary>
    /// <param name="p_strFlagName">The name of the falg whose value is to be set.</param>
    /// <param name="p_strValue">The value to which to set the flag.</param>
    /// <param name="p_pifPlugin">The plugin that is responsible for setting the flag's value.</param>
    public void SetFlagValue(string p_strFlagName, string p_strValue, PluginInfo p_pifPlugin)
    {
      if (!m_dicFlags.ContainsKey(p_strFlagName))
        m_dicFlags[p_strFlagName] = new FlagValue();
      m_dicFlags[p_strFlagName].Value = p_strValue;
      m_dicFlags[p_strFlagName].Owner = p_pifPlugin;
    }

    /// <summary>
    /// Removes the specified flag if the given plugin is the owner of the current value.
    /// </summary>
    /// <param name="p_strFlagName">The name of the flag to remove.</param>
    /// <param name="p_pifPlugin">The owner of the flag to remove.</param>
    public void RemoveFlags(PluginInfo p_pifPlugin)
    {
      List<string> lstFlags = new List<string>(m_dicFlags.Keys);
      foreach (string strFlag in lstFlags)
        if (m_dicFlags[strFlag].Owner == p_pifPlugin)
          m_dicFlags.Remove(strFlag);
    }
  }
}
