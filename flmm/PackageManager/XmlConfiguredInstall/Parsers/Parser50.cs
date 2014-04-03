using System;
using System.Xml;

namespace Fomm.PackageManager.XmlConfiguredInstall.Parsers
{
  /// <summary>
  /// Parses version 5.0 mod configuration files.
  /// </summary>
  public class Parser50 : Parser40
  {
    #region Properties

    /// <seealso cref="Parser.ConfigurationFileVersion"/>
    protected override string ConfigurationFileVersion
    {
      get
      {
        return "5.0";
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes teh object with the given values.
    /// </summary>
    /// <param name="p_xmlConfig">The modules configuration file.</param>
    /// <param name="p_fomodMod">The mod whose configuration file we are parsing.</param>
    /// <param name="p_dsmSate">The state of the install.</param>
    /// <param name="p_pexParserExtension">The parser extension that provides game-specific config file parsing.</param>
    public Parser50(XmlDocument p_xmlConfig, fomod p_fomodMod, DependencyStateManager p_dsmSate,
                    ParserExtension p_pexParserExtension)
      : base(p_xmlConfig, p_fomodMod, p_dsmSate, p_pexParserExtension)
    {
    }

    #endregion

    #region Parsing Methods

    /// <summary>
    /// Reads the dependency information from the given node.
    /// </summary>
    /// <param name="p_xndCompositeDependency">The node from which to load the dependency information.</param>
    /// <returns>A <see cref="CompositeDependency"/> representing the dependency described in the given node.</returns>
    protected override CompositeDependency loadDependency(XmlNode p_xndCompositeDependency)
    {
      if (p_xndCompositeDependency == null)
      {
        return null;
      }
      var dopOperator =
        (DependencyOperator)
          Enum.Parse(typeof (DependencyOperator), p_xndCompositeDependency.Attributes["operator"].InnerText);
      var cpdDependency = new CompositeDependency(dopOperator);
      var xnlDependencies = p_xndCompositeDependency.ChildNodes;
      foreach (XmlNode xndDependency in xnlDependencies)
      {
        switch (xndDependency.Name)
        {
          case "dependencies":
            cpdDependency.Dependencies.Add(loadDependency(xndDependency));
            break;
          case "fileDependency":
            var strDependency = xndDependency.Attributes["file"].InnerText.ToLower();
            var mfsModState =
              (ModFileState) Enum.Parse(typeof (ModFileState), xndDependency.Attributes["state"].InnerText);
            cpdDependency.Dependencies.Add(new FileDependency(strDependency, mfsModState, StateManager));
            break;
          case "flagDependency":
            var strFlagName = xndDependency.Attributes["flag"].InnerText;
            var strValue = xndDependency.Attributes["value"].InnerText;
            cpdDependency.Dependencies.Add(new FlagDependency(strFlagName, strValue, StateManager));
            break;
          case "gameDependency":
            var verMinFalloutVersion = new Version(xndDependency.Attributes["version"].InnerText);
            cpdDependency.Dependencies.Add(new GameVersionDependency(StateManager, verMinFalloutVersion));
            break;
          case "fommDependency":
            var verMinFommVersion = new Version(xndDependency.Attributes["version"].InnerText);
            cpdDependency.Dependencies.Add(new FommDependency(StateManager, verMinFommVersion));
            break;
          default:
            var dpnExtensionDependency = ParserExtension.ParseDependency(xndDependency, StateManager);
            if (dpnExtensionDependency != null)
            {
              cpdDependency.Dependencies.Add(dpnExtensionDependency);
            }
            else
            {
              throw new ParserException("Invalid dependency node: " + xndDependency.Name +
                                        ". At this point the config file has been validated against the schema, so there's something wrong with the parser.");
            }
            break;
        }
      }
      return cpdDependency;
    }

    #endregion
  }
}