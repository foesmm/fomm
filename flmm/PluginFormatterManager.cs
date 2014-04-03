using System;
using System.Collections.Generic;
using System.Drawing;
using Fomm.Util;

namespace Fomm
{
  public partial class PluginFormat
  {
    /// <summary>
    /// Manages all of the <see cref="IPluginFormatProvider"/>s.
    /// </summary>
    public class PluginFormatterManager
    {
      private Int32 m_intIndex;
      private Set<IPluginFormatProvider> m_setProviders = new Set<IPluginFormatProvider>();

      /// <summary>
      /// This registers a <see cref="IPluginFormatProvider"/> with the manager.
      /// </summary>
      /// <param name="p_pfpProvider">The provider to register.</param>
      public void RegisterProvider(IPluginFormatProvider p_pfpProvider)
      {
        p_pfpProvider.PluginFormatterManager = this;
        m_setProviders.Add(p_pfpProvider);
      }

      /// <summary>
      /// Creates a <see cref="PluginFormat"/> based on the given values.
      /// </summary>
      /// <remarks>
      /// Only the manager can create <see cref="PluginFormat"/>s. This is so a strict ordering
      /// can be enforced across the <see cref="IPluginFormatProvider"/>s, ensuring the last one
      /// to change a plugin's format wins.
      /// </remarks>
      /// <param name="p_ffmFontFamily">The desired font family, or <lang langref="null"/> if the format won't affect the font family.</param>
      /// <param name="p_fltFontSizeEM">The desired font size in em-points, or <lang langref="null"/> if the format won't affect the font size.</param>
      /// <param name="p_fstFontStyle">The desired font style, or <lang langref="null"/> if the format won't affect the font style.</param>
      /// <param name="p_clrColour">The desired font colour, or <lang langref="null"/> if the format won't affect the font colour.</param>
      /// <param name="p_clrHighlight">The desired highlight colour, or <lang langref="null"/> if the format won't affect the highlight colour.</param>
      /// <param name="p_strMessage">The desired message, or <lang langref="null"/> if the format won't affect the message.</param>
      /// <returns>A <see cref="PluginFormat"/> representing the given values.</returns>
      public PluginFormat CreateFormat(FontFamily p_ffmFontFamily, float? p_fltFontSizeEM, FontStyle? p_fstFontStyle,
                                       Color? p_clrColour, Color? p_clrHighlight, string p_strMessage)
      {
        return new PluginFormat(m_intIndex++, p_ffmFontFamily, p_fltFontSizeEM, p_fstFontStyle, p_clrColour,
                                p_clrHighlight, p_strMessage);
      }

      /// <summary>
      /// Gets the format to apply to the specified plugin.
      /// </summary>
      /// <remarks>
      /// Ths return <see cref="PluginFormat"/> is the result of merging a formats for the plugin
      /// from all registered providers. In case where format properties conflict, the lsat one to
      /// change the property wins.
      /// </remarks>
      /// <param name="p_strPluginName">The name of the plugin for which to retrieve the plugin format.</param>
      /// <returns>The format to apply to the specified plugin.</returns>
      public PluginFormat GetFormat(string p_strPluginName)
      {
        var lstFormats = new List<PluginFormat>();
        foreach (var pfpProvider in m_setProviders)
        {
          if (pfpProvider.HasFormat(p_strPluginName))
          {
            lstFormats.Add(pfpProvider.GetFormat(p_strPluginName));
          }
        }
        lstFormats.Sort();
        var pftMergedFormat = new PluginFormat();
        for (var i = 0; i < lstFormats.Count; pftMergedFormat.Merge(lstFormats[i++]))
        {
        }
        return pftMergedFormat;
      }
    }
  }
}