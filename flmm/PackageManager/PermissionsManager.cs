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
		private static PermissionSet permissions;

		/// <summary>
		/// Initializes the permissions manager with the permissions required by an install script.
		/// </summary>
		internal static void Init()
		{
#if TRACE
			Trace.WriteLine("");
			Trace.WriteLine("Setting up File IO Permissions for: ");
			Trace.Indent();
			Trace.Write("     FOMM tmp Dir: ");
			Trace.WriteLine(Program.tmpPath);
			Trace.Write(" Install Info Dir: ");
			Trace.WriteLine(Program.GameMode.InstallInfoDirectory);
			Trace.Write("   System tmp Dir: ");
			Trace.WriteLine(Path.GetTempPath());
			Trace.WriteLine("   Settings Files: ");
			Trace.Indent();
			foreach (string strValue in Program.GameMode.SettingsFiles.Values)
				Trace.WriteLine(strValue);
			Trace.Unindent();
			Trace.WriteLine("      Other Paths: ");
			Trace.Indent();
			foreach (string strValue in Program.GameMode.AdditionalPaths.Values)
				Trace.WriteLine(strValue);
			Trace.Unindent();
			Trace.Unindent();
			Trace.Flush();
#endif
			permissions = new PermissionSet(PermissionState.None);
			//do the following paths need to add to this?
			// savesPath - fallout 3
			FileIOPermission fipFilePermission = new FileIOPermission(FileIOPermissionAccess.AllAccess, new string[] {
                Program.tmpPath, Path.GetTempPath(),
				Program.GameMode.InstallInfoDirectory,
				Program.GameMode.PluginsPath
            });
			
			List<string> lstPaths = new List<string>(Program.GameMode.SettingsFiles.Values);
			lstPaths.AddRange(Program.GameMode.AdditionalPaths.Values);
			fipFilePermission.AddPathList(FileIOPermissionAccess.AllAccess, lstPaths.ToArray());

			permissions.AddPermission(fipFilePermission);
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
				if (permissions == null)
					throw new InvalidOperationException("You must call Init() before using the permissions manager.");
				return permissions;
			}
		}
	}
}
