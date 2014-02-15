using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.AccessControl;

using Microsoft.Win32;

namespace fomm.Updater
{
	/// <summary>
	/// Description of Update.
	/// </summary>
	public static class UpdateHelper
	{
	    private const string uacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
	    private const string uacRegistryValue = "EnableLUA";
	    
	    private static string uninstallRegistryKey
	    {
	    	get
	    	{
	    		return "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
	    	}
	    }
	    
	    private static uint STANDARD_RIGHTS_READ = 0x00020000;
	    private static uint TOKEN_QUERY = 0x0008;
	    private static uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
	
	    [DllImport("advapi32.dll", SetLastError = true)]
	    [return: MarshalAs(UnmanagedType.Bool)]
	    static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);
	
	    [DllImport("advapi32.dll", SetLastError = true)]
	    public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);
	
	    public enum TOKEN_INFORMATION_CLASS
	    {
	        TokenUser = 1,
	        TokenGroups,
	        TokenPrivileges,
	        TokenOwner,
	        TokenPrimaryGroup,
	        TokenDefaultDacl,
	        TokenSource,
	        TokenType,
	        TokenImpersonationLevel,
	        TokenStatistics,
	        TokenRestrictedSids,
	        TokenSessionId,
	        TokenGroupsAndPrivileges,
	        TokenSessionReference,
	        TokenSandBoxInert,
	        TokenAuditPolicy,
	        TokenOrigin,
	        TokenElevationType,
	        TokenLinkedToken,
	        TokenElevation,
	        TokenHasRestrictions,
	        TokenAccessInformation,
	        TokenVirtualizationAllowed,
	        TokenVirtualizationEnabled,
	        TokenIntegrityLevel,
	        TokenUIAccess,
	        TokenMandatoryPolicy,
	        TokenLogonSid,
	        MaxTokenInfoClass
	    }
	
	    public enum TOKEN_ELEVATION_TYPE
	    {
	        TokenElevationTypeDefault = 1,
	        TokenElevationTypeFull,
	        TokenElevationTypeLimited
	    }
	
	    public static bool IsUacEnabled
	    {
	        get
	        {
	            RegistryKey uacKey = Registry.LocalMachine.OpenSubKey(uacRegistryKey, false);
	            bool result = uacKey.GetValue(uacRegistryValue).Equals(1);
	            return result;
	        }
	    }
	
	    public static bool IsProcessElevated
	    {
	        get
	        {
	            if (IsUacEnabled)
	            {
	                IntPtr tokenHandle;
	                if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_READ, out tokenHandle))
	                {
	                    throw new ApplicationException("Could not get process token.  Win32 Error Code: " + Marshal.GetLastWin32Error());
	                }
	
	                TOKEN_ELEVATION_TYPE elevationResult = TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault;
	
	                int elevationResultSize = Marshal.SizeOf((int)elevationResult);
	                uint returnedSize = 0;
	                IntPtr elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);
	
	                bool success = GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevationType, elevationTypePtr, (uint)elevationResultSize, out returnedSize);
	                if (success)
	                {
	                    elevationResult = (TOKEN_ELEVATION_TYPE)Marshal.ReadInt32(elevationTypePtr);
	                    bool isProcessAdmin = elevationResult == TOKEN_ELEVATION_TYPE.TokenElevationTypeFull;
	                    return isProcessAdmin;
	                }
	                else
	                {
	                    throw new ApplicationException("Unable to determine the current elevation.");
	                }
	            }
	            else
	            {
	                WindowsIdentity identity = WindowsIdentity.GetCurrent();
	                WindowsPrincipal principal = new WindowsPrincipal(identity);
	                bool result = principal.IsInRole(WindowsBuiltInRole.Administrator);
	                return result;
	            }
	        }
	    }
		
		public static bool HasWriteAccessToFolder(string strPath)
		{
			try
	        {
	            bool writeable = false;
	            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
	            DirectorySecurity security = Directory.GetAccessControl(strPath);
	            AuthorizationRuleCollection authRules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));
	
	            foreach (FileSystemAccessRule accessRule in authRules)
	            {
	
	                if (principal.IsInRole(accessRule.IdentityReference as SecurityIdentifier))
	                {
	                    if ((FileSystemRights.WriteData & accessRule.FileSystemRights) == FileSystemRights.WriteData)
	                    {
	                        if (accessRule.AccessControlType == AccessControlType.Allow)
	                        {
	                            writeable = true;
	                        }
	                        else if (accessRule.AccessControlType == AccessControlType.Deny)
	                        {
	                            //Deny usually overrides any Allow
	                            return false;
	                        }
	
	                    } 
	                }
	            }
	            return writeable;
	        }
	        catch (UnauthorizedAccessException)
	        {
	            return false;
	        }
		}
		
		public static void RunElevated()
		{
			Process p = new Process();
			p.StartInfo.Verb = "runas";
			p.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);
			p.StartInfo.UseShellExecute = true;
			p.Start();
		}
		
		public static bool IsLegacyFommInstalled
		{
			get
			{
				bool bLegacyInstalled = false;
				string SubKey;
				SubKey = String.Format("{0}\\{{{1}}}_is1", uninstallRegistryKey, "Generic Mod Manager_is1");
				bLegacyInstalled |= (Registry.LocalMachine.OpenSubKey(SubKey, false) != null);
				
				SubKey = String.Format("{0}\\{{{1}}}_is1", uninstallRegistryKey, Fomm.ProductInfo.GUID);
				bLegacyInstalled |= (Registry.LocalMachine.OpenSubKey(SubKey, false) != null);
				
				return bLegacyInstalled;
			}
		}
		
		public static bool IsFommInstalled
		{
			get
			{
				return IsLegacyFommInstalled || 
					(Registry.LocalMachine.OpenSubKey(String.Format("{0}\\{{{1}}}_is1", uninstallRegistryKey, Fomm.ProductInfo.GUID), false) != null);
			}
		}
		
		public static bool WriteUninstallationInfo()
		{
			return false;
		}
	}
}
