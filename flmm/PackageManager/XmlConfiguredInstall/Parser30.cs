using System;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Schema;
using System.IO;
using System.Drawing;

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

		#endregion

		#region Parsing Methods

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
