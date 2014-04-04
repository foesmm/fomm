using System;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  ///   The possible states of a mod file.
  /// </summary>
  public enum ModFileState
  {
    /// <summary>
    ///   Indicates the mod file is not installed.
    /// </summary>
    Missing,

    /// <summary>
    ///   Indicates the mod file is installed, but not active.
    /// </summary>
    Inactive,

    /// <summary>
    ///   Indicates the mod file is installed and active.
    /// </summary>
    Active
  }

  /// <summary>
  ///   A dependency that requires a specified file to be in a specified <see cref="ModFileState" />.
  /// </summary>
  public class FileDependency : IDependency
  {
    private ModFileState m_mfsState = ModFileState.Active;
    private DependencyStateManager m_dsmStateManager;

    #region Properties

    /// <summary>
    ///   Gets the path of the file that must be in the specified <see cref="State" />.
    /// </summary>
    /// <value>The path of the file that must be in the specified <see cref="State" />.</value>
    public string File { get; private set; }

    /// <summary>
    ///   Gets the <see cref="ModFileState" /> that the specified <see cref="File" /> must be in.
    /// </summary>
    /// <value>The <see cref="ModFileState" /> that the specified <see cref="File" /> must be in.</value>
    public ModFileState State
    {
      get
      {
        return m_mfsState;
      }
    }

    /// <summary>
    ///   Gets whether or not the dependency is fufilled.
    /// </summary>
    /// <remarks>
    ///   The dependency is fufilled if the specified <see cref="File" /> is in the
    ///   specified <see cref="State" />.
    /// </remarks>
    /// <value>Whether or not the dependency is fufilled.</value>
    /// <seealso cref="IDependency.IsFufilled" />
    public bool IsFufilled
    {
      get
      {
        switch (m_mfsState)
        {
          case ModFileState.Active:
            return (m_dsmStateManager.InstalledPlugins.ContainsKey(File) &&
                    m_dsmStateManager.InstalledPlugins[File]);
          case ModFileState.Inactive:
            return (m_dsmStateManager.InstalledPlugins.ContainsKey(File) &&
                    !m_dsmStateManager.InstalledPlugins[File]);
          case ModFileState.Missing:
            return (!m_dsmStateManager.InstalledPlugins.ContainsKey(File));
        }
        return false;
      }
    }

    /// <summary>
    ///   Gets a message describing whether or not the dependency is fufilled.
    /// </summary>
    /// <remarks>
    ///   If the dependency is fufilled the message is "Passed." If the dependency is not fufilled the
    ///   message uses the pattern:
    ///   File '&lt;file>' is not &lt;state>.
    /// </remarks>
    /// <value>A message describing whether or not the dependency is fufilled.</value>
    /// <seealso cref="IDependency.Message" />
    public string Message
    {
      get
      {
        if (IsFufilled)
        {
          return "Passed";
        }
        return String.Format("File '{0}' is not {1}.", File, State);
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_strFile">The file that must be is the specified state.</param>
    /// <param name="p_mfsState">The state in which the specified file must be.</param>
    /// <param name="p_dsmStateManager">The manager that reports the currect install state.</param>
    public FileDependency(string p_strFile, ModFileState p_mfsState, DependencyStateManager p_dsmStateManager)
    {
      m_mfsState = p_mfsState;
      File = p_strFile;
      m_dsmStateManager = p_dsmStateManager;
    }

    #endregion

    /// <summary>
    ///   Generates a text representation of the dependency.
    /// </summary>
    /// <returns>A text representation of the dependency.</returns>
    public override string ToString()
    {
      return File + " (" + m_mfsState + ") : " + IsFufilled;
    }
  }
}