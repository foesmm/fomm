using System;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  /// A plugin type that doesn't change.
  /// </summary>
  public class StaticPluginType : IPluginType
  {
    private PluginType m_ptpType = PluginType.Invalid;

    #region Properties

    /// <summary>
    /// Gets the plugin type.
    /// </summary>
    /// <value>The plugin type.</value>
    public PluginType Type
    {
      get
      {
        return m_ptpType;
      }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_ptpType">The plugin type.</param>
    public StaticPluginType(PluginType p_ptpType)
    {
      m_ptpType = p_ptpType;
    }

    #endregion
  }
}