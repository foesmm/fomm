using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Fomm.Games.Fallout3.Tools.TESsnip.HexBox.Design
{
  /// <summary>
  ///   Display only fixed-piched fonts
  /// </summary>
  internal class HexFontEditor : FontEditor
  {
    private object value;

    /// <summary>
    ///   Edits the value
    /// </summary>
    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider,
                                     object newValue)
    {
      value = newValue;
      if (provider != null)
      {
        var service1 =
          (IWindowsFormsEditorService) provider.GetService(typeof (IWindowsFormsEditorService));
        if (service1 != null)
        {
          var fontDialog = new FontDialog();
          fontDialog.ShowApply = false;
          fontDialog.ShowColor = false;
          fontDialog.AllowVerticalFonts = false;
          fontDialog.AllowScriptChange = false;
          fontDialog.FixedPitchOnly = true;
          fontDialog.ShowEffects = false;
          fontDialog.ShowHelp = false;

          var font = newValue as Font;
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

      newValue = value;
      value = null;
      return newValue;
    }

    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.Modal;
    }
  }
}