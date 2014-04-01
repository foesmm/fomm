using System;
using System.Xml;
using Fomm.PackageManager.XmlConfiguredInstall;
using Fomm.PackageManager.XmlConfiguredInstall.Parsers;

namespace Fomm.Games.Fallout3.Script.XmlConfiguredInstall.Parsers
{
  /// <summary>
  /// The Fallout 3 parser extension for version 1.0 XML configuration files.
  /// </summary>
  public class Fallout3Parser10Extension : ParserExtension
  {
    /// <summary>
    /// Parses the given dependency.
    /// </summary>
    /// <param name="p_xndDependency">The dependency to parse.</param>
    /// <param name="p_dsmSate">The state manager for this install.</param>
    /// <returns>the dependency represented by the given node.</returns>
    public override IDependency ParseDependency(XmlNode p_xndDependency, DependencyStateManager p_dsmSate)
    {
      switch (p_xndDependency.Name)
      {
        case "foseDependancy":
          Version verMinFoseVersion = new Version(p_xndDependency.Attributes["version"].InnerText);
          return new FoseDependency((Fallout3DependencyStateManager) p_dsmSate, verMinFoseVersion);
      }
      return null;
    }
  }
}