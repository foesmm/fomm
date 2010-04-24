using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Fomm.TESsnip;
using Fomm.AutoSorter;

namespace Fomm.PackageManager
{
	/// <summary>
	/// This class provides the base functionality for fomod scripts.
	/// </summary>
	public abstract class ModScript : IDisposable
	{
		private List<string> m_lstActivePlugins = null;
		private fomod m_fomodMod = null;
		private TextureManager m_txmTextureManager = null;
		private BsaManager m_bamBsaManager = null;

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
		/// Gets the <see cref="BsaManager"/> this script is using.
		/// </summary>
		/// <value>The <see cref="BsaManager"/> this script is using.</value>
		public BsaManager BsaManager
		{
			get
			{
				return m_bamBsaManager;
			}
		}

		/// <summary>
		/// Gets the <see cref="TextureManager"/> this script is using.
		/// </summary>
		/// <value>The <see cref="TextureManager"/> this script is using.</value>
		public TextureManager TextureManager
		{
			get
			{
				return m_txmTextureManager;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		/// <param name="p_fomodMod">The <see cref="fomod"/> against which to run the script.</param>
		public ModScript(fomod p_fomodMod)
		{
			m_fomodMod = p_fomodMod;
			m_txmTextureManager = new TextureManager();
			m_bamBsaManager = new BsaManager();

			//make sure the permissions manager is initialized.
			// static members are (usually) only loaded upon first access.
			// this can cause a problem for our permissions manager as if
			// the first time it is called is in a domain with limited access
			// to the machine then the initialization will fail.
			// to prevent this, we call it now to make sure it is ready when we need it.
			object objIgnore = PermissionsManager.CurrentPermissions;
		}

		#endregion

		#region Plugin Management

		/// <summary>
		/// Gets a list of all install plugins.
		/// </summary>
		/// <returns>A list of all install plugins.</returns>
		public string[] GetAllPlugins()
		{
			PermissionsManager.CurrentPermissions.Assert();
			List<string> lstFiles = new List<string>();
			lstFiles.AddRange(Directory.GetFiles("data", "*.esm"));
			lstFiles.AddRange(Directory.GetFiles("data", "*.esp"));
			FileInfo[] finfiles = new FileInfo[lstFiles.Count];
			for (int i = 0; i < finfiles.Length; i++)
				finfiles[i] = new FileInfo(lstFiles[i]);
			Array.Sort<FileInfo>(finfiles, delegate(FileInfo a, FileInfo b)
			{
				return a.LastWriteTime.CompareTo(b.LastWriteTime);
			});
			string[] strSortedPlugins = new string[finfiles.Length];
			for (int i = 0; i < finfiles.Length; i++)
				strSortedPlugins[i] = finfiles[i].Name;
			return strSortedPlugins;
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
			if (File.Exists(Program.PluginsFile))
			{
				string[] strLines = File.ReadAllLines(Program.PluginsFile);
				for (int i = 0; i < strLines.Length; i++)
					strLines[i] = strLines[i].Trim().ToLowerInvariant();
				m_lstActivePlugins = new List<string>(strLines);
			}
			else
				m_lstActivePlugins = new List<string>();
		}

		/// <summary>
		/// Retrieves a list of currently active plugins.
		/// </summary>
		/// <returns>A list of currently active plugins.</returns>
		public string[] GetActivePlugins()
		{
			LoadActivePlugins();
			PermissionsManager.CurrentPermissions.Assert();
			FileInfo[] files = new FileInfo[m_lstActivePlugins.Count];
			for (int i = 0; i < files.Length; i++)
				files[i] = new FileInfo(Path.Combine("data", m_lstActivePlugins[i]));
			Array.Sort<FileInfo>(files, delegate(FileInfo a, FileInfo b)
			{
				return a.LastWriteTime.CompareTo(b.LastWriteTime);
			});
			string[] result = new string[files.Length];
			for (int i = 0; i < files.Length; i++)
				result[i] = files[i].Name;
			return result;
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
			DateTime dteTimestamp = new DateTime(2008, 1, 1);
			TimeSpan tspTwoMins = TimeSpan.FromMinutes(2);

			for (int i = 0; i < strPluginNames.Length; i++)
			{
				File.SetLastWriteTime(Path.Combine("data", strPluginNames[p_intPlugins[i]]), dteTimestamp);
				dteTimestamp += tspTwoMins;
			}
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
			DateTime dteTimestamp = new DateTime(2008, 1, 1);
			TimeSpan tspTwoMins = TimeSpan.FromMinutes(2);

			for (int i = 0; i < p_intPosition; i++)
			{
				if (Array.BinarySearch<int>(p_intPlugins, i) >= 0)
					continue;
				File.SetLastWriteTime(Path.Combine("data", strPluginNames[i]), dteTimestamp);
				dteTimestamp += tspTwoMins;
			}
			for (int i = 0; i < p_intPlugins.Length; i++)
			{
				File.SetLastWriteTime(Path.Combine("data", strPluginNames[p_intPlugins[i]]), dteTimestamp);
				dteTimestamp += tspTwoMins;
			}
			for (int i = p_intPosition; i < strPluginNames.Length; i++)
			{
				if (Array.BinarySearch<int>(p_intPlugins, i) >= 0)
					continue;
				File.SetLastWriteTime(Path.Combine("data", strPluginNames[i]), dteTimestamp);
				dteTimestamp += tspTwoMins;
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
		/// Indicates whether or not FOSE is present.
		/// </summary>
		/// <returns><lang cref="true"/> if FOSE is installed; <lang cref="false"/> otherwise.</returns>
		public bool ScriptExtenderPresent()
		{
			PermissionsManager.CurrentPermissions.Assert();
			return File.Exists("fose_loader.exe");
		}

		/// <summary>
		/// Gets the version of FOSE that is installed.
		/// </summary>
		/// <returns>The version of FOSE that is installed, or <lang cref="null"/> if FOSE
		/// is not installed.</returns>
		public Version GetFoseVersion()
		{
			PermissionsManager.CurrentPermissions.Assert();
			if (!File.Exists("fose_loader.exe"))
				return null;
			return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo("fose_loader.exe").FileVersion.Replace(", ", "."));
		}

		/// <summary>
		/// Gets the version of Fallout that is installed.
		/// </summary>
		/// <returns>The version of Fallout, or <lang cref="null"/> if Fallout
		/// is not installed.</returns>
		public Version GetFalloutVersion()
		{
			PermissionsManager.CurrentPermissions.Assert();
			if (File.Exists("Fallout3.exe"))
				return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo("Fallout3.exe").FileVersion.Replace(", ", "."));
			if (File.Exists("Fallout3ng.exe"))
				return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo("Fallout3ng.exe").FileVersion.Replace(", ", "."));
			return null;
		}

		/// <summary>
		/// Gets the version of GECK that is installed.
		/// </summary>
		/// <returns>The version of GECK, or <lang cref="null"/> if GECK
		/// is not installed.</returns>
		public Version GetGeckVersion()
		{
			PermissionsManager.CurrentPermissions.Assert();
			if (!File.Exists("geck.exe"))
				return null;
			return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo("geck.exe").FileVersion.Replace(", ", "."));
		}

		#endregion

		#region Ini File Value Retrieval

		/// <summary>
		/// Retrieves the specified Fallout.ini value as a string.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as a string.</returns>
		public string GetFalloutIniString(string p_strSection, string p_strKey)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return NativeMethods.GetPrivateProfileString(p_strSection, p_strKey, null, Program.FOIniPath);
		}

		/// <summary>
		/// Retrieves the specified Fallout.ini value as an integer.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as an integer.</returns>
		public int GetFalloutIniInt(string p_strSection, string p_strKey)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return NativeMethods.GetPrivateProfileIntA(p_strSection, p_strKey, 0, Program.FOIniPath);
		}

		/// <summary>
		/// Retrieves the specified FalloutPrefs.ini value as a string.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as a string.</returns>
		public string GetPrefsIniString(string p_strSection, string p_strKey)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return NativeMethods.GetPrivateProfileString(p_strSection, p_strKey, null, Program.FOPrefsIniPath);
		}

		/// <summary>
		/// Retrieves the specified FalloutPrefs.ini value as an integer.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as an integer.</returns>
		public int GetPrefsIniInt(string p_strSection, string p_strKey)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return NativeMethods.GetPrivateProfileIntA(p_strSection, p_strKey, 0, Program.FOPrefsIniPath);
		}

		/// <summary>
		/// Retrieves the specified GECKCustom.ini value as a string.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as a string.</returns>
		public string GetGeckIniString(string p_strSection, string p_strKey)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return NativeMethods.GetPrivateProfileString(p_strSection, p_strKey, null, Program.GeckIniPath);
		}

		/// <summary>
		/// Retrieves the specified GECKCustom.ini value as an integer.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as an integer.</returns>
		public int GetGeckIniInt(string p_strSection, string p_strKey)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return NativeMethods.GetPrivateProfileIntA(p_strSection, p_strKey, 0, Program.GeckIniPath);
		}

		/// <summary>
		/// Retrieves the specified GECKPrefs.ini value as a string.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as a string.</returns>
		public string GetGeckPrefsIniString(string p_strSection, string p_strKey)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return NativeMethods.GetPrivateProfileString(p_strSection, p_strKey, null, Program.GeckPrefsIniPath);
		}

		/// <summary>
		/// Retrieves the specified GECKPrefs.ini value as an integer.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as an integer.</returns>
		public int GetGeckPrefsIniInt(string p_strSection, string p_strKey)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return NativeMethods.GetPrivateProfileIntA(p_strSection, p_strKey, 0, Program.GeckPrefsIniPath);
		}

		#endregion

		#region Script Compilation

		/// <summary>
		/// Sets up the script compiler for the given plugins.
		/// </summary>
		/// <param name="p_plgPlugins">The plugins for which to set up the script compiler.</param>
		public void SetupScriptCompiler(Plugin[] p_plgPlugins)
		{
			PermissionsManager.CurrentPermissions.Assert();
			Fomm.ScriptCompiler.ScriptCompiler.Setup(p_plgPlugins);
		}

		/// <summary>
		/// Compiles the result script.
		/// </summary>
		public void CompileResultScript(SubRecord sr, out Record r2, out string msg)
		{
			Fomm.ScriptCompiler.ScriptCompiler.CompileResultScript(sr, out r2, out msg);
		}

		/// <summary>
		/// Compiles a script.
		/// </summary>
		public void CompileScript(Record r2, out string msg)
		{
			Fomm.ScriptCompiler.ScriptCompiler.Compile(r2, out msg);
		}

		#endregion

		#region Misc Info

		/// <summary>
		/// Gets the specified value from the RendererInfo.txt file.
		/// </summary>
		/// <param name="p_strValue">The value to retrieve from the file.</param>
		/// <returns>The specified value from the RendererInfo.txt file, or
		/// <lang cref="null"/> if the value is not found.</returns>
		public string GetRendererInfo(string p_strValue)
		{
			PermissionsManager.CurrentPermissions.Assert();
			string[] strLines = File.ReadAllLines(Program.FORendererFile);
			for (int i = 1; i < strLines.Length; i++)
			{
				if (!strLines[i].Contains(":"))
					continue;
				string strCurrentValue = strLines[i].Remove(strLines[i].IndexOf(':')).Trim();
				if (strCurrentValue.Equals(p_strValue))
					return strLines[i].Substring(strLines[i].IndexOf(':') + 1).Trim();
			}
			return null;
		}

		/// <summary>
		/// Determines if archive invalidation is active.
		/// </summary>
		/// <returns><lang cref="true"/> if archive invalidation is active;
		/// <lang cref="false"/> otherwise.</returns>
		public bool IsAIActive()
		{
			return ArchiveInvalidation.IsActive();
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Cleans up used resources.
		/// </summary>
		public void Dispose()
		{
			if (m_txmTextureManager != null)
				m_txmTextureManager.Dispose();
			if (m_bamBsaManager != null)
				m_bamBsaManager.Dispose();
		}

		#endregion
	}
}
