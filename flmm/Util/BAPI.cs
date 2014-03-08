using System;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Fomm.Games;

/*
 * BOSS dll importer, refs:
 *
 * http://msdn.microsoft.com/en-us/library/aa288468%28v=vs.71%29.aspx
 * http://stackoverflow.com/questions/10852634/using-a-32bit-or-64bit-dll-in-c-sharp-dllimport
 * http://code.google.com/p/better-oblivion-sorting-software/source/browse/code/tags/2.1.0/boss-api/BOSS-API.h
 */
namespace Fomm.Util
{

  public class piBAPI
  {
    // http://code.google.com/p/better-oblivion-sorting-software/source/browse/code/tags/2.1.0/boss-common/Common/Error.cpp
    public const UInt32 BOSS_OK                                          = 0;

    public const UInt32 BOSS_ERROR_NO_MASTER_FILE                        = 1;  //Deprecated.
    public const UInt32 BOSS_ERROR_FILE_READ_FAIL                        = 2;
    public const UInt32 BOSS_ERROR_FILE_WRITE_FAIL                       = 3;
    public const UInt32 BOSS_ERROR_FILE_NOT_UTF8                         = 4;
    public const UInt32 BOSS_ERROR_FILE_NOT_FOUND                        = 5;
    public const UInt32 BOSS_ERROR_FILE_PARSE_FAIL                       = 6;
    public const UInt32 BOSS_ERROR_CONDITION_EVAL_FAIL                   = 7;
    public const UInt32 BOSS_ERROR_REGEX_EVAL_FAIL                       = 8;
    public const UInt32 BOSS_ERROR_NO_GAME_DETECTED                      = 9;
    public const UInt32 BOSS_ERROR_ENCODING_CONVERSION_FAIL              = 10;
    public const UInt32 BOSS_ERROR_PLUGIN_BEFORE_MASTER                  = 39;
    public const UInt32 BOSS_ERROR_INVALID_SYNTAX                        = 40;

    public const UInt32 BOSS_ERROR_FIND_ONLINE_MASTERLIST_REVISION_FAIL  = 11;
    public const UInt32 BOSS_ERROR_FIND_ONLINE_MASTERLIST_DATE_FAIL      = 12;
    public const UInt32 BOSS_ERROR_READ_UPDATE_FILE_LIST_FAIL            = 13;
    public const UInt32 BOSS_ERROR_FILE_CRC_MISMATCH                     = 14;

    public const UInt32 BOSS_ERROR_FS_FILE_MOD_TIME_READ_FAIL            = 15;
    public const UInt32 BOSS_ERROR_FS_FILE_MOD_TIME_WRITE_FAIL           = 16;
    public const UInt32 BOSS_ERROR_FS_FILE_RENAME_FAIL                   = 17;
    public const UInt32 BOSS_ERROR_FS_FILE_DELETE_FAIL                   = 18;
    public const UInt32 BOSS_ERROR_FS_CREATE_DIRECTORY_FAIL              = 19;
    public const UInt32 BOSS_ERROR_FS_ITER_DIRECTORY_FAIL                = 20;

    public const UInt32 BOSS_ERROR_CURL_INIT_FAIL                        = 21;
    public const UInt32 BOSS_ERROR_CURL_SET_ERRBUFF_FAIL                 = 22;
    public const UInt32 BOSS_ERROR_CURL_SET_OPTION_FAIL                  = 23;
    public const UInt32 BOSS_ERROR_CURL_SET_PROXY_FAIL                   = 24;
    public const UInt32 BOSS_ERROR_CURL_SET_PROXY_TYPE_FAIL              = 25;
    public const UInt32 BOSS_ERROR_CURL_SET_PROXY_AUTH_FAIL              = 26;
    public const UInt32 BOSS_ERROR_CURL_SET_PROXY_AUTH_TYPE_FAIL         = 27;
    public const UInt32 BOSS_ERROR_CURL_PERFORM_FAIL                     = 28;
    public const UInt32 BOSS_ERROR_CURL_USER_CANCEL                      = 29;

    public const UInt32 BOSS_ERROR_GUI_WINDOW_INIT_FAIL                  = 30;

    public const UInt32 BOSS_OK_NO_UPDATE_NECESSARY                      = 31;
    public const UInt32 BOSS_ERROR_LO_MISMATCH                           = 32;
    public const UInt32 BOSS_ERROR_NO_MEM                                = 33;
    public const UInt32 BOSS_ERROR_INVALID_ARGS                          = 34;
    public const UInt32 BOSS_ERROR_NETWORK_FAIL                          = 35;
    public const UInt32 BOSS_ERROR_NO_INTERNET_CONNECTION                = 36;
    public const UInt32 BOSS_ERROR_NO_TAG_MAP                            = 37;
    public const UInt32 BOSS_ERROR_PLUGINS_FULL                          = 38;
    public const UInt32 BOSS_ERROR_UNKNOWN                               = 999999;

    public const UInt32 BOSS_GAME_AUTODETECT = 0;
    public const UInt32 BOSS_GAME_OBLIVION   = 1;
    public const UInt32 BOSS_GAME_NEHRIM     = 2;
    public const UInt32 BOSS_GAME_SKYRIM     = 3;
    public const UInt32 BOSS_GAME_FALLOUT3   = 4;
    public const UInt32 BOSS_GAME_FALLOUTNV  = 5;
    public const UInt32 BOSS_GAME_MORROWIND  = 6;

    protected UInt32? _boss_db = null;
    protected GameMode _gm;

    // IsCompatibleVersion
    [DllImport("boss32.dll", EntryPoint = "IsCompatibleVersion")]
    protected static extern bool bapi32_IsCompatibleVersion(UInt32 bossVersionMajor, UInt32 bossVersionMinor, UInt32 bossVersionPatch);
    [DllImport("boss64.dll", EntryPoint = "IsCompatibleVersion")]
    protected static extern bool bapi64_IsCompatibleVersion(UInt32 bossVersionMajor, UInt32 bossVersionMinor, UInt32 bossVersionPatch);

    // CleanUpApi -- frees memory allocated by GetVersionString and GetLastErrorDetails
    [DllImport("boss32.dll", EntryPoint = "CleanUpAPI")]
    protected static extern void bapi32_CleanUpAPI();
    [DllImport("boss64.dll", EntryPoint = "CleanUpAPI")]
    protected static extern void bapi64_CleanUpAPI();

    // GetLastErrorDetails
    [DllImport("boss32.dll", EntryPoint = "GetLastErrorDetails")]
    protected static extern UInt32 bapi32_GetLastErrorDetails(ref IntPtr details);
    [DllImport("boss64.dll", EntryPoint = "GetLastErrorDetails")]
    protected static extern UInt32 bapi64_GetLastErrorDetails(ref IntPtr details);

    // GetVersionString
    [DllImport("boss32.dll", EntryPoint = "GetVersionString")]
    protected static extern UInt32 bapi32_GetVersionString(ref IntPtr pVersion);
    [DllImport("boss64.dll", EntryPoint = "GetVersionString")]
    protected static extern UInt32 bapi64_GetVersionString(ref IntPtr pVersion);

    // CreateBossDb
    [DllImport("boss32.dll", EntryPoint = "CreateBossDb")]
    protected static extern UInt32 bapi32_CreateBossDb(ref UInt32 boss_db, UInt32 clientGame, string dataPath);
    [DllImport("boss64.dll", EntryPoint = "CreateBossDb")]
    protected static extern UInt32 bapi64_CreateBossDb(ref UInt32 boss_db, UInt32 clientGame, string dataPath);

    // GetLoadOrder
    [DllImport("boss32.dll", EntryPoint = "GetLoadOrder")]
    protected static extern UInt32 bapi32_GetLoadOrder(UInt32 boss_db, ref IntPtr plugins, ref IntPtr numPlugins);
    [DllImport("boss64.dll", EntryPoint = "GetLoadOrder")]
    protected static extern UInt32 bapi64_GetLoadOrder(UInt32 boss_db, ref IntPtr plugins, ref IntPtr numPlugins);

    // UpdateMasterlist -- Fetches the latest masterlist to the path given
    [DllImport("boss32.dll", EntryPoint = "UpdateMasterlist")]
    protected static extern UInt32 bapi32_UpdateMasterlist(UInt32 boss_db, [MarshalAs(UnmanagedType.LPStr)] string listpath);
    [DllImport("boss64.dll", EntryPoint = "UpdateMasterlist")]
    protected static extern UInt32 bapi64_UpdateMasterlist(UInt32 boss_db, [MarshalAs(UnmanagedType.LPStr)] string listpath);


    public bool? IsAvailable
    {
      get
      {
        return IsCompatibleVersion(2, 1, 1);
      }
    }

    // Constructor
    public piBAPI(GameMode gm)
    {
      string bossPath;
      RegistryKey rk;

      _gm = gm;

      // Check registry for BOSS directory and append to path so DllImport can find it.
      rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\BOSS");
      if (rk != null)
      {
        bossPath = rk.GetValue("Installed Path").ToString();
        if (bossPath != null)
        {
          Environment.SetEnvironmentVariable("Path",
            Environment.GetEnvironmentVariable("Path") + Path.PathSeparator +
            bossPath + Path.DirectorySeparatorChar + "API" + Path.DirectorySeparatorChar);
        }
      }

      GetBossDb();

      if (true == IsAvailable)
      {
        UpdateMasterlist();
      }
    }

    public bool Is64bitProcess()
    {
      // This should be safe.  If not, can update to .Net 4.0.
      return IntPtr.Size == 8;
    }

    #region extern wrappers
    public void CleanUpAPI()
    {
      switch (Is64bitProcess())
      {
        case true:
          bapi64_CleanUpAPI();
          break;

        case false:
          bapi32_CleanUpAPI();
          break;
      }
    }

    public bool IsCompatibleVersion(UInt32 bossVersionMajor, UInt32 bossVersionMinor, UInt32 bossVersionPatch)
    {
      bool ret = false;

      try
      {
        ret = Is64bitProcess() ?
          bapi64_IsCompatibleVersion(bossVersionMajor, bossVersionMinor, bossVersionPatch) :
          bapi32_IsCompatibleVersion(bossVersionMajor, bossVersionMinor, bossVersionPatch);
      }
      catch
      {
      }

      return ret;
    }

    public string GetVersionString()
    {
      string ret = "";
      IntPtr pVersion = new IntPtr();
      UInt32 apiRet = BOSS_ERROR_UNKNOWN;

      if (GetBossDb())
      {
        switch (Is64bitProcess())
        {
          case true:
            apiRet = bapi64_GetVersionString(ref pVersion);
            break;

          case false:
            apiRet = bapi32_GetVersionString(ref pVersion);
            break;
        }

        if (BOSS_OK == apiRet)
        {
          ret = Marshal.PtrToStringAnsi(pVersion);
          CleanUpAPI();
        }
      }
      return ret;
    }

    public string GetLastErrorDetails()
    {
      string ret = "";
      IntPtr pVersion = new IntPtr();
      UInt32 apiRet = BOSS_ERROR_UNKNOWN;

      if (GetBossDb())
      {
        switch (Is64bitProcess())
        {
          case true:
            apiRet = bapi64_GetLastErrorDetails(ref pVersion);
            break;

          case false:
            apiRet = bapi32_GetLastErrorDetails(ref pVersion);
            break;
        }

        if (BOSS_OK == apiRet)
        {
          ret = Marshal.PtrToStringAnsi(pVersion);
          CleanUpAPI();
        }
      }
      return ret;
    }

    protected bool GetBossDb()
    {
      bool ret = false;
      UInt32 boss_db;
      UInt32 clientGame;
      UInt32 callret;

      if (_boss_db == null)
      {
        switch (_gm.GetType().ToString())
        {
          case "Fomm.Games.Fallout3.Fallout3GameMode":
            clientGame = BOSS_GAME_FALLOUT3;
            break;

          case "Fomm.Games.FalloutNewVegas.FalloutNewVegasGameMode":
            clientGame = BOSS_GAME_FALLOUTNV;
            break;

          default:
            clientGame = BOSS_GAME_AUTODETECT;
            break;
        }

        try
        {
          boss_db = 0;
          callret = 99;
          switch (Is64bitProcess())
          {
            case true:
              callret = bapi64_CreateBossDb(ref boss_db, clientGame, "");
              break;

            case false:
              callret = bapi32_CreateBossDb(ref boss_db, clientGame, "");
              break;
          }

          if (callret == BOSS_OK)
          {
            _boss_db = boss_db;
            ret = true;
          }
        }
        catch
        {
          _boss_db = 0;
        }
      }
      else
      {
        ret = true;
      }

      return ret;
    }

    public string[] GetLoadOrder()
    {
      String[] ret;
      IntPtr[] pPluginArray;
      IntPtr cnt;
      IntPtr pPlugins;
      UInt32 boss_db;

      ret = new String[0];
      cnt = new IntPtr();
      pPlugins = new IntPtr();

      try
      {
        if (GetBossDb())
        {
          boss_db = _boss_db.GetValueOrDefault(0);
          switch (Is64bitProcess())
          {
            case true:
              bapi64_GetLoadOrder(boss_db, ref pPlugins, ref cnt);
              break;

            case false:
              bapi32_GetLoadOrder(boss_db, ref pPlugins, ref cnt);
              break;
          }

          Array.Resize(ref ret, cnt.ToInt32());
          pPluginArray = new IntPtr[cnt.ToInt32()];
          Marshal.Copy(pPlugins, pPluginArray, 0, cnt.ToInt32());

          for (int i = 0; i < cnt.ToInt32(); i++)
          {
            ret[i] = Marshal.PtrToStringAnsi(pPluginArray[i]);
          }
        }

      }
      catch
      {
      }

      return ret;
    }

    public bool UpdateMasterlist()
    {
      bool ret = false;
      UInt32 boss_db;
      UInt32 apiRet = BOSS_ERROR_UNKNOWN;

      if (GetBossDb())
      {
        boss_db = _boss_db.GetValueOrDefault(0);
        switch (Is64bitProcess())
        {
          case true:
            apiRet = bapi64_UpdateMasterlist(boss_db, Path.Combine(_gm.InstallInfoDirectory, "lotemplate.txt"));
            break;

          case false:
            apiRet = bapi32_UpdateMasterlist(boss_db, Path.Combine(_gm.InstallInfoDirectory, "lotemplate.txt"));
            break;
        }

        ret = (apiRet == BOSS_OK);
      }

      return ret;
    }

    #endregion
  }
}