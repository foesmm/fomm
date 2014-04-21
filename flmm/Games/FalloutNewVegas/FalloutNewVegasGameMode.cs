using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Fomm.Commands;
using Fomm.Games.Fallout3;
using Fomm.Games.Fallout3.Tools;
using Fomm.Games.Fallout3.Tools.AutoSorter;
using Fomm.Games.Fallout3.Tools.BSA;
using Fomm.Games.Fallout3.Tools.TESsnip;
using Fomm.Games.FalloutNewVegas.PluginFormatProviders;
using Fomm.Games.FalloutNewVegas.Script;
using Fomm.Games.FalloutNewVegas.Script.XmlConfiguredInstall.Parsers;
using Fomm.Games.FalloutNewVegas.Settings;
using Fomm.Games.FalloutNewVegas.Tools.AutoSorter;
using Fomm.PackageManager;
using Fomm.PackageManager.XmlConfiguredInstall.Parsers;
using Microsoft.Win32;
using SaveForm = Fomm.Games.FalloutNewVegas.Tools.SaveForm;

namespace Fomm.Games.FalloutNewVegas
{
  /// <summary>
  ///   Provides information required for the programme to manage Fallout: New Vegas plugins.
  /// </summary>
  public class FalloutNewVegasGameMode : Fallout3GameMode
  {
    /// <summary>
    ///   This class provides strongly-typed access to this game mode's settings files.
    /// </summary>
    public new class SettingsFilesSet : Fallout3GameMode.SettingsFilesSet
    {
      private const string FODefaultIniPathKey = "FODefaultIniPath";

      #region Properties

      /// <summary>
      ///   Gets or sets the path to the fallout_default.ini file.
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
    ///   Gets the name of the game whose plugins are being managed.
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
    ///   Gets the modDirectory of the GameMode.
    /// </summary>
    /// <value>The modDirectory of the GameMode.</value>
    public override string ModDirectory
    {
      get
      {
        var strModDirectory = Properties.Settings.Default.falloutNewVegasModDirectory;
        if (String.IsNullOrEmpty(strModDirectory))
        {
          throw new Exception("The Mod Directory for Fallout: New Vegas Mods has not been set.");
        }
        if (!Directory.Exists(strModDirectory))
        {
          Directory.CreateDirectory(strModDirectory);
        }
        return strModDirectory;
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
        var result = new Command<MainForm>("Launch Fallout: NV", "Launches plain Fallout: New Vegas.", LaunchGame);
        if (File.Exists("nvse_loader.exe"))
        {
          result = new Command<MainForm>("Launch NVSE", "Launches Fallout: New Vegas using NVSE.", LaunchGame);
        }
        if (File.Exists("fnv4gb.exe"))
        {
          result = new Command<MainForm>("Launch FNV4GB", "Launches Fallout: New Vegas with 4GB Patch.", LaunchGame);
        }
        if (!String.IsNullOrEmpty(Properties.Settings.Default.falloutNewVegasLaunchCommand))
        {
          result = new Command<MainForm>("Launch Custom", "Launches Fallout: New Vegas with custom command.", LaunchGame);
        }
        return result;
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
        var strFalloutEsm = Path.Combine(PluginsPath, "falloutnv.esm");
        return Icon.ExtractAssociatedIcon(strFalloutEsm);
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
        var strDirectory = Properties.Settings.Default.falloutNewVegasInstallInfoDirectory;
        if (String.IsNullOrEmpty(strDirectory))
        {
          throw new Exception("The InstallInfoDirectory for Fallout: New Vegas has not been set.");
        }
        if (!Directory.Exists(strDirectory))
        {
          Directory.CreateDirectory(strDirectory);
        }
        return strDirectory;
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
        if (File.Exists("FalloutNV.exe"))
        {
          return
            new Version(FileVersionInfo.GetVersionInfo("FalloutNV.exe")
                                       .FileVersion.Replace(", ", "."));
        }
        if (File.Exists("FalloutNVng.exe"))
        {
          return
            new Version(FileVersionInfo.GetVersionInfo("FalloutNVng.exe")
                                       .FileVersion.Replace(", ", "."));
        }
        return null;
      }
    }

    /// <summary>
    ///   Gets the path to the per user Fallout: New Vegas data.
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
      if (!Properties.Settings.Default.falloutNewVegasDoneSetup)
      {
        var sfmSetup = new SetupForm();
        if (sfmSetup.ShowDialog() == DialogResult.Cancel)
        {
          return false;
        }
        Properties.Settings.Default.falloutNewVegasDoneSetup = true;
        Properties.Settings.Default.Save();
      }

      ((SettingsFilesSet) SettingsFiles).FODefaultIniPath = Path.Combine(PluginsPath, @"..\fallout_default.ini");
      if (File.Exists("FNVEdit.exe"))
      {
        Tools.Add(new Command<MainForm>("FNVEdit", "Launches FNVEdit, if it is installed.", LaunchFNVEdit));
      }
      Tools.Add(new CheckedCommand<MainForm>("Archive Invalidation", "Toggles Archive Invalidation.",
                                             ArchiveInvalidation.IsActive(), ToggleArchiveInvalidation));

      if (!File.Exists(((SettingsFilesSet) SettingsFiles).FOIniPath))
      {
        MessageBox.Show(
          "You have no Fallout INI file. Please run Fallout: New Vegas to initialize the file before installing any mods or turning on Archive Invalidation.",
          "Missing INI", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }

      ScanForReadonlyPlugins();
      ScanForReadonlyFiles();

      return true;
    }

    /// <summary>
    ///   Creates the plugin manager that will be used by this game mode.
    /// </summary>
    /// <returns>The plugin manager that will be used by this game mode.</returns>
    protected override Fallout3PluginManager CreatePluginManager()
    {
      return new FalloutNewVegasPluginManager();
    }

    /// <summary>
    ///   Creates the settings file set that will be used by this game mode.
    /// </summary>
    /// <returns>The settings file set that will be used by this game mode.</returns>
    protected override Fallout3GameMode.SettingsFilesSet CreateSettingsFileSet()
    {
      return new SettingsFilesSet();
    }

    /// <summary>
    ///   Sets up the plugin format providers for this game mode.
    /// </summary>
    protected override void SetupPluginFormatProviders()
    {
      base.SetupPluginFormatProviders();
      BoldESMPluginFormatProvider = new BoldESMPluginFormatProvider();
      ColorizerPluginFormatProider = new ColorizerPluginFormatProvider();
    }

    /// <summary>
    ///   Sets up the paths for this game mode.
    /// </summary>
    protected override void SetupPaths()
    {
      base.SetupPaths();
      AdditionalPaths["PluginsFile"] =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FalloutNV/plugins.txt");
      AdditionalPaths.Remove("DLCDir");
    }

    /// <summary>
    ///   Gets up the game-specific settings pages.
    /// </summary>
    protected override void SetupSettingsPages()
    {
      var gsp = new GeneralSettingsPage();
      gsp.Text = "Fallout: New Vegas";
      gsp.BackColor = Color.FromKnownColor(KnownColor.Transparent);

      SettingsPages.Add(gsp);
    }

    /// <summary>
    ///   Sets up the launch commands for the game.
    /// </summary>
    protected override void SetupLaunchCommands()
    {
      GameLaunchCommands.Add(new Command<MainForm>("Launch Fallout: New Vegas", "Launches plain Fallout: New Vegas.",
                                                   LaunchFalloutNVPlain));
      if (File.Exists("fnv4gb.exe"))
      {
        GameLaunchCommands.Add(new Command<MainForm>("Launch 4GB Fallout: New Vegas", "Launches Fallout: New Vegas with 4GB Patch.",
                                                     LaunchFalloutNV4GB));
      }

      if (File.Exists("nvse_loader.exe"))
      {
        GameLaunchCommands.Add(new Command<MainForm>("Launch NVSE Fallout: New Vegas", "Launches Fallout: New Vegas with NVSE.",
                                                     LaunchFalloutNVNVSE));
      }

      GameLaunchCommands.Add(new Command<MainForm>("Launch Custom Fallout: New Vegas",
                                                   "Launches Fallout: New Vegas with custom command.",
                                                   LaunchFalloutNVCustom));
    }

    /// <summary>
    ///   Sets up the tools for this game mode.
    /// </summary>
    protected override void SetupTools()
    {
      Tools.Add(new Command<MainForm>("BSA Browser", "Views and unpacks BSA files.", LaunchBSABrowserTool));
      Tools.Add(new Command<MainForm>("BSA Creator", "Creates BSA files.", LaunchBSACreatorTool));
      Tools.Add(new Command<MainForm>("TESsnip", "An ESP/ESM editor.", LaunchTESsnipTool));
      Tools.Add(new Command<MainForm>("Shader Editor", "A shader (SDP) editor.", LaunchShaderEditTool));
      Tools.Add(new Command<MainForm>("CREditor", "Edits critical records in an ESP/ESM.", LaunchCREditorTool));
      Tools.Add(new Command<MainForm>("Conflict Detector",
                                      "Checks for conflicts with mod-author specified critical records.",
                                      LaunchConflictDetector));
      Tools.Add(new Command<MainForm>("Save Games", "Save game info viewer.", LaunchSaveGamesViewer));

      GameSettingsTools.Add(new Command<MainForm>("Graphics Settings", "Changes the graphics settings.",
                                                  LaunchGraphicsSettingsTool));

      RightClickTools.Add(new Command<MainForm>("Open in TESsnip...", "Open the selected plugins in TESsnip.",
                                                LaunchTESsnipToolWithSelectedPlugins));
      RightClickTools.Add(new Command<MainForm>("Open in CREditor...", "Open the selected plugins in TESsnip.",
                                                LaunchCREditorToolWithSelectedPlugins));

      LoadOrderTools.Add(new Command<MainForm>("Load Order Report...",
                                               "Generates a report on the current load order, as compared to the BOSS recomendation.",
                                               LaunchLoadOrderReport));
      LoadOrderTools.Add(new Command<MainForm>("BOSS Auto Sort", "Auto-sorts the plugins using BOSS's masterlist.",
                                               LaunchSortPlugins));
    }

    #endregion

    public override bool updateBOSS()
    {
      var ret = false;

      try
      {
        var bupUpdater = new FalloutNewVegasBOSSUpdater();
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

    #region Tool Launch Methods

    #region Game Launch

    #region Steam Helpers

    /// <summary>
    ///   This ensures that the steam client has loaded.
    /// </summary>
    /// <param name="p_eeaArguments">The main mod management form.</param>
    /// <returns>
    ///   <lang langref="true" /> if Steam is running;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public bool StartSteam(MainForm p_eeaArguments)
    {
      foreach (var clsProcess in Process.GetProcesses())
      {
        if (clsProcess.ProcessName.ToLowerInvariant().Contains("steam"))
        {
          return true;
        }
      }

      var strSteam =
        (string) Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamExe", null);
      if (!String.IsNullOrEmpty(strSteam))
      {
        try
        {
          var psi = new ProcessStartInfo();
          psi.FileName = strSteam;
          psi.WorkingDirectory = Path.GetDirectoryName(strSteam);
          psi.UseShellExecute = false;

          using (var fswClientBlob = new FileSystemWatcher(psi.WorkingDirectory))
          {
            //isn't there a more elegant way of determining if Steam has loaded,
            // rather than counting how many time the ClientRegistry.blob file
            // has been modified?
            fswClientBlob.EnableRaisingEvents = true;
            fswClientBlob.Filter = "ClientRegistry.blob";
            var intSteamClientBlobChangeCount = 0;
            fswClientBlob.Changed += (s, e) =>
            {
              intSteamClientBlobChangeCount++;
            };

            if (Process.Start(psi) != null)
            {
              for (var i = 0; i < 120 && intSteamClientBlobChangeCount < 4; i++)
              {
                Thread.Sleep(500);
              }
              if (intSteamClientBlobChangeCount >= 4)
              {
                return true;
              }
            }
          }
        }
        catch (Exception) {}
      }
      MessageBox.Show(p_eeaArguments,
                      "Unable to start Steam automatically." + Environment.NewLine +
                      "Your game may not launch correctly.", "Steam Missing", MessageBoxButtons.OK,
                      MessageBoxIcon.Warning);
      return false;
    }

    /// <summary>
    ///   This finds the SteamAppId for FO:NV.
    /// </summary>
    /// <returns>The SteamAppId for FO:NV</returns>
    /// <exception cref="Exception">Thrown is the id cannot be found.</exception>
    protected Int32 GetSteamAppId()
    {
      var keyUninstall = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
      var strNames = keyUninstall.GetSubKeyNames();
      foreach (var strKeyName in strNames)
      {
        if (strKeyName.StartsWith("steam app", StringComparison.InvariantCultureIgnoreCase))
        {
          var strDisplayName = (string) keyUninstall.OpenSubKey(strKeyName).GetValue("Displayname");
          if ("fallout: new vegas".Equals(strDisplayName, StringComparison.InvariantCultureIgnoreCase))
          {
            int intAppId;
            if (Int32.TryParse(strKeyName.Split(' ')[2], out intAppId))
            {
              return intAppId;
            }
          }
        }
      }

      var strInstallScriptPath = Path.Combine(Environment.CurrentDirectory, "InstallScript.vdf");
      if (File.Exists(strInstallScriptPath))
      {
        var rgxAppId = new Regex(@"HKEY_LOCAL_MACHINE\\\\Software\\\\Valve\\\\Steam\\\\Apps\\\\(\d+)");
        var strInstallScript = File.ReadAllText(strInstallScriptPath);
        if (rgxAppId.IsMatch(strInstallScript))
        {
          int intAppId;
          if (Int32.TryParse(rgxAppId.Match(strInstallScript).Groups[1].Value, out intAppId))
          {
            return intAppId;
          }
        }
      }
      throw new Exception("Unable to determine Steam App Id for Fallout: New Vegas.");
    }

    #endregion

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
    public void LaunchFalloutNVCustom(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (PrelaunchCheckOrder())
      {
        if (p_eeaArguments.Argument.HasOpenUtilityWindows)
        {
          MessageBox.Show("Please close all utility windows before launching Fallout");
          return;
        }
        var command = Properties.Settings.Default.falloutNewVegasLaunchCommand;
        var args = Properties.Settings.Default.falloutNewVegasLaunchCommandArgs;
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
    ///   Launches the game, with FNV4GB.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchFalloutNV4GB(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (PrelaunchCheckOrder())
      {
        if (!File.Exists("fnv4gb.exe"))
        {
          MessageBox.Show("FNV4GB does not appear to be installed");
          return;
        }
        if (p_eeaArguments.Argument.HasOpenUtilityWindows)
        {
          MessageBox.Show("Please close all utility windows before launching Fallout");
          return;
        }

        try
        {
          var psi = new ProcessStartInfo();
          psi.FileName = "fnv4gb.exe";
          //this configuration force the FO:NV launcher, which
          // ensures FNV4GB loads
          var steamAppId = GetSteamAppId();
          if (steamAppId > 0)
          {
            psi.Arguments = String.Format("-SteamAppId {0}", steamAppId);
          }

          if (psi.EnvironmentVariables.ContainsKey("SteamAppID"))
          {
            psi.EnvironmentVariables.Remove("SteamAppID");
          }
          if (psi.EnvironmentVariables.ContainsKey("SteamGameId"))
          {
            psi.EnvironmentVariables.Remove("SteamGameId");
          }
          psi.UseShellExecute = false;
          psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath("fnv4gb.exe"));
          if (Process.Start(psi) == null)
          {
            MessageBox.Show("Failed to launch 'fnv4gb.exe'");
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Failed to launch 'fnv4gb.exe'\n" + ex.Message);
        }
      }
    }

    /// <summary>
    ///   Launches the game, with NVSE.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchFalloutNVNVSE(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (PrelaunchCheckOrder())
      {
        if (!File.Exists("nvse_loader.exe"))
        {
          MessageBox.Show("NVSE does not appear to be installed");
          return;
        }
        if (p_eeaArguments.Argument.HasOpenUtilityWindows)
        {
          MessageBox.Show("Please close all utility windows before launching Fallout");
          return;
        }

        try
        {
          var psi = new ProcessStartInfo();
          psi.FileName = "nvse_loader.exe";
          //this configuration force the FO:NV launcher, which
          // ensures NVSE loads
          if (psi.EnvironmentVariables.ContainsKey("SteamAppID"))
          {
            psi.EnvironmentVariables.Remove("SteamAppID");
          }
          if (psi.EnvironmentVariables.ContainsKey("SteamGameId"))
          {
            psi.EnvironmentVariables.Remove("SteamGameId");
          }
          psi.UseShellExecute = false;
          psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath("nvse_loader.exe"));
          if (Process.Start(psi) == null)
          {
            MessageBox.Show("Failed to launch 'nvse_loader.exe'");
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Failed to launch 'nvse_loader.exe'\n" + ex.Message);
        }
      }
    }

    /// <summary>
    ///   Launches the game, without NVSE.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public void LaunchFalloutNVPlain(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (PrelaunchCheckOrder())
      {
        if (p_eeaArguments.Argument.HasOpenUtilityWindows)
        {
          MessageBox.Show("Please close all utility windows before launching fallout");
          return;
        }
        var command = File.Exists("falloutNV.exe") ? "falloutNV.exe" : "falloutNVng.exe";
        var booSteamStarted = StartSteam(p_eeaArguments.Argument);

        try
        {
          var psi = new ProcessStartInfo();
          if (booSteamStarted && !psi.EnvironmentVariables.ContainsKey("SteamAppID"))
          {
            psi.EnvironmentVariables.Add("SteamAppID", GetSteamAppId().ToString());
          }
          psi.FileName = command;
          psi.UseShellExecute = false;
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
    ///   Launches the game, using NVSE if present.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public override void LaunchGame(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      var command = Properties.Settings.Default.falloutNewVegasLaunchCommand;
      if (!String.IsNullOrEmpty(command))
      {
        LaunchFalloutNVCustom(p_objCommand, p_eeaArguments);
      }
      else if (File.Exists("fnv4gb.exe"))
      {
        LaunchFalloutNV4GB(p_objCommand, p_eeaArguments);
      }
      else if (File.Exists("nvse_loader.exe"))
      {
        LaunchFalloutNVNVSE(p_objCommand, p_eeaArguments);
      }
      else
      {
        LaunchFalloutNVPlain(p_objCommand, p_eeaArguments);
      }
    }

    #endregion

    #region Tools Menu

    /// <summary>
    ///   Launches FNVEdit, if present.
    /// </summary>
    /// <param name="p_objCommand">The command that is executing.</param>
    /// <param name="p_eeaArguments">
    ///   An <see cref="ExecutedEventArgs
    ///   
    ///   <MainForm>
    ///     "/> containing the
    ///     main mod management form.
    /// </param>
    public virtual void LaunchFNVEdit(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (!File.Exists("FNVEdit.exe"))
      {
        MessageBox.Show(p_eeaArguments.Argument, "Could not find FNVEdit. Please install it.", "Missing FNVEdit",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      try
      {
        var psi = new ProcessStartInfo();
        psi.FileName = "FNVEdit.exe";
        psi.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(psi.FileName));
        if (Process.Start(psi) == null)
        {
          MessageBox.Show("Failed to launch FNVEdit.");
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to launch FNVEdit." + Environment.NewLine + ex.Message);
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
    public override void LaunchSaveGamesViewer(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
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
    public override void ToggleArchiveInvalidation(object p_objCommand, ExecutedEventArgs<MainForm> p_eeaArguments)
    {
      if (FalloutNewVegas.Tools.ArchiveInvalidation.Update())
      {
        ((CheckedCommand<MainForm>) p_objCommand).IsChecked =
          FalloutNewVegas.Tools.ArchiveInvalidation.IsActive();
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
        return base.DefaultCSharpScript.Replace(": Fallout3BaseScript", ": FalloutNewVegasBaseScript");
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
      return new FalloutNewVegasModInstallScript(p_fomodMod, p_mibInstaller);
    }

    /// <summary>
    ///   Creates a mod upgrade script for the given <see cref="fomod" />.
    /// </summary>
    /// <param name="p_fomodMod">The mod for which to create an installer script.</param>
    /// <param name="p_mibInstaller">The installer for which the script is being created.</param>
    /// <returns>A mod upgrade script for the given <see cref="fomod" />.</returns>
    public override ModInstallScript CreateUpgradeScript(fomod p_fomodMod, ModInstallerBase p_mibInstaller)
    {
      return new FalloutNewVegasModUpgradeScript(p_fomodMod, p_mibInstaller);
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
        case "5.0":
          return new FalloutNewVegasParser50Extension();
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
      return Path.Combine(Program.ProgrammeInfoDirectory, String.Format(@"FalloutNV\ModConfig{0}.xsd", p_strVersion));
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
    public new static string GetCommandLineHelp()
    {
      var stbHelp = new StringBuilder();
      stbHelp.AppendLine("*.dat, *.bsa, *.esm, *.esp, *.sdp");
      stbHelp.AppendLine("Open the specified file in the relevent utility");
      stbHelp.AppendLine();
      stbHelp.AppendLine("-setup, -bsa-unpacker, -bsa-creator, -tessnip, -sdp-editor");
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
            Application.Run(new Fallout3.Tools.ShaderEdit.MainForm(p_strArgs[0]));
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
            Application.Run(new Fallout3.Tools.ShaderEdit.MainForm());
            return true;
        }
      }
      return false;
    }

    /// <summary>
    ///   Handles the command line arguments that affect an instance of the mod manager.
    /// </summary>
    /// <param name="p_strArgs">The command line arguments that were passed to the programme.</param>
    /// <returns>
    ///   <lang langref="true" /> if at least one of the arguments were handled;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public override bool HandleInAppArguments(string[] p_strArgs)
    {
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
    public override bool VerifyWorkingDirectory(string p_strPath)
    {
      if (String.IsNullOrEmpty(p_strPath))
      {
        return false;
      }

      var strExes = new[]
      {
        Path.Combine(p_strPath, "falloutNV.exe"),
        Path.Combine(p_strPath, "falloutNVng.exe")
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
      var strWorkingDirectory = Properties.Settings.Default.falloutNewVegasWorkingDirectory;

      if (String.IsNullOrEmpty(strWorkingDirectory) || !Directory.Exists(strWorkingDirectory))
      {
        try
        {
          strWorkingDirectory =
            Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Bethesda Softworks\FalloutNV",
                              "Installed Path", null) as string;
        }
        catch
        {
          strWorkingDirectory = null;
        }
      }

      using (var wdfForm = new WorkingDirectorySelectionForm(
        "Could not find Fallout: New Vegas directory." + Environment.NewLine +
        "Fallout's registry entry appears to be missing or incorrect." + Environment.NewLine +
        "Please enter the path to your Fallout: New Vegas game file, or click \"Auto Detect\" to search" +
        " for the install directory. Note that Auto Detection can take several minutes.",
        "Fallout: New Vegas Game Directory:",
        new[]
        {
          "falloutNV.exe", "falloutNVng.exe"
        }))
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
      p_strErrorMessage = null;
      return true;
    }
  }
}
