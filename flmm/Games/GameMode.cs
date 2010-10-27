using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Fomm.PackageManager;

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

		protected void SetModDirectory()
		{
			ModDirectory = Settings.GetString("FomodDir");
			if (String.IsNullOrEmpty(ModDirectory))
				ModDirectory = Path.Combine(Program.ExecutableDirectory, "mods");
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
