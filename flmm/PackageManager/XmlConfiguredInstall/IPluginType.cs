using System;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  /// The possible plugin types.
  /// </summary>
  public enum PluginType
  {
    /// <summary>
    /// Indicates the plugin must be installed.
    /// </summary>
    Required,

    /// <summary>
    /// Indicates the plugin is optional.
    /// </summary>
    Optional,

    /// <summary>
    /// Indicates the plugin is recommended for stability.
    /// </summary>
    Recommended,

    /// <summary>
    /// Indicates that using the plugin could result in instability (i.e., a prerequisite plugin is missing).
    /// </summary>
    NotUsable,

    /// <summary>
    /// Indicates that using the plugin could result in instability if loaded
    /// with the currently active plugins (i.e., a prerequisite plugin is missing),
    /// but that the prerequisite plugin is installed, just not activated.
    /// </summary>
    CouldBeUsable,

    /// <summary>
    /// This state should not be used.
    /// </summary>
    Invalid
  }

  /// <summary>
  /// Defines the interface for a plugin type object.
  /// </summary>
  public interface IPluginType
  {
    /// <summary>
    /// Gets the plugin type.
    /// </summary>
    /// <value>The plugin type.</value>
    PluginType Type { get; }
  }
}