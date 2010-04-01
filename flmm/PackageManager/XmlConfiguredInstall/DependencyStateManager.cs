using System;
using System.Collections.Generic;
using System.Text;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	public class DependencyStateManager
	{
		private class FlagValue
		{
			public string Value;
			public PluginInfo Owner;
		}

		private Dictionary<string, FlagValue> m_dicFlags = new Dictionary<string, FlagValue>();

		public Dictionary<string, bool> InstalledPlugins { get; protected set; }

		/// <summary>
		/// Gets the current values of the flags that have been set.
		/// </summary>
		/// <value>The current values of the flags that have been set.</value>
		public Dictionary<string, string> FlagValues
		{
			get
			{
				Dictionary<string, string> dicValues = new Dictionary<string, string>();
				foreach (KeyValuePair<string, FlagValue> kvpValue in m_dicFlags)
					dicValues[kvpValue.Key] = kvpValue.Value.Value;
				return dicValues;
			}
		}

		public DependencyStateManager(Dictionary<string, bool> p_dicInstalledPlugins)
		{
			InstalledPlugins = p_dicInstalledPlugins;
		}

		public void SetFlagValue(string p_strFlagName, string p_strValue, PluginInfo p_pifPlugin)
		{
			if (!m_dicFlags.ContainsKey(p_strFlagName))
				m_dicFlags[p_strFlagName] = new FlagValue();
			m_dicFlags[p_strFlagName].Value = p_strValue;
			m_dicFlags[p_strFlagName].Owner = p_pifPlugin;
		}

		/// <summary>
		/// Removes the specified flag if the given plugin is the owner of the current value.
		/// </summary>
		/// <param name="p_strFlagName">The name of the flag to remove.</param>
		/// <param name="p_pifPlugin">The owner of the flag to remove.</param>
		public void RemoveFlags(PluginInfo p_pifPlugin)
		{
			List<string> lstFlags = new List<string>(m_dicFlags.Keys);
			foreach (string strFlag in lstFlags)
				if (m_dicFlags[strFlag].Owner == p_pifPlugin)
					m_dicFlags.Remove(strFlag);
		}
	}
}
