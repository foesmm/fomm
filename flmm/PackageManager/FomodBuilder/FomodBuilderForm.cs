using System;
using System.Windows.Forms;
using Fomm.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using Fomm.Util;
using Fomm.PackageManager.XmlConfiguredInstall;
using System.Xml.Schema;
using System.Xml;
using System.Drawing;
using Fomm.PackageManager.Controls;

namespace Fomm.PackageManager.FomodBuilder
{
	/// <summary>
	/// This form builds a FOMod form existing files.
	/// </summary>
	public partial class FomodBuilderForm : Form
	{
		/// <summary>
		/// The possible validation states of the form.
		/// </summary>
		protected enum ValidationState
		{
			/// <summary>
			/// Indicates there are no errors or warnings.
			/// </summary>
			Passed,

			/// <summary>
			/// Indicates there are warnings.
			/// </summary>
			/// <remarks>
			/// Warnings are non-fatal errors.
			/// </remarks>
			Warnings,

			/// <summary>
			/// Indicates there are errors.
			/// </summary>
			Errors
		}

		private ReadmeGeneratorForm m_rgdGenerator = new ReadmeGeneratorForm();
		private bool m_booInfoEntered = false;
		private string m_strNewFomodPath = null;

		#region Properties

		/// <summary>
		/// Gets the path of the fomod that was built.
		/// </summary>
		/// <remarks>
		/// This value will be <lang cref="null"/> if the fomod was not successfully built.
		/// </remarks>
		/// <value>The path of the fomod that was built.</value>
		public string FomodPath
		{
			get
			{
				return m_strNewFomodPath;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public FomodBuilderForm()
		{
			InitializeComponent();

			Icon = Fomm.Properties.Resources.fomm02;
			Settings.GetWindowPosition("FomodBuilderForm", this);

			tbxPFPPath.DataBindings.Add("Enabled", cbxPFP, "Checked");
			butSelectPFPFolder.DataBindings.Add("Enabled", cbxPFP, "Checked");
			fseScriptEditor.DataBindings.Add("Enabled", cbxUseScript, "Checked");
			tbxPFPPath.Text = Settings.GetString("pfpOutputPath");
		}

		/// <summary>
		/// The PFP edit constructor.
		/// </summary>
		/// <param name="p_pfpPack">The PFP to edit.</param>
		/// <param name="p_strSourcesPath">The path to the directory contains the required source files.</param>
		public FomodBuilderForm(PremadeFomodPack p_pfpPack, string p_strSourcesPath)
			: this()
		{
			List<KeyValuePair<string, string>> lstCopyInstructions = p_pfpPack.GetCopyInstructions(p_strSourcesPath);
			string strPremadeSource = Archive.GenerateArchivePath(p_pfpPack.PFPPath, p_pfpPack.PremadePath);
			lstCopyInstructions.Add(new KeyValuePair<string, string>(strPremadeSource, "/"));

			List<KeyValuePair<string, string>> lstSourceLocations = p_pfpPack.GetSources();
			lstSourceLocations.Add(new KeyValuePair<string, string>(p_pfpPack.PFPPath, null));

			List<string> lstSources = new List<string>();
			List<SourceDownloadSelector.SourceDownloadLocation> lstLocations = new List<SourceDownloadSelector.SourceDownloadLocation>();
			foreach (KeyValuePair<string, string> kvpSource in lstSourceLocations)
			{
				lstLocations.Add(new SourceDownloadSelector.SourceDownloadLocation(Path.Combine(p_strSourcesPath, kvpSource.Key), kvpSource.Value, String.IsNullOrEmpty(kvpSource.Value)));
				lstSources.Add(kvpSource.Key);
			}
			ffsFileStructure.SetCopyInstructions(lstSources, lstCopyInstructions);
			tbxFomodFileName.Text = p_pfpPack.FomodName;
			sdsDownloadLocations.DataSource = lstLocations;
			cbxPFP.Checked = true;
			tbxPFPPath.Text = Path.GetDirectoryName(p_pfpPack.PFPPath);
		}

		#endregion

		/// <summary>
		/// Raises the <see cref="Form.Closing"/> event.
		/// </summary>
		/// <remarks>
		/// Saves the window's position.
		/// </remarks>
		/// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
		protected override void OnClosing(CancelEventArgs e)
		{
			Settings.SetWindowPosition("FomodBuilderForm", this);
			base.OnClosing(e);
		}

		#region Navigation

		/// <summary>
		/// Handles the <see cref="VerticalTabControl.SelectedTabPageChanged"/> event of the main
		/// navigation tab control.
		/// </summary>
		/// <remarks>
		/// This handles initialization of tabs as the selected tab changes.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="VerticalTabControl.TabPageEventArgs"/> describing the event arguments.</param>
		private void vtcFomodData_SelectedTabPageChanged(object sender, VerticalTabControl.TabPageEventArgs e)
		{
			if (e.TabPage == vtpDownloadLocations)
				UpdateDownloadLocationsList();
			else if (e.TabPage == vtpReadme)
				SetReadmeDefault();
			else if (e.TabPage == vtpScript)
				SetScriptDefault();
			else if (e.TabPage == vtpInfo)
			{
				m_booInfoEntered = true;
				SetInfoDefault();
			}
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the OK button.
		/// </summary>
		/// <remarks>
		/// This ensures that the information is valid before creating the FOMod/Premade FOMod Pack.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="EventArgs"/> describing the event arguments.</param>
		private void butOK_Click(object sender, EventArgs e)
		{
			switch (PerformValidation())
			{
				case ValidationState.Errors:
					MessageBox.Show(this, "You must correct the errors before saving.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				case ValidationState.Warnings:
					if (MessageBox.Show(this, "There are warnings." + Environment.NewLine + "Warnings can be ignored, but they can indicate missing information that you meant to enter." + Environment.NewLine + "Would you like to continue?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
						return;
					break;
				case ValidationState.Passed:
					break;
				default:
					throw new InvalidEnumArgumentException("Unexpected value for ValidationState enum.");
			}

			Readme rmeReadme = redReadmeEditor.Readme;
			FomodScript fscScript = fseScriptEditor.Script;
			XmlDocument xmlInfo = m_booInfoEntered ? fomod.SaveInfo(finInfo) : null;

			if (cbxFomod.Checked)
			{
				NewFomodBuilder fgnGenerator = new NewFomodBuilder();
				m_strNewFomodPath = fgnGenerator.BuildFomod(tbxFomodFileName.Text, ffsFileStructure.GetCopyInstructions(), rmeReadme, xmlInfo, m_booInfoEntered, finInfo.Screenshot, fscScript);
				if (String.IsNullOrEmpty(m_strNewFomodPath))
					return;
			}
			if (cbxPFP.Checked)
			{
				Settings.SetString("pfpOutputPath", tbxPFPPath.Text);

				string strVersion = "1.0";
				XmlDocument xmlInfoTmp = fomod.SaveInfo(finInfo);
				if (xmlInfoTmp != null)
				{
					XmlNode xndVersion = xmlInfoTmp.SelectSingleNode("/fomod/Version");
					if (xndVersion != null)
						strVersion = xndVersion.InnerText;
				}
				Dictionary<string, string> dicDownloadLocations = new Dictionary<string, string>();
				foreach (SourceDownloadSelector.SourceDownloadLocation sdlLocation in sdsDownloadLocations.DataSource)
					dicDownloadLocations[sdlLocation.Source] = sdlLocation.Included ? null : sdlLocation.URL;
				PremadeFomodPackBuilder fpbPackBuilder = new PremadeFomodPackBuilder();
				string strPFPPAth = fpbPackBuilder.BuildPFP(tbxFomodFileName.Text, strVersion, ffsFileStructure.GetCopyInstructions(), dicDownloadLocations, rmeReadme, xmlInfo, m_booInfoEntered, finInfo.Screenshot, fscScript, tbxPFPPath.Text);
				if (String.IsNullOrEmpty(strPFPPAth))
					return;
			}
			DialogResult = DialogResult.OK;
		}

		#endregion

		#region Validation

		/// <summary>
		/// Validates the data on this form.
		/// </summary>
		/// <remarks>
		/// This method validates the form data, and displays any errors or warnings.
		/// </remarks>
		/// <returns>The currnt validation state of the form's data.</returns>
		protected ValidationState PerformValidation()
		{
			bool booHasErrors = false;
			bool booHasWarnings = false;
			sspError.Clear();
			sspWarning.Clear();

			//Source Tab Validation
			if (!ValidateSources())
			{
				sspError.SetStatus(vtpSources, "Missing required information.");
				booHasErrors = true;
			}

			//download locations tab validation
			UpdateDownloadLocationsList();
			IList<SourceDownloadSelector.SourceDownloadLocation> lstLocations = (IList<SourceDownloadSelector.SourceDownloadLocation>)sdsDownloadLocations.DataSource;
			foreach (SourceDownloadSelector.SourceDownloadLocation sdlLocation in lstLocations)
				if (String.IsNullOrEmpty(sdlLocation.URL) && !sdlLocation.Included)
				{
					if (cbxPFP.Checked)
					{
						sspError.SetStatus(vtpDownloadLocations, "Download locations not specified for all sources.");
						booHasErrors = true;
					}
					else
					{
						sspWarning.SetStatus(vtpDownloadLocations, "Download locations not specified for all sources.");
						booHasWarnings = true;
					}
					break;
				}

			//readme tab validation
			SetReadmeDefault();
			if (redReadmeEditor.Readme == null)
			{
				sspWarning.SetStatus(vtpReadme, "No Readme file present.");
				booHasWarnings = true;
			}

			//fomod info Validation
			SetInfoDefault();
			if (!finInfo.PerformValidation())
			{
				sspError.SetStatus(vtpInfo, "Invalid information.");
				booHasErrors = true;
			}
			else if (String.IsNullOrEmpty(finInfo.Name) ||
				String.IsNullOrEmpty(finInfo.Author) ||
				String.IsNullOrEmpty(finInfo.HumanReadableVersion) ||
				String.IsNullOrEmpty(finInfo.Website) ||
				String.IsNullOrEmpty(finInfo.Description))
			{
				sspWarning.SetStatus(vtpInfo, "Missing information.");
				booHasWarnings = true;
			}

			//script validation
			SetScriptDefault();
			if (cbxUseScript.Checked)
			{
				if (fseScriptEditor.Script == null)
				{
					sspWarning.SetStatus(vtpScript, "Missing script.");
					booHasWarnings = true;
				}
				else if (!fseScriptEditor.IsValid)
				{
					sspError.SetStatus(vtpScript, "Invalid script.");
					booHasErrors = true;
				}
			}

			//save location validation
			if (!cbxFomod.Checked && !cbxPFP.Checked)
			{
				sspError.SetError(vtpOutput, "No items selected for creation.");
				booHasErrors = true;
			}
			else if (!ValidatePFPSavePath())
			{
				sspError.SetError(vtpOutput, "Premade FOMod Pack save location is required.");
				booHasErrors = true;
			}

			if (booHasErrors)
				return ValidationState.Errors;
			if (booHasWarnings)
				return ValidationState.Warnings;
			return ValidationState.Passed;
		}

		#region Sources Tab

		/// <summary>
		/// Validates the source files of the FOMod.
		/// </summary>
		/// <returns><lang cref="true"/> if the user has entered a file name for the FOMod, and selected
		/// files to include; <lang cref="false"/> otherwise.</returns>
		protected bool ValidateSources()
		{
			bool booPassed = ValidateFomodFileName();
			booPassed &= ValidateFomodFiles();
			return booPassed;
		}

		/// <summary>
		/// Ensures that the user has entered a file name.
		/// </summary>
		/// <returns><lang cref="true"/> if the user has entered a file name; <lang cref="false"/> otherwise.</returns>
		protected bool ValidateFomodFileName()
		{
			sspError.SetError(tbxFomodFileName, null);
			if (String.IsNullOrEmpty(tbxFomodFileName.Text))
			{
				sspError.SetError(tbxFomodFileName, "FOMod File Name is required.");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Ensures that the user has selected files to include in the FOMod.
		/// </summary>
		/// <returns><lang cref="true"/> if the user has selected files to include in the FOMod;
		/// <lang cref="false"/> otherwise.</returns>
		protected bool ValidateFomodFiles()
		{
			sspError.SetStatus(ffsFileStructure, null);
			if (ffsFileStructure.GetCopyInstructions().Count == 0)
			{
				sspError.SetStatus(ffsFileStructure, "You must select file to include in the FOMod.");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Handles the <see cref="Control.Validating"/> event of the file name textbox.
		/// </summary>
		/// <remarks>
		/// This validates the file name.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
		private void tbxFomodFileName_Validating(object sender, CancelEventArgs e)
		{
			ValidateFomodFileName();
		}

		#endregion

		#region Save Locations Tab

		/// <summary>
		/// Ensures that the user has entered a Premade FOMod Pack save path, if a PFP is being created.
		/// </summary>
		/// <returns><lang cref="true"/> if the user has entered a path and a Premade FOMod Pack is being created;
		/// <lang cref="false"/> otherwise.</returns>
		protected bool ValidatePFPSavePath()
		{
			sspError.SetError(cbxPFP, null);
			if (String.IsNullOrEmpty(tbxPFPPath.Text) && cbxPFP.Checked)
			{
				sspError.SetError(cbxPFP, "Premade FOMod Pack save location is required.");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Handles the <see cref="Control.Validating"/> event of the Premade FOMod Pack save path textbox.
		/// </summary>
		/// <remarks>
		/// This validates the Premade FOMod Pack save path.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
		private void tbxPFPPath_Validating(object sender, CancelEventArgs e)
		{
			ValidatePFPSavePath();
		}

		#endregion

		#endregion

		#region Download Locations

		/// <summary>
		/// Populates the source download location list with the selected sources.
		/// </summary>
		protected void UpdateDownloadLocationsList()
		{
			IList<SourceDownloadSelector.SourceDownloadLocation> lstOldLocations = (IList<SourceDownloadSelector.SourceDownloadLocation>)sdsDownloadLocations.DataSource;
			List<SourceDownloadSelector.SourceDownloadLocation> lstLocations = new List<SourceDownloadSelector.SourceDownloadLocation>();
			string[] strSources = ffsFileStructure.Sources;
			bool booFound = false;
			foreach (string strSource in strSources)
			{
				booFound = false;
				foreach (SourceDownloadSelector.SourceDownloadLocation sdlOldLocation in lstOldLocations)
					if (sdlOldLocation.Source.Equals(strSource))
					{
						lstLocations.Add(sdlOldLocation);
						booFound = true;
						break;
					}
				if (!booFound)
					lstLocations.Add(new SourceDownloadSelector.SourceDownloadLocation(strSource, null, false));
			}
			sdsDownloadLocations.DataSource = lstLocations;
		}

		#endregion

		#region Readme

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the generate readme button.
		/// </summary>
		/// <remarks>
		/// This display the <see cref="ReadmeGeneratorForm"/>, then selects the approriate readme
		/// editor and sets its text, based on the form's output.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butGenerateReadme_Click(object sender, EventArgs e)
		{
			m_rgdGenerator.Sources = ffsFileStructure.Sources;
			if (m_rgdGenerator.ShowDialog(this) == DialogResult.OK)
				redReadmeEditor.Readme = new Readme(m_rgdGenerator.Format, m_rgdGenerator.GeneratedReadme);
		}

		/// <summary>
		/// If no readme has been entered, this method looks for a readme file in the selected
		/// files, and, if one is found, uses it to populate the readme editor.
		/// </summary>
		protected void SetReadmeDefault()
		{
			if (redReadmeEditor.Readme == null)
			{
				ReadmeFormat fmtReadmeFormat = ReadmeFormat.PlainText;
				string strReadme = null;
				string strReadmeName = "readme - " + tbxFomodFileName.Text.ToLowerInvariant();
				Regex rgxReadme = new Regex(strReadmeName + @"\.\w\w\w\w?$", RegexOptions.IgnoreCase);
				IList<KeyValuePair<string, string>> lstFiles = ffsFileStructure.GetCopyInstructions();
				foreach (KeyValuePair<string, string> kvpFile in lstFiles)
				{
					if (rgxReadme.IsMatch(kvpFile.Value))
					{
						string strExtension = Path.GetExtension(kvpFile.Value).ToLowerInvariant();
						if (strExtension.Equals(".txt"))
							fmtReadmeFormat = ReadmeFormat.PlainText;
						else if (strExtension.Equals(".rtf"))
							fmtReadmeFormat = ReadmeFormat.RichText;
						else if (strExtension.Equals(".html") || strExtension.Equals(".htm"))
							fmtReadmeFormat = ReadmeFormat.HTML;
						else
							continue;
						if (kvpFile.Key.StartsWith(Archive.ARCHIVE_PREFIX))
						{
							KeyValuePair<string, string> kvpArchiveInfo = Archive.ParseArchivePath(kvpFile.Key);
							Archive arcArchive = new Archive(kvpArchiveInfo.Key);
							strReadme = TextUtil.ByteToString(arcArchive.GetFileContents(kvpArchiveInfo.Value));
						}
						else if (File.Exists(kvpFile.Key))
						{
							strReadme = File.ReadAllText(kvpFile.Key);
							break;
						}
					}
				}
				redReadmeEditor.Readme = new Readme(fmtReadmeFormat, strReadme);
			}
		}

		#endregion

		#region Script

		/// <summary>
		/// Handles the <see cref="CheckBox.CheckChanged"/> event of the use script check box.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void cbxUseScript_CheckedChanged(object sender, EventArgs e)
		{
			if (cbxUseScript.Checked)
				SetScriptDefault();
		}

		/// <summary>
		/// If no script has been entered, this method looks for a script file in the selected
		/// files, and, if one is found, uses it to populate the script editor. If one is not found,
		/// the script is populated with the default value.
		/// </summary>
		protected void SetScriptDefault()
		{
			if (fseScriptEditor.Script == null)
			{
				FomodScript fscInstallScript = null;
				string strScriptPath = null;
				foreach (string strScriptName in FomodScript.ScriptNames)
				{
					strScriptPath = Path.Combine("fomod", strScriptName);
					IList<KeyValuePair<string, string>> lstFiles = ffsFileStructure.FindFomodFiles(strScriptPath);
					if (lstFiles.Count > 0)
					{
						fscInstallScript = new FomodScript(strScriptName, null);
						strScriptPath = lstFiles[0].Value;
						break;
					}
				}

				if (fscInstallScript == null)
				{
					if (cbxUseScript.Checked)
						fscInstallScript = new FomodScript(FomodScriptType.CSharp, FomodScriptEditor.DEFAULT_CSHARP_SCRIPT);
				}
				else
				{
					cbxUseScript.Checked = true;
					if (strScriptPath.StartsWith(Archive.ARCHIVE_PREFIX))
					{
						KeyValuePair<string, string> kvpArchiveInfo = Archive.ParseArchivePath(strScriptPath);
						Archive arcArchive = new Archive(kvpArchiveInfo.Key);
						fscInstallScript.Text = TextUtil.ByteToString(arcArchive.GetFileContents(kvpArchiveInfo.Value));
					}
					else if (File.Exists(strScriptPath))
						fscInstallScript.Text = File.ReadAllText(strScriptPath);
				}

				fseScriptEditor.Script = fscInstallScript;
			}
		}

		#endregion

		#region Info

		/// <summary>
		/// If no info has been entered, this method looks for an info file in the selected
		/// files, and, if one is found, uses it to populate the info editor. If one is not found,
		/// the editor is populated with default values.
		/// </summary>
		protected void SetInfoDefault()
		{
			string strInfoFileName = "fomod" + Path.DirectorySeparatorChar + "info.xml";
			IList<KeyValuePair<string, string>> lstFiles = ffsFileStructure.FindFomodFiles(strInfoFileName);
			if (lstFiles.Count > 0)
			{
				XmlDocument xmlInfo = new XmlDocument();
				KeyValuePair<string, string> kvpScript = lstFiles[0];
				if (kvpScript.Value.StartsWith(Archive.ARCHIVE_PREFIX))
				{
					KeyValuePair<string, string> kvpArchiveInfo = Archive.ParseArchivePath(kvpScript.Value);
					Archive arcArchive = new Archive(kvpArchiveInfo.Key);
					string strInfo = TextUtil.ByteToString(arcArchive.GetFileContents(kvpArchiveInfo.Value));
					xmlInfo.LoadXml(strInfo);
				}
				else if (File.Exists(kvpScript.Value))
					xmlInfo.Load(kvpScript.Value);

				fomod.LoadInfo(xmlInfo, finInfo, false);
			}
			else if (String.IsNullOrEmpty(finInfo.ModName))
				finInfo.ModName = tbxFomodFileName.Text;

			string strScreenshotFileName = "fomod" + Path.DirectorySeparatorChar + "screenshot.*";
			lstFiles = ffsFileStructure.FindFomodFiles(strScreenshotFileName);
			if (lstFiles.Count > 0)
			{
				KeyValuePair<string, string> kvpScreenshot = lstFiles[0];
				if (kvpScreenshot.Value.StartsWith(Archive.ARCHIVE_PREFIX))
				{
					KeyValuePair<string, string> kvpArchiveInfo = Archive.ParseArchivePath(kvpScreenshot.Value);
					Archive arcArchive = new Archive(kvpArchiveInfo.Key);
					byte[] bteScreenshot = arcArchive.GetFileContents(kvpArchiveInfo.Value);
					finInfo.Screenshot = new Screenshot(kvpArchiveInfo.Value, bteScreenshot);
				}
				else if (File.Exists(kvpScreenshot.Value))
					finInfo.Screenshot = new Screenshot(kvpScreenshot.Value, File.ReadAllBytes(kvpScreenshot.Value));
			}
		}

		#endregion

		/// <summary>
		/// Handles the <see cref="XmlEditor.GotAutoCompleteList"/> event of the XML configration script
		/// editor.
		/// </summary>
		/// <remarks>
		/// This methods populates the code completion list with the file paths in the FOMod file structure
		/// when the value being completed is the source value of a file tag.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="RegeneratableAutoCompleteListEventArgs"/> decribing the event arguments.</param>
		private void xedScript_GotAutoCompleteList(object sender, RegeneratableAutoCompleteListEventArgs e)
		{
			if (!String.IsNullOrEmpty(e.ElementPath) && Path.GetFileName(e.ElementPath).Equals("file") && (e.AutoCompleteType == AutoCompleteType.AttributeValues) && (e.Siblings[e.Siblings.Length - 1].Equals("source")))
			{
				string strPrefix = e.LastWord.EndsWith("=") ? "" : e.LastWord;
				List<KeyValuePair<string, string>> lstFiles = ffsFileStructure.FindFomodFiles(strPrefix + "*");
				foreach (KeyValuePair<string, string> kvpFile in lstFiles)
					e.AutoCompleteList.Add(new XmlCompletionData(AutoCompleteType.AttributeValues, kvpFile.Key, null));
				e.GenerateOnNextKey = true;
				e.ExtraInsertionCharacters.Add(Path.DirectorySeparatorChar);
			}
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the select PFP folder button.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butSelectPFPFolder_Click(object sender, EventArgs e)
		{
			if (fbdPFPPath.ShowDialog(this) == DialogResult.OK)
				tbxPFPPath.Text = fbdPFPPath.SelectedPath;
		}
	}
}
