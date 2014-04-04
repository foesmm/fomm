using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;

namespace Fomm.PackageManager.XmlConfiguredInstall.Parsers
{
  /// <summary>
  ///   Parses version 3.0 mod configuration files.
  /// </summary>
  public class Parser30 : Parser20
  {
    #region Properties

    /// <seealso cref="Parser.ConfigurationFileVersion" />
    protected override string ConfigurationFileVersion
    {
      get
      {
        return "3.0";
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   A simple constructor that initializes teh object with the given values.
    /// </summary>
    /// <param name="p_xmlConfig">The modules configuration file.</param>
    /// <param name="p_fomodMod">The mod whose configuration file we are parsing.</param>
    /// <param name="p_dsmSate">The state of the install.</param>
    /// <param name="p_pexParserExtension">The parser extension that provides game-specific config file parsing.</param>
    public Parser30(XmlDocument p_xmlConfig, fomod p_fomodMod, DependencyStateManager p_dsmSate,
                    ParserExtension p_pexParserExtension)
      : base(p_xmlConfig, p_fomodMod, p_dsmSate, p_pexParserExtension) {}

    #endregion

    #region Abstract Method Implementations

    /// <seealso cref="Parser.GetModDependencies()" />
    public override CompositeDependency GetModDependencies()
    {
      var xndModDependencies = XmlConfig.SelectSingleNode("/config/moduleDependencies");
      if (xndModDependencies == null)
      {
        return null;
      }
      return loadDependency(xndModDependencies);
    }

    /// <seealso cref="Parser.GetHeaderInfo()" />
    public override HeaderInfo GetHeaderInfo()
    {
      var xndTitle = XmlConfig.SelectSingleNode("/config/moduleName");
      var strTitle = xndTitle.InnerText;
      var clrColour =
        Color.FromArgb(
          (Int32) (UInt32.Parse(xndTitle.Attributes["colour"].Value, NumberStyles.HexNumber, null) | 0xff000000));
      var tpsPosition = (TextPosition) Enum.Parse(typeof (TextPosition), xndTitle.Attributes["position"].Value);

      var xndImage = XmlConfig.SelectSingleNode("/config/moduleImage");
      if (xndImage != null)
      {
        var strImagePath = xndImage.Attributes["path"].Value;
        var imgImage = String.IsNullOrEmpty(strImagePath)
          ? Fomod.GetScreenshotImage()
          : new Bitmap(Fomod.GetImage(strImagePath));
        var booShowImage = Boolean.Parse(xndImage.Attributes["showImage"].Value) && (imgImage != null);
        var booShowFade = Boolean.Parse(xndImage.Attributes["showFade"].Value);
        var intHeight = Int32.Parse(xndImage.Attributes["height"].Value);
        if ((intHeight == -1) && booShowImage)
        {
          intHeight = 75;
        }
        return new HeaderInfo(strTitle, clrColour, tpsPosition, imgImage, booShowImage, booShowFade, intHeight);
      }
      var imgScreenshot = Fomod.GetScreenshotImage();
      return new HeaderInfo(strTitle, clrColour, tpsPosition, imgScreenshot, imgScreenshot != null, true,
                            (imgScreenshot != null) ? 75 : -1);
    }

    #endregion

    #region Parsing Methods

    /// <summary>
    ///   Gets the mod plugins, organized into their groups.
    /// </summary>
    /// <returns>The mod plugins, organized into their groups.</returns>
    public override IList<PluginGroup> loadGroupedPlugins(XmlNode p_xndFileGroups)
    {
      var lstGroups = new List<PluginGroup>();
      if (p_xndFileGroups != null)
      {
        foreach (XmlNode xndGroup in p_xndFileGroups.ChildNodes)
        {
          lstGroups.Add(parseGroup(xndGroup));
        }
        switch (p_xndFileGroups.Attributes["order"].InnerText)
        {
          case "Ascending":
            lstGroups.Sort((x, y) =>
            {
              if (String.IsNullOrEmpty(x.Name))
              {
                if (String.IsNullOrEmpty(y.Name))
                {
                  return 0;
                }
                return -1;
              }
              return x.Name.CompareTo(y.Name);
            });
            break;
          case "Descending":
            lstGroups.Sort((x, y) =>
            {
              if (String.IsNullOrEmpty(y.Name))
              {
                if (String.IsNullOrEmpty(x.Name))
                {
                  return 0;
                }
                return -1;
              }
              return y.Name.CompareTo(x.Name);
            });
            break;
        }
      }
      return lstGroups;
    }

    /// <summary>
    ///   Creates a plugin group based on the given info.
    /// </summary>
    /// <param name="p_xndGroup">The configuration file node corresponding to the group to add.</param>
    /// <returns>The added group.</returns>
    protected override PluginGroup parseGroup(XmlNode p_xndGroup)
    {
      var strName = p_xndGroup.Attributes["name"].InnerText;
      var gtpType = (GroupType) Enum.Parse(typeof (GroupType), p_xndGroup.Attributes["type"].InnerText);
      var strPluginOrder = SortOrder.None;
      switch (p_xndGroup.FirstChild.Attributes["order"].InnerText)
      {
        case "Ascending":
          strPluginOrder = SortOrder.Ascending;
          break;
        case "Descending":
          strPluginOrder = SortOrder.Descending;
          break;
      }
      var pgpGroup = new PluginGroup(strName, gtpType, strPluginOrder);
      var xnlPlugins = p_xndGroup.FirstChild.ChildNodes;
      foreach (XmlNode xndPlugin in xnlPlugins)
      {
        var pifPlugin = parsePlugin(xndPlugin);
        pgpGroup.addPlugin(pifPlugin);
      }
      return pgpGroup;
    }

    /// <summary>
    ///   Reads the dependency information from the given node.
    /// </summary>
    /// <param name="p_xndCompositeDependency">The node from which to load the dependency information.</param>
    /// <returns>A <see cref="CompositeDependency" /> representing the dependency described in the given node.</returns>
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
          case "falloutDependency":
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