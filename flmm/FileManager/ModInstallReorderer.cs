using System;
using System.Collections.Generic;
using Fomm.PackageManager;
using System.IO;
using System.Windows.Forms;
using fomm.Transactions;
using System.Text;

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
			string strErrorMsg = null;
			string strErrorCaption = null;
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

							string strNewBackupPath = Path.Combine(strBackupPath, strNewBackupFile);
							string strOldBackupPath = Path.Combine(strBackupPath, strOldBackupFile);
							if (!TransactionalFileManager.FileExists(strOldBackupPath))
							{
								//we set an error string that is displayed later as showing dialogs during
								// the transaction and the lock is not good form and can cause anomalies.
								//note that the anomalies are not serious, rather they are merely UI
								// misrepresentations (basically the UI gets out of sync with the
								// Install Log - this can't cause any corruption, but it looks odd to
								// the user).
								strErrorMsg = "The version of the file for " + InstallLog.Current.GetModName(strNewOwner) + " does not exist. This is likely because files in the data folder have been altered manually.";
								strErrorCaption = "Missing Version";
								return false;
							}
							TransactionalFileManager.Copy(strDataPath, strNewBackupPath, true);
							TransactionalFileManager.Copy(strOldBackupPath, strDataPath, true);
							TransactionalFileManager.Delete(strBackupPath);
						}
						tsTransaction.Complete();
					}
				}
			}
			catch (Exception e)
			{
				StringBuilder stbError = new StringBuilder("A problem occurred during reorder: ");
				stbError.AppendLine().AppendLine(e.Message);
				if (e.InnerException != null)
					stbError.AppendLine(e.InnerException.Message);
				if (e is RollbackException)
				{
					foreach (RollbackException.ExceptedResourceManager erm in ((RollbackException)e).ExceptedResourceManagers)
					{
						stbError.AppendLine(erm.ResourceManager.ToString());
						stbError.AppendLine(erm.Exception.Message);
						if (erm.Exception.InnerException != null)
							stbError.AppendLine(erm.Exception.InnerException.Message);
					}
				}
				System.Windows.Forms.MessageBox.Show(stbError.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			finally
			{
				ReleaseTransactionalFileManager();
				if (strErrorMsg != null)
					System.Windows.Forms.MessageBox.Show(strErrorMsg, strErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return true;
		}
	}
}
