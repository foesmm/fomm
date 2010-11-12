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
using Fomm.Games.Fallout3;
using WebsiteAPIs;
#if TRACE
using System.Diagnostics;
#endif

namespace Fomm.Games.FalloutNewVegas
{
	/// <summary>
	/// Provides information required for the programme to manage Fallout: New Vegas plugins.
	/// </summary>
	public class FalloutNewVegasGameMode : Fallout3GameMode
	{
		/// <summary>
		/// This class provides strongly-typed access to this game mode's settings files.
		/// </summary>
		public new class SettingsFilesSet : Fallout3GameMode.SettingsFilesSet
		{
			private const string FODefaultIniPathKey = "FODefaultIniPath";

			#region Properties

			/// <summary>
			/// Gets or sets the path to the fallout_default.ini file.
			/// </summary>
			/// <value>The path to the fallout_default.ini file.</value>
			public string FODefaultIniPath
			{
				get
				{
					return this[FODefaultIniPathKey];
				}
				set
				{
					this[FODefaultIniPathKey] = value;
				}
			}

			#endregion
		}

		#region Properties

		/// <summary>
		/// Gets the nexus site for this game.
		/// </summary>
		/// <value>The nexus site for this game.</value>
		/// <seealso cref="HasNexusSite"/>
		public override NexusSite NexusSite
		{
			get
			{
				return NexusSite.FalloutNV;
			}
		}

		/// <summary>
		/// Gets the name of the game whose plugins are being managed.
		/// </summary>
		/// <value>The name of the game whose plugins are being managed.</value>
		public override string GameName
		{
			get
			{
				return "Fallout: New Vegas";
			}
		}

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
		/// Gets the game launch command.
		/// </summary>
		/// <value>The game launch command.</value>
		public override GameTool LaunchCommand
		{
			get
			{
				if (String.IsNullOrEmpty(Properties.Settings.Default.falloutNewVegasLaunchCommand) && File.Exists("nvse_loader.exe"))
					return new GameTool("Launch NVSE", "Launches Fallout: New Vegas using FOSE.", LaunchGame);
				return new GameTool("Launch Fallout: NV", "Launches Fallout: New Vegas using FOSE.", LaunchGame);
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
				string strFalloutEsm = Path.Combine(PluginsPath, "falloutnv.esm");
				return System.Drawing.Icon.ExtractAssociatedIcon(strFalloutEsm);
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
		/// Gets the version of the installed game.
		/// </summary>
		/// <value>The version of the installed game.</value>
		public override Version GameVersion
		{
			get
			{
				if (File.Exists("FalloutNV.exe"))
					return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo("FalloutNV.exe").FileVersion.Replace(", ", "."));
				if (File.Exists("FalloutNVng.exe"))
					return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo("FalloutNVng.exe").FileVersion.Replace(", ", "."));
				return null;
			}
		}

		/// <summary>
		/// Gets the path to the per user Fallout: New Vegas data.
		/// </summary>
		/// <value>The path to the per user Fallout: New Vegas data.</value>
		protected override string UserGameDataPath
		{
			get
			{
				return Path.Combine(Program.PersonalDirectory, "My games\\FalloutNV");
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public FalloutNewVegasGameMode()
		{
		}

		#endregion

		#region Initialization

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

			((SettingsFilesSet)SettingsFiles).FODefaultIniPath = Path.Combine(PluginsPath, @"..\fallout_default.ini");

			if (!File.Exists(((SettingsFilesSet)SettingsFiles).FOIniPath))
				MessageBox.Show("You have no Fallout INI file. Please run Fallout: New Vegas to initialize the file before installing any mods or turning on Archive Invalidation.", "Missing INI", MessageBoxButtons.OK, MessageBoxIcon.Information);

			ScanForReadonlyPlugins();
			ScanForReadonlySettingsFiles();

			return true;
		}

		/// <summary>
		/// Creates the plugin manager that will be used by this game mode.
		/// </summary>
		/// <returns>The plugin manager that will be used by this game mode.</returns>
		protected override Fallout3PluginManager CreatePluginManager()
		{
			return new FalloutNewVegasPluginManager();
		}

		/// <summary>
		/// Sets up the paths for this game mode.
		/// </summary>
		protected override void SetupPaths()
		{
			base.SetupPaths();
			AdditionalPaths["PluginsFile"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FalloutNV/plugins.txt");
			AdditionalPaths.Remove("DLCDir");
		}

		/// <summary>
		/// Gets up the game-specific settings pages.
		/// </summary>
		protected override void SetupSettingsPages()
		{
			SettingsPages.Add(new GeneralSettingsPage());
		}

		/// <summary>
		/// Sets up the launch commands for the game.
		/// </summary>
		protected override void SetupLaunchCommands()
		{
			GameLaunchCommands.Add(new GameTool("Launch Fallout: New Vegas", "Launches plain Fallout: New Vegas.", LaunchFalloutNVPlain));
			GameLaunchCommands.Add(new GameTool("Launch NVSE", "Launches Fallout: New Vegas with NVSE.", LaunchFalloutNVNVSE));
			GameLaunchCommands.Add(new GameTool("Launch Custom Fallout: New Vegas", "Launches Fallout: New Vegas with custom command.", LaunchFalloutNVCustom));
		}

		/// <summary>
		/// Sets up the tools for this game mode.
		/// </summary>
		protected override void SetupTools()
		{
			Tools.Add(new GameTool("BSA Tool", "Creates and unpacks BSA files.", LaunchBSATool));
			Tools.Add(new GameTool("TESsnip", "An ESP/ESM editor.", LaunchTESsnipTool));
			Tools.Add(new GameTool("Shader Editor", "A shader (SDP) editor.", LaunchShaderEditTool));
			Tools.Add(new GameTool("CREditor", "Edits critical records in an ESP/ESM.", LaunchCREditorTool));
			Tools.Add(new GameTool("Archive Invalidation", "Toggles Archive Invalidation.", ToggleArchiveInvalidation));
			//m_lstTools.Add(new GameTool("Install Tweaker", "Advanced Fallout 3 tweaking.", LaunchInstallTweakerTool));
			Tools.Add(new GameTool("Conflict Detector", "Checks for conflicts with mod-author specified critical records.", LaunchConflictDetector));
			Tools.Add(new GameTool("Save Games", "Save game info viewer.", LaunchSaveGamesViewer));

			GameSettingsTools.Add(new GameTool("Graphics Settings", "Changes the graphics settings.", LaunchGraphicsSettingsTool));

			RightClickTools.Add(new GameTool("Open in TESsnip...", "Open the selected plugins in TESsnip.", LaunchTESsnipToolWithSelectedPlugins));
			RightClickTools.Add(new GameTool("Open in CREditor...", "Open the selected plugins in TESsnip.", LaunchCREditorToolWithSelectedPlugins));

			LoadOrderTools.Add(new GameTool("Load Order Report...", "Generates a report on the current load order, as compared to the BOSS recomendation.", LaunchLoadOrderReport));
			LoadOrderTools.Add(new GameTool("BOSS Auto Sort", "Auto-sorts the plugins using BOSS's masterlist.", LaunchSortPlugins));
		}

		#endregion

		#region Tool Launch Methods

		#region Game Launch

		/// <summary>
		/// Launches the game with a custom command.
		/// </summary>
		/// <param name="p_frmMainForm">The main mod management form.</param>
		public void LaunchFalloutNVCustom(MainForm p_frmMainForm)
		{
			if (p_frmMainForm.HasOpenUtilityWindows)
			{
				MessageBox.Show("Please close all utility windows before launching Fallout");
				return;
			}
			string command = Properties.Settings.Default.falloutNewVegasLaunchCommand;
			string args = Properties.Settings.Default.falloutNewVegasLaunchCommandArgs;
			if (String.IsNullOrEmpty(command))
			{
				MessageBox.Show("No custom launch command has been set", "Error");
				return;
			}
			try
			{
				System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
				psi.Arguments = args;
				psi.FileName = command;
				psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(command));
				if (System.Diagnostics.Process.Start(psi) == null)
				{
					MessageBox.Show("Failed to launch '" + command + "'");
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to launch '" + command + "'\n" + ex.Message);
				return;
			}
			p_frmMainForm.Close();
		}

		/// <summary>
		/// Launches the game, with NVSE.
		/// </summary>
		/// <param name="p_frmMainForm">The main mod management form.</param>
		public void LaunchFalloutNVNVSE(MainForm p_frmMainForm)
		{
			if (!File.Exists("nvse_loader.exe"))
			{
				MessageBox.Show("NVSE does not appear to be installed");
				return;
			}
			if (p_frmMainForm.HasOpenUtilityWindows)
			{
				MessageBox.Show("Please close all utility windows before launching Fallout");
				return;
			}
			try
			{
				System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
				psi.FileName = "nvse_loader.exe";
				psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath("nvse_loader.exe"));
				if (System.Diagnostics.Process.Start(psi) == null)
				{
					MessageBox.Show("Failed to launch 'nvse_loader.exe'");
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to launch 'nvse_loader.exe'\n" + ex.Message);
				return;
			}
			p_frmMainForm.Close();
		}

		/// <summary>
		/// Launches the game, without NVSE.
		/// </summary>
		/// <param name="p_frmMainForm">The main mod management form.</param>
		public void LaunchFalloutNVPlain(MainForm p_frmMainForm)
		{
			if (p_frmMainForm.HasOpenUtilityWindows)
			{
				MessageBox.Show("Please close all utility windows before launching fallout");
				return;
			}
			string command;
			if (File.Exists("falloutNV.exe"))
				command = "falloutNV.exe";
			else
				command = "falloutNVng.exe";
			try
			{
				System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
				psi.FileName = command;
				psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(command));
				if (System.Diagnostics.Process.Start(psi) == null)
				{
					MessageBox.Show("Failed to launch '" + command + "'");
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to launch '" + command + "'\n" + ex.Message);
				return;
			}
			p_frmMainForm.Close();
		}

		/// <summary>
		/// Launches the game, using NVSE if present.
		/// </summary>
		/// <param name="p_frmMainForm">The main mod management form.</param>
		public override void LaunchGame(MainForm p_frmMainForm)
		{
			string command = Properties.Settings.Default.falloutNewVegasLaunchCommand;
			string args = Properties.Settings.Default.falloutNewVegasLaunchCommandArgs;
			if (String.IsNullOrEmpty(command))
			{
				if (File.Exists("nvse_loader.exe"))
					command = "nvse_loader.exe";
				else if (File.Exists("falloutNV.exe"))
					command = "falloutNV.exe";
				else
					command = "falloutNVng.exe";
				args = null;
			}
			try
			{
				System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
				psi.Arguments = args;
				psi.FileName = command;
				psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(command));
				if (System.Diagnostics.Process.Start(psi) == null)
				{
					MessageBox.Show("Failed to launch '" + command + "'");
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to launch '" + command + "'\n" + ex.Message);
				return;
			}
		}

		#endregion

		#region Tools Menu

		/// <summary>
		/// Launches the save games viewer.
		/// </summary>
		/// <param name="p_frmMainForm">The main mod management form.</param>
		public override void LaunchSaveGamesViewer(MainForm p_frmMainForm)
		{
			List<string> lstActive = new List<string>();
			//the original implementation populated the inactive list with all plugins
			// we only populate it with inactive plugins - hopefully that's OK
			List<string> lstInactive = new List<string>();

			foreach (ListViewItem lviPlugin in p_frmMainForm.PluginsListViewItems)
			{
				if (lviPlugin.Checked)
					lstActive.Add(lviPlugin.Text);
				else
					lstInactive.Add(lviPlugin.Text);
			}
			(new Tools.SaveForm(lstActive.ToArray(), lstInactive.ToArray())).Show();
		}

		/// <summary>
		/// Toggles archive invalidation.
		/// </summary>
		/// <param name="p_frmMainForm">The main mod management form.</param>
		public override void ToggleArchiveInvalidation(MainForm p_frmMainForm)
		{
			Fomm.Games.FalloutNewVegas.Tools.ArchiveInvalidation.Update();
		}

		#endregion

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
		public static new string GetCommandLineHelp()
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
			return false;
		}

		#endregion

		/// <summary>
		/// Verifies that the given path is a valid working directory for the game mode.
		/// </summary>
		/// <param name="p_strPath">The path to validate as a working directory.</param>
		/// <returns><lang cref="true"/> if the path is a vlid working directory;
		/// <lang cref="false"/> otherwise.</returns>
		public override bool VerifyWorkingDirectory(string p_strPath)
		{
			if (String.IsNullOrEmpty(p_strPath))
				return false;

			string[] strExes = new string[] { Path.Combine(p_strPath, "falloutNV.exe"),
												Path.Combine(p_strPath, "falloutNVng.exe") };
			bool booFound = false;
			foreach (string strExe in strExes)
				if (File.Exists(strExe))
				{
					booFound = true;
					break;
				}
			return booFound;
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
#if TRACE
			Trace.WriteLine("Looking for Fallout New Vegas.");
			Trace.Indent();
#endif
			string strWorkingDirectory = Properties.Settings.Default.falloutNewVegasWorkingDirectory;

			if (String.IsNullOrEmpty(strWorkingDirectory) || !Directory.Exists(strWorkingDirectory))
			{
				try
				{
					strWorkingDirectory = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Bethesda Softworks\FalloutNV", "Installed Path", null) as string;
				}
				catch
				{
					strWorkingDirectory = null;
				}
			}

			using (WorkingDirectorySelectionForm wdfForm = new WorkingDirectorySelectionForm(
					"Could not find Fallout: New Vegas directory." + Environment.NewLine +
					"Fallout's registry entry appears to be missing or incorrect." + Environment.NewLine +
					"Please enter the path to your Fallout: New Vegas game file, or click \"Auto Detect\" to search" +
					" for the install directory. Note that Auto Detection can take several minutes.",
					"Fallout 3 Game Directory:",
					new string[] { "falloutNV.exe", "falloutNVng.exe" }))
			{
				while (!VerifyWorkingDirectory(strWorkingDirectory))
				{
					if (wdfForm.ShowDialog() == DialogResult.Cancel)
					{
						p_strErrorMessage = "Could not find Fallout: New Vegas directory.";
						return false;
					}
					strWorkingDirectory = wdfForm.WorkingDirectory;
				}
			}
			Directory.SetCurrentDirectory(strWorkingDirectory);
			Properties.Settings.Default.falloutNewVegasWorkingDirectory = strWorkingDirectory;
			Properties.Settings.Default.Save();
#if TRACE
				Trace.WriteLine("Found: " + Path.GetFullPath("."));
#endif
			p_strErrorMessage = null;
			return true;
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
