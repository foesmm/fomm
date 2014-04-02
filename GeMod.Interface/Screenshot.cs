using System;
using System.Drawing;
using System.IO;

namespace GeMod.Interface
{
  /// <summary>
  /// Describes the screenshot of a fomod.
  /// </summary>
  public class Screenshot
  {
    private string m_strExtension;
    private byte[] m_bteData;

    #region Properties

    /// <summary>
    /// Gets or sets the extension of the screenshot.
    /// </summary>
    /// <value>The extension of the screenshot.</value>
    public string Extension
    {
      get
      {
        return m_strExtension;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
        {
          m_strExtension = null;
        }
        else if (!value.StartsWith("."))
        {
          m_strExtension = "." + value;
        }
        else
        {
          m_strExtension = value;
        }
      }
    }

    /// <summary>
    /// Gets or sets the screenshot data.
    /// </summary>
    /// <value>The screenshot data.</value>
    public byte[] Data
    {
      get
      {
        return m_bteData;
      }
      set
      {
        m_bteData = value;
      }
    }

    /// <summary>
    /// Gets the screenshot image.
    /// </summary>
    /// <value>The screenshot image.</value>
    public Image Image
    {
      get
      {
        if (m_bteData == null)
        {
          return null;
        }
        Image imgReal = null;
        using (MemoryStream msmImage = new MemoryStream(m_bteData))
        {
          Image imgSreenshot = Image.FromStream(msmImage);
          imgReal = new Bitmap(imgSreenshot);
          msmImage.Close();
        }
        return imgReal;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_strPath">The path of the screenshot file. This is used to determine the <see cref="Extension"/>.</param>
    /// <param name="p_bteData">The screenshot data.</param>
    public Screenshot(string p_strPath, byte[] p_bteData)
    {
      Extension = Path.GetExtension(p_strPath);
      Data = p_bteData;
    }

    #endregion
  }
}