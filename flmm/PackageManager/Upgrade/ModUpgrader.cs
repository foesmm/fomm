using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Fomm.PackageManager.Upgrade
{
	/// <summary>
	/// Performs an in-place upgrade of a <see cref="fomod"/>.
	/// </summary>
	/// <remarks>
	/// An in-place upgrade installs one fomod over top of another. This differs from deactivating the old
	/// fomod and activating the new fomod in that a deactivation/activation will cause the new fomod's
	/// files/changes to overwrite existing data, whereas an in-place upgrade puts the new fomod's data
	/// in same priority as that of the old fomod.
	/// 
	/// For example, assume Mod A (v1.0) installs File1, and Mod B overwrites File1 with a new version. If
	/// Mod A (v1.0) is upgraded with Mod A (v2.0), Mod A (v2.0)'s File1 will be place in the overwrite
	/// directory and Mod B's File1 will still be the version used.
	/// </remarks>
	public class ModUpgrader : ModInstaller
	{
		private fomod m_fomodOriginalMod = null;
		private BackgroundWorkerProgressDialog m_bwdProgress = null;

		#region Properties

		/// <seealso cref="ModInstallScript.ExceptionMessage"/>
		protected override string ExceptionMessage
		{
			get
			{
				return "A problem occurred during in-place upgrade: " + Environment.NewLine + "{0}" + Environment.NewLine + "The mod was not upgraded.";
			}
		}

		/// <seealso cref="ModInstallScript.SuccessMessage"/>
		protected override string SuccessMessage
		{
			get
			{
				return "The mod was successfully upgraded.";
			}
		}

		/// <seealso cref="ModInstallScript.FailMessage"/>
		protected override string FailMessage
		{
			get
			{
				return "The mod was not upgraded.";
			}
		}

		/// <summary>
		/// Gets the message to display inthe progress dialog.
		/// </summary>
		/// <value>The message to display inthe progress dialog.</value>
		protected virtual string ProgressMessage
		{
			get
			{
				return "Upgrading Fomod";
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		/// <param name="p_fomodMod">The <see cref="fomod"/> to be upgraded.</param>
		internal ModUpgrader(fomod p_fomodMod)
			: this(p_fomodMod, p_fomodMod.BaseName)
		{
		}

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		/// <param name="p_fomodMod">The <see cref="fomod"/> to be upgraded.</param>
		internal ModUpgrader(fomod p_fomodMod, string p_strOldBaseName)
			: base(new UpgradeFomod(p_fomodMod.filepath))
		{
			m_fomodOriginalMod = p_fomodMod;
			((UpgradeFomod)Fomod).SetBaseName(p_strOldBaseName);
		}

		#endregion

		#region Install Methods

		/// <summary>
		/// Indicates that this script's work has already been completed if
		/// the <see cref="fomod"/>'s installed version is equal to the
		/// current <see cref="Fomod"/>'s version.
		/// </summary>
		/// <returns><lang cref="true"/> if the <see cref="Fomod"/>'s installed version is equal to the
		/// current <see cref="Fomod"/>'s version; <lang cref="false"/> otherwise.</returns>
		/// <seealso cref="ModInstallScript.CheckAlreadyDone()"/>
		protected override bool CheckAlreadyDone()
		{
			InstallLog.FomodInfo fifInfo = InstallLog.Current.GetModInfo(Fomod.BaseName);
			string strCurrentVersion =  (fifInfo == null) ? null : fifInfo.Version;
			return Fomod.HumanReadableVersion.Equals(strCurrentVersion);
		}

		/// <summary>
		/// Performs an in-place upgrade of the <see cref="fomod"/>.
		/// </summary>
		internal void Upgrade()
		{
			Run();
		}

		/// <summary>
		/// Performs an in-place upgrade of the <see cref="fomod"/>.
		/// </summary>
		protected override bool DoScript()
		{
			TransactionalFileManager.Snapshot(Program.FOIniPath);
			TransactionalFileManager.Snapshot(Program.FOPrefsIniPath);
			TransactionalFileManager.Snapshot(Program.GeckIniPath);
			TransactionalFileManager.Snapshot(Program.GeckPrefsIniPath);
			TransactionalFileManager.Snapshot(InstallLog.Current.InstallLogPath);
			TransactionalFileManager.Snapshot(Program.PluginsFile);

			bool booUpgraded = false;
			try
			{
				MergeModule = new InstallLogMergeModule();
				if (Fomod.HasInstallScript)
				{
					FomodScript fscInstallScript = Fomod.GetInstallScript();
					switch (fscInstallScript.Type)
					{
						case FomodScriptType.CSharp:
							booUpgraded = RunCustomInstallScript();
							break;
						case FomodScriptType.XMLConfig:
							booUpgraded = RunXmlInstallScript();
							break;
					}
				}
				else
					booUpgraded = RunBasicInstallScript(ProgressMessage);
				if (booUpgraded)
				{
					using (m_bwdProgress = new BackgroundWorkerProgressDialog(ReconcileDifferences))
					{
						m_bwdProgress.OverallMessage = "Finalizing Upgrade";
						m_bwdProgress.ItemProgressStep = 1;
						m_bwdProgress.OverallProgressStep = 1;
						if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
							return false;
					}
					string strOldBaseName = Fomod.BaseName;
					((UpgradeFomod)Fomod).SetBaseName(((UpgradeFomod)Fomod).OriginalBaseName);
					InstallLog.Current.MergeUpgrade(Fomod, strOldBaseName, MergeModule);
					((UpgradeFomod)Fomod).SetBaseName(strOldBaseName);
					CommitActivePlugins();
				}
			}
			catch (Exception e)
			{
				booUpgraded = false;
				throw e;
			}
			m_fomodOriginalMod.IsActive = DetermineFomodActiveStatus(booUpgraded);
			return booUpgraded;
		}

		/// <summary>
		/// Determines whether or not the fomod should be activated, based on whether
		/// or not the script was successful.
		/// </summary>
		/// <param name="p_booSucceeded">Whether or not the script was successful.</param>
		/// <returns><lang cref="true"/> if the script was successful;
		/// <lang cref="false"/> otherwise.</returns>
		protected virtual bool DetermineFomodActiveStatus(bool p_booSucceeded)
		{
			return p_booSucceeded;
		}

		#endregion

		/// <summary>
		/// This undoes any changes that were made by the previous version of the fomod being upgraded, but
		/// were not made by the current version.
		/// </summary>
		/// <remarks>
		/// This method is used for the background worker.
		/// </remarks>
		private void ReconcileDifferences()
		{
			m_bwdProgress.OverallProgressMaximum = 3;
			m_bwdProgress.ItemMessage = "Synchronizing Files";
			InstallLogMergeModule ilmPreviousChanges = InstallLog.Current.GetMergeModule(Fomod.BaseName);
			m_bwdProgress.ItemProgressMaximum = ilmPreviousChanges.DataFiles.Count;
			foreach (string strFile in ilmPreviousChanges.DataFiles)
			{
				if (!MergeModule.ContainsFile(strFile))
					UninstallDataFile(strFile);
				if (m_bwdProgress.Cancelled())
					return;
				m_bwdProgress.StepItemProgress();
			}
			m_bwdProgress.StepOverallProgress();

			m_bwdProgress.ItemMessage = "Synchronizing Ini Edits";
			m_bwdProgress.ItemProgressMaximum = ilmPreviousChanges.IniEdits.Count;
			foreach (InstallLogMergeModule.IniEdit iniEdit in ilmPreviousChanges.IniEdits)
			{
				if (!MergeModule.IniEdits.Contains(iniEdit))
					UneditIni(iniEdit.File, iniEdit.Section, iniEdit.Key);
				if (m_bwdProgress.Cancelled())
					return;
				m_bwdProgress.StepItemProgress();
			}
			m_bwdProgress.StepOverallProgress();

			m_bwdProgress.ItemMessage = "Synchronizing Shader Edits";
			m_bwdProgress.ItemProgressMaximum = ilmPreviousChanges.SdpEdits.Count;
			foreach (InstallLogMergeModule.SdpEdit sdpEdit in ilmPreviousChanges.SdpEdits)
			{
				if (!MergeModule.SdpEdits.Contains(sdpEdit))
					UneditShader(sdpEdit.Package, sdpEdit.ShaderName);
				if (m_bwdProgress.Cancelled())
					return;
				m_bwdProgress.StepItemProgress();
			}
			m_bwdProgress.StepOverallProgress();
		}

		#region File Creation

		/// <summary>
		/// Writes the file represented by the given byte array to the given path.
		/// </summary>
		/// <remarks>
		/// This method writes the given data as a file at the given path, if it is owned
		/// by the fomod being upgraded. If the specified data file is not owned by the fomod
		/// being upgraded, the file is instead written to the overwrites directory.
		/// 
		/// If the file was not previously installed by the fomod, then the normal install rules apply,
		/// including confirming overwrite if applicable.
		/// </remarks>
		/// <param name="p_strPath">The path where the file is to be created.</param>
		/// <param name="p_bteData">The data that is to make up the file.</param>
		/// <returns><lang cref="true"/> if the file was written; <lang cref="false"/> if the user chose
		/// not to overwrite an existing file.</returns>
		/// <exception cref="IllegalFilePathException">Thrown if <paramref name="p_strPath"/> is
		/// not safe.</exception>
		public override bool GenerateDataFile(string p_strPath, byte[] p_bteData)
		{
			PermissionsManager.CurrentPermissions.Assert();
			FileManagement.AssertFilePathIsSafe(p_strPath);

			IList<string> lstInstallers = InstallLog.Current.GetInstallingMods(p_strPath);
			if (lstInstallers.Contains(Fomod.BaseName))
			{
				string strWritePath = null;
				if (!lstInstallers[lstInstallers.Count - 1].Equals(Fomod.BaseName))
				{
					string strDirectory = Path.GetDirectoryName(p_strPath);
					string strBackupPath = Path.GetFullPath(Path.Combine(Program.overwriteDir, strDirectory));
					string strOldModKey = InstallLog.Current.GetModKey(Fomod.BaseName);
					string strFile = strOldModKey + "_" + Path.GetFileName(p_strPath);
					strWritePath = Path.Combine(strBackupPath, strFile);
				}
				else
					strWritePath = Path.GetFullPath(Path.Combine("Data", p_strPath));
				TransactionalFileManager.WriteAllBytes(strWritePath, p_bteData);
				MergeModule.AddFile(p_strPath);
				return true;
			}

			return base.GenerateDataFile(p_strPath, p_bteData);
		}

		#endregion

		#region Ini Editing

		/// <summary>
		/// Sets the specified value in the specified Ini file to the given value.
		/// </summary>
		/// <remarks>
		/// This method edits the specified Ini value, if the latest edit is owned
		/// by the fomod being upgraded. If the latest edit is not owned by the fomod
		/// being upgraded, the edit is simply archived in the appropriate location in the
		/// install log.
		/// 
		/// If the edit was not previously installed by the fomod, then the normal install rules apply,
		/// including confirming overwrite if applicable.
		/// </remarks>
		/// <param name="p_strFile">The Ini file to edit.</param>
		/// <param name="p_strSection">The section in the Ini file to edit.</param>
		/// <param name="p_strKey">The key in the Ini file to edit.</param>
		/// <param name="p_strValue">The value to which to set the key.</param>
		/// <returns><lang cref="true"/> if the value was set; <lang cref="false"/>
		/// if the user chose not to overwrite the existing value.</returns>
		protected override bool EditINI(string p_strFile, string p_strSection, string p_strKey, string p_strValue)
		{
			PermissionsManager.CurrentPermissions.Assert();

			IList<string> lstInstallers = InstallLog.Current.GetInstallingMods(p_strFile, p_strSection, p_strKey);
			if (lstInstallers.Contains(Fomod.BaseName))
			{
				string strLoweredFile = p_strFile.ToLowerInvariant();
				string strLoweredSection = p_strSection.ToLowerInvariant();
				string strLoweredKey = p_strKey.ToLowerInvariant();
				if (lstInstallers[lstInstallers.Count - 1].Equals(Fomod.BaseName))
					NativeMethods.WritePrivateProfileStringA(strLoweredSection, strLoweredKey, p_strValue, strLoweredFile);
				MergeModule.AddIniEdit(strLoweredFile, strLoweredSection, strLoweredKey, p_strValue);
				return true;
			}

			return base.EditINI(p_strFile, p_strSection, p_strKey, p_strValue);
		}

		#endregion

		#region Shader Editing

		/// <summary>
		/// Edits the specified shader with the specified data.
		/// </summary>
		/// <remarks>
		/// This method edits the specified shader, if the latest edit is owned
		/// by the fomod being upgraded. If the latest edit is not owned by the fomod
		/// being upgraded, the edit is simply archived in the appropriate location in the
		/// install log.
		/// 
		/// If the edit was not previously installed by the fomod, then the normal install rules apply,
		/// including confirming overwrite if applicable.
		/// </remarks>
		/// <param name="p_intPackage">The package containing the shader to edit.</param>
		/// <param name="p_strShaderName">The shader to edit.</param>
		/// <param name="p_bteData">The value to which to edit the shader.</param>
		/// <returns><lang cref="true"/> if the value was set; <lang cref="false"/>
		/// if the user chose not to overwrite the existing value.</returns>
		/// <exception cref="ShaderException">Thrown if the shader could not be edited.</exception>
		public override bool EditShader(int p_intPackage, string p_strShaderName, byte[] p_bteData)
		{
			PermissionsManager.CurrentPermissions.Assert();

			IList<string> lstInstallers = InstallLog.Current.GetInstallingMods(p_intPackage, p_strShaderName);
			if (lstInstallers.Contains(Fomod.BaseName))
			{
				if (lstInstallers[lstInstallers.Count - 1].Equals(Fomod.BaseName))
				{
					byte[] oldData;
					if (!SDPArchives.EditShader(p_intPackage, p_strShaderName, p_bteData, out oldData))
						throw new ShaderException("Failed to edit the shader");
				}
				MergeModule.AddSdpEdit(p_intPackage, p_strShaderName, p_bteData);
				return true;
			}

			return base.EditShader(p_intPackage, p_strShaderName, p_bteData);
		}

		#endregion
	}
}
