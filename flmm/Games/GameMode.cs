using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Fomm.Commands;
using Fomm.Controls;
using Fomm.Games.Fallout3.Tools.TESsnip;
using Fomm.PackageManager;
using Fomm.PackageManager.XmlConfiguredInstall;
using Fomm.PackageManager.XmlConfiguredInstall.Parsers;

namespace Fomm.Games
{
  /// <summary>
  ///   The base class for game modes.
  /// </summary>
  /// <remarks>
  ///   A Game Mode is a state in which the programme manages plugins for a specific game.
  /// </remarks>
  public abstract class GameMode
  {
    #region Properties

    /// <summary>
    ///   Gets the name of the game whose plugins are being managed.
    /// </summary>
    /// <value>The name of the game whose plugins are being managed.</value>
    public abstract string GameName { get; }

    /// <summary>
    ///   Gets the game launch command.
    /// </summary>
    /// <value>The game launch command.</value>
    public abstract Command<MainForm> LaunchCommand { get; }

    /// <summary>
    ///   Gets the icon used for the plugin file type.
    /// </summary>
    /// <value>The icon used for the plugin file type.</value>
    public abstract Icon PluginFileIcon { get; }

    /// <summary>
    ///   Gets the modDirectory of the GameMode.
    /// </summary>
    /// <value>The modDirectory of the GameMode.</value>
    public abstract string ModDirectory { get; }

    /// <summary>
    ///   Gets the modInfoCacheDirectory of the GameMode.
    /// </summary>
    /// <value>The modInfoCacheDirectory of the GameMode.</value>
    public abstract string ModInfoCacheDirectory { get; }

    /// <summary>
    ///   Gets the directory where installation information is stored for this game mode.
    /// </summary>
    /// <remarks>
    ///   This is where install logs, overwrites, and the like are stored.
    /// </remarks>
    /// <value>The directory where installation information is stored for this game mode.</value>
    public abstract string InstallInfoDirectory { get; }

    /// <summary>
    ///   Gets the directory where overwrites are stored for this game mode.
    /// </summary>
    /// <value>The directory where overwrites are stored for this game mode.</value>
    public string OverwriteDirectory
    {
      get
      {
        var strDirectory = Path.Combine(InstallInfoDirectory, "overwrites");
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
    public abstract IDictionary<string, string> SettingsFiles { get; }

    /// <summary>
    ///   Gets any other paths used in the game mode.
    /// </summary>
    /// <value>Any other paths used in the game mode.</value>
    public abstract IDictionary<string, string> AdditionalPaths { get; }

    /// <summary>
    ///   Gets the <see cref="IPluginFormatProvider" />s provided by the game mode.
    /// </summary>
    /// <value>The <see cref="IPluginFormatProvider" />s provided by the game mode.</value>
    public abstract IList<IPluginFormatProvider> PluginFormatProviders { get; }

    /// <summary>
    ///   Gets the path to the game's save game files.
    /// </summary>
    /// <value>The path to the game's save game files.</value>
    public abstract string SavesPath { get; }

    #region Tool Injection

    /// <summary>
    ///   Gets the list of tools to add to the tools menu.
    /// </summary>
    /// <value>The list of tools to add to the tools menu.</value>
    public abstract IList<Command<MainForm>> Tools { get; }

    /// <summary>
    ///   Gets the list of tools to add to the game settings menu.
    /// </summary>
    /// <value>The list of tools to add to the game settings menu.</value>
    public abstract IList<Command<MainForm>> GameSettingsTools { get; }

    /// <summary>
    ///   Gets the list of tools to add to the right-click menu.
    /// </summary>
    /// <value>The list of tools to add to the right-click menu.</value>
    public abstract IList<Command<MainForm>> RightClickTools { get; }

    /// <summary>
    ///   Gets the list of tools to add to the load order menu.
    /// </summary>
    /// <value>The list of tools to add to the load order menu.</value>
    public abstract IList<Command<MainForm>> LoadOrderTools { get; }

    /// <summary>
    ///   Gets the list of game launch commands.
    /// </summary>
    /// <value>The list of game launch commands.</value>
    public abstract IList<Command<MainForm>> GameLaunchCommands { get; }

    #endregion

    /// <summary>
    ///   Gets the settings pages that privode management of game mode-specific settings.
    /// </summary>
    /// <value>The settings pages that privode management of game mode-specific settings.</value>
    public abstract IList<SettingsPage> SettingsPages { get; }

    /// <summary>
    ///   Gets the path to the game directory were pluings are to be installed.
    /// </summary>
    /// <value>The path to the game directory were pluings are to be installed.</value>
    public abstract string PluginsPath { get; }

    /// <summary>
    ///   Gets the plugin manager for this game mode.
    /// </summary>
    /// <value>The plugin manager for this game mode.</value>
    public abstract PluginManager PluginManager { get; }

    /// <summary>
    ///   Gets the version of the installed game.
    /// </summary>
    /// <value>The version of the installed game.</value>
    public abstract Version GameVersion { get; }

    #endregion

    public abstract void PreInit();

    #region Script

    /// <summary>
    ///   Gets the default script for a mod.
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
    ///   Creates a mod install script for the given <see cref="fomod" />.
    /// </summary>
    /// <param name="p_fomodMod">The mod for which to create an installer script.</param>
    /// <param name="p_mibInstaller">The installer for which the script is being created.</param>
    /// <returns>A mod install script for the given <see cref="fomod" />.</returns>
    public abstract ModInstallScript CreateInstallScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller);

    /// <summary>
    ///   Creates a mod upgrade script for the given <see cref="fomod" />.
    /// </summary>
    /// <param name="p_fomodMod">The mod for which to create an installer script.</param>
    /// <param name="p_mibInstaller">The installer for which the script is being created.</param>
    /// <returns>A mod upgrade script for the given <see cref="fomod" />.</returns>
    public abstract ModInstallScript CreateUpgradeScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller);

    /// <summary>
    ///   Creates a <see cref="DependencyStateManager" /> for the given <see cref="ModInstallScript" />.
    /// </summary>
    /// <param name="p_misInstallScript">
    ///   The <see cref="ModInstallScript" /> for which the
    ///   <see cref="DependencyStateManager" /> is being created.
    /// </param>
    /// <returns>A <see cref="DependencyStateManager" /> for the given <see cref="ModInstallScript" />.</returns>
    public abstract DependencyStateManager CreateDependencyStateManager(ModInstallScript p_misInstallScript);

    /// <summary>
    ///   The factory method that creates the appropriate parser extension for the specified configuration file version.
    /// </summary>
    /// <param name="p_strVersion">The XML configuration file version for which to return a parser extension.</param>
    /// <returns>
    ///   The appropriate parser extension for the specified configuration file version, or
    ///   <lang langref="null" /> if no extension is available.
    /// </returns>
    public abstract ParserExtension CreateParserExtension(string p_strVersion);

    /// <summary>
    ///   The factory method that returns the appropriate parser extension for the specified configuration file version.
    /// </summary>
    /// <param name="p_strVersion">The XML configuration file version for which to return a parser extension.</param>
    /// <returns>The appropriate parser extension for the specified configuration file version.</returns>
    public ParserExtension GetParserExtension(string p_strVersion)
    {
      var pexExtension = CreateParserExtension(p_strVersion);
      return pexExtension ?? new ParserExtension();
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
    public abstract string GetGameSpecificXMLConfigSchemaPath(string p_strVersion);

    /// <summary>
    ///   Gets the path to the schema file for the specified configuration file version.
    /// </summary>
    /// <param name="p_strVersion">The XML configuration file version for which to return a parser extension.</param>
    /// <returns>The path to the schema file for the specified configuration file version.</returns>
    public string GetXMLConfigSchemaPath(string p_strVersion)
    {
      var strSchemaPath = GetGameSpecificXMLConfigSchemaPath(p_strVersion);
      return strSchemaPath ??
             Path.Combine(Program.ProgrammeInfoDirectory, String.Format("ModConfig{0}.xsd", p_strVersion));
    }

    #endregion

    #region Command Line Arguments

    /// <summary>
    ///   Handles the command line arguments that run outside of an instance of FOMM.
    /// </summary>
    /// <param name="p_strArgs">The command line arguments that were passed to the programme.</param>
    /// <returns>
    ///   <lang langref="true" /> if at least one of the arguments were handled;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public abstract bool HandleStandaloneArguments(string[] p_strArgs);

    /// <summary>
    ///   Handles the command line arguments that affect an instance of FOMM.
    /// </summary>
    /// <param name="p_strArgs">The command line arguments that were passed to the programme.</param>
    /// <returns>
    ///   <lang langref="true" /> if at least one of the arguments were handled;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public abstract bool HandleInAppArguments(string[] p_strArgs);

    #endregion

    #region plugin support

    protected struct LoadOrderInfo
    {
      public bool active;
      public int idx;
    }

    protected Dictionary<string, LoadOrderInfo> fullModList = new Dictionary<string, LoadOrderInfo>();

    public void BuildPluginList()
    {
      var i = 0;

      fullModList.Clear();

      foreach (var s in PluginManager.OrderedPluginList)
      {
        LoadOrderInfo loi;
        loi.active = PluginManager.IsPluginActive(Path.Combine(Program.GameMode.PluginsPath, s));
        loi.idx = i;
        fullModList.Add(Path.GetFileName(s).ToLower(), loi);
        i++;
      }
    }

    /*
     * Given a plugin name, evaluates that all required
     * masters are present, active, and preceed the
     * plugin.
     * 
     * Return values:
     * 0 = OK
     * 1 = A required master is missing
     * 2 = A required master is present but disabled
     * 3 = A required master is present and enabled but after this plugin
     * 
    */

    public int getPluginDependencyStatus(string name, bool showMessage = false)
    {
      var ret = 0;

      // Don't check dependency information if plugin is inactive.
      if (PluginManager.IsPluginActive(Path.Combine(Program.GameMode.PluginsPath, name)))
      {
        // Get the list of masters of the queried plugin
        var plgPlugin = new Plugin(Path.Combine(Program.GameMode.PluginsPath, name), true);
        var masters = new List<string>();
        foreach (var sr in ((Record) plgPlugin.Records[0]).SubRecords)
        {
          switch (sr.Name)
          {
            case "MAST":
              masters.Add(sr.GetStrData().ToLower());
              break;
          }
        }

        int i;
        for (i = 0; i < masters.Count; i++)
        {
          if (fullModList.ContainsKey(masters[i]))
          {
            if (fullModList[masters[i]].active)
            {
              if (fullModList[masters[i]].idx > fullModList[name.ToLower()].idx)
              {
                // Master present and active but in wrong order.
                ret = 3;
                if (showMessage)
                {
                  MessageBox.Show("The plugin '" + name + "'  is being loaded before its master '" + masters[i] +
                                  "'.  Fix the load order to continue.");
                }
                break;
              }
            }
            else
            {
              // Master present but inactive
              ret = 2;
              if (showMessage)
              {
                MessageBox.Show("The plugin '" + name + "'  requires master '" + masters[i] +
                                "' To be active.  Activate it or disable this plugin.");
              }
              break;
            }
          }
          else
          {
            // Missing master file
            ret = 1;
            if (showMessage)
            {
              MessageBox.Show("The plugin '" + name + "' is missing a master, '" + masters[i] +
                              "' which it requires.  Deactivate this plugin, or install and activate the missing master.");
            }
            break;
          }
        }
      }

      return ret;
    }

    #endregion

    /// <summary>
    ///   Sets the working directory for the programme.
    /// </summary>
    /// <remarks>
    ///   This is often the directory where the game whose plugins are being managed is installed; however,
    ///   this cannot be assumed.
    /// </remarks>
    /// <param name="p_strErrorMessage">The out parameter that is set to the error message, if an error occurred.</param>
    /// <returns>
    ///   <lang langref="true" /> if the working directory was successfully set;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public abstract bool SetWorkingDirectory(out string p_strErrorMessage);

    /// <summary>
    ///   This initializes the game mode.
    /// </summary>
    /// <remarks>
    ///   This usually performs housekeeping taks and ensures the file system is in a state consistent
    ///   with what the game mode is expecting.
    /// </remarks>
    /// <returns>
    ///   <lang langref="true" /> if the game mode was able to initialize;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public abstract bool Init();

    /// <summary>
    ///   Determines if the specified file is a plugin for the game mode.
    /// </summary>
    /// <param name="p_strPath">The path to the file for which it is to be determined if it is a plugin file.</param>
    /// <returns>
    ///   <lang langref="true" /> if the specified file is a plugin file in the game mode;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public abstract bool IsPluginFile(string p_strPath);
  }
}