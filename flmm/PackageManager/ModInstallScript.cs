using System;
using ChinhDo.Transactions;

namespace Fomm.PackageManager
{
	/// <summary>
	/// the base script for scripts that install or uninstall mods.
	/// </summary>
	public abstract class ModInstallScript : ModScript
	{
		protected static readonly object objInstallLock = new object();
		private TxFileManager m_tfmFileManager = null;
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
		/// Gets or sets a value indicating whether to not overwrite
		/// any Ini values.
		/// </summary>
		/// <value>A value indicating whether to not overwrite
		/// any Ini values.</value>
		protected bool DontOverwriteAllIni
		{
			get
			{
				return m_booDontOverwriteAllIni;
			}
			set
			{
				m_booDontOverwriteAllIni = value;
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

		/// <summary>
		/// Initializes the current transactional file manager.
		/// </summary>
		protected void InitTransactionalFileManager()
		{
			m_tfmFileManager = new TxFileManager();
		}

		/// <summary>
		/// Releases the current transactional file manager.
		/// </summary>
		protected void ReleaseTransactionalFileManager()
		{
			m_tfmFileManager = null;
		}

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
	}
}
