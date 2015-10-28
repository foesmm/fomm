using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Fomm.Commands;
using Fomm.Controls;
using Fomm.Games.Fallout3.PluginFormatProviders;
using Fomm.Games.Fallout3.Script;
using Fomm.Games.Fallout3.Script.XmlConfiguredInstall;
using Fomm.Games.Fallout3.Script.XmlConfiguredInstall.Parsers;
using Fomm.Games.Fallout3.Settings;
using Fomm.Games.Fallout3.Tools;
using Fomm.Games.Fallout3.Tools.AutoSorter;
using Fomm.Games.Fallout3.Tools.BSA;
using Fomm.Games.Fallout3.Tools.CriticalRecords;
using Fomm.Games.Fallout3.Tools.GraphicsSettings;
using Fomm.Games.Fallout3.Tools.InstallTweaker;
using Fomm.Games.Fallout3.Tools.TESsnip;
using Fomm.PackageManager;
using Fomm.PackageManager.XmlConfiguredInstall;
using Fomm.PackageManager.XmlConfiguredInstall.Parsers;
using Microsoft.Win32;

namespace Fomm.Games.Fallout3
{
  /// <summary>
  ///   Provides information required for the programme to manage Fallout 3 plugins.
  /// </summary>
  public class Fallout3GameMode : GameMode
  {
    /// <summary>
    ///   This class provides strongly-typed access to this game mode's settings files.
    /// </summary>
    public class SettingsFilesSet : Dictionary<string, string>
    {
      private const string FOIniPathKey = "FOIniPath";
      private const string FOPrefsIniPathKey = "FOPrefsIniPath";
      private const string GeckIniPathKey = "GeckIniPath";
      private const string GeckPrefsIniPathKey = "GeckPrefsIniPath";

      #region Properties

      /// <summary>
      ///   Gets or sets the path to the Fallout.ini file.
      /// </summary>
      /// <value>The path to the Fallout.ini file.</value>
      public string FOIniPath
      {
        get
        {
          return this[FOIniPathKey];
        }
        set
        {
          this[FOIniPathKey] = value;
        }
      }

      /// <summary>
      ///   Gets or sets the path to the FalloutPrefs.ini file.
      /// </summary>
      /// <value>The path to the FalloutPrefs.ini file.</value>
      public string FOPrefsIniPath
      {
        get
        {
          return this[FOPrefsIniPathKey];
        }
        set
        {
          this[FOPrefsIniPathKey] = value;
        }
      }

      /// <summary>
      ///   Gets or sets the path to the Geck.ini file.
      /// </summary>
      /// <value>The path to the Geck.ini file.</value>
      public string GeckIniPath
      {
        get
        {
          return this[GeckIniPathKey];
        }
        set
        {
          this[GeckIniPathKey] = value;
        }
      }

      /// <summary>
      ///   Gets or sets the path to the GeckPrefs.ini file.
      /// </summary>
      /// <value>The path to the GeckPrefs.ini file.</value>
      public string GeckPrefsIniPath
      {
        get
        {
          return this[GeckPrefsIniPathKey];
        }
        set
        {
          this[GeckPrefsIniPathKey] = value;
        }
      }

      #endregion
    }

    private string m_strSavesPath;
    private Dictionary<string, string> m_dicAdditionalPaths = new Dictionary<string, string>();
    private SettingsFilesSet m_sfsSettingsFiles;

    private Dictionary<string, IPluginFormatProvider> m_dicPluginFormatProviders =
      new Dictionary<string, IPluginFormatProvider>();

    private List<Command<MainForm>> m_lstTools = new List<Command<MainForm>>();
    private List<Command<MainForm>> m_lstGameSettingsTools = new List<Command<MainForm>>();
    private List<Command<MainForm>> m_lstRightClickTools = new List<Command<MainForm>>();
    private List<Command<MainForm>> m_lstLoadOrderTools = new List<Command<MainForm>>();
    private List<Command<MainForm>> m_lstGameLaunchCommands = new List<Command<MainForm>>();
    private List<SettingsPage> m_lstSettingsPages = new List<SettingsPage>();
    private Fallout3PluginManager m_pmgPluginManager;

    #region Properties

    /// <summary>
    ///   Gets the name of the game whose plugins are being managed.
    /// </summary>
    /// <value>The name of the game whose plugins are being managed.</value>
    public override string GameName
    {
      get
      {
        return "Fallout 3";
      }
    }

    /// <summary>
    ///   Gets the modDirectory of the GameMode.
    /// </summary>
    /// <value>The modDirectory of the GameMode.</value>
    public override string ModDirectory
    {
      get
      {
        var strModDirectory = Properties.Settings.Default.fallout3ModDirectory;
        if (String.IsNullOrEmpty(strModDirectory))
        {
          throw new Exception("The Mod Directory for Fallout 3 Mods has not been set.");
        }
        if (!Directory.Exists(strModDirectory))
        {
          Directory.CreateDirectory(strModDirectory);
        }
        return strModDirectory;
      }
    }

    /// <summary>
    ///   Gets the modInfoCacheDirectory of the GameMode.
    /// </summary>
    /// <value>The modInfoCacheDirectory of the GameMode.</value>
    public override string ModInfoCacheDirectory
    {
      get
      {
        var strCache = Path.Combine(ModDirectory, "cache");
        if (!Directory.Exists(strCache))
        {
          Directory.CreateDirectory(strCache);
        }
        return strCache;
      }
    }

    /// <summary>
    ///   Gets the game launch command.
    /// </summary>
    /// <value>The game launch command.</value>
    public override Command<MainForm> LaunchCommand
    {
      get
      {
        if (String.IsNullOrEmpty(Properties.Settings.Default.fallout3LaunchCommand) && File.Exists("fose_loader.exe"))
        {
          return new Command<MainForm>("Launch FOSE", "Launches Fallout 3 using FOSE.", LaunchGame);
        }
        return new Command<MainForm>("Launch Fallout 3", "Launches Fallout 3 using FOSE.", LaunchGame);
      }
    }

    /// <summary>
    ///   Gets the icon used for the plugin file type.
    /// </summary>
    /// <value>The icon used for the plugin file type.</value>
    public override Icon PluginFileIcon
    {
      get
      {
        var strFalloutEsm = Path.Combine(PluginsPath, "fallout3.esm");
        return Icon.ExtractAssociatedIcon(strFalloutEsm);
      }
    }

    private string pp;

    /// <summary>
    ///   Gets the path to the game directory were pluings are to be installed.
    /// </summary>
    /// <value>The path to the game directory were pluings are to be installed.</value>
    public override string PluginsPath
    {
      get
      {
        if (pp == null)
        {
          pp = Path.Combine(Environment.CurrentDirectory, "Data");
        }
        return pp;
      }
    }

    /// <summary>
    ///   Gets the path to the plugins.txt file.
    /// </summary>
    /// <remarks>
    ///   plugins.txt is a Fallout 3 file that tracks active plugins.
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
    ///   Gets the directory where installation information is stored for this game mode.
    /// </summary>
    /// <remarks>
    ///   This is where install logs, overwrites, and the like are stored.
    /// </remarks>
    /// <value>The directory where installation information is stored for this game mode.</value>
    public override string InstallInfoDirectory
    {
      get
      {
        var strDirectory = Properties.Settings.Default.fallout3InstallInfoDirectory;
        if (String.IsNullOrEmpty(strDirectory))
        {
          throw new Exception("The InstallInfoDirectory for Fallout 3 has not been set.");
        }
        if (!Directory.Exists(strDirectory))
        {
          Directory.CreateDirectory(strDirectory);
        }
        return strDirectory;
      }
    }

    /// <summary>
    ///   Gets the settings files used in the game mode.
    /// </summary>
    /// <value>The settings files used in the game mode.</value>
    public override IDictionary<string, string> SettingsFiles
    {
      get
      {
        return m_sfsSettingsFiles;
      }
    }

    /// <summary>
    ///   Gets any other paths used in the game mode.
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
    ///   Gets the <see cref="IPluginFormatProvider" />s provided by the game mode.
    /// </summary>
    /// <value>The <see cref="IPluginFormatProvider" />s provided by the game mode.</value>
    public override IList<IPluginFormatProvider> PluginFormatProviders
    {
      get
      {
        return new List<IPluginFormatProvider>(m_dicPluginFormatProviders.Values);
      }
    }

    /// <summary>
    ///   Gets the path to the game's save game files.
    /// </summary>
    /// <value>The path to the game's save game files.</value>
    public override string SavesPath
    {
      get
      {
        return m_strSavesPath;
      }
    }

    /// <summary>
    ///   Gets the path to the directory where Windows live install the DLCs.
    /// </summary>
    /// <value>The path to the directory where Windows live install the DLCs.</value>
    private string DLCDirectory
    {
      get
      {
        return m_dicAdditionalPaths["DLCDir"];
      }
    }

    /// <summary>
    ///   Gets or sets the CriticalRecordPluginFormatProvider.
    /// </summary>
    /// <value>The CriticalRecordPluginFormatProvider.</value>
    protected CriticalRecordPluginFormatProvider CriticalRecordPluginFormatProvider
    {
      get
      {
        return (CriticalRecordPluginFormatProvider) m_dicPluginFormatProviders["ConflictDetector"];
      }
      set
      {
        m_dicPluginFormatProviders["ConflictDetector"] = value;
      }
    }

    /// <summary>
    ///   Sets the BoldESMPluginFormatProvider.
    /// </summary>
    /// <value>The BoldESMPluginFormatProvider.</value>
    protected BoldESMPluginFormatProvider BoldESMPluginFormatProvider
    {
      set
      {
        m_dicPluginFormatProviders["ESMBoldify"] = value;
      }
    }

    protected ColorizerPluginFormatProvider ColorizerPluginFormatProider
    {
      set
      {
        m_dicPluginFormatProviders["Colorize"] = value;
      }
    }

    #region Tool Injection

    /// <summary>
    ///   Gets the list of tools to add to the tools menu.
    /// </summary>
    /// <value>The list of tools to add to the tools menu.</value>
    public override IList<Command<MainForm>> Tools
    {
      get
      {
        return m_lstTools;
      }
    }

    /// <summary>
    ///   Gets the list of tools to add to the game settings menu.
    /// </summary>
    /// <value>The list of tools to add to the game settings menu.</value>
    public override IList<Command<MainForm>> GameSettingsTools
    {
      get
      {
        return m_lstGameSettingsTools;
      }
    }

    /// <summary>
    ///   Gets the list of tools to add to the right-click menu.
    /// </summary>
    /// <value>The list of tools to add to the right-click menu.</value>
    public override IList<Command<MainForm>> RightClickTools
    {
      get
      {
        return m_lstRightClickTools;
      }
    }

    /// <summary>
    ///   Gets the list of tools to add to the load order menu.
    /// </summary>
    /// <value>The list of tools to add to the load order menu.</value>
    public override IList<Command<MainForm>> LoadOrderTools
    {
      get
      {
        return m_lstLoadOrderTools;
      }
    }

    /// <summary>
    ///   Gets the list of game launch commands.
    /// </summary>
    /// <value>The list of game launch commands.</value>
    public override IList<Command<MainForm>> GameLaunchCommands
    {
      get
      {
        return m_lstGameLaunchCommands;
      }
    }

    #endregion

    /// <summary>
    ///   Gets the settings pages that privode management of game mode-specific settings.
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
    ///   Gets the plugin manager for this game mode.
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
    ///   Gets the Fallout 3 rederer info file.
    /// </summary>
    /// <value>The Fallout 3 rederer info file.</value>
    public string FORendererFile
    {
      get
      {
        return m_dicAdditionalPaths["FORendererFile"];
      }
    }

    /// <summary>
    ///   Gets the version of the installed game.
    /// </summary>
    /// <value>The version of the installed game.</value>
    public override Version GameVersion
    {
      get
      {
        if (File.Exists("Fallout3.exe"))
        {
          return
            new Version(FileVersionInfo.GetVersionInfo("Fallout3.exe").FileVersion.Replace(", ", "."));
        }
        if (File.Exists("Fallout3ng.exe"))
        {
          return
            new Version(FileVersionInfo.GetVersionInfo("Fallout3ng.exe")
                                       .FileVersion.Replace(", ", "."));
        }
        return null;
      }
    }

    /// <summary>
    ///   Gets the path to the per user Fallout 3 data.
    /// </summary>
    /// <value>The path to the per user Fallout 3 data.</value>
    protected virtual string UserGameDataPath
    {
      get
      {
        return Path.Combine(Program.PersonalDirectory, "My games\\Fallout3");
      }
    }

    #endregion

    public override void PreInit()
    {
      m_sfsSettingsFiles = CreateSettingsFileSet();
      m_pmgPluginManager = CreatePluginManager();
      SetupPluginFormatProviders();
      SetupPaths();
      SetupSettingsPages();
      SetupTools();
      SetupLaunchCommands();
    }

    #region Initialization

    /// <summary>
    ///   This initializes the game mode.
    /// </summary>
    /// <remarks>
    ///   This gets the user to specify the directories where the programme will store info
    ///   such as install logs, if the directories have not already been setup.
    ///   This method also checks for DLCs, and cleans up any missing FOMods.
    /// </remarks>
    /// <returns>
    ///   <lang langref="true" /> if the game mode was able to initialize;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public override bool Init()
    {
      if (!Properties.Settings.Default.fallout3DoneSetup)
      {
        var sfmSetup = new SetupForm();
        if (sfmSetup.ShowDialog() == DialogResult.Cancel)
        {
          return false;
        }
        Properties.Settings.Default.fallout3DoneSetup = true;
        Properties.Settings.Default.Save();
      }

      CheckForDLCs();

      if (!File.Exists(((SettingsFilesSet) SettingsFiles).FOIniPath))
      {
        MessageBox.Show(
          "You have no Fallout INI file. Please run Fallout 3 to initialize the file before installing any mods or turning on Archive Invalidation.",
          "Missing INI", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      if (File.Exists("FO3Edit.exe"))
      {
        m_lstTools.Add(new Command<MainForm>("FO3Edit", "Launches FO3Edit, if it is installed.", LaunchFO3Edit));
      }
      m_lstTools.Add(new CheckedCommand<MainForm>("Archive Invalidation", "Toggles Archive Invalidation.",
                                                  ArchiveInvalidation.IsActive(),
                                                  ToggleArchiveInvalidation));

      ScanForReadonlyPlugins();
      ScanForReadonlyFiles();

      var m = new FOMMMigrator();
      if (!m.Migrate())
      {
        return false;
      }

      return true;
    }

    /// <summary>
    ///   Creates the plugin manager that will be used by this game mode.
    /// </summary>
    /// <returns>The plugin manager that will be used by this game mode.</returns>
    protected virtual Fallout3PluginManager CreatePluginManager()
    {
      return new Fallout3PluginManager();
    }

    /// <summary>
    ///   Creates the settings file set that will be used by this game mode.
    /// </summary>
    /// <returns>The settings file set that will be used by this game mode.</returns>
    protected virtual SettingsFilesSet CreateSettingsFileSet()
    {
      return new SettingsFilesSet();
    }

    /// <summary>
    ///   Sets up the plugin format providers for this game mode.
    /// </summary>
    protected virtual void SetupPluginFormatProviders()
    {
      CriticalRecordPluginFormatProvider = new CriticalRecordPluginFormatProvider();
      BoldESMPluginFormatProvider = new BoldESMPluginFormatProvider();
      ColorizerPluginFormatProider = new ColorizerPluginFormatProvider();
    }

    /// <summary>
    ///   Sets up the paths for this game mode.
    /// </summary>
    protected virtual void SetupPaths()
    {
      ((SettingsFilesSet) SettingsFiles).FOIniPath = Path.Combine(UserGameDataPath, "Fallout.ini");
      ((SettingsFilesSet) SettingsFiles).FOPrefsIniPath = Path.Combine(UserGameDataPath, "FalloutPrefs.ini");
      ((SettingsFilesSet) SettingsFiles).GeckIniPath = Path.Combine(UserGameDataPath, "GECKCustom.ini");
      ((SettingsFilesSet) SettingsFiles).GeckPrefsIniPath = Path.Combine(UserGameDataPath, "GECKPrefs.ini");

      m_dicAdditionalPaths["FORendererFile"] = Path.Combine(UserGameDataPath, "RendererInfo.txt");
      m_dicAdditionalPaths["PluginsFile"] =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fallout3/plugins.txt");
      m_dicAdditionalPaths["DLCDir"] =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\xlive\\DLC");

      m_strSavesPath = Path.Combine(UserGameDataPath,
                                    NativeMethods.GetPrivateProfileString("General", "SLocalSavePath", "Games",
                                                                          ((SettingsFilesSet) SettingsFiles).FOIniPath));
    }

    /// <summary>
    ///   Gets up the game-specific settings pages.
    /// </summary>
    protected virtual void SetupSettingsPages()
    {
      var gsp = new GeneralSettingsPage();
      gsp.Text = "Fallout 3";
      gsp.BackColor = Color.FromKnownColor(KnownColor.Transparent);

      m_lstSettingsPages.Add(gsp);
    }

    /// <summary>
    ///   Sets up the launch commands for the game.
    /// </summary>
    protected virtual void SetupLaunchCommands()
    {
      m_lstGameLaunchCommands.Add(new Command<MainForm>("Launch Fallout 3", "Launches plain Fallout 3.",
                                                        LaunchFallout3Plain));
      if (File.Exists("fose_loader.exe"))
      {
        m_lstGameLaunchCommands.Add(new Command<MainForm>("Launch FOSE", "Launches Fallout 3 with FOSE.",
                                                          LaunchFallout3FOSE));
      }
      m_lstGameLaunchCommands.Add(new Command<MainForm>("Launch Custom Fallout 3",
                                                        "Launches Fallout 3 with custom command.", LaunchFallout3Custom));
    }

    /// <summary>
    ///   Sets up the tools for this game mode.
    /// </summary>
    protected virtual void SetupTools()
    {
      m_lstTools.Add(new Command<MainForm>("BSA Browser", "Views and unpacks BSA files.", LaunchBSABrowserTool));
      m_lstTools.Add(new Command<MainForm>("BSA Creator", "Creates BSA files.", LaunchBSACreatorTool));
      m_lstTools.Add(new Command<MainForm>("TESsnip", "An ESP/ESM editor.", LaunchTESsnipTool));
      m_lstTools.Add(new Command<MainForm>("Shader Editor", "A shader (SDP) editor.", LaunchShaderEditTool));
      m_lstTools.Add(new Command<MainForm>("CREditor", "Edits critical records in an ESP/ESM.", LaunchCREditorTool));
      m_lstTools.Add(new Command<MainForm>("Install Tweaker", "Advanced Fallout 3 tweaking.", LaunchInstallTweakerTool));
      m_lstTools.Add(new Command<MainForm>("Conflict Detector",
                                           "Checks for conflicts with mod-author specified critical records.",
                                           LaunchConflictDetector));
      m_lstTools.Add(new Command<MainForm>("Save Games", "Save game info viewer.", LaunchSaveGamesViewer));

      m_lstGameSettingsTools.Add(new Command<MainForm>("Graphics Settings", "Changes the graphics settings.",
                                                       LaunchGraphicsSettingsTool));

      m_lstRightClickTools.Add(new Command<MainForm>("Open in TESsnip...", "Open the selected plugins in TESsnip.",
                                                     LaunchTESsnipToolWithSelectedPlugins));
      m_lstRightClickTools.Add(new Command<MainForm>("Open in CREditor...", "Open the selected plugins in TESsnip.",
                                                     LaunchCREditorToolWithSelectedPlugins));

      m_lstLoadOrderTools.Add(new Command<MainForm>("Load Order Report...",
                                                    "Generates a report on the current load order, as compared to the BOSS recomendation.",
                                                    LaunchLoadOrderReport));
      m_lstLoadOrderTools.Add(new Command<MainForm>("BOSS Auto Sort", "Auto-sorts the plugins using BOSS's masterlist.",
                                                    LaunchSortPlugins));
    }

    #endregion

    #region Tool Launch Methods

    #region Game Launch

    public bool PrelaunchCheckOrder()
    {
      var retVal = true;
      int i;

      // Do checks
      List<string> keys = new List<string>(fullModList.Keys);
      for (i = 0; i < keys.Count; i++)
      {
        if (!retVal)
        {
          break;
        }
        var key = keys[i];
        retVal = (getPluginDependencyStatus(key, true) == 0);
      }

      return retVal;
    }

    /// <summary>
    ///   Launches the game with a custom command.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchFallout3Custom(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (PrelaunchCheckOrder())
      {
        if (p_eeaArguments.Argument.HasOpenUtilityWindows)
        {
          MessageBox.Show("Please close all utility windows before launching fallout");
          return;
        }
        var command = Properties.Settings.Default.fallout3LaunchCommand;
        var args = Properties.Settings.Default.fallout3LaunchCommandArgs;
        if (String.IsNullOrEmpty(command))
        {
          MessageBox.Show("No custom launch command has been set", "Error");
          return;
        }
        try
        {
          var psi = new ProcessStartInfo();
          psi.Arguments = args;
          psi.FileName = command;
          psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(command));
          if (Process.Start(psi) == null)
          {
            MessageBox.Show("Failed to launch '" + command + "'");
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Failed to launch '" + command + "'\n" + ex.Message);
        }
      }
    }

    /// <summary>
    ///   Launches the game, with FOSE.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchFallout3FOSE(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (PrelaunchCheckOrder())
      {
        if (!File.Exists("fose_loader.exe"))
        {
          MessageBox.Show("fose does not appear to be installed");
          return;
        }
        if (p_eeaArguments.Argument.HasOpenUtilityWindows)
        {
          MessageBox.Show("Please close all utility windows before launching fallout");
          return;
        }
        try
        {
          var psi = new ProcessStartInfo();
          psi.FileName = "fose_loader.exe";
          psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath("fose_loader.exe"));
          if (Process.Start(psi) == null)
          {
            MessageBox.Show("Failed to launch 'fose_loader.exe'");
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Failed to launch 'fose_loader.exe'\n" + ex.Message);
        }
      }
    }

    /// <summary>
    ///   Launches the game, without FOSE.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchFallout3Plain(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (PrelaunchCheckOrder())
      {
        if (p_eeaArguments.Argument.HasOpenUtilityWindows)
        {
          MessageBox.Show("Please close all utility windows before launching fallout");
          return;
        }
        var command = File.Exists("fallout3.exe") ? "fallout3.exe" : "fallout3ng.exe";
        try
        {
          var psi = new ProcessStartInfo();
          psi.FileName = command;
          psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(command));
          if (Process.Start(psi) == null)
          {
            MessageBox.Show("Failed to launch '" + command + "'");
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Failed to launch '" + command + "'\n" + ex.Message);
        }
      }
    }

    /// <summary>
    ///   Launches the game, using FOSE if present.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public virtual void LaunchGame(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      var command = Properties.Settings.Default.fallout3LaunchCommand;
      var args = Properties.Settings.Default.fallout3LaunchCommandArgs;

      if (PrelaunchCheckOrder())
      {
        if (String.IsNullOrEmpty(command))
        {
          if (File.Exists("fose_loader.exe"))
          {
            command = "fose_loader.exe";
          }
          else if (File.Exists("fallout3.exe"))
          {
            command = "fallout3.exe";
          }
          else
          {
            command = "fallout3ng.exe";
          }
          args = null;
        }
        try
        {
          var psi = new ProcessStartInfo();
          psi.Arguments = args;
          psi.FileName = command;
          psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(command));
          if (Process.Start(psi) == null)
          {
            MessageBox.Show("Failed to launch '" + command + "'");
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Failed to launch '" + command + "'\n" + ex.Message);
        }
      }
    }

    #endregion

    #region Load Order Menu

    /// <summary>
    ///   Auto-sorts the plugins using BOSS's masterlist.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchSortPlugins(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (MessageBox.Show(p_eeaArguments.Argument,
                          "This is currently a beta feature, and the load order template may not be optimal.\n" +
                          "Ensure you have a backup of your load order before running this tool.\n" +
                          "Are you sure you wish to continue?", "Warning", MessageBoxButtons.YesNo,
                          MessageBoxIcon.Warning) != DialogResult.Yes)
      {
        return;
      }

      var plugins = PluginManager.OrderedPluginList;
      for (var i = 0; i < plugins.Length; i++)
      {
        plugins[i] = Path.GetFileName(plugins[i]);
      }
      var losSorter = new LoadOrderSorter();
      if (!losSorter.HasMasterList)
      {
        if (DialogResult.Yes ==
            MessageBox.Show("There is no BOSS masterlist present, would you like to fetch the latest one?",
                            "Update BOSS", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
        {
          if (updateBOSS())
          {
            if (losSorter.HasMasterList)
            {
              losSorter.LoadList();
            }
            else
            {
              MessageBox.Show("BOSS masterlist still missing!", "BOSS update error", MessageBoxButtons.OK,
                              MessageBoxIcon.Exclamation);
              return;
            }
          }
        }
        else
        {
          return;
        }
      }

      losSorter.SortList(plugins);
      for (var i = 0; i < plugins.Length; i++)
      {
        PluginManager.SetLoadOrder(Path.Combine(PluginsPath, plugins[i]), i);
      }
      p_eeaArguments.Argument.RefreshPluginList();
    }

    public virtual bool updateBOSS()
    {
      var ret = false;

      try
      {
        var bupUpdater = new Fallout3BOSSUpdater();
        bupUpdater.UpdateMasterlist(LoadOrderSorter.LoadOrderTemplatePath);
        ret = true;
      }
      catch (Exception e)
      {
        MessageBox.Show("There was an error updating BOSS\n" + e.Message, "BOSS update error", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
      }

      return ret;
    }

    /// <summary>
    ///   Generates a report on the current load order, as compared to the BOSS recomendation.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchLoadOrderReport(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      var losSorter = new LoadOrderSorter();
      if (!losSorter.HasMasterList)
      {
        if (DialogResult.Yes ==
            MessageBox.Show("There is no BOSS masterlist present, would you like to fetch the latest one?",
                            "Update BOSS", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
        {
          if (updateBOSS())
          {
            if (losSorter.HasMasterList)
            {
              losSorter.LoadList();
            }
            else
            {
              MessageBox.Show("BOSS masterlist still missing!", "BOSS update error", MessageBoxButtons.OK,
                              MessageBoxIcon.Exclamation);
              return;
            }
          }
        }
        else
        {
          return;
        }
      }

      var plugins = PluginManager.OrderedPluginList;
      var active = new bool[plugins.Length];
      var corrupt = new bool[plugins.Length];
      var masters = new string[plugins.Length][];
      var mlist = new List<string>();
      for (var i = 0; i < plugins.Length; i++)
      {
        active[i] = PluginManager.IsPluginActive(plugins[i]);
        plugins[i] = Path.GetFileName(plugins[i]);
        Plugin p;
        try
        {
          p = new Plugin(Path.Combine(PluginsPath, plugins[i]), true);
        }
        catch
        {
          p = null;
          corrupt[i] = true;
        }
        if (p != null)
        {
          foreach (var sr in ((Record) p.Records[0]).SubRecords)
          {
            if (sr.Name != "MAST")
            {
              continue;
            }
            mlist.Add(sr.GetStrData().ToLowerInvariant());
          }
          if (mlist.Count > 0)
          {
            masters[i] = mlist.ToArray();
            mlist.Clear();
          }
        }
      }

      var s = losSorter.GenerateReport(plugins, active, corrupt, masters);
      TextEditor.ShowEditor(s, TextEditorType.Text, false);
    }

    #endregion

    #region Right Click Menu

    /// <summary>
    ///   Launches the TESsnip tool, passing it the given plugins.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchTESsnipToolWithSelectedPlugins(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (p_eeaArguments.Argument.SelectedPlugins.Count == 0)
      {
        return;
      }
      var lstPlugins = new List<string>();
      foreach (var strPluginName in p_eeaArguments.Argument.SelectedPlugins)
      {
        lstPlugins.Add(Path.Combine(Program.GameMode.PluginsPath, strPluginName));
      }
      var tes = new TESsnip(lstPlugins.ToArray());
      tes.FormClosed += delegate
      {
        p_eeaArguments.Argument.RefreshPluginList();
        GC.Collect();
      };
      tes.Show();
    }

    /// <summary>
    ///   Launches the CREditor tool, passing it the given plugins.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchCREditorToolWithSelectedPlugins(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (p_eeaArguments.Argument.SelectedPlugins.Count == 0)
      {
        return;
      }
      var lstPlugins = new List<string>();
      foreach (var strPluginName in p_eeaArguments.Argument.SelectedPlugins)
      {
        lstPlugins.Add(Path.Combine(Program.GameMode.PluginsPath, strPluginName));
      }
      var crfEditor =
        new CriticalRecordsForm(lstPlugins.ToArray());
      crfEditor.Show();
    }

    #endregion

    #region Game Settings Menu

    /// <summary>
    ///   Launches the Graphics Settings tool.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchGraphicsSettingsTool(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      var gsfGraphicsSettingsForm = new GraphicsSettings();
      gsfGraphicsSettingsForm.ShowDialog();
    }

    #endregion

    #region Tools Menu

    /// <summary>
    ///   Launches FO3Edit, if present.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public virtual void LaunchFO3Edit(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (!File.Exists("FO3Edit.exe"))
      {
        MessageBox.Show(p_eeaArguments.Argument, "Could not find FO3Edit. Please install it.", "Missing FO3Edit",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      try
      {
        var psi = new ProcessStartInfo();
        psi.FileName = "FO3Edit.exe";
        psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(psi.FileName));
        if (Process.Start(psi) == null)
        {
          MessageBox.Show("Failed to launch FO3Edit.");
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to launch FO3Edit." + Environment.NewLine + ex.Message);
      }
    }

    /// <summary>
    ///   Launches the save games viewer.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public virtual void LaunchSaveGamesViewer(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      var lstActive = new List<string>();
      //the original implementation populated the inactive list with all plugins
      // we only populate it with inactive plugins - hopefully that's OK
      var lstInactive = new List<string>();

      foreach (var strPlugin in PluginManager.OrderedPluginList)
      {
        if (PluginManager.IsPluginActive(strPlugin))
        {
          lstActive.Add(Path.GetFileName(strPlugin));
        }
        else
        {
          lstInactive.Add(Path.GetFileName(strPlugin));
        }
      }
      (new SaveForm(lstActive.ToArray(), lstInactive.ToArray())).Show();
    }

    /// <summary>
    ///   Launches the conflict detector tool.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchConflictDetector(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      var strMessage =
        "This is an experimental feature that relies on fomod authors specifying which parts of their plugins are critical." +
        Environment.NewLine +
        "Using this feature will not hurt anything, but it is not guaranteed to find any or all conflicts.";
      if (
        MessageBox.Show(p_eeaArguments.Argument, strMessage, "Warning", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Information) == DialogResult.Cancel)
      {
        return;
      }
      var pcdDetector = new PluginConflictDetector(CriticalRecordPluginFormatProvider);
      pcdDetector.CheckForConflicts();
      p_eeaArguments.Argument.LoadPluginInfo();
    }

    /// <summary>
    ///   Launches the BSA Browser.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchBSABrowserTool(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      new BSABrowser().Show();
    }

    /// <summary>
    ///   Launches the BSA Creator.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchBSACreatorTool(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      new BSACreator().Show();
    }

    /// <summary>
    ///   Launches the Install Tweaker tool.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchInstallTweakerTool(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (p_eeaArguments.Argument.IsPackageManagerOpen)
      {
        MessageBox.Show(p_eeaArguments.Argument, "Please close the Package Manager before running the install tweaker.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }
      (new InstallationTweaker()).ShowDialog();
    }

    /// <summary>
    ///   Launches the TESsnip tool.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchTESsnipTool(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      var tes = new TESsnip();
      tes.FormClosed += delegate
      {
        p_eeaArguments.Argument.RefreshPluginList();
        GC.Collect();
      };
      tes.Show();
    }

    /// <summary>
    ///   Launches the Shader Edit tool.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchShaderEditTool(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      new Tools.ShaderEdit.MainForm().Show();
    }

    /// <summary>
    ///   Launches the CREditor tool.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchCREditorTool(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      var crfEditor = new CriticalRecordsForm();
      crfEditor.Show();
      GC.Collect();
    }

    /// <summary>
    ///   Toggles archive invalidation.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public virtual void ToggleArchiveInvalidation(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (ArchiveInvalidation.Update())
      {
        ((CheckedCommand<MainForm>) p_objCommand).IsChecked = ArchiveInvalidation.IsActive();
      }
    }

    #endregion

    #endregion

    #region Scripts

    /// <summary>
    ///   Gets the default script for a mod.
    /// </summary>
    /// <value>The default script for a mod.</value>
    public override string DefaultCSharpScript
    {
      get
      {
        return @"using System;
using fomm.Scripting;

class Script : Fallout3BaseScript {
  public static bool OnActivate() {
        //Install all files from the fomod and activate any esps
        PerformBasicInstall();
    return true;
  }

  /// <summary>
  /// Recursively copies all files and folders from one location to another.
  /// </summary>
  /// <param name=""p_strFrom"">The source from whence to copy the files.</param>
  /// <param name=""p_strTo"">The destination for the copied files.</param>
  protected static void InstallFolderFromFomod (string p_strFrom, string p_strTo)
  {
    List<string> lstFOMODFiles = GetFomodFolderFileList(p_strFrom);
    m_bwdProgress.ItemProgress = 0;
    m_bwdProgress.ItemProgressMaximum = lstFOMODFiles.Count;

    String strFrom = p_strFrom.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();
    if (!strFrom.EndsWith(Path.DirectorySeparatorChar.ToString()))
      strFrom += Path.DirectorySeparatorChar;
    String strTo = p_strTo.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    if ((strTo.Length > 0) && (!strTo.EndsWith(Path.DirectorySeparatorChar.ToString())))
      strTo += Path.DirectorySeparatorChar;
    String strFOMODFile = null;
    for (Int32 i = 0; i < lstFOMODFiles.Count; i++)
    {
      if (m_bwdProgress.Cancelled())
        return;

      strFOMODFile = lstFOMODFiles[i];
      string strNewFileName = strFOMODFile.Substring(strFrom.Length, strFOMODFile.Length - strFrom.Length);
      CopyDataFile(strFOMODFile, Path.Combine(strTo, strNewFileName));

      m_bwdProgress.StepItemProgress();
    }
  }

  /// <summary>
  /// Gets a list of all files in the specified FOMOD folder.
  /// </summary>
  /// <param name=""p_strPath"">The FOMOD folder whose file list is to be retrieved.</param>
  /// <returns>The list of all files in the specified FOMOD folder.</returns>
  protected static List<string> GetFomodFolderFileList(string p_strPath)
  {
    if (m_strFomodFiles == null)
    {
      m_strFomodFiles = GetFomodFileList();
      for (Int32 i = m_strFomodFiles.Length - 1; i >= 0; i--)
        m_strFomodFiles[i] = m_strFomodFiles[i].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }
    String strPath = p_strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();
    List<string> lstFiles = new List<string>();
    foreach (string strFile in m_strFomodFiles)
      if (strFile.ToLowerInvariant().StartsWith(strPath))
        lstFiles.Add(strFile);
    return lstFiles;
  }
  static string[] m_strFomodFiles = null;
}
";
      }
    }

    /// <summary>
    ///   Creates a mod install script for the given <see cref="fomod" />.
    /// </summary>
    /// <param name="p_fomodMod">The mod for which to create an installer script.</param>
    /// <param name="p_mibInstaller">The installer for which the script is being created.</param>
    /// <returns>A mod install script for the given <see cref="fomod" />.</returns>
    public override ModInstallScript CreateInstallScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller)
    {
      return new Fallout3ModInstallScript(p_fomodMod, p_mibInstaller);
    }

    /// <summary>
    ///   Creates a mod upgrade script for the given <see cref="fomod" />.
    /// </summary>
    /// <param name="p_fomodMod">The mod for which to create an installer script.</param>
    /// <param name="p_mibInstaller">The installer for which the script is being created.</param>
    /// <returns>A mod upgrade script for the given <see cref="fomod" />.</returns>
    public override ModInstallScript CreateUpgradeScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller)
    {
      return new Fallout3ModUpgradeScript(p_fomodMod, p_mibInstaller);
    }

    /// <summary>
    ///   Creates a <see cref="DependencyStateManager" /> for the given <see cref="ModInstallScript" />.
    /// </summary>
    /// <param name="p_misInstallScript">
    ///   The <see cref="ModInstallScript" /> for which the
    ///   <see cref="DependencyStateManager" /> is being created.
    /// </param>
    /// <returns>A <see cref="DependencyStateManager" /> for the given <see cref="ModInstallScript" />.</returns>
    public override DependencyStateManager CreateDependencyStateManager(ModInstallScript p_misInstallScript)
    {
      return new Fallout3DependencyStateManager(p_misInstallScript);
    }

    /// <summary>
    ///   The factory method that creates the appropriate parser extension for the specified configuration file version.
    /// </summary>
    /// <param name="p_strVersion">The XML configuration file version for which to return a parser extension.</param>
    /// <returns>
    ///   The appropriate parser extension for the specified configuration file version, or
    ///   <lang langref="null" /> if no extension is available.
    /// </returns>
    public override ParserExtension CreateParserExtension(string p_strVersion)
    {
      switch (p_strVersion)
      {
        case "1.0":
          return new Fallout3Parser10Extension();
        case "2.0":
        case "3.0":
        case "4.0":
        case "5.0":
          return new Fallout3Parser20Extension();
        default:
          return null;
      }
    }

    /// <summary>
    ///   Gets the path to the schema file for the specified configuration file version.
    /// </summary>
    /// <param name="p_strVersion">The XML configuration file version for which to return a parser extension.</param>
    /// <returns>
    ///   The path to the schema file for the specified configuration file version, or
    ///   <lang langref="null" /> if there is no game-specific schema for the specified configuration
    ///   file version.
    /// </returns>
    public override string GetGameSpecificXMLConfigSchemaPath(string p_strVersion)
    {
      return Path.Combine(Program.ProgrammeInfoDirectory, String.Format(@"Fallout3\ModConfig{0}.xsd", p_strVersion));
    }

    #endregion

    #region Command Line Arguments

    /// <summary>
    ///   Return command line help for the arguments provided by the game mode.
    /// </summary>
    /// <remarks>
    ///   This method should only return the text required to describe the arguments. All header,
    ///   footer, and context text is already provided.
    /// </remarks>
    /// <returns>Command line help for the arguments provided by the game mode.</returns>
    public static string GetCommandLineHelp()
    {
      var stbHelp = new StringBuilder();
      stbHelp.AppendLine("*.dat, *.bsa, *.esm, *.esp, *.sdp");
      stbHelp.AppendLine("Open the specified file in the relevent utility");
      stbHelp.AppendLine();
      stbHelp.AppendLine("-setup, -bsa-unpacker, -bsa-creator, -tessnip, -sdp-editor, -install-tweaker");
      stbHelp.AppendLine("Open the specified utility window, without opening the main form where appropriate");
      return stbHelp.ToString();
    }

    /// <summary>
    ///   Handles the command line arguments that run outside of an instance of FOMM.
    /// </summary>
    /// <param name="p_strArgs">The command line arguments that were passed to the programme.</param>
    /// <returns>
    ///   <lang langref="true" /> if at least one of the arguments were handled;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public override bool HandleStandaloneArguments(string[] p_strArgs)
    {
      if (!p_strArgs[0].StartsWith("-") && File.Exists(p_strArgs[0]))
      {
        switch (Path.GetExtension(p_strArgs[0]).ToLowerInvariant())
        {
          case ".dat":
          case ".bsa":
            Application.Run(new BSABrowser(p_strArgs[0]));
            return true;
          case ".sdp":
            Application.Run(new Tools.ShaderEdit.MainForm(p_strArgs[0]));
            return true;
          case ".esp":
          case ".esm":
            Application.Run(new TESsnip(new[]
            {
              p_strArgs[0]
            }));
            return true;
        }
      }
      else
      {
        switch (p_strArgs[0])
        {
          case "-setup":
            bool booNewMutex;
            var mutex = new Mutex(true, "fommMainMutex", out booNewMutex);
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
            Application.Run(new BSABrowser());
            return true;
          case "-bsa-creator":
            Application.Run(new BSACreator());
            return true;
          case "-tessnip":
            Application.Run(new TESsnip());
            return true;
          case "-sdp-editor":
            Application.Run(new Tools.ShaderEdit.MainForm());
            return true;
        }
      }
      return false;
    }

    /// <summary>
    ///   Handles the command line arguments that affect an instance of FOMM.
    /// </summary>
    /// <param name="p_strArgs">The command line arguments that were passed to the programme.</param>
    /// <returns>
    ///   <lang langref="true" /> if at least one of the arguments were handled;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public override bool HandleInAppArguments(string[] p_strArgs)
    {
      if (Array.IndexOf(p_strArgs, "-install-tweaker") != -1)
      {
        Application.Run(new InstallationTweaker());
        return true;
      }
      return false;
    }

    #endregion

    /// <summary>
    ///   Verifies that the given path is a valid working directory for the game mode.
    /// </summary>
    /// <param name="p_strPath">The path to validate as a working directory.</param>
    /// <returns>
    ///   <lang langref="true" /> if the path is a vlid working directory;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public virtual bool VerifyWorkingDirectory(string p_strPath)
    {
      if (String.IsNullOrEmpty(p_strPath))
      {
        return false;
      }

      var strExes = new[]
      {
        Path.Combine(p_strPath, "fallout3.exe"),
        Path.Combine(p_strPath, "fallout3ng.exe")
      };
      var booFound = false;
      foreach (var strExe in strExes)
      {
        if (File.Exists(strExe))
        {
          booFound = true;
          break;
        }
      }
      return booFound;
    }

    /// <summary>
    ///   Sets the working directory for the programme.
    /// </summary>
    /// <remarks>
    ///   This sets the working directory to the Fallout 3 install folder.
    /// </remarks>
    /// <param name="p_strErrorMessage">The out parameter that is set to the error message, if an error occurred.</param>
    /// <returns>
    ///   <lang langref="true" /> if the working directory was successfully set;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public override bool SetWorkingDirectory(out string p_strErrorMessage)
    {
      var strWorkingDirectory = Properties.Settings.Default.fallout3WorkingDirectory;

      if (String.IsNullOrEmpty(strWorkingDirectory) || !Directory.Exists(strWorkingDirectory))
      {
        try
        {
          strWorkingDirectory =
            Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Bethesda Softworks\Fallout3",
                              "Installed Path", null) as string;
        }
        catch
        {
          strWorkingDirectory = null;
        }
      }

      using (var wdfForm = new WorkingDirectorySelectionForm(
        "Could not find Fallout 3 directory." + Environment.NewLine +
        "Fallout's registry entry appears to be missing or incorrect." + Environment.NewLine +
        "Please enter the path to your Fallout 3 game file, or click \"Auto Detect\" to search" +
        " for the install directory. Note that Auto Detection can take several minutes.",
        "Fallout 3 Game Directory:",
        new[]
        {
          "fallout3.exe", "fallout3ng.exe"
        }))
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
      p_strErrorMessage = null;
      return true;
    }

    /// <summary>
    ///   This checks for DLCs isntall by Windows Live, and optionally moves them so
    ///   they are compatible with FOSE.
    /// </summary>
    protected void CheckForDLCs()
    {
      if (Directory.Exists(DLCDirectory) && !Properties.Settings.Default.fallout3IgnoreDLC)
      {
        if (Program.GetFiles(DLCDirectory, "Anchorage.esm", SearchOption.AllDirectories).Length == 1)
        {
          if (!File.Exists("data\\Anchorage.esm") && !File.Exists("data\\Anchorage - Main.bsa") &&
              !File.Exists("data\\Anchorage - Sounds.bsa"))
          {
            var f1 = Directory.GetFiles(DLCDirectory, "Anchorage.esm", SearchOption.AllDirectories);
            var f2 = Directory.GetFiles(DLCDirectory, "Anchorage - Main.bsa", SearchOption.AllDirectories);
            var f3 = Directory.GetFiles(DLCDirectory, "Anchorage - Sounds.bsa", SearchOption.AllDirectories);
            if (f1.Length == 1 && f2.Length == 1 && f3.Length == 1)
            {
              switch (MessageBox.Show("You seem to have bought the DLC Anchorage.\n" +
                                      "Would you like to move it to fallout's data directory to allow for offline use and fose compatibility?\n" +
                                      "Note that this may cause issues with any save games created after it was purchased but before it was moved.\n" +
                                      "Click yes to move, cancel to ignore, and no if you don't want fomm to offer to move any DLC for you again.",
                                      "Question", MessageBoxButtons.YesNoCancel))
              {
                case DialogResult.Yes:
                  File.Move(f1[0], "data\\Anchorage.esm");
                  File.Move(f2[0], "data\\Anchorage - Main.bsa");
                  File.Move(f3[0], "data\\Anchorage - Sounds.bsa");
                  break;
                case DialogResult.No:
                  Properties.Settings.Default.fallout3IgnoreDLC = true;
                  Properties.Settings.Default.Save();
                  break;
              }
            }
          }
        }
        if (Program.GetFiles(DLCDirectory, "ThePitt.esm", SearchOption.AllDirectories).Length == 1)
        {
          if (!File.Exists("data\\ThePitt.esm") && !File.Exists("data\\ThePitt - Main.bsa") &&
              !File.Exists("data\\ThePitt - Sounds.bsa"))
          {
            var f1 = Directory.GetFiles(DLCDirectory, "ThePitt.esm", SearchOption.AllDirectories);
            var f2 = Directory.GetFiles(DLCDirectory, "ThePitt - Main.bsa", SearchOption.AllDirectories);
            var f3 = Directory.GetFiles(DLCDirectory, "ThePitt - Sounds.bsa", SearchOption.AllDirectories);
            if (f1.Length == 1 && f2.Length == 1 && f3.Length == 1)
            {
              switch (MessageBox.Show("You seem to have bought the DLC The Pitt.\n" +
                                      "Would you like to move it to fallout's data directory to allow for offline use and fose compatibility?\n" +
                                      "Note that this may cause issues with any save games created after it was purchased but before it was moved.\n" +
                                      "Click yes to move, cancel to ignore, and no if you don't want fomm to offer to move any DLC for you again.",
                                      "Question", MessageBoxButtons.YesNoCancel))
              {
                case DialogResult.Yes:
                  File.Move(f1[0], "data\\ThePitt.esm");
                  File.Move(f2[0], "data\\ThePitt - Main.bsa");
                  File.Move(f3[0], "data\\ThePitt - Sounds.bsa");
                  break;
                case DialogResult.No:
                  Properties.Settings.Default.fallout3IgnoreDLC = true;
                  Properties.Settings.Default.Save();
                  break;
              }
            }
          }
        }

        if (Program.GetFiles(DLCDirectory, "BrokenSteel.esm", SearchOption.AllDirectories).Length == 1)
        {
          if (!File.Exists("Data\\BrokenSteel.esm"))
          {
            var files = new string[8][];
            files[0] = Directory.GetFiles(DLCDirectory, "BrokenSteel.esm", SearchOption.AllDirectories);
            files[1] = Directory.GetFiles(DLCDirectory, "BrokenSteel - Main.bsa", SearchOption.AllDirectories);
            files[2] = Directory.GetFiles(DLCDirectory, "BrokenSteel - Sounds.bsa", SearchOption.AllDirectories);
            files[3] = Directory.GetFiles(DLCDirectory, "2 weeks later.bik", SearchOption.AllDirectories);
            files[4] = Directory.GetFiles(DLCDirectory, "B09.bik", SearchOption.AllDirectories);
            files[5] = Directory.GetFiles(DLCDirectory, "B27.bik", SearchOption.AllDirectories);
            files[6] = Directory.GetFiles(DLCDirectory, "B28.bik", SearchOption.AllDirectories);
            files[7] = Directory.GetFiles(DLCDirectory, "B29.bik", SearchOption.AllDirectories);
            var missing = false;
            for (var i = 0; i < 8; i++)
            {
              if (files[i].Length != 1)
              {
                missing = true;
                break;
              }
              if ((i < 3 && File.Exists(Path.Combine(PluginsPath, Path.GetFileName(files[i][0])))) ||
                  (i > 4 && File.Exists(Path.Combine(PluginsPath, Path.Combine("Video", Path.GetFileName(files[i][0]))))))
              {
                missing = true;
                break;
              }
            }
            if (!missing)
            {
              switch (MessageBox.Show("You seem to have bought the DLC Broken Steel.\n" +
                                      "Would you like to move it to fallout's data directory to allow for offline use and fose compatibility?\n" +
                                      "Note that this may cause issues with any save games created after it was purchased but before it was moved.\n" +
                                      "Click yes to move, cancel to ignore, and no if you don't want fomm to offer to move any DLC for you again.",
                                      "Question", MessageBoxButtons.YesNoCancel))
              {
                case DialogResult.Yes:
                  if (File.Exists("data\\video\\2 weeks later.bik"))
                  {
                    File.Move("data\\video\\2 weeks later.bik", "data\\Video\\2 weeks later.bik.old");
                  }
                  if (File.Exists("data\\video\\b09.bik"))
                  {
                    File.Move("data\\video\\b09.bik", "data\\Video\\b09.bik.old");
                  }
                  for (var i = 0; i < 3; i++)
                  {
                    File.Move(files[i][0], Path.Combine(PluginsPath, Path.GetFileName(files[i][0])));
                  }
                  for (var i = 3; i < 8; i++)
                  {
                    File.Move(files[i][0],
                              Path.Combine(PluginsPath, Path.Combine("Video", Path.GetFileName(files[i][0]))));
                  }
                  break;
                case DialogResult.No:
                  Properties.Settings.Default.fallout3IgnoreDLC = true;
                  Properties.Settings.Default.Save();
                  break;
              }
            }
          }
        }

        if (Program.GetFiles(DLCDirectory, "PointLookout.esm ", SearchOption.AllDirectories).Length == 1)
        {
          if (!File.Exists("data\\PointLookout.esm ") && !File.Exists("data\\PointLookout - Main.bsa") &&
              !File.Exists("data\\PointLookout - Sounds.bsa"))
          {
            var f1 = Directory.GetFiles(DLCDirectory, "PointLookout.esm", SearchOption.AllDirectories);
            var f2 = Directory.GetFiles(DLCDirectory, "PointLookout - Main.bsa", SearchOption.AllDirectories);
            var f3 = Directory.GetFiles(DLCDirectory, "PointLookout - Sounds.bsa", SearchOption.AllDirectories);
            if (f1.Length == 1 && f2.Length == 1 && f3.Length == 1)
            {
              switch (MessageBox.Show("You seem to have bought the DLC Point lookout.\n" +
                                      "Would you like to move it to fallout's data directory to allow for offline use and fose compatibility?\n" +
                                      "Note that this may cause issues with any save games created after it was purchased but before it was moved.\n" +
                                      "Click yes to move, cancel to ignore, and no if you don't want fomm to offer to move any DLC for you again.",
                                      "Question", MessageBoxButtons.YesNoCancel))
              {
                case DialogResult.Yes:
                  File.Move(f1[0], "data\\PointLookout.esm");
                  File.Move(f2[0], "data\\PointLookout - Main.bsa");
                  File.Move(f3[0], "data\\PointLookout - Sounds.bsa");
                  break;
                case DialogResult.No:
                  Properties.Settings.Default.fallout3IgnoreDLC = true;
                  Properties.Settings.Default.Save();
                  break;
              }
            }
          }
        }

        if (Program.GetFiles(DLCDirectory, "Zeta.esm ", SearchOption.AllDirectories).Length == 1)
        {
          if (!File.Exists("data\\Zeta.esm ") && !File.Exists("data\\Zeta - Main.bsa") &&
              !File.Exists("data\\Zeta - Sounds.bsa"))
          {
            var f1 = Directory.GetFiles(DLCDirectory, "Zeta.esm", SearchOption.AllDirectories);
            var f2 = Directory.GetFiles(DLCDirectory, "Zeta - Main.bsa", SearchOption.AllDirectories);
            var f3 = Directory.GetFiles(DLCDirectory, "Zeta - Sounds.bsa", SearchOption.AllDirectories);
            if (f1.Length == 1 && f2.Length == 1 && f3.Length == 1)
            {
              switch (MessageBox.Show("You seem to have bought the DLC Mothership Zeta.\n" +
                                      "Would you like to move it to fallout's data directory to allow for offline use and fose compatibility?\n" +
                                      "Note that this may cause issues with any save games created after it was purchased but before it was moved.\n" +
                                      "Click yes to move, cancel to ignore, and no if you don't want fomm to offer to move any DLC for you again.",
                                      "Question", MessageBoxButtons.YesNoCancel))
              {
                case DialogResult.Yes:
                  File.Move(f1[0], "data\\Zeta.esm");
                  File.Move(f2[0], "data\\Zeta - Main.bsa");
                  File.Move(f3[0], "data\\Zeta - Sounds.bsa");
                  break;
                case DialogResult.No:
                  Properties.Settings.Default.fallout3IgnoreDLC = true;
                  Properties.Settings.Default.Save();
                  break;
              }
            }
          }
        }
      }
    }

    /// <summary>
    ///   This chaecks for any files that are readonly.
    /// </summary>
    protected void ScanForReadonlyFiles()
    {
      var lstFiles = new List<string>(SettingsFiles.Values);
      foreach (var strPath in m_dicAdditionalPaths.Values)
      {
        if (File.Exists(strPath))
        {
          lstFiles.Add(strPath);
        }
      }
      foreach (var strFile in lstFiles)
      {
        var fifPlugin = new FileInfo(strFile);
        if (fifPlugin.Exists && ((fifPlugin.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly))
        {
          var booAsk = Properties.Settings.Default.falloutNewVegasAskAboutReadOnlySettingsFiles;
          var booMakeWritable = Properties.Settings.Default.falloutNewVegasUnReadOnlySettingsFiles;
          var booRemember = false;
          if (booAsk)
          {
            booMakeWritable =
              (RememberSelectionMessageBox.Show(null,
                                                String.Format(
                                                  "'{0}' is read-only, so it can't be managed by {1}. Would you like to make it not read-only?",
                                                  fifPlugin.Name, Program.ProgrammeAcronym), "Read Only",
                                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, out booRemember) ==
               DialogResult.Yes);
          }
          if (booMakeWritable)
          {
            fifPlugin.Attributes &= ~FileAttributes.ReadOnly;
          }
          if (booRemember)
          {
            Properties.Settings.Default.falloutNewVegasAskAboutReadOnlySettingsFiles = false;
            Properties.Settings.Default.falloutNewVegasUnReadOnlySettingsFiles = booMakeWritable;
            Properties.Settings.Default.Save();
          }
        }
      }
    }

    /// <summary>
    ///   This chaecks for any FOMods that are readonly, and so can't have they're load order changed.
    /// </summary>
    protected void ScanForReadonlyPlugins()
    {
      var difPluginsDirectory = new DirectoryInfo(Program.GameMode.PluginsPath);
      var lstPlugins = new List<FileInfo>(Program.GetFiles(difPluginsDirectory, "*.esp"));
      lstPlugins.AddRange(Program.GetFiles(difPluginsDirectory, "*.esm"));

      foreach (var fifPlugin in lstPlugins)
      {
        if ((fifPlugin.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
        {
          if (
            MessageBox.Show(null,
                            String.Format(
                              "'{0}' is read-only, so its load order cannot be changed. Would you like to make it not read-only?",
                              fifPlugin.Name), "Read Only", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
            DialogResult.Yes)
          {
            fifPlugin.Attributes &= ~FileAttributes.ReadOnly;
          }
        }
      }
    }

    /// <summary>
    ///   Determines if the specified file is a plugin for the game mode.
    /// </summary>
    /// <param name="p_strPath">The path to the file for which it is to be determined if it is a plugin file.</param>
    /// <returns>
    ///   <lang langref="true" /> if the specified file is a plugin file in the game mode;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public override bool IsPluginFile(string p_strPath)
    {
      var strExt = Path.GetExtension(p_strPath).ToLowerInvariant();
      return (strExt == ".esp" || strExt == ".esm");
    }
  }
}