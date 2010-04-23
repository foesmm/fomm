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

		private ModInstallScript m_misInstallScript = null;
		private Dictionary<string, FlagValue> m_dicFlags = new Dictionary<string, FlagValue>();

		#region Properties

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

		/// <summary>
		/// Gets the installed version of FOSE.
		/// </summary>
		/// <remarks>
		/// <lang cref="null"/> is returned if FOSE is not installed.
		/// </remarks>
		/// <value>The installed version of FOSE.</value>
		public Version FoseVersion
		{
			get
			{
			return m_misInstallScript.GetFoseVersion();
			}
		}

		/// <summary>
		/// Gets the installed version of Fallout 3.
		/// </summary>
		/// <remarks>
		/// <lang cref="null"/> is returned if Fallout 3 is not installed.
		/// </remarks>
		/// <value>The installed version of Fallout 3.</value>
		public Version FalloutVersion
		{
			get
			{
				return m_misInstallScript.GetFalloutVersion();
			}
		}

		/// <summary>
		/// Gets the installed version of FOMM.
		/// </summary>
		/// <remarks>
		/// <lang cref="null"/> is returned if FOMM is not installed.
		/// </remarks>
		/// <value>The installed version of FOMM.</value>
		public Version FommVersion
		{
			get
			{
				return m_misInstallScript.GetFommVersion();
			}
		}

		#endregion

		#region Constructors

		public DependencyStateManager(ModInstallScript p_misInstallScript)
		{
			m_misInstallScript = p_misInstallScript;

			Dictionary<string, bool> dicPlugins = new Dictionary<string, bool>();
			string[] strPlugins = m_misInstallScript.GetAllPlugins();
			foreach (string strPlugin in strPlugins)
				dicPlugins.Add(strPlugin.ToLowerInvariant(), IsPluginActive(strPlugin));
			InstalledPlugins = dicPlugins;
		}

		#endregion

		/// <summary>
		/// Determins if the specified plugin is active.
		/// </summary>
		/// <param name="p_strFile">The plugin whose state is to be dtermined.</param>
		/// <returns>true if the specified plugin is active; false otherwise.</returns>
		protected bool IsPluginActive(string p_strFile)
		{
			string[] strAtiveInstalledPlugins = GetActiveInstalledPlugins();
			foreach (string strActivePlugin in strAtiveInstalledPlugins)
				if (strActivePlugin.Equals(p_strFile.ToLowerInvariant()))
					return true;
			return false;
		}

		/// <summary>
		/// Gets a list of all active installed plugins.
		/// </summary>
		/// <returns>A list of all active installed plugins.</returns>
		protected string[] GetActiveInstalledPlugins()
		{
			if (m_strActiveInstalledPlugins == null)
			{
				string[] strActivePlugins = m_misInstallScript.GetActivePlugins();
				List<string> lstActiveInstalled = new List<string>();
				foreach (string strActivePlugin in strActivePlugins)
					if (FileManagement.DataFileExists(strActivePlugin))
						lstActiveInstalled.Add(strActivePlugin.ToLowerInvariant());
				m_strActiveInstalledPlugins = lstActiveInstalled.ToArray();
			}
			return m_strActiveInstalledPlugins;
		}
		string[] m_strActiveInstalledPlugins = null;

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
