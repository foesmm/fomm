using System;
using System.Drawing;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  ///   This class describes the header of the XML configured script options form.
  /// </summary>
  public class HeaderInfo
  {
    private Color m_clrColour = SystemColors.ControlText;
    private TextPosition m_tpsTitlePosition = TextPosition.Right;
    private bool m_booShowImage = true;
    private bool m_booShowFade = true;
    private Int32 m_intHeight = -1;

    #region Properties

    /// <summary>
    ///   Gets the title of the form.
    /// </summary>
    /// <value>The title of the form.</value>
    public string Title { get; private set; }

    /// <summary>
    ///   Gets the colour of the title of the form.
    /// </summary>
    /// <value>The colour of the title of the form.</value>
    public Color TextColour
    {
      get
      {
        return m_clrColour;
      }
    }

    /// <summary>
    ///   Gets the image to display in the header.
    /// </summary>
    /// <value>The image to display in the header.</value>
    public Image Image { get; private set; }

    /// <summary>
    ///   Gets the position of the title in the header.
    /// </summary>
    /// <value>The position of the title in the header.</value>
    public TextPosition TextPosition
    {
      get
      {
        return m_tpsTitlePosition;
      }
    }

    /// <summary>
    ///   Gets whether or not to display the image in the header.
    /// </summary>
    /// <value>Whether or not to display the image in the header.</value>
    public bool ShowImage
    {
      get
      {
        return m_booShowImage;
      }
    }

    /// <summary>
    ///   Gets whether or not to display the fade effect in the header.
    /// </summary>
    /// <value>Whether or not to display the fade effect in the header.</value>
    public bool ShowFade
    {
      get
      {
        return m_booShowImage && m_booShowFade;
      }
    }

    /// <summary>
    ///   Gets the desired height of the header.
    /// </summary>
    /// <value>The desired height of the header.</value>
    public Int32 Height
    {
      get
      {
        return m_intHeight;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_strTitle">The title of the form.</param>
    /// <param name="p_clrColour">The colour of the title of the form.</param>
    /// <param name="p_tpsTitlePosition">The position of the title in the header.</param>
    /// <param name="p_imgImage">The image to display in the header.</param>
    /// <param name="p_booShowImage">Whether or not to display the image in the header.</param>
    /// <param name="p_booShowFade">Whether or not to display the fade effect in the header.</param>
    /// <param name="p_intHeight">The desired height of the header.</param>
    public HeaderInfo(string p_strTitle, Color p_clrColour, TextPosition p_tpsTitlePosition, Image p_imgImage,
                      bool p_booShowImage, bool p_booShowFade, Int32 p_intHeight)
    {
      Title = p_strTitle;
      m_clrColour = p_clrColour;
      Image = p_imgImage;
      m_tpsTitlePosition = p_tpsTitlePosition;
      m_booShowImage = p_booShowImage;
      m_booShowFade = p_booShowFade;
      m_intHeight = p_intHeight;
    }

    #endregion
  }
}