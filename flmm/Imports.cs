using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Fomm
{
  internal static class NativeMethods
  {
    [DllImport("kernel32", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public static extern int GetPrivateProfileIntA(string section, string value, int def, string path);

    public static UInt64 GetPrivateProfileUInt64(string section, string value, UInt64 def, string path)
    {
      var strValue = GetPrivateProfileString(section, value, null, path);
      if (String.IsNullOrEmpty(strValue))
      {
        return def;
      }
      UInt64 ulngValue;
      if (UInt64.TryParse(strValue, out ulngValue))
      {
        return ulngValue;
      }
      return def;
    }

    public static void WritePrivateProfileIntA(string section, string value, int val, string path)
    {
      WritePrivateProfileStringA(section, value, val.ToString(), path);
    }

    [DllImport("kernel32", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    private static extern int GetPrivateProfileStringA(string section, string value, string def,
                                                       [MarshalAs(UnmanagedType.LPArray)] byte[] buf, int buflen,
                                                       string path);

    public static string GetPrivateProfileString(string section, string value, string def, string path)
    {
      var buffer = new byte[256];
      var len = GetPrivateProfileStringA(section, value, def, buffer, 256, path);
      return Encoding.Default.GetString(buffer, 0, len);
    }

    [DllImport("kernel32", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public static extern int WritePrivateProfileStringA(string section, string value, string val, string path);
  }
}