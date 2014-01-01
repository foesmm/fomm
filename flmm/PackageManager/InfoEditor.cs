using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Fomm.PackageManager
{
	partial class InfoEditor : Form
	{
		private readonly fomod m_fomodMod;

		public InfoEditor(fomod p_fomodMod)
		{
			InitializeComponent();
			this.Icon = Fomm.Properties.Resources.fomm02;
			Properties.Settings.Default.windowPositions.GetWindowPosition("InfoEditor", this);

			m_fomodMod = p_fomodMod;
			finInfo.LoadFomod(m_fomodMod);
		}

		private void butSave_Click(object sender, EventArgs e)
		{
			if (!finInfo.SaveFomod(m_fomodMod))
				MessageBox.Show(this, "You must correct the errors before saving.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			else
				DialogResult = DialogResult.OK;
		}

		private void InfoEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			Properties.Settings.Default.windowPositions.SetWindowPosition("InfoEditor", this);
			Properties.Settings.Default.Save();
		}

		private void butEditReadme_Click(object sender, EventArgs e)
		{
			EditReadmeForm erfEditor = new EditReadmeForm();
			if (!m_fomodMod.HasReadme)
				erfEditor.Readme = new Readme(ReadmeFormat.PlainText, "");
			else
				erfEditor.Readme = m_fomodMod.GetReadme();
			if (erfEditor.ShowDialog(this) == DialogResult.OK)
				m_fomodMod.SetReadme(erfEditor.Readme);
		}
	}
}