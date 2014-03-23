using System;
using System.Security;
using System.Security.Permissions;
using System.IO;
using System.Collections.Generic;

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
      permissions = new PermissionSet(PermissionState.None);
      //do the following paths need to add to this?
      // savesPath - fallout 3
      FileIOPermission fipFilePermission = new FileIOPermission(FileIOPermissionAccess.AllAccess, new string[]
      {
        Program.tmpPath,
        Path.GetTempPath(),
        Program.GameMode.InstallInfoDirectory,
        Program.GameMode.PluginsPath
      });

      List<string> lstPaths = new List<string>(Program.GameMode.SettingsFiles.Values);
      lstPaths.AddRange(Program.GameMode.AdditionalPaths.Values);
      fipFilePermission.AddPathList(FileIOPermissionAccess.AllAccess, lstPaths.ToArray());
      fipFilePermission.AddPathList(FileIOPermissionAccess.Read, Environment.CurrentDirectory);

      permissions.AddPermission(fipFilePermission);
      permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
      permissions.AddPermission(new UIPermission(UIPermissionWindow.AllWindows));

      // Not sure what permissions are needed for GetTempFileName() to work, so we add them all.
      permissions.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted));
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

    /// <summary>
    /// Gets whether or not the permissions manager has been initialized.
    /// </summary>
    /// <value>Whether or not the permissions manager has been initialized.</value>
    internal static bool IsInitialized
    {
      get
      {
        return (permissions != null);
      }
    }
  }
}
