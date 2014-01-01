using System;
using System.Windows.Forms;
using Microsoft.Win32;
using SevenZip;
using Fomm.Controls;
using System.Drawing;
using Fomm.Util;

namespace Fomm
{
	/// <summary>
	/// The settings form.
	/// </summary>
	partial class SettingsForm : Form
	{
		private bool booToolTipShown = false;

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public SettingsForm()
		{
			InitializeComponent();
			this.Icon = Fomm.Properties.Resources.fomm02;

			LoadGeneralSettings();
			LoadFOMODSettings();
			LoadGameModeSettings();
		}

		#endregion

		#region Tool Tip

		/// <summary>
		/// Handles the <see cref="Control.MouseHover"/> event of the tab page control.
		/// </summary>
		/// <remarks>
		/// This displays the tool tip for the file associations group box when it is disabled.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void tpgGeneral_MouseHover(object sender, EventArgs e)
		{
			if (!gbxAssociations.Enabled && gbxAssociations.ClientRectangle.Contains(gbxAssociations.PointToClient(Cursor.Position)))
			{
				booToolTipShown = true;
				Point pntToolTipLocation = gbxAssociations.PointToClient(Cursor.Position);
				ttpTip.Show(ttpTip.GetToolTip(gbxAssociations), gbxAssociations, pntToolTipLocation.X, pntToolTipLocation.Y + Cursor.Current.Size.Height);
			}
		}

		/// <summary>
		/// Handles the <see cref="Control.MouseMove"/> event of the tab page control.
		/// </summary>
		/// <remarks>
		/// This hides the tool tip for the file associations group box when appropriate.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="MouseEventArgs"/> describing the event arguments.</param>
		private void tpgGeneral_MouseMove(object sender, MouseEventArgs e)
		{
			if (booToolTipShown && !gbxAssociations.ClientRectangle.Contains(gbxAssociations.PointToClient(Cursor.Position)))
			{
				booToolTipShown = false;
				ttpTip.Hide(gbxAssociations);
			}
		}

		#endregion

		#region Settings Loading

		/// <summary>
		/// Loads the general settings into the controls.
		/// </summary>
		protected void LoadGeneralSettings()
		{
			cbDisableUAC.Checked = Properties.Settings.Default.NoUACCheck;
			cbDisableIPC.Checked = Properties.Settings.Default.DisableIPC;

			if (!UacUtil.IsElevated)
			{
				gbxAssociations.Enabled = false;
				ttpTip.SetToolTip(gbxAssociations, "Run FOMM as Administrator to change these settings.");
			}

			string key = Registry.GetValue(@"HKEY_CLASSES_ROOT\.bsa", null, null) as string;
			switch (key)
			{
				case "BethesdaSoftworks_Archive":
					cbAssociateBsa.Checked = true;
					break;
				case null:
					break;
				default:
					cbAssociateBsa.Enabled = false;
					break;
			}

			key = Registry.GetValue(@"HKEY_CLASSES_ROOT\.sdp", null, null) as string;
			switch (key)
			{
				case "BethesdaSoftworks_ShaderPackage":
					cbAssociateSdp.Checked = true;
					break;
				case null:
					break;
				default:
					cbAssociateSdp.Enabled = false;
					break;
			}

			key = Registry.GetValue(@"HKEY_CLASSES_ROOT\.fomod", null, null) as string;
			switch (key)
			{
				case "FOMM_Mod_Archive":
					cbAssociateFomod.Checked = true;
					break;
				case null:
					break;
				default:
					cbAssociateFomod.Enabled = false;
					break;
			}

			key = Registry.GetValue(@"HKEY_CLASSES_ROOT\.zip", null, null) as string;
			if (key == null)
				cbShellExtensions.Enabled = false;
			else
			{
				if (Registry.GetValue("HKEY_CLASSES_ROOT\\" + key + "\\Shell\\Convert_to_fomod\\command", null, null) != null)
					cbShellExtensions.Checked = true;
			}
		}

		/// <summary>
		/// Loads the FOMod settings into the controls.
		/// </summary>
		protected void LoadFOMODSettings()
		{
			cbxFomodCompression.DataSource = Enum.GetValues(typeof(CompressionLevel));
			cbxFomodFormat.DataSource = Enum.GetValues(typeof(OutArchiveFormat));
			cbxFomodCompression.SelectedItem = Properties.Settings.Default.fomodCompressionLevel;
			cbxFomodFormat.SelectedItem = Properties.Settings.Default.fomodCompressionFormat;

			cbxPFPCompression.DataSource = Enum.GetValues(typeof(CompressionLevel));
			cbxPFPFormat.DataSource = Enum.GetValues(typeof(OutArchiveFormat));
			cbxPFPCompression.SelectedItem = Properties.Settings.Default.pfpCompressionLevel;
			cbxPFPFormat.SelectedItem = Properties.Settings.Default.pfpCompressionFormat;

			cbUseDocs.Checked = Properties.Settings.Default.UseDocsFolder;
		}

		/// <summary>
		/// Adds the game mode <see cref="SettingsPage"/>s to the settings form.
		/// </summary>
		protected void LoadGameModeSettings()
		{
			foreach (SettingsPage spgSettings in Program.GameMode.SettingsPages)
			{
				tbcTabs.TabPages.Add(spgSettings.Name, spgSettings.Text);
				spgSettings.Dock = DockStyle.Fill;
				tbcTabs.TabPages[spgSettings.Name].UseVisualStyleBackColor = true;
				tbcTabs.TabPages[spgSettings.Name].Controls.Add(spgSettings);
				tbcTabs.TabPages[spgSettings.Name].Tag = spgSettings;
				spgSettings.LoadSettings();
			}
		}

		#endregion

		/// <summary>
		/// Hanldes the <see cref="Control.Click"/> event of the OK button.
		/// </summary>
		/// <remarks>
		/// This persists the settings.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butOK_Click(object sender, EventArgs e)
		{
			if (!SaveGameModeSettings())
				return;

			SaveGeneralSettings();
			SaveFOMODSettings();

			Properties.Settings.Default.Save();
			DialogResult = DialogResult.OK;
		}

		#region Settings Persistence

		/// <summary>
		/// Adds a shell extension for the file type represented by the specified key.
		/// </summary>
		/// <param name="key">The key representing the file type for which to add a shell extension.</param>
		private void AddShellExtension(string key)
		{
			if (key == null)
				return;
			Registry.SetValue("HKEY_CLASSES_ROOT\\" + key + "\\Shell\\Convert_to_fomod", null, "Convert to fomod");
			Registry.SetValue("HKEY_CLASSES_ROOT\\" + key + "\\Shell\\Convert_to_fomod\\command", null, "\"" + Application.ExecutablePath + "\" \"%1\"", RegistryValueKind.String);
		}

		/// <summary>
		/// Removes a shell extension for the file type represented by the specified key.
		/// </summary>
		/// <param name="key">The key representing the file type for which to remove a shell extension.</param>
		private void RemoveShellExtension(string key)
		{
			if (key == null) return;
			RegistryKey rk = Registry.ClassesRoot.OpenSubKey(key + "\\Shell", true);
			if (Array.IndexOf<string>(rk.GetSubKeyNames(), "Convert_to_fomod") != -1)
				rk.DeleteSubKeyTree("Convert_to_fomod");
			rk.Close();
		}

		/// <summary>
		/// Persists the general settings.
		/// </summary>
		protected void SaveGeneralSettings()
		{
			Properties.Settings.Default.NoUACCheck = cbDisableUAC.Checked;
			Properties.Settings.Default.DisableIPC = cbDisableIPC.Checked;

			if (UacUtil.IsElevated)
			{
				string[] strKeys = Registry.ClassesRoot.GetSubKeyNames();
				if (!cbAssociateBsa.Checked)
				{
					if (Array.IndexOf<string>(strKeys, "BethesdaSoftworks_Archive") != -1)
					{
						Registry.ClassesRoot.DeleteSubKeyTree("BethesdaSoftworks_Archive");
						Registry.ClassesRoot.DeleteSubKeyTree(".bsa");
					}
				}
				else
				{
					Registry.SetValue(@"HKEY_CLASSES_ROOT\.bsa", null, "BethesdaSoftworks_Archive");
					Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_Archive", null, "Bethesda File Archive", RegistryValueKind.String);
					Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_Archive\DefaultIcon", null, Application.ExecutablePath + ",0", RegistryValueKind.String);
					Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_Archive\shell\open\command", null, "\"" + Application.ExecutablePath + "\" \"%1\"", RegistryValueKind.String);
				}

				if (!cbAssociateSdp.Checked)
				{
					if (Array.IndexOf<string>(strKeys, "BethesdaSoftworks_ShaderPackage") != -1)
					{
						Registry.ClassesRoot.DeleteSubKeyTree("BethesdaSoftworks_ShaderPackage");
						Registry.ClassesRoot.DeleteSubKeyTree(".sdp");
					}
				}
				else
				{
					Registry.SetValue(@"HKEY_CLASSES_ROOT\.sdp", null, "BethesdaSoftworks_ShaderPackage");
					Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_ShaderPackage", null, "Bethesda Shader Package", RegistryValueKind.String);
					Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_ShaderPackage\DefaultIcon", null, Application.ExecutablePath + ",0", RegistryValueKind.String);
					Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_ShaderPackage\shell\open\command", null, "\"" + Application.ExecutablePath + "\" \"%1\"", RegistryValueKind.String);
				}

				if (!cbAssociateFomod.Checked)
				{
					if (Array.IndexOf<string>(strKeys, "FOMM_Mod_Archive") != -1)
					{
						Registry.ClassesRoot.DeleteSubKeyTree("FOMM_Mod_Archive");
						Registry.ClassesRoot.DeleteSubKeyTree(".fomod");
					}
				}
				else
				{
					Registry.SetValue(@"HKEY_CLASSES_ROOT\.fomod", null, "FOMM_Mod_Archive");
					Registry.SetValue(@"HKEY_CLASSES_ROOT\FOMM_Mod_Archive", null, "Fallout Mod Manager Archive", RegistryValueKind.String);
					Registry.SetValue(@"HKEY_CLASSES_ROOT\FOMM_Mod_Archive\DefaultIcon", null, Application.ExecutablePath + ",0", RegistryValueKind.String);
					Registry.SetValue(@"HKEY_CLASSES_ROOT\FOMM_Mod_Archive\shell\open\command", null, "\"" + Application.ExecutablePath + "\" \"%1\"", RegistryValueKind.String);
				}

				if (cbShellExtensions.Checked)
				{
					AddShellExtension(Registry.GetValue(@"HKEY_CLASSES_ROOT\.zip", null, null) as string);
					AddShellExtension(Registry.GetValue(@"HKEY_CLASSES_ROOT\.rar", null, null) as string);
					AddShellExtension(Registry.GetValue(@"HKEY_CLASSES_ROOT\.7z", null, null) as string);
				}
				else
				{
					RemoveShellExtension(Registry.GetValue(@"HKEY_CLASSES_ROOT\.zip", null, null) as string);
					RemoveShellExtension(Registry.GetValue(@"HKEY_CLASSES_ROOT\.rar", null, null) as string);
					RemoveShellExtension(Registry.GetValue(@"HKEY_CLASSES_ROOT\.7z", null, null) as string);
				}
			}
		}

		/// <summary>
		/// Persists the FOMod settings.
		/// </summary>
		protected void SaveFOMODSettings()
		{
			Properties.Settings.Default.fomodCompressionLevel = (CompressionLevel)cbxFomodCompression.SelectedItem;
			Properties.Settings.Default.fomodCompressionFormat = (OutArchiveFormat)cbxFomodFormat.SelectedItem;

			Properties.Settings.Default.pfpCompressionLevel = (CompressionLevel)cbxPFPCompression.SelectedItem;
			Properties.Settings.Default.pfpCompressionFormat = (OutArchiveFormat)cbxPFPFormat.SelectedItem;

			Properties.Settings.Default.UseDocsFolder = cbUseDocs.Checked;
		}

		/// <summary>
		/// Persists the game-mode specific settings.
		/// </summary>
		/// <returns><lang cref="true"/> if ettings were saved;
		/// <lang cref="false"/> otherwise.</returns>
		protected bool SaveGameModeSettings()
		{
			bool booIsValid = true;
			bool booIsPageValid = true;
			foreach (TabPage tpgSettings in tbcTabs.TabPages)
				if (tpgSettings.Tag is SettingsPage)
				{
					booIsPageValid = ((SettingsPage)tpgSettings.Tag).SaveSettings();
					booIsValid &= booIsPageValid;
					if (!booIsPageValid)
						tbcTabs.SelectedTab = tpgSettings;
				}
			return booIsValid;
		}

		#endregion
	}
}