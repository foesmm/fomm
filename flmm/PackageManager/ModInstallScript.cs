using System;
using ChinhDo.Transactions;
using System.IO;
using System.Collections.Generic;
using fomm.Transactions;
using System.Windows.Forms;
using System.Text;
using Fomm.PackageManager.ModInstallLog;
using System.ComponentModel;
using System.Drawing;
using Fomm.AutoSorter;

namespace Fomm.PackageManager
{
	/// <summary>
	/// the base script for scripts that install or uninstall mods.
	/// </summary>
	public abstract class ModInstallScript : IDisposable
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
		private BackgroundWorkerProgressDialog m_bwdProgress = null;
		private List<string> m_lstActivePlugins = null;
		private fomod m_fomodMod = null;

		#region Properties

		/// <summary>
		/// Gets the list of active plugins.
		/// </summary>
		/// <value>The list of active plugins.</value>
		protected List<string> ActivePlugins
		{
			get
			{
				LoadActivePlugins();
				return m_lstActivePlugins;
			}
			set
			{
				m_lstActivePlugins = value;
			}
		}

		/// <summary>
		/// Gets the mod that is being scripted against.
		/// </summary>
		/// <value>The mod that is being scripted against.</value>
		public fomod Fomod
		{
			get
			{
				return m_fomodMod;
			}
		}

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
		{
			m_fomodMod = p_fomodMod;

			//make sure the permissions manager is initialized.
			// static members are (usually) only loaded upon first access.
			// this can cause a problem for our permissions manager as if
			// the first time it is called is in a domain with limited access
			// to the machine then the initialization will fail.
			// to prevent this, we call it now to make sure it is ready when we need it.
			object objIgnore = PermissionsManager.CurrentPermissions;

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
			return Run(false, true);
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
		protected bool Run(bool p_booSuppressSuccessMessage, bool p_booSetFOModReadOnly)
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
							bool booCancelled = false;
							if (p_booSetFOModReadOnly && (Fomod != null))
							{
								if (Fomod.ReadOnlyInitStepCount > 1)
								{
									using (m_bwdProgress = new BackgroundWorkerProgressDialog(BeginFOModReadOnlyTransaction))
									{
										m_bwdProgress.OverallMessage = "Preparing FOMod...";
										m_bwdProgress.ShowItemProgress = false;
										m_bwdProgress.OverallProgressMaximum = Fomod.ReadOnlyInitStepCount;
										m_bwdProgress.OverallProgressStep = 1;
										try
										{
											Fomod.ReadOnlyInitStepStarted += new CancelEventHandler(Fomod_ReadOnlyInitStepStarted);
											Fomod.ReadOnlyInitStepFinished += new CancelEventHandler(Fomod_ReadOnlyInitStepFinished);
											if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
												booCancelled = true;
										}
										finally
										{
											Fomod.ReadOnlyInitStepStarted -= new CancelEventHandler(Fomod_ReadOnlyInitStepStarted);
											Fomod.ReadOnlyInitStepFinished -= new CancelEventHandler(Fomod_ReadOnlyInitStepFinished);
										}
									}
								}
								else
									Fomod.BeginReadOnlyTransaction();
							}
							if (!booCancelled)
							{
								booSuccess = DoScript();
								if (booSuccess)
									tsTransaction.Complete();
							}
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

		/// <summary>
		/// Handles the <see cref="fomod.ReadOnlyInitStepFinished"/> event of the FOMod.
		/// </summary>
		/// <remarks>
		/// This steps the progress in the progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
		private void Fomod_ReadOnlyInitStepFinished(object sender, CancelEventArgs e)
		{
			m_bwdProgress.StepOverallProgress();
		}

		/// <summary>
		/// Handles the <see cref="fomod.ReadOnlyInitStepStarted"/> event of the FOMod.
		/// </summary>
		/// <remarks>
		/// This cancels the operation if the user has clicked cancel.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
		private void Fomod_ReadOnlyInitStepStarted(object sender, CancelEventArgs e)
		{
			e.Cancel = m_bwdProgress.Cancelled();
		}

		/// <summary>
		/// Puts the FOMod into read-only mode.
		/// </summary>
		/// <remarks>
		/// This method is called by a <see cref="BackgroundWorkerProgressDialog"/>.
		/// </remarks>
		private void BeginFOModReadOnlyTransaction()
		{
			Fomod.BeginReadOnlyTransaction();
		}

		#endregion

		#region UI

		#region MessageBox

		/// <summary>
		/// Shows a message box with the given message.
		/// </summary>
		/// <param name="p_strMessage">The message to display in the message box.</param>
		public void MessageBox(string p_strMessage)
		{
			PermissionsManager.CurrentPermissions.Assert();
			System.Windows.Forms.MessageBox.Show(p_strMessage);
		}

		/// <summary>
		/// Shows a message box with the given message and title.
		/// </summary>
		/// <param name="p_strMessage">The message to display in the message box.</param>
		/// <param name="p_strTitle">The message box's title, display in the title bar.</param>
		public void MessageBox(string p_strMessage, string p_strTitle)
		{
			PermissionsManager.CurrentPermissions.Assert();
			System.Windows.Forms.MessageBox.Show(p_strMessage, p_strTitle);
		}

		/// <summary>
		/// Shows a message box with the given message, title, and buttons.
		/// </summary>
		/// <param name="p_strMessage">The message to display in the message box.</param>
		/// <param name="p_strTitle">The message box's title, display in the title bar.</param>
		/// <param name="p_mbbButtons">The buttons to show in the message box.</param>
		public DialogResult MessageBox(string p_strMessage, string p_strTitle, MessageBoxButtons p_mbbButtons)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return System.Windows.Forms.MessageBox.Show(p_strMessage, p_strTitle, p_mbbButtons);
		}

		#endregion

		/// <summary>
		/// Displays a selection form to the user.
		/// </summary>
		/// <remarks>
		/// The items, previews, and descriptions are repectively ordered. In other words,
		/// the i-th item in <paramref name="p_strItems"/> uses the i-th preview in
		/// <paramref name="p_strPreviews"/> and the i-th description in <paramref name="p_strDescriptions"/>.
		/// 
		/// Similarly, the idices return as results correspond to the indices of the items in
		/// <paramref name="p_strItems"/>.
		/// </remarks>
		/// <param name="p_strItems">The items from which to select.</param>
		/// <param name="p_strPreviews">The preview image file names for the items.</param>
		/// <param name="p_strDescriptions">The descriptions of the items.</param>
		/// <param name="p_strTitle">The title of the selection form.</param>
		/// <param name="p_booSelectMany">Whether more than one item can be selected.</param>
		/// <returns>The indices of the selected items.</returns>
		public int[] Select(string[] p_strItems, string[] p_strPreviews, string[] p_strDescriptions, string p_strTitle, bool p_booSelectMany)
		{
			PermissionsManager.CurrentPermissions.Assert();
			Image[] imgPreviews = null;
			if (p_strPreviews != null)
			{
				imgPreviews = new Image[p_strPreviews.Length];
				int intMissingImages = 0;
				for (int i = 0; i < p_strPreviews.Length; i++)
				{
					if (p_strPreviews[i] == null)
						continue;
					try
					{
						imgPreviews[i] = Fomod.GetImage(p_strPreviews[i]);
					}
					catch (Exception e)
					{
						if ((e is FileNotFoundException) || (e is DecompressionException))
							intMissingImages++;
						else
							throw e;
					}
				}
				//for now I don't think the user needs to be able to detect this.
				// i don't think it is severe enough to be an exception, as it may be
				// intentional, and if it is a bug it should be readily apparent
				// during testing.
				/*if (intMissingImages > 0)
				{
					m_strLastError = "There were " + intMissingImages + " filenames specified for preview images which could not be loaded";
				}*/
			}
			SelectForm sfmSelectForm = new SelectForm(p_strItems, p_strTitle, p_booSelectMany, imgPreviews, p_strDescriptions);
			sfmSelectForm.ShowDialog();
			int[] intResults = new int[sfmSelectForm.SelectedIndex.Length];
			for (int i = 0; i < sfmSelectForm.SelectedIndex.Length; i++)
				intResults[i] = sfmSelectForm.SelectedIndex[i];
			return intResults;
		}

		/// <summary>
		/// Creates a form that can be used in custom mod scripts.
		/// </summary>
		/// <returns>A form that can be used in custom mod scripts.</returns>
		public Form CreateCustomForm()
		{
			PermissionsManager.CurrentPermissions.Assert();
			return new Form();
		}

		#endregion

		#region Version Checking

		/// <summary>
		/// Gets the version of FOMM.
		/// </summary>
		/// <returns>The version of FOMM.</returns>
		public Version GetFommVersion()
		{
			return Program.MVersion;
		}

		/// <summary>
		/// Gets the version of Fallout that is installed.
		/// </summary>
		/// <returns>The version of Fallout, or <lang cref="null"/> if Fallout
		/// is not installed.</returns>
		public Version GetGameVersion()
		{
			PermissionsManager.CurrentPermissions.Assert();
			return Program.GameMode.GameVersion;
		}

		#endregion

		#region Plugin Management

		/// <summary>
		/// Gets a list of all installed plugins.
		/// </summary>
		/// <returns>A list of all installed plugins.</returns>
		public string[] GetAllPlugins()
		{
			PermissionsManager.CurrentPermissions.Assert();
			return Program.GameMode.PluginManager.OrderedPluginList;
		}

		#region Plugin Activation Info

		/// <summary>
		/// Loads the list of active plugins.
		/// </summary>
		private void LoadActivePlugins()
		{
			if (m_lstActivePlugins != null)
				return;
			PermissionsManager.CurrentPermissions.Assert();
			string[] strPlugins = Program.GameMode.PluginManager.ActivePluginList;
			for (int i = 0; i < strPlugins.Length; i++)
				strPlugins[i] = strPlugins[i].Trim().ToLowerInvariant();
			m_lstActivePlugins = new List<string>(strPlugins);
		}

		/// <summary>
		/// Retrieves a list of currently active plugins.
		/// </summary>
		/// <returns>A list of currently active plugins.</returns>
		public string[] GetActivePlugins()
		{
			LoadActivePlugins();
			PermissionsManager.CurrentPermissions.Assert();
			return Program.GameMode.PluginManager.SortPluginList(m_lstActivePlugins.ToArray());
		}

		#endregion

		#region Load Order Management

		/// <summary>
		/// Sets the load order of the plugins.
		/// </summary>
		/// <remarks>
		/// Each plugin will be moved from its current index to its indice's position
		/// in <paramref name="p_intPlugins"/>.
		/// </remarks>
		/// <param name="p_intPlugins">The new load order of the plugins. Each entry in this array
		/// contains the current index of a plugin. This array must contain all current indices.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="p_intPlugins"/> does not
		/// contain all current plugins.</exception>
		/// <exception cref="IndexOutOfRangeException">Thrown if an index in <paramref name="p_intPlugins"/>
		/// is outside the range of current plugins. In other words, it is thrown if an entry in
		/// <paramref name="p_intPlugins"/> refers to a non-existant plugin.</exception>
		public void SetLoadOrder(int[] p_intPlugins)
		{
			string[] strPluginNames = GetAllPlugins();
			if (p_intPlugins.Length != strPluginNames.Length)
				throw new ArgumentException("Length of new load order array was different to the total number of plugins");

			for (int i = 0; i < p_intPlugins.Length; i++)
				if (p_intPlugins[i] < 0 || p_intPlugins[i] >= p_intPlugins.Length)
					throw new IndexOutOfRangeException("A plugin index was out of range");

			PermissionsManager.CurrentPermissions.Assert();
			for (int i = 0; i < strPluginNames.Length; i++)
				Program.GameMode.PluginManager.SetLoadOrder(Path.Combine(Program.GameMode.PluginsPath, strPluginNames[p_intPlugins[i]]), i);
		}

		/// <summary>
		/// Moves the specified plugins to the given position in the load order.
		/// </summary>
		/// <remarks>
		/// Note that the order of the given list of plugins is not maintained. They are re-ordered
		/// to be in the same order as they are in the before-operation load order. This, I think,
		/// is somewhat counter-intuitive and may change, though likely not so as to not break
		/// backwards compatibility.
		/// </remarks>
		/// <param name="p_intPlugins">The list of plugins to move to the given position in the
		/// load order. Each entry in this array contains the current index of a plugin.</param>
		/// <param name="p_intPosition">The position in the load order to which to move the specified
		/// plugins.</param>
		public void SetLoadOrder(int[] p_intPlugins, int p_intPosition)
		{
			string[] strPluginNames = GetAllPlugins();
			PermissionsManager.CurrentPermissions.Assert();
			Array.Sort<int>(p_intPlugins);

			Int32 intLoadOrder = 0;
			for (int i = 0; i < p_intPosition; i++)
			{
				if (Array.BinarySearch<int>(p_intPlugins, i) >= 0)
					continue;
				Program.GameMode.PluginManager.SetLoadOrder(Path.Combine(Program.GameMode.PluginsPath, strPluginNames[i]), intLoadOrder++);
			}
			for (int i = 0; i < p_intPlugins.Length; i++)
				Program.GameMode.PluginManager.SetLoadOrder(Path.Combine(Program.GameMode.PluginsPath, strPluginNames[p_intPlugins[i]]), intLoadOrder++);
			for (int i = p_intPosition; i < strPluginNames.Length; i++)
			{
				if (Array.BinarySearch<int>(p_intPlugins, i) >= 0)
					continue;
				Program.GameMode.PluginManager.SetLoadOrder(Path.Combine(Program.GameMode.PluginsPath, strPluginNames[i]), intLoadOrder++);
			}
		}

		/// <summary>
		/// Determines if the plugins have been auto-sorted.
		/// </summary>
		/// <returns><lang cref="true"/> if the plugins have been auto-sorted;
		/// <lang cref="false"/> otherwise.</returns>
		public bool IsLoadOrderAutoSorted()
		{
			PermissionsManager.CurrentPermissions.Assert();
			return LoadOrderSorter.CheckList(GetAllPlugins());
		}

		/// <summary>
		/// Determins where in the load order the specified plugin would be inserted
		/// if the plugins were auto-sorted.
		/// </summary>
		/// <param name="p_strPlugin">The name of the plugin whose auto-sort insertion
		/// point is to be determined.</param>
		/// <returns>The index where the specified plugin would be inserted were the
		/// plugins to be auto-sorted.</returns>
		public int GetAutoInsertionPoint(string p_strPlugin)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return LoadOrderSorter.GetInsertionPos(GetAllPlugins(), p_strPlugin);
		}

		/// <summary>
		/// Auto-sorts the specified plugins.
		/// </summary>
		/// <remarks>
		/// This is, apparently, a beta function. Use with caution.
		/// </remarks>
		/// <param name="p_strPlugins">The list of plugins to auto-sort.</param>
		public void AutoSortPlugins(string[] p_strPlugins)
		{
			PermissionsManager.CurrentPermissions.Assert();
			LoadOrderSorter.SortList(p_strPlugins);
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
			if (!File.Exists(Path.Combine(Program.GameMode.PluginsPath, p_strName)))
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
			Program.GameMode.PluginManager.CommitActivePlugins(ActivePlugins);
			ActivePlugins = null;
		}

		#endregion

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
		/// <param name="p_strPath">The file path, relative to the Data folder, whose writability is to be verified.</param>
		/// <returns><lang cref="true"/> if the location specified by <paramref name="p_strPath"/>
		/// can be written; <lang cref="false"/> otherwise.</returns>
		protected bool TestDoOverwrite(string p_strPath)
		{
			string strDataPath = Path.GetFullPath(Path.Combine(Program.GameMode.PluginsPath, p_strPath));
			if (!File.Exists(strDataPath))
				return true;
			string strLoweredPath = strDataPath.ToLowerInvariant();
			if (m_lstOverwriteFolders.Contains(Path.GetDirectoryName(strLoweredPath)))
				return true;
			if (m_lstDontOverwriteFolders.Contains(Path.GetDirectoryName(strLoweredPath)))
				return false;
			if (m_booOverwriteAll)
				return true;
			if (m_booDontOverwriteAll)
				return false;

			string strOldMod = InstallLog.Current.GetCurrentFileOwnerName(p_strPath);
			string strMessage = null;
			if (strOldMod != null)
			{
				strMessage = String.Format("Data file '{{0}}' has already been installed by '{0}'" + Environment.NewLine +
								"Overwrite with this mod's file?", strOldMod);
			}
			else
			{
				strMessage = "Data file '{0}' already exists." + Environment.NewLine +
								"Overwrite with this mod's file?";
			}
			switch (Overwriteform.ShowDialog(String.Format(strMessage, p_strPath), true))
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
			string strDataPath = Path.GetFullPath(Path.Combine(Program.GameMode.PluginsPath, p_strPath));
			if (!Directory.Exists(Path.GetDirectoryName(strDataPath)))
				TransactionalFileManager.CreateDirectory(Path.GetDirectoryName(strDataPath));
			else
			{
				if (!TestDoOverwrite(p_strPath))
					return false;

				if (File.Exists(strDataPath))
				{
					string strDirectory = Path.GetDirectoryName(p_strPath);
					string strBackupPath = Path.GetFullPath(Path.Combine(Program.GameMode.OverwriteDirectory, strDirectory));
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
			string strDataPath = Path.GetFullPath(Path.Combine(Program.GameMode.PluginsPath, p_strFile));
			string strKey = InstallLog.Current.GetModKey(p_strFomodBaseName);
			string strDirectory = Path.GetDirectoryName(p_strFile);
			string strBackupDirectory = Path.GetFullPath(Path.Combine(Program.GameMode.OverwriteDirectory, strDirectory));
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
						string strStopDirectory = Program.GameMode.OverwriteDirectory;
						strStopDirectory = strStopDirectory.Remove(0, strStopDirectory.LastIndexOfAny(new char[] { Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar }));
						TrimEmptyDirectories(strRestoreFromPath, strStopDirectory);
					}
					else
					{
						//remove any empty directories from the data folder we may have created
						string strStopDirectory = Program.GameMode.PluginsPath;
						strStopDirectory = strStopDirectory.Remove(0, strStopDirectory.LastIndexOfAny(new char[] { Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar }));
						TrimEmptyDirectories(strDataPath, strStopDirectory);
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
				TrimEmptyDirectories(strOverwritePath, Program.GameMode.OverwriteDirectory);
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

		#region Ini File Value Retrieval

		/// <summary>
		/// Retrieves the specified settings value as a string.
		/// </summary>
		/// <param name="p_strSettingsFileName">The name of the settings file from which to retrieve the value.</param>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as a string.</returns>
		protected string GetSettingsString(string p_strSettingsFileName, string p_strSection, string p_strKey)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return NativeMethods.GetPrivateProfileString(p_strSection, p_strKey, null, Program.GameMode.SettingsFiles[p_strSettingsFileName]);
		}

		/// <summary>
		/// Retrieves the specified settings value as an integer.
		/// </summary>
		/// <param name="p_strSettingsFileName">The name of the settings file from which to retrieve the value.</param>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as an integer.</returns>
		protected Int32 GetSettingsInt(string p_strSettingsFileName, string p_strSection, string p_strKey)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return NativeMethods.GetPrivateProfileIntA(p_strSection, p_strKey, 0, Program.GameMode.SettingsFiles[p_strSettingsFileName]);
		}

		#endregion

		#region Ini Editing

		/// <summary>
		/// Sets the specified value in the specified Ini file to the given value.
		/// </summary>
		/// <param name="p_strSettingsFileName">The name of the settings file to edit.</param>
		/// <param name="p_strSection">The section in the Ini file to edit.</param>
		/// <param name="p_strKey">The key in the Ini file to edit.</param>
		/// <param name="p_strValue">The value to which to set the key.</param>
		/// <returns><lang cref="true"/> if the value was set; <lang cref="false"/>
		/// if the user chose not to overwrite the existing value.</returns>
		protected virtual bool EditINI(string p_strSettingsFileName, string p_strSection, string p_strKey, string p_strValue)
		{
			string strFile = Program.GameMode.SettingsFiles[p_strSettingsFileName];
			if (m_booDontOverwriteAllIni)
				return false;

			PermissionsManager.CurrentPermissions.Assert();
			string strLoweredFile = strFile.ToLowerInvariant();
			string strLoweredSection = p_strSection.ToLowerInvariant();
			string strLoweredKey = p_strKey.ToLowerInvariant();
			string strOldMod = InstallLog.Current.GetCurrentIniEditorModName(strFile, p_strSection, p_strKey);
			string strOldValue = NativeMethods.GetPrivateProfileString(p_strSection, p_strKey, null, strFile);
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
				switch (Overwriteform.ShowDialog(String.Format(strMessage, p_strKey, p_strSection, strFile, strOldValue, p_strValue), false))
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

		#endregion

		#region Ini Unediting

		/// <summary>
		/// Undoes the edit made to the spcified key.
		/// </summary>
		/// <param name="p_strSettingsFileName">The name of the settings file to edit.</param>
		/// <param name="p_strSection">The section in the Ini file to unedit.</param>
		/// <param name="p_strKey">The key in the Ini file to unedit.</param>
		protected void UneditIni(string p_strSettingsFileName, string p_strSection, string p_strKey)
		{
			string strLoweredFile = Program.GameMode.SettingsFiles[p_strSettingsFileName].ToLowerInvariant();
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
				NativeMethods.WritePrivateProfileStringA(p_strSection, p_strKey, strPreviousValue, strLoweredFile);
			}
			//TODO: how do we remove an Ini key? Right now, if there was no previous value the current value
			// remains
		}

		#endregion

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Cleans up used resources.
		/// </summary>
		public virtual void Dispose()
		{
		}

		#endregion
	}
}
