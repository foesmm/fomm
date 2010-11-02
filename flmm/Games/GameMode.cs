using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Fomm.PackageManager;
using Fomm.PackageManager.XmlConfiguredInstall;
using Fomm.PackageManager.XmlConfiguredInstall.Parsers;

namespace Fomm.Games
{
	public abstract class GameMode
	{
		private string m_strModDirectory = null;
		private string m_strModInfoCacheDirectory = null;

		#region Properties

		/// <summary>
		/// Gets or sets the modDirectory of the GameMode.
		/// </summary>
		/// <value>The modDirectory of the GameMode.</value>
		public string ModDirectory
		{
			get
			{
				if (String.IsNullOrEmpty(m_strModDirectory))
					SetModDirectory();
				return m_strModDirectory;
			}
			protected set
			{
				m_strModDirectory = value;
			}
		}

		/// <summary>
		/// Gets or sets the modInfoCacheDirectory of the GameMode.
		/// </summary>
		/// <value>The modInfoCacheDirectory of the GameMode.</value>
		public string ModInfoCacheDirectory
		{
			get
			{
				if (String.IsNullOrEmpty(m_strModInfoCacheDirectory))
					SetModInfoCacheDirectory();
				return m_strModInfoCacheDirectory;
			}
			protected set
			{
				m_strModInfoCacheDirectory = value;
			}
		}

		public abstract string OverwriteDirectory
		{
			get;
		}

		public abstract string UserGameDataPath
		{
			get;
		}

		public abstract IDictionary<string, string> SettingsFiles
		{
			get;
		}

		public abstract string SavesPath
		{
			get;
		}

		public abstract string UserSettingsPath
		{
			get;
		}

		public abstract IDictionary<string, string> AdditionalPaths
		{
			get;
		}

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

		public abstract string PluginsPath
		{
			get;
		}

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
			if (!Directory.Exists(ModDirectory)) Directory.CreateDirectory(ModDirectory);
			if (!Directory.Exists(ModInfoCacheDirectory)) Directory.CreateDirectory(ModInfoCacheDirectory);

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

		protected void SetModDirectory()
		{
			m_strModDirectory = Settings.GetString("FomodDir");
			if (String.IsNullOrEmpty(m_strModDirectory))
				m_strModDirectory = Path.Combine(Program.ExecutableDirectory, "mods");
		}

		protected void SetModInfoCacheDirectory()
		{
			ModInfoCacheDirectory = Path.Combine(ModDirectory, "cache");
		}

		public abstract bool HandleStandaloneArguments(string[] p_strArgs);

		public abstract bool HandleInAppArguments(string[] p_strArgs);

		public abstract bool SetWorkingDirectory(out string p_strErrorMessage);

		public abstract void Init();

		public abstract bool IsPluginFile(string p_strPath);
	}
}
