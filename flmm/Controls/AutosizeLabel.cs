using System;
using System.Drawing;
using System.Windows.Forms;

namespace Fomm.Controls
{
  /// <summary>
  /// A multiline label that resizes with the content.
  /// </summary>
  public class AutosizeLabel : RichTextBox
  {
    /// <summary>
    /// The default constructor.
    /// </summary>
    public AutosizeLabel()
    {
      Multiline = true;
      ScrollBars = RichTextBoxScrollBars.None;
      BorderStyle = BorderStyle.None;
      BackColor = SystemColors.Control;
      ReadOnly = true;
      TabStop = false;
      Cursor = Cursors.Arrow;
      Enter += delegate
      {
        Form findForm = FindForm();
        if (findForm != null)
        {
          findForm.Controls["nonExistant"].Focus();
        }
      };
      SetTextColor();
    }

    /// <summary>
    /// Resizes the label as the content size changes.
    /// </summary>
    /// <param name="e">A <see cref="ContentsResizedEventArgs"/> describing the event arguments.</param>
    protected override void OnContentsResized(ContentsResizedEventArgs e)
    {
      Height = e.NewRectangle.Height + 5;
      base.OnContentsResized(e);
    }

    /// <summary>
    /// Makes sure all text doesn't look disabled.
    /// </summary>
    /// <param name="e">A <see cref="EventArgs"/> describing the event arguments.</param>
    protected override void OnTextChanged(EventArgs e)
    {
      base.OnTextChanged(e);
      SetTextColor();
    }

    /// <summary>
    /// Forces the text color not to look disabled.
    /// </summary>
    protected void SetTextColor()
    {
      SelectAll();
      SelectionColor = SystemColors.ControlText;
      Select(0, 0);
    }
  }
}