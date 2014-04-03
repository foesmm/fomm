using System.Collections.Generic;
using System.Drawing;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  /// Describes a plugin.
  /// </summary>
  /// <remarks>
  /// This class tracks the name, description, type, and files/folders associated with a plugin.
  /// </remarks>
  public class PluginInfo
  {
    private IPluginType m_ptpType;

    #region Properties

    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    /// <value>The name of the plugin.</value>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    /// <value>The description of the plugin.</value>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the plugin image.
    /// </summary>
    /// <value>The plugin image</value>
    public Image Image { get; private set; }

    /// <summary>
    /// Gets the <see cref="PluginType"/> of the plugin.
    /// </summary>
    /// <value>The <see cref="PluginType"/> of the plugin.</value>
    public PluginType Type
    {
      get
      {
        return m_ptpType.Type;
      }
    }

    /// <summary>
    /// Gets the list of files and folders associated with the plugin.
    /// </summary>
    /// <value>The list of files and folders associated with the plugin.</value>
    public List<PluginFile> Files { get; private set; }

    /// <summary>
    /// Gets the list of flags that should be set to the specifid value if the plugin is in the specified state.
    /// </summary>
    /// <value>The list of flags that should be set to the specifid value if the plugin is in the specified state.</value>
    public List<ConditionalFlag> Flags { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the values of the object.
    /// </summary>
    /// <param name="p_strName">The name of the plugin.</param>
    /// <param name="p_strDesc">The description of the plugin.</param>
    /// <param name="p_imgImage">The plugin image.</param>
    /// <param name="p_ptpType">The <see cref="PluginType"/> of the plugin.</param>
    public PluginInfo(string p_strName, string p_strDesc, Image p_imgImage, IPluginType p_ptpType)
    {
      Name = p_strName;
      Files = new List<PluginFile>();
      Flags = new List<ConditionalFlag>();
      m_ptpType = p_ptpType;
      Description = p_strDesc;
      Image = p_imgImage;
    }

    #endregion
  }
}