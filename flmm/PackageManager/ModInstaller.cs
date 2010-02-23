using System;
using ChinhDo.Transactions;
using System.Xml;
using System.Security;
using System.Security.Permissions;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using fomm.Transactions;

namespace Fomm.PackageManager
{
	/// <summary>
	/// Installs a <see cref="fomod"/>.
	/// </summary>
	public class ModInstaller : ModInstallScript
	{
		private BackgroundWorkerProgressDialog m_bwdProgress = null;

		#region Properties

		/// <seealso cref="ModInstallScript.ExceptionMessage"/>
		protected override string ExceptionMessage
		{
			get
			{
				return "A problem occurred during install: " + Environment.NewLine + "{0}" + Environment.NewLine + "The mod was not installed.";
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
				return "The mod was not installed.";
			}
		}

		#endregion

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

		#region Install Methods

		/// <summary>
		/// Indicates that this script's work has already been completed if
		/// the <see cref="Fomod"/> is already active.
		/// </summary>
		/// <returns><lang cref="true"/> if the <see cref="Fomod"/> is active;
		/// <lang cref="false"/> otherwise.</returns>
		/// <seealso cref="ModInstallScript.CheckAlreadyDone()"/>
		protected override bool CheckAlreadyDone()
		{
			return Fomod.IsActive;
		}

		/// <summary>
		/// Installs the mod and activates it.
		/// </summary>
		internal void Install()
		{
			Run();
		}

		/// <summary>
		/// Installs the mod and activates it.
		/// </summary>
		protected override bool DoScript()
		{
			TransactionalFileManager.Snapshot(Program.FOIniPath);
			TransactionalFileManager.Snapshot(Program.FOPrefsIniPath);
			TransactionalFileManager.Snapshot(Program.GeckIniPath);
			TransactionalFileManager.Snapshot(Program.GeckPrefsIniPath);
			TransactionalFileManager.Snapshot(InstallLog.Current.InstallLogPath);
			TransactionalFileManager.Snapshot(Program.PluginsFile);

			try
			{
				MergeModule = new InstallLogMergeModule();
				if (Fomod.HasInstallScript)
					Fomod.IsActive = RunCustomInstallScript();
				else
					Fomod.IsActive = RunBasicInstallScript();
				if (Fomod.IsActive)
				{
					InstallLog.Current.Merge(Fomod, MergeModule);
					CommitActivePlugins();
				}
			}
			catch (Exception e)
			{
				Fomod.IsActive = false;
				throw e;
			}
			if (!Fomod.IsActive)
				return false;
			return true;
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
			try
			{
				using (m_bwdProgress = new BackgroundWorkerProgressDialog(PerformBasicInstall))
				{
					m_bwdProgress.OverallMessage = "Installing Fomod";
					m_bwdProgress.ShowItemProgress = false;
					m_bwdProgress.OverallProgressStep = 1;
					if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
						return false;
				}
			}
			finally
			{
				m_bwdProgress = null;
			}
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
			List<string> lstFiles = Fomod.GetFileList();
			m_bwdProgress.OverallProgressMaximum = lstFiles.Count;
			foreach (string strFile in lstFiles)
			{
				if (m_bwdProgress.Cancelled())
					return;
				InstallFileFromFomod(strFile);
				string strExt = Path.GetExtension(strFile).ToLowerInvariant();
				if ((strExt == ".esp" || strExt == ".esm") && strFile.IndexOfAny(chrDirectorySeperators) == -1)
					SetPluginActivation(strFile, true);
				m_bwdProgress.StepOverallProgress();
			}
		}

		#endregion
	}
}
