using System;
using System.Windows.Forms;
using Fomm.Properties;

namespace Fomm.PackageManager
{
  internal enum TextEditorType
  {
    Text
  }

  partial class TextEditor : Form
  {
    private string saved;
    private static RichTextBox rtbEdit;

    private TextEditor(string text, TextEditorType type)
    {
      InitializeComponent();
      Icon = Resources.fomm02;
      Settings.Default.windowPositions.GetWindowPosition("TextEditor", this);
      switch (type)
      {
        case TextEditorType.Text:
          rtbEdit = new RichTextBox();
          panel1.Controls.Add(rtbEdit);
          rtbEdit.Text = text;
          rtbEdit.Dock = DockStyle.Fill;
          rtbEdit.TextChanged += textChanged;
          Text = "Readme editor";
          break;
      }
    }

    private bool changed;

    private void textChanged(object sender, EventArgs e)
    {
      changed = true;
      rtbEdit.TextChanged -= textChanged;
    }

    public static string ShowEditor(string initial, TextEditorType type, bool dialog)
    {
      var se = new TextEditor(initial, type);
      if (dialog)
      {
        se.ShowDialog();
      }
      else
      {
        se.Show();
      }
      return se.saved;
    }

    private void bSave_Click(object sender, EventArgs e)
    {
      if (rtbEdit.TextLength == 0)
      {
        saved = "";
      }
      else
      {
        saved = rtbEdit.Rtf;
      }
      rtbEdit.TextChanged += textChanged;
      changed = false;
    }

    private void TextEditor_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (changed)
      {
        switch (MessageBox.Show("Save changes?", "", MessageBoxButtons.YesNoCancel))
        {
          case DialogResult.Yes:
            bSave_Click(null, null);
            break;
          case DialogResult.No:
            break;
          default:
            e.Cancel = true;
            return;
        }
      }
      Settings.Default.windowPositions.SetWindowPosition("TextEditor", this);
      Settings.Default.Save();
    }
  }
}