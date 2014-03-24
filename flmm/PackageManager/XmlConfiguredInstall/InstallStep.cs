using System;
using System.Collections.Generic;
using System.Text;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  /// A step in the XML configured install of a mod.
  /// </summary>
  public class InstallStep
  {
    private string m_strName = null;
    private CompositeDependency m_cdpVisibilityDependency = null;
    private IList<PluginGroup> m_lstGroupedPlugins = null;

    #region Properties

    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    /// <value>The name of the step.</value>
    public string Name
    {
      get
      {
        return m_strName;
      }
    }

    /// <summary>
    /// Gets whether this step is visible.
    /// </summary>
    /// <value>Whether this step is visible.</value>
    public bool Visible
    {
      get
      {
        if (m_cdpVisibilityDependency == null)
          return true;
        return m_cdpVisibilityDependency.IsFufilled;
      }
    }

    /// <summary>
    /// Gets the grouped list of plugins to display in this step.
    /// </summary>
    /// <value>The grouped list of plugins to display in this step.</value>
    public IList<PluginGroup> GroupedPlugins
    {
      get
      {
        return m_lstGroupedPlugins;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_strName">The name of the install step.</param>
    /// <param name="p_cdpVisibilityDependency">The <see cref="CompositeDependency"/> that determines the visibility of this step.</param>
    /// <param name="p_lstGroupedPlugins">The grouped list of plugins to display in this step.</param>
    public InstallStep(string p_strName, CompositeDependency p_cdpVisibilityDependency, IList<PluginGroup> p_lstGroupedPlugins)
    {
      m_strName = p_strName;
      m_cdpVisibilityDependency = p_cdpVisibilityDependency;
      m_lstGroupedPlugins = p_lstGroupedPlugins;
    }

    #endregion
  }
}
