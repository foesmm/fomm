using System;
using System.Runtime.InteropServices;

namespace Fomm {
    internal static class NativeMethods {
        [DllImport("ShaderDisasm", CharSet=CharSet.Ansi)]
        public unsafe static extern sbyte* Disasm(byte[] data, int len, byte Color);

        [DllImport("ShaderDisasm", CharSet=CharSet.Ansi)]
        public unsafe static extern byte* Asm(byte[] data, int len);

        [DllImport("ShaderDisasm", CharSet=CharSet.Ansi)]
        public unsafe static extern byte* Compile(string data, int len, string EntryPoint, string Profile, byte Debug);

        [DllImport("kernel32", CharSet=CharSet.Ansi)]
        [System.Security.SuppressUnmanagedCodeSecurity()]
        public static extern int GetPrivateProfileIntA(string section, string value, int def, string path);

        public static void WritePrivateProfileIntA(string section, string value, int val, string path) {
            WritePrivateProfileStringA(section, value, val.ToString(), path);
        }

        [DllImport("kernel32", CharSet=CharSet.Ansi)]
        [System.Security.SuppressUnmanagedCodeSecurity()]
        private static extern int GetPrivateProfileStringA(string section, string value, string def,
            [MarshalAs(UnmanagedType.LPArray)]
            byte[] buf, int buflen, string path);

        public static string GetPrivateProfileString(string section, string value, string def, string path) {
            byte[] buffer=new byte[256];
            int len=GetPrivateProfileStringA(section, value, def, buffer, 256, path);
            return System.Text.Encoding.Default.GetString(buffer, 0, len);
        }

        [DllImport("kernel32", CharSet=CharSet.Ansi)]
        [System.Security.SuppressUnmanagedCodeSecurity()]
        public static extern int WritePrivateProfileStringA(string section, string value, string val, string path);
    }
}
