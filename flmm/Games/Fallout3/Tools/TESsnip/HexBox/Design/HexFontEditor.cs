using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Fomm.Games.Fallout3.Tools.TESsnip.HexBox.Design
{
  /// <summary>
  /// Display only fixed-piched fonts
  /// </summary>
  internal class HexFontEditor : FontEditor
  {
    private object value;

    /// <summary>
    /// Edits the pValue
    /// </summary>
    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object pValue)
    {
      value = pValue;
      if (provider != null)
      {
        IWindowsFormsEditorService service1 =
          (IWindowsFormsEditorService) provider.GetService(typeof (IWindowsFormsEditorService));
        if (service1 != null)
        {
          FontDialog fontDialog = new FontDialog();
          fontDialog.ShowApply = false;
          fontDialog.ShowColor = false;
          fontDialog.AllowVerticalFonts = false;
          fontDialog.AllowScriptChange = false;
          fontDialog.FixedPitchOnly = true;
          fontDialog.ShowEffects = false;
          fontDialog.ShowHelp = false;

          Font font = pValue as Font;
          if (font != null)
          {
            fontDialog.Font = font;
          }
          if (fontDialog.ShowDialog() == DialogResult.OK)
          {
            value = fontDialog.Font;
          }

          fontDialog.Dispose();
        }
      }

      pValue = value;
      value = null;
      return pValue;
    }

    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.Modal;
    }
  }
}