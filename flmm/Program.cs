/*
 *    Fallout Mod Manager
 *    Copyright (C) 2008, 2009  Timeslip
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
		public const string Version = "0.12.5";
		public static readonly Version MVersion = new Version(Version + ".0");
		/*private static string typefromint(int i, bool name) {
			switch(i) {
			case 0x00:
				if(name) return "String"; else return "string";
			case 0x01:
				if(name) return "integer"; else return "int";
			case 0x02:
				if(name) return "float"; else return "float";
			case 0x03:
				if(name) return "object id"; else return "ref";
			case 0x04:
				if(name) return "object reference ID"; else return "ref";
			case 0x05:
				if(name) return "Actor value"; else return "short";
			case 0x06:
				if(name) return "Actor"; else return "ref";
			case 0x07:
				if(name) return "spell item"; else return "ref";
			case 0x08:
				if(name) return "axis"; else return "axis";
			case 0x09:
				if(name) return "Cell"; else return "ref";
			case 0x0a:
				if(name) return "Animation group"; else return "ref";
			case 0x0b:
				if(name) return "magic item"; else return "ref";
			case 0x0c:
				if(name) return "Sound"; else return "ref";
			case 0x0d:
				if(name) return "Topic"; else return "ref";
			case 0x0e:
				if(name) return "Quest"; else return "ref";
			case 0x0f:
				if(name) return "Race"; else return "ref";
			case 0x10:
				if(name) return "Class"; else return "ref";
			case 0x11:
				if(name) return "Fraction"; else return "ref";
			case 0x12:
				if(name) return "Gender"; else return "sex";
			case 0x13:
				if(name) return "Global"; else return "ref";
			case 0x14:
				if(name) return "Furniture"; else return "ref";
			case 0x15:
				if(name) return "object id"; else return "ref";
			case 0x16:
				if(name) return "Variable name"; else return "string";
			case 0x17:
				if(name) return "Stage"; else return "short";
			case 0x18:
				if(name) return "Map marker"; else return "ref";
			case 0x19:
				if(name) return "actor base"; else return "ref";
			case 0x1a:
				if(name) return "Container"; else return "ref";
			case 0x1b:
				if(name) return "World space"; else return "ref";
			case 0x1c:
				if(name) return "Crime type"; else return "short";
			case 0x1d:
				if(name) return "Package"; else return "ref";
			case 0x1e:
				if(name) return "Combat style"; else return "ref";
			case 0x1f:
				if(name) return "Magic effect"; else return "ref";
			case 0x20:
				if(name) return "Form type"; else return "ref";
			case 0x21:
				if(name) return "Weather ID"; else return "ref";
			case 0x23:
				if(name) return "Owner"; else return "ref";
			case 0x24:
				if(name) return "Effect shader ID"; else return "ref";
			case 0x25:
				if(name) return "FormList"; else return "ref";
			case 0x27:
				if(name) return "Perk"; else return "ref";
			case 0x28:
				if(name) return "Note"; else return "ref";
			case 0x29:
				if(name) return "Misc stat"; else return "int";
			case 0x2a:
				if(name) return "Imagespace Modifier ID"; else return "ref";
			case 0x2b:
				if(name) return "Imagespace"; else return "ref";
			case 0x2e:
				if(name) return "Voice type"; else return "ref";
			case 0x2f:
				if(name) return "Encounter zone"; else return "ref";
			case 0x30:
				if(name) return "Idle form"; else return "ref";
			case 0x31:
				if(name) return "Message"; else return "ref";
			case 0x32:
				if(name) return "object ID"; else return "ref";
			case 0x33:
				if(name) return "Alignment"; else return "ref";
			case 0x34:
				if(name) return "Equip type"; else return "ref";
			case 0x35:
				if(name) return "object ID"; else return "ref";
			case 0x36:
				if(name) return "Music"; else return "ref";
			case 0x37:
				if(name) return "Crittical stage"; else return "ref";
			default:
				if(name) return "!!!Unknown!!!"; else return "ref";
			}
		}
         
		 private static void DumpFunctions() {
			StreamWriter sw=new StreamWriter("Functions.xml");
			System.Collections.Generic.List<string> args=new System.Collections.Generic.List<string>();
			string[] lines=File.ReadAllLines("functiondump.txt");
			for(int i=0;i<lines.Length;i++) {
				string[] func=lines[i].Split('\t');
				sw.Write("<Func name=\""+func[1]+"\" opcode=\""+func[0]+"\" ");
				if(func.Length>2&&func[2]!="") sw.Write("short=\""+func[2]+"\" ");
				if(func.Length>3&&func[3]!="") sw.Write("desc=\""+func[3].Replace('"', '\'')+"\" ");
				if(func[1].ToLowerInvariant()=="showmessage") sw.Write("paddingbytes=\"6\" ");
				if(i+1<lines.Length&&lines[i+1][0]=='\t') {
					bool optional=false;
					int opcount=0;
					while(i+1<lines.Length&&lines[i+1][0]=='\t') {
						i++;
						func=lines[i].Split('\t');
						if(func[2]=="1"&&!optional) {
							optional=true;
							sw.Write("requiredargs=\""+opcount+"\" ");
						}
						opcount++;
						int argtype=int.Parse(func[1], System.Globalization.NumberStyles.AllowHexSpecifier, null);
						args.Add("  <Arg name=\""+typefromint(argtype, true)+"\" type=\""+typefromint(argtype, false)+"\" />");
					}
					sw.WriteLine(">");
					foreach(string s in args) sw.WriteLine(s);
					sw.WriteLine("</Func>");
					args.Clear();
				} else sw.WriteLine(" />");
			}
			sw.Close();
		 }
		 */

#if TRACE
		public static string TRACE_FILE = "TraceLog" + DateTime.Now.ToString("yyyyMMddHHmm") + ".txt";
#endif

		public static readonly string tmpPath = Path.Combine(Path.GetTempPath(), "fomm");
		public static readonly string exeDir = Path.GetDirectoryName(Application.ExecutablePath);
		public static readonly string Fallout3SaveDir = Path.Combine(String.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)) ? Registry.GetValue(@"HKEY_CURRENT_USER\software\microsoft\windows\currentversion\explorer\user shell folders", "Personal", null).ToString() : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My games\\Fallout3");
		public static readonly string FOIniPath = Path.Combine(Fallout3SaveDir, "Fallout.ini");
		public static readonly string FOPrefsIniPath = Path.Combine(Fallout3SaveDir, "FalloutPrefs.ini");
		public static readonly string GeckIniPath = Path.Combine(Fallout3SaveDir, "GECKCustom.ini");
		public static readonly string GeckPrefsIniPath = Path.Combine(Fallout3SaveDir, "GECKPrefs.ini");
		public static readonly string FORendererFile = Path.Combine(Fallout3SaveDir, "RendererInfo.txt");
		public static readonly string FOSavesPath = Path.Combine(Fallout3SaveDir, NativeMethods.GetPrivateProfileString("General", "SLocalSavePath", "Games", FOIniPath));
		private static string packageDir;
		public static readonly string fommDir = Path.Combine(exeDir, "fomm");
		public static readonly string overwriteDir = Path.Combine(exeDir, "overwrites");
		public static readonly string LocalDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fallout3");
		public static readonly string PluginsFile = Path.Combine(LocalDataPath, "plugins.txt");
		public static readonly string DLCDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\xlive\\DLC");

		private static bool monoMode;
		public static bool MonoMode { get { return monoMode; } }

		public static string PackageDir { get { return packageDir; } }

		private static void WriteHelp()
		{
			Console.WriteLine("Command line options:");
			Console.WriteLine();
			Console.WriteLine("*.fomod, *.rar, *.7z, *.zip, *.bsa, *.esm, *.esp, *.sdp");
			Console.WriteLine("Open the specified file in the relevent utility");
			Console.WriteLine();
			Console.WriteLine("-setup, -bsa-unpacker, -bsa-creator, -tessnip, -sdp-editor, -package-manager, -install-tweaker");
			Console.WriteLine("Open the specified utility window, without opening the main form where appropriate");
			Console.WriteLine();
			Console.WriteLine("-mono");
			Console.WriteLine("Run in mono compatibility mode. Disables some features which are known to be broken under mono");
			Console.WriteLine();
			Console.WriteLine("-no-uac-check");
			Console.WriteLine("Don't check for vista UAC issues");
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
			if (Array.IndexOf<string>(args, "-mono") != -1) monoMode = true;
#if TRACE
			TRACE_FILE = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), TRACE_FILE);
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
			if (args.Length > 0 && (args[0] == "-?" || args[0] == "/?" || args[0] == "-help"))
			{
				WriteHelp();
				return;
			}

			System.Threading.Mutex mutex;
			bool newMutex;
			Directory.SetCurrentDirectory(exeDir);
			//Style setup
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Settings.Init();

			packageDir = Settings.GetString("FomodDir");
			if (packageDir == null) packageDir = Path.Combine(exeDir, "mods");
#if TRACE
				Trace.WriteLine("We know where the mods live: " + packageDir);
#endif
			string autoLoad = null;

			if (args.Length > 0)
			{
				if (!args[0].StartsWith("-") && File.Exists(args[0]))
				{
					switch (Path.GetExtension(args[0]).ToLowerInvariant())
					{
						case ".rar":
						case ".7z":
						case ".zip":
						case ".fomod":
							mutex = new System.Threading.Mutex(true, "fommMainMutex", out newMutex);
							mutex.Close();
							if (!newMutex)
							{
								Messaging.TransmitMessage(args[0]);
								return;
							}
							else
							{
								autoLoad = args[0];
								break;
							}
						case ".dat":
						case ".bsa":
							Application.Run(new BSABrowser(args[0]));
							return;
						case ".sdp":
							Application.Run(new ShaderEdit.MainForm(args[0]));
							return;
						case ".esp":
						case ".esm":
							Application.Run(new TESsnip.TESsnip(new string[] { args[0] }));
							return;
					}
				}
				else
				{
					switch (args[0])
					{
						case "-setup":
							mutex = new System.Threading.Mutex(true, "fommMainMutex", out newMutex);
							if (!newMutex)
							{
								MessageBox.Show("fomm is already running", "Error");
								mutex.Close();
								return;
							}
							Application.Run(new SetupForm(false));
							mutex.Close();
							return;
						case "-bsa-unpacker":
							Application.Run(new BSABrowser());
							return;
						case "-bsa-creator":
							Application.Run(new BSACreator());
							return;
						case "-tessnip":
							Application.Run(new TESsnip.TESsnip());
							return;
						case "-sdp-editor":
							Application.Run(new ShaderEdit.MainForm());
							return;
						case "-u":
							string strGuid = args[1];
							string strPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
							ProcessStartInfo psiInfo = new ProcessStartInfo(strPath + @"\msiexec.exe", "/x " + strGuid);
							Process.Start(psiInfo);
							return;
					}
				}
			}
#if TRACE
				Trace.WriteLine("Creating mutex.");
				Trace.Indent();
#endif

			mutex = new System.Threading.Mutex(true, "fommMainMutex", out newMutex);
			if (!newMutex)
			{
#if TRACE
					Trace.WriteLine("FOMM is already running.");

#endif
				MessageBox.Show("fomm is already running", "Error");
				mutex.Close();
				return;
			}
#if TRACE
				Trace.Unindent();
#endif
			try
			{
#if TRACE
					Trace.WriteLine("Looking for Fallout 3.");
					Trace.Indent();
#endif
				//If we aren't in fallouts directory, look it up in the registry
				if (!File.Exists("Fallout3.exe") && !File.Exists("Fallout3ng.exe"))
				{
					if (File.Exists("..\\Fallout3.exe") || File.Exists("..\\Fallout3ng.exe"))
					{
						Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), ".."));
#if TRACE
							Trace.WriteLine("Found, we think (1): " + Path.GetFullPath("."));
#endif
					}
					else
					{
						string path = Settings.GetString("FalloutDir");
						if (path == null)
						{
							try
							{
								path = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Bethesda Softworks\Fallout3", "Installed Path", null) as string;
							}
							catch { path = null; }

						}
						if (path != null)
						{
							Directory.SetCurrentDirectory(path);
#if TRACE
								Trace.WriteLine("Found, we think (2): " + Path.GetFullPath("."));
#endif
						}
					}
				}
#if TRACE
					Trace.WriteLine("Verifying Fallout 3 location: " + Path.GetFullPath("."));
#endif
				//Check that we're in fallout's directory and that we have write access
				bool cancellaunch = true;
				if (File.Exists("fallout3.exe") || File.Exists("fallout3ng.exe"))
				{
#if TRACE
						Trace.WriteLine("Check for UAC.");
						Trace.Indent();
#endif
					if (!Settings.GetBool("NoUACCheck") || Array.IndexOf<string>(args, "-no-uac-check") == -1)
					{
						try
						{
							File.Delete("limited");
							string VistaVirtualStore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualStore\\");
							VistaVirtualStore = Path.Combine(VistaVirtualStore, Directory.GetCurrentDirectory().Remove(0, 3));
							VistaVirtualStore = Path.Combine(VistaVirtualStore, "limited");
							if (File.Exists(VistaVirtualStore)) File.Delete(VistaVirtualStore);
							FileStream fs = File.Create("limited");
							fs.Close();
							if (File.Exists(VistaVirtualStore))
							{
#if TRACE
									Trace.WriteLine("UAC is messing us up.");
#endif
								MessageBox.Show("Vista's UAC is preventing Fallout mod manager from obtaining write access to fallout's installation directory.\n" +
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
							MessageBox.Show("Unable to get write permissions for fallout's installation directory." + Environment.NewLine + "Please read the 'Readme - fomm.txt' file found in the fomm subfolder of your FOMM installation.", "Error");
						}
					}
					else cancellaunch = false;
				}
				else
				{
#if TRACE
						Trace.WriteLine("Could not find Fallout.");
#endif
					MessageBox.Show("Could not find fallout 3 directory\nFallout's registry entry appear to be missing or incorrect. Install fomm into fallout's base directory instead.", "Error");
				}
#if TRACE
					Trace.Unindent();
					Trace.WriteLine("We know where Fallout lives: " + Path.GetFullPath("."));
					Trace.Unindent();
#endif

				if (cancellaunch) return;

				if (!Directory.Exists(tmpPath)) Directory.CreateDirectory(tmpPath);
				if (!Directory.Exists(PackageDir)) Directory.CreateDirectory(PackageDir);
				if (!Directory.Exists(fommDir)) Directory.CreateDirectory(fommDir);
				if (!Directory.Exists(LocalDataPath)) Directory.CreateDirectory(LocalDataPath);
				if (!Directory.Exists(overwriteDir)) Directory.CreateDirectory(overwriteDir);

#if TRACE
					Trace.WriteLine("Checking DLC location.");
					Trace.Indent();
#endif

				if (Directory.Exists(DLCDir) && Settings.GetString("IgnoreDLC") != "True")
				{
#if TRACE
						Trace.Write("Anchorage...");
#endif
					if (GetFiles(DLCDir, "Anchorage.esm", SearchOption.AllDirectories).Length == 1)
					{
						if (!File.Exists("data\\Anchorage.esm") && !File.Exists("data\\Anchorage - Main.bsa") && !File.Exists("data\\Anchorage - Sounds.bsa"))
						{
							string[] f1 = Directory.GetFiles(DLCDir, "Anchorage.esm", SearchOption.AllDirectories);
							string[] f2 = Directory.GetFiles(DLCDir, "Anchorage - Main.bsa", SearchOption.AllDirectories);
							string[] f3 = Directory.GetFiles(DLCDir, "Anchorage - Sounds.bsa", SearchOption.AllDirectories);
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
										Settings.SetString("IgnoreDLC", "True");
										break;
								}
							}
						}
					}
#if TRACE
						Trace.WriteLine("Done");
						Trace.Write("The Pitt...");
#endif
					if (GetFiles(DLCDir, "ThePitt.esm", SearchOption.AllDirectories).Length == 1)
					{
						if (!File.Exists("data\\ThePitt.esm") && !File.Exists("data\\ThePitt - Main.bsa") && !File.Exists("data\\ThePitt - Sounds.bsa"))
						{
							string[] f1 = Directory.GetFiles(DLCDir, "ThePitt.esm", SearchOption.AllDirectories);
							string[] f2 = Directory.GetFiles(DLCDir, "ThePitt - Main.bsa", SearchOption.AllDirectories);
							string[] f3 = Directory.GetFiles(DLCDir, "ThePitt - Sounds.bsa", SearchOption.AllDirectories);
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
										Settings.SetString("IgnoreDLC", "True");
										break;
								}
							}
						}
					}
#if TRACE
						Trace.WriteLine("Done.");
						Trace.Write("Broken Steel...");
#endif
					if (GetFiles(DLCDir, "BrokenSteel.esm", SearchOption.AllDirectories).Length == 1)
					{
						if (!File.Exists("Data\\BrokenSteel.esm"))
						{
							string[][] files = new string[8][];
							files[0] = Directory.GetFiles(DLCDir, "BrokenSteel.esm", SearchOption.AllDirectories);
							files[1] = Directory.GetFiles(DLCDir, "BrokenSteel - Main.bsa", SearchOption.AllDirectories);
							files[2] = Directory.GetFiles(DLCDir, "BrokenSteel - Sounds.bsa", SearchOption.AllDirectories);
							files[3] = Directory.GetFiles(DLCDir, "2 weeks later.bik", SearchOption.AllDirectories);
							files[4] = Directory.GetFiles(DLCDir, "B09.bik", SearchOption.AllDirectories);
							files[5] = Directory.GetFiles(DLCDir, "B27.bik", SearchOption.AllDirectories);
							files[6] = Directory.GetFiles(DLCDir, "B28.bik", SearchOption.AllDirectories);
							files[7] = Directory.GetFiles(DLCDir, "B29.bik", SearchOption.AllDirectories);
							bool missing = false;
							for (int i = 0; i < 8; i++)
							{
								if (files[i].Length != 1)
								{
									missing = true;
									break;
								}
								if ((i < 3 && File.Exists(Path.Combine("Data", Path.GetFileName(files[i][0])))) ||
								(i > 4 && File.Exists(Path.Combine("Data\\Video", Path.GetFileName(files[i][0])))))
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
										for (int i = 0; i < 3; i++)
										{
											File.Move(files[i][0], Path.Combine("Data", Path.GetFileName(files[i][0])));
										}
										for (int i = 3; i < 8; i++)
										{
											File.Move(files[i][0], Path.Combine("Data\\Video", Path.GetFileName(files[i][0])));
										}
										break;
									case DialogResult.No:
										Settings.SetString("IgnoreDLC", "True");
										break;
								}
							}
						}
					}
#if TRACE
						Trace.WriteLine("Done.");
						Trace.Write("Point Lookout...");
#endif
					if (GetFiles(DLCDir, "PointLookout.esm ", SearchOption.AllDirectories).Length == 1)
					{
						if (!File.Exists("data\\PointLookout.esm ") && !File.Exists("data\\PointLookout - Main.bsa") && !File.Exists("data\\PointLookout - Sounds.bsa"))
						{
							string[] f1 = Directory.GetFiles(DLCDir, "PointLookout.esm", SearchOption.AllDirectories);
							string[] f2 = Directory.GetFiles(DLCDir, "PointLookout - Main.bsa", SearchOption.AllDirectories);
							string[] f3 = Directory.GetFiles(DLCDir, "PointLookout - Sounds.bsa", SearchOption.AllDirectories);
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
										Settings.SetString("IgnoreDLC", "True");
										break;
								}
							}
						}
					}
#if TRACE
						Trace.WriteLine("Done.");
						Trace.Write("Zeta...");
#endif
					if (GetFiles(DLCDir, "Zeta.esm ", SearchOption.AllDirectories).Length == 1)
					{
						if (!File.Exists("data\\Zeta.esm ") && !File.Exists("data\\Zeta - Main.bsa") && !File.Exists("data\\Zeta - Sounds.bsa"))
						{
							string[] f1 = Directory.GetFiles(DLCDir, "Zeta.esm", SearchOption.AllDirectories);
							string[] f2 = Directory.GetFiles(DLCDir, "Zeta - Main.bsa", SearchOption.AllDirectories);
							string[] f3 = Directory.GetFiles(DLCDir, "Zeta - Sounds.bsa", SearchOption.AllDirectories);
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
										Settings.SetString("IgnoreDLC", "True");
										break;
								}
							}
						}
					}
#if TRACE
						Trace.WriteLine("Done.");
#endif
				}
#if TRACE
				Trace.Unindent();
				string str7zPath = Path.Combine(Program.fommDir, "7z-32bit.dll");
				Trace.WriteLine("7z Path: " + str7zPath + " (Exists: " + File.Exists(str7zPath) + ")");
				Trace.Flush();
#endif
				SevenZipCompressor.SetLibraryPath(Path.Combine(Program.fommDir, "7z-32bit.dll"));
#if TRACE
				Trace.WriteLine("Install Log Version: " + InstallLog.Current.GetInstallLogVersion());
				Trace.Indent();
#endif
				//check to see if we need to upgrade the install log format
				if (InstallLog.Current.GetInstallLogVersion() < InstallLog.CURRENT_VERSION)
				{
#if TRACE
						Trace.Write("Upgrade to " + InstallLog.CURRENT_VERSION + " required...");
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
						MessageBox.Show("An error occurred while upgrading your log file. A crash dump will have been saved in 'fomm\\crashdump.txt'" + Environment.NewLine +
										"Please make a bug report and include the contents of that file.", "Upgrade Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						HandleException(e);
						return;
					}
#if TRACE
						Trace.WriteLine("Done.");
#endif
				}
#if TRACE
				Trace.Unindent();
				Trace.Write("Uninstalling missing FOMods...");
#endif

				//let's uninstall any fomods that have been deleted since we last ran
				IList<InstallLog.FomodInfo> lstMods = InstallLog.Current.GetVersionedModList();
				foreach (InstallLog.FomodInfo fifMod in lstMods)
				{
					string strFomodPath = Path.Combine(Program.PackageDir, fifMod.BaseName + ".fomod");
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
					MessageBox.Show("An error occurred while scanning your fomods for new versions. A crash dump will have been saved in 'fomm\\crashdump.txt'" + Environment.NewLine +
										"Please make a bug report and include the contents of that file.", "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					HandleException(e);
					return;
				}

#if TRACE
				Trace.WriteLine("Done.");
				Trace.WriteLine("Running Application.");
				Trace.Flush();
#endif
				if (Array.IndexOf<string>(args, "-install-tweaker") != -1)
				{
					Application.Run(new InstallTweaker.InstallationTweaker());
				}
				else
				{
					if (autoLoad == null && Array.IndexOf<string>(args, "-package-manager") != -1) autoLoad = string.Empty;
					try
					{
						Application.Run(new MainForm(autoLoad));
					}
					catch (Exception e)
					{
						MessageBox.Show("Something bad seems to have happened. As long as it wasn't too bad, a crash dump will have been saved in 'fomm\\crashdump.txt'\n" +
											"Please include the contents of that file if you want to make a bug report", "Error");
						HandleException(e);
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
			MessageBox.Show("Something bad seems to have happened. As long as it wasn't too bad, a crash dump will have been saved in 'fomm\\crashdump.txt'\n" +
				"Please include the contents of that file if you want to make a bug report", "Error");
			HandleException(e.Exception);
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
			MessageBox.Show("Something bad seems to have happened. As long as it wasn't too bad, a crash dump will have been saved in 'fomm\\crashdump.txt'\n" +
				"Please include the contents of that file if you want to make a bug report", "Error");
			HandleException(e.ExceptionObject as Exception);
		}

		static void HandleException(Exception ex)
		{
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
				string strDumpFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "crashdump.txt");
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