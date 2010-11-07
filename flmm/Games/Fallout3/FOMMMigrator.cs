using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.IO;
using Fomm.Util;
using ChinhDo.Transactions;
using fomm.Transactions;

namespace Fomm.Games.Fallout3
{
	/// <summary>
	/// This migrates files used by the mod manager from they're old FOMM (pre 0.13.0)
	/// locations to the new locations.
	/// </summary>
	public class FOMMMigrator
	{
		private BackgroundWorkerProgressDialog m_bwdProgress = null;

		/// <summary>
		/// Starts the migration, if necessary.
		/// </summary>
		/// <returns><lang cref="false"/> if the migration failed;
		/// <lang cref="true"/> otherwise.</returns>
		public bool Migrate()
		{
			if (Properties.Settings.Default.migratedFromPre0130)
				return true;

			string strOldFOMMLocation = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Fallout Mod Manager_is1", "InstallLocation", "").ToString();
			if (String.IsNullOrEmpty(strOldFOMMLocation))
				strOldFOMMLocation = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Fallout Mod Manager_is1", "InstallLocation", "").ToString();
			if (String.IsNullOrEmpty(strOldFOMMLocation))
				return true;

			Version verOldFOMM = new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo(Path.Combine(strOldFOMMLocation, "fomm.exe")).FileVersion.Replace(", ", "."));
			if (verOldFOMM < new Version("0.11.0.0"))
			{
				string strTooOldMessage = "An older version of the mod manager was detected. Unfortunately, migrating from your version of FOMM is not supported." + Environment.NewLine +
											"Would you like to continue?" + Environment.NewLine + Environment.NewLine +
											"It is recommended that you select \"No\", start your old FOMM, deactivate all of your mods, uninstall the old FOMM, manually copy your mods into: " + Environment.NewLine +
											Program.GameMode.ModDirectory + Environment.NewLine +
											" and start this programme again.";
				if (MessageBox.Show(strTooOldMessage, "Cannot Migrate", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
					return false;
			}
			else
			{
				string strMessage = "An older version of the mod manager was detected. Would you like to migrate your mods into the new programme?" + Environment.NewLine +
									"If you answer \"No\", you will have to manually copy your mods into: " + Environment.NewLine +
									Program.GameMode.ModDirectory + Environment.NewLine +
									"You will also have to reinstall the mods, so make sure you deactivate them in the old FOMM first." + Environment.NewLine +
									"Clicking \"Cancel\" will close the programme so you can deactivate the mods in the old FOMM, if you so choose." + Environment.NewLine +
									"If you are confused, click \"Yes\".";
				switch (MessageBox.Show(strMessage, "Migrate", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
				{
					case DialogResult.Cancel:
						return false;
					case DialogResult.No:
						Properties.Settings.Default.migratedFromPre0130 = true;
						Properties.Settings.Default.Save();
						return true;
				}
			}

			using (TransactionScope tsTransaction = new TransactionScope())
			{
				using (m_bwdProgress = new BackgroundWorkerProgressDialog(DoMigration))
				{
					m_bwdProgress.OverallProgressMaximum = 3;
					m_bwdProgress.OverallProgressStep = 1;
					m_bwdProgress.ItemProgressStep = 1;
					m_bwdProgress.OverallMessage = "Migrating...";
					m_bwdProgress.WorkMethodArguments = strOldFOMMLocation;
					if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
						return false;

				}
				tsTransaction.Complete();
			}

			Properties.Settings.Default.migratedFromPre0130 = true;
			Properties.Settings.Default.Save();

			return true;
		}

		/// <summary>
		/// This performs the mirgration.
		/// </summary>
		/// <param name="p_objArgs">The path to the old FOMM installation.</param>
		protected void DoMigration(object p_objArgs)
		{
			string strOldFOMMLocation = (string)p_objArgs;
			TxFileManager tfmFileManager = new TxFileManager();

			//copy the mods
			string[] strMods = Directory.GetFiles(Path.Combine(strOldFOMMLocation, "mods"), "*.fomod");
			m_bwdProgress.ItemMessage = "Copying mods...";
			m_bwdProgress.ItemProgressMaximum = strMods.Length;
			m_bwdProgress.ItemProgress = 0;
			string strModFileName = null;
			foreach (string strMod in strMods)
			{
				strModFileName = Path.GetFileName(strMod);
				m_bwdProgress.ItemMessage = "Copying mods (" + strModFileName + ")...";
				tfmFileManager.Copy(strMod, Path.Combine(Program.GameMode.ModDirectory, strModFileName), true);
				//File.Copy(strMod, Path.Combine(Program.GameMode.ModDirectory, Path.GetFileName(strMod)));
				m_bwdProgress.StepItemProgress();
				if (m_bwdProgress.Cancelled())
					return;
			}

			m_bwdProgress.StepOverallProgress();

			//copy overwrites folder
			string[] strOverwriteFiles = Directory.GetFiles(Path.Combine(strOldFOMMLocation, "overwrites"), "*.*", SearchOption.AllDirectories);
			m_bwdProgress.ItemMessage = "Copying overwrites...";
			m_bwdProgress.ItemProgressMaximum = strOverwriteFiles.Length;
			m_bwdProgress.ItemProgress = 0;
			FileUtil.Copy(tfmFileManager, Path.Combine(strOldFOMMLocation, "overwrites"), ((Fallout3GameMode)Program.GameMode).OverwriteDirectory, OverwriteFileCopied);
			

			m_bwdProgress.StepOverallProgress();

			//copy install logs
			string[] strMiscFiles = Directory.GetFiles(Path.Combine(strOldFOMMLocation, "fomm"), "InstallLog.xml*");
			m_bwdProgress.ItemMessage = "Copying info files...";
			m_bwdProgress.ItemProgressMaximum = strMiscFiles.Length;
			m_bwdProgress.ItemProgress = 0;
			foreach (string strFile in strMiscFiles)
			{
				tfmFileManager.Copy(strFile, Path.Combine(Program.GameMode.InstallInfoDirectory, Path.GetFileName(strFile)), true);
				m_bwdProgress.StepItemProgress();
				if (m_bwdProgress.Cancelled())
					return;
			}

			m_bwdProgress.StepOverallProgress();
		}

		/// <summary>
		/// Called when an overwrite file has been copied as part of the migration.
		/// </summary>
		/// <remarks>
		/// This allows the user to cancel the operation.
		/// </remarks>
		/// <param name="p_strFile">The file that was copied.</param>
		/// <returns><lang cref="true"/> if the user has cancelled;
		/// <lang cref="false"/> otherwise.</returns>
		protected bool OverwriteFileCopied(string p_strFile)
		{
			return m_bwdProgress.Cancelled();
		}
	}
}
