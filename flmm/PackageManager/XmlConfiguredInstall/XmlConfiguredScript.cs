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
		private ModInstallScript m_misInstallScript = null;

		/// <summary>
		/// A simple constructor.
		/// </summary>
		/// <param name="misInstaller">The <see cref="ModInstallScript"/> that is installing the mod.</param>
		public XmlConfiguredScript(ModInstallScript misInstaller)
		{
			m_misInstallScript = misInstaller;
		}

		/// <summary>
		/// Hanldes the activation of the mod.
		/// </summary>
		/// <returns>true if the mod installed correctly; false otherwise.</returns>
		public bool Install()
		{
			XmlDocument xmlConfig = loadModuleConfig("fomod/ModuleConfig.xml");
			XmlNodeList xnlDependancies = xmlConfig.SelectNodes("/config/moduleDependancies/*");
			foreach (XmlNode xndDependancy in xnlDependancies)
			{
				switch (xndDependancy.Name)
				{
					case "foseDependancy":
						Version verFOSE = new Version(xndDependancy.Attributes["version"].InnerText);
						RequireFoseVersion(verFOSE);
						break;
					case "falloutDependancy":
						Version verGame = new Version(xndDependancy.Attributes["version"].InnerText);
						RequireGameVersion(verGame);
						break;
					case "fommDependancy":
						Version verFOMM = new Version(xndDependancy.Attributes["version"].InnerText);
						RequireFommVersion(verFOMM);
						break;
					case "fileDependancy":
						string strFile = xndDependancy.Attributes["file"].InnerText.ToLower();
						if (!FileManagement.DataFileExists(strFile))
						{
							m_misInstallScript.MessageBox("The following required file is missing: " + strFile);
							return false;
						}
						if ((strFile.EndsWith(".esm") || strFile.EndsWith(".esp")) &&
							(!IsPluginActive(strFile)) &&
							(m_misInstallScript.MessageBox("The following required file is installed but not active: " + strFile + System.Environment.NewLine + "Would you like to active it?", "Required File", MessageBoxButtons.YesNo) == DialogResult.No))
							return false;
						break;
				}
			}

			Dictionary<string, bool> dicPlugins = new Dictionary<string, bool>();
			string[] strPlugins = m_misInstallScript.GetAllPlugins();
			foreach (string strPlugin in strPlugins)
				dicPlugins.Add(strPlugin.ToLower(), IsPluginActive(strPlugin));

			XmlNode xndGroups = xmlConfig.SelectSingleNode("/config/optionalFileGroups");
			OptionsForm ofmOptions = new OptionsForm(this, dicPlugins, xmlConfig);
			bool booPerformInstall = false;
			if ((xndGroups == null) || (xndGroups.ChildNodes.Count == 0))
				booPerformInstall = true;
			else
				booPerformInstall = (ofmOptions.ShowDialog() == DialogResult.OK);

			if (booPerformInstall)
			{
				XmlNodeList xnlRequiredInstallFiles = xmlConfig.SelectNodes("/config/requiredInstallFiles/*");
				List<OptionsForm.PluginFile> lstRequiredFiles = OptionsForm.readFileInfo(xnlRequiredInstallFiles);
				foreach (OptionsForm.PluginFile pflRequiredFile in lstRequiredFiles)
				{
					string strSource = pflRequiredFile.Source;
					string strDest = pflRequiredFile.Destination;
					if (!pflRequiredFile.IsFolder)
					{
						if (!m_misInstallScript.CopyDataFile(strSource, strDest))
							throw new ApplicationException("Could not install " + strSource + " to " + strDest);
						if (strDest.ToLower().EndsWith(".esm") || strDest.ToLower().EndsWith(".esp"))
							m_misInstallScript.SetPluginActivation(strDest, true);
					}
					else
					{
						if (!CopyDataFolder(strSource, strDest))
							throw new ApplicationException("Could not install " + strSource + " to " + strDest);
						if (strDest.Length == 0)
						{
							if (!strSource.EndsWith("/"))
								strSource += "/";
							string[] strFiles = GetFomodFolderFileList(strSource);
							foreach (string strFile in strFiles)
								if (strFile.ToLower().EndsWith(".esm") || strFile.ToLower().EndsWith(".esp"))
								{
									string strNewFileName = strFile.Substring(strSource.Length, strFile.Length - strSource.Length);
									m_misInstallScript.SetPluginActivation(strNewFileName, true);
								}
						}
					}
				}

				List<OptionsForm.PluginFile> lstInstallFiles = ofmOptions.FilesToInstall;
				List<OptionsForm.PluginFile> lstActivateFiles = ofmOptions.PluginsToActivate;
				bool booActivate = false;
				foreach (OptionsForm.PluginFile plfFile in lstInstallFiles)
				{
					booActivate = lstActivateFiles.Contains(plfFile);
					if (plfFile.IsFolder)
					{
						string strSource = plfFile.Source;
						if (!CopyDataFolder(strSource, plfFile.Destination))
							throw new ApplicationException("Could not install " + strSource + " to " + plfFile.Destination);
						//if the destination length is greater than 0, then nothing in
						// this folder is directly in the Data folder as so cannot be
						// activated
						if (plfFile.Destination.Length == 0)
						{
							if (!strSource.EndsWith("/"))
								strSource += "/";
							string[] strFiles = GetFomodFolderFileList(strSource);
							foreach (string strFile in strFiles)
								if (strFile.ToLower().EndsWith(".esm") || strFile.ToLower().EndsWith(".esp"))
								{
									string strNewFileName = strFile.Substring(strSource.Length, strFile.Length - strSource.Length);
									m_misInstallScript.SetPluginActivation(strNewFileName, booActivate);
								}
						}
					}
					else
					{
						string strSource = plfFile.Source;
						string strDest = plfFile.Destination;
						if (!m_misInstallScript.CopyDataFile(strSource, strDest))
							throw new ApplicationException("Could not install " + strSource + " to " + strDest);
						if (String.IsNullOrEmpty(strDest))
						{
							if (strSource.ToLower().EndsWith(".esm") || strSource.ToLower().EndsWith(".esp"))
								m_misInstallScript.SetPluginActivation(strSource, booActivate);
						}
						else if (strDest.ToLower().EndsWith(".esm") || strDest.ToLower().EndsWith(".esp"))
							m_misInstallScript.SetPluginActivation(strDest, booActivate);
					}
				}

				return true;
			}

			return false;
		}

		#region Xml Helper Methods

		/// <summary>
		/// Loads the module configuration schema from the FOMOD.
		/// </summary>
		/// <returns>The module configuration schema.</returns>
		private XmlSchema loadModuleConfigSchema()
		{
			string strSchemaPath = Path.Combine(Program.fommDir, "ModConfig.xsd");
			byte[] bteSchema = File.ReadAllBytes(strSchemaPath);
			XmlSchema xscSchema = null;
			using (MemoryStream stmSchema = new MemoryStream(bteSchema))
			{
				using (StreamReader srdSchemaReader = new StreamReader(stmSchema, true))
				{
					xscSchema = XmlSchema.Read(srdSchemaReader, delegate(object sender, ValidationEventArgs e) { throw e.Exception; });
					srdSchemaReader.Close();
				}
				stmSchema.Close();
			}
			return xscSchema;
		}

		/// <summary>
		/// Loads the module configuration file from the FOMOD.
		/// </summary>
		/// <param name="p_strConfigPath">The path to the configuration file.</param>
		/// <param name="p_strSchemaPath">The path to the configuration file schema.</param>
		/// <returns>The module configuration file.</returns>
		public XmlDocument loadModuleConfig(string p_strConfigPath)
		{
			XmlSchema xscSchema = loadModuleConfigSchema();
			XmlDocument xmlConfig = new XmlDocument();
			byte[] bteConfig = m_misInstallScript.Fomod.GetFile(p_strConfigPath);
			using (MemoryStream stmConfig = new MemoryStream(bteConfig))
			{
				using (StreamReader srdConfigReader = new StreamReader(stmConfig, true))
				{
					xmlConfig.LoadXml(srdConfigReader.ReadToEnd());
					xmlConfig.Schemas.Add(xscSchema);
					xmlConfig.Validate(new ValidationEventHandler(delegate(object s, ValidationEventArgs e) { throw e.Exception; }));
					srdConfigReader.Close();
				}
				stmConfig.Close();
			}
			return xmlConfig;
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
			List<string> lstFOMODFiles = m_misInstallScript.Fomod.GetFileList();
			String strFrom = p_strFrom.Replace('\\', '/').ToLower();
			if (!strFrom.EndsWith("/"))
				strFrom += "/";
			String strTo = p_strTo.Replace('\\', '/');
			if ((strTo.Length > 0) && (!strTo.EndsWith("/")))
				strTo += "/";
			String strFOMODFile = null;
			for (Int32 i = 0; i < lstFOMODFiles.Count; i++)
			{
				strFOMODFile = lstFOMODFiles[i];
				if (strFOMODFile.ToLower().StartsWith(strFrom))
				{
					string strNewFileName = strFOMODFile.Substring(strFrom.Length, strFOMODFile.Length - strFrom.Length);
					if (!m_misInstallScript.CopyDataFile(strFOMODFile, Path.Combine(strTo, strNewFileName)))
						return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Recursively installs all files in the specified folder.
		/// </summary>
		/// <param name="p_strFolder">The folder containing the files to install.</param>
		/// <returns>true if the install succeeded; false otherwise.</returns>
		protected bool InstallFolderFromFomod(string p_strFolder)
		{
			List<string> lstFOMODFiles = m_misInstallScript.Fomod.GetFileList();
			String strFrom = p_strFolder.Replace('\\', '/');
			String strFOMODFile = null;
			for (Int32 i = 0; i < lstFOMODFiles.Count; i++)
			{
				strFOMODFile = lstFOMODFiles[i];
				if (strFOMODFile.StartsWith(strFrom))
				{
					if (!m_misInstallScript.InstallFileFromFomod(strFOMODFile))
						return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Gets the specified image from the FOMOD.
		/// </summary>
		/// <param name="p_strFilename">The filename of the image to get.</param>
		/// <returns>The specified image</returns>
		public Image GetImageFromFomod(string p_strFilename)
		{
			byte[] bteImageData = m_misInstallScript.Fomod.GetFile(p_strFilename);
			MemoryStream stmImageData = new MemoryStream(bteImageData);
			Image imgImage = Image.FromStream(stmImageData);
			stmImageData.Close();
			return imgImage;
		}

		/// <summary>
		/// Gets a list of all files in the specified FOMOD folder.
		/// </summary>
		/// <param name="p_strPath">The FOMOD folder whose file list is to be retrieved.</param>
		/// <returns>The list of all files in the specified FOMOD folder.</returns>
		protected string[] GetFomodFolderFileList(string p_strPath)
		{
			if (m_strFomodFiles == null)
				m_strFomodFiles = m_misInstallScript.Fomod.GetFileList().ToArray();
			String strPath = p_strPath.Replace('\\', '/').ToLower();
			List<string> lstFiles = new List<string>();
			foreach (string strFile in m_strFomodFiles)
				if (strFile.ToLower().StartsWith(strPath))
					lstFiles.Add(strFile);
			return lstFiles.ToArray();
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
						lstActiveInstalled.Add(strActivePlugin.ToLower());
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
				if (strActivePlugin.Equals(p_strFile.ToLower()))
					return true;
			return false;
		}

		#endregion
	}
}
