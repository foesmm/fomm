using System;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Schema;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	/// <summary>
	/// Parses version 3.0 mod configuration files.
	/// </summary>
	public class Parser30 : Parser20
	{
		#region Properties

		/// <seealso cref="Parser.SchemaFileName"/>
		protected override string SchemaFileName
		{
			get
			{
				return "ModConfig3.0.xsd";
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
		public Parser30(XmlDocument p_xmlConfig, fomod p_fomodMod, DependencyStateManager p_dsmSate)
			: base(p_xmlConfig, p_fomodMod, p_dsmSate)
		{
		}

		#endregion

		#region Abstract Method Implementations

		/// <seealso cref="Parser.GetModDependencies()"/>
		public override CompositeDependency GetModDependencies()
		{
			XmlNode xndModDependencies = XmlConfig.SelectSingleNode("/config/moduleDependencies");
			if (xndModDependencies == null)
				return null;
			return loadDependency(xndModDependencies);
		}

		/// <seealso cref="Parser.GetGroupedPlugins()"/>
		public override IList<PluginGroup> GetGroupedPlugins()
		{
			List<PluginGroup> lstGroups = new List<PluginGroup>();
			XmlNode xndGroups = XmlConfig.SelectSingleNode("/config/optionalFileGroups");
			foreach (XmlNode xndGroup in xndGroups.ChildNodes)
				lstGroups.Add(parseGroup(xndGroup));
			switch (xndGroups.Attributes["order"].InnerText)
			{
				case "Ascending":
					lstGroups.Sort((x, y) =>
					{
						if (String.IsNullOrEmpty(x.Name))
						{
							if (String.IsNullOrEmpty(y.Name))
								return 0;
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
								return 0;
							return -1;
						}
						return y.Name.CompareTo(x.Name);
					});
					break;
			}
			return lstGroups;
		}

		/// <seealso cref="Parser.GetHeaderInfo()"/>
		public override HeaderInfo GetHeaderInfo()
		{
			XmlNode xndTitle = XmlConfig.SelectSingleNode("/config/moduleName");
			string strTitle = xndTitle.InnerText;
			Color clrColour = Color.FromArgb((Int32)(UInt32.Parse(xndTitle.Attributes["colour"].Value, NumberStyles.HexNumber, null) | 0xff000000));
			TextPosition tpsPosition = (TextPosition)Enum.Parse(typeof(TextPosition), xndTitle.Attributes["position"].Value);

			XmlNode xndImage = XmlConfig.SelectSingleNode("/config/moduleImage");
			if (xndImage != null)
			{
				string strImagePath = xndImage.Attributes["path"].Value;
				Image imgImage = String.IsNullOrEmpty(strImagePath) ? Fomod.GetScreenshot().Image : new Bitmap(Fomod.GetImage(strImagePath));
				bool booShowImage = Boolean.Parse(xndImage.Attributes["showImage"].Value) && (imgImage != null);
				bool booShowFade = Boolean.Parse(xndImage.Attributes["showFade"].Value);
				Int32 intHeight = Int32.Parse(xndImage.Attributes["height"].Value);
				if ((intHeight == -1) && booShowImage)
					intHeight = 75;
				return new HeaderInfo(strTitle, clrColour, tpsPosition, imgImage, booShowImage, booShowFade, intHeight);
			}
			Image imgScreenshot = Fomod.GetScreenshot().Image;
			return new HeaderInfo(strTitle, clrColour, tpsPosition, imgScreenshot, imgScreenshot != null, true, (imgScreenshot != null) ? 75 : -1);
		}

		#endregion

		#region Parsing Methods

		/// <summary>
		/// Creates a plugin group based on the given info.
		/// </summary>
		/// <param name="p_xndGroup">The configuration file node corresponding to the group to add.</param>
		/// <returns>The added group.</returns>
		protected override PluginGroup parseGroup(XmlNode p_xndGroup)
		{
			string strName = p_xndGroup.Attributes["name"].InnerText;
			GroupType gtpType = (GroupType)Enum.Parse(typeof(GroupType), p_xndGroup.Attributes["type"].InnerText);
			SortOrder strPluginOrder = SortOrder.None;
			switch (p_xndGroup.FirstChild.Attributes["order"].InnerText)
			{
				case "Ascending":
					strPluginOrder = SortOrder.Ascending;
					break;
				case "Descending":
					strPluginOrder = SortOrder.Descending;
					break;
			}
			PluginGroup pgpGroup = new PluginGroup(strName, gtpType, strPluginOrder);
			XmlNodeList xnlPlugins = p_xndGroup.FirstChild.ChildNodes;
			foreach (XmlNode xndPlugin in xnlPlugins)
			{
				PluginInfo pifPlugin = parsePlugin(xndPlugin);
				pgpGroup.addPlugin(pifPlugin);
			}
			return pgpGroup;
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
					default:
						throw new ParserException("Invalid dependency node: " + xndDependency.Name + ". At this point the config file has been validated against the schema, so there's something wrong with the parser.");
				}
			}
			return cpdDependency;
		}

		#endregion
	}
}
