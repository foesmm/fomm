using System;
using System.Windows.Forms;
using Microsoft.Win32;
using SevenZip;

namespace Fomm
{
	partial class SettingsForm : Form
	{
		private readonly bool FinishedSetup;
		private bool m_booInternal = true;

		public SettingsForm(bool Internal)
		{
			InitializeComponent();
			this.Icon = Fomm.Properties.Resources.fomm02;
			m_booInternal = Internal;

			FinishedSetup = false;
			LoadGeneralOptions();
			LoadFOMODOptions();
			LoadUpdatesOptions();
			FinishedSetup = true;
		}

		protected void LoadGeneralOptions()
		{
			string tmp;
			if (m_booInternal)
			{
				cbFomod.Enabled = false;
				tbFomod.Text = "Cannot change this option while fomm is running";
				cbFallout.Enabled = false;
				tbFallout.Text = "Cannot change this option while fomm is running";
			}
			else
			{
				tmp = Settings.GetString("FomodDir");
				if (tmp != null)
				{
					cbFomod.Checked = true;
					tbFomod.Text = tmp;
				}
				tmp = Settings.GetString("FalloutDir");
				if (tmp != null)
				{
					cbFallout.Checked = true;
					tbFallout.Text = tmp;
				}
			}
			tmp = Settings.GetString("LaunchCommand");
			if (tmp != null)
			{
				cbLaunch.Checked = true;
				tbLaunch.Text = tmp;
				tmp = Settings.GetString("LaunchCommandArgs");
				if (tmp != null) tbLaunchArgs.Text = tmp;
			}
			cbEsmShow.Checked = Settings.GetBool("ShowEsmInBold");
			cbDisableUAC.Checked = Settings.GetBool("NoUACCheck");
			cbDisableIPC.Checked = Settings.GetBool("DisableIPC");
			cbUseDocs.Checked = Settings.GetBool("UseDocsFolder");
			ckbCheckFomodVersions.Checked = Settings.GetBool("checkForNewModVersions");
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
			if (key == null) cbShellExtensions.Enabled = false;
			else
			{
				if (Registry.GetValue("HKEY_CLASSES_ROOT\\" + key + "\\Shell\\Convert_to_fomod\\command", null, null) != null) cbShellExtensions.Checked = true;
			}
		}

		protected void LoadFOMODOptions()
		{	
			cbxFomodCompression.DataSource = Enum.GetValues(typeof(CompressionLevel));
			cbxFomodFormat.DataSource = Enum.GetValues(typeof(OutArchiveFormat));
			cbxFomodCompression.SelectedItem = (CompressionLevel)Settings.GetInt("fomodCompressionLevel", (Int32)CompressionLevel.Ultra);
			cbxFomodFormat.SelectedItem = (OutArchiveFormat)Settings.GetInt("fomodCompressionFormat", (Int32)OutArchiveFormat.Zip);

			cbxPFPCompression.DataSource = Enum.GetValues(typeof(CompressionLevel));
			cbxPFPFormat.DataSource = Enum.GetValues(typeof(OutArchiveFormat));
			cbxPFPCompression.SelectedItem = (CompressionLevel)Settings.GetInt("pfpCompressionLevel", (Int32)CompressionLevel.Ultra);
			cbxPFPFormat.SelectedItem = (OutArchiveFormat)Settings.GetInt("pfpCompressionFormat", (Int32)OutArchiveFormat.SevenZip);
		}

		protected void LoadUpdatesOptions()
		{
			tbxBOSSUrl.Text = Properties.Settings.Default.fallout3MasterListUpdateUrl;
		}

		private void SetupForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (cbFomod.Checked) Settings.SetString("FomodDir", tbFomod.Text);
			else Settings.RemoveString("FomodDir");
			if (cbFallout.Checked) Settings.SetString("FalloutDir", tbFallout.Text);
			else Settings.RemoveString("FalloutDir");
			if (cbLaunch.Checked)
			{
				Settings.SetString("LaunchCommand", tbLaunch.Text);
				Settings.SetString("LaunchCommandArgs", tbLaunchArgs.Text);
			}
			else
			{
				Settings.RemoveString("LaunchCommand");
				Settings.RemoveString("LaunchCommandArgs");
			}
			Settings.SetInt("fomodCompressionFormat", (Int32)cbxFomodFormat.SelectedItem);
			Settings.SetInt("fomodCompressionLevel", (Int32)cbxFomodCompression.SelectedItem);
			Settings.SetInt("pfpCompressionFormat", (Int32)cbxPFPFormat.SelectedItem);
			Settings.SetInt("pfpCompressionLevel", (Int32)cbxPFPCompression.SelectedItem);
			Settings.SetString("MasterListUpdateUrl", tbxBOSSUrl.Text);
		}

		private void cbFomod_CheckedChanged(object sender, EventArgs e)
		{
			tbFomod.ReadOnly = !cbFomod.Checked;
			bBrowseFomod.Enabled = cbFomod.Checked;
		}

		private void cbFallout_CheckedChanged(object sender, EventArgs e)
		{
			tbFallout.ReadOnly = !cbFallout.Checked;
			bBrowseFallout.Enabled = cbFallout.Checked;
		}

		private void cbLaunch_CheckedChanged(object sender, EventArgs e)
		{
			tbLaunch.ReadOnly = !cbLaunch.Checked;
			tbLaunchArgs.ReadOnly = !cbLaunch.Checked;
		}

		private void bBrowseFomod_Click(object sender, EventArgs e)
		{
			if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;
			tbFomod.Text = folderBrowserDialog1.SelectedPath;
		}

		private void bBrowseFallout_Click(object sender, EventArgs e)
		{
			if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;
			tbFallout.Text = folderBrowserDialog1.SelectedPath;
		}

		private void cbAssociateFomod_CheckedChanged(object sender, EventArgs e)
		{
			if (!FinishedSetup) return;
			if (!cbAssociateFomod.Checked)
			{
				Registry.ClassesRoot.DeleteSubKeyTree("FOMM_Mod_Archive");
				Registry.ClassesRoot.DeleteSubKeyTree(".fomod");
			}
			else
			{
				Registry.SetValue(@"HKEY_CLASSES_ROOT\.fomod", null, "FOMM_Mod_Archive");
				Registry.SetValue(@"HKEY_CLASSES_ROOT\FOMM_Mod_Archive", null, "Fallout Mod Manager Archive", RegistryValueKind.String);
				Registry.SetValue(@"HKEY_CLASSES_ROOT\FOMM_Mod_Archive\DefaultIcon", null, Application.ExecutablePath + ",0", RegistryValueKind.String);
				Registry.SetValue(@"HKEY_CLASSES_ROOT\FOMM_Mod_Archive\shell\open\command", null, "\"" + Application.ExecutablePath + "\" \"%1\"", RegistryValueKind.String);
			}
		}

		private void cbAssociateBsa_CheckedChanged(object sender, EventArgs e)
		{
			if (!FinishedSetup) return;
			if (!cbAssociateBsa.Checked)
			{
				Registry.ClassesRoot.DeleteSubKeyTree("BethesdaSoftworks_Archive");
				Registry.ClassesRoot.DeleteSubKeyTree(".bsa");
			}
			else
			{
				Registry.SetValue(@"HKEY_CLASSES_ROOT\.bsa", null, "BethesdaSoftworks_Archive");
				Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_Archive", null, "Bethesda File Archive", RegistryValueKind.String);
				Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_Archive\DefaultIcon", null, Application.ExecutablePath + ",0", RegistryValueKind.String);
				Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_Archive\shell\open\command", null, "\"" + Application.ExecutablePath + "\" \"%1\"", RegistryValueKind.String);
			}
		}

		private void cbAssociateSdp_CheckedChanged(object sender, EventArgs e)
		{
			if (!FinishedSetup) return;
			if (!cbAssociateSdp.Checked)
			{
				Registry.ClassesRoot.DeleteSubKeyTree("BethesdaSoftworks_ShaderPackage");
				Registry.ClassesRoot.DeleteSubKeyTree(".sdp");
			}
			else
			{
				Registry.SetValue(@"HKEY_CLASSES_ROOT\.sdp", null, "BethesdaSoftworks_ShaderPackage");
				Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_ShaderPackage", null, "Bethesda Shader Package", RegistryValueKind.String);
				Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_ShaderPackage\DefaultIcon", null, Application.ExecutablePath + ",0", RegistryValueKind.String);
				Registry.SetValue(@"HKEY_CLASSES_ROOT\BethesdaSoftworks_ShaderPackage\shell\open\command", null, "\"" + Application.ExecutablePath + "\" \"%1\"", RegistryValueKind.String);
			}
		}

		private static void AddShellExtension(string key)
		{
			if (key == null) return;
			Registry.SetValue("HKEY_CLASSES_ROOT\\" + key + "\\Shell\\Convert_to_fomod", null, "Convert to fomod");
			Registry.SetValue("HKEY_CLASSES_ROOT\\" + key + "\\Shell\\Convert_to_fomod\\command", null, "\"" + Application.ExecutablePath + "\" \"%1\"", RegistryValueKind.String);
		}
		private static void RemoveShellExtension(string key)
		{
			if (key == null) return;
			RegistryKey rk = Registry.ClassesRoot.OpenSubKey(key + "\\Shell", true);
			if (Array.IndexOf<string>(rk.GetSubKeyNames(), "Convert_to_fomod") != -1) rk.DeleteSubKeyTree("Convert_to_fomod");
			rk.Close();
		}
		private void cbShellExtensions_CheckedChanged(object sender, EventArgs e)
		{
			if (!FinishedSetup) return;
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

		private void bEsmShow_CheckedChanged(object sender, EventArgs e)
		{
			if (!FinishedSetup) return;
			Settings.SetBool("ShowEsmInBold", cbEsmShow.Checked);
		}

		private void cbDisableIPC_CheckedChanged(object sender, EventArgs e)
		{
			if (!FinishedSetup) return;
			Settings.SetBool("DisableIPC", cbDisableIPC.Checked);
		}

		private void cbDisableUAC_CheckedChanged(object sender, EventArgs e)
		{
			if (!FinishedSetup) return;
			Settings.SetBool("NoUACCheck", cbDisableUAC.Checked);
		}

		private void cbUseDocs_CheckedChanged(object sender, EventArgs e)
		{
			if (!FinishedSetup) return;
			Settings.SetBool("UseDocsFolder", cbUseDocs.Checked);
		}

		/// <summary>
		/// Handles the <see cref="CheckBox.CheckedChanged"/> event of the check for new fomod version checkbox.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void ckbCheckFomodVersions_CheckedChanged(object sender, EventArgs e)
		{
			if (!FinishedSetup) return;
			Settings.SetBool("checkForNewModVersions", ckbCheckFomodVersions.Checked);
		}
	}
}