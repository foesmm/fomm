using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Fomm.PackageManager;
using Fomm.PackageManager.XmlConfiguredInstall;
using Fomm.PackageManager.XmlConfiguredInstall.Parsers;
using System.Drawing;
using Fomm.Controls;
using Microsoft.Win32;

namespace Fomm.Games
{
	public abstract class GameMode
	{
		#region Properties

		/// <summary>
		/// Gets the game launch command.
		/// </summary>
		/// <value>The game launch command.</value>
		public abstract GameTool LaunchCommand
		{
			get;
		}

		/// <summary>
		/// Gets the icon used for the plugin file type.
		/// </summary>
		/// <value>The icon used for the plugin file type.</value>
		public abstract Icon PluginFileIcon
		{
			get;
		}

		/// <summary>
		/// Gets the modDirectory of the GameMode.
		/// </summary>
		/// <value>The modDirectory of the GameMode.</value>
		public abstract string ModDirectory
		{
			get;
		}

		/// <summary>
		/// Gets the modInfoCacheDirectory of the GameMode.
		/// </summary>
		/// <value>The modInfoCacheDirectory of the GameMode.</value>
		public abstract string ModInfoCacheDirectory
		{
			get;
		}

		/// <summary>
		/// Gets the directory where installation information is stored for this game mode.
		/// </summary>
		/// <remarks>
		/// This is where install logs, overwrites, and the like are stored.
		/// </remarks>
		/// <value>The directory where installation information is stored for this game mode.</value>
		public abstract string InstallInfoDirectory
		{
			get;
		}

		/// <summary>
		/// Gets the directory where overwrites are stored for this game mode.
		/// </summary>
		/// <value>The directory where overwrites are stored for this game mode.</value>
		public string OverwriteDirectory
		{
			get
			{
				string strDirectory = Path.Combine(InstallInfoDirectory, "overwrites");
				if (!Directory.Exists(strDirectory))
					Directory.CreateDirectory(strDirectory);
				return strDirectory;
			}
		}
		
		public abstract IDictionary<string, string> SettingsFiles
		{
			get;
		}

		public abstract IDictionary<string, string> AdditionalPaths
		{
			get;
		}
		
		/// <summary>
		/// Gets the path to the game's save game files.
		/// </summary>
		/// <value>The path to the game's save game files.</value>
		public abstract string SavesPath
		{
			get;
		}		

		#region Tool Injection

		public abstract IList<GameTool> Tools
		{
			get;
		}

		public abstract IList<GameTool> GameSettingsTools
		{
			get;
		}

		public abstract IList<GameTool> RightClickTools
		{
			get;
		}

		public abstract IList<GameTool> LoadOrderTools
		{
			get;
		}

		public abstract IList<GameTool> GameLaunchCommands
		{
			get;
		}

		#endregion

		public abstract IList<SettingsPage> SettingsPages
		{
			get;
		}

		/// <summary>
		/// Gets the path to the game directory were pluings are to be installed.
		/// </summary>
		/// <value>The path to the game directory were pluings are to be installed.</value>
		public abstract string PluginsPath
		{
			get;
		}

		/// <summary>
		/// Gets the plugin manager for this game mode.
		/// </summary>
		/// <value>The plugin manager for this game mode.</value>
		public abstract PluginManager PluginManager
		{
			get;
		}

		/// <summary>
		/// Gets the version of the installed game.
		/// </summary>
		/// <value>The version of the installed game.</value>
		public abstract Version GameVersion
		{
			get;
		}

		#endregion

		#region Constructors

		public GameMode()
		{
			//this folder can't be created here, as the path may not be set
			// further, this folder should be created by the game, so I don't think the appropriate way
			// to handle thing if it's missing is to create it
			//if (!Directory.Exists(UserSettingsPath)) Directory.CreateDirectory(UserSettingsPath);
		}

		#endregion

		#region Script

		/// <summary>
		/// Gets the default script for a mod.
		/// </summary>
		/// <value>The default script for a mod.</value>
		public virtual string DefaultCSharpScript
		{
			get
			{
				return @"using System;
using fomm.Scripting;

class Script : GenericBaseScript {
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
		public abstract ModInstallScript CreateInstallScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller);

		/// <summary>
		/// Creates a mod upgrade script for the given <see cref="fomod"/>.
		/// </summary>
		/// <param name="p_fomodMod">The mod for which to create an installer script.</param>
		/// <param name="p_mibInstaller">The installer for which the script is being created.</param>
		/// <returns>A mod upgrade script for the given <see cref="fomod"/>.</returns>
		public abstract ModInstallScript CreateUpgradeScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller);

		/// <summary>
		/// Creates a <see cref="DependencyStateManager"/> for the given <see cref="ModInstallScript"/>.
		/// </summary>
		/// <param name="p_misInstallScript">The <see cref="ModInstallScript"/> for which the
		/// <see cref="DependencyStateManager"/> is being created.</param>
		/// <returns>A <see cref="DependencyStateManager"/> for the given <see cref="ModInstallScript"/>.</returns>
		public abstract DependencyStateManager CreateDependencyStateManager(ModInstallScript p_misInstallScript);

		/// <summary>
		/// The factory method that creates the appropriate parser extension for the specified configuration file version.
		/// </summary>
		/// <param name="p_strVersion">The XML configuration file version for which to return a parser extension.</param>
		/// <returns>The appropriate parser extension for the specified configuration file version, or
		/// <lang cref="null"/> if no extension is available.</returns>
		public abstract ParserExtension CreateParserExtension(string p_strVersion);

		/// <summary>
		/// The factory method that returns the appropriate parser extension for the specified configuration file version.
		/// </summary>
		/// <param name="p_strVersion">The XML configuration file version for which to return a parser extension.</param>
		/// <returns>The appropriate parser extension for the specified configuration file version.</returns>
		public ParserExtension GetParserExtension(string p_strVersion)
		{
			ParserExtension pexExtension = CreateParserExtension(p_strVersion);
			return pexExtension ?? new ParserExtension();
		}

		/// <summary>
		/// Gets the path to the schema file for the specified configuration file version.
		/// </summary>
		/// <param name="p_strVersion">The XML configuration file version for which to return a parser extension.</param>
		/// <returns>The path to the schema file for the specified configuration file version, or
		/// <lang cref="null"/> if there is no game-specific schema for the specified configuration
		/// file version.</returns>
		public abstract string GetGameSpecificXMLConfigSchemaPath(string p_strVersion);

		/// <summary>
		/// Gets the path to the schema file for the specified configuration file version.
		/// </summary>
		/// <param name="p_strVersion">The XML configuration file version for which to return a parser extension.</param>
		/// <returns>The path to the schema file for the specified configuration file version.</returns>
		public string GetXMLConfigSchemaPath(string p_strVersion)
		{
			string strSchemaPath = GetGameSpecificXMLConfigSchemaPath(p_strVersion);
			return strSchemaPath ?? Path.Combine(Program.ExecutableDirectory, String.Format(@"fomm\ModConfig{0}.xsd", p_strVersion));
		}

		#endregion

		public abstract bool HandleStandaloneArguments(string[] p_strArgs);

		public abstract bool HandleInAppArguments(string[] p_strArgs);

		public abstract bool SetWorkingDirectory(out string p_strErrorMessage);

		public abstract bool Init();

		public abstract bool IsPluginFile(string p_strPath);

		/// <summary>
		/// hecks for any updates that are available for any game-specific components.
		/// </summary>
		/// <remarks><lang cref="true"/> if updates were available; otherwise <lang cref="false"/>.</remarks>
		public virtual bool CheckForUpdates()
		{
			return false;
		}
	}
}
