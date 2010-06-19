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

namespace Fomm.PackageManager.FomodBuilder
{
	/// <summary>
	/// This form builds a FOMOD form existing files.
	/// </summary>
	public partial class FomodBuilderForm : Form
	{
		protected const string DEFAULT_CSHARP_SCRIPT = @"using System;
using fomm.Scripting;

class Script : BaseScript {
	public static bool OnActivate() {
        //Install all files from the fomod and activate any esps
        PerformBasicInstall();
		return true;
	}
}
";
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

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public FomodBuilderForm()
		{
			InitializeComponent();

			Icon = Fomm.Properties.Resources.fomm02;
			Settings.GetWindowPosition("FomodBuilderForm", this);

			xedReadme.SetHighlighting("HTML");
			cbxVersion.Items.Add("1.0");
			cbxVersion.Items.Add("2.0");
			cbxVersion.Items.Add("3.0");
			cbxVersion.SelectedIndex = 2;
			LoadConfigSchema();

			tbxPFPPath.DataBindings.Add("Enabled", cbxPFP, "Checked");
			butSelectPFPFolder.DataBindings.Add("Enabled", cbxPFP, "Checked");
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
				SetInfoDefault();
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the OK button.
		/// </summary>
		/// <remarks>
		/// This ensures that the information is valid before creating the FOMOD/Premade FOMOD Pack.
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
			if (((ddtReadme.SelectedTabPage == ddpPlainText) && String.IsNullOrEmpty(tbxReadme.Text)) ||
				((ddtReadme.SelectedTabPage == ddpRichText) && String.IsNullOrEmpty(rteReadme.Text)) ||
				((ddtReadme.SelectedTabPage == ddpHTML) && String.IsNullOrEmpty(xedReadme.Text)))
			{
				sspWarning.SetStatus(vtpReadme, "No Readme file present.");
				booHasWarnings = true;
			}

			//fomod info Validation
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
			if (cbxUseScript.Checked)
			{
				if (((ddtScript.SelectedTabPage == dtpCSharp) && String.IsNullOrEmpty(sedScript.Text)) ||
					((ddtScript.SelectedTabPage == dtpXML) && String.IsNullOrEmpty(xedScript.Text)))
				{
					sspWarning.SetStatus(vtpScript, "Missing script.");
					booHasWarnings = true;
				}
				else if (((ddtScript.SelectedTabPage == dtpCSharp) && !sedScript.ValidateSyntax()) ||
					((ddtScript.SelectedTabPage == dtpXML) && !xedScript.ValidateXml()))
				{
					sspError.SetStatus(vtpScript, "Invalid script.");
					booHasErrors = true;
				}
			}

			//save location validation
			if (!cbxFomod.Checked && !cbxPFP.Checked)
				sspError.SetError(vtpOutput, "No items selected for creation.");
			else if (!ValidatePFPSavePath())
				sspError.SetError(vtpOutput, "Premade FOMOD Pack save location is required.");

			if (booHasErrors)
				return ValidationState.Errors;
			if (booHasWarnings)
				return ValidationState.Warnings;
			return ValidationState.Passed;
		}

		#region Sources Tab

		/// <summary>
		/// Validates the source files of the FOMOD.
		/// </summary>
		/// <returns><lang cref="true"/> if the user has entered a file name for the FOMOD, and selected
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
				sspError.SetError(tbxFomodFileName, "FOMOD File Name is required.");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Ensures that the user has selected files to include in the FOMOD.
		/// </summary>
		/// <returns><lang cref="true"/> if the user has selected files to include in the FOMOD;
		/// <lang cref="false"/> otherwise.</returns>
		protected bool ValidateFomodFiles()
		{
			sspError.SetStatus(ffsFileStructure, null);
			if (ffsFileStructure.GetCopyPaths().Count == 0)
			{
				sspError.SetStatus(ffsFileStructure, "You must select file to include in the FOMOD.");
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
		/// Ensures that the user has entered a Premade FOMOD Pack save path, if a PFP is being created.
		/// </summary>
		/// <returns><lang cref="true"/> if the user has entered a path and a Premade FOMOD Pack is being created;
		/// <lang cref="false"/> otherwise.</returns>
		protected bool ValidatePFPSavePath()
		{
			sspError.SetError(cbxPFP, null);
			if (String.IsNullOrEmpty(tbxPFPPath.Text) && cbxPFP.Checked)
			{
				sspError.SetError(cbxPFP, "Premade FOMOD Pack save location is required.");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Handles the <see cref="Control.Validating"/> event of the Premade FOMOD Pack save path textbox.
		/// </summary>
		/// <remarks>
		/// This validates the Premade FOMOD Pack save path.
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
			{
				switch (m_rgdGenerator.Format)
				{
					case ReadmeFileSelector.ReadmeFormat.PlainText:
						ddtReadme.SelectedTabPage = ddpPlainText;
						tbxReadme.Text = m_rgdGenerator.GeneratedReadme;
						break;
					case ReadmeFileSelector.ReadmeFormat.RichText:
						ddtReadme.SelectedTabPage = ddpRichText;
						try
						{
							rteReadme.Rtf = m_rgdGenerator.GeneratedReadme;
						}
						catch
						{
							rteReadme.Text = m_rgdGenerator.GeneratedReadme;
						}
						break;
					case ReadmeFileSelector.ReadmeFormat.HTML:
						ddtReadme.SelectedTabPage = ddpHTML;
						xedReadme.Text = m_rgdGenerator.GeneratedReadme;
						break;
					default:
						throw new InvalidEnumArgumentException("Unrecognized value for ReadmeFileSelector.ReadmeFormat enum.");
				}
			}
		}

		/// <summary>
		/// If no readme has been entered, this method looks for a readme file in the selected
		/// files, and, if one is found, uses it to populate the readme editor.
		/// </summary>
		protected void SetReadmeDefault()
		{
			if (((ddtReadme.SelectedTabPage == ddpPlainText) && String.IsNullOrEmpty(tbxReadme.Text)) ||
				((ddtReadme.SelectedTabPage == ddpRichText) && String.IsNullOrEmpty(rteReadme.Text)) ||
				((ddtReadme.SelectedTabPage == ddpHTML) && String.IsNullOrEmpty(xedReadme.Text)))
			{
				ReadmeFileSelector.ReadmeFormat fmtReadmeFormat = ReadmeFileSelector.ReadmeFormat.PlainText;
				string strReadme = null;
				string strReadmeName = "readme - " + tbxFomodFileName.Text.ToLowerInvariant();
				Regex rgxReadme = new Regex(strReadmeName + @"\.\w\w\w\w?$", RegexOptions.IgnoreCase);
				IList<KeyValuePair<string, string>> lstFiles = ffsFileStructure.GetCopyPaths();
				foreach (KeyValuePair<string, string> kvpFile in lstFiles)
				{
					if (rgxReadme.IsMatch(kvpFile.Value))
					{
						string strExtension = Path.GetExtension(kvpFile.Value).ToLowerInvariant();
						if (strExtension.Equals(".txt"))
							fmtReadmeFormat = ReadmeFileSelector.ReadmeFormat.PlainText;
						else if (strExtension.Equals(".rtf"))
							fmtReadmeFormat = ReadmeFileSelector.ReadmeFormat.RichText;
						else if (strExtension.Equals(".html") || strExtension.Equals(".htm"))
							fmtReadmeFormat = ReadmeFileSelector.ReadmeFormat.HTML;
						else
							continue;
						if (kvpFile.Key.StartsWith(Archive.ARCHIVE_PREFIX))
						{
							KeyValuePair<string, string> kvpArchiveInfo = Archive.ParseArchive(kvpFile.Key);
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
				switch (fmtReadmeFormat)
				{
					case ReadmeFileSelector.ReadmeFormat.PlainText:
						ddtReadme.SelectedTabPage = ddpPlainText;
						tbxReadme.Text = strReadme;
						break;
					case ReadmeFileSelector.ReadmeFormat.RichText:
						ddtReadme.SelectedTabPage = ddpRichText;
						try
						{
							rteReadme.Rtf = strReadme;
						}
						catch (Exception)
						{
							rteReadme.Text = strReadme;
						}
						break;
					case ReadmeFileSelector.ReadmeFormat.HTML:
						ddtReadme.SelectedTabPage = ddpHTML;
						xedReadme.Text = strReadme;
						break;
					default:
						throw new InvalidEnumArgumentException("Unrecognized value for ReadmeFileSelector.ReadmeFormat.");
				}
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
			ddtScript.Enabled = cbxUseScript.Checked;
		}

		/// <summary>
		/// If no script has been entered, this method looks for a script file in the selected
		/// files, and, if one is found, uses it to populate the script editor. If one is not found,
		/// the script is populated with the default value.
		/// </summary>
		protected void SetScriptDefault()
		{
			if (cbxUseScript.Checked &&
				(((ddtScript.SelectedTabPage == dtpCSharp) && String.IsNullOrEmpty(sedScript.Text)) ||
				((ddtScript.SelectedTabPage == dtpXML) && String.IsNullOrEmpty(xedScript.Text))))
			{
				string strScriptName = "fomod" + Path.DirectorySeparatorChar + "script.cs";
				IList<KeyValuePair<string, string>> lstFiles = ffsFileStructure.FindFomodFiles(strScriptName);
				if (lstFiles.Count == 0)
				{
					strScriptName = "fomod" + Path.DirectorySeparatorChar + "ModuleConfig.xml";
					lstFiles = ffsFileStructure.FindFomodFiles(strScriptName);
				}
				if (lstFiles.Count == 0)
				{
					ddtScript.SelectedTabPage = dtpCSharp;
					sedScript.Text = DEFAULT_CSHARP_SCRIPT;
					return;
				}

				string strScript = null;
				KeyValuePair<string, string> kvpScript = lstFiles[0];
				if (kvpScript.Value.StartsWith(Archive.ARCHIVE_PREFIX))
				{
					KeyValuePair<string, string> kvpArchiveInfo = Archive.ParseArchive(kvpScript.Value);
					Archive arcArchive = new Archive(kvpArchiveInfo.Key);
					strScript = TextUtil.ByteToString(arcArchive.GetFileContents(kvpArchiveInfo.Value));
				}
				else if (File.Exists(kvpScript.Value))
					strScript = File.ReadAllText(kvpScript.Value);

				if (strScriptName.EndsWith("ModuleConfig.xml"))
				{
					switch (Parser.GetConfigVersion(strScript))
					{
						case "1.0":
							cbxVersion.SelectedIndex = 0;
							break;
						case "2.0":
							cbxVersion.SelectedIndex = 1;
							break;
						case "3.0":
							cbxVersion.SelectedIndex = 2;
							break;
						default:
							ddtScript.SelectedTabPage = dtpCSharp;
							sedScript.Text = DEFAULT_CSHARP_SCRIPT;
							return;
					}
					Regex rgxXMLConfigCleanup = new Regex(@"<\?xml[^>]+\?>.*?<config[^>]*>", RegexOptions.Singleline);
					strScript = rgxXMLConfigCleanup.Replace(strScript, "<config>");
					ddtScript.SelectedTabPage = dtpXML;
					xedScript.Text = strScript;
				}
				else
				{
					ddtScript.SelectedTabPage = dtpCSharp;
					sedScript.Text = strScript;
				}
			}
		}

		/// <summary>
		/// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of the XML config version drop down list.
		/// </summary>
		/// <param name="sender">The object the raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void cbxVersion_SelectedIndexChanged(object sender, EventArgs e)
		{
			LoadConfigSchema();
		}

		/// <summary>
		/// This loads the selected version of the XML configuration script's schema into the XML editor.
		/// </summary>
		/// <remarks>
		/// Loading the schema into the editor allows for validation and auto-completion.
		/// </remarks>
		protected void LoadConfigSchema()
		{
			string strSchemaPath = Path.Combine(Program.exeDir, String.Format(@"fomm\ModConfig{0}.xsd", cbxVersion.SelectedItem.ToString()));
			using (FileStream fsmSchema = new FileStream(strSchemaPath, FileMode.Open))
			{
				xedScript.Schema = XmlSchema.Read(fsmSchema, null);
				fsmSchema.Close();
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
					KeyValuePair<string, string> kvpArchiveInfo = Archive.ParseArchive(kvpScript.Value);
					Archive arcArchive = new Archive(kvpArchiveInfo.Key);
					string strInfo = TextUtil.ByteToString(arcArchive.GetFileContents(kvpArchiveInfo.Value));
					xmlInfo.LoadXml(strInfo);
				}
				else if (File.Exists(kvpScript.Value))
					xmlInfo.Load(File.ReadAllText(kvpScript.Value));

				fomod.LoadInfo(xmlInfo, finInfo, false);
			}

			string strScreenshotFileName = "fomod" + Path.DirectorySeparatorChar + "screenshot.*";
			lstFiles = ffsFileStructure.FindFomodFiles(strScreenshotFileName);
			if (lstFiles.Count > 0)
			{
				KeyValuePair<string, string> kvpScreenshot = lstFiles[0];
				if (kvpScreenshot.Value.StartsWith(Archive.ARCHIVE_PREFIX))
				{
					KeyValuePair<string, string> kvpArchiveInfo = Archive.ParseArchive(kvpScreenshot.Value);
					Archive arcArchive = new Archive(kvpArchiveInfo.Key);
					finInfo.Screenshot = arcArchive.GetFileContents(kvpArchiveInfo.Value);
				}
				else if (File.Exists(kvpScreenshot.Value))
					finInfo.Screenshot = File.ReadAllBytes(kvpScreenshot.Value);
			}
		}

		#endregion

		/// <summary>
		/// Handles the <see cref="XmlEditor.GotAutoCompleteList"/> event of the XML configration script
		/// editor.
		/// </summary>
		/// <remarks>
		/// This methods populates the code completion list with the file paths in the FOMOD file structure
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
	}
}
