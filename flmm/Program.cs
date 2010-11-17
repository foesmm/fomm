/*
 *    Fallout Mod Manager
 *    Copyright (C) 2008, 2009  Timeslip
 *    Copyright (C) 2010  Timeslip, Q
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
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Fomm.PackageManager;
using Fomm.InstallLogUpgraders;
using Fomm.PackageManager.Upgrade;
using SevenZip;
using Microsoft.Win32;
using Fomm.Util;
using System.Collections.Generic;
using Fomm.PackageManager.ModInstallLog;
using Fomm.Games;
using Fomm.Games.Fallout3;
using System.Threading;
using Fomm.Games.FalloutNewVegas;
using System.Text;

namespace Fomm
{
	struct Pair<A, B>
	{
		public A a;
		public B b;

		public Pair(A a, B b) { this.a = a; this.b = b; }

		public A Key { get { return a; } set { a = value; } }
		public B Value { get { return b; } set { b = value; } }

		public override string ToString()
		{
			return a.ToString();
		}
	}

	class fommException : Exception { public fommException(string msg) : base(msg) { } }

	public static class Program
	{
		public const string Version = "0.13.19";
		public static readonly Version MVersion = new Version(Version + ".0");

#if TRACE
		public static string TRACE_FILE = "TraceLog" + DateTime.Now.ToString("yyyyMMddHHmm") + ".txt";
#endif

		private static readonly string m_strExecutableDirectory = Path.GetDirectoryName(Application.ExecutablePath);
		public static readonly string tmpPath = Path.Combine(Path.GetTempPath(), ProgrammeAcronym);

		#region Properties

		/// <summary>
		/// Gets the programme acronym.
		/// </summary>
		/// <remarks>
		/// This is used whe creating temporary files, folders, etc.
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
		/// Gets the path to where per user application data is stored.
		/// </summary>
		/// <value>The path to where per user application data is stored.</value>
		public static string LocalApplicationDataPath
		{
			get
			{
				string strPath = Path.Combine(PersonalDirectory, ProgrammeAcronym);
				if (!Directory.Exists(strPath))
					Directory.CreateDirectory(strPath);
				return strPath;
			}
		}

		/// <summary>
		/// Gets the path to the directory where programme data is stored.
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
		/// Gets the programme's executable directory.
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
		/// Gets the Personal directory of the current user.
		/// </summary>
		/// <remarks>
		/// Typically, this is the Documents folder of the current user.
		/// </remarks>
		/// <value>The Personal directory of the current user.</value>
		public static string PersonalDirectory
		{
			get
			{
				string strPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				if (String.IsNullOrEmpty(strPath))
					return Registry.GetValue(@"HKEY_CURRENT_USER\software\microsoft\windows\currentversion\explorer\user shell folders", "Personal", null).ToString();
				return strPath;
			}
		}

		private static bool monoMode;
		public static bool MonoMode { get { return monoMode; } }

		public static GameMode GameMode = null;

		#endregion

		/// <summary>
		/// Prints command line argument help.
		/// </summary>
		private static void WriteHelp()
		{
			StringBuilder stbHelp = new StringBuilder();
			stbHelp.AppendLine("Command line options:");
			stbHelp.AppendLine();
			stbHelp.AppendLine("*.fomod, *.rar, *.7z, *.zip");
			stbHelp.AppendLine("Open the specified file in the relevent utility");
			stbHelp.AppendLine();
			stbHelp.AppendLine("-mono");
			stbHelp.AppendLine("Run in mono compatibility mode. Disables some features which are known to be broken under mono");
			stbHelp.AppendLine();
			stbHelp.AppendLine("-no-uac-check");
			stbHelp.AppendLine("Don't check for vista UAC issues");

			stbHelp.AppendLine();
			stbHelp.AppendLine("-game <game_name>");
			stbHelp.AppendLine("Run the mod manager in the specified mode. Valid values for <game_name> are:");
			foreach (string strGame in Enum.GetNames(typeof(SupportedGameModes)))
				stbHelp.AppendLine("\t" + strGame);

			string strGameModeHelp = Fallout3GameMode.GetCommandLineHelp();
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
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			if (!Properties.Settings.Default.settingsUpgraded)
			{
				Properties.Settings.Default.Upgrade();
				Properties.Settings.Default.settingsUpgraded = true;
				Properties.Settings.Default.Save();
			}

			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
			if (Array.IndexOf<string>(args, "-mono") != -1) monoMode = true;
#if TRACE
			TRACE_FILE = Path.Combine(LocalApplicationDataPath, TRACE_FILE);
			TextWriterTraceListener twlListener = new TextWriterTraceListener(TRACE_FILE);
			try
			{
				Trace.Listeners.Add(twlListener);
				string msg = DateTime.Now.ToLongDateString() + " - " + DateTime.Now.ToLongTimeString() + Environment.NewLine +
					"Fomm " + Version + (monoMode ? " (Mono)" : "") + Environment.NewLine + "OS version: " + Environment.OSVersion.ToString() +
					Environment.NewLine + Environment.NewLine;
				Trace.WriteLine(msg);
				Trace.WriteLine("Where we currently are (1): " + Path.GetFullPath("."));
				Trace.WriteLine("We know where FOMM lives: " + Application.ExecutablePath);
#endif
			Directory.SetCurrentDirectory(ExecutableDirectory);
			//Style setup
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if (args.Length > 0 && (args[0] == "-?" || args[0] == "/?" || args[0] == "-help"))
			{
				WriteHelp();
				return;
			}

			SupportedGameModes sgmSelectedGame = Properties.Settings.Default.rememberedGameMode;
			bool booChooseGame = true;
			if ((args.Length > 0) && args[0].StartsWith("-"))
			{
				switch (args[0])
				{
					case "-game":
						try
						{
							sgmSelectedGame = (SupportedGameModes)Enum.Parse(typeof(SupportedGameModes), args[1], true);
							booChooseGame = false;
						}
						catch
						{
						}
						break;
				}
			}

			bool booChangeGameMode = false;
			do
			{
				if (booChangeGameMode || (booChooseGame && !Properties.Settings.Default.rememberGameMode))
				{
					GameModeSelector gmsSelector = new GameModeSelector();
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
#if TRACE
					Trace.WriteLine("We are managing mods for: " + GameMode.GameName);
#endif
				Mutex mutex;
				bool booNewMutex;
				string autoLoad = null;

				if (!booChangeGameMode && (args.Length > 0))
				{
					bool booArgsHandled = true;
					if (!args[0].StartsWith("-") && File.Exists(args[0]))
					{
						switch (Path.GetExtension(args[0]).ToLowerInvariant())
						{
							case ".rar":
							case ".7z":
							case ".zip":
							case ".fomod":
								mutex = new System.Threading.Mutex(true, "fommMainMutex", out booNewMutex);
								mutex.Close();
								if (!booNewMutex)
								{
									Messaging.TransmitMessage(args[0]);
									return;
								}
								else
								{
									autoLoad = args[0];
									break;
								}
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
								string strGuid = args[1];
								string strPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
								ProcessStartInfo psiInfo = new ProcessStartInfo(strPath + @"\msiexec.exe", "/x " + strGuid);
								Process.Start(psiInfo);
								return;
							default:
								booArgsHandled = false;
								break;
						}
					}
					if (!booArgsHandled && GameMode.HandleStandaloneArguments(args))
						return;
				}

#if TRACE
					Trace.WriteLine("Creating mutex.");
					Trace.Indent();
#endif
				mutex = new System.Threading.Mutex(true, "fommMainMutex", out booNewMutex);
				if (!booNewMutex)
				{
#if TRACE
						Trace.WriteLine("FOMM is already running.");
#endif
					MessageBox.Show(ProgrammeAcronym + " is already running", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					mutex.Close();
					return;
				}
#if TRACE
					Trace.Unindent();
#endif

				try
				{
					string strErrorMessage = null;
					if (!GameMode.SetWorkingDirectory(out strErrorMessage))
					{
						MessageBox.Show(null, strErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						booChangeGameMode = false;
						if (Properties.Settings.Default.rememberGameMode)
						{
							booChangeGameMode = true;
							Properties.Settings.Default.rememberGameMode = false;
							Properties.Settings.Default.Save();
						}
						continue;
					}

					//Check that we're in fallout's directory and that we have write access
					bool cancellaunch = true;
#if TRACE
						Trace.WriteLine("Check for UAC.");
						Trace.Indent();
#endif
					if (!Properties.Settings.Default.NoUACCheck || Array.IndexOf<string>(args, "-no-uac-check") == -1)
					{
						try
						{
							File.Delete("limited");
							string strVirtualStore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualStore\\");
							strVirtualStore = Path.Combine(strVirtualStore, Directory.GetCurrentDirectory().Remove(0, 3));
							strVirtualStore = Path.Combine(strVirtualStore, "limited");
							if (File.Exists(strVirtualStore)) File.Delete(strVirtualStore);
							FileStream fs = File.Create("limited");
							fs.Close();
							if (File.Exists(strVirtualStore))
							{
#if TRACE
									Trace.WriteLine("UAC is messing us up.");
#endif
								MessageBox.Show("UAC is preventing Fallout mod manager from obtaining write access to fallout's installation directory.\n" +
								"Either right click fomm.exe and check the 'run as administrator' checkbox on the comptibility tab, or disable UAC", "Error");
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
#if TRACE
								Trace.WriteLine("Can't write to Fallout's directory.");
#endif
							MessageBox.Show("Unable to get write permissions for:" + Environment.NewLine + GameMode.PluginsPath + Environment.NewLine + "Please read" + Environment.NewLine + Path.Combine(Program.ProgrammeInfoDirectory,"Readme - fomm.txt") + Environment.NewLine + "for the solution.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
					else
						cancellaunch = false;
#if TRACE
						Trace.Unindent();
						Trace.WriteLine("We set the working directory: " + Path.GetFullPath("."));
						Trace.Unindent();
#endif

					if (cancellaunch) return;

					if (!Directory.Exists(tmpPath)) Directory.CreateDirectory(tmpPath);

					string str7zPath = Path.Combine(Program.ProgrammeInfoDirectory, "7z-32bit.dll");
#if TRACE
						Trace.WriteLine("7z Path: " + str7zPath + " (Exists: " + File.Exists(str7zPath) + ")");
						Trace.Flush();
#endif
					SevenZipCompressor.SetLibraryPath(str7zPath);

#if TRACE
						Trace.WriteLine("Game Mode Specific Initialization:");
						Trace.Indent();
#endif
					if (!GameMode.Init())
						return;
					PermissionsManager.Init();
					InstallLog.Reload();
#if TRACE
					Trace.WriteLine("Install Info Dir: " + GameMode.InstallInfoDirectory);
					Trace.WriteLine("We know where the mods live: " + GameMode.ModDirectory);
					Trace.Unindent();
					Trace.WriteLine("Done Game Mode Specific Initialization.");
					Trace.WriteLine("Install Log Version: " + InstallLog.Current.GetInstallLogVersion());
					Trace.Indent();
#endif
					//check to see if we need to upgrade the install log format
					if (InstallLog.Current.GetInstallLogVersion() < InstallLog.CURRENT_VERSION)
					{
#if TRACE
						Trace.WriteLine("Upgrade to " + InstallLog.CURRENT_VERSION + " required...");
						Trace.Indent();
#endif
						InstallLogUpgrader iluUgrader = new InstallLogUpgrader();
						try
						{
							MessageBox.Show("FOMM needs to upgrade some of its files. This could take a few minutes, depending on how many mods are installed.", "Upgrade Required");
							if (!iluUgrader.UpgradeInstallLog())
							{
#if TRACE
									Trace.WriteLine("Refused.");
#endif
								MessageBox.Show("FOMM needs to upgrade its files before it can run. Please allow the upgrade to complete, or install an older version of FOMM.", "Upgrade Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
								return;
							}
						}
						catch (Exception e)
						{
#if TRACE
								TraceException(e);
#endif
							HandleException(e, "An error occurred while upgrading your log file.", "Upgrade Error");
							return;
						}
#if TRACE
						Trace.Unindent();
						Trace.WriteLine("Done Install Log Upgrade.");
#endif
					}
#if TRACE
						Trace.Unindent();
						Trace.Write("Uninstalling missing FOMods...");
#endif
					//let's uninstall any fomods that have been deleted since we last ran
					IList<FomodInfo> lstMods = InstallLog.Current.GetVersionedModList();
					foreach (FomodInfo fifMod in lstMods)
					{
						string strFomodPath = Path.Combine(GameMode.ModDirectory, fifMod.BaseName + ".fomod");
						if (!File.Exists(strFomodPath))
						{
							string strMessage = "'" + fifMod.BaseName + ".fomod' was deleted without being deactivated. " + Environment.NewLine +
												"If you don't uninstall the FOMod, FOMM will close and you will " +
												"have to put the FOMod back in the mods folder." + Environment.NewLine +
												"Would you like to uninstall the missing FOMod?";
							if (MessageBox.Show(strMessage, "Missing FOMod", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
								return;
							ModUninstaller mduUninstaller = new ModUninstaller(fifMod.BaseName);
							mduUninstaller.Uninstall(true);
						}
					}

					try
					{
#if TRACE
							Trace.WriteLine("Done.");
							Trace.Write("Scanning for upgraded FOMODs...");
#endif
						//check to see if any fomod versions have changed, and whether to upgrade them
						UpgradeScanner upsScanner = new UpgradeScanner();
						upsScanner.Scan();
					}
					catch (Exception e)
					{
#if TRACE
							TraceException(e);
#endif
						HandleException(e, "An error occurred while scanning your fomods for new versions.", "Scan Error");
						return;
					}

#if TRACE
						Trace.WriteLine("Done.");
						Trace.WriteLine("Running Application.");
						Trace.Flush();
#endif
					if (booChangeGameMode || !GameMode.HandleInAppArguments(args))
					{
						try
						{
							MainForm frmMain = new MainForm(autoLoad);
							Application.Run(frmMain);
							booChangeGameMode = frmMain.ChangeGameMode;
						}
						catch (Exception e)
						{
							HandleException(e, "Something bad seems to have happened.", "Error");
						}
					}
#if TRACE
						Trace.Flush();
#endif

					//backup the install log
					if (File.Exists(InstallLog.Current.InstallLogPath))
					{
						string strLogPath = InstallLog.Current.InstallLogPath + ".bak";
						FileInfo fifInstallLog = new FileInfo(InstallLog.Current.InstallLogPath);
						FileInfo fifInstallLogBak = null;
						if (File.Exists(strLogPath))
							fifInstallLogBak = new FileInfo(strLogPath);

						if ((fifInstallLogBak == null) || (fifInstallLogBak.LastWriteTimeUtc != fifInstallLog.LastWriteTimeUtc))
						{
							for (Int32 i = 4; i > 0; i--)
							{
								if (File.Exists(strLogPath + i))
									File.Copy(strLogPath + i, strLogPath + (i + 1), true);
							}
							if (File.Exists(strLogPath))
								File.Copy(strLogPath, strLogPath + "1", true);
							File.Copy(InstallLog.Current.InstallLogPath, InstallLog.Current.InstallLogPath + ".bak", true);
						}
					}

					FileUtil.ForceDelete(tmpPath);
				}
				finally
				{
					if (mutex != null)
						mutex.Close();
				}
			} while (booChangeGameMode);
#if TRACE
			}
			finally
			{
				Trace.Flush();
			}
#endif
		}

		static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
#if TRACE
			Trace.WriteLine("");
			Trace.WriteLine("Unhandled Exception Occurred:");
			Exception ex = e.Exception;
			if (ex != null)
				TraceException(ex);
			else if (e.Exception != null)
				Trace.WriteLine("\tNOT AN EXCEPTION. Error Type: " + e.Exception.GetType());
			else
				Trace.WriteLine("\tNO EXCEPTION.");
#endif
			HandleException(e.Exception, "Something bad seems to have happened.", "Error");
			Application.ExitThread();
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
#if TRACE
			Trace.WriteLine("");
			Trace.WriteLine("Unhandled Exception Occurred:");
			Exception ex = e.ExceptionObject as Exception;
			if (ex != null)
				TraceException(ex);
			else if (e.ExceptionObject != null)
				Trace.WriteLine("\tNOT AN EXCEPTION. Error Type: " + e.ExceptionObject.GetType());
			else
				Trace.WriteLine("\tNO EXCEPTION.");
#endif
			HandleException(e.ExceptionObject as Exception, "Something bad seems to have happened.", "Error");
		}

		static void HandleException(Exception ex, string p_strPromptMessage, string p_strPromptCaption)
		{
			MessageBox.Show(p_strPromptMessage + Environment.NewLine +
							"As long as it wasn't too bad, a crash dump will have been saved in" + Environment.NewLine +
							LocalApplicationDataPath + "\\crashdump.txt" + Environment.NewLine +
							"Please include the contents of that file if you want to make a bug report", p_strPromptCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
#if TRACE
			Trace.WriteLine("");
			Trace.WriteLine("Crashdumping an Exception:");
			if (ex != null)
				TraceException(ex);
			else
				Trace.WriteLine("\tNO EXCEPTION.");
			Trace.Flush();
#endif
			if (ex != null)
			{
				PermissionsManager.CurrentPermissions.Assert();
				string msg = DateTime.Now.ToLongDateString() + " - " + DateTime.Now.ToLongTimeString() + Environment.NewLine +
					"Fomm " + Version + (monoMode ? " (Mono)" : "") + Environment.NewLine + "OS version: " + Environment.OSVersion.ToString() +
					Environment.NewLine + Environment.NewLine + ex.ToString() + Environment.NewLine;
				if (ex is BadImageFormatException)
				{
					BadImageFormatException biex = (BadImageFormatException)ex;
					msg += "File Name:\t" + biex.FileName + Environment.NewLine;
					msg += "Fusion Log:\t" + biex.FusionLog + Environment.NewLine;
				}
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
					msg += ex.ToString() + Environment.NewLine;
				}
				string strDumpFile = Path.Combine(LocalApplicationDataPath, "crashdump.txt");
				File.WriteAllText(strDumpFile, msg);
			}
		}

#if TRACE
		public static void TraceException(Exception e)
		{
			Trace.WriteLine("Error: ");
			Trace.WriteLine(e.Message);
			Trace.WriteLine(e.ToString());
			if (e is BadImageFormatException)
			{
				BadImageFormatException biex = (BadImageFormatException)e;
				Trace.WriteLine("File Name:\t" + biex.FileName);
				Trace.WriteLine("Fusion Log:\t" + biex.FusionLog);
			}
			if (e.InnerException != null)
			{
				Trace.WriteLine("Inner Exception: ");
				Trace.WriteLine(e.InnerException.Message);
				Trace.WriteLine(e.InnerException.ToString());
			}
		}
#endif

		internal static bool IsSafeFileName(string s)
		{
			s = s.Replace('/', '\\');
			if (s.IndexOfAny(Path.GetInvalidPathChars()) != -1) return false;
			if (Path.IsPathRooted(s)) return false;
			if (s.StartsWith(".") || Array.IndexOf<char>(Path.GetInvalidFileNameChars(), s[0]) != -1) return false;
			if (s.Contains("\\..\\")) return false;
			if (s.EndsWith(".") || Array.IndexOf<char>(Path.GetInvalidFileNameChars(), s[s.Length - 1]) != -1) return false;
			return true;
		}

		internal static string CreateTempDirectory()
		{
			string tmp;
			for (int i = 0; i < 32000; i++)
			{
				tmp = Path.Combine(tmpPath, i.ToString());
				if (!Directory.Exists(tmp))
				{
					Directory.CreateDirectory(tmp);
					return tmp + Path.DirectorySeparatorChar;
				}
			}
			throw new fommException("Could not create temp folder because directory is full");
		}

		internal static string[] GetFiles(string path, string pattern) { return GetFiles(path, pattern, SearchOption.TopDirectoryOnly); }
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
		internal static FileInfo[] GetFiles(DirectoryInfo info, string pattern) { return GetFiles(info, pattern, SearchOption.TopDirectoryOnly); }
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