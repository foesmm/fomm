using System;
using System.Security;
using System.Security.Permissions;
using System.IO;
using System.Collections.Generic;
#if TRACE
using System.Diagnostics;
using Microsoft.Win32;
#endif

namespace Fomm.PackageManager
{
	/// <summary>
	/// Manages the permissions required by the application to install a mod.
	/// </summary>
	/// <remarks>
	/// This must be initialized before a custom script is called.
	/// </remarks>
	internal class PermissionsManager
	{
		private static readonly PermissionSet permissions;

		/// <summary>
		/// The static constructor.
		/// </summary>
		/// <remarks>
		/// This sets up the required permissions.
		/// </remarks>
		static PermissionsManager()
		{
#if TRACE
			Trace.WriteLine("");
			Trace.WriteLine("Setting up File IO Permissions for: ");
			Trace.Indent();
			Trace.Write("       Exe Dir: ");
			Trace.WriteLine(Program.ExecutableDirectory);
			Trace.Write("      Save Dir: ");
			Trace.WriteLine(Program.GameMode.SavesPath);
			Trace.Indent();
			Trace.Write("      Registry says profile lives here: ");
			Trace.WriteLine(Registry.GetValue(@"HKEY_CURRENT_USER\software\microsoft\windows\currentversion\explorer\user shell folders", "Personal", "Not Found").ToString());
			Trace.Unindent();
			Trace.Write("  FOMM tmp Dir: ");
			Trace.WriteLine(Program.tmpPath);
			Trace.Write("User Setting Path: ");
			Trace.WriteLine(Program.GameMode.UserSettingsPath);
			Trace.Write("   Current Dir: ");
			Trace.WriteLine(Environment.CurrentDirectory);
			Trace.Write(" Overwrite Dir: ");
			Trace.WriteLine(Program.GameMode.OverwriteDirectory);
			Trace.Write("System tmp Dir: ");
			Trace.WriteLine(Path.GetTempPath());
			Trace.Unindent();
			Trace.Flush();
#endif
			permissions = new PermissionSet(PermissionState.None);
			//do the following paths need to add to this?
			// savesPath - fallout 3
			FileIOPermission fipFilePermission = new FileIOPermission(FileIOPermissionAccess.AllAccess, new string[] {
                Program.tmpPath, Path.GetTempPath(),
				Program.GameMode.InstallInfoDirectory			
            });

			List<string> lstPaths = new List<string>(Program.GameMode.SettingsFiles.Values);
			lstPaths.AddRange(Program.GameMode.AdditionalPaths.Values);
			fipFilePermission.AddPathList(FileIOPermissionAccess.AllAccess, lstPaths.ToArray());

			permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
			permissions.AddPermission(new UIPermission(UIPermissionWindow.AllWindows));
		}

		/// <summary>
		/// Gets the current permissions set.
		/// </summary>
		/// <value>The current permissions set.</value>
		internal static PermissionSet CurrentPermissions
		{
			get
			{
				return permissions;
			}
		}
	}
}
