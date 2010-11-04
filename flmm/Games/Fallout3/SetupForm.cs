using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Fomm.Controls;
using System.IO;

namespace Fomm.Games.Fallout3
{
	/// <summary>
	/// This is the setup form for the Fallout 3 game mode.
	/// </summary>
	public partial class SetupForm : Form
	{
		#region Contructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public SetupForm()
		{
			InitializeComponent();
			this.Icon = Properties.Resources.fomm02;

			tbxModDirectory.Text = Properties.Settings.Default.fallout3ModDirectory;
			if (String.IsNullOrEmpty(tbxModDirectory.Text))
				tbxModDirectory.Text = Path.Combine(Program.GameMode.PersonalDirectory, Path.Combine("fomm", Path.Combine("Fallout 3","mods")));
		}

		#endregion

		#region Validation

		/// <summary>
		/// Validates the selected mod directory.
		/// </summary>
		/// <returns><lang cref="true"/> if the selected mod directory is valid;
		/// <lang cref="false"/> otherwise.</returns>
		protected bool ValidateModDirectory()
		{
			erpValidator.SetError(butModDirectory, null);
			if (String.IsNullOrEmpty(tbxModDirectory.Text))
			{
				erpValidator.SetError(butModDirectory, "You must select a Mod Directory.");
				return false;
			}
			if (!Directory.Exists(tbxModDirectory.Text))
			{
				if (MessageBox.Show(this, "The selected Mod Directory does not exist." + Environment.NewLine + "Would you like to create it?", "Missing Directory", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					Directory.CreateDirectory(tbxModDirectory.Text);
					return true;
				}
				erpValidator.SetError(butModDirectory, "Mod Directory does not exist.");
				return false;
			}
			return true;
		}

		#endregion

		#region Navigation

		/// <summary>
		/// Handles the <see cref="WizardControl.SelectedTabPageChanged"/> event of the wizard control.
		/// </summary>
		/// <remarks>
		/// This validates each page as it is navigated away from.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="VerticalTabControl.TabPageEventArgs"/> describing the event arguments.</param>
		private void wizSetup_SelectedTabPageChanged(object sender, VerticalTabControl.TabPageEventArgs e)
		{
			if (e.TabPage == vtpDirectories)
			{
				if (!ValidateModDirectory())
					wizSetup.SelectedTabPage = e.TabPage;
			}
		}

		/// <summary>
		/// Handles the <see cref="WizardControl.Cancelled"/> event of the wizard control.
		/// </summary>
		/// <remarks>
		/// This cancels the wizard.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void wizSetup_Cancelled(object sender, EventArgs e)
		{
			if (MessageBox.Show(this, "If you cancel the setup FOMM will close.", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
				DialogResult = DialogResult.Cancel;
		}

		/// <summary>
		/// Handles the <see cref="WizardControl.Finished"/> event of the wizard control.
		/// </summary>
		/// <remarks>
		/// This finishes the wizard and persists the selected values.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void wizSetup_Finished(object sender, EventArgs e)
		{
			if (ValidateModDirectory())
			{
				Properties.Settings.Default.fallout3ModDirectory = tbxModDirectory.Text;
				Properties.Settings.Default.Save();
				DialogResult = DialogResult.OK;
			}
		}

		#endregion

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the select mod directory button.
		/// </summary>
		/// <remarks>
		/// This opens the folder selection dialog for the mod directory.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butModDirectory_Click(object sender, EventArgs e)
		{
			fbdModDirectory.SelectedPath = tbxModDirectory.Text;
			if (fbdModDirectory.ShowDialog(this) == DialogResult.OK)
				tbxModDirectory.Text = fbdModDirectory.SelectedPath;
		}
	}
}
