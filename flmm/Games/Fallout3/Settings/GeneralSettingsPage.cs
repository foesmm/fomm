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
	/// <summary>
	/// The page of general settings for the Fallout 3 game mode.
	/// </summary>
	public partial class GeneralSettingsPage : SettingsPage
	{
		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public GeneralSettingsPage()
		{
			InitializeComponent();
			Text = "Fallout 3";
		}

		#endregion

		#region Settings Management

		/// <summary>
		/// Loads the settings into the page's controls.
		/// </summary>
		public override void LoadSettings()
		{
			tbxModDirectory.Text = Properties.Settings.Default.fallout3ModDirectory;
			tbxWorkingDirectory.Text = Properties.Settings.Default.fallout3WorkingDirectory;
			tbxCommand.Text = Properties.Settings.Default.fallout3LaunchCommand;
			tbxCommandArguments.Text = Properties.Settings.Default.fallout3LaunchCommandArgs;
		}

		/// <summary>
		/// Persists the settings from the page's controls.
		/// </summary>
		/// <returns><lang cref="true"/> if the changed settings require a programme restart;
		/// <lang cref="false"/> otherwise.</returns>
		public override bool SaveSettings()
		{
			Properties.Settings.Default.fallout3ModDirectory = tbxModDirectory.Text;
			Properties.Settings.Default.fallout3LaunchCommand = tbxCommand.Text;
			Properties.Settings.Default.fallout3LaunchCommandArgs = tbxCommandArguments.Text;

			if (!tbxWorkingDirectory.Text.Equals(Properties.Settings.Default.fallout3WorkingDirectory))
			{
				Properties.Settings.Default.fallout3WorkingDirectory = tbxWorkingDirectory.Text;
				return true;
			}
			return false;
		}

		#endregion

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the select mod directory button.
		/// </summary>
		/// <remarks>
		/// This opens the folder selection dialog for the selection of the mod directory.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butSelectModDirectory_Click(object sender, EventArgs e)
		{
			fbdModDirectory.SelectedPath = tbxModDirectory.Text;
			if (fbdModDirectory.ShowDialog(this.FindForm()) == DialogResult.OK)
				tbxModDirectory.Text = fbdModDirectory.SelectedPath;
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the select working directory button.
		/// </summary>
		/// <remarks>
		/// This opens the folder selection dialog for the selection of the working directory.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butSelectWorkingDirectory_Click(object sender, EventArgs e)
		{
			fbdWorkingDirectory.SelectedPath = tbxWorkingDirectory.Text;
			if (fbdWorkingDirectory.ShowDialog(this.FindForm()) == DialogResult.OK)
				tbxWorkingDirectory.Text = fbdWorkingDirectory.SelectedPath;
		}
	}
}
