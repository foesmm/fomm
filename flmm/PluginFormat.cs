﻿using System;
using System.Drawing;

namespace Fomm
{
  /// <summary>
  /// The format to apply to a plugin in the main form list.
  /// </summary>
  /// <remarks>
  /// The plugin format system allows several different participants to contribute to the styling
  /// of plugins in the master list.
  /// </remarks>
  /// <seealso cref="PluginFormat.PluginFormatterManager"/>
  public partial class PluginFormat : IComparable<PluginFormat>, IEquatable<PluginFormat>
  {
    private Int32 m_intIndex = -1;
    private FontFamily m_ffmFontFamily;
    private float? m_fltFontSizeEM;
    private FontStyle? m_fstFontStyle;
    private string m_strMessage;
    private Color? m_clrColour;
    private Color? m_clrHighlight;

    #region Properties

    /// <summary>
    /// Gets the font family to apply to the plugin item.
    /// </summary>
    /// <remarks>
    /// If this value is <lang cref="null"/> then no change should be made to
    /// the plugin's item's font family.
    /// </remarks>
    /// <value>The font family to apply to the plugin item.</value>
    public FontFamily FontFamily
    {
      get
      {
        return m_ffmFontFamily;
      }
    }

    /// <summary>
    /// Gets the font size to apply to the plugin item.
    /// </summary>
    /// <remarks>
    /// If this value is <lang cref="null"/> then no change should be made to
    /// the plugin's item's font size.
    /// </remarks>
    /// <value>The font size to apply to the plugin item.</value>
    public float? FontSizeEM
    {
      get
      {
        return m_fltFontSizeEM;
      }
    }

    /// <summary>
    /// Gets the font style to apply to the plugin item.
    /// </summary>
    /// <remarks>
    /// If this value is <lang cref="null"/> then no change should be made to
    /// the plugin's item's font style.
    /// </remarks>
    /// <value>The font style to apply to the plugin item.</value>
    public FontStyle? FontStyle
    {
      get
      {
        return m_fstFontStyle;
      }
    }

    /// <summary>
    /// Gets the font colour to apply to the plugin item.
    /// </summary>
    /// <remarks>
    /// If this value is <lang cref="null"/> then no change should be made to
    /// the plugin's item's font colour.
    /// </remarks>
    /// <value>The font colour to apply to the plugin item.</value>
    public Color? Colour
    {
      get
      {
        return m_clrColour;
      }
    }

    /// <summary>
    /// Gets the highlight to apply to the plugin item.
    /// </summary>
    /// <remarks>
    /// If this value is <lang cref="null"/> then no change should be made to
    /// the plugin's item's highlight.
    /// </remarks>
    /// <value>The highlight to apply to the plugin item.</value>
    public Color? Highlight
    {
      get
      {
        return m_clrHighlight;
      }
    }

    /// <summary>
    /// Gets the message to show for the plugin.
    /// </summary>
    /// <remarks>
    /// If this value is <lang cref="null"/> then no change should be made to
    /// the plugin's description.
    /// </remarks>
    /// <value>The message to show for the plugin.</value>
    public string Message
    {
      get
      {
        return m_strMessage;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    protected PluginFormat()
    {
    }

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_intIndex">An indexer that allows the <see cref="PluginFormatterManager"/> to strictly order formats. This parameter is provided by the <see cref="PluginFormatterManager"/></param>
    /// <param name="p_ffmFontFamily">The font family to apply to the plugin item.</param>
    /// <param name="p_fltFontSizeEM">The font size to apply to the plugin item.</param>
    /// <param name="p_fstFontStyle">The font style to apply to the plugin item.</param>
    /// <param name="p_clrColour">The font colour to apply to the plugin item.</param>
    /// <param name="p_clrHighlight">The highlight to apply to the plugin item.</param>
    /// <param name="p_strMessage">The message to show for the plugin.</param>
    protected PluginFormat(Int32 p_intIndex, FontFamily p_ffmFontFamily, float? p_fltFontSizeEM,
                           FontStyle? p_fstFontStyle, Color? p_clrColour, Color? p_clrHighlight, string p_strMessage)
    {
      m_intIndex = p_intIndex;
      m_ffmFontFamily = p_ffmFontFamily;
      m_fltFontSizeEM = p_fltFontSizeEM;
      m_fstFontStyle = p_fstFontStyle;
      m_clrColour = p_clrColour;
      m_clrHighlight = p_clrHighlight;
      m_strMessage = p_strMessage;
    }

    #endregion

    /// <summary>
    /// Merges the font properties of this format with the given base font.
    /// </summary>
    /// <param name="p_fntBaseFont">The font to modify as per the state of this format.</param>
    /// <returns>A font that is the result of altering the base font as described by the format.</returns>
    public Font ResolveFont(Font p_fntBaseFont)
    {
      FontStyle fstStyle = p_fntBaseFont.Style;
      if (FontStyle.HasValue)
      {
        fstStyle |= FontStyle.Value;
      }
      return new Font(FontFamily ?? p_fntBaseFont.FontFamily, FontSizeEM ?? p_fntBaseFont.SizeInPoints, fstStyle);
    }

    /// <summary>
    /// Merges the properties of the given <see cref="PluginFormat"/> with this format.
    /// </summary>
    /// <param name="p_pftFormat">The <see cref="PluginFormat"/> whose properties are to be merged with this object's.</param>
    public void Merge(PluginFormat p_pftFormat)
    {
      m_ffmFontFamily = p_pftFormat.FontFamily ?? m_ffmFontFamily;
      m_fltFontSizeEM = p_pftFormat.FontSizeEM ?? m_fltFontSizeEM;
      if (FontStyle.HasValue && p_pftFormat.FontStyle.HasValue)
      {
        m_fstFontStyle = FontStyle.Value | p_pftFormat.FontStyle.Value;
      }
      else
      {
        m_fstFontStyle = p_pftFormat.FontStyle ?? m_fstFontStyle;
      }
      m_clrColour = p_pftFormat.Colour ?? m_clrColour;
      m_clrHighlight = p_pftFormat.Highlight ?? m_clrHighlight;
      if (!String.IsNullOrEmpty(m_strMessage) && !String.IsNullOrEmpty(p_pftFormat.Message))
      {
        m_strMessage += @"\par ";
      }
      m_strMessage += p_pftFormat.Message;
    }

    #region IComparable<PluginFormat> Members

    /// <summary>
    /// Compares this <see cref="PluginFormat"/> to the given <see cref="PluginFormat"/>
    /// </summary>
    /// <remarks>
    /// <see cref="PluginFormat"/>s are strictly ordered by their <see cref="PluginFormat.m_intIndex"/>.
    /// </remarks>
    /// <param name="other">The <see cref="PluginFormat"/> to which to compare this one.</param>
    /// <returns>A value less than 0 if this instance is less than <paramref name="other"/>, or
    /// 0 if this instance is equal to <paramref name="other"/>, or
    /// a value greater than 0 if this instance is greater than <paramref name="other"/>.
    /// </returns>
    public int CompareTo(PluginFormat other)
    {
      return m_intIndex.CompareTo(other.m_intIndex);
    }

    #endregion

    #region IEquatable<PluginFormat> Members

    /// <summary>
    /// Determines if the given <see cref="PluginFormat"/> is equal to this one.
    /// </summary>
    /// <remarks>
    /// Two <see cref="PluginFormat"/>s are equal if and only if their
    /// <see cref="PluginFormat.m_intIndex"/>s are equal.
    /// </remarks>
    /// <param name="other">The <see cref="PluginFormat"/> to which to compare this one.</param>
    /// <returns><lang cref="true"/> if the given <see cref="PluginFormat"/> is equal to this one;
    /// <lang cref="false"/> otherwise.</returns>
    public bool Equals(PluginFormat other)
    {
      return CompareTo(other) == 0;
    }

    #endregion
  }
}