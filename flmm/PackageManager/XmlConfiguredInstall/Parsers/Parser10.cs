using System;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Schema;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace Fomm.PackageManager.XmlConfiguredInstall.Parsers
{
	/// <summary>
	/// Parses version 1.0 mod configuration files.
	/// </summary>
	public class Parser10 : Parser
	{
		#region Properties

		/// <seealso cref="Parser.ConfigurationFileVersion"/>
		protected override string ConfigurationFileVersion
		{
			get
			{
				return "1.0";
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
		public Parser10(XmlDocument p_xmlConfig, fomod p_fomodMod, DependencyStateManager p_dsmSate, ParserExtension p_pexParserExtension)
			: base(p_xmlConfig, p_fomodMod, p_dsmSate, p_pexParserExtension)
		{
		}

		#endregion

		#region Abstract Method Implementations

		/// <seealso cref="Parser.GetModDependencies()"/>
		public override CompositeDependency GetModDependencies()
		{
			CompositeDependency cpdDependency = new CompositeDependency(DependencyOperator.And);
			XmlNodeList xnlDependencies = XmlConfig.SelectNodes("/config/moduleDependancies/*");
			foreach (XmlNode xndDependency in xnlDependencies)
			{
				switch (xndDependency.Name)
				{
					case "falloutDependancy":
						Version verMinFalloutVersion = new Version(xndDependency.Attributes["version"].InnerText);
						cpdDependency.Dependencies.Add(new GameVersionDependency(StateManager, verMinFalloutVersion));
						break;
					case "fommDependancy":
						Version verMinFommVersion = new Version(xndDependency.Attributes["version"].InnerText);
						cpdDependency.Dependencies.Add(new FommDependency(StateManager, verMinFommVersion));
						break;
					case "fileDependancy":
						string strDependency = xndDependency.Attributes["file"].InnerText.ToLower();
						cpdDependency.Dependencies.Add(new FileDependency(strDependency, ModFileState.Active, StateManager));
						break;
					default:
						IDependency dpnExtensionDependency = ParserExtension.ParseDependency(xndDependency, StateManager);
						if (dpnExtensionDependency != null)
							cpdDependency.Dependencies.Add(dpnExtensionDependency);
						else
							throw new ParserException("Invalid dependency node: " + xndDependency.Name + ". At this point the config file has been validated against the schema, so there's something wrong with the parser.");
						break;
				}
			}
			return cpdDependency;
		}

		/// <seealso cref="Parser.GetInstallSteps()"/>
		public override IList<InstallStep> GetInstallSteps()
		{
			XmlNode xndGroups = XmlConfig.SelectSingleNode("/config/optionalFileGroups");
			IList<PluginGroup> lstGroups = loadGroupedPlugins(xndGroups);
			List<InstallStep> lstSteps = new List<InstallStep>();
			if (lstGroups.Count > 0)
				lstSteps.Add(new InstallStep(null, null, lstGroups));
			return lstSteps;
		}

		/// <seealso cref="Parser.GetRequiredInstallFiles()"/>
		public override IList<PluginFile> GetRequiredInstallFiles()
		{
			XmlNodeList xnlRequiredInstallFiles = XmlConfig.SelectNodes("/config/requiredInstallFiles/*");
			return readFileInfo(xnlRequiredInstallFiles);
		}

		/// <seealso cref="Parser.GetConditionalFileInstallPatterns()"/>
		public override IList<ConditionalFileInstallPattern> GetConditionalFileInstallPatterns()
		{
			return new List<ConditionalFileInstallPattern>();
		}

		/// <seealso cref="Parser.GetHeaderInfo()"/>
		public override HeaderInfo GetHeaderInfo()
		{
			Image imgScreenshot = Fomod.GetScreenshotImage();
			return new HeaderInfo(XmlConfig.SelectSingleNode("/config/moduleName").InnerText, Color.FromKnownColor(KnownColor.ControlText), TextPosition.Left, imgScreenshot, imgScreenshot != null, true, (imgScreenshot != null) ? 75 : -1);
		}

		#endregion

		#region Parsing Methods

		/// <summary>
		/// Gets the mod plugins, organized into their groups.
		/// </summary>
		/// <returns>The mod plugins, organized into their groups.</returns>
		public virtual IList<PluginGroup> loadGroupedPlugins(XmlNode p_xndFileGroups)
		{
			List<PluginGroup> lstGroups = new List<PluginGroup>();
			if (p_xndFileGroups != null)
				foreach (XmlNode xndGroup in p_xndFileGroups)
					lstGroups.Add(parseGroup(xndGroup));
			return lstGroups;
		}

		/// <summary>
		/// Creates a plugin group based on the given info.
		/// </summary>
		/// <param name="p_xndGroup">The configuration file node corresponding to the group to add.</param>
		/// <returns>The added group.</returns>
		protected virtual PluginGroup parseGroup(XmlNode p_xndGroup)
		{
			string strName = p_xndGroup.Attributes["name"].InnerText;
			GroupType gtpType = (GroupType)Enum.Parse(typeof(GroupType), p_xndGroup.Attributes["type"].InnerText);
			PluginGroup pgpGroup = new PluginGroup(strName, gtpType, SortOrder.Ascending);
			XmlNodeList xnlPlugins = p_xndGroup.FirstChild.ChildNodes;
			foreach (XmlNode xndPlugin in xnlPlugins)
			{
				PluginInfo pifPlugin = parsePlugin(xndPlugin);
				pgpGroup.addPlugin(pifPlugin);
			}
			return pgpGroup;
		}

		/// <summary>
		/// Reads a plugin's information from the configuration file.
		/// </summary>
		/// <param name="p_xndPlugin">The configuration file node corresponding to the plugin to read.</param>
		/// <returns>The plugin information.</returns>
		protected virtual PluginInfo parsePlugin(XmlNode p_xndPlugin)
		{
			string strName = p_xndPlugin.Attributes["name"].InnerText;
			string strDesc = p_xndPlugin.SelectSingleNode("description").InnerText.Trim();
			IPluginType iptType = null;
			XmlNode xndTypeDescriptor = p_xndPlugin.SelectSingleNode("typeDescriptor").FirstChild;
			switch (xndTypeDescriptor.Name)
			{
				case "type":
					iptType = new StaticPluginType((PluginType)Enum.Parse(typeof(PluginType), xndTypeDescriptor.Attributes["name"].InnerText));
					break;
				case "dependancyType":
					PluginType ptpDefaultType = (PluginType)Enum.Parse(typeof(PluginType), xndTypeDescriptor.SelectSingleNode("defaultType").Attributes["name"].InnerText);
					iptType = new DependencyPluginType(ptpDefaultType);
					DependencyPluginType dptDependentType = (DependencyPluginType)iptType;

					XmlNodeList xnlPatterns = xndTypeDescriptor.SelectNodes("patterns/*");
					foreach (XmlNode xndPattern in xnlPatterns)
					{
						PluginType ptpType = (PluginType)Enum.Parse(typeof(PluginType), xndPattern.SelectSingleNode("type").Attributes["name"].InnerText);
						CompositeDependency cdpDependency = loadDependency(xndPattern.SelectSingleNode("dependancies"));
						dptDependentType.AddPattern(ptpType, cdpDependency);
					}
					break;
			}
			XmlNode xndImage = p_xndPlugin.SelectSingleNode("image");
			Image imgImage = null;
			if (xndImage != null)
			{
				string strImageFilePath = xndImage.Attributes["path"].InnerText;
				imgImage = Fomod.GetImage(strImageFilePath);
			}
			PluginInfo pifPlugin = new PluginInfo(strName, strDesc, imgImage, iptType);

			XmlNodeList xnlPluginFiles = p_xndPlugin.SelectNodes("files/*");
			pifPlugin.Files.AddRange(readFileInfo(xnlPluginFiles));
			return pifPlugin;
		}

		/// <summary>
		/// Reads the dependency information from the given node.
		/// </summary>
		/// <param name="p_xndCompositeDependency">The node from which to load the dependency information.</param>
		/// <returns>A <see cref="CompositeDependency"/> representing the dependency described in the given node.</returns>
		protected virtual CompositeDependency loadDependency(XmlNode p_xndCompositeDependency)
		{
			DependencyOperator dopOperator = (DependencyOperator)Enum.Parse(typeof(DependencyOperator), p_xndCompositeDependency.Attributes["operator"].InnerText);
			CompositeDependency cpdDependency = new CompositeDependency(dopOperator);
			XmlNodeList xnlDependencies = p_xndCompositeDependency.ChildNodes;
			foreach (XmlNode xndDependency in xnlDependencies)
			{
				switch (xndDependency.Name)
				{
					case "dependancies":
						cpdDependency.Dependencies.Add(loadDependency(xndDependency));
						break;
					case "dependancy":
						string strDependency = xndDependency.Attributes["file"].InnerText.ToLower();
						ModFileState mfsModState = (ModFileState)Enum.Parse(typeof(ModFileState), xndDependency.Attributes["state"].InnerText);
						cpdDependency.Dependencies.Add(new FileDependency(strDependency, mfsModState, StateManager));
						break;
				}
			}
			return cpdDependency;
		}

		/// <summary>
		/// Reads the file info from the given XML nodes.
		/// </summary>
		/// <param name="p_xnlFiles">The list of XML nodes containing the file info to read.</param>
		/// <returns>An ordered list of <see cref="PluginFile"/>s representing the data in the given list.</returns>
		protected List<PluginFile> readFileInfo(XmlNodeList p_xnlFiles)
		{
			List<PluginFile> lstFiles = new List<PluginFile>();
			foreach (XmlNode xndFile in p_xnlFiles)
			{
				string strSource = xndFile.Attributes["source"].InnerText;
				string strDest = (xndFile.Attributes["destination"] == null) ? strSource : xndFile.Attributes["destination"].InnerText;
				bool booAlwaysInstall = Boolean.Parse(xndFile.Attributes["alwaysInstall"].InnerText);
				bool booInstallIfUsable = Boolean.Parse(xndFile.Attributes["installIfUsable"].InnerText);
				Int32 intPriority = Int32.Parse(xndFile.Attributes["priority"].InnerText);
				switch (xndFile.Name)
				{
					case "file":
						lstFiles.Add(new PluginFile(strSource, strDest, false, intPriority, booAlwaysInstall, booInstallIfUsable));
						break;
					case "folder":
						lstFiles.Add(new PluginFile(strSource, strDest, true, intPriority, booAlwaysInstall, booInstallIfUsable));
						break;
					default:
						throw new ParserException("Invalid file node: " + xndFile.Name + ". At this point the config file has been validated against the schema, so there's something wrong with the parser.");
				}
			}
			lstFiles.Sort();
			return lstFiles;
		}

		#endregion
	}
}
