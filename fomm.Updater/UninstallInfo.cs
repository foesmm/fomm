using System;
using System.Reflection;

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
    public Boolean NoModify
    {
      get
      {
        return (this.dwNoModify > 0);
      }
      set
      {
        this.dwNoModify = value ? 1 : 0;
      }
    }
    private Int32 dwNoRepair = 0;
    public Boolean NoRepair
    {
      get
      {
        return (this.dwNoRepair > 0);
      }
      set
      {
        this.dwNoRepair = value ? 1 : 0;
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
    }
    private Int32 dwVersionMajor = 0;
    public Int32 VersionMajor
    {
      get
      {
        return this.dwVersionMajor;
      }
    }
    private Int32 dwVersionMinor = 0;
    public Int32 VersionMinor
    {
      get
      {
        return this.dwVersionMinor;
      }
    }
    #endregion

    public void SetVersion(string version)
    {
      this.SetVersion(System.Version.Parse(version));
    }

    public void SetVersion(Version version)
    {
      this.setVersionInternal(version);
    }

    private void setVersionInternal(Version version)
    {
      this.dwVersion = ((version.Major & 0xFF) << 24) + ((version.Minor & 0xFF) << 16) + ((version.Build & 0xFFFF));
      this.dwVersionMajor = version.Major;
      this.dwVersionMinor = version.Minor;
      this.strDisplayVersion = version.ToString();
    }

    public UninstallInfo(Guid guid)
      : this(String.Format("{{{0}}}", guid.ToString()))
    {
    }

    public UninstallInfo(string strKey)
    {
      strRegistryKey = String.Format("{0}\\{1}", uninstallRegistryKey, strKey);
      uiRegistryKey = Registry.LocalMachine.OpenSubKey(strRegistryKey, true);
      bUninstallInfoFound = ReadValues();
    }

    public bool ReadValues()
    {
      if (uiRegistryKey == null)
      {
        return false;
      }

      strDisplayIcon = (string)uiRegistryKey.GetValue("DisplayIcon", strDisplayIcon);
      strDisplayName = (string)uiRegistryKey.GetValue("DisplayName", strDisplayName);
      strDisplayVersion = (string)uiRegistryKey.GetValue("DisplayVersion", strDisplayVersion);
      dwEstimatedSize = (Int32)uiRegistryKey.GetValue("EstimatedSize", dwEstimatedSize);
      strHelpLink = (string)uiRegistryKey.GetValue("HelpLink", strHelpLink);
      strInstallDate = (string)uiRegistryKey.GetValue("InstallDate", strInstallDate);
      strInstallLocation = (string)uiRegistryKey.GetValue("InstallLocation", strInstallLocation);
      dwNoModify = (Int32)uiRegistryKey.GetValue("NoModify", dwNoModify);
      dwNoRepair = (Int32)uiRegistryKey.GetValue("NoRepair", dwNoRepair);
      strPublisher = (string)uiRegistryKey.GetValue("Publisher", strPublisher);
      strReadme = (string)uiRegistryKey.GetValue("Readme", strReadme);
      strUninstallString = (string)uiRegistryKey.GetValue("UninstallString", strUninstallString);
      strURLInfoAbout = (string)uiRegistryKey.GetValue("URLInfoAbout", strURLInfoAbout);
      strURLUpdateInfo = (string)uiRegistryKey.GetValue("URLUpdateInfo", strURLUpdateInfo);
      dwVersion = (Int32)uiRegistryKey.GetValue("Version", dwVersion);
      dwVersionMajor = (Int32)uiRegistryKey.GetValue("VersionMajor", dwVersionMajor);
      dwVersionMinor = (Int32)uiRegistryKey.GetValue("VersionMinor", dwVersionMinor);

      return true;
    }

    public bool WriteValues()
    {
      try
      {
        if (uiRegistryKey == null)
        {
          uiRegistryKey = Registry.LocalMachine.CreateSubKey(strRegistryKey);
        }

        uiRegistryKey.SetValue("DisplayIcon", strDisplayIcon, RegistryValueKind.String);
        uiRegistryKey.SetValue("DisplayName", strDisplayName, RegistryValueKind.String);
        uiRegistryKey.SetValue("DisplayVersion", strDisplayVersion, RegistryValueKind.String);
        uiRegistryKey.SetValue("EstimatedSize", dwEstimatedSize, RegistryValueKind.DWord);
        uiRegistryKey.SetValue("HelpLink", strHelpLink, RegistryValueKind.String);
        uiRegistryKey.SetValue("InstallDate", strInstallDate, RegistryValueKind.String);
        uiRegistryKey.SetValue("InstallLocation", strInstallLocation, RegistryValueKind.String);
        uiRegistryKey.SetValue("NoModify", dwNoModify, RegistryValueKind.DWord);
        uiRegistryKey.SetValue("NoRepair", dwNoRepair, RegistryValueKind.DWord);
        uiRegistryKey.SetValue("Publisher", strPublisher, RegistryValueKind.String);
        uiRegistryKey.SetValue("Readme", strReadme, RegistryValueKind.String);
        uiRegistryKey.SetValue("UninstallString", strUninstallString, RegistryValueKind.String);
        uiRegistryKey.SetValue("URLInfoAbout", strURLInfoAbout, RegistryValueKind.String);
        uiRegistryKey.SetValue("URLUpdateInfo", strURLUpdateInfo, RegistryValueKind.String);
        uiRegistryKey.SetValue("Version", dwVersion, RegistryValueKind.DWord);
        uiRegistryKey.SetValue("VersionMajor", dwVersionMajor, RegistryValueKind.DWord);
        uiRegistryKey.SetValue("VersionMinor", dwVersionMinor, RegistryValueKind.DWord);
        return true;
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.WriteLine(e.Message);
        return false;
      }
    }

    internal static bool HasInstanceField(Type type, object instance, string fieldName)
    {
      BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
          | BindingFlags.Static;
      FieldInfo field = type.GetField(fieldName, bindFlags);
      return (field != null);
    }

    internal static FieldInfo[] GetInstanceFields(Type type, object instance)
    {
      BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.NonPublic;
      return type.GetFields(bindFlags);
    }

    /// <summary>
    /// Uses reflection to get the field value from an object.
    /// </summary>
    ///
    /// <param name="type">The instance type.</param>
    /// <param name="instance">The instance object.</param>
    /// <param name="fieldName">The field's name which is to be fetched.</param>
    ///
    /// <returns>The field value from the object.</returns>
    internal static object GetInstanceField(Type type, object instance, string fieldName)
    {
      BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
          | BindingFlags.Static;
      FieldInfo field = type.GetField(fieldName, bindFlags);
      return field.GetValue(instance);
    }

    internal static void SetInstanceField(Type type, object instance, string fieldName, object value)
    {
      BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
          | BindingFlags.Static;
      FieldInfo field = type.GetField(fieldName, bindFlags);
      field.SetValue(instance, value);
    }

    public bool IsInstalled()
    {
      return bUninstallInfoFound;
    }
  }
}
