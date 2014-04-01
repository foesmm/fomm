using System.Collections.Generic;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  /// A plugin type that is dependent upon the state of external conditions.
  /// </summary>
  public class DependencyPluginType : IPluginType
  {
    /// <summary>
    /// A pattern that is matched against external conditions to determine whether
    /// or not its plugin type is elected.
    /// </summary>
    private class DependencyTypePattern
    {
      private PluginType m_ptpType = PluginType.Invalid;
      private CompositeDependency m_cdpDependency;

      #region Properties

      /// <summary>
      /// The plugin type this pattern returns if it is fufilled.
      /// </summary>
      /// <value>The plugin type this pattern returns if it is fufilled.</value>
      public PluginType Type
      {
        get
        {
          return m_ptpType;
        }
      }

      /// <summary>
      /// Gets the dependency that must by fufilled for this pattern's plugin type
      /// to be elected.
      /// </summary>
      /// <value>The dependency that must by fufilled for this pattern's plugin type
      /// to be elected.</value>
      public CompositeDependency Dependency
      {
        get
        {
          return m_cdpDependency;
        }
      }

      #endregion

      #region Constructors

      /// <summary>
      /// A simple constructor that initializes the object with the given values.
      /// </summary>
      /// <param name="p_ptpType">The plugin type this pattern returns if it is fufilled.</param>
      /// <param name="p_cdpDependency">The dependency that must by fufilled for this pattern's plugin type
      /// to be elected.</param>
      public DependencyTypePattern(PluginType p_ptpType, CompositeDependency p_cdpDependency)
      {
        m_ptpType = p_ptpType;
        m_cdpDependency = p_cdpDependency;
      }

      #endregion
    }

    private PluginType m_ptpDefaultType = PluginType.Invalid;
    private List<DependencyTypePattern> m_lstPatterns = new List<DependencyTypePattern>();

    #region Properties

    /// <summary>
    /// Gets the plugin type.
    /// </summary>
    /// <remarks>
    /// The returned type is dependent upon external state. A list of patterns are matched
    /// against external state (e.g., installed files); the first pattern that is fufilled
    /// determines the returned type.
    /// 
    /// If no pattern is fufilled, a default type if returned.
    /// </remarks>
    /// <value>The plugin type.</value>
    public PluginType Type
    {
      get
      {
        foreach (DependencyTypePattern dtpPattern in m_lstPatterns)
        {
          if (dtpPattern.Dependency.IsFufilled)
          {
            return dtpPattern.Type;
          }
        }
        return m_ptpDefaultType;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_ptpDefaultType">The default <see cref="PluginType"/> to return
    /// if no patterns are fufilled.</param>
    public DependencyPluginType(PluginType p_ptpDefaultType)
    {
      m_ptpDefaultType = p_ptpDefaultType;
    }

    #endregion

    /// <summary>
    /// Adds a pattern that return the given plugin type if the given dependency is fufilled.
    /// </summary>
    /// <param name="p_ptpType">The type the pattern will return if the dependency is fufilled.</param>
    /// <param name="p_cdpDependency">The dependency that must be fufilled in order for the pattern
    /// to return the plugin type.</param>
    public void AddPattern(PluginType p_ptpType, CompositeDependency p_cdpDependency)
    {
      m_lstPatterns.Add(new DependencyTypePattern(p_ptpType, p_cdpDependency));
    }
  }
}