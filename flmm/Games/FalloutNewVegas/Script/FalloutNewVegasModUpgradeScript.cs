using System;
using System.Collections.Generic;
using Fomm.PackageManager;
using Fomm.PackageManager.ModInstallLog;
using System.IO;

namespace Fomm.Games.FalloutNewVegas.Script
{
	public class FalloutNewVegasModUpgradeScript : FalloutNewVegasModInstallScript
	{
		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		/// <param name="p_fomodMod">The <see cref="fomod"/> against which to run the script.</param>
		public FalloutNewVegasModUpgradeScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller)
			: base(p_fomodMod, p_mibInstaller)
		{
		}

		#endregion

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
					string strBackupPath = Path.Combine(Program.GameMode.OverwriteDirectory, strDirectory);
					string strOldModKey = InstallLog.Current.GetModKey(Fomod.BaseName);
					string strFile = strOldModKey + "_" + Path.GetFileName(p_strPath);
					strWritePath = Path.Combine(strBackupPath, strFile);
				}
				else
					strWritePath = Path.Combine(Program.GameMode.PluginsPath, p_strPath);
				Installer.TransactionalFileManager.WriteAllBytes(strWritePath, p_bteData);
				Installer.MergeModule.AddFile(p_strPath);
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
				Installer.MergeModule.AddIniEdit(strLoweredFile, strLoweredSection, strLoweredKey, p_strValue);
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

			string strShaderKey = String.Format("sdp:{0}/{1}", p_intPackage, p_strShaderName);
			IList<string> lstInstallers = InstallLog.Current.GetGameSpecifcValueInstallingMods(strShaderKey);
			if (lstInstallers.Contains(Fomod.BaseName))
			{
				if (lstInstallers[lstInstallers.Count - 1].Equals(Fomod.BaseName))
				{
					byte[] oldData;
					if (!Fallout3.Tools.BSA.SDPArchives.EditShader(p_intPackage, p_strShaderName, p_bteData, out oldData))
						throw new ShaderException("Failed to edit the shader");
				}
				Installer.MergeModule.AddGameSpecificValueEdit(strShaderKey, p_bteData);
				return true;
			}

			return base.EditShader(p_intPackage, p_strShaderName, p_bteData);
		}

		#endregion
	}
}
