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
		public const string Version = "0.11.1";
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

		public static readonly string tmpPath = Path.Combine(Path.GetTempPath(), "fomm");
		public static readonly string exeDir = Path.GetDirectoryName(Application.ExecutablePath);
		public static readonly string Fallout3SaveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My games\\Fallout3");
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

			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			Settings.Init();

			if (Array.IndexOf<string>(args, "-mono") != -1) monoMode = true;

			packageDir = Settings.GetString("FomodDir");
			if (packageDir == null) packageDir = Path.Combine(exeDir, "mods");

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

			mutex = new System.Threading.Mutex(true, "fommMainMutex", out newMutex);
			if (!newMutex)
			{
				MessageBox.Show("fomm is already running", "Error");
				mutex.Close();
				return;
			}
			try
			{
				//If we aren't in fallouts directory, look it up in the registry
				if (!File.Exists("Fallout3.exe") && !File.Exists("Fallout3ng.exe"))
				{
					if (File.Exists("..\\Fallout3.exe") || File.Exists("..\\Fallout3ng.exe"))
					{
						Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), ".."));
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
						}
					}
				}


				//Check that we're in fallout's directory and that we have write access
				bool cancellaunch = true;
				if (File.Exists("fallout3.exe") || File.Exists("fallout3ng.exe"))
				{
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
							MessageBox.Show("Unable to get write permissions for fallout's installation directory", "Error");
						}
					}
					else cancellaunch = false;
				}
				else MessageBox.Show("Could not find fallout 3 directory\nFallout's registry entry appear to be missing or incorrect. Install fomm into fallout's base directory instead.", "Error");

				if (cancellaunch) return;

				if (!Directory.Exists(tmpPath)) Directory.CreateDirectory(tmpPath);
				if (!Directory.Exists(PackageDir)) Directory.CreateDirectory(PackageDir);
				if (!Directory.Exists(fommDir)) Directory.CreateDirectory(fommDir);
				if (!Directory.Exists(LocalDataPath)) Directory.CreateDirectory(LocalDataPath);
				if (!Directory.Exists(overwriteDir)) Directory.CreateDirectory(overwriteDir);

				if (Directory.Exists(DLCDir) && Settings.GetString("IgnoreDLC") != "True")
				{
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
				}

				//check to see if we need to upgrade the install log format
				if (InstallLog.Current.GetInstallLogVersion() < new Version("0.1.0.0"))
				{
					string[] strModInstallFiles = Directory.GetFiles(Program.PackageDir, "*.XMl", SearchOption.TopDirectoryOnly);
					InstallLogUpgrader iluUgrader = new InstallLogUpgrader();
					try
					{
						MessageBox.Show("FOMM needs to upgrade some of its files. This could take a few minutes, depending on how many mods are installed.", "Upgrade Required");
						iluUgrader.UpgradeInstallLog();
					}
					catch (Exception e)
					{
						MessageBox.Show("An error occurred while upgrading your log file. A crash dump will have been saved in 'fomm\\crashdump.txt'" + Environment.NewLine +
										"Please make a bug report and include the contents of that file.", "Upgrade Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						HandleException(e);
						return;
					}
				}

				if (Array.IndexOf<string>(args, "-install-tweaker") != -1)
				{
					Application.Run(new InstallTweaker.InstallationTweaker());
				}
				else
				{
					if (autoLoad == null && Array.IndexOf<string>(args, "-package-manager") != -1) autoLoad = string.Empty;
					Application.Run(new MainForm(autoLoad));
				}

				if (Directory.Exists(tmpPath))
				{
					try
					{
						Directory.Delete(tmpPath, true);
					}
					catch (Exception e)
					{
						if (!(e is IOException || e is UnauthorizedAccessException)) throw;
						//someone's probably stuck a readonly file in a mod again...
						DirectoryInfo di = new DirectoryInfo(tmpPath);
						FileInfo[] fis = di.GetFiles("*", SearchOption.AllDirectories);
						foreach (FileInfo fi in fis)
						{
							if ((fi.Attributes & FileAttributes.ReadOnly) != 0) fi.Attributes ^= FileAttributes.ReadOnly;
							if ((fi.Attributes & FileAttributes.System) != 0) fi.Attributes ^= FileAttributes.System;
							fi.Delete();
						}
						DirectoryInfo[] dis = di.GetDirectories("*", SearchOption.AllDirectories);
						foreach (DirectoryInfo di2 in dis)
						{
							if ((di2.Attributes & FileAttributes.ReadOnly) != 0) di2.Attributes ^= FileAttributes.ReadOnly;
						}
						if ((di.Attributes & FileAttributes.ReadOnly) != 0) di.Attributes ^= FileAttributes.ReadOnly;
						di.Delete(true);
					}
				}
			}
			finally
			{
				if (mutex != null)
					mutex.Close();
			}
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			MessageBox.Show("Something bad seems to have happened. As long as it wasn't too bad, a crash dump will have been saved in 'fomm\\crashdump.txt'\n" +
				"Please include the contents of that file if you want to make a bug report", "Error");
			HandleException(e.ExceptionObject as Exception);
		}

		static void HandleException(Exception ex)
		{
			if (ex != null)
			{
				string msg = DateTime.Now.ToLongDateString() + " - " + DateTime.Now.ToLongTimeString() + Environment.NewLine +
					"Fomm " + Version + (monoMode ? " (Mono)" : "") + Environment.NewLine + "OS version: " + Environment.OSVersion.ToString() +
				Environment.NewLine + Environment.NewLine + ex.ToString() + Environment.NewLine;
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
					msg += ex.ToString() + Environment.NewLine;
				}
				File.WriteAllText("fomm\\crashdump.txt", msg);
			}
		}

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