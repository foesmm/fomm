using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	/// <summary>
	/// The possible plugin group types.
	/// </summary>
	public enum GroupType
	{
		/// <summary>
		/// At least one plugin in the group must be selected.
		/// </summary>
		SelectAtLeastOne,

		/// <summary>
		/// At most one plugin in the group must be selected.
		/// </summary>
		SelectAtMostOne,

		/// <summary>
		/// Exactly one plugin in the group must be selected.
		/// </summary>
		SelectExactlyOne,

		/// <summary>
		/// All plugins in the group must be selected.
		/// </summary>
		SelectAll,

		/// <summary>
		/// Any number of plugins in the group may be selected.
		/// </summary>
		SelectAny,

		/// <summary>
		/// This state should not be used.
		/// </summary>
		Inavlid
	}

	/// <summary>
	/// Represents a group of plugins.
	/// </summary>
	public class PluginGroup
	{
		private List<PluginInfo> m_lstPlugins = new List<PluginInfo>();
		private SortOrder m_srtPluginOrder = SortOrder.Ascending;

		#region Properties

		/// <summary>
		/// Gets or sets the name of the group.
		/// </summary>
		/// <value>The name of the group.</value>
		public string Name { get; protected set; }

		/// <summary>
		/// Gets or sets the type of the group.
		/// </summary>
		/// <value>The type of the group.</value>
		public GroupType Type { get; protected set; }

		/// <summary>
		/// Gets the plugins that are part of this group.
		/// </summary>
		/// <value>The plugins that are part of this group.</value>
		public IList<PluginInfo> Plugins
		{
			get
			{
				if (m_srtPluginOrder != SortOrder.None)
				{
					m_lstPlugins.Sort((x, y) =>
					{
						switch (m_srtPluginOrder)
						{
							case SortOrder.Ascending:
								if (String.IsNullOrEmpty(x.Name))
								{
									if (String.IsNullOrEmpty(y.Name))
										return 0;
									return -1;
								}
								return x.Name.CompareTo(y.Name);
							case SortOrder.Descending:
								if (String.IsNullOrEmpty(y.Name))
								{
									if (String.IsNullOrEmpty(x.Name))
										return 0;
									return -1;
								}
								return y.Name.CompareTo(x.Name);							
						}
						return 0;
					});
				}
				return m_lstPlugins;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_strName">The name of the group.</param>
		/// <param name="p_gtpType">The plugins that are part of this group.</param>
		/// <param name="p_srtPluginOrder">the order by which to sort the plugins in this group.</param>
		public PluginGroup(string p_strName, GroupType p_gtpType, SortOrder p_srtPluginOrder)
		{
			Name = p_strName;
			Type = p_gtpType;
			m_srtPluginOrder = p_srtPluginOrder;
		}

		#endregion

		/// <summary>
		/// Adds the given plugin to the group.
		/// </summary>
		/// <param name="p_pifPlugin">The plugin to add to the group.</param>
		public void addPlugin(PluginInfo p_pifPlugin)
		{
			m_lstPlugins.Add(p_pifPlugin);
		}
	}
}
