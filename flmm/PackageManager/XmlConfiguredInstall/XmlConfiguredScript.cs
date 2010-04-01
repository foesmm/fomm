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
			byte[] bteConfig = m_misInstallScript.Fomod.GetFile("fomod/ModuleConfig.xml");
			using (MemoryStream stmConfig = new MemoryStream(bteConfig))
			{
				using (StreamReader srdConfigReader = new StreamReader(stmConfig, true))
				{
					xmlConfig.LoadXml(srdConfigReader.ReadToEnd());
					srdConfigReader.Close();
				}
				stmConfig.Close();
			}

			Dictionary<string, bool> dicPlugins = new Dictionary<string, bool>();
			string[] strPlugins = m_misInstallScript.GetAllPlugins();
			foreach (string strPlugin in strPlugins)
				dicPlugins.Add(strPlugin.ToLowerInvariant(), IsPluginActive(strPlugin));

			m_dsmStateManager = new DependencyStateManager(dicPlugins);

			Parser prsParser = Parser.GetParser(xmlConfig, m_misInstallScript.Fomod, m_dsmStateManager);
			ModDependencies mdpModDependencies = prsParser.GetModDependencies();
			foreach (KeyValuePair<string, Version> kvpDependencies in mdpModDependencies.ProgrammeDependencies)
			{
				switch (kvpDependencies.Key)
				{
					case "foseDependency":
						RequireFoseVersion(kvpDependencies.Value);
						break;
					case "falloutDependency":
						RequireGameVersion(kvpDependencies.Value);
						break;
					case "fommDependency":
						RequireFommVersion(kvpDependencies.Value);
						break;
					default:
						throw new VersionException(kvpDependencies.Value, null, kvpDependencies.Key);
				}
			}
			foreach (string strFile in mdpModDependencies.FileDependencies)
			{
				if (!FileManagement.DataFileExists(strFile))
				{
					m_misInstallScript.MessageBox("The following required file is missing: " + strFile);
					return false;
				}
				if ((strFile.EndsWith(".esm") || strFile.EndsWith(".esp")) &&
					(!IsPluginActive(strFile)) &&
					(m_misInstallScript.MessageBox("The following required file is installed but not active: " + strFile + System.Environment.NewLine + "Would you like to active it?", "Required File", MessageBoxButtons.YesNo) == DialogResult.No))
					return false;
			}

			IList<PluginGroup> lstGroups = prsParser.GetGroupedPlugins();
			OptionsForm ofmOptions = new OptionsForm(this, prsParser.ModName, m_dsmStateManager, lstGroups);
			bool booPerformInstall = false;
			if (lstGroups.Count == 0)
				booPerformInstall = true;
			else
				booPerformInstall = (ofmOptions.ShowDialog() == DialogResult.OK);

			if (booPerformInstall)
			{
				using (m_bwdProgress = new BackgroundWorkerProgressDialog(InstallFiles))
				{
					m_bwdProgress.WorkMethodArguments = new InstallFilesArguments(prsParser, ofmOptions);
					m_bwdProgress.OverallMessage = "Installing " + prsParser.ModName;
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
				if (!CopyDataFolder(strSource, strDest))
					throw new ApplicationException("Could not install " + strSource + " to " + strDest);

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

				if (!m_misInstallScript.CopyDataFile(strSource, strDest))
					throw new ApplicationException("Could not install " + strSource + " to " + strDest);

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
		/// Ensures the the specified minimum version of FOSE is installed.
		/// </summary>
		/// <param name="p_verMinVersion">The mimum required version of FOSE.</param>
		/// <exception cref="VersionException">Thrown if the installed version of FOSE is less than the required version.</exception>
		protected void RequireFoseVersion(Version p_verMinVersion)
		{
			Version verFoseVersion = m_misInstallScript.GetFoseVersion();
			if (verFoseVersion == null)
				m_misInstallScript.MessageBox("This mod requires FOSE v " + p_verMinVersion + " or higher. Please download from http://silverlock.org");
			else if (verFoseVersion < p_verMinVersion)
				m_misInstallScript.MessageBox("This mod requires FOSE v " + p_verMinVersion + " or higher. You have " + verFoseVersion + ". Please update from http://silverlock.org");
			else
				return;
			throw new VersionException(p_verMinVersion, verFoseVersion, "FOSE");
		}

		/// <summary>
		/// Ensures the the specified minimum version of Fallout 3 is installed.
		/// </summary>
		/// <param name="p_verMinVersion">The mimum required version of Fallout 3.</param>
		/// <exception cref="VersionException">Thrown if the installed version of Fallout 3 is less than the required version.</exception>
		protected void RequireGameVersion(Version p_verMinVersion)
		{
			Version verGameVersion = m_misInstallScript.GetFalloutVersion();
			if (verGameVersion < p_verMinVersion)
			{
				m_misInstallScript.MessageBox("This mod requires Fallout 3 v" + p_verMinVersion + " or higher. You have " + verGameVersion + ". Please update your game.");
				throw new VersionException(p_verMinVersion, verGameVersion, "Fallout 3");
			}
			return;
		}

		/// <summary>
		/// Ensures the the specified minimum version of FOMM is installed.
		/// </summary>
		/// <param name="p_verMinVersion">The mimum required version of FOMM.</param>
		/// <exception cref="VersionException">Thrown if the installed version of FOMM is less than the required version.</exception>
		protected void RequireFommVersion(Version p_verMinVersion)
		{
			Version verFommVersion = m_misInstallScript.GetFommVersion();
			if (verFommVersion < p_verMinVersion)
			{
				m_misInstallScript.MessageBox("This mod requires FOMM v" + p_verMinVersion + " or higher. You have " + verFommVersion + ". Please update from https://sourceforge.net/projects/fomm");
				throw new VersionException(p_verMinVersion, verFommVersion, "FOMM");
			}
			return;
		}

		/// <summary>
		/// Recursively copies all files and folders from one location to another.
		/// </summary>
		/// <param name="p_strFrom">The source from whence to copy the files.</param>
		/// <param name="p_strTo">The destination for the copied files.</param>
		/// <returns>true if the copy succeeded; false otherwise.</returns>
		protected bool CopyDataFolder(string p_strFrom, string p_strTo)
		{
			List<string> lstFOMODFiles = GetFomodFolderFileList(p_strFrom);
			m_bwdProgress.ItemProgress = 0;
			m_bwdProgress.ItemProgressMaximum = lstFOMODFiles.Count;

			String strFrom = p_strFrom.Replace('\\', '/').ToLowerInvariant();
			if (!strFrom.EndsWith("/"))
				strFrom += "/";
			String strTo = p_strTo.Replace('\\', '/');
			if ((strTo.Length > 0) && (!strTo.EndsWith("/")))
				strTo += "/";
			String strFOMODFile = null;
			for (Int32 i = 0; i < lstFOMODFiles.Count; i++)
			{
				if (m_bwdProgress.Cancelled())
					return true;

				strFOMODFile = lstFOMODFiles[i];
				string strNewFileName = strFOMODFile.Substring(strFrom.Length, strFOMODFile.Length - strFrom.Length);
				if (!m_misInstallScript.CopyDataFile(strFOMODFile, Path.Combine(strTo, strNewFileName)))
					return false;

				m_bwdProgress.StepItemProgress();
			}
			return true;
		}

		/// <summary>
		/// Gets a list of all files in the specified FOMOD folder.
		/// </summary>
		/// <param name="p_strPath">The FOMOD folder whose file list is to be retrieved.</param>
		/// <returns>The list of all files in the specified FOMOD folder.</returns>
		protected List<string> GetFomodFolderFileList(string p_strPath)
		{
			if (m_strFomodFiles == null)
				m_strFomodFiles = m_misInstallScript.Fomod.GetFileList().ToArray();
			String strPath = p_strPath.Replace('\\', '/').ToLowerInvariant();
			List<string> lstFiles = new List<string>();
			foreach (string strFile in m_strFomodFiles)
				if (strFile.ToLowerInvariant().StartsWith(strPath))
					lstFiles.Add(strFile);
			return lstFiles;
		}
		string[] m_strFomodFiles = null;

		/// <summary>
		/// Gets a list of all active installed plugins.
		/// </summary>
		/// <returns>A list of all active installed plugins.</returns>
		protected string[] GetActiveInstalledPlugins()
		{
			if (m_strActiveInstalledPlugins == null)
			{
				string[] strActivePlugins = m_misInstallScript.GetActivePlugins();
				List<string> lstActiveInstalled = new List<string>();
				foreach (string strActivePlugin in strActivePlugins)
					if (FileManagement.DataFileExists(strActivePlugin))
						lstActiveInstalled.Add(strActivePlugin.ToLowerInvariant());
				m_strActiveInstalledPlugins = lstActiveInstalled.ToArray();
			}
			return m_strActiveInstalledPlugins;
		}
		string[] m_strActiveInstalledPlugins = null;

		/// <summary>
		/// Determins if the specified plugin is active.
		/// </summary>
		/// <param name="p_strFile">The plugin whose state is to be dtermined.</param>
		/// <returns>true if the specified plugin is active; false otherwise.</returns>
		protected bool IsPluginActive(string p_strFile)
		{
			string[] strAtiveInstalledPlugins = GetActiveInstalledPlugins();
			foreach (string strActivePlugin in strAtiveInstalledPlugins)
				if (strActivePlugin.Equals(p_strFile.ToLowerInvariant()))
					return true;
			return false;
		}

		#endregion
	}
}
