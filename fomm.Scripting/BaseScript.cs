using System;
using Fomm.PackageManager;
using DialogResult = System.Windows.Forms.DialogResult;
using System.Windows.Forms;
using System.Drawing;

namespace fomm.Scripting
{
	public struct SelectOption
	{
		public string Item;
		public string Preview;
		public string Desc;

		public SelectOption(string item, string preview, string desc)
		{
			Item = item;
			Preview = preview;
			Desc = desc;
		}
	}

	public enum TextureFormat
	{
		R8G8B8 = 20,
		A8R8G8B8 = 21,
		X8R8G8B8 = 22,
		DXT1 = ('D' << 0) | ('X' << 8) | ('T' << 16) | ('1' << 24),
		DXT3 = ('D' << 0) | ('X' << 8) | ('T' << 16) | ('3' << 24),
		DXT5 = ('D' << 0) | ('X' << 8) | ('T' << 16) | ('5' << 24),
	}

	/// <summary>
	/// The base class for custom install scripts.
	/// </summary>
	public abstract class BaseScript
	{
		private static ModInstaller m_mdiScript = null;
		private static string m_strLastError = null;

		/// <summary>
		/// Sets up the script.
		/// </summary>
		/// <remarks>
		/// This method sets the <see cref="ModInstaller"/> this script will use
		/// to perform its work.
		/// </remarks>
		/// <param name="p_mdiScript">The <see cref="ModInstaller"/> this script will use
		/// to perform its work.</param>
		public static void Setup(ModInstaller p_mdiScript)
		{
			m_mdiScript = p_mdiScript;
		}

		#region Method Execution

		private delegate void GenereicVoidMethodDelegate();
		private delegate object GenereicReturnMethodDelegate();

		/// <summary>
		/// Executes the given void method.
		/// </summary>
		/// <remarks>
		/// This method is used to execute all void method calls the script needs to make.
		/// This allows for centralized error handling.
		/// 
		/// It should be noted that using delegates does engender a very slight performance hit,
		/// but given the nature of this application (more precisely, that this is a single-user
		/// application) there should not be any noticable difference.
		/// </remarks>
		/// <param name="p_gmdMethod">The method to execute.</param>
		/// <see cref="ExecuteMethod(GenereicReturnMethodDelegate p_gmdMethod)"/>
		private static void ExecuteMethod(GenereicVoidMethodDelegate p_gmdMethod)
		{
			try
			{
				p_gmdMethod();
			}
			catch (Exception e)
			{
				m_strLastError = e.Message;
				if (e.InnerException != null)
					m_strLastError += "\n" + e.InnerException.Message;
			}
		}

		/// <summary>
		/// Executes the given method with a return value.
		/// </summary>
		/// <remarks>
		/// This method is used to execute all method calls that return a value that
		/// the script needs to make. This allows for centralized error handling.
		/// 
		/// It should be noted that using delegates does engender a very slight performance hit,
		/// but given the nature of this application (more precisely, that this is a single-user
		/// application) there should not be any noticable difference.
		/// </remarks>
		/// <param name="p_gmdMethod">The method to execute.</param>
		/// <see cref="ExecuteMethod(GenereicVoidMethodDelegate p_gmdMethod)"/>
		private static object ExecuteMethod(GenereicReturnMethodDelegate p_gmdMethod)
		{
			try
			{
				return p_gmdMethod();
			}
			catch (Exception e)
			{
				m_strLastError = e.Message;
				if (e.InnerException != null)
					m_strLastError += "\n" + e.InnerException.Message;
			}
			return null;
		}

		#endregion

		/// <summary>
		/// Returns the last error that occurred.
		/// </summary>
		/// <returns>The last error that occurred.</returns>
		public static string GetLastError()
		{
			return m_strLastError;
		}

		#region Installation

		/// <summary>
		/// Performs a basic install of the mod.
		/// </summary>
		/// <remarks>
		/// A basic install installs all of the file in the mod to the Data directory
		/// or activates all esp and esm files.
		/// </remarks>
		/// <seealso cref="ModInstaller.PerformBasicInstall()"/>
		public static void PerformBasicInstall()
		{
			ExecuteMethod(() => m_mdiScript.PerformBasicInstall());
		}

		#endregion

		#region File Management

		/// <summary>
		/// Installs the speified file from the FOMOD to the specified location on the file system.
		/// </summary>
		/// <param name="p_strFrom">The path of the file in the FOMOD to install.</param>
		/// <param name="p_strTo">The path on the file system where the file is to be created.</param>
		/// <returns><lang cref="true"/> if the file was written; <lang cref="false"/> otherwise.</returns>
		/// <seealso cref="ModInstaller.CopyDataFile(string p_strFrom, string p_strTo)"/>
		public static bool CopyDataFile(string p_strFrom, string p_strTo)
		{
			return (bool)(ExecuteMethod(() => m_mdiScript.CopyDataFile(p_strFrom, p_strTo)) ?? false);
		}

		/// <summary>
		/// Retrieves the list of files in the FOMOD.
		/// </summary>
		/// <returns>The list of files in the FOMOD.</returns>
		/// <seealso cref="fomod.GetFileList()"/>
		public static string[] GetFomodFileList()
		{
			return (string[])ExecuteMethod(() => m_mdiScript.Fomod.GetFileList().ToArray());
		}

		/// <summary>
		/// Installs the speified file from the FOMOD to the file system.
		/// </summary>
		/// <param name="p_strFile">The path of the file to install.</param>
		/// <returns><lang cref="true"/> if the file was written; <lang cref="false"/> otherwise.</returns>
		/// <seealso cref="ModInstaller.InstallFileFromFomod(string p_strFile)"/>
		public static bool InstallFileFromFomod(string p_strFile)
		{
			return (bool)(ExecuteMethod(() => m_mdiScript.InstallFileFromFomod(p_strFile)) ?? false);
		}

		/// <summary>
		/// Retrieves the specified file from the fomod.
		/// </summary>
		/// <param name="p_strFile">The file to retrieve.</param>
		/// <returns>The requested file data.</returns>
		/// <seealso cref="fomod.GetFile(string p_strFile)"/>
		public static byte[] GetFileFromFomod(string p_strFile)
		{
			return (byte[])ExecuteMethod(() => m_mdiScript.Fomod.GetFile(p_strFile));
		}

		/// <summary>
		/// Gets a filtered list of all files in a user's Data directory.
		/// </summary>
		/// <param name="p_strPath">The subdirectory of the Data directory from which to get the listing.</param>
		/// <param name="p_strPattern">The pattern against which to filter the file paths.</param>
		/// <param name="p_booAllFolders">Whether or not to search through subdirectories.</param>
		/// <returns>A filtered list of all files in a user's Data directory.</returns>
		/// <seealso cref="FileManagement.GetExistingDataFileList(string p_strPath, string p_strPattern, bool p_booAllFolders)"/>
		public static string[] GetExistingDataFileList(string p_strPath, string p_strPattern, bool p_booAllFolders)
		{
			return (string[])ExecuteMethod(() => FileManagement.GetExistingDataFileList(p_strPath, p_strPattern, p_booAllFolders));
		}

		/// <summary>
		/// Determines if the specified file exists in the user's Data directory.
		/// </summary>
		/// <param name="p_strPath">The path of the file whose existence is to be verified.</param>
		/// <returns><lang cref="true"/> if the specified file exists; <lange cref="false"/>
		/// otherwise.</returns>
		/// <see cref="FileManagement.DataFileExists(string p_strPath)"/>
		public static bool DataFileExists(string p_strPath)
		{
			return (bool)(ExecuteMethod(() => FileManagement.DataFileExists(p_strPath)) ?? false);
		}

		/// <summary>
		/// Gets the speified file from the user's Data directory.
		/// </summary>
		/// <param name="p_strPath">The path of the file to retrieve.</param>
		/// <returns>The specified file.</returns>
		/// <see cref="FileManagement.GetExistingDataFile(string p_strPath)"/>
		public static byte[] GetExistingDataFile(string p_strPath)
		{
			return (byte[])ExecuteMethod(() => FileManagement.GetExistingDataFile(p_strPath));
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
		/// <returns><lang cref="true"/> if the file was written; <lang cref="false"/> otherwise.</returns>
		/// <seealso cref="ModInstaller.GenerateDataFile(string p_strPath, byte[] p_bteData)"/>
		public static bool GenerateDataFile(string p_strPath, byte[] p_bteData)
		{
			return (bool)(ExecuteMethod(() => m_mdiScript.GenerateDataFile(p_strPath, p_bteData)) ?? false);
		}

		#endregion

		#region UI

		#region MessageBox

		/// <summary>
		/// Shows a message box with the given message.
		/// </summary>
		/// <param name="p_strMessage">The message to display in the message box.</param>
		/// <seealso cref="ModScript.MessageBox(string p_strMessage)"/>
		public static void MessageBox(string p_strMessage)
		{
			ExecuteMethod(() => m_mdiScript.MessageBox(p_strMessage));
		}

		/// <summary>
		/// Shows a message box with the given message and title.
		/// </summary>
		/// <param name="p_strMessage">The message to display in the message box.</param>
		/// <param name="p_strTitle">The message box's title, display in the title bar.</param>
		/// <seealso cref="ModScript.MessageBox(string p_strMessage, string p_strTitle)"/>
		public static void MessageBox(string p_strMessage, string p_strTitle)
		{
			ExecuteMethod(() => m_mdiScript.MessageBox(p_strMessage, p_strTitle));
		}

		/// <summary>
		/// Shows a message box with the given message, title, and buttons.
		/// </summary>
		/// <param name="p_strMessage">The message to display in the message box.</param>
		/// <param name="p_strTitle">The message box's title, display in the title bar.</param>
		/// <param name="p_mbbButtons">The buttons to show in the message box.</param>
		/// <seealso cref="ModScript.MessageBox(string p_strMessage, string p_strTitle, MessageBoxButtons p_mbbButtons)"/>
		public static DialogResult MessageBox(string p_strMessage, string p_strTitle, MessageBoxButtons p_mbbButtons)
		{
			return (DialogResult)ExecuteMethod(() => m_mdiScript.MessageBox(p_strMessage, p_strTitle, p_mbbButtons));
		}

		#endregion

		/// <summary>
		/// Displays a selection form to the user.
		/// </summary>
		/// <param name="p_sopOptions">The options from which to select.</param>
		/// <param name="p_strTitle">The title of the selection form.</param>
		/// <param name="p_booSelectMany">Whether more than one item can be selected.</param>
		/// <returns>The indices of the selected items.</returns>
		/// <seealso cref="Select(string[] p_strItems, string[] p_strPreviews, string[] p_strDescriptions, string p_strTitle, bool p_booSelectMany)"/>
		public static int[] Select(SelectOption[] p_sopOptions, string p_strTitle, bool p_booSelectMany)
		{
			bool booHasPreviews = false;
			bool booHasDescriptions = false;
			foreach (SelectOption so in p_sopOptions)
			{
				if (so.Preview != null)
					booHasPreviews = true;
				if (so.Desc != null)
					booHasDescriptions = true;
			}
			string[] strItems = new string[p_sopOptions.Length];
			string[] strPreviews = booHasPreviews ? new string[p_sopOptions.Length] : null;
			string[] strDescriptions = booHasDescriptions ? new string[p_sopOptions.Length] : null;
			for (int i = 0; i < p_sopOptions.Length; i++)
			{
				strItems[i] = p_sopOptions[i].Item;
				if (booHasPreviews)
					strPreviews[i] = p_sopOptions[i].Preview;
				if (booHasDescriptions)
					strDescriptions[i] = p_sopOptions[i].Desc;
			}
			return Select(strItems, strPreviews, strDescriptions, p_strTitle, p_booSelectMany);
		}

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
		/// <seealso cref="ModScript.Select(string[] p_strItems, string[] p_strPreviews, string[] p_strDescriptions, string p_strTitle, bool p_booSelectMany)"/>
		public static int[] Select(string[] p_strItems, string[] p_strPreviews, string[] p_strDescriptions, string p_strTitle, bool p_booSelectMany)
		{
			return (int[])ExecuteMethod(() => m_mdiScript.Select(p_strItems, p_strPreviews, p_strDescriptions, p_strTitle, p_booSelectMany));
		}

		/// <summary>
		/// Creates a form that can be used in custom mod scripts.
		/// </summary>
		/// <returns>A form that can be used in custom mod scripts.</returns>
		/// <seealso cref="ModScript.CreateCustomForm()"/>
		public static Form CreateCustomForm()
		{
			return (Form)ExecuteMethod(() => m_mdiScript.CreateCustomForm());
		}

		#endregion

		#region Version Checking

		/// <summary>
		/// Gets the version of FOMM.
		/// </summary>
		/// <returns>The version of FOMM.</returns>
		/// <seealso cref="ModScript.GetFommVersion()"/>
		public static Version GetFommVersion()
		{
			return (Version)ExecuteMethod(() => m_mdiScript.GetFommVersion());
		}

		/// <summary>
		/// Indicates whether or not FOSE is present.
		/// </summary>
		/// <returns><lang cref="true"/> if FOSE is installed; <lang cref="false"/> otherwise.</returns>
		/// <seealso cref="ModScript.ScriptExtenderPresent()"/>
		public static bool ScriptExtenderPresent()
		{
			return (bool)(ExecuteMethod(() => m_mdiScript.ScriptExtenderPresent()) ?? false);
		}

		/// <summary>
		/// Gets the version of FOSE that is installed.
		/// </summary>
		/// <returns>The version of FOSE that is installed, or <lang cref="null"/> if FOSE
		/// is not installed.</returns>
		/// <seealso cref="ModScript.GetFoseVersion()"/>
		public static Version GetFoseVersion()
		{
			return (Version)ExecuteMethod(() => m_mdiScript.GetFoseVersion());
		}

		/// <summary>
		/// Gets the version of Fallout that is installed.
		/// </summary>
		/// <returns>The version of Fallout, or <lang cref="null"/> if Fallout
		/// is not installed.</returns>
		/// <seealso cref="ModScript.GetFalloutVersion()"/>
		public static Version GetFalloutVersion()
		{
			return (Version)ExecuteMethod(() => m_mdiScript.GetFalloutVersion());
		}

		/// <summary>
		/// Gets the version of GECK that is installed.
		/// </summary>
		/// <returns>The version of GECK, or <lang cref="null"/> if GECK
		/// is not installed.</returns>
		/// <seealso cref="ModScript.GetGeckVersion()"/>
		public static Version GetGeckVersion()
		{
			return (Version)ExecuteMethod(() => m_mdiScript.GetGeckVersion());
		}

		#endregion

		#region BSA Management

		/// <summary>
		/// Retrieves the list of files in the specified BSA.
		/// </summary>
		/// <param name="p_strBsa">The BSA whose file listing is requested.</param>
		/// <returns>The list of files contained in the specified BSA.</returns>
		/// <seealso cref="BsaManager.GetBSAFileList(string p_strBsa)"/>
		public static string[] GetBSAFileList(string p_strBsa)
		{
			return (string[])ExecuteMethod(() => m_mdiScript.BsaManager.GetBSAFileList(p_strBsa));
		}

		/// <summary>
		/// Gets the specified file from the specified BSA.
		/// </summary>
		/// <param name="p_strBsa">The BSA from which to extract the specified file.</param>
		/// <param name="p_strFile">The files to extract form the specified BSA.</param>
		/// <returns>The data of the specified file.</returns>
		/// <seealso cref="BsaManager.GetDataFileFromBSA(string p_strBsa, string p_strFile)"/>
		public static byte[] GetDataFileFromBSA(string p_strBsa, string p_strFile)
		{
			return (byte[])ExecuteMethod(() => m_mdiScript.BsaManager.GetDataFileFromBSA(p_strBsa, p_strFile));
		}

		#endregion

		#region Plugin Management

		/// <summary>
		/// Gets a list of all install plugins.
		/// </summary>
		/// <returns>A list of all install plugins.</returns>
		/// <seealso cref="ModScript.GetAllPlugins()"/>
		public static string[] GetAllPlugins()
		{
			return (string[])ExecuteMethod(() => m_mdiScript.GetAllPlugins());
		}

		#region Plugin Activation Management

		/// <summary>
		/// Retrieves a list of currently active plugins.
		/// </summary>
		/// <returns>A list of currently active plugins.</returns>
		/// <seealso cref="ModScript.GetActivePlugins()"/>
		public static string[] GetActivePlugins()
		{
			return (string[])ExecuteMethod(() => m_mdiScript.GetActivePlugins());
		}

		/// <summary>
		/// Sets the activated status of a plugin (i.e., and esp or esm file).
		/// </summary>
		/// <param name="p_strName">The name of the plugin to activate or deactivate.</param>
		/// <param name="p_booActivate">Whether to activate the plugin.</param>
		/// <seealso cref="ModInstaller.SetPluginActivation(string p_strName, bool p_booActivate)"/>
		public static void SetPluginActivation(string p_strName, bool p_booActivate)
		{
			ExecuteMethod(() => m_mdiScript.SetPluginActivation(p_strName, p_booActivate));
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
		/// <seealso cref="ModScript.SetLoadOrder(int[] p_intPlugins)"/>
		public static void SetLoadOrder(int[] p_intPlugins)
		{
			ExecuteMethod(() => m_mdiScript.SetLoadOrder(p_intPlugins));
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
		/// <seealso cref="ModScript.SetLoadOrder(int[] p_intPlugins, int p_intPosition)"/>
		public static void SetLoadOrder(int[] p_intPlugins, int p_intPosition)
		{
			ExecuteMethod(() => m_mdiScript.SetLoadOrder(p_intPlugins, p_intPosition));
		}

		/// <summary>
		/// Determines if the plugins have been auto-sorted.
		/// </summary>
		/// <returns><lang cref="true"/> if the plugins have been auto-sorted;
		/// <lang cref="false"/> otherwise.</returns>
		/// <seealso cref="ModScript.IsLoadOrderAutoSorted()"/>
		public static bool IsLoadOrderAutoSorted()
		{
			return (bool)(ExecuteMethod(() => m_mdiScript.IsLoadOrderAutoSorted()) ?? false);
		}

		/// <summary>
		/// Determins where in the load order the specified plugin would be inserted
		/// if the plugins were auto-sorted.
		/// </summary>
		/// <param name="p_strPlugin">The name of the plugin whose auto-sort insertion
		/// point is to be determined.</param>
		/// <returns>The index where the specified plugin would be inserted were the
		/// plugins to be auto-sorted.</returns>
		/// <seealso cref="ModScript.GetAutoInsertionPoint(string p_strPlugin)"/>
		public static int GetAutoInsertionPoint(string p_strPlugin)
		{
			return (int)ExecuteMethod(() => m_mdiScript.GetAutoInsertionPoint(p_strPlugin));
		}

		/// <summary>
		/// Auto-sorts the specified plugins.
		/// </summary>
		/// <remarks>
		/// This is, apparently, a beta function. Use with caution.
		/// </remarks>
		/// <param name="p_strPlugins">The list of plugins to auto-sort.</param>
		/// <seealso cref="ModScript.AutoSortPlugins(string[] p_strPlugins)"/>
		public static void AutoSortPlugins(string[] p_strPlugins)
		{
			ExecuteMethod(() => m_mdiScript.AutoSortPlugins(p_strPlugins));
		}

		#endregion

		#endregion

		#region Ini File Value Management

		#region Ini File Value Retrieval

		/// <summary>
		/// Retrieves the specified Fallout.ini value as a string.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as a string.</returns>
		/// <seealso cref="ModScript.GetFalloutIniString(string p_strSection, string p_strKey)"/>
		public static string GetFalloutIniString(string p_strSection, string p_strKey)
		{
			return (string)ExecuteMethod(() => m_mdiScript.GetFalloutIniString(p_strSection, p_strKey));
		}

		/// <summary>
		/// Retrieves the specified Fallout.ini value as an integer.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <seealso cref="ModScript.GetFalloutIniInt(string p_strSection, string p_strKey)"/>
		public static int GetFalloutIniInt(string p_strSection, string p_strKey)
		{
			return (int)ExecuteMethod(() => m_mdiScript.GetFalloutIniInt(p_strSection, p_strKey));
		}

		/// <summary>
		/// Retrieves the specified FalloutPrefs.ini value as a string.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as a string.</returns>
		/// <seealso cref="ModScript.GetPrefsIniString(string p_strSection, string p_strKey)"/>
		public static string GetPrefsIniString(string p_strSection, string p_strKey)
		{
			return (string)ExecuteMethod(() => m_mdiScript.GetPrefsIniString(p_strSection, p_strKey));
		}

		/// <summary>
		/// Retrieves the specified FalloutPrefs.ini value as an integer.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as an integer.</returns>
		/// <seealso cref="ModScript.GetPrefsIniInt(string p_strSection, string p_strKey)"/>
		public static int GetPrefsIniInt(string p_strSection, string p_strKey)
		{
			return (int)ExecuteMethod(() => m_mdiScript.GetPrefsIniInt(p_strSection, p_strKey));
		}

		/// <summary>
		/// Retrieves the specified GECKCustom.ini value as a string.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as a string.</returns>
		/// <seealso cref="ModScript.GetGeckIniString(string p_strSection, string p_strKey)"/>
		public static string GetGeckIniString(string p_strSection, string p_strKey)
		{
			return (string)ExecuteMethod(() => m_mdiScript.GetGeckIniString(p_strSection, p_strKey));
		}

		/// <summary>
		/// Retrieves the specified GECKCustom.ini value as an integer.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as an integer.</returns>
		/// <seealso cref="ModScript.GetGeckIniInt(string p_strSection, string p_strKey)"/>
		public static int GetGeckIniInt(string p_strSection, string p_strKey)
		{
			return (int)ExecuteMethod(() => m_mdiScript.GetGeckIniInt(p_strSection, p_strKey));
		}

		/// <summary>
		/// Retrieves the specified GECKPrefs.ini value as a string.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as a string.</returns>
		/// <seealso cref="ModScript.GetGeckPrefsIniString(string p_strSection, string p_strKey)"/>
		public static string GetGeckPrefsIniString(string p_strSection, string p_strKey)
		{
			return (string)ExecuteMethod(() => m_mdiScript.GetGeckPrefsIniString(p_strSection, p_strKey));
		}

		/// <summary>
		/// Retrieves the specified GECKPrefs.ini value as an integer.
		/// </summary>
		/// <param name="p_strSection">The section containing the value to retrieve.</param>
		/// <param name="p_strKey">The key of the value to retrieve.</param>
		/// <returns>The specified value as an integer.</returns>
		/// <seealso cref="ModScript.GetGeckPrefsIniInt(string p_strSection, string p_strKey)"/>
		public static int GetGeckPrefsIniInt(string p_strSection, string p_strKey)
		{
			return (int)ExecuteMethod(() => m_mdiScript.GetGeckPrefsIniInt(p_strSection, p_strKey));
		}

		#endregion

		#region Ini Editing

		/// <summary>
		/// Sets the specified value in the Fallout.ini file to the given value. 
		/// </summary>
		/// <param name="p_strSection">The section in the Ini file to edit.</param>
		/// <param name="p_strKey">The key in the Ini file to edit.</param>
		/// <param name="p_strValue">The value to which to set the key.</param>
		/// <param name="p_booSaveOld">Not used.</param>
		/// <returns><lang cref="true"/> if the value was set; <lang cref="false"/>
		/// if the user chose not to overwrite the existing value.</returns>
		/// <seealso cref="ModInstaller.EditFalloutINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)"/>
		public static bool EditFalloutINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
		{
			return (bool)(ExecuteMethod(() => m_mdiScript.EditFalloutINI(p_strSection, p_strKey, p_strValue, p_booSaveOld)) ?? false);
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
		/// <seealso cref="ModInstaller.EditPrefsINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)"/>
		public static bool EditPrefsINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
		{
			return (bool)(ExecuteMethod(() => m_mdiScript.EditPrefsINI(p_strSection, p_strKey, p_strValue, p_booSaveOld)) ?? false);
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
		/// <seealso cref="ModInstaller.EditGeckINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)"/>
		public static bool EditGeckINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
		{
			return (bool)(ExecuteMethod(() => m_mdiScript.EditGeckINI(p_strSection, p_strKey, p_strValue, p_booSaveOld)) ?? false);
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
		/// <seealso cref="ModInstaller.EditGeckPrefsINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)"/>
		public static bool EditGeckPrefsINI(string p_strSection, string p_strKey, string p_strValue, bool p_booSaveOld)
		{
			return (bool)(ExecuteMethod(() => m_mdiScript.EditGeckPrefsINI(p_strSection, p_strKey, p_strValue, p_booSaveOld)) ?? false);
		}

		#endregion

		#endregion

		#region Misc Info

		/// <summary>
		/// Gets the specified value from the RendererInfo.txt file.
		/// </summary>
		/// <param name="p_strValue">The value to retrieve from the file.</param>
		/// <returns>The specified value from the RendererInfo.txt file, or
		/// <lang cref="null"/> if the value is not found.</returns>
		/// <seealso cref="ModScript.GetRendererInfo(string p_strValue)"/>
		public static string GetRendererInfo(string p_strValue)
		{
			return (string)ExecuteMethod(() => m_mdiScript.GetRendererInfo(p_strValue));
		}

		/// <summary>
		/// Determines if archive invalidation is active.
		/// </summary>
		/// <returns><lang cref="true"/> if archive invalidation is active;
		/// <lang cref="false"/> otherwise.</returns>
		/// <seealso cref="ModScript.IsAIActive()"/>
		public static bool IsAIActive()
		{
			return (bool)(ExecuteMethod(() => m_mdiScript.IsAIActive()) ?? false);
		}

		#endregion

		#region Shader Editing

		/// <summary>
		/// Edits the specified shader with the specified data.
		/// </summary>
		/// <param name="p_intPackage">The package containing the shader to edit.</param>
		/// <param name="p_strShaderName">The shader to edit.</param>
		/// <param name="p_bteData">The value to which to edit the shader.</param>
		/// <returns><lang cref="true"/> if the value was set; <lang cref="false"/>
		/// if the user chose not to overwrite the existing value.</returns>
		/// <seealso cref="ModInstaller.EditShader(int p_intPackage, string p_strShaderName, byte[] p_bteData)"/>
		public static bool EditShader(int p_intPackage, string p_strShaderName, byte[] p_bteData)
		{
			return (bool)(ExecuteMethod(() => m_mdiScript.EditShader(p_intPackage, p_strShaderName, p_bteData)) ?? false);
		}

		#endregion

		#region Script Compilation

		/// <summary>
		/// Sets up the script compiler for the given plugins.
		/// </summary>
		/// <param name="p_plgPlugins">The plugins for which to set up the script compiler.</param>
		/// <seealso cref="ModScript.SetupScriptCompiler(Fomm.TESsnip.Plugin[] p_plgPlugins)"/>
		public static void SetupScriptCompiler(Plugin[] p_plgPlugins)
		{
			Fomm.TESsnip.Plugin[] tspPlugins = new Fomm.TESsnip.Plugin[p_plgPlugins.Length];
			for (int i = 0; i < p_plgPlugins.Length; i++)
				tspPlugins[i] = p_plgPlugins[i].Base;
			ExecuteMethod(() => m_mdiScript.SetupScriptCompiler(tspPlugins));
		}

		/// <summary>
		/// Compiles the result script.
		/// </summary>
		/// <seealso cref="ModScript.CompileResultScript(Fomm.TESsnip.SubRecord sr, out Fomm.TESsnip.Record r2, out string msg)"/>
		public static void CompileResultScript(SubRecord sr, out Record r2, out string msg)
		{
			Fomm.TESsnip.Record r;
			try
			{
				m_mdiScript.CompileResultScript(sr.Base, out r, out msg);
			}
			catch (Exception e)
			{
				m_strLastError = e.Message;
				r = null;
				msg = null;
			}
			if (r != null)
				r2 = new Record(r);
			else
				r2 = null;
		}


		/// <summary>
		/// Compiles a script.
		/// </summary>
		/// <seealso cref="ModScript.CompileScript(Fomm.TESsnip.Record r2, out string msg)"/>
		public static void CompileScript(Record r2, out string msg)
		{
			try
			{
				m_mdiScript.CompileScript(r2.Base, out msg);
				r2.SubRecords.Clear();
				for (int i = 0; i < r2.Base.SubRecords.Count; i++)
					r2.SubRecords.Add(new SubRecord(r2.Base.SubRecords[i]));
			}
			catch (Exception e)
			{
				m_strLastError = e.Message;
				msg = null;
			}
		}

		#endregion

		#region Texture Management

		/// <summary>
		/// Loads the given texture.
		/// </summary>
		/// <param name="p_bteTexture">The texture to load.</param>
		/// <returns>A pointer to the loaded texture.</returns>
		/// <seealso cref="TextureManager.LoadTexture(byte[] p_bteTexture)"/>
		public static IntPtr LoadTexture(byte[] p_bteTexture)
		{
			return (IntPtr)ExecuteMethod(() => m_mdiScript.TextureManager.LoadTexture(p_bteTexture));
		}

		/// <summary>
		/// Creates a texture with the given dimensions.
		/// </summary>
		/// <param name="p_intWidth">The width of the texture.</param>
		/// <param name="p_intHeight">The height of the texture.</param>
		/// <returns>A pointer to the new texture.</returns>
		/// <seealso cref="TextureManager.CreateTexture(int p_intWidth, int p_intHeight)"/>
		public static IntPtr CreateTexture(int p_intWidth, int p_intHeight)
		{
			return (IntPtr)ExecuteMethod(() => m_mdiScript.TextureManager.CreateTexture(p_intWidth, p_intHeight));
		}

		/// <summary>
		/// Saves the specified texture.
		/// </summary>
		/// <param name="p_ptrTexture">The pointer to the texture to save.</param>
		/// <param name="p_intFormat">The format in which to save the texture.</param>
		/// <param name="p_booMipmaps">Whether or not to create mipmaps (or maybe whether or
		/// not the given texture contains mipmaps?).</param>
		/// <returns>The saved texture.</returns>
		/// <seealso cref="TextureManager.SaveTexture(IntPtr p_ptrTexture, int p_intFormat, bool p_booMipmaps)"/>
		public static byte[] SaveTexture(IntPtr p_ptrTexture, int p_intFormat, bool p_booMipmaps)
		{
			return (byte[])ExecuteMethod(() => m_mdiScript.TextureManager.SaveTexture(p_ptrTexture, p_intFormat, p_booMipmaps));
		}

		/// <summary>
		/// Copies part of one texture to another.
		/// </summary>
		/// <param name="p_ptrSource">A pointer to the texture from which to make the copy.</param>
		/// <param name="p_rctSourceRect">The area of the source texture from which to make the copy.</param>
		/// <param name="p_ptrDestination">A pointer to the texture to which to make the copy.</param>
		/// <param name="p_rctDestinationRect">The area of the destination texture to which to make the copy.</param>
		/// <seealso cref="TextureManager.CopyTexture(IntPtr p_ptrSource, Rectangle p_rctSourceRect, IntPtr p_ptrDestination, Rectangle p_rctDestinationRect)"/>
		public static void CopyTexture(IntPtr p_ptrSource, Rectangle p_rctSourceRect, IntPtr p_ptrDestination, Rectangle p_rctDestinationRect)
		{
			ExecuteMethod(() => m_mdiScript.TextureManager.CopyTexture(p_ptrSource, p_rctSourceRect, p_ptrDestination, p_rctDestinationRect));
		}

		/// <summary>
		/// Gets the dimensions of the specified texture.
		/// </summary>
		/// <param name="p_ptrTexture">A pointer to the texture whose dimensions are to be determined.</param>
		/// <param name="p_intWidth">The out parameter that will contain the width of the texture.</param>
		/// <param name="p_intHeight">The out parameter that will contain the height of the texture.</param>
		/// <seealso cref="TextureManager.GetTextureSize(IntPtr p_ptrTexture, out int p_intWidth, out int p_intHeight)"/>
		public static void GetTextureSize(IntPtr p_ptrTexture, out int p_intWidth, out int p_intHeight)
		{
			try
			{
				m_mdiScript.TextureManager.GetTextureSize(p_ptrTexture, out p_intWidth, out p_intHeight);
			}
			catch (Exception e)
			{
				m_strLastError = e.Message;
				p_intWidth = -1;
				p_intHeight = -1;
			}
		}

		/// <summary>
		/// Retrieves the texture data for the specified texture.
		/// </summary>
		/// <param name="p_ptrTexture">A pointer to the texture whose data is to be retrieved.</param>
		/// <param name="p_intPitch">The out parameter that will contain the texture's pitch.</param>
		/// <returns>The texture data.</returns>
		/// <seealso cref="TextureManager.GetTextureData(IntPtr p_ptrTexture, out int p_intPitch)"/>
		public static byte[] GetTextureData(IntPtr p_ptrTexture, out int p_intPitch)
		{
			try
			{
				return m_mdiScript.TextureManager.GetTextureData(p_ptrTexture, out p_intPitch);
			}
			catch (Exception e)
			{
				m_strLastError = e.Message;
				p_intPitch = -1;
			}
			return null;
		}

		/// <summary>
		/// Sets the data for the specified texture.
		/// </summary>
		/// <param name="p_ptrTexture">A pointer to the texture whose data is to be set.</param>
		/// <param name="p_bteData">The data to which to set the texture.</param>
		/// <seealso cref="TextureManager.SetTextureData(IntPtr p_ptrTexture, byte[] p_bteData)"/>
		public static void SetTextureData(IntPtr p_ptrTexture, byte[] p_bteData)
		{
			ExecuteMethod(() => m_mdiScript.TextureManager.SetTextureData(p_ptrTexture, p_bteData));
		}

		/// <summary>
		/// Releases the specified texture.
		/// </summary>
		/// <param name="p_ptrTexture">A pointer to the texture to release.</param>
		/// <seealso cref="TextureManager.ReleaseTexture(IntPtr p_ptrTexture)"/>
		public static void ReleaseTexture(IntPtr p_ptrTexture)
		{
			ExecuteMethod(() => m_mdiScript.TextureManager.ReleaseTexture(p_ptrTexture));
		}

		#endregion
	}
}
