using System;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Collections.Generic;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	public abstract class Parser
	{
		/// <summary>
		/// The factory method that returns the appropriate parser for the given configuration file.
		/// </summary>
		/// <param name="p_xmlConfig">The configuration file for which to return a parser.</param>
		/// <param name="p_fomodMod">The mod whose configuration file is being parsed.</param>
		/// <param name="p_dicUsersPlugins">The user's installed plugins, and their states.</param>
		/// <returns>The appropriate parser for the given configuration file.</returns>
		/// <exception cref="ParserException">Thrown if no parser is found for the given configuration file.</exception>
		public static Parser GetParser(XmlDocument p_xmlConfig, fomod p_fomodMod, Dictionary<string, bool> p_dicUsersPlugins)
		{
			string strConfigVersion = "1.0";
			string strSchemaName = p_xmlConfig.ChildNodes[1].Attributes["xsi:noNamespaceSchemaLocation"].InnerText;
			Int32 intStartPos = strSchemaName.LastIndexOf("ModConfig") + 9;
			if (intStartPos > 8)
			{
				Int32 intEndPost = strSchemaName.LastIndexOf(".xsd");
				strConfigVersion = strSchemaName.Substring(intStartPos, strSchemaName.Length - intStartPos - 4);
			}
			switch (strConfigVersion)
			{
				case "1.0":
					return new Parser10(p_xmlConfig, p_fomodMod, p_dicUsersPlugins);
				case "2.0":
					return new Parser20(p_xmlConfig, p_fomodMod, p_dicUsersPlugins);
			}
			throw new ParserException("Unrecognized Module Configuration version (" + strConfigVersion + "). Perhaps a newer version of FOMM is required.");
		}

		private XmlDocument m_xmlConfig = null;
		private fomod m_fomodMod = null;
		private Dictionary<string, bool> m_dicUsersPlugins = null;

		#region Properties

		/// <summary>
		/// Gets the name of the schema used by the parser.
		/// </summary>
		/// <value>The name of the schema used by the parser.</value>
		protected abstract string SchemaFileName { get; }

		/// <summary>
		/// Gets the name of the mod whose configuration file is being parsed.
		/// </summary>
		/// <value>The name of the mod whose configuration file is being parsed.</value>
		public abstract string ModName { get; }

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

		protected fomod Fomod
		{
			get
			{
				return m_fomodMod;
			}
		}

		protected Dictionary<string, bool> UsersPlugins
		{
			get
			{
				return m_dicUsersPlugins;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes teh object with the given values.
		/// </summary>
		/// <param name="p_xmlConfig">The modules configuration file.</param>
		/// <param name="p_fomodMod">The mod whose configuration file we are parsing.</param>
		/// <param name="p_dicUsersPlugins">A list of the user's installed plugins and their states.</param>
		public Parser(XmlDocument p_xmlConfig, fomod p_fomodMod, Dictionary<string, bool> p_dicUsersPlugins)
		{
			m_xmlConfig = p_xmlConfig;
			m_fomodMod = p_fomodMod;
			m_dicUsersPlugins = p_dicUsersPlugins;
			validateModuleConfig();
		}

		#endregion

		#region Xml Helper Methods

		/// <summary>
		/// Loads the module configuration file from the FOMOD.
		/// </summary>
		/// <param name="p_strConfigPath">The path to the configuration file.</param>
		/// <param name="p_strSchemaPath">The path to the configuration file schema.</param>
		/// <returns>The module configuration file.</returns>
		private void validateModuleConfig()
		{
			XmlSchema xscSchema = loadModuleConfigSchema();
			m_xmlConfig.Schemas.Add(xscSchema);
			m_xmlConfig.Validate((s, e) => { throw e.Exception; });
		}

		/// <summary>
		/// Loads the module configuration schema from the FOMOD.
		/// </summary>
		/// <returns>The module configuration schema.</returns>
		private XmlSchema loadModuleConfigSchema()
		{
			string strSchemaPath = Path.Combine(Program.fommDir, SchemaFileName);
			byte[] bteSchema = File.ReadAllBytes(strSchemaPath);
			XmlSchema xscSchema = null;
			using (MemoryStream stmSchema = new MemoryStream(bteSchema))
			{
				using (StreamReader srdSchemaReader = new StreamReader(stmSchema, true))
				{
					xscSchema = XmlSchema.Read(srdSchemaReader, delegate(object sender, ValidationEventArgs e) { throw e.Exception; });
					srdSchemaReader.Close();
				}
				stmSchema.Close();
			}
			return xscSchema;
		}

		#endregion

		/// <summary>
		/// Gets the mod level dependencies of the mod.
		/// </summary>
		/// <returns>The mod level dependencies of the mod.</returns>
		public abstract ModDependencies GetModDependencies();

		/// <summary>
		/// Gets the mod plugins, organized into their groups.
		/// </summary>
		/// <returns>The mod plugins, organized into their groups.</returns>
		public abstract IList<PluginGroup> GetGroupedPlugins();

		/// <summary>
		/// Gets the mod's required install files.
		/// </summary>
		/// <returns>The mod's required install files.</returns>
		public abstract IList<PluginFile> GetRequiredInstallFiles();
	}
}
