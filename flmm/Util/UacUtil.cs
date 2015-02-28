using System;
using System.Runtime.InteropServices;

namespace Fomm.Util
{

  #region Enumerations

  /// <summary>
  ///   The TOKEN_ELEVATION enum.
  /// </summary>
  public struct TOKEN_ELEVATION
  {
    public UInt32 TokenIsElevated;
  }

  /// <summary>
  ///   The TOKEN_INFORMATION_CLASS enum.
  /// </summary>
  public enum TOKEN_INFORMATION_CLASS
  {
    TokenUser = 1,
    TokenGroups = 2,
    TokenPrivileges = 3,
    TokenOwner = 4,
    TokenPrimaryGroup = 5,
    TokenDefaultDacl = 6,
    TokenSource = 7,
    TokenType = 8,
    TokenImpersonationLevel = 9,
    TokenStatistics = 10,
    TokenRestrictedSids = 11,
    TokenSessionId = 12,
    TokenGroupsAndPrivileges = 13,
    TokenSessionReference = 14,
    TokenSandBoxInert = 15,
    TokenAuditPolicy = 16,
    TokenOrigin = 17,
    TokenElevationType = 18,
    TokenLinkedToken = 19,
    TokenElevation = 20,
    TokenHasRestrictions = 21,
    TokenAccessInformation = 22,
    TokenVirtualizationAllowed = 23,
    TokenVirtualizationEnabled = 24,
    TokenIntegrityLevel = 25,
    TokenUIAccess = 26,
    TokenMandatoryPolicy = 27,
    TokenLogonSid = 28,
    MaxTokenInfoClass = 29
  }

  #endregion

  /// <summary>
  ///   Utility class for getting information about UAC.
  /// </summary>
  public class UacUtil
  {
    /// <summary>
    ///   Constant from the Windows SDK.
    /// </summary>
    public const uint TOKEN_QUERY = 0x0008;

    /// <summary>
    ///   Opens the access token of a process.
    /// </summary>
    /// <param name="ProcessHandle">The process whose token is to be opened.</param>
    /// <param name="DesiredAccess">The desired access token we wish to open.</param>
    /// <param name="TokenHandle">The output parameter for the opened token.</param>
    /// <returns>
    ///   <lang langref="true" /> if the desired token was opened;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

    /// <summary>
    ///   Gets the current process's handle.
    /// </summary>
    /// <returns>The cur</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetCurrentProcess();

    /// <summary>
    ///   Gets information about the specified process access token.
    /// </summary>
    /// <param name="TokenHandle">The handle to the token about which to get the information.</param>
    /// <param name="TokenInformationClass">The type of infromation we want.</param>
    /// <param name="TokenInformation">The structure into which the information will be copied.</param>
    /// <param name="TokenInformationLength">The length of the information data structure.</param>
    /// <param name="ReturnLength">The length of the return information.</param>
    /// <returns>
    ///   <lang langref="true" /> if the information was successfully retrieved;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetTokenInformation(
      IntPtr TokenHandle,
      TOKEN_INFORMATION_CLASS TokenInformationClass,
      IntPtr TokenInformation,
      uint TokenInformationLength,
      out uint ReturnLength);

    /// <summary>
    ///   Closes the given handle.
    /// </summary>
    /// <param name="hObject">The handle to close.</param>
    /// <returns>
    ///   <lang langref="true" /> if the was closed;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    /// <summary>
    ///   Loads the specified library.
    /// </summary>
    /// <param name="lpFileName">The library to load.</param>
    /// <returns>A handle to the loaded library.</returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = false)]
    public static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
    public static extern IntPtr GetProcAddress(IntPtr hmodule, string procName);

    /// <summary>
    ///   Gets whether the OS has UAC.
    /// </summary>
    /// <value>Whether the OS has UAC.</value>
    public static bool IsUACOperatingSystem
    {
      get
      {
        //TODO: check for native C# way to check this out
        var hmodule = LoadLibrary("kernel32");

        //a function that only exists on Vista and above
        // this is a hack, as the function we use may not exist on some future OS
        var strFunction = "CreateThreadpoolWait";
        return ((hmodule.ToInt64() != 0) && (GetProcAddress(hmodule, strFunction).ToInt64() != 0));
      }
    }

    /// <summary>
    ///   Gets whether or not the current process is elevated.
    /// </summary>
    /// <remarks>
    ///   This return <lang langref="true" /> if:
    ///   The current OS supports UAC, UAC is on, and the process is being run as an elevated user.
    ///   OR
    ///   The current OS supports UAC, UAC is off, and the process is being run by an administrator.
    ///   OR
    ///   The current OS doesn't support UAC.
    ///   Otherwise, this returns <lang langref="false" />.
    /// </remarks>
    /// <value>Whether or not the current process is elevated.</value>
    public static bool IsElevated
    {
      get
      {
        if (!IsUACOperatingSystem)
        {
          return true;
        }

        bool booCallSucceeded;
        IntPtr hToken;

        var ptrProcessHandle = GetCurrentProcess();
        if (ptrProcessHandle == IntPtr.Zero)
        {
          throw new Exception("Could not get hanlde to current process.");
        }

        if (!(OpenProcessToken(ptrProcessHandle, TOKEN_QUERY, out hToken)))
        {
          throw new Exception("Could not open process token.");
        }

        try
        {
          TOKEN_ELEVATION tevTokenElevation;
          tevTokenElevation.TokenIsElevated = 0;

          var intTokenElevationSize = Marshal.SizeOf(tevTokenElevation);
          var pteTokenElevation = Marshal.AllocHGlobal(intTokenElevationSize);
          try
          {
            Marshal.StructureToPtr(tevTokenElevation, pteTokenElevation, true);
            UInt32 uintReturnLength;
            booCallSucceeded = GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevation, pteTokenElevation,
                                                   (UInt32) intTokenElevationSize, out uintReturnLength);
            if ((!booCallSucceeded) || (intTokenElevationSize != uintReturnLength))
            {
              throw new Exception("Could not get token information.");
            }
            tevTokenElevation = (TOKEN_ELEVATION) Marshal.PtrToStructure(pteTokenElevation, typeof (TOKEN_ELEVATION));
          }
          finally
          {
            Marshal.FreeHGlobal(pteTokenElevation);
          }

          return (tevTokenElevation.TokenIsElevated != 0);
        }
        finally
        {
          CloseHandle(hToken);
        }
      }
    }
  }
}