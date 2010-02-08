using System;
using System.Transactions;
using ChinhDo.Transactions;
using System.Xml;
using System.Security;
using System.Security.Permissions;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Fomm.PackageManager
{
	/// <summary>
	/// Installs a <see cref="fomod"/>.
	/// </summary>
	public class ModInstaller : ModInstallScript
	{
		private List<string> m_lstOverwriteFolders = new List<string>();
		private List<string> m_lstDontOverwriteFolders = new List<string>();
		private bool m_booDontOverwriteAll = false;
		private bool m_booOverwriteAll = false;
		private bool m_booDontOverwriteAllIni = false;
		private bool m_booOverwriteAllIni = false;

		private InstallLogMergeModule m_ilmModInstallLog = null;

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		/// <param name="p_fomodMod">The <see cref="fomod"/> to be installed.</param>
		internal ModInstaller(fomod p_fomodMod)
			: base(p_fomodMod)
		{
		}

		#endregion

		/// <summary>
		/// Installs the mod and activates it.
		/// </summary>
		internal void Install()
		{
			//the install process modifies INI and config files.
			// if multiple sources (i.e., installs) try to modify
			// these files simultaneously the outcome is not well known
			// (e.g., one install changes SETTING1 in a config file to valueA
			// while simultaneously another install changes SETTING1 in the
			// file to value2 - after each install commits its changes it is
			// not clear what the value of SETTING1 will be).
			// as a result, we only allow one mod to be installed at a time,
			// hence the lock.
			lock (ModInstallScript.objInstallLock)
			{
				if (Fomod.IsActive)
					return;
				try
				{
					using (TransactionScope tsTransaction = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0)))
					{
						InitTransactionalFileManager();
						TransactionalFileManager.Snapshot(Program.FOIniPath);
						TransactionalFileManager.Snapshot(Program.FOPrefsIniPath);
						TransactionalFileManager.Snapshot(Program.GeckIniPath);
						TransactionalFileManager.Snapshot(Program.GeckPrefsIniPath);
						TransactionalFileManager.Snapshot(InstallLog.Current.InstallLogPath);
						TransactionalFileManager.Snapshot(Program.PluginsFile);

						m_ilmModInstallLog = new InstallLogMergeModule();
						if (Fomod.HasInstallScript)
							Fomod.IsActive = RunCustomInstallScript();
						else
							Fomod.IsActive = RunBasicInstallScript();
						if (Fomod.IsActive)
						{
							InstallLog.Current.Merge(Fomod.baseName, m_ilmModInstallLog);
							CommitActivePlugins();
							tsTransaction.Complete();
						}
					}
				}
				catch (Exception e)
				{
					Fomod.IsActive = false;
					string strMessage = "A problem occurred during install: " + Environment.NewLine + e.Message;
					if (e.InnerException != null)
						strMessage += Environment.NewLine + e.InnerException.Message;
					strMessage += Environment.NewLine + "The mod was not installed.";
					System.Windows.Forms.MessageBox.Show(strMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				if (!Fomod.IsActive)
					m_ilmModInstallLog = null;
				else
					MessageBox("The mod was successfully installed.", "Success");
				m_lstOverwriteFolders.Clear();
				m_lstDontOverwriteFolders.Clear();
				ReleaseTransactionalFileManager();
				m_booDontOverwriteAll = false;
				m_booOverwriteAll = false;
				m_booDontOverwriteAllIni = false;
				m_booOverwriteAllIni = false;
				ActivePlugins = null;
			}
		}

		protected void CommitActivePlugins()
		{
			if (ActivePlugins == null)
				return;
			File.WriteAllLines(Program.PluginsFile, ActivePlugins.ToArray());
			ActivePlugins = null;
		}

		/// <summary>
		/// Runs the custom install script included in the fomod.
		/// </summary>
		/// <returns><lang cref="true"/> if the installation was successful;
		/// <lang cref="false"/> otherwise.</returns>
		protected bool RunCustomInstallScript()
		{
			string strScript = Fomod.GetInstallScript();
			return ScriptCompiler.Execute(strScript, this);
		}

		/// <summary>
		/// Runs the basic install script.
		/// </summary>
		/// <returns><lang cref="true"/> if the installation was successful;
		/// <lang cref="false"/> otherwise.</returns>
		protected bool RunBasicInstallScript()
		{
			PerformBasicInstall();
			return true;
		}

		/// <summary>
		/// Performs a basic install of the mod.
		/// </summary>
		/// <remarks>
		/// A basic install installs all of the file in the mod to the Data directory
		/// or activates all esp and esm files.
		/// </remarks>
		public void PerformBasicInstall()
		{
			char[] chrDirectorySeperators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
			foreach (string strFile in Fomod.GetFileList())
			{
				InstallFileFromFomod(strFile);
				string strExt = Path.GetExtension(strFile).ToLowerInvariant();
				if ((strExt == ".esp" || strExt == ".esm") && strFile.IndexOfAny(chrDirectorySeperators) == -1)
					SetPluginActivation(strFile, true);
			}
		}

		#region Plugin Activation

		/// <summary>
		/// Sets the activated status of a plugin (i.e., and esp or esm file).
		/// </summary>
		/// <param name="p_strName">The name of the plugin to activate or deactivate.</param>
		/// <param name="p_booActivate">Whether to activate the plugin.</param>
		/// <exception cref="IllegalFilePathException">Thrown if the given path is not safe.</exception>
		/// <exception cref="FileNotFoundException">Thrown if the given plugin name
		/// is invalid or does not exist.</exception>
		public void SetPluginActivation(string p_strName, bool p_booActivate)
		{
			PermissionsManager.CurrentPermissions.Assert();
			p_strName = p_strName.ToLowerInvariant();
			if (p_strName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
				throw new IllegalFilePathException(p_strName);
			if (!File.Exists(Path.Combine("Data", p_strName)))
				throw new FileNotFoundException("Plugin does not exist", p_strName);
			if (p_booActivate)
			{
				if (!ActivePlugins.Contains(p_strName))
					ActivePlugins.Add(p_strName);
			}
			else
				ActivePlugins.Remove(p_strName);
		}

		#endregion

		#region File Management

		/// <summary>
		/// Verifies if the given file can be written.
		/// </summary>
		/// <remarks>
		/// This method checks if the given path is valid. If so, and the file does not
		/// exist, the file can be written. If the file does exist, than the user is
		/// asked to overwrite the file.
		/// </remarks>
		/// <param name="p_strPath">The file path whose writability is to be verified.</param>
		/// <returns><lang cref="true"/> if the location specified by <paramref name="p_strPath"/>
		/// can be written; <lang cref="false"/> otherwise.</returns>
		protected bool TestDoOverwrite(string p_strPath)
		{
			if (!File.Exists(p_strPath))
				return true;
			string strLoweredPath = p_strPath.ToLowerInvariant();
			if (m_lstOverwriteFolders.Contains(strLoweredPath))
				return true;
			if (m_lstDontOverwriteFolders.Contains(strLoweredPath))
				return false;
			if (m_booOverwriteAll)
				return true;
			if (m_booDontOverwriteAll)
				return false;
			switch (Overwriteform.ShowDialog("Data file '" + p_strPath + "' already exists.\nOverwrite?", true))
			{
				case OverwriteResult.Yes:
					return true;
				case OverwriteResult.No:
					return false;
				case OverwriteResult.NoToAll:
					m_booDontOverwriteAll = true;
					return false;
				case OverwriteResult.YesToAll:
					m_booOverwriteAll = true;
					return true;
				case OverwriteResult.NoToFolder:
					Queue<string> folders = new Queue<string>();
					folders.Enqueue(Path.GetDirectoryName(strLoweredPath));
					while (folders.Count > 0)
					{
						strLoweredPath = folders.Dequeue();
						if (!m_lstOverwriteFolders.Contains(strLoweredPath))
						{
							m_lstDontOverwriteFolders.Add(strLoweredPath);
							foreach (string s in Directory.GetDirectories(strLoweredPath))
							{
								folders.Enqueue(s.ToLowerInvariant());
							}
						}
					}
					return false;
				case OverwriteResult.YesToFolder:
					folders = new Queue<string>();
					folders.Enqueue(Path.GetDirectoryName(strLoweredPath));
					while (folders.Count > 0)
					{
						strLoweredPath = folders.Dequeue();
						if (!m_lstDontOverwriteFolders.Contains(strLoweredPath))
						{
							m_lstOverwriteFolders.Add(strLoweredPath);
							foreach (string s in Directory.GetDirectories(strLoweredPath))
							{
								folders.Enqueue(s.ToLowerInvariant());
							}
						}
					}
					return true;
				default:
					throw new Exception("Sanity check failed: OverwriteDialog returned a value not present in the OverwriteResult enum");
			}
		}

		/// <summary>
		/// Installs the speified file from the FOMOD to the file system.
		/// </summary>
		/// <param name="p_strFile">The path of the file to install.</param>
		/// <returns><lang cref="true"/> if the file was written; <lang cref="false"/> if the user chose
		/// not to overwrite an existing file.</returns>
		public bool InstallFileFromFomod(string p_strFile)
		{
			PermissionsManager.CurrentPermissions.Assert();
			byte[] bteFomodFile = Fomod.GetFile(p_strFile);
			return GenerateDataFile(p_strFile, bteFomodFile);
		}

		/// <summary>
		/// Installs the speified file from the FOMOD to the specified location on the file system.
		/// </summary>
		/// <param name="p_strFrom">The path of the file in the FOMOD to install.</param>
		/// <param name="p_strTo">The path on the file system where the file is to be created.</param>
		/// <returns><lang cref="true"/> if the file was written; <lang cref="false"/> if the user chose
		/// not to overwrite an existing file.</returns>
		/// <exception cref="FileNotFoundException">Thrown if the file referenced by
		/// <paramref name="p_strFrom"/> is not in the FOMOD.</exception>
		/// <exception cref="IllegalFilePathException">Thrown if <paramref name="p_strTo"/> is
		/// not safe.</exception>
		public bool CopyDataFile(string p_strFrom, string p_strTo)
		{
			byte[] bteBytes = Fomod.GetFile(p_strFrom);
			return GenerateDataFile(p_strTo, bteBytes);
		}

		/// <summary>
		/// Writes the file represented by the given byte array to the given path.
		/// </summary>
		/// <remarks>
		/// This method writes the given data as a file at the given path. If the file
		/// already exists the user is prompted to overwrite the file.
		/// </remarks>
		/// <param name="p_strPath">The path where the file is to be created.</param>
		/// <param name="p_bteData">The data that is to make up the file.</param>
		/// <returns><lang cref="true"/> if the file was written; <lang cref="false"/> if the user chose
		/// not to overwrite an existing file.</returns>
		/// <exception cref="IllegalFilePathException">Thrown if <paramref name="p_strPath"/> is
		/// not safe.</exception>
		public bool GenerateDataFile(string p_strPath, byte[] p_bteData)
		{
			PermissionsManager.CurrentPermissions.Assert();
			FileManagement.AssertFilePathIsSafe(p_strPath);
			string strDataPath = Path.GetFullPath(Path.Combine("Data", p_strPath));
			if (!Directory.Exists(Path.GetDirectoryName(strDataPath)))
				TransactionalFileManager.CreateDirectory(Path.GetDirectoryName(strDataPath));
			else
			{
				if (!TestDoOverwrite(strDataPath))
					return false;

				if (File.Exists(strDataPath))
				{
					string strDirectory = Path.GetDirectoryName(p_strPath);
					string strBackupPath = Path.GetFullPath(Path.Combine(Program.overwriteDir, strDirectory));
					string strOldModKey = InstallLog.Current.GetCurrentFileOwnerKey(p_strPath);
					//if this mod installed a file, and now we are overwriting itm
					// the install log will tell us no one owns the file, or the wrong mod owns the
					// file. so, if this mod has installed this file already just replace it, don't
					// back it up.
					if (!m_ilmModInstallLog.ContainsFile(p_strPath))
					{
						if (!Directory.Exists(strBackupPath))
							TransactionalFileManager.CreateDirectory(strBackupPath);
					
						//if we are overwriting an original value, back it up
						if (strOldModKey == null)
						{
							m_ilmModInstallLog.BackupOriginalDataFile(p_strPath);
							strOldModKey = InstallLog.Current.OriginalValuesKey;
						}
						string strFile = strOldModKey + "_" + Path.GetFileName(p_strPath);

						strBackupPath = Path.Combine(strBackupPath, strFile);
						TransactionalFileManager.Copy(strDataPath, strBackupPath, true);
					}
					TransactionalFileManager.Delete(strDataPath);
				}
			}

			TransactionalFileManager.WriteAllBytes(strDataPath, p_bteData);
			m_ilmModInstallLog.AddFile(p_strPath);
			return true;
		}

		#endregion

		#region Ini Editing

		/// <summary>
		/// Sets the specified value in the specified Ini file to the given value.
		/// </summary>
		/// <param name="p_strFile">The Ini file to edit.</param>
		/// <param name="p_strSection">The section in the Ini file to edit.</param>
		/// <param name="p_strKey">The key in the Ini file to edit.</param>
		/// <param name="p_strValue">The value to which to set the key.</param>
		/// <returns><lang cref="true"/> if the value was set; <lang cref="false"/>
		/// if the user chose not to overwrite the existing value.</returns>
		protected bool EditINI(string p_strFile, string p_strSection, string p_strKey, string p_strValue)
		{
			if (m_booDontOverwriteAllIni)
				return false;

			PermissionsManager.CurrentPermissions.Assert();
			string strLoweredFile = p_strFile.ToLowerInvariant();
			string strLoweredSection = p_strSection.ToLowerInvariant();
			string strLoweredKey = p_strKey.ToLowerInvariant();
			string strOldMod = InstallLog.Current.GetCurrentIniEditorModName(p_strFile, p_strSection, p_strKey);
			string strOldValue = NativeMethods.GetPrivateProfileString(p_strSection, p_strKey, null, p_strFile);
			if (!m_booOverwriteAllIni)
			{
				string strMessage = null;
				if (strOldMod != null)
				{
					strMessage = String.Format("Key '{{0}}' in section '{{1}}' of {{2}}} has already been overwritten by '{0}'\n" +
									"Overwrite again with this mod?\n" +
									"Current value '{{3}}', new value '{{4}}'", strOldMod);
				}
				else
				{
					strMessage = "The mod wants to modify key '{0}' in section '{1}' of {2}.\n" +
									"Allow the change?\n" +
									"Current value '{3}', new value '{4}'";
				}
				switch (Overwriteform.ShowDialog(String.Format(strMessage, p_strKey, p_strSection, p_strFile, strOldValue, p_strValue), false))
				{
					case OverwriteResult.YesToAll:
						m_booOverwriteAllIni = true;
						break;
					case OverwriteResult.NoToAll:
						m_booDontOverwriteAllIni = true;
						break;
					case OverwriteResult.Yes:
						break;
					default:
						return false;
				}
			}

			//if we are overwriting an original value, back it up
			if ((strOldMod == null) || (strOldValue != null))
				m_ilmModInstallLog.BackupOriginalIniValue(strLoweredFile, strLoweredSection, strLoweredKey, strOldValue);

			NativeMethods.WritePrivateProfileStringA(strLoweredSection, strLoweredKey, p_strValue, strLoweredFile);
			m_ilmModInstallLog.AddIniEdit(strLoweredFile, strLoweredSection, strLoweredKey, p_strValue);
			return true;
		}

		/// <summary>
		/// Sets the specified value in the Fallout.ini file to the given value. 
		/// </summary>
		/// <param name="p_strSection">The section in the Ini file to edit.</param>
		/// <param name="p_strKey">The key in the Ini file to edit.</param>
		/// <param name="p_strValue">The value to which to set the key.</param>
		/// <param name="p_booSaveOld">Not used.</param>
		/// <returns><lang cref="true"/> if the value was set; <lang cref="false"/>
		/// if the user chose not to overwrite the existing value.</returns>
		public bool EditFalloutINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
		{
			return EditINI(Program.FOIniPath, p_strSection, p_strKey, p_strValue);
		}

		/// <summary>
		/// Sets the specified value in the FalloutPrefs.ini file to the given value. 
		/// </summary>
		/// <param name="p_strSection">The section in the Ini file to edit.</param>
		/// <param name="p_strKey">The key in the Ini file to edit.</param>
		/// <param name="p_strValue">The value to which to set the key.</param>
		/// <param name="p_booSaveOld">Not used.</param>
		/// <returns><lang cref="true"/> if the value was set; <lang cref="false"/>
		/// if the user chose not to overwrite the existing value.</returns>
		public bool EditPrefsINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
		{
			return EditINI(Program.FOPrefsIniPath, p_strSection, p_strKey, p_strValue);
		}

		/// <summary>
		/// Sets the specified value in the GECKCustom.ini file to the given value. 
		/// </summary>
		/// <param name="p_strSection">The section in the Ini file to edit.</param>
		/// <param name="p_strKey">The key in the Ini file to edit.</param>
		/// <param name="p_strValue">The value to which to set the key.</param>
		/// <param name="p_booSaveOld">Not used.</param>
		/// <returns><lang cref="true"/> if the value was set; <lang cref="false"/>
		/// if the user chose not to overwrite the existing value.</returns>
		public bool EditGeckINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
		{
			return EditINI(Program.GeckIniPath, p_strSection, p_strKey, p_strValue);
		}

		/// <summary>
		/// Sets the specified value in the GECKPrefs.ini file to the given value. 
		/// </summary>
		/// <param name="p_strSection">The section in the Ini file to edit.</param>
		/// <param name="p_strKey">The key in the Ini file to edit.</param>
		/// <param name="p_strValue">The value to which to set the key.</param>
		/// <param name="p_booSaveOld">Not used.</param>
		/// <returns><lang cref="true"/> if the value was set; <lang cref="false"/>
		/// if the user chose not to overwrite the existing value.</returns>
		public bool EditGeckPrefsINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
		{
			return EditINI(Program.GeckPrefsIniPath, p_strSection, p_strKey, p_strValue);
		}

		#endregion

		#region Shader Editing

		/// <summary>
		/// Edits the specified shader with the specified data.
		/// </summary>
		/// <param name="p_intPackage">The package containing the shader to edit.</param>
		/// <param name="p_strShaderName">The shader to edit.</param>
		/// <param name="p_bteData">The value to which to edit the shader.</param>
		/// <returns><lang cref="true"/> if the value was set; <lang cref="false"/>
		/// if the user chose not to overwrite the existing value.</returns>
		/// <exception cref="ShaderException">Thrown if the shader could not be edited.</exception>
		public bool EditShader(int p_intPackage, string p_strShaderName, byte[] p_bteData)
		{
			string strOldMod = InstallLog.Current.GetCurrentShaderEditorModName(p_intPackage, p_strShaderName);
			string strMessage = null;
			if (strOldMod != null)
			{
				strMessage = String.Format("Shader '{0}' in package '{1}' has already been overwritten by '{2}'\n" +
											"Overwrite the changes?", p_strShaderName, p_intPackage, strOldMod);
				if (System.Windows.Forms.MessageBox.Show(strMessage, "Confirm Overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
					return false;
			}

			PermissionsManager.CurrentPermissions.Assert();
			byte[] oldData;
			if (!SDPArchives.EditShader(p_intPackage, p_strShaderName, p_bteData, out oldData))
				throw new ShaderException("Failed to edit the shader");

			//if we are overwriting an original shader, back it up
			if ((strOldMod == null) || (oldData != null))
				m_ilmModInstallLog.BackupOriginalSpd(p_intPackage, p_strShaderName, oldData);

			m_ilmModInstallLog.AddSdpEdit(p_intPackage, p_strShaderName, p_bteData);
			return true;
		}

		#endregion
	}
}
