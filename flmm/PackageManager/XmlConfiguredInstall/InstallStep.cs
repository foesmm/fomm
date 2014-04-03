using System.Collections.Generic;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  /// A step in the XML configured install of a mod.
  /// </summary>
  public class InstallStep
  {
    private CompositeDependency m_cdpVisibilityDependency;

    #region Properties

    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    /// <value>The name of the step.</value>
    public string Name { get; private set; }

    /// <summary>
    /// Gets whether this step is visible.
    /// </summary>
    /// <value>Whether this step is visible.</value>
    public bool Visible
    {
      get
      {
        if (m_cdpVisibilityDependency == null)
        {
          return true;
        }
        return m_cdpVisibilityDependency.IsFufilled;
      }
    }

    /// <summary>
    /// Gets the grouped list of plugins to display in this step.
    /// </summary>
    /// <value>The grouped list of plugins to display in this step.</value>
    public IList<PluginGroup> GroupedPlugins { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_strName">The name of the install step.</param>
    /// <param name="p_cdpVisibilityDependency">The <see cref="CompositeDependency"/> that determines the visibility of this step.</param>
    /// <param name="p_lstGroupedPlugins">The grouped list of plugins to display in this step.</param>
    public InstallStep(string p_strName, CompositeDependency p_cdpVisibilityDependency,
                       IList<PluginGroup> p_lstGroupedPlugins)
    {
      Name = p_strName;
      m_cdpVisibilityDependency = p_cdpVisibilityDependency;
      GroupedPlugins = p_lstGroupedPlugins;
    }

    #endregion
  }
}