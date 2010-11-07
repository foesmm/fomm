using System;
using System.IO;
using System.Drawing;
using Fomm.Games.FalloutNewVegas.Settings;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using Fomm.Controls;
using Fomm.PackageManager;
using Fomm.PackageManager.XmlConfiguredInstall;
using Fomm.PackageManager.XmlConfiguredInstall.Parsers;
using Fomm.Games.FalloutNewVegas.Script.XmlConfiguredInstall.Parsers;
using Fomm.Games.FalloutNewVegas.Script;

namespace Fomm.Games.FalloutNewVegas
{
	/// <summary>
	/// Provides information required for the programme to manage Fallout: New Vegas plugins.
	/// </summary>
	public class FalloutNewVegasGameMode : GameMode
	{
		public static class SettingsFile
		{
			public static readonly string FOIniPath = "FOIniPath";
			public static readonly string FOPrefsIniPath = "FOPrefsIniPath";
			public static readonly string GeckIniPath = "GeckIniPath";
			public static readonly string GeckPrefsIniPath = "GeckPrefsIniPath";
		}

		private string m_strSavesPath = null;
		private Dictionary<string, string> m_dicAdditionalPaths = new Dictionary<string, string>();
		private Dictionary<string, string> m_dicSettingsFiles = new Dictionary<string, string>();
		private List<GameTool> m_lstTools = new List<GameTool>();
		private List<GameTool> m_lstGameSettingsTools = new List<GameTool>();
		private List<GameTool> m_lstRightClickTools = new List<GameTool>();
		private List<GameTool> m_lstLoadOrderTools = new List<GameTool>();
		private List<GameTool> m_lstGameLaunchCommands = new List<GameTool>();
		private List<SettingsPage> m_lstSettingsPages = new List<SettingsPage>();
		private Fomm.Games.Fallout3.Fallout3PluginManager m_pmgPluginManager = new Fomm.Games.Fallout3.Fallout3PluginManager();

		#region Properties

		/// <summary>
		/// Gets the modDirectory of the GameMode.
		/// </summary>
		/// <value>The modDirectory of the GameMode.</value>
		public override string ModDirectory
		{
			get
			{
				string strModDirectory = Properties.Settings.Default.falloutNewVegasModDirectory;
				if (String.IsNullOrEmpty(strModDirectory))
					throw new Exception("The Mod Directory for Fallout: New Vegas Mods has not been set.");
				if (!Directory.Exists(strModDirectory))
					Directory.CreateDirectory(strModDirectory);
				return strModDirectory;
			}
		}

		/// <summary>
		/// Gets the modInfoCacheDirectory of the GameMode.
		/// </summary>
		/// <value>The modInfoCacheDirectory of the GameMode.</value>
		public override string ModInfoCacheDirectory
		{
			get
			{
				string strCache = Path.Combine(ModDirectory, "cache");
				if (!Directory.Exists(strCache))
					Directory.CreateDirectory(strCache);
				return strCache;
			}
		}

		/// <summary>
		/// Gets the game launch command.
		/// </summary>
		/// <value>The game launch command.</value>
		public override GameTool LaunchCommand
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the icon used for the plugin file type.
		/// </summary>
		/// <value>The icon used for the plugin file type.</value>
		public override Icon PluginFileIcon
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the path to the game directory were pluings are to be installed.
		/// </summary>
		/// <value>The path to the game directory were pluings are to be installed.</value>
		public override string PluginsPath
		{
			get
			{
				throw new NotImplementedException();
				//we may not need to overrid this property
				return Path.Combine(Environment.CurrentDirectory, "Data");
			}
		}

		/// <summary>
		/// Gets the path to the plugins.txt file.
		/// </summary>
		/// <remarks>
		/// plugins.txt is a Fallout 3 file that tracks active plugins.
		/// </remarks>
		/// <value>The path to the plugins.txt file.</value>
		public string PluginsFilePath
		{
			get
			{
				return m_dicAdditionalPaths["PluginsFile"];
			}
		}

		/// <summary>
		/// Gets the directory where installation information is stored for this game mode.
		/// </summary>
		/// <remarks>
		/// This is where install logs, overwrites, and the like are stored.
		/// </remarks>
		/// <value>The directory where installation information is stored for this game mode.</value>
		public override string InstallInfoDirectory
		{
			get
			{
				string strDirectory = Properties.Settings.Default.falloutNewVegasInstallInfoDirectory;
				if (String.IsNullOrEmpty(strDirectory))
					throw new Exception("The InstallInfoDirectory for Fallout: New Vegas has not been set.");
				if (!Directory.Exists(strDirectory))
					Directory.CreateDirectory(strDirectory);
				return strDirectory;
			}
		}

		/// <summary>
		/// Gets the settings files used in the game mode.
		/// </summary>
		/// <value>The settings files used in the game mode.</value>
		public override IDictionary<string, string> SettingsFiles
		{
			get
			{
				return m_dicSettingsFiles;
			}
		}

		/// <summary>
		/// Gets any other paths used in the game mode.
		/// </summary>
		/// <value>Any other paths used in the game mode.</value>
		public override IDictionary<string, string> AdditionalPaths
		{
			get
			{
				return m_dicAdditionalPaths;
			}
		}

		/// <summary>
		/// Gets the path to the game's save game files.
		/// </summary>
		/// <value>The path to the game's save game files.</value>
		public override string SavesPath
		{
			get
			{
				return m_strSavesPath;
			}
		}

		#region Tool Injection

		/// <summary>
		/// Gets the list of tools to add to the tools menu.
		/// </summary>
		/// <value>The list of tools to add to the tools menu.</value>
		public override IList<GameTool> Tools
		{
			get
			{
				return m_lstTools;
			}
		}

		/// <summary>
		/// Gets the list of tools to add to the game settings menu.
		/// </summary>
		/// <value>The list of tools to add to the game settings menu.</value>
		public override IList<GameTool> GameSettingsTools
		{
			get
			{
				return m_lstGameSettingsTools;
			}
		}

		/// <summary>
		/// Gets the list of tools to add to the right-click menu.
		/// </summary>
		/// <value>The list of tools to add to the right-click menu.</value>
		public override IList<GameTool> RightClickTools
		{
			get
			{
				return m_lstRightClickTools;
			}
		}

		/// <summary>
		/// Gets the list of tools to add to the load order menu.
		/// </summary>
		/// <value>The list of tools to add to the load order menu.</value>
		public override IList<GameTool> LoadOrderTools
		{
			get
			{
				return m_lstLoadOrderTools;
			}
		}

		/// <summary>
		/// Gets the list of game launch commands.
		/// </summary>
		/// <value>The list of game launch commands.</value>
		public override IList<GameTool> GameLaunchCommands
		{
			get
			{
				return m_lstGameLaunchCommands;
			}
		}

		#endregion

		/// <summary>
		/// Gets the settings pages that privode management of game mode-specific settings.
		/// </summary>
		/// <value>The settings pages that privode management of game mode-specific settings.</value>
		public override IList<SettingsPage> SettingsPages
		{
			get
			{
				return m_lstSettingsPages;
			}
		}

		/// <summary>
		/// Gets the plugin manager for this game mode.
		/// </summary>
		/// <value>The plugin manager for this game mode.</value>
		public override PluginManager PluginManager
		{
			get
			{
				return m_pmgPluginManager;
			}
		}

		/// <summary>
		/// Gets the version of the installed game.
		/// </summary>
		/// <value>The version of the installed game.</value>
		public override Version GameVersion
		{
			get
			{
				throw new NotImplementedException();
				if (File.Exists("Fallout3.exe"))
					return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo("Fallout3.exe").FileVersion.Replace(", ", "."));
				if (File.Exists("Fallout3ng.exe"))
					return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo("Fallout3ng.exe").FileVersion.Replace(", ", "."));
				return null;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public FalloutNewVegasGameMode()
		{
			SetupPaths();
			SetupSettingsPages();
			SetupTools();
			SetupLaunchCommands();
		}

		#endregion

		#region Initialization

		/// <summary>
		/// Sets up the paths for this game mode.
		/// </summary>
		protected virtual void SetupPaths()
		{
			throw new NotImplementedException();
			/*
			string strUserGameDataPath = Path.Combine(Program.PersonalDirectory, "My games\\Fallout3");

			m_dicSettingsFiles[SettingsFile.FOIniPath] = Path.Combine(strUserGameDataPath, "Fallout.ini");
			m_dicSettingsFiles[SettingsFile.FOPrefsIniPath] = Path.Combine(strUserGameDataPath, "FalloutPrefs.ini");
			m_dicSettingsFiles[SettingsFile.GeckIniPath] = Path.Combine(strUserGameDataPath, "GECKCustom.ini");
			m_dicSettingsFiles[SettingsFile.GeckPrefsIniPath] = Path.Combine(strUserGameDataPath, "GECKPrefs.ini");

			m_dicAdditionalPaths["FORendererFile"] = Path.Combine(strUserGameDataPath, "RendererInfo.txt");
			m_dicAdditionalPaths["PluginsFile"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fallout3/plugins.txt");
			m_dicAdditionalPaths["DLCDir"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\xlive\\DLC");

			m_strSavesPath = Path.Combine(strUserGameDataPath, NativeMethods.GetPrivateProfileString("General", "SLocalSavePath", "Games", m_dicSettingsFiles[SettingsFile.FOIniPath]));
			*/
		}

		/// <summary>
		/// Gets up the game-specific settings pages.
		/// </summary>
		protected virtual void SetupSettingsPages()
		{
			SettingsPages.Add(new GeneralSettingsPage());
		}

		/// <summary>
		/// Sets up the launch commands for the game.
		/// </summary>
		protected virtual void SetupLaunchCommands()
		{
			throw new NotImplementedException();
			/*
			m_lstGameLaunchCommands.Add(new GameTool("Launch Fallout 3", "Launches plain Fallout 3.", LaunchFallout3Plain));
			m_lstGameLaunchCommands.Add(new GameTool("Launch FOSE", "Launches Fallout 3 with FOSE.", LaunchFallout3FOSE));
			m_lstGameLaunchCommands.Add(new GameTool("Launch Custom Fallout 3", "Launches Fallout 3 with custom command.", LaunchFallout3Custom));
			 */
		}

		/// <summary>
		/// Sets up the tools for this game mode.
		/// </summary>
		protected virtual void SetupTools()
		{
			throw new NotImplementedException();
			/*
			m_lstTools.Add(new GameTool("BSA Tool", "Creates and unpacks BSA files.", LaunchBSATool));
			m_lstTools.Add(new GameTool("TESsnip", "An ESP/ESM editor.", LaunchTESsnipTool));
			m_lstTools.Add(new GameTool("Shader Editor", "A shader (SDP) editor.", LaunchShaderEditTool));
			m_lstTools.Add(new GameTool("CREditor", "Edits critical records in an ESP/ESM.", LaunchCREditorTool));
			m_lstTools.Add(new GameTool("Archive Invalidation", "Toggles Archive Invalidation.", ToggleArchiveInvalidation));
			m_lstTools.Add(new GameTool("Install Tweaker", "Advanced Fallout 3 tweaking.", LaunchInstallTweakerTool));
			m_lstTools.Add(new GameTool("Conflict Detector", "Checks for conflicts with mod-author specified critical records.", LaunchConflictDetector));
			m_lstTools.Add(new GameTool("Save Games", "Save game info viewer.", LaunchSaveGamesViewer));

			m_lstGameSettingsTools.Add(new GameTool("Graphics Settings", "Changes the graphics settings.", LaunchGraphicsSettingsTool));

			m_lstRightClickTools.Add(new GameTool("Open in TESsnip...", "Open the selected plugins in TESsnip.", LaunchTESsnipToolWithSelectedPlugins));
			m_lstRightClickTools.Add(new GameTool("Open in CREditor...", "Open the selected plugins in TESsnip.", LaunchCREditorToolWithSelectedPlugins));

			m_lstLoadOrderTools.Add(new GameTool("Load Order Report...", "Generates a report on the current load order, as compared to the BOSS recomendation.", LaunchLoadOrderReport));
			m_lstLoadOrderTools.Add(new GameTool("BOSS Auto Sort", "Auto-sorts the plugins using BOSS's masterlist.", LaunchSortPlugins));
			 */
		}

		#endregion

		#region Scripts

		/// <summary>
		/// Gets the default script for a mod.
		/// </summary>
		/// <value>The default script for a mod.</value>
		public override string DefaultCSharpScript
		{
			get
			{
				return @"using System;
using fomm.Scripting;

class Script : FalloutNewVegasBaseScript {
	public static bool OnActivate() {
        //Install all files from the fomod and activate any esps
        PerformBasicInstall();
		return true;
	}
}
";
			}
		}

		/// <summary>
		/// Creates a mod install script for the given <see cref="fomod"/>.
		/// </summary>
		/// <param name="p_fomodMod">The mod for which to create an installer script.</param>
		/// <param name="p_mibInstaller">The installer for which the script is being created.</param>
		/// <returns>A mod install script for the given <see cref="fomod"/>.</returns>
		public override ModInstallScript CreateInstallScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller)
		{
			return new FalloutNewVegasModInstallScript(p_fomodMod, p_mibInstaller);
		}

		/// <summary>
		/// Creates a mod upgrade script for the given <see cref="fomod"/>.
		/// </summary>
		/// <param name="p_fomodMod">The mod for which to create an installer script.</param>
		/// <param name="p_mibInstaller">The installer for which the script is being created.</param>
		/// <returns>A mod upgrade script for the given <see cref="fomod"/>.</returns>
		public override ModInstallScript CreateUpgradeScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller)
		{
			return new FalloutNewVegasModUpgradeScript(p_fomodMod, p_mibInstaller);
		}

		/// <summary>
		/// Creates a <see cref="DependencyStateManager"/> for the given <see cref="ModInstallScript"/>.
		/// </summary>
		/// <param name="p_misInstallScript">The <see cref="ModInstallScript"/> for which the
		/// <see cref="DependencyStateManager"/> is being created.</param>
		/// <returns>A <see cref="DependencyStateManager"/> for the given <see cref="ModInstallScript"/>.</returns>
		public override DependencyStateManager CreateDependencyStateManager(ModInstallScript p_misInstallScript)
		{
			return new Fomm.Games.Fallout3.Script.XmlConfiguredInstall.Fallout3DependencyStateManager(p_misInstallScript);
		}

		/// <summary>
		/// The factory method that creates the appropriate parser extension for the specified configuration file version.
		/// </summary>
		/// <param name="p_strVersion">The XML configuration file version for which to return a parser extension.</param>
		/// <returns>The appropriate parser extension for the specified configuration file version, or
		/// <lang cref="null"/> if no extension is available.</returns>
		public override ParserExtension CreateParserExtension(string p_strVersion)
		{
			switch (p_strVersion)
			{
				case "5.0":
					return new FalloutNewVegasParser50Extension();
				default:
					return null;
			}
		}

		/// <summary>
		/// Gets the path to the schema file for the specified configuration file version.
		/// </summary>
		/// <param name="p_strVersion">The XML configuration file version for which to return a parser extension.</param>
		/// <returns>The path to the schema file for the specified configuration file version, or
		/// <lang cref="null"/> if there is no game-specific schema for the specified configuration
		/// file version.</returns>
		public override string GetGameSpecificXMLConfigSchemaPath(string p_strVersion)
		{
			return Path.Combine(Program.ProgrammeInfoDirectory, String.Format(@"FalloutNV\ModConfig{0}.xsd", p_strVersion));
		}

		#endregion

		#region Command Line Arguments

		/// <summary>
		/// Return command line help for the arguments provided by the game mode.
		/// </summary>
		/// <remarks>
		/// This method should only return the text required to describe the arguments. All header,
		/// footer, and context text is already provided.
		/// </remarks>
		/// <returns>Command line help for the arguments provided by the game mode.</returns>
		public static string GetCommandLineHelp()
		{
			StringBuilder stbHelp = new StringBuilder();
			stbHelp.AppendLine("*.dat, *.bsa, *.esm, *.esp, *.sdp");
			stbHelp.AppendLine("Open the specified file in the relevent utility");
			stbHelp.AppendLine();
			stbHelp.AppendLine("-setup, -bsa-unpacker, -bsa-creator, -tessnip, -sdp-editor");
			stbHelp.AppendLine("Open the specified utility window, without opening the main form where appropriate");
			return stbHelp.ToString();
		}

		/// <summary>
		/// Handles the command line arguments that run outside of an instance of FOMM.
		/// </summary>
		/// <param name="p_strArgs">The command line arguments that were passed to the programme.</param>
		/// <returns><lang cref="true"/> if at least one of the arguments were handled;
		/// <lang cref="false"/> otherwise.</returns>
		public override bool HandleStandaloneArguments(string[] p_strArgs)
		{
			if (!p_strArgs[0].StartsWith("-") && File.Exists(p_strArgs[0]))
			{
				switch (Path.GetExtension(p_strArgs[0]).ToLowerInvariant())
				{
					case ".dat":
					case ".bsa":
						Application.Run(new Fallout3.Tools.BSA.BSABrowser(p_strArgs[0]));
						return true;
					case ".sdp":
						Application.Run(new Fallout3.Tools.ShaderEdit.MainForm(p_strArgs[0]));
						return true;
					case ".esp":
					case ".esm":
						Application.Run(new Fallout3.Tools.TESsnip.TESsnip(new string[] { p_strArgs[0] }));
						return true;
				}
			}
			else
			{
				switch (p_strArgs[0])
				{
					case "-setup":
						bool booNewMutex = false;
						Mutex mutex = new Mutex(true, "fommMainMutex", out booNewMutex);
						if (!booNewMutex)
						{
							MessageBox.Show("fomm is already running", "Error");
							mutex.Close();
							return true;
						}
						Application.Run(new SettingsForm());
						mutex.Close();
						return true;
					case "-bsa-unpacker":
						Application.Run(new Fallout3.Tools.BSA.BSABrowser());
						return true;
					case "-bsa-creator":
						Application.Run(new Fallout3.Tools.BSA.BSACreator());
						return true;
					case "-tessnip":
						Application.Run(new Fallout3.Tools.TESsnip.TESsnip());
						return true;
					case "-sdp-editor":
						Application.Run(new Fallout3.Tools.ShaderEdit.MainForm());
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Handles the command line arguments that affect an instance of the mod manager.
		/// </summary>
		/// <param name="p_strArgs">The command line arguments that were passed to the programme.</param>
		/// <returns><lang cref="true"/> if at least one of the arguments were handled;
		/// <lang cref="false"/> otherwise.</returns>
		public override bool HandleInAppArguments(string[] p_strArgs)
		{
			return true;
		}

		#endregion

		/// <summary>
		/// Verifies that the given path is a valid working directory for the game mode.
		/// </summary>
		/// <param name="p_strPath">The path to validate as a working directory.</param>
		/// <returns><lang cref="true"/> if the path is a vlid working directory;
		/// <lang cref="false"/> otherwise.</returns>
		public static bool VerifyWorkingDirectory(string p_strPath)
		{
			throw new NotImplementedException();
			/*if (String.IsNullOrEmpty(p_strPath))
				return false;

			string[] strExes = new string[] { Path.Combine(p_strPath, "fallout3.exe"),
												Path.Combine(p_strPath, "fallout3ng.exe") };
			bool booFound = false;
			foreach (string strExe in strExes)
				if (File.Exists(strExe))
				{
					booFound = true;
					break;
				}
			return booFound;*/
		}

		/// <summary>
		/// Sets the working directory for the programme.
		/// </summary>
		/// <remarks>
		/// This sets the working directory to the Fallout 3 install folder.
		/// </remarks>
		/// <param name="p_strErrorMessage">The out parameter that is set to the error message, if an error occurred.</param>
		/// <returns><lang cref="true"/> if the working directory was successfully set;
		/// <lang cref="false"/> otherwise.</returns>
		public override bool SetWorkingDirectory(out string p_strErrorMessage)
		{
			throw new NotImplementedException();
#if TRACE
			Trace.WriteLine("Looking for Fallout New Vegas.");
			Trace.Indent();
#endif
			/*string strWorkingDirectory = Properties.Settings.Default.fallout3WorkingDirectory;

			if (String.IsNullOrEmpty(strWorkingDirectory) || !Directory.Exists(strWorkingDirectory))
			{
				try
				{
					strWorkingDirectory = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Bethesda Softworks\Fallout3", "Installed Path", null) as string;
				}
				catch
				{
					strWorkingDirectory = null;
				}
			}

			using (WorkingDirectorySelectionForm wdfForm = new WorkingDirectorySelectionForm(
					"Could not find Fallout 3 directory." + Environment.NewLine +
					"Fallout's registry entry appears to be missing or incorrect." + Environment.NewLine +
					"Please enter the path to your Fallout 3 game file, or click \"Auto Detect\" to search" +
					" for the install directory. Note that Auto Detection can take several minutes.",
					"Fallout 3 Game Directory:",
					new string[] { "fallout3.exe", "fallout3ng.exe" }))
			{
				while (!VerifyWorkingDirectory(strWorkingDirectory))
				{
					if (wdfForm.ShowDialog() == DialogResult.Cancel)
					{
						p_strErrorMessage = "Could not find Fallout 3 directory.";
						return false;
					}
					strWorkingDirectory = wdfForm.WorkingDirectory;
				}
			}
			Directory.SetCurrentDirectory(strWorkingDirectory);
			Properties.Settings.Default.fallout3WorkingDirectory = strWorkingDirectory;
			Properties.Settings.Default.Save();
#if TRACE
				Trace.WriteLine("Found: " + Path.GetFullPath("."));
#endif
			p_strErrorMessage = null;
			return true;*/
		}

		/// <summary>
		/// This initializes the game mode.
		/// </summary>
		/// <remarks>
		/// This gets the user to specify the directories where the programme will store info
		/// such as install logs, if the directories have not already been setup.
		/// 
		/// This method also checks for DLCs, and cleans up any missing FOMods.
		/// </remarks>
		/// <returns><lang cref="true"/> if the game mode was able to initialize;
		/// <lang cref="false"/> otherwise.</returns>
		public override bool Init()
		{
			if (!Properties.Settings.Default.falloutNewVegasDoneSetup)
			{
				SetupForm sfmSetup = new SetupForm();
				if (sfmSetup.ShowDialog() == DialogResult.Cancel)
					return false;
				Properties.Settings.Default.falloutNewVegasDoneSetup = true;
				Properties.Settings.Default.Save();
			}

			ScanForReadonlyPlugins();

			return true;
		}

		/// <summary>
		/// This chaecks for any FOMods that have been manually deleted since the programme last ran.
		/// </summary>
		protected void ScanForReadonlyPlugins()
		{
			DirectoryInfo difPluginsDirectory = new DirectoryInfo(Program.GameMode.PluginsPath);
			List<FileInfo> lstPlugins = new List<FileInfo>(Program.GetFiles(difPluginsDirectory, "*.esp"));
			lstPlugins.AddRange(Program.GetFiles(difPluginsDirectory, "*.esm"));

			for (Int32 i = 0; i < lstPlugins.Count; i++)
			{
				FileInfo fifPlugin = lstPlugins[i];
				if ((fifPlugin.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					if (MessageBox.Show(null, String.Format("'{0}' is read-only, so its load order cannot be changed. Would you like to make it not read-only?", fifPlugin.Name), "Read Only", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						fifPlugin.Attributes &= ~FileAttributes.ReadOnly;
				}
			}
		}

		/// <summary>
		/// Determines if the specified file is a plugin for the game mode.
		/// </summary>
		/// <param name="p_strPath">The path to the file for which it is to be determined if it is a plugin file.</param>
		/// <returns><lang cref="true"/> if the specified file is a plugin file in the game mode;
		/// <lang cref="false"/> otherwise.</returns>
		public override bool IsPluginFile(string p_strPath)
		{
			string strExt = Path.GetExtension(p_strPath).ToLowerInvariant();
			return (strExt == ".esp" || strExt == ".esm");
		}

		/// <summary>
		/// hecks for any updates that are available for any game-specific components.
		/// </summary>
		/// <remarks><lang cref="true"/> if updates were available; otherwise <lang cref="false"/>.</remarks>
		public override bool CheckForUpdates()
		{
			//check for new load order tepmlate
			Tools.AutoSorter.FalloutNewVegasBOSSUpdater bupUpdater = new Tools.AutoSorter.FalloutNewVegasBOSSUpdater();
			Int32 intLOVersion = bupUpdater.GetMasterlistVersion();
			if (intLOVersion > new Fallout3.Tools.AutoSorter.LoadOrderSorter().GetFileVersion())
			{
				if (MessageBox.Show("A new version of the load order template is available: Release " + intLOVersion +
					"\nDo you wish to download?", "Message", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					bupUpdater.UpdateMasterlist(Fomm.Games.Fallout3.Tools.AutoSorter.LoadOrderSorter.LoadOrderTemplatePath);
					MessageBox.Show("The load order template was updated.", "Update Complete.", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				return true;
			}
			return false;
		}
	}
}
