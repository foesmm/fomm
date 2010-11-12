using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Fomm.Games.Fallout3.Tools.TESsnip;
using System.Text;
using Fomm.Util;

namespace Fomm.Games.Fallout3
{
	/// <summary>
	/// Activates/deactivates Fallout: New Vegas plugins.
	/// </summary>
	public class FalloutNewVegasPluginManager : Fallout3PluginManager
	{
		/// <summary>
		/// Determines if the specified plugin is critical to the current game.
		/// </summary>
		/// <param name="p_strPluginPath">The full path to the plugin for which it is to be determined whether or not it is critical.</param>
		/// <returns><lang cref="true"/> if the specified pluing is critical;
		/// <lang cref="false"/> otherwise.</returns>
		public override bool IsCriticalPlugin(string p_strPluginPath)
		{
			return Path.GetFileName(p_strPluginPath).Equals("falloutnv.esm", StringComparison.OrdinalIgnoreCase);
		}
	}
}
