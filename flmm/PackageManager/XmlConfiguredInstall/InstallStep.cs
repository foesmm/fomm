using System;
using System.Collections.Generic;
using System.Text;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	public class InstallStep
	{
		private string m_strName = null;
		private CompositeDependency m_cdpVisibilityDependency = null;
		private IList<PluginGroup> m_lstGroupedPlugins = null;

		public string Name
		{
			get
			{
				return m_strName;
			}
		}

		public bool Visible
		{
			get
			{
				if (m_cdpVisibilityDependency == null)
					return true;
				return m_cdpVisibilityDependency.IsFufilled;
			}
		}

		public IList<PluginGroup> GroupedPlugins
		{
			get
			{
				return m_lstGroupedPlugins;
			}
		}

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_strName">The name of the install step.</param>
		public InstallStep(string p_strName, CompositeDependency p_cdpVisibilityDependency, IList<PluginGroup> p_lstGroupedPlugins)
		{
			m_strName = p_strName;
			m_cdpVisibilityDependency = p_cdpVisibilityDependency;
			m_lstGroupedPlugins = p_lstGroupedPlugins;
		}

		#endregion
	}
}
