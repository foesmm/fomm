using System;
using System.Runtime.InteropServices;

namespace Fomm
{
  internal static class NativeMethods
  {
    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi)]
    public static extern unsafe sbyte* Disasm(byte[] data, int len, byte Color);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi)]
    public static extern unsafe byte* Asm(byte[] data, int len);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi)]
    public static extern unsafe byte* Compile(string data, int len, string EntryPoint, string Profile, byte Debug);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    public static extern void ddsInit(IntPtr hwnd);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi)]
    public static extern unsafe IntPtr ddsShrink(byte[] data, int len, out int oSize);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi)]
    public static extern void ddsClose();

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    public static extern IntPtr ddsLoad(byte[] data, int len);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    public static extern IntPtr ddsCreate(int width, int height);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    public static extern void ddsBlt(IntPtr source, int sL, int sT, int sW, int sH, IntPtr dest, int dL, int dT, int dW,
                                     int dH);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    public static extern IntPtr ddsSave(IntPtr ptr, int format, int mipmaps, out int length);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    public static extern void ddsRelease(IntPtr tex);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    public static extern void ddsGetSize(IntPtr tex, out int width, out int height);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    public static extern IntPtr ddsLock(IntPtr tex, out int length, out int pitch);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    public static extern void ddsUnlock(IntPtr tex);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    public static extern void ddsSetData(IntPtr tex, byte[] data, int len);

    [DllImport("kernel32", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    public static extern int GetPrivateProfileIntA(string section, string value, int def, string path);

    public static UInt64 GetPrivateProfileUInt64(string section, string value, UInt64 def, string path)
    {
      string strValue = GetPrivateProfileString(section, value, null, path);
      if (String.IsNullOrEmpty(strValue))
      {
        return def;
      }
      UInt64 ulngValue = 0;
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

    [DllImport("kernel32", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    private static extern int GetPrivateProfileStringA(string section, string value, string def,
                                                       [MarshalAs(UnmanagedType.LPArray)] byte[] buf, int buflen,
                                                       string path);

    public static string GetPrivateProfileString(string section, string value, string def, string path)
    {
      byte[] buffer = new byte[256];
      int len = GetPrivateProfileStringA(section, value, def, buffer, 256, path);
      return System.Text.Encoding.Default.GetString(buffer, 0, len);
    }

    [DllImport("kernel32", CharSet = CharSet.Ansi), System.Security.SuppressUnmanagedCodeSecurity()]
    public static extern int WritePrivateProfileStringA(string section, string value, string val, string path);
  }
}