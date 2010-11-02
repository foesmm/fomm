using System;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Schema;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace Fomm.PackageManager.XmlConfiguredInstall.Parsers
{
	/// <summary>
	/// Parses version 4.0 mod configuration files.
	/// </summary>
	public class Parser40 : Parser30
	{
		#region Properties

		/// <seealso cref="Parser.ConfigurationFileVersion"/>
		protected override string ConfigurationFileVersion
		{
			get
			{
				return "4.0";
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
		public Parser40(XmlDocument p_xmlConfig, fomod p_fomodMod, DependencyStateManager p_dsmSate, ParserExtension p_pexParserExtension)
			: base(p_xmlConfig, p_fomodMod, p_dsmSate, p_pexParserExtension)
		{
		}

		#endregion

		#region Abstract Method Implementations

		/// <seealso cref="Parser.GetInstallSteps()"/>
		public override IList<InstallStep> GetInstallSteps()
		{
			List<InstallStep> lstSteps = new List<InstallStep>();
			XmlNode xndSteps = XmlConfig.SelectSingleNode("/config/installSteps");
			if (xndSteps != null)
			{
				foreach (XmlNode xndStep in xndSteps.ChildNodes)
					lstSteps.Add(parseInstallStep(xndStep));
				switch (xndSteps.Attributes["order"].InnerText)
				{
					case "Ascending":
						lstSteps.Sort((x, y) =>
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
						lstSteps.Sort((x, y) =>
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
			}
			return lstSteps;
		}

		#endregion

		#region Parsing Methods

		/// <summary>
		/// Creates an install step based on the given info.
		/// </summary>
		/// <param name="p_xndStep">The configuration file node corresponding to the install step to add.</param>
		/// <returns>The added install step.</returns>
		protected InstallStep parseInstallStep(XmlNode p_xndStep)
		{
			string strName = p_xndStep.Attributes["name"].InnerText;
			CompositeDependency cmpVisibility = loadDependency(p_xndStep.SelectSingleNode("visible"));
			IList<PluginGroup> lstGroups = loadGroupedPlugins(p_xndStep.SelectSingleNode("optionalFileGroups"));
			InstallStep stpStep = new InstallStep(strName, cmpVisibility, lstGroups);
			return stpStep;
		}

		#endregion
	}
}
