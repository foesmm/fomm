using System;
using System.Drawing;

namespace Fomm.Games
{
  /// <summary>
  /// Encapsulates the information about a plugin.
  /// </summary>
  public class PluginInfo
  {
    #region Properties

    /// <summary>
    /// Gets or sets the description of the plugin.
    /// </summary>
    /// <value>The description of the plugin.</value>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the image of the plugin.
    /// </summary>
    /// <value>The picture of the plugin.</value>
    public Image Picture { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_strDescription">The description of the plugin.</param>
    /// <param name="p_imgPicture">The picture of the plugin.</param>
    public PluginInfo(string p_strDescription, Image p_imgPicture)
    {
      Description = p_strDescription;
      Picture = p_imgPicture;
    }

    #endregion
  }
}