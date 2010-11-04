using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Fomm.Controls;

namespace Fomm.Games.Fallout3.Settings
{
	public partial class UpdateSettingsPage : SettingsPage
	{
		public UpdateSettingsPage()
		{
			InitializeComponent();
		}

		public override void Load()
		{
			tbxBOSSUrl.Text = Properties.Settings.Default.fallout3MasterListUpdateUrl;
		}

		public override void Save()
		{
			Properties.Settings.Default.fallout3MasterListUpdateUrl = tbxBOSSUrl.Text;
		}
	}
}
