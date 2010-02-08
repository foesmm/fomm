using System;
using System.Security;
using System.Security.Permissions;
using System.IO;

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
