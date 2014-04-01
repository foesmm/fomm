using System;
using System.Collections.Generic;
using System.Text;

namespace Fomm
{
  /// <summary>
  /// The interface for plugin format providers.
  /// </summary>
  /// <seealso cref="PluginFormat.PluginFormatterManager"/>
  public interface IPluginFormatProvider
  {
    /// <summary>
    /// Sets the <see cref="PluginFormat.PluginFormatterManager"/> to use.
    /// </summary>
    /// <value>The <see cref="PluginFormat.PluginFormatterManager"/> to use.</value>
    PluginFormat.PluginFormatterManager PluginFormatterManager { set; }

    /// <summary>
    /// Determins if the provider has a format for the specified plugin.
    /// </summary>
    /// <param name="p_strPluginName">The name of the plugin for which to check if there is a format.</param>
    /// <returns><lang cref="true"/> if this provider has a format for the specified plugin;
    /// <lang cref="false"/> otherwise.</returns>
    bool HasFormat(string p_strPluginName);

    /// <summary>
    /// Gets the provider's format for the specified plugin.
    /// </summary>
    /// <param name="p_strPluginName">The name of the plugin whose format is to be returned.</param>
    /// <returns>The provider's format for the specified plugin, or <lang cref="null"/> if the
    /// provider does not have a format for the speficied plugin.</returns>
    PluginFormat GetFormat(string p_strPluginName);
  }
}