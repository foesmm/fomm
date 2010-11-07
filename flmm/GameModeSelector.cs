using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Fomm.Games;

namespace Fomm
{
	public partial class GameModeSelector : Form
	{
		public SupportedGameModes SelectedGameMode
		{
			get
			{
				if (radFallout3.Checked)
					return SupportedGameModes.Fallout3;
				if (radFalloutNV.Checked)
					return SupportedGameModes.FalloutNV;
				throw new Exception("Unrecognized game selection.");
			}
		}

		public GameModeSelector()
		{
			InitializeComponent();
			this.Icon = Properties.Resources.fomm02;
		}

		private void butOK_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.rememberGameMode = cbxRemember.Checked;
			if (cbxRemember.Checked)
				Properties.Settings.Default.rememberedGameMode = SelectedGameMode;
			Properties.Settings.Default.Save();
			DialogResult = DialogResult.OK;
		}
	}
}
