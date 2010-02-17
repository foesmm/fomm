using System;
using System.Collections.Generic;
using Fomm.PackageManager;
using System.IO;
using System.Windows.Forms;
using fomm.Transactions;

namespace Fomm.FileManager
{
	public class ModInstallReorderer : ModInstallScript
	{
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
			InitTransactionalFileManager();
			try
			{
				lock (ModInstallScript.objInstallLock)
				{
					using (TransactionScope tsTransaction = new TransactionScope())
					{
						TransactionalFileManager.Snapshot(InstallLog.Current.InstallLogPath);

						string strOldOwner = InstallLog.Current.GetCurrentFileOwnerKey(p_strFile);
						InstallLog.Current.SetInstallingModsOrder(p_strFile, p_lstOrderedMods);
						string strNewOwner = InstallLog.Current.GetCurrentFileOwnerKey(p_strFile);

						if (!strNewOwner.Equals(strOldOwner))
						{
							string strDataPath = Path.GetFullPath(Path.Combine("data", p_strFile));
							string strDirectory = Path.GetDirectoryName(p_strFile);
							string strBackupPath = Path.GetFullPath(Path.Combine(Program.overwriteDir, strDirectory));
							//the old backup file is becoming the new file
							string strOldBackupFile = strNewOwner + "_" + Path.GetFileName(p_strFile);
							//the old owner is becoming the new backup file
							string strNewBackupFile = strOldOwner + "_" + Path.GetFileName(p_strFile);
							TransactionalFileManager.Copy(strDataPath, Path.Combine(strBackupPath, strNewBackupFile), true);
							strBackupPath = Path.Combine(strBackupPath, strOldBackupFile);
							TransactionalFileManager.Copy(strBackupPath, strDataPath, true);
							TransactionalFileManager.Delete(strBackupPath);
						}
						tsTransaction.Complete();
					}
				}
			}
			catch (Exception e)
			{
				System.Windows.Forms.MessageBox.Show("A problem occurred during reorder: " + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			ReleaseTransactionalFileManager();
			return true;
		}
	}
}
