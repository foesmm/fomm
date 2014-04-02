using System.Xml;

namespace Fomm.PackageManager.XmlConfiguredInstall.Parsers
{
  /// <summary>
  /// This base class allows for game-modes to inject some game-specific parsing of
  /// XML based mod config files.
  /// </summary>
  public class ParserExtension
  {
    /// <summary>
    /// Parses the given dependency.
    /// </summary>
    /// <param name="p_xndDependency">The dependency to parse.</param>
    /// <param name="p_dsmSate">The state manager for this install.</param>
    /// <returns>the dependency represented by the given node.</returns>
    public virtual IDependency ParseDependency(XmlNode p_xndDependency, DependencyStateManager p_dsmSate)
    {
      return null;
    }
  }
}