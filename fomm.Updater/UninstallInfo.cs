using System;

using Microsoft.Win32;

namespace Fomm.Updater
{
	/// <summary>
	/// Description of UninstallInfo.
	/// </summary>
	public class UninstallInfo
	{
	    private const string uninstallRegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
	    private string strRegistryKey = null;
		private RegistryKey uiRegistryKey = null;
		private bool bUninstallInfoFound = false;

		public UninstallInfo(Guid guid): this(String.Format("{{{0}}}", guid.ToString()))
		{
		}
		
		public UninstallInfo(string strKey)
		{
			strRegistryKey = String.Format("{0}\\{1}", uninstallRegistryKey, strKey);
			this.uiRegistryKey = Registry.LocalMachine.OpenSubKey(strRegistryKey, true);
			this.bUninstallInfoFound = !(this.uiRegistryKey == null);
		}
		
		public bool IsValid
		{
			get
			{
				return bUninstallInfoFound;
			}
		}
		
		public string UninstallString
		{
			get
			{
				if (uiRegistryKey == null)
					return "";
				return uiRegistryKey.GetValue("UninstallString", "").ToString();
			}
			set
			{
				if (uiRegistryKey == null)
					uiRegistryKey = Registry.LocalMachine.CreateSubKey(strRegistryKey);
				uiRegistryKey.SetValue("UninstallString", value, RegistryValueKind.String);
			}
		}
	}
}
