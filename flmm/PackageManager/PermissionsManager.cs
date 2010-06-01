using System;
using System.Security;
using System.Security.Permissions;
using System.IO;
#if TRACE
using System.Diagnostics;
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
			Trace.WriteLine(Program.exeDir);
			Trace.Write("      Save Dir: ");
			Trace.WriteLine(Program.Fallout3SaveDir);
			Trace.Write("  FOMM tmp Dir: ");
			Trace.WriteLine(Program.tmpPath);
			Trace.Write("Local Data Dir: ");
			Trace.WriteLine(Program.LocalDataPath);
			Trace.Write("   Current Dir: ");
			Trace.WriteLine(Environment.CurrentDirectory);
			Trace.Write(" Overwrite Dir: ");
			Trace.WriteLine(Program.overwriteDir);
			Trace.Write("System tmp Dir: ");
			Trace.WriteLine(Path.GetTempPath());
			Trace.Unindent();
			Trace.Flush();
#endif
			permissions = new PermissionSet(PermissionState.None);
			permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, new string[] {
                Program.exeDir, Program.Fallout3SaveDir, Program.tmpPath, Program.LocalDataPath, Environment.CurrentDirectory,
				Program.overwriteDir, Path.GetTempPath()
            }));
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
