using System;
using System.Collections.Generic;
using System.Text;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  /// A plugin file or folder.
  /// </summary>
  /// <remarks>
  /// This class describes the location of the file/folder in the FOMod, as well as where the
  /// file/folder should be installed.
  /// </remarks>
  public class PluginFile : IComparable<PluginFile>
  {
    private string m_strSource = null;
    private string m_strDest = null;
    private bool m_booIsFolder = false;
    private bool m_booAlwaysInstall = false;
    private bool m_booInstallIfUsable = false;
    private Int32 m_intPriority = 0;

    #region Properties

    /// <summary>
    /// Gets the file's/folder's location in the FOMod.
    /// </summary>
    /// <value>The file's/folder's location in the FOMod.</value>
    public string Source
    {
      get
      {
        return m_strSource;
      }
    }

    /// <summary>
    /// Gets where the file/folder should be installed.
    /// </summary>
    /// <value>Where the file/folder should be installed.</value>
    public string Destination
    {
      get
      {
        return m_strDest;
      }
    }

    /// <summary>
    /// Gets whether this item is a folder.
    /// </summary>
    /// <value>Whether this item is a folder.</value>
    public bool IsFolder
    {
      get
      {
        return m_booIsFolder;
      }
    }

    /// <summary>
    /// Gets whether this item should always be installed, regardless of whether or not the plugin is selected.
    /// </summary>
    /// <value>Whether this item should always be installed, regardless of whether or not the plugin is selected.</value>
    public bool AlwaysInstall
    {
      get
      {
        return m_booAlwaysInstall;
      }
    }

    /// <summary>
    /// Gets whether this item should be installed if the plugins is usable, regardless of whether or not the plugin is selected.
    /// </summary>
    /// <value>Whether this item should be installed if the plugins is usable, regardless of whether or not the plugin is selected.</value>
    public bool InstallIfUsable
    {
      get
      {
        return m_booInstallIfUsable;
      }
    }

    /// <summary>
    /// Gets the priority of this item.
    /// </summary>
    /// <remarks>
    /// A higher number indicates the file or folder should be installed after the
    /// items with lower numbers. This value does not have to be unique.
    /// </remarks>
    /// <value>The priority of this item.</value>
    public Int32 Priority
    {
      get
      {
        return m_intPriority;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the values of the object.
    /// </summary>
    /// <param name="p_strSource">The file's/folder's location in the FOMod.</param>
    /// <param name="p_strDest">Where the file/folder should be installed.</param>
    /// <param name="p_booIsFolder">Whether this item is a folder.</param>
    /// <param name="p_intPriority">The priority of the item.</param>
    /// <param name="p_booAlwaysInstall">Whether this item should always be installed, regardless of whether or not the plugin is selected.</param>
    /// <param name="p_booInstallIfUsable">Whether this item should be installed when the plugin is not <see cref="PluginType.NotUsable"/>, regardless of whether or not the plugin is selected.</param>
    public PluginFile(string p_strSource, string p_strDest, bool p_booIsFolder, Int32 p_intPriority,
                      bool p_booAlwaysInstall, bool p_booInstallIfUsable)
    {
      m_strSource = p_strSource;
      m_strDest = p_strDest;
      m_booIsFolder = p_booIsFolder;
      m_booAlwaysInstall = p_booAlwaysInstall;
      m_booInstallIfUsable = p_booInstallIfUsable;
      m_intPriority = p_intPriority;
    }

    #endregion

    #region IComparable<PluginFile> Members

    /// <summary>
    /// Determines whether this PluginFile is less than, equal to,
    /// or greater than the given PluginFile.
    /// </summary>
    /// <param name="other">The PluginFile to which to compare this PluginFile.</param>
    /// <returns>A value less than 0 if this PluginFile is less than the given PluginFile,
    /// or 0 if this PluginFile is equal to the given PluginFile,
    ///or a value greater than 0 if this PluginFile is greater than the given PluginFile.</returns>
    public int CompareTo(PluginFile other)
    {
      Int32 intResult = m_intPriority.CompareTo(other.Priority);
      if (intResult == 0)
      {
        intResult = m_booIsFolder.CompareTo(other.IsFolder);
        if (intResult == 0)
        {
          intResult = m_strSource.CompareTo(other.Source);
          if (intResult == 0)
          {
            intResult = m_strDest.CompareTo(other.Destination);
          }
        }
      }
      return intResult;
    }

    #endregion
  }
}