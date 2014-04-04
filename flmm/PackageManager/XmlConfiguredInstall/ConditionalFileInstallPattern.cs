using System.Collections.Generic;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  ///   A pattern that is matched against external conditions to determine whether
  ///   or not its files are installed.
  /// </summary>
  public class ConditionalFileInstallPattern
  {
    #region Properties

    /// <summary>
    ///   Gets the dependency that must by fufilled for this pattern's files to be installed.
    /// </summary>
    /// <value>The dependency that must by fufilled for this pattern's files to be installed.</value>
    public CompositeDependency Dependency { get; private set; }

    /// <summary>
    ///   Gets the list of files that are to be installed if the pattern's dependency is fufilled.
    /// </summary>
    /// <value>The list of files that are to be installed if the pattern's dependency is fufilled.</value>
    public IList<PluginFile> Files { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    ///   A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_cdpDependency">The dependency that must by fufilled for this pattern's files to be installed.</param>
    /// <param name="p_lstFiles">The files that are to be installed if the given dependency is fufilled.</param>
    public ConditionalFileInstallPattern(CompositeDependency p_cdpDependency, IList<PluginFile> p_lstFiles)
    {
      Dependency = p_cdpDependency;
      Files = p_lstFiles;
    }

    #endregion
  }
}