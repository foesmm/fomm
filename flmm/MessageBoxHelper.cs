using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Fomm
{
  public class MessageBoxHelper
  {
    public enum MessageBoxCheckFlags : uint
    {
      MB_OK = 0x00000000,
      MB_OKCANCEL = 0x00000001,
      MB_YESNO = 0x00000004,
      MB_ICONHAND = 0x00000010,
      MB_ICONQUESTION = 0x00000020,
      MB_ICONEXCLAMATION = 0x00000030,
      MB_ICONINFORMATION = 0x00000040
    }

    [DllImport("shlwapi.dll", SetLastError = true)]
    public static extern int SHMessageBoxCheck(
      [In] IntPtr hwnd,
      [In] String pszText,
      [In] String pszTitle,
      [In] MessageBoxCheckFlags uType,
      [In] int iDefault,
      [In] string pszRegVal
    );
  }
}
