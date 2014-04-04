using System;
using System.IO;
using Fomm.Games.Fallout3;

namespace Fomm.Games.FalloutNewVegas
{
  /// <summary>
  ///   Activates/deactivates Fallout: New Vegas plugins.
  /// </summary>
  public class FalloutNewVegasPluginManager : Fallout3PluginManager
  {
    /// <summary>
    ///   Determines if the specified plugin is critical to the current game.
    /// </summary>
    /// <param name="p_strPluginPath">
    ///   The full path to the plugin for which it is to be determined whether or not it is
    ///   critical.
    /// </param>
    /// <returns>
    ///   <lang langref="true" /> if the specified pluing is critical;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public override bool IsCriticalPlugin(string p_strPluginPath)
    {
      return Path.GetFileName(p_strPluginPath).Equals("falloutnv.esm", StringComparison.OrdinalIgnoreCase);
    }
  }
}