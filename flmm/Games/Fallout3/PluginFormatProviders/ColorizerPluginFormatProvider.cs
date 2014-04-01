using System;
using System.IO;
using System.Drawing;
using Fomm.Games.Fallout3.Tools.TESsnip;

namespace Fomm.Games.Fallout3.PluginFormatProviders
{
  // Colorizes plugins based on their dependency status.
  public class ColorizerPluginFormatProvider : IPluginFormatProvider
  {
    private PluginFormat.PluginFormatterManager m_pfmManager = null;

    #region IPluginFormatProvider Members

    // Sets the PluginFormat.PluginFormatterManager to use.
    public PluginFormat.PluginFormatterManager PluginFormatterManager
    {
      set
      {
        m_pfmManager = value;
      }
    }

    public virtual bool HasFormat(string p_strPluginName)
    {
      return true;
    }

    // Gets the provider color for the specified plugin.
    public virtual PluginFormat GetFormat(string p_strPluginName)
    {
      Color clr;

      clr = Color.Black;
      switch (Program.GameMode.getPluginDependencyStatus(p_strPluginName))
      {
        case 1:
          // Missing master
          clr = Color.DarkRed;
        break;

        case 2:
          // Present but disabled
          clr = Color.DarkOrange;
        break;

        case 3:
          // Present and active but in wrong order
          clr = Color.Sienna;
        break;

        default:
        case 0:
          clr = Color.Black;
        break;
      }

      return m_pfmManager.CreateFormat(null, null, null, clr, null, null);
    }

    #endregion
  }
}
