using System;
using System.Xml;
using System.Xml.Schema;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	/// <summary>
	/// The xml mod configuration script.
	/// </summary>
	/// <remarks>
	/// This runs a script that is customized by an xml file in the fomod.
	/// </remarks>
	public class XmlConfiguredScript
	{
		/// <summary>
		/// The arguments that are needed by the <see cref="InstallFiles"/> method
		/// that is used by the background worker.
		/// </summary>
		protected class InstallFilesArguments
		{
			#region Properties

			/// <summary>
			/// Gets or sets the xml configuration parser.
			/// </summary>
			/// <value>The xml configuration parser.</value>
			public Parser Parser { get; protected set; }

			/// <summary>
			/// Gets or sets the options form used to selected what needs to be installed.
			/// </summary>
			/// <value>The options form used to selected what needs to be installed.</value>
			public OptionsForm Form { get; protected set; }

			#endregion

			#region Constructors

			/// <summary>
			/// A simple constructor that initializes the object with the given values.
			/// </summary>
			/// <param name="p_xmlConfig">The xml configuration file.</param>
			/// <param name="p_ofmForm">The options form used to selected what needs to be installed.</param>
			public InstallFilesArguments(Parser p_psrParser, OptionsForm p_ofmForm)
			{
				Parser = p_psrParser;
				Form = p_ofmForm;
			}

			#endregion
		}

		private ModInstallScript m_misInstallScript = null;
		private BackgroundWorkerProgressDialog m_bwdProgress = null;
		private DependencyStateManager m_dsmStateManager = null;

		#region Constructors

		/// <summary>
		/// A simple constructor.
		/// </summary>
		/// <param name="misInstaller">The <see cref="ModInstallScript"/> that is installing the mod.</param>
		public XmlConfiguredScript(ModInstallScript misInstaller)
		{
			m_misInstallScript = misInstaller;
		}

		#endregion

		#region Install Methods

		/// <summary>
		/// Displays the option form and starts the background worker to do the install.
		/// </summary>
		/// <returns><lang cref="true"/> if the mod installed correctly; <lang cref="false"/> otherwise.</returns>
		public bool Install()
		{
			XmlDocument xmlConfig = new XmlDocument();
			string strConfig = m_misInstallScript.Fomod.GetInstallScript().Text;
			xmlConfig.LoadXml(strConfig);

			//remove comments so we don't have to deal with them later
			XmlNodeList xnlComments = xmlConfig.SelectNodes("//comment()");
			foreach (XmlNode xndComment in xnlComments)
				xndComment.ParentNode.RemoveChild(xndComment);

			m_dsmStateManager = new DependencyStateManager(m_misInstallScript);

			Parser prsParser = Parser.GetParser(xmlConfig, m_misInstallScript.Fomod, m_dsmStateManager);
			CompositeDependency cpdModDependencies = prsParser.GetModDependencies();
			if ((cpdModDependencies != null) && !cpdModDependencies.IsFufilled)
				throw new DependencyException(cpdModDependencies.Message);

			IList<InstallStep> lstSteps = prsParser.GetInstallSteps();
			HeaderInfo hifHeaderInfo = prsParser.GetHeaderInfo();
			OptionsForm ofmOptions = new OptionsForm(this, hifHeaderInfo, m_dsmStateManager, lstSteps);
			bool booPerformInstall = false;
			if (lstSteps.Count == 0)
				booPerformInstall = true;
			else
				booPerformInstall = (ofmOptions.ShowDialog() == DialogResult.OK);

			if (booPerformInstall)
			{
				using (m_bwdProgress = new BackgroundWorkerProgressDialog(InstallFiles))
				{
					m_bwdProgress.WorkMethodArguments = new InstallFilesArguments(prsParser, ofmOptions);
					m_bwdProgress.OverallMessage = "Installing " + hifHeaderInfo.Title;
					m_bwdProgress.OverallProgressStep = 1;
					m_bwdProgress.ItemProgressStep = 1;
					if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
						return false;
				}
				return true;
			}

			return false;
		}

		/// <summary>
		/// Installs and activates files are required. This method is used by the background worker.
		/// </summary>
		/// <param name="p_ifaArgs">The arguments used to configure what is installed.</param>
		protected void InstallFiles(object p_ifaArgs)
		{
			if (!(p_ifaArgs is InstallFilesArguments))
				throw new ArgumentException("The given argument obejct is not of type InstallFilesArguments.", "p_ifaArgs");

			Parser prsParser = ((InstallFilesArguments)p_ifaArgs).Parser;
			OptionsForm ofmOptions = ((InstallFilesArguments)p_ifaArgs).Form;

			IList<PluginFile> lstRequiredFiles = prsParser.GetRequiredInstallFiles();
			List<PluginFile> lstInstallFiles = ofmOptions.FilesToInstall;
			m_bwdProgress.OverallProgressMaximum = lstRequiredFiles.Count + lstInstallFiles.Count;

			foreach (PluginFile pflRequiredFile in lstRequiredFiles)
			{
				if (m_bwdProgress.Cancelled())
					return;
				if (!InstallPluginFile(pflRequiredFile, true))
					return;
				m_bwdProgress.StepOverallProgress();
			}

			List<PluginFile> lstActivateFiles = ofmOptions.PluginsToActivate;
			foreach (PluginFile plfFile in lstInstallFiles)
			{
				if (m_bwdProgress.Cancelled())
					return;
				if (!InstallPluginFile(plfFile, lstActivateFiles.Contains(plfFile)))
					return;
				m_bwdProgress.StepOverallProgress();
			}

			IList<ConditionalFileInstallPattern> lstConditionInstallPatterns = prsParser.GetConditionalFileInstallPatterns();
			foreach (ConditionalFileInstallPattern cipPattern in lstConditionInstallPatterns)
			{
				if (cipPattern.Dependency.IsFufilled)
					foreach (PluginFile plfFile in cipPattern.Files)
					{
						if (m_bwdProgress.Cancelled())
							return;
						if (!InstallPluginFile(plfFile, true))
							return;
						m_bwdProgress.StepOverallProgress();
					}
			}
		}

		/// <summary>
		/// Installs the given <see cref="OptionsForm.PluginFile"/>, and activates any
		/// esm/esp files it encompasses as requested.
		/// </summary>
		/// <param name="plfFile">The file to install.</param>
		/// <param name="booActivate">Whether or not to activate any esp/esm files.</param>
		/// <returns><lang cref="false"/> if the user cancelled the install;
		/// <lang cref="true"/> otherwise.</returns>
		protected bool InstallPluginFile(PluginFile plfFile, bool booActivate)
		{
			string strSource = plfFile.Source;
			string strDest = plfFile.Destination;
			m_bwdProgress.ItemMessage = "Installing " + (String.IsNullOrEmpty(strDest) ? strSource : strDest);
			if (plfFile.IsFolder)
			{
				CopyDataFolder(strSource, strDest);

				if (m_bwdProgress.Cancelled())
					return false;

				//if the destination length is greater than 0, then nothing in
				// this folder is directly in the Data folder as so cannot be
				// activated
				if (strDest.Length == 0)
				{
					List<string> lstFiles = GetFomodFolderFileList(strSource);
					m_bwdProgress.ItemMessage = "Activating " + (String.IsNullOrEmpty(strDest) ? strSource : strDest);
					m_bwdProgress.ItemProgress = 0;
					m_bwdProgress.ItemProgressMaximum = lstFiles.Count;

					if (!strSource.EndsWith("/"))
						strSource += "/";
					foreach (string strFile in lstFiles)
					{
						if (strFile.ToLowerInvariant().EndsWith(".esm") || strFile.ToLowerInvariant().EndsWith(".esp"))
						{
							string strNewFileName = strFile.Substring(strSource.Length, strFile.Length - strSource.Length);
							m_misInstallScript.SetPluginActivation(strNewFileName, booActivate);
						}
						if (m_bwdProgress.Cancelled())
							return false;
						m_bwdProgress.StepItemProgress();
					}
				}
			}
			else
			{
				m_bwdProgress.ItemProgress = 0;
				m_bwdProgress.ItemProgressMaximum = 2;

				m_misInstallScript.CopyDataFile(strSource, strDest);

				m_bwdProgress.StepItemProgress();

				if (String.IsNullOrEmpty(strDest))
				{
					if (strSource.ToLowerInvariant().EndsWith(".esm") || strSource.ToLowerInvariant().EndsWith(".esp"))
						m_misInstallScript.SetPluginActivation(strSource, booActivate);
				}
				else if (strDest.ToLowerInvariant().EndsWith(".esm") || strDest.ToLowerInvariant().EndsWith(".esp"))
					m_misInstallScript.SetPluginActivation(strDest, booActivate);

				m_bwdProgress.StepItemProgress();
			}
			return true;
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Recursively copies all files and folders from one location to another.
		/// </summary>
		/// <param name="p_strFrom">The source from whence to copy the files.</param>
		/// <param name="p_strTo">The destination for the copied files.</param>
		protected void CopyDataFolder(string p_strFrom, string p_strTo)
		{
			List<string> lstFOMODFiles = GetFomodFolderFileList(p_strFrom);
			m_bwdProgress.ItemProgress = 0;
			m_bwdProgress.ItemProgressMaximum = lstFOMODFiles.Count;

			String strFrom = p_strFrom.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();
			if (!strFrom.EndsWith(Path.DirectorySeparatorChar.ToString()))
				strFrom += Path.DirectorySeparatorChar;
			String strTo = p_strTo.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			if ((strTo.Length > 0) && (!strTo.EndsWith(Path.DirectorySeparatorChar.ToString())))
				strTo += Path.DirectorySeparatorChar;
			String strFOMODFile = null;
			for (Int32 i = 0; i < lstFOMODFiles.Count; i++)
			{
				if (m_bwdProgress.Cancelled())
					return;

				strFOMODFile = lstFOMODFiles[i];
				string strNewFileName = strFOMODFile.Substring(strFrom.Length, strFOMODFile.Length - strFrom.Length);
				m_misInstallScript.CopyDataFile(strFOMODFile, Path.Combine(strTo, strNewFileName));

				m_bwdProgress.StepItemProgress();
			}
		}

		/// <summary>
		/// Gets a list of all files in the specified FOMod folder.
		/// </summary>
		/// <param name="p_strPath">The FOMod folder whose file list is to be retrieved.</param>
		/// <returns>The list of all files in the specified FOMod folder.</returns>
		protected List<string> GetFomodFolderFileList(string p_strPath)
		{
			if (m_strFomodFiles == null)
			{
				m_strFomodFiles = m_misInstallScript.Fomod.GetFileList().ToArray();
				for (Int32 i = m_strFomodFiles.Length - 1; i >= 0; i--)
					m_strFomodFiles[i] = m_strFomodFiles[i].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}
			String strPath = p_strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();
			List<string> lstFiles = new List<string>();
			foreach (string strFile in m_strFomodFiles)
				if (strFile.ToLowerInvariant().StartsWith(strPath))
					lstFiles.Add(strFile);
			return lstFiles;
		}
		string[] m_strFomodFiles = null;

		#endregion
	}
}
