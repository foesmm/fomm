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

        #region Properties
        private string strDisplayIcon = "";
        public string DisplayIcon
        {
            get
            {
                return this.DisplayIcon;
            }
            set
            {
                this.strDisplayIcon = value;
            }
        }
        private string strDisplayName = "";
        public string DisplayName
        {
            get
            {
                return this.strDisplayName;
            }
            set
            {
                this.strDisplayName = value;
            }
        }
        private string strDisplayVersion = "";
        public string DisplayVersion
        {
            get
            {
                return this.strDisplayVersion;
            }
            set
            {
                this.strDisplayVersion = value;
            }
        }
        private Int32 dwEstimatedSize = 0;
        public Int32 EstimatedSize
        {
            get
            {
                return this.dwEstimatedSize;
            }
            set
            {
                this.dwEstimatedSize = value;
            }
        }
        private string strHelpLink = "";
        public string HelpLink
        {
            get
            {
                return this.strHelpLink;
            }
            set
            {
                this.strHelpLink = value;
            }
        }
        private string strInstallDate = "";
        public string InstallDate
        {
            get
            {
                return this.strInstallDate;
            }
            set
            {
                this.strInstallDate = value;
            }
        }
        private string strInstallLocation = "";
        public string InstallLocation
        {
            get
            {
                return this.strInstallLocation;
            }
            set
            {
                this.strInstallLocation = value;
            }
        }
        private Int32 dwNoModify = 0;
        public Int32 NoModify
        {
            get
            {
                return this.dwNoModify;
            }
            set
            {
                this.dwNoModify = value;
            }
        }
        private Int32 dwNoRepair = 0;
        public Int32 NoRepair
        {
            get
            {
                return this.dwNoRepair;
            }
            set
            {
                this.dwNoRepair = value;
            }
        }
        private string strPublisher = "";
        public string Publisher
        {
            get
            {
                return this.strPublisher;
            }
            set
            {
                this.strPublisher = value;
            }
        }
        private string strReadme = "";
        public string Readme
        {
            get
            {
                return this.strReadme;
            }
            set
            {
                this.strReadme = value;
            }
        }
        private string strUninstallString = "";
        public string UninstallString
        {
            get
            {
                return this.strUninstallString;
            }
            set
            {
                this.strUninstallString = value;
            }
        }
        private string strURLInfoAbout = "";
        public string URLInfoAbout
        {
            get
            {
                return this.strURLInfoAbout;
            }
            set
            {
                this.strURLInfoAbout = value;
            }
        }
        private string strURLUpdateInfo = "";
        public string URLUpdateInfo
        {
            get
            {
                return this.strURLUpdateInfo;
            }
            set
            {
                this.strURLUpdateInfo = value;
            }
        }
        private Int32 dwVersion = 0;
        public Int32 Version
        {
            get
            {
                return this.dwVersion;
            }
            set
            {
                this.dwVersion = value;
            }
        }
        private Int32 dwVersionMajor = 0;
        public Int32 VersionMajor
        {
            get
            {
                return this.dwVersionMajor;
            }
            set
            {
                this.dwVersionMajor = value;
            }
        }
        private Int32 dwVersionMinor = 0;
        public Int32 VersionMinor
        {
            get
            {
                return this.dwVersionMinor;
            }
            set
            {
                this.dwVersionMinor = value;
            }
        }
        #endregion

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
	}
}
