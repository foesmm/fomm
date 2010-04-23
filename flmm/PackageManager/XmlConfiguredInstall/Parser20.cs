using System;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Schema;
using System.IO;
using System.Drawing;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	/// <summary>
	/// Parses version 2.0 mod configuration files.
	/// </summary>
	public class Parser20 : Parser10
	{
		#region Properties

		/// <seealso cref="Parser.SchemaFileName"/>
		protected override string SchemaFileName
		{
			get
			{
				return "ModConfig2.0.xsd";
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
		public Parser20(XmlDocument p_xmlConfig, fomod p_fomodMod, DependencyStateManager p_dsmSate)
			: base(p_xmlConfig, p_fomodMod, p_dsmSate)
		{
		}

		#endregion

		#region Abstract Method Implementations

		/// <seealso cref="Parser.GetModDependencies()"/>
		public override CompositeDependency GetModDependencies()
		{
			CompositeDependency cpdDependency = new CompositeDependency(DependencyOperator.And);
			XmlNodeList xnlDependencies = XmlConfig.SelectNodes("/config/moduleDependencies/*");
			foreach (XmlNode xndDependency in xnlDependencies)
			{
				switch (xndDependency.Name)
				{
					case "foseDependency":
						Version verMinFoseVersion = new Version(xndDependency.Attributes["version"].InnerText);
						cpdDependency.Dependencies.Add(new FoseDependency(StateManager, verMinFoseVersion));
						break;
					case "falloutDependency":
						Version verMinFalloutVersion = new Version(xndDependency.Attributes["version"].InnerText);
						cpdDependency.Dependencies.Add(new FalloutDependency(StateManager, verMinFalloutVersion));
						break;
					case "fommDependency":
						Version verMinFommVersion = new Version(xndDependency.Attributes["version"].InnerText);
						cpdDependency.Dependencies.Add(new FommDependency(StateManager, verMinFommVersion));
						break;
					case "fileDependency":
						string strDependency = xndDependency.Attributes["file"].InnerText.ToLower();
						cpdDependency.Dependencies.Add(new FileDependency(strDependency, ModFileState.Active, StateManager));
						break;
					default:
						throw new ParserException("Invalid dependency node: " + xndDependency.Name + ". At this point the config file has been validated against the schema, so there's something wrong with the parser.");
				}
			}
			return cpdDependency;
		}

		/// <seealso cref="Parser.GetConditionalFileInstallPatterns()"/>
		public override IList<ConditionalFileInstallPattern> GetConditionalFileInstallPatterns()
		{
			XmlNodeList xnlRequiredInstallFiles = XmlConfig.SelectNodes("/config/conditionalFileInstalls/patterns/*");
			return readConditionalFileInstallInfo(xnlRequiredInstallFiles);
		}

		#endregion

		#region Parsing Methods

		/// <summary>
		/// Reads a plugin's information from the configuration file.
		/// </summary>
		/// <param name="p_xndPlugin">The configuration file node corresponding to the plugin to read.</param>
		/// <returns>The plugin information.</returns>
		protected override PluginInfo parsePlugin(XmlNode p_xndPlugin)
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
				case "dependencyType":
					PluginType ptpDefaultType = (PluginType)Enum.Parse(typeof(PluginType), xndTypeDescriptor.SelectSingleNode("defaultType").Attributes["name"].InnerText);
					iptType = new DependencyPluginType(ptpDefaultType);
					DependencyPluginType dptDependentType = (DependencyPluginType)iptType;

					XmlNodeList xnlPatterns = xndTypeDescriptor.SelectNodes("patterns/*");
					foreach (XmlNode xndPattern in xnlPatterns)
					{
						PluginType ptpType = (PluginType)Enum.Parse(typeof(PluginType), xndPattern.SelectSingleNode("type").Attributes["name"].InnerText);
						CompositeDependency cdpDependency = loadDependency(xndPattern.SelectSingleNode("dependencies"));
						dptDependentType.AddPattern(ptpType, cdpDependency);
					}
					break;
				default:
					throw new ParserException("Invalid plaug type descriptor node: " + xndTypeDescriptor.Name + ". At this point the config file has been validated against the schema, so there's something wrong with the parser.");
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

			XmlNodeList xnlPluginFlags = p_xndPlugin.SelectNodes("conditionFlags/*");
			pifPlugin.Flags.AddRange(readFlagInfo(xnlPluginFlags));

			return pifPlugin;
		}

		/// <summary>
		/// Reads the dependency information from the given node.
		/// </summary>
		/// <param name="p_xndCompositeDependency">The node from which to load the dependency information.</param>
		/// <returns>A <see cref="CompositeDependency"/> representing the dependency described in the given node.</returns>
		protected override CompositeDependency loadDependency(XmlNode p_xndCompositeDependency)
		{
			DependencyOperator dopOperator = (DependencyOperator)Enum.Parse(typeof(DependencyOperator), p_xndCompositeDependency.Attributes["operator"].InnerText);
			CompositeDependency cpdDependency = new CompositeDependency(dopOperator);
			XmlNodeList xnlDependencies = p_xndCompositeDependency.ChildNodes;
			foreach (XmlNode xndDependency in xnlDependencies)
			{
				switch (xndDependency.Name)
				{
					case "dependencies":
						cpdDependency.Dependencies.Add(loadDependency(xndDependency));
						break;
					case "fileDependency":
						string strDependency = xndDependency.Attributes["file"].InnerText.ToLower();
						ModFileState mfsModState = (ModFileState)Enum.Parse(typeof(ModFileState), xndDependency.Attributes["state"].InnerText);
						cpdDependency.Dependencies.Add(new FileDependency(strDependency, mfsModState, StateManager));
						break;
					case "flagDependency":
						string strFlagName = xndDependency.Attributes["flag"].InnerText;
						string strValue = xndDependency.Attributes["value"].InnerText;
						cpdDependency.Dependencies.Add(new FlagDependency(strFlagName, strValue, StateManager));
						break;
					default:
						throw new ParserException("Invalid plugin dependency node: " + xndDependency.Name + ". At this point the config file has been validated against the schema, so there's something wrong with the parser.");
				}
			}
			return cpdDependency;
		}

		/// <summary>
		/// Reads the condtition flag info from the given XML nodes.
		/// </summary>
		/// <param name="p_xnlFlags">The list of XML nodes containing the condition flag info to read.</param>
		/// <returns>An ordered list of <see cref="ConditionalFlag"/>s representing the data in the given list.</returns>
		private List<ConditionalFlag> readFlagInfo(XmlNodeList p_xnlFlags)
		{
			List<ConditionalFlag> lstFlags = new List<ConditionalFlag>();
			foreach (XmlNode xndFlag in p_xnlFlags)
			{
				string strName = xndFlag.Attributes["name"].InnerText;
				string strValue = xndFlag.InnerXml;
				lstFlags.Add(new ConditionalFlag(strName, strValue));
			}
			return lstFlags;
		}

		/// <summary>
		/// Reads the conditional file install info from the given XML nodes.
		/// </summary>
		/// <param name="p_xnlConditionalFileInstalls">The list of XML nodes containing the conditional file
		/// install info to read.</param>
		/// <returns>An ordered list of <see cref="ConditionalFileInstallPattern"/>s representing the
		/// data in the given list.</returns>
		private IList<ConditionalFileInstallPattern> readConditionalFileInstallInfo(XmlNodeList p_xnlConditionalFileInstalls)
		{
			List<ConditionalFileInstallPattern> lstPatterns = new List<ConditionalFileInstallPattern>();
			foreach (XmlNode xndPattern in p_xnlConditionalFileInstalls)
			{
				CompositeDependency cdpDependency = loadDependency(xndPattern.SelectSingleNode("dependencies"));
				IList<PluginFile> lstFiles = readFileInfo(xndPattern.SelectNodes("files/*"));
				lstPatterns.Add(new ConditionalFileInstallPattern(cdpDependency, lstFiles));
			}
			return lstPatterns;
		}

		#endregion
	}
}
