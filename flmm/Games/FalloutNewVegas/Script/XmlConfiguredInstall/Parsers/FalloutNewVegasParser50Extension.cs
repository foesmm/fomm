using System;
using Fomm.PackageManager.XmlConfiguredInstall.Parsers;
using System.Xml;
using Fomm.PackageManager.XmlConfiguredInstall;
using Fomm.Games.Fallout3.Script.XmlConfiguredInstall.Parsers;
using Fomm.Games.Fallout3.Script.XmlConfiguredInstall;

namespace Fomm.Games.FalloutNewVegas.Script.XmlConfiguredInstall.Parsers
{
	/// <summary>
	/// The Fallout: New Vegas parser extension for version 5.0 XML configuration files.
	/// </summary>
	public class FalloutNewVegasParser50Extension : Fallout3Parser10Extension
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
				case "nvseDependency":
					Version verMinNvseVersion = new Version(p_xndDependency.Attributes["version"].InnerText);
					return new NvseDependency((Fallout3DependencyStateManager)p_dsmSate, verMinNvseVersion);
			}
			return null;
		}
	}
}
