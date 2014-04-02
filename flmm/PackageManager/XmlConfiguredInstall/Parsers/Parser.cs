using System;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Fomm.PackageManager.XmlConfiguredInstall.Parsers
{
  /// <summary>
  /// Provides a contract and base functionality for XML configuration file parsers.
  /// </summary>
  public abstract class Parser
  {
    /// <summary>
    /// Extracts the config version from a XML configuration file.
    /// </summary>
    protected static readonly Regex m_rgxVersion = new Regex(
      "xsi:noNamespaceSchemaLocation=\"[^\"]*ModConfig(.*?).xsd", RegexOptions.Singleline);

    /// <summary>
    /// Gets the config version used by the given XML configuration file.
    /// </summary>
    /// <param name="p_strXml">The XML files who version is to be determined.</param>
    /// <returns>The config version used the given XML configuration file, or <lang cref="null"/>
    /// if the given file is not recognized as a configuration file.</returns>
    public static string GetConfigVersion(string p_strXml)
    {
      if (!m_rgxVersion.IsMatch(p_strXml))
      {
        return null;
      }
      string strConfigVersion = m_rgxVersion.Match(p_strXml).Groups[1].Value;
      if (String.IsNullOrEmpty(strConfigVersion))
      {
        strConfigVersion = "1.0";
      }
      return strConfigVersion;
    }

    /// <summary>
    /// The factory method that returns the appropriate parser for the given configuration file.
    /// </summary>
    /// <param name="p_xmlConfig">The configuration file for which to return a parser.</param>
    /// <param name="p_fomodMod">The mod whose configuration file is being parsed.</param>
    /// <param name="p_dsmSate">The state of the install.</param>
    /// <returns>The appropriate parser for the given configuration file.</returns>
    /// <exception cref="ParserException">Thrown if no parser is found for the given configuration file.</exception>
    public static Parser GetParser(XmlDocument p_xmlConfig, fomod p_fomodMod, DependencyStateManager p_dsmSate)
    {
      string strConfigVersion = "1.0";
      string strSchemaName =
        p_xmlConfig.SelectSingleNode("config").Attributes["xsi:noNamespaceSchemaLocation"].InnerText.ToLowerInvariant();
      Int32 intStartPos = strSchemaName.LastIndexOf("modconfig") + 9;
      if (intStartPos > 8)
      {
        Int32 intLength = strSchemaName.Length - intStartPos - 4;
        if (intLength > 0)
        {
          strConfigVersion = strSchemaName.Substring(intStartPos, intLength);
        }
      }
      ParserExtension pexParserExtension = Program.GameMode.GetParserExtension(strConfigVersion);
      switch (strConfigVersion)
      {
        case "1.0":
          return new Parser10(p_xmlConfig, p_fomodMod, p_dsmSate, pexParserExtension);
        case "2.0":
          return new Parser20(p_xmlConfig, p_fomodMod, p_dsmSate, pexParserExtension);
        case "3.0":
          return new Parser30(p_xmlConfig, p_fomodMod, p_dsmSate, pexParserExtension);
        case "4.0":
          return new Parser40(p_xmlConfig, p_fomodMod, p_dsmSate, pexParserExtension);
        case "5.0":
          return new Parser50(p_xmlConfig, p_fomodMod, p_dsmSate, pexParserExtension);
      }
      throw new ParserException("Unrecognized Module Configuration version (" + strConfigVersion +
                                "). Perhaps a newer version of FOMM is required.");
    }

    public ParserExtension m_pexParserExtension = null;
    private XmlDocument m_xmlConfig;
    private fomod m_fomodMod;
    private DependencyStateManager m_dsmSate;

    #region Properties

    /// <summary>
    /// Gets the parser extension that provides game-specific config file parsing.
    /// </summary>
    /// <value>The parser extension that provides game-specific config file parsing.</value>
    protected ParserExtension ParserExtension
    {
      get
      {
        return m_pexParserExtension;
      }
    }

    /// <summary>
    /// Gets the name of the schema used by the parser.
    /// </summary>
    /// <value>The name of the schema used by the parser.</value>
    protected abstract string ConfigurationFileVersion { get; }

    /// <summary>
    /// Gets the xml configuration file.
    /// </summary>
    /// <value>The xml configuration file.</value>
    protected XmlDocument XmlConfig
    {
      get
      {
        return m_xmlConfig;
      }
    }

    /// <summary>
    /// Gets the fomod being installed.
    /// </summary>
    /// <value>The fomod being installed.</value>
    protected fomod Fomod
    {
      get
      {
        return m_fomodMod;
      }
    }

    /// <summary>
    /// Gets the dependency state manager.
    /// </summary>
    /// <value>The dependency state manager.</value>
    protected DependencyStateManager StateManager
    {
      get
      {
        return m_dsmSate;
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
    public Parser(XmlDocument p_xmlConfig, fomod p_fomodMod, DependencyStateManager p_dsmSate,
                  ParserExtension p_pexParserExtension)
    {
      m_pexParserExtension = p_pexParserExtension;
      m_xmlConfig = p_xmlConfig;
      m_fomodMod = p_fomodMod;
      m_dsmSate = p_dsmSate;
      validateModuleConfig();
    }

    #endregion

    #region Xml Helper Methods

    /// <summary>
    /// Loads the module configuration file from the FOMod.
    /// </summary>
    /// <param name="p_strConfigPath">The path to the configuration file.</param>
    /// <param name="p_strSchemaPath">The path to the configuration file schema.</param>
    /// <returns>The module configuration file.</returns>
    private void validateModuleConfig()
    {
      XmlSchema xscSchema = loadModuleConfigSchema();
      m_xmlConfig.Schemas.Add(xscSchema);
      m_xmlConfig.Validate((s, e) =>
      {
        throw e.Exception;
      });
    }

    /// <summary>
    /// Loads the module configuration schema from the FOMod.
    /// </summary>
    /// <returns>The module configuration schema.</returns>
    private XmlSchema loadModuleConfigSchema()
    {
      string strSchemaPath = Program.GameMode.GetXMLConfigSchemaPath(ConfigurationFileVersion);
      byte[] bteSchema = File.ReadAllBytes(strSchemaPath);
      XmlSchema xscSchema = null;
      using (MemoryStream stmSchema = new MemoryStream(bteSchema))
      {
        XmlReaderSettings xrsSettings = new XmlReaderSettings();
        xrsSettings.IgnoreComments = true;
        xrsSettings.IgnoreWhitespace = true;
        using (XmlReader xrdSchemaReader = XmlReader.Create(stmSchema, xrsSettings, strSchemaPath))
        {
          xscSchema = XmlSchema.Read(xrdSchemaReader, delegate(object sender, ValidationEventArgs e)
          {
            throw e.Exception;
          });
        }
        stmSchema.Close();
      }
      return xscSchema;
    }

    #endregion

    /// <summary>
    /// Gets the header info of the mod.
    /// </summary>
    /// <returns>The header info of the mod.</returns>
    public abstract HeaderInfo GetHeaderInfo();

    /// <summary>
    /// Gets the mod level dependencies of the mod.
    /// </summary>
    /// <returns>The mod level dependencies of the mod.</returns>
    public abstract CompositeDependency GetModDependencies();

    /// <summary>
    /// Gets the mod install steps.
    /// </summary>
    /// <remarks>
    /// The steps contain the mod plugins, organized into their groups.
    /// </remarks>
    /// <returns>The mod install steps.</returns>
    public abstract IList<InstallStep> GetInstallSteps();

    /// <summary>
    /// Gets the mod's required install files.
    /// </summary>
    /// <returns>The mod's required install files.</returns>
    public abstract IList<PluginFile> GetRequiredInstallFiles();

    /// <summary>
    /// Gets the mod's required install files.
    /// </summary>
    /// <returns>The mod's required install files.</returns>
    public abstract IList<ConditionalFileInstallPattern> GetConditionalFileInstallPatterns();
  }
}