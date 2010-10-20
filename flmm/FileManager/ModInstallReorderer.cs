using System;
using System.Collections.Generic;
using Fomm.PackageManager;
using System.IO;
using System.Windows.Forms;
using fomm.Transactions;
using System.Text;
using Fomm.PackageManager.ModInstallLog;

namespace Fomm.FileManager
{
	public class ModInstallReorderer : ModInstallScript
	{
		private string m_strFailMessage = null;
		private string m_strFile = null;
		private IList<string> m_lstOrderedMods = null;
			
		#region Properties

		/// <seealso cref="ModInstallScript.ExceptionMessage"/>
		protected override string ExceptionMessage
		{
			get
			{
				return "A problem occurred during reorder: " + Environment.NewLine + "{0}";
			}
		}

		/// <seealso cref="ModInstallScript.SuccessMessage"/>
		protected override string SuccessMessage
		{
			get
			{
				return "The mod was successfully installed.";
			}
		}

		/// <seealso cref="ModInstallScript.FailMessage"/>
		protected override string FailMessage
		{
			get
			{
				return m_strFailMessage;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		internal ModInstallReorderer()
			: base(null)
		{
		}

		#endregion

		/// <summary>
		/// Checks to see if the script work has already been done.
		/// </summary>
		/// <remarks>
		/// This always returns <lang cref="false"/>.
		/// </remarks>
		/// <returns><lang cref="true"/> if the script work has already been done and the script
		/// doesn't need to execute; <lang cref="false"/> otherwise.</returns>
		protected override bool CheckAlreadyDone()
		{
			return false;
		}

		/// <summary>
		/// Reorders the installers of the specified file.
		/// </summary>
		/// <remarks>
		/// This changes the version of the specified file that is in the user's data directory.
		/// </remarks>
		/// <param name="p_strFile">The file whose installers are to be reordered.</param>
		/// <param name="p_lstOrderedMods">The new order of the file's installers.</param>
		/// <returns><lang cref="true"/> if the file installers were reordered;
		/// <lang cref="false"/> otherwise.</returns>
		internal bool ReorderFileInstallers(string p_strFile, List<string> p_lstOrderedMods)
		{
			m_strFile = p_strFile;
			m_lstOrderedMods = p_lstOrderedMods;
			return Run(true, true);
		}

		/// <summary>
		/// This does the moving of files and log alteration.
		/// </summary>
		/// <returns><lang cref="true"/> if the script work was completed successfully and needs to
		/// be committed; <lang cref="false"/> otherwise.</returns>
		/// <exception cref="InvalidOperationException">Thrown if m_strFile or m_lstOrderedMods are
		/// <lang cref="null"/>.</exception>
		/// <seealso cref="ModInstallScript.DoScript"/>
		protected override bool DoScript()
		{
			if ((m_strFile == null) || (m_lstOrderedMods == null))
				throw new InvalidOperationException("The File and OrderedMods properties must be set before calling Run(); or Run(string, IList<string>) can be used instead.");

			TransactionalFileManager.Snapshot(InstallLog.Current.InstallLogPath);

			string strOldOwner = InstallLog.Current.GetCurrentFileOwnerKey(m_strFile);
			InstallLog.Current.SetInstallingModsOrder(m_strFile, m_lstOrderedMods);
			string strNewOwner = InstallLog.Current.GetCurrentFileOwnerKey(m_strFile);

			if (!strNewOwner.Equals(strOldOwner))
			{
				string strDataPath = Path.GetFullPath(Path.Combine("data", m_strFile));
				strDataPath = Directory.GetFiles(Path.GetDirectoryName(strDataPath), Path.GetFileName(strDataPath))[0];
				
				string strDirectory = Path.GetDirectoryName(m_strFile);
				string strBackupPath = Path.GetFullPath(Path.Combine(Program.overwriteDir, strDirectory));
				//the old backup file is becoming the new file
				string strOldBackupFile = strNewOwner + "_" + Path.GetFileName(strDataPath);
				//the old owner is becoming the new backup file
				string strNewBackupFile = strOldOwner + "_" + Path.GetFileName(strDataPath);

				string strNewBackupPath = Path.Combine(strBackupPath, strNewBackupFile);
				string strOldBackupPath = Path.Combine(strBackupPath, strOldBackupFile);			
				if (!TransactionalFileManager.FileExists(strOldBackupPath))
				{
					m_strFailMessage = "The version of the file for " + InstallLog.Current.GetModName(strNewOwner) + " does not exist. This is likely because files in the data folder have been altered manually.";
					return false;
				}
				TransactionalFileManager.Copy(strDataPath, strNewBackupPath, true);
				string strOldBackupFileName = Path.GetFileName(Directory.GetFiles(strBackupPath, strOldBackupFile)[0]);
				string strCasedFileName = strOldBackupFileName.Substring(strOldBackupFileName.IndexOf('_') + 1);
				string strNewDataPath = Path.Combine(Path.GetDirectoryName(strDataPath), strCasedFileName);
				TransactionalFileManager.Delete(strNewDataPath);
				TransactionalFileManager.Move(strOldBackupPath, strNewDataPath);
			}
			return true;
		}
	}
}
