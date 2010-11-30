using System;
using System.Security;
using System.Security.Permissions;
using System.IO;
using System.Collections.Generic;
#if TRACE
using System.Diagnostics;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
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
			Directory.GetAccessControl(Path.GetTempPath());
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
			fipFilePermission.AddPathList(FileIOPermissionAccess.Read, Environment.CurrentDirectory);

			permissions.AddPermission(fipFilePermission);
			permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
			permissions.AddPermission(new UIPermission(UIPermissionWindow.AllWindows));
#if TRACE
			Trace.Write("Demanding access to System tmp Dir...");
			try
			{
				permissions.Demand();
				Trace.WriteLine("Succeeded.");
			}
			catch (Exception e)
			{
				Trace.WriteLine("Failed:");
				Program.TraceException(e);
			}
			Trace.WriteLine("   System tmp Dir Permissions:");
			Trace.Indent();
			DirectorySecurity drsPermissions = Directory.GetAccessControl(Path.GetTempPath());
			Dictionary<string, List<FileSystemRights>> dicRights = new Dictionary<string, List<FileSystemRights>>();
			foreach (FileSystemAccessRule fsrRule in drsPermissions.GetAccessRules(true, true, typeof(NTAccount)))
			{
				if (!dicRights.ContainsKey(fsrRule.IdentityReference.Value))
					dicRights[fsrRule.IdentityReference.Value] = new List<FileSystemRights>();
				dicRights[fsrRule.IdentityReference.Value].Add(fsrRule.FileSystemRights);
			}
			foreach (KeyValuePair<string, List<FileSystemRights>> kvpRight in dicRights)
			{
				Trace.WriteLine(kvpRight.Key + " =>");
				Trace.Indent();
				foreach (FileSystemRights fsrRight in kvpRight.Value)
					Trace.WriteLine(fsrRight.ToString());
				Trace.Unindent();
			}
			Trace.Unindent();
			Trace.Write("Testing access to System tmp Dir...");
			try
			{
				File.WriteAllText(Path.Combine(Path.GetTempPath(), "testFile.txt"), "This is fun.");
				Trace.WriteLine("Passed: " + File.ReadAllText(Path.Combine(Path.GetTempPath(), "testFile.txt")));
				File.Delete(Path.Combine(Path.GetTempPath(), "testFile.txt"));
			}
			catch (Exception e)
			{
				Trace.WriteLine("Failed: ");
				Program.TraceException(e);
			}
#endif
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
