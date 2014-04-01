using System;
using System.Windows.Forms;
using Fomm.Properties;

namespace Fomm.PackageManager
{
  partial class InfoEditor : Form
  {
    private readonly fomod m_fomodMod;

    public InfoEditor(fomod p_fomodMod)
    {
      InitializeComponent();
      Icon = Resources.fomm02;
      Settings.Default.windowPositions.GetWindowPosition("InfoEditor", this);

      m_fomodMod = p_fomodMod;
      finInfo.LoadFomod(m_fomodMod);
    }

    private void butSave_Click(object sender, EventArgs e)
    {
      if (!finInfo.SaveFomod(m_fomodMod))
      {
        MessageBox.Show(this, "You must correct the errors before saving.", Resources.ErrorStr, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
      }
      else
      {
        DialogResult = DialogResult.OK;
      }
    }

    private void InfoEditor_FormClosing(object sender, FormClosingEventArgs e)
    {
      Settings.Default.windowPositions.SetWindowPosition("InfoEditor", this);
      Settings.Default.Save();
    }

    private void butEditReadme_Click(object sender, EventArgs e)
    {
      EditReadmeForm erfEditor = new EditReadmeForm();
      erfEditor.Readme = !m_fomodMod.HasReadme ? new Readme(ReadmeFormat.PlainText, "") : m_fomodMod.GetReadme();
      if (erfEditor.ShowDialog(this) == DialogResult.OK)
      {
        m_fomodMod.SetReadme(erfEditor.Readme);
      }
    }
  }
}