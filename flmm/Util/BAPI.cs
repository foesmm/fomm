using System;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;

/*
 * BOSS dll importer, refs:
 * http://msdn.microsoft.com/en-us/library/aa288468%28v=vs.71%29.aspx
 * http://stackoverflow.com/questions/10852634/using-a-32bit-or-64bit-dll-in-c-sharp-dllimport
 */
namespace Fomm.Util
{
  public class piBAPI
  {
    [DllImport("boss32.dll", EntryPoint = "IsCompatibleVersion", CallingConvention = CallingConvention.Cdecl)]
    protected static extern bool bapi32_IsCompatibleVersion(UInt32 bossVersionMajor, UInt32 bossVersionMinor, UInt32 bossVersionPatch);

    [DllImport("boss64.dll", EntryPoint = "IsCompatibleVersion", CallingConvention = CallingConvention.Cdecl)]
    protected static extern bool bapi64_IsCompatibleVersion(UInt32 bossVersionMajor, UInt32 bossVersionMinor, UInt32 bossVersionPatch);

    public bool? IsAvailable
    {
      get
      {
        return IsCompatibleVersion(2, 1, 0);
      }
    }
    
    public piBAPI()
    {
      string bossPath;
      RegistryKey rk;

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
    }
    
    public bool Is64bitProcess()
    {
      return IntPtr.Size == 8;
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
  }
}