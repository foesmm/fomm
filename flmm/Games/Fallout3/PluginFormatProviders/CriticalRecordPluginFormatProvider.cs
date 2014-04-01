using System.Collections.Generic;
using System.Drawing;

namespace Fomm.Games.Fallout3.PluginFormatProviders
{
  /// <summary>
  /// A plugin format provider that higlights plugins based on record conflict information.
  /// </summary>
  public class CriticalRecordPluginFormatProvider : IPluginFormatProvider
  {
    private PluginFormat.PluginFormatterManager m_pfmManager;
    private Dictionary<string, PluginFormat> m_dicFormat = new Dictionary<string, PluginFormat>();

    /// <summary>
    /// Adds a format for the specified plugin.
    /// </summary>
    /// <param name="p_strPluginName">The name of the plugin for which to add a format.</param>
    /// <param name="p_clrHighlight">The highlight color of the format.</param>
    /// <param name="p_strMessage">The message of the format.</param>
    public void AddFormat(string p_strPluginName, Color p_clrHighlight, string p_strMessage)
    {
      PluginFormat pftFormat = m_pfmManager.CreateFormat(null, null, null, null, p_clrHighlight, p_strMessage);
      m_dicFormat[p_strPluginName] = pftFormat;
    }

    /// <summary>
    /// Clears the format cache.
    /// </summary>
    public void Clear()
    {
      m_dicFormat.Clear();
    }

    #region IPluginFormatProvider Members

    /// <summary>
    /// Sets the <see cref="PluginFormat.PluginFormatterManager"/> to use.
    /// </summary>
    /// <value>The <see cref="PluginFormat.PluginFormatterManager"/> to use.</value>
    public PluginFormat.PluginFormatterManager PluginFormatterManager
    {
      set
      {
        m_pfmManager = value;
      }
    }

    /// <summary>
    /// Determins if the provider has a format for the specified plugin.
    /// </summary>
    /// <param name="p_strPluginName">The name of the plugin for which to check if there is a format.</param>
    /// <returns><lang cref="true"/> if this provider has a format for the specified plugin;
    /// <lang cref="false"/> otherwise.</returns>
    public bool HasFormat(string p_strPluginName)
    {
      return m_dicFormat.ContainsKey(p_strPluginName);
    }

    /// <summary>
    /// Gets the provider's format for the specified plugin.
    /// </summary>
    /// <param name="p_strPluginName">The name of the plugin whose format is to be returned.</param>
    /// <returns>The provider's format for the specified plugin, or <lang cref="null"/> if the
    /// provider does not have a format for the speficied plugin.</returns>
    public PluginFormat GetFormat(string p_strPluginName)
    {
      if (!m_dicFormat.ContainsKey(p_strPluginName))
      {
        return null;
      }
      return m_dicFormat[p_strPluginName];
    }

    #endregion
  }
}