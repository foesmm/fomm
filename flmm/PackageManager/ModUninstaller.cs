using System;
using ChinhDo.Transactions;
using System.Windows.Forms;
using System.IO;
using fomm.Transactions;

namespace Fomm.PackageManager
{
	class ModUninstaller : ModInstallScript
	{
		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		/// <param name="p_fomodMod">The <see cref="fomod"/> to be uninstalled.</param>
		public ModUninstaller(fomod p_fomodMod)
			: base(p_fomodMod)
		{
		}

		#endregion

		/// <summary>
		/// Uninstalls the mod and deactivates it.
		/// </summary>
		/// <seealso cref="Uninstall(bool p_booSuppressSuccessMessage)"/>
		public void Uninstall()
		{
			Uninstall(false);
		}

		/// <summary>
		/// Uninstalls the mod and deactivates it.
		/// </summary>
		/// <param name="p_booSuppressSuccessMessage">Indicates whether to
		/// supress the success message. This is useful for batch uninstalls.</param>
		/// <seealso cref="Uninstall()"/>
		public void Uninstall(bool p_booSuppressSuccessMessage)
		{
			//the uninstall process modifies INI and config files.
			// if multiple sources (i.e., installs) try to modify
			// these files simultaneously the outcome is not well known
			// (e.g., one uninstall changes SETTING1 in a config file to valueA
			// while simultaneously another uninstall changes SETTING1 in the
			// file to value2 - after each uninstall commits its changes it is
			// not clear what the value of SETTING1 will be).
			// as a result, we only allow one mod to be uninstalled at a time,
			// hence the lock.
			lock (ModInstallScript.objInstallLock)
			{
				if (!Fomod.IsActive)
					return;
				try
				{
					using (TransactionScope tsTransaction = new TransactionScope())
					{
						InitTransactionalFileManager();
						TransactionalFileManager.Snapshot(Program.FOIniPath);
						TransactionalFileManager.Snapshot(Program.FOPrefsIniPath);
						TransactionalFileManager.Snapshot(Program.GeckIniPath);
						TransactionalFileManager.Snapshot(Program.GeckPrefsIniPath);
						TransactionalFileManager.Snapshot(InstallLog.Current.InstallLogPath);

						MergeModule = InstallLog.Current.GetMergeModule(Fomod.baseName);
						if (Fomod.HasUninstallScript)
							Fomod.IsActive = !RunCustomUninstallScript();
						else
							Fomod.IsActive = !RunBasicUninstallScript();
						if (!Fomod.IsActive)
						{
							InstallLog.Current.UnmergeModule(Fomod.baseName);
							tsTransaction.Complete();
						}
					}
				}
				catch (Exception e)
				{
					Fomod.IsActive = true;
					string strMessage = "A problem occurred during uninstall: " + Environment.NewLine + e.Message;
					if (e.InnerException != null)
						strMessage += Environment.NewLine + e.InnerException.Message;
					strMessage += Environment.NewLine + "The mod was not uninstalled.";
					System.Windows.Forms.MessageBox.Show(strMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				if (Fomod.IsActive)
					MergeModule = null;
				else if (!p_booSuppressSuccessMessage)
					MessageBox("The mod was successfully uninstalled.", "Success");
				ReleaseTransactionalFileManager();
			}
		}

		/// <summary>
		/// Runs the custom uninstall script included in the fomod.
		/// </summary>
		/// <returns><lang cref="true"/> if the uninstallation was successful;
		/// <lang cref="false"/> otherwise.</returns>
		/// <exception cref="FileNotFoundException">Thrown if the uninstall script
		/// cannot be found.</exception>
		protected bool RunCustomUninstallScript()
		{
			string strScript = Fomod.GetUninstallScript();
			if (strScript == null)
				throw new FileNotFoundException("No uninstall script found, even though fomod claimed to have one.");
			return false;
		}

		/// <summary>
		/// Runs the basic uninstall script.
		/// </summary>
		/// <returns><lang cref="true"/> if the installation was successful;
		/// <lang cref="false"/> otherwise.</returns>
		protected bool RunBasicUninstallScript()
		{
			PerformBasicUninstall();
			return true;
		}

		/// <summary>
		/// Performs a basic uninstall of the mod.
		/// </summary>
		/// <remarks>
		/// A basic uninstall removes all of the files that were installed by the mod,
		/// and undos all of the edits the mod made during install.
		/// </remarks>
		protected void PerformBasicUninstall()
		{
			foreach (string strFile in MergeModule.DataFiles)
				UninstallDataFile(strFile);
			foreach (InstallLogMergeModule.IniEdit iniEdit in MergeModule.IniEdits)
				UneditIni(iniEdit.File, iniEdit.Section, iniEdit.Key);
			foreach (InstallLogMergeModule.SdpEdit sdpEdit in MergeModule.SdpEdits)
				UneditShader(sdpEdit.Package, sdpEdit.ShaderName);
		}

		/// <summary>
		/// Uninstalls the specified file.
		/// </summary>
		/// <remarks>
		/// If the mod we are uninstalling nothing is done. If the mod we are uninstalling overwrote
		/// a file when it installed the specified file then the overwritten file is restored. Otherwise
		/// the file is deleted.
		/// </remarks>
		/// <param name="p_strPath">The path to the file that is to be uninstalled.</param>
		protected void UninstallDataFile(string p_strFile)
		{
			PermissionsManager.CurrentPermissions.Assert();
			FileManagement.AssertFilePathIsSafe(p_strFile);
			string strDataPath = Path.GetFullPath(Path.Combine("data", p_strFile));
			string strKey = InstallLog.Current.GetModKey(Fomod.baseName);
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
						TransactionalFileManager.Copy(strRestoreFromPath, strDataPath, true);
						TransactionalFileManager.Delete(strRestoreFromPath);

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

			string strKey = InstallLog.Current.GetModKey(Fomod.baseName);
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

		/// <summary>
		/// Undoes the edit made to the specified shader.
		/// </summary>
		/// <param name="p_intPackage">The package containing the shader to edit.</param>
		/// <param name="p_strShaderName">The shader to edit.</param>
		/// <exception cref="ShaderException">Thrown if the shader could not be unedited.</exception>
		protected void UneditShader(int p_intPackage, string p_strShaderName)
		{
			string strLoweredShaderName = p_strShaderName.ToLowerInvariant();

			string strKey = InstallLog.Current.GetModKey(Fomod.baseName);
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
	}
}
