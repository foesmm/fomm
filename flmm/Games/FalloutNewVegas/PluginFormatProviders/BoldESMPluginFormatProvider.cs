using System.IO;
using Fomm.Games.Fallout3.Tools.TESsnip;

namespace Fomm.Games.FalloutNewVegas.PluginFormatProviders
{
  /// <summary>
  ///   A plugin format provider that makes ESM plugins bold if the plugin list.
  /// </summary>
  public class BoldESMPluginFormatProvider : Fallout3.PluginFormatProviders.BoldESMPluginFormatProvider
  {
    /// <summary>
    ///   Determins if the provider has a format for the specified plugin.
    /// </summary>
    /// <param name="p_strPluginName">The name of the plugin for which to check if there is a format.</param>
    /// <returns>
    ///   <lang langref="true" /> if this provider has a format for the specified plugin;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public override bool HasFormat(string p_strPluginName)
    {
      return Properties.Settings.Default.falloutNewVegasBoldifyESMs &&
             Plugin.GetIsEsm(Path.Combine(Program.GameMode.PluginsPath, p_strPluginName));
    }
  }
}