using System;
using ChinhDo.Transactions;
using System.IO;
using System.Collections.Generic;
using fomm.Transactions;
using System.Windows.Forms;
using System.Text;
using Fomm.PackageManager.ModInstallLog;

namespace Fomm.PackageManager
{
	/// <summary>
	/// the base script for scripts that install or uninstall mods.
	/// </summary>
	public abstract class ModInstallScript : ModScript
	{
		protected static readonly object objInstallLock = new object();
		private TxFileManager m_tfmFileManager = null;
		private List<string> m_lstOverwriteFolders = new List<string>();
		private List<string> m_lstDontOverwriteFolders = new List<string>();
		private bool m_booDontOverwriteAll = false;
		private bool m_booOverwriteAll = false;
		private bool m_booDontOverwriteAllIni = false;
		private bool m_booOverwriteAllIni = false;
		private InstallLogMergeModule m_ilmModInstallLog = null;

		#region Properties

		/// <summary>
		/// Gets the transactional file manager the script is using.
		/// </summary>
		/// <value>The transactional file manager the script is using.</value>
		protected TxFileManager TransactionalFileManager
		{
			get
			{
				if (m_tfmFileManager == null)
					throw new InvalidOperationException("The transactional file manager must be initialized by calling InitTransactionalFileManager() before it is used.");
				return m_tfmFileManager;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to overwrite
		/// all Ini values.
		/// </summary>
		/// <value>A value indicating whether to overwrite
		/// all Ini values.</value>
		protected bool OverwriteAllIni
		{
			get
			{
				return m_booOverwriteAllIni;
			}
			set
			{
				m_booOverwriteAllIni = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to overwrite
		/// all files.
		/// </summary>
		/// <value>A value indicating whether to overwrite
		/// all files.</value>
		protected bool OverwriteAllFiles
		{
			get
			{
				return m_booOverwriteAll;
			}
			set
			{
				m_booOverwriteAll = value;
			}
		}

		/// <summary>
		/// Gets or sets the merge module we are using.
		/// </summary>
		/// <value>The merge module we are using.</value>
		internal InstallLogMergeModule MergeModule
		{
			get
			{
				return m_ilmModInstallLog;
			}
			set
			{
				m_ilmModInstallLog = value;
			}
		}

		/// <summary>
		/// Gets the message to display to the user when an exception is caught.
		/// </summary>
		/// <remarks>
		/// In order to display the exception message, the placeholder {0} should be used.
		/// </remarks>
		/// <value>The message to display to the user when an exception is caught.</value>
		protected abstract string ExceptionMessage
		{
			get;
		}

		/// <summary>
		/// Gets the message to display upon failure of the script.
		/// </summary>
		/// <remarks>
		/// If the value of this property is <lang cref="null"/> then no message will be
		/// displayed.
		/// </remarks>
		/// <value>The message to display upon failure of the script.</value>
		protected abstract string FailMessage
		{
			get;
		}

		/// <summary>
		/// Gets the message to display upon success of the script.
		/// </summary>
		/// <remarks>
		/// If the value of this property is <lang cref="null"/> then no message will be
		/// displayed.
		/// </remarks>
		/// <value>The message to display upon success of the script.</value>
		protected abstract string SuccessMessage
		{
			get;
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		/// <param name="p_fomodMod">The <see cref="fomod"/> to be installed or uninstalled.</param>
		public ModInstallScript(fomod p_fomodMod)
			: base(p_fomodMod)
		{
			m_tfmFileManager = new TxFileManager();
		}

		#endregion

		#region Script Execution

		/// <summary>
		/// Checks to see if the script work has already been done.
		/// </summary>
		/// <returns><lang cref="true"/> if the script work has already been done and the script
		/// doesn't need to execute; <lang cref="false"/> otherwise.</returns>
		protected virtual bool CheckAlreadyDone()
		{
			return true;
		}

		/// <summary>
		/// Does the script-specific work.
		/// </summary>
		/// <remarks>
		/// This is the method that needs to be overridden by implementers to do
		/// their script-specific work.
		/// </remarks>
		/// <returns><lang cref="true"/> if the script work was completed successfully and needs to
		/// be committed; <lang cref="false"/> otherwise.</returns>
		protected abstract bool DoScript();

		/// <summary>
		/// Runs the install script.
		/// </summary>
		protected bool Run()
		{
			return Run(false);
		}

		/// <summary>
		/// Runs the install script.
		/// </summary>
		/// <remarks>
		/// This contains the boilerplate code that needs to be done for all install-type
		/// scripts. Implementers must override the <see cref="DoScript()"/> method to
		/// implement their script-specific functionality.
		/// </remarks>
		/// <param name="p_booSuppressSuccessMessage">Indicates whether to
		/// supress the success message. This is useful for batch installs.</param>
		/// <seealso cref="DoScript()"/>
		protected bool Run(bool p_booSuppressSuccessMessage)
		{
			bool booSuccess = false;
			if (CheckAlreadyDone())
				booSuccess = true;

			if (!booSuccess)
			{
				try
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
						using (TransactionScope tsTransaction = new TransactionScope())
						{
							m_tfmFileManager = new TxFileManager();
							if (Fomod != null)
								Fomod.BeginReadOnlyTransaction();
							booSuccess = DoScript();
							if (booSuccess)
								tsTransaction.Complete();
						}
					}
				}
				catch (Exception e)
				{
#if TRACE
					Program.TraceException(e);
#endif
					StringBuilder stbError = new StringBuilder(e.Message);
					if (e is FileNotFoundException)
						stbError.Append(" (" + ((FileNotFoundException)e).FileName + ")");
					if (e is IllegalFilePathException)
						stbError.Append(" (" + ((IllegalFilePathException)e).Path + ")");
					if (e.InnerException != null)
						stbError.AppendLine().AppendLine(e.InnerException.Message);
					if (e is RollbackException)
						foreach (RollbackException.ExceptedResourceManager erm in ((RollbackException)e).ExceptedResourceManagers)
						{
							stbError.AppendLine(erm.ResourceManager.ToString());
							stbError.AppendLine(erm.Exception.Message);
							if (erm.Exception.InnerException != null)
								stbError.AppendLine(erm.Exception.InnerException.Message);
						}
					string strMessage = String.Format(ExceptionMessage, stbError.ToString());
					System.Windows.Forms.MessageBox.Show(strMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
				finally
				{
					m_lstOverwriteFolders.Clear();
					m_lstDontOverwriteFolders.Clear();
					m_tfmFileManager = null;
					m_booDontOverwriteAll = false;
					m_booOverwriteAll = false;
					m_booDontOverwriteAllIni = false;
					m_booOverwriteAllIni = false;
					ActivePlugins = null;
					m_ilmModInstallLog = null;
					if (Fomod != null)
						Fomod.EndReadOnlyTransaction();
				}
			}
			if (booSuccess && !p_booSuppressSuccessMessage && !String.IsNullOrEmpty(SuccessMessage))
				MessageBox(SuccessMessage, "Success");
			else if (!booSuccess && !String.IsNullOrEmpty(FailMessage))
				MessageBox(FailMessage, "Failure");
			return booSuccess;
		}

		#endregion

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

		protected void CommitActivePlugins()
		{
			if (ActivePlugins == null)
				return;
			File.WriteAllLines(Program.PluginsFile, ActivePlugins.ToArray());
			ActivePlugins = null;
		}

		#endregion

		#region File Management

		#region File Creation

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
		/// Installs the speified file from the FOMod to the file system.
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
		/// Installs the speified file from the FOMod to the specified location on the file system.
		/// </summary>
		/// <param name="p_strFrom">The path of the file in the FOMod to install.</param>
		/// <param name="p_strTo">The path on the file system where the file is to be created.</param>
		/// <returns><lang cref="true"/> if the file was written; <lang cref="false"/> if the user chose
		/// not to overwrite an existing file.</returns>
		/// <exception cref="FileNotFoundException">Thrown if the file referenced by
		/// <paramref name="p_strFrom"/> is not in the FOMod.</exception>
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
		public virtual bool GenerateDataFile(string p_strPath, byte[] p_bteData)
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
					if (!MergeModule.ContainsFile(p_strPath))
					{
						if (!Directory.Exists(strBackupPath))
							TransactionalFileManager.CreateDirectory(strBackupPath);

						//if we are overwriting an original value, back it up
						if (strOldModKey == null)
						{
							MergeModule.BackupOriginalDataFile(p_strPath);
							strOldModKey = InstallLog.Current.OriginalValuesKey;
						}
						string strFile = Path.GetFileName(Directory.GetFiles(Path.GetDirectoryName(strDataPath), Path.GetFileName(strDataPath))[0]);
						strFile = strOldModKey + "_" + strFile;

						strBackupPath = Path.Combine(strBackupPath, strFile);
						TransactionalFileManager.Copy(strDataPath, strBackupPath, true);
					}
					TransactionalFileManager.Delete(strDataPath);
				}
			}

			TransactionalFileManager.WriteAllBytes(strDataPath, p_bteData);
			MergeModule.AddFile(p_strPath);
			return true;
		}

		#endregion

		#region File Removal

		/// <summary>
		/// Uninstalls the specified file.
		/// </summary>
		/// <remarks>
		/// If the mod we are uninstalling doesn't own the file, then its version is removed
		/// from the overwrites directory. If the mod we are uninstalling overwrote a file when it
		/// installed the specified file, then the overwritten file is restored. Otherwise
		/// the file is deleted.
		/// </remarks>
		/// <param name="p_strPath">The path to the file that is to be uninstalled.</param>
		/// <seealso cref="UninstallDataFile(string p_strFomodBaseName, string p_strFile)"/>
		protected void UninstallDataFile(string p_strFile)
		{
			UninstallDataFile(Fomod.BaseName, p_strFile);
		}

		/// <summary>
		/// Uninstalls the specified file.
		/// </summary>
		/// <remarks>
		/// If the mod we are uninstalling doesn't own the file, then its version is removed
		/// from the overwrites directory. If the mod we are uninstalling overwrote a file when it
		/// installed the specified file, then the overwritten file is restored. Otherwise
		/// the file is deleted.
		/// 
		/// This variant of <see cref="UninstallDataFile"/> is for use when uninstalling a file
		/// for a mod whose FOMod is missing.
		/// </remarks>
		/// <param name="p_strFomodBaseName">The base name of the <see cref="fomod"/> whose file
		/// is being uninstalled.</param>
		/// <param name="p_strPath">The path to the file that is to be uninstalled.</param>
		/// <seealso cref="UninstallDataFile(string p_strFile)"/>
		protected void UninstallDataFile(string p_strFomodBaseName, string p_strFile)
		{
			PermissionsManager.CurrentPermissions.Assert();
			FileManagement.AssertFilePathIsSafe(p_strFile);
			string strDataPath = Path.GetFullPath(Path.Combine("data", p_strFile));
			string strKey = InstallLog.Current.GetModKey(p_strFomodBaseName);
			string strDirectory = Path.GetDirectoryName(p_strFile);
			string strBackupDirectory = Path.GetFullPath(Path.Combine(Program.overwriteDir, strDirectory));
			if (File.Exists(strDataPath))
			{
				string strCurrentOwnerKey = InstallLog.Current.GetCurrentFileOwnerKey(p_strFile);
				//if we didn't install the file, then leave it alone
				if (strKey.Equals(strCurrentOwnerKey))
				{
					//if we did install the file, replace it with the file we overwrote
					// if we didn't overwrite a file, then just delete it
					TransactionalFileManager.Delete(strDataPath);

					string strPreviousOwnerKey = InstallLog.Current.GetPreviousFileOwnerKey(p_strFile);
					if (strPreviousOwnerKey != null)
					{
						string strFile = strPreviousOwnerKey + "_" + Path.GetFileName(p_strFile);
						string strRestoreFromPath = Path.Combine(strBackupDirectory, strFile);
						if (File.Exists(strRestoreFromPath))
						{
							string strBackupFileName = Path.GetFileName(Directory.GetFiles(Path.GetDirectoryName(strRestoreFromPath), Path.GetFileName(strRestoreFromPath))[0]);
							string strCasedFileName = strBackupFileName.Substring(strBackupFileName.IndexOf('_') + 1);
							string strNewDataPath = Path.Combine(Path.GetDirectoryName(strDataPath), strCasedFileName);
							TransactionalFileManager.Copy(strRestoreFromPath, strNewDataPath, true);
							TransactionalFileManager.Delete(strRestoreFromPath);
						}

						//remove anny empty directories from the overwrite folder we may have created
						TrimEmptyDirectories(strRestoreFromPath, Program.overwriteDir);
					}
					else
					{
						//remove any empty directories from the data folder we may have created
						TrimEmptyDirectories(strDataPath, "data");
					}
				}
			}

			//remove our version of the file from the backup directory
			string strOverwriteFile = strKey + "_" + Path.GetFileName(p_strFile);
			string strOverwritePath = Path.Combine(strBackupDirectory, strOverwriteFile);
			if (File.Exists(strOverwritePath))
			{
				TransactionalFileManager.Delete(strOverwritePath);
				//remove anny empty directories from the overwrite folder we may have created
				TrimEmptyDirectories(strOverwritePath, Program.overwriteDir);
			}
		}

		/// <summary>
		/// Deletes any empty directories found between the start path and the end directory.
		/// </summary>
		/// <param name="p_strStartPath">The path from which to start looking for empty directories.</param>
		/// <param name="p_strStopDirectory">The directory at which to stop looking.</param>
		protected void TrimEmptyDirectories(string p_strStartPath, string p_strStopDirectory)
		{
			string strEmptyDirectory = Path.GetDirectoryName(p_strStartPath).ToLowerInvariant();
			if (!Directory.Exists(strEmptyDirectory))
				return;
			while (true)
			{
				if ((Directory.GetFiles(strEmptyDirectory).Length + Directory.GetDirectories(strEmptyDirectory).Length == 0) &&
					!strEmptyDirectory.EndsWith(p_strStopDirectory.ToLowerInvariant()))
					Directory.Delete(strEmptyDirectory);
				else
					break;
				strEmptyDirectory = Path.GetDirectoryName(strEmptyDirectory);
			}
		}

		#endregion

		#endregion

		#region Ini Management

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
		protected virtual bool EditINI(string p_strFile, string p_strSection, string p_strKey, string p_strValue)
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

		#region Ini Unediting

		/// <summary>
		/// Undoes the edit made to the spcified key.
		/// </summary>
		/// <param name="p_strFile">The Ini file to unedit.</param>
		/// <param name="p_strSection">The section in the Ini file to unedit.</param>
		/// <param name="p_strKey">The key in the Ini file to unedit.</param>
		protected void UneditIni(string p_strFile, string p_strSection, string p_strKey)
		{
			string strLoweredFile = p_strFile.ToLowerInvariant();
			string strLoweredSection = p_strSection.ToLowerInvariant();
			string strLoweredKey = p_strKey.ToLowerInvariant();

			string strKey = InstallLog.Current.GetModKey(Fomod.BaseName);
			string strCurrentOwnerKey = InstallLog.Current.GetCurrentIniEditorModKey(strLoweredFile, strLoweredSection, strLoweredKey);
			//if we didn't edit the value, then leave it alone
			if (!strKey.Equals(strCurrentOwnerKey))
				return;

			//if we did edit the value, replace if with the value we overwrote
			// if we didn't overwrite a value, then just delete it
			string strPreviousValue = InstallLog.Current.GetPreviousIniValue(strLoweredFile, strLoweredSection, strLoweredKey);
			if (strPreviousValue != null)
			{
				PermissionsManager.CurrentPermissions.Assert();
				NativeMethods.WritePrivateProfileStringA(p_strSection, p_strKey, strPreviousValue, p_strFile);
			}
			//TODO: how do we remove an Ini key? Right now, if there was no previous value the current value
			// remains
		}

		#endregion

		#endregion

		#region Shader Management

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
		public virtual bool EditShader(int p_intPackage, string p_strShaderName, byte[] p_bteData)
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
				MergeModule.BackupOriginalSpd(p_intPackage, p_strShaderName, oldData);

			MergeModule.AddSdpEdit(p_intPackage, p_strShaderName, p_bteData);
			return true;
		}

		#endregion

		#region Shader Unediting

		/// <summary>
		/// Undoes the edit made to the specified shader.
		/// </summary>
		/// <param name="p_intPackage">The package containing the shader to edit.</param>
		/// <param name="p_strShaderName">The shader to edit.</param>
		/// <exception cref="ShaderException">Thrown if the shader could not be unedited.</exception>
		protected void UneditShader(int p_intPackage, string p_strShaderName)
		{
			string strLoweredShaderName = p_strShaderName.ToLowerInvariant();

			string strKey = InstallLog.Current.GetModKey(Fomod.BaseName);
			string strCurrentOwnerKey = InstallLog.Current.GetCurrentShaderEditorModKey(p_intPackage, strLoweredShaderName);
			//if we didn't edit the shader, then leave it alone
			if (!strKey.Equals(strCurrentOwnerKey))
				return;

			//if we did edit the shader, replace it with the shader we overwrote
			// if we didn't overwrite the shader, then just delete it
			byte[] btePreviousData = InstallLog.Current.GetPreviousSdpData(p_intPackage, strLoweredShaderName);
			if (btePreviousData != null)
			{
				/*TODO: I'm not sure if this is the strictly correct way to unedit a shader
				 * the original unedit code was:
				 * 
				 *	if (m_xelModInstallLogSdpEdits != null)
				 *	{
				 *		foreach (XmlNode node in m_xelModInstallLogSdpEdits.ChildNodes)
				 *		{
				 *			//TODO: Remove this workaround for the release version
				 *			if (node.Attributes.GetNamedItem("crc") == null)
				 *			{
				 *				InstallLog.UndoShaderEdit(int.Parse(node.Attributes.GetNamedItem("package").Value), node.Attributes.GetNamedItem("shader").Value, 0);
				 *			}
				 *			else
				 *			{
				 *				InstallLog.UndoShaderEdit(int.Parse(node.Attributes.GetNamedItem("package").Value), node.Attributes.GetNamedItem("shader").Value,
				 *					uint.Parse(node.Attributes.GetNamedItem("crc").Value));
				 *			}
				 *		}
				 *	}
				 *	
				 * where InstallLog.UndoShaderEdit was:
				 * 
				 *	public void UndoShaderEdit(int package, string shader, uint crc)
				 *	{
				 *		XmlNode node = sdpEditsNode.SelectSingleNode("sdp[@package='" + package + "' and @shader='" + shader + "']");
				 *		if (node == null) return;
				 *		byte[] b = new byte[node.InnerText.Length / 2];
				 *		for (int i = 0; i < b.Length; i++)
				 *		{
				 *			b[i] = byte.Parse("" + node.InnerText[i * 2] + node.InnerText[i * 2 + 1], System.Globalization.NumberStyles.AllowHexSpecifier);
				 *		}
				 *		if (SDPArchives.RestoreShader(package, shader, b, crc)) sdpEditsNode.RemoveChild(node);
				 *	}
				 *	
				 * after looking at SDPArchives it is not clear to me why a crc was being used.
				 * if ever it becomes evident that a crc is required, I will have to alter the log to store
				 *  a crc and pass it to the RestoreShader method.
				 */

				PermissionsManager.CurrentPermissions.Assert();
				if (!SDPArchives.RestoreShader(p_intPackage, p_strShaderName, btePreviousData, 0))
					throw new ShaderException("Failed to unedit the shader");
			}
			//TODO: how do we delete a shader? Right now, if there was no previous shader the current shader
			// remains
		}

		#endregion

		#endregion
	}
}
