/*
 *    Fallout Mod Manager
 *    Copyright (C) 2008, 2009  Timeslip
 *    Copyright (C) 2010  Timeslip, Q
 *    Copyright (C) 2011, 2012, 2013  Prideslayer
 *    Copyright (C) 2014  Prideslayer, Niveus Everto
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Fomm.Games;
using Fomm.Games.Fallout3;
using Fomm.Games.FalloutNewVegas;
using Fomm.InstallLogUpgraders;
using Fomm.PackageManager;
using Fomm.PackageManager.ModInstallLog;
using Fomm.PackageManager.Upgrade;
using Fomm.Properties;
using Fomm.Util;
using Microsoft.Win32;
using SevenZip;

namespace Fomm
{
  internal struct Pair<A, B>
  {
    public A a;
    public B b;

    public Pair(A a, B b)
    {
      this.a = a;
      this.b = b;
    }

    public A Key
    {
      get
      {
        return a;
      }
      set
      {
        a = value;
      }
    }

    public B Value
    {
      get
      {
        return b;
      }
      set
      {
        b = value;
      }
    }

    public override string ToString()
    {
      return a.ToString();
    }
  }

  internal class fommException : Exception
  {
    public fommException(string msg) : base(msg) {}
  }

  public static class Program
  {
    // @todo: restore Fomm.ProductInfo.Version
    public const string Version = Fomm.ProductInfo.Version;
    public static readonly Version MVersion = new Version(Version);

    private static readonly string m_strExecutableDirectory = Path.GetDirectoryName(Application.ExecutablePath);
    public static readonly string tmpPath = Path.Combine(Path.GetTempPath(), ProgrammeAcronym);

    #region Properties

    /// <summary>
    ///   Gets the programme acronym.
    /// </summary>
    /// <remarks>
    ///   This is used whe creating temporary files, folders, etc.
    /// </remarks>
    /// <value>The programme acronym.</value>
    public static string ProgrammeAcronym
    {
      get
      {
        return "FOMM";
      }
    }

    /// <summary>
    ///   Gets the path to where per user application data is stored.
    /// </summary>
    /// <value>The path to where per user application data is stored.</value>
    public static string LocalApplicationDataPath
    {
      get
      {
        var strPath = Path.Combine(PersonalDirectory, ProgrammeAcronym);
        if (!Directory.Exists(strPath))
        {
          Directory.CreateDirectory(strPath);
        }
        return strPath;
      }
    }

    /// <summary>
    ///   Gets the path to the directory where programme data is stored.
    /// </summary>
    /// <value>The path to the directory where programme data is stored.</value>
    public static string ProgrammeInfoDirectory
    {
      get
      {
        return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "data");
      }
    }

    /// <summary>
    ///   Gets the programme's executable directory.
    /// </summary>
    /// <value>The programme's executable directory.</value>
    public static string ExecutableDirectory
    {
      get
      {
        return m_strExecutableDirectory;
      }
    }

    /// <summary>
    ///   Gets the Personal directory of the current user.
    /// </summary>
    /// <remarks>
    ///   Typically, this is the Documents folder of the current user.
    /// </remarks>
    /// <value>The Personal directory of the current user.</value>
    public static string PersonalDirectory
    {
      get
      {
        var strPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        if (String.IsNullOrEmpty(strPath))
        {
          return
            Registry.GetValue(
              @"HKEY_CURRENT_USER\software\microsoft\windows\currentversion\explorer\user shell folders", "Personal",
              null).ToString();
        }
        return strPath;
      }
    }

    public static bool MonoMode { get; private set; }

    public static GameMode GameMode = null;

    #endregion

    /// <summary>
    ///   Prints command line argument help.
    /// </summary>
    private static void WriteHelp()
    {
      var stbHelp = new StringBuilder();
      stbHelp.AppendLine("Command line options:");
      stbHelp.AppendLine();
      stbHelp.AppendLine("*.fomod, *.rar, *.7z, *.zip");
      stbHelp.AppendLine("Open the specified file in the relevent utility");
      stbHelp.AppendLine();
      stbHelp.AppendLine("-mono");
      stbHelp.AppendLine(
        "Run in mono compatibility mode. Disables some features which are known to be broken under mono");
      stbHelp.AppendLine();
      stbHelp.AppendLine("-no-uac-check");
      stbHelp.AppendLine("Don't check for vista UAC issues");

      stbHelp.AppendLine();
      stbHelp.AppendLine("-game <game_name>");
      stbHelp.AppendLine("Run the mod manager in the specified mode. Valid values for <game_name> are:");
      foreach (var strGame in Enum.GetNames(typeof (SupportedGameModes)))
      {
        stbHelp.AppendLine("\t" + strGame);
      }

      var strGameModeHelp = Fallout3GameMode.GetCommandLineHelp();
      if (!String.IsNullOrEmpty(strGameModeHelp))
      {
        stbHelp.AppendLine();
        stbHelp.AppendLine("When -game Fallout3 is specified:");
        stbHelp.AppendLine(strGameModeHelp);
      }

      strGameModeHelp = FalloutNewVegasGameMode.GetCommandLineHelp();
      if (!String.IsNullOrEmpty(strGameModeHelp))
      {
        stbHelp.AppendLine();
        stbHelp.AppendLine("When -game FalloutNV is specified:");
        stbHelp.AppendLine(strGameModeHelp);
      }

      MessageBox.Show(stbHelp.ToString(), "Help");
    }

    /// <summary>
    ///   The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      if (!Settings.Default.settingsUpgraded)
      {
        Settings.Default.Upgrade();
        Settings.Default.settingsUpgraded = true;
        Settings.Default.Save();
      }

      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
      Application.ThreadException += Application_ThreadException;
      if (Array.IndexOf(args, "-mono") != -1)
      {
        MonoMode = true;
      }
      Directory.SetCurrentDirectory(ExecutableDirectory);
      //Style setup
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      if (args.Length > 0 && (args[0] == "-?" || args[0] == "/?" || args[0] == "-help"))
      {
        WriteHelp();
        return;
      }

      var sgmSelectedGame = Settings.Default.rememberedGameMode;
      var booChooseGame = true;
      if ((args.Length > 0) && args[0].StartsWith("-"))
      {
        switch (args[0])
        {
          case "-game":
            try
            {
              sgmSelectedGame = (SupportedGameModes) Enum.Parse(typeof (SupportedGameModes), args[1], true);
              booChooseGame = false;
            }
            catch {}
            break;
        }
      }

      var booChangeGameMode = false;
      do
      {
        if (booChangeGameMode || (booChooseGame && !Settings.Default.rememberGameMode))
        {
          var gmsSelector = new GameModeSelector();
          gmsSelector.ShowDialog();
          sgmSelectedGame = gmsSelector.SelectedGameMode;
        }
        switch (sgmSelectedGame)
        {
          case SupportedGameModes.Fallout3:
            GameMode = new Fallout3GameMode();
            break;

          case SupportedGameModes.FalloutNV:
            GameMode = new FalloutNewVegasGameMode();
            break;

          default:
            MessageBox.Show("Unrecognized game selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Mutex mutex;
        bool booNewMutex;
        string autoLoad = null;

        if (!booChangeGameMode && (args.Length > 0))
        {
          var booArgsHandled = true;
          if (!args[0].StartsWith("-") && File.Exists(args[0]))
          {
            switch (Path.GetExtension(args[0]).ToLowerInvariant())
            {
              case ".rar":
              case ".7z":
              case ".zip":
              case ".fomod":
                mutex = new Mutex(true, "fommMainMutex", out booNewMutex);
                mutex.Close();
                if (!booNewMutex)
                {
                  Messaging.TransmitMessage(args[0]);
                  return;
                }
                autoLoad = args[0];
                break;
              default:
                booArgsHandled = false;
                break;
            }
          }
          else
          {
            switch (args[0])
            {
              case "-u":
                var strGuid = args[1];
                var strPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
                var psiInfo = new ProcessStartInfo(strPath + @"\msiexec.exe", "/x " + strGuid);
                Process.Start(psiInfo);
                return;
              default:
                booArgsHandled = false;
                break;
            }
          }
          if (!booArgsHandled && GameMode.HandleStandaloneArguments(args))
          {
            return;
          }
        }

        mutex = new Mutex(true, "fommMainMutex", out booNewMutex);
        if (!booNewMutex)
        {
          MessageBox.Show(ProgrammeAcronym + " is already running", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          mutex.Close();
          return;
        }

        try
        {
          string strErrorMessage;
          if (!GameMode.SetWorkingDirectory(out strErrorMessage))
          {
            MessageBox.Show(null, strErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            booChangeGameMode = false;
            if (Settings.Default.rememberGameMode)
            {
              booChangeGameMode = true;
              Settings.Default.rememberGameMode = false;
              Settings.Default.Save();
            }
            continue;
          }

          GameMode.PreInit();

          //Check that we're in fallout's directory and that we have write access
          var cancellaunch = true;
          if (!Settings.Default.NoUACCheck || Array.IndexOf(args, "-no-uac-check") == -1)
          {
            try
            {
              File.Delete("limited");
              var strVirtualStore =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualStore\\");
              strVirtualStore = Path.Combine(strVirtualStore, Directory.GetCurrentDirectory().Remove(0, 3));
              strVirtualStore = Path.Combine(strVirtualStore, "limited");
              if (File.Exists(strVirtualStore))
              {
                File.Delete(strVirtualStore);
              }
              var fs = File.Create("limited");
              fs.Close();
              if (File.Exists(strVirtualStore))
              {
                MessageBox.Show(
                  "UAC is preventing Fallout mod manager from obtaining write access to fallout's installation directory.\n" +
                  "Either right click fomm.exe and check the 'run as administrator' checkbox on the comptibility tab, or disable UAC",
                  "Error");
                File.Delete("limited");
              }
              else
              {
                File.Delete("limited");
                cancellaunch = false;
              }
            }
            catch
            {
              MessageBox.Show(
                "Unable to get write permissions for:" + Environment.NewLine + GameMode.PluginsPath +
                Environment.NewLine + "Please read" + Environment.NewLine +
                Path.Combine(ProgrammeInfoDirectory, "Readme - fomm.txt") + Environment.NewLine +
                "for the solution.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
          }
          else
          {
            cancellaunch = false;
          }

          if (cancellaunch)
          {
            return;
          }

          if (!Directory.Exists(tmpPath))
          {
            Directory.CreateDirectory(tmpPath);
          }

          var str7zPath = "";
          
          if (Environment.Is64BitProcess)
          {
            str7zPath = Path.Combine(ProgrammeInfoDirectory, "7z-64bit.dll");
          }
          else
          {
            str7zPath = Path.Combine(ProgrammeInfoDirectory, "7z-32bit.dll");
          }
          SevenZipBase.SetLibraryPath(str7zPath);

          if (!GameMode.Init())
          {
            return;
          }
          PermissionsManager.Init();

          //check to see if we need to upgrade the install log format
          if (InstallLog.Current.GetInstallLogVersion() != InstallLog.CURRENT_VERSION)
          {
            var iluUgrader = new InstallLogUpgrader();
            try
            {
              MessageBox.Show(
                "FOMM needs to upgrade some of its files. This could take a few minutes, depending on how many mods are installed.",
                "Upgrade Required");
              if (!iluUgrader.UpgradeInstallLog())
              {
                MessageBox.Show(
                  "FOMM needs to upgrade its files before it can run. Please allow the upgrade to complete, or install an older version of FOMM.",
                  "Upgrade Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
              }
            }
            catch (Exception e)
            {
              HandleException(e, "An error occurred while upgrading your log file.", "Upgrade Error");
              return;
            }
          }

          InstallLog.Reload();

          //let's uninstall any fomods that have been deleted since we last ran
          var lstMods = InstallLog.Current.GetVersionedModList();
          foreach (var fifMod in lstMods)
          {
            var strFomodPath = Path.Combine(GameMode.ModDirectory, fifMod.BaseName + ".fomod");
            if (!File.Exists(strFomodPath))
            {
              var strMessage = "'" + fifMod.BaseName + ".fomod' was deleted without being deactivated. " +
                               Environment.NewLine +
                               "If you don't uninstall the FOMod, FOMM will close and you will " +
                               "have to put the FOMod back in the mods folder." + Environment.NewLine +
                               "Would you like to uninstall the missing FOMod?";
              if (MessageBox.Show(strMessage, "Missing FOMod", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) ==
                  DialogResult.No)
              {
                return;
              }
              var mduUninstaller = new ModUninstaller(fifMod.BaseName);
              mduUninstaller.Uninstall(true);
            }
          }

          try
          {
            //check to see if any fomod versions have changed, and whether to upgrade them
            var upsScanner = new UpgradeScanner();
            upsScanner.Scan();
          }
          catch (Exception e)
          {
            HandleException(e, "An error occurred while scanning your fomods for new versions.", "Scan Error");
            return;
          }

          if (booChangeGameMode || !GameMode.HandleInAppArguments(args))
          {
            try
            {
              var frmMain = new MainForm(autoLoad);
              frmMain.Text += " (" + MVersion + ") - " + GameMode.GameName;

              Application.Run(frmMain);
              booChangeGameMode = frmMain.ChangeGameMode;
            }
            catch (Exception e)
            {
              HandleException(e, "Something bad seems to have happened.", "Error");
            }
          }

          //backup the install log
          if (File.Exists(InstallLog.Current.InstallLogPath))
          {
            var strLogPath = InstallLog.Current.InstallLogPath + ".bak";
            var fifInstallLog = new FileInfo(InstallLog.Current.InstallLogPath);
            FileInfo fifInstallLogBak = null;
            if (File.Exists(strLogPath))
            {
              fifInstallLogBak = new FileInfo(strLogPath);
            }

            if ((fifInstallLogBak == null) || (fifInstallLogBak.LastWriteTimeUtc != fifInstallLog.LastWriteTimeUtc))
            {
              for (var i = 4; i > 0; i--)
              {
                if (File.Exists(strLogPath + i))
                {
                  File.Copy(strLogPath + i, strLogPath + (i + 1), true);
                }
              }
              if (File.Exists(strLogPath))
              {
                File.Copy(strLogPath, strLogPath + "1", true);
              }
              File.Copy(InstallLog.Current.InstallLogPath, InstallLog.Current.InstallLogPath + ".bak", true);
            }
          }

          FileUtil.ForceDelete(tmpPath);
        }
        finally
        {
          if (mutex != null)
          {
            mutex.Close();
          }
        }
      }
      while (booChangeGameMode);
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      HandleException(e.Exception, "Something bad seems to have happened.", "Error");
      Application.ExitThread();
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      HandleException(e.ExceptionObject as Exception, "Something bad seems to have happened.", "Error");
    }

    private static void HandleException(Exception ex, string p_strPromptMessage, string p_strPromptCaption)
    {
      MessageBox.Show(p_strPromptMessage + Environment.NewLine +
                      "As long as it wasn't too bad, a crash dump will have been saved in" + Environment.NewLine +
                      LocalApplicationDataPath + "\\crashdump.txt" + Environment.NewLine +
                      "Please include the contents of that file if you want to make a bug report", p_strPromptCaption,
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
      if (ex != null)
      {
        if (PermissionsManager.IsInitialized)
        {
          PermissionsManager.CurrentPermissions.Assert();
        }
        var msg = DateTime.Now.ToLongDateString() + " - " + DateTime.Now.ToLongTimeString() + Environment.NewLine +
                  "Fomm " + Version + (MonoMode ? " (Mono)" : "") + Environment.NewLine + "OS version: " +
                  Environment.OSVersion +
                  Environment.NewLine + Environment.NewLine + ex + Environment.NewLine;
        if (ex is BadImageFormatException)
        {
          var biex = (BadImageFormatException) ex;
          msg += "File Name:\t" + biex.FileName + Environment.NewLine;
          msg += "Fusion Log:\t" + biex.FusionLog + Environment.NewLine;
        }
        while (ex.InnerException != null)
        {
          ex = ex.InnerException;
          msg += "Inner Exception:" + Environment.NewLine + ex + Environment.NewLine;
        }
        var strDumpFile = Path.Combine(LocalApplicationDataPath, "crashdump.txt");
        File.WriteAllText(strDumpFile, msg);
      }
    }

    internal static bool IsSafeFileName(string s)
    {
      s = s.Replace('/', '\\');
      if (s.IndexOfAny(Path.GetInvalidPathChars()) != -1)
      {
        return false;
      }
      if (Path.IsPathRooted(s))
      {
        return false;
      }
      if (s.StartsWith(".") || Array.IndexOf(Path.GetInvalidFileNameChars(), s[0]) != -1)
      {
        return false;
      }
      if (s.Contains("\\..\\"))
      {
        return false;
      }
      if (s.EndsWith(".") || Array.IndexOf(Path.GetInvalidFileNameChars(), s[s.Length - 1]) != -1)
      {
        return false;
      }
      return true;
    }

    internal static string CreateTempDirectory()
    {
      for (var i = 0; i < 32000; i++)
      {
        var tmp = Path.Combine(tmpPath, i.ToString());
        if (!Directory.Exists(tmp))
        {
          Directory.CreateDirectory(tmp);
          return tmp + Path.DirectorySeparatorChar;
        }
      }
      throw new fommException("Could not create temp folder because directory is full");
    }

    internal static string[] GetFiles(string path, string pattern)
    {
      return GetFiles(path, pattern, SearchOption.TopDirectoryOnly);
    }

    internal static string[] GetFiles(string path, string pattern, SearchOption option)
    {
      try
      {
        return Directory.GetFiles(path, pattern, option);
      }
      catch (IOException)
      {
        return new string[0];
      }
    }

    internal static FileInfo[] GetFiles(DirectoryInfo info, string pattern)
    {
      return GetFiles(info, pattern, SearchOption.TopDirectoryOnly);
    }

    internal static FileInfo[] GetFiles(DirectoryInfo info, string pattern, SearchOption option)
    {
      try
      {
        return info.GetFiles(pattern, option);
      }
      catch (IOException)
      {
        return new FileInfo[0];
      }
    }
  }
}
