using System;
using Fomm.PackageManager;
using DialogResult = System.Windows.Forms.DialogResult;
using System.Windows.Forms;
using System.IO;

namespace fomm.Scripting
{
  public struct SelectOption
  {
    public string Item;
    public string Preview;
    public string Desc;

    public SelectOption(string item, string preview, string desc)
    {
      Item = item;
      Preview = preview;
      Desc = desc;
    }
  }

  public enum TextureFormat
  {
    R8G8B8 = 20,
    A8R8G8B8 = 21,
    X8R8G8B8 = 22,
    DXT1 = ('D' << 0) | ('X' << 8) | ('T' << 16) | ('1' << 24),
    DXT3 = ('D' << 0) | ('X' << 8) | ('T' << 16) | ('3' << 24),
    DXT5 = ('D' << 0) | ('X' << 8) | ('T' << 16) | ('5' << 24),
  }

  /// <summary>
  /// The base class for custom install scripts.
  /// </summary>
  public abstract class GenericBaseScript
  {
    private static string m_strLastError;

    protected static ModInstaller Installer { get; private set; }

    protected static string LastError
    {
      get
      {
        return m_strLastError;
      }
      set
      {
        m_strLastError = value;
      }
    }

    /// <summary>
    /// Sets up the script.
    /// </summary>
    /// <remarks>
    /// This method sets the <see cref="ModInstaller"/> this script will use
    /// to perform its work.
    /// </remarks>
    /// <param name="p_mdiScript">The <see cref="ModInstaller"/> this script will use
    /// to perform its work.</param>
    public static void Setup(ModInstaller p_mdiScript)
    {
      Installer = p_mdiScript;
    }

    #region Method Execution

    protected delegate void GenereicVoidMethodDelegate();

    protected delegate object GenereicReturnMethodDelegate();

    /// <summary>
    /// Executes the given void method.
    /// </summary>
    /// <remarks>
    /// This method is used to execute all void method calls the script needs to make.
    /// This allows for centralized error handling.
    /// 
    /// It should be noted that using delegates does engender a very slight performance hit,
    /// but given the nature of this application (more precisely, that this is a single-user
    /// application) there should not be any noticable difference.
    /// </remarks>
    /// <param name="p_gmdMethod">The method to execute.</param>
    /// <see cref="ExecuteMethod(GenereicReturnMethodDelegate)"/>
    protected static void ExecuteMethod(GenereicVoidMethodDelegate p_gmdMethod)
    {
      try
      {
        p_gmdMethod();
      }
      catch (Exception e)
      {
        m_strLastError = e.Message;
        if (e.InnerException != null)
        {
          m_strLastError += "\n" + e.InnerException.Message;
        }
      }
    }

    /// <summary>
    /// Executes the given method with a return value.
    /// </summary>
    /// <remarks>
    /// This method is used to execute all method calls that return a value that
    /// the script needs to make. This allows for centralized error handling.
    /// 
    /// It should be noted that using delegates does engender a very slight performance hit,
    /// but given the nature of this application (more precisely, that this is a single-user
    /// application) there should not be any noticable difference.
    /// </remarks>
    /// <param name="p_gmdMethod">The method to execute.</param>
    /// <see cref="ExecuteMethod(GenereicVoidMethodDelegate)"/>
    protected static object ExecuteMethod(GenereicReturnMethodDelegate p_gmdMethod)
    {
      try
      {
        return p_gmdMethod();
      }
      catch (Exception e)
      {
        m_strLastError = e.Message;
        if (e.InnerException != null)
        {
          m_strLastError += "\n" + e.InnerException.Message;
        }
      }
      return null;
    }

    #endregion

    /// <summary>
    /// Returns the last error that occurred.
    /// </summary>
    /// <returns>The last error that occurred.</returns>
    public static string GetLastError()
    {
      return m_strLastError;
    }

    #region Installation

    /// <summary>
    /// Performs a basic install of the mod.
    /// </summary>
    /// <remarks>
    /// A basic install installs all of the file in the mod to the Data directory
    /// or activates all esp and esm files.
    /// </remarks>
    /// <seealso cref="ModInstaller.PerformBasicInstall()"/>
    public static void PerformBasicInstall()
    {
      ExecuteMethod(() => Installer.PerformBasicInstall());
    }

    #endregion

    #region File Management

    /// <summary>
    /// Installs the specified file from the FOMod to the specified location on the file system.
    /// </summary>
    /// <param name="p_strFrom">The path of the file in the FOMod to install.</param>
    /// <param name="p_strTo">The path on the file system where the file is to be created.</param>
    /// <returns><lang langref="true"/> if the file was written; <lang langref="false"/> otherwise.</returns>
    /// <seealso cref="ModInstaller.CopyDataFile(string, string)"/>
    public static bool CopyDataFile(string p_strFrom, string p_strTo)
    {
      return (bool) (ExecuteMethod(() => Installer.Script.CopyDataFile(p_strFrom, p_strTo)) ?? false);
    }

    /// <summary>
    /// Retrieves the list of files in the FOMod.
    /// </summary>
    /// <returns>The list of files in the FOMod.</returns>
    /// <seealso cref="fomod.GetFileList()"/>
    public static string[] GetFomodFileList()
    {
      var strFiles = (string[]) ExecuteMethod(() => Installer.Fomod.GetFileList().ToArray());
      for (var i = strFiles.Length - 1; i >= 0; i--)
      {
        strFiles[i] = strFiles[i].Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      }
      return strFiles;
    }

    /// <summary>
    /// Installs the speified file from the FOMod to the file system.
    /// </summary>
    /// <param name="p_strFile">The path of the file to install.</param>
    /// <returns><lang langref="true"/> if the file was written; <lang langref="false"/> otherwise.</returns>
    /// <seealso cref="ModInstaller.InstallFileFromFomod(string)"/>
    public static bool InstallFileFromFomod(string p_strFile)
    {
      return (bool) (ExecuteMethod(() => Installer.Script.InstallFileFromFomod(p_strFile)) ?? false);
    }

    /// <summary>
    /// Retrieves the specified file from the fomod.
    /// </summary>
    /// <param name="p_strFile">The file to retrieve.</param>
    /// <returns>The requested file data.</returns>
    /// <seealso cref="fomod.GetFile(string)"/>
    public static byte[] GetFileFromFomod(string p_strFile)
    {
      return (byte[]) ExecuteMethod(() => Installer.Fomod.GetFile(p_strFile));
    }

    /// <summary>
    /// Gets a filtered list of all files in a user's Data directory.
    /// </summary>
    /// <param name="p_strPath">The subdirectory of the Data directory from which to get the listing.</param>
    /// <param name="p_strPattern">The pattern against which to filter the file paths.</param>
    /// <param name="p_booAllFolders">Whether or not to search through subdirectories.</param>
    /// <returns>A filtered list of all files in a user's Data directory.</returns>
    /// <seealso cref="FileManagement.GetExistingDataFileList(string, string, bool)"/>
    public static string[] GetExistingDataFileList(string p_strPath, string p_strPattern, bool p_booAllFolders)
    {
      return
        (string[]) ExecuteMethod(() => FileManagement.GetExistingDataFileList(p_strPath, p_strPattern, p_booAllFolders));
    }

    /// <summary>
    /// Determines if the specified file exists in the user's Data directory.
    /// </summary>
    /// <param name="p_strPath">The path of the file whose existence is to be verified.</param>
    /// <returns><lang langref="true"/> if the specified file exists; <lange langref="false"/>
    /// otherwise.</returns>
    /// <see cref="FileManagement.DataFileExists(string)"/>
    public static bool DataFileExists(string p_strPath)
    {
      return (bool) (ExecuteMethod(() => FileManagement.DataFileExists(p_strPath)) ?? false);
    }

    /// <summary>
    /// Gets the speified file from the user's Data directory.
    /// </summary>
    /// <param name="p_strPath">The path of the file to retrieve.</param>
    /// <returns>The specified file, or <lang langref="null"/> if the file does not exist.</returns>
    /// <see cref="FileManagement.GetExistingDataFile(string)"/>
    public static byte[] GetExistingDataFile(string p_strPath)
    {
      return (byte[]) ExecuteMethod(() => FileManagement.GetExistingDataFile(p_strPath));
    }

    /// <summary>
    /// Writes the file represented by the given byte array to the given path.
    /// </summary>
    /// <remarks>
    /// This method writes the given data as a file at the given path. If the file
    /// already exists the user is prompted to overwrite the file.
    /// </remarks>
    /// <param name="p_strPath">The path where the file is to be created.</param>
    /// <param name="p_bteData">The data that is to make up the file.</param>
    /// <returns><lang langref="true"/> if the file was written; <lang langref="false"/> otherwise.</returns>
    /// <seealso cref="ModInstaller.GenerateDataFile(string, byte[])"/>
    public static bool GenerateDataFile(string p_strPath, byte[] p_bteData)
    {
      return (bool) (ExecuteMethod(() => Installer.Script.GenerateDataFile(p_strPath, p_bteData)) ?? false);
    }

    #endregion

    #region UI

    #region MessageBox

    /// <summary>
    /// Shows a message box with the given message.
    /// </summary>
    /// <param name="p_strMessage">The message to display in the message box.</param>
    /// <seealso cref="ModScript.MessageBox(string)"/>
    public static void MessageBox(string p_strMessage)
    {
      ExecuteMethod(() => Installer.Script.MessageBox(p_strMessage));
    }

    /// <summary>
    /// Shows a message box with the given message and title.
    /// </summary>
    /// <param name="p_strMessage">The message to display in the message box.</param>
    /// <param name="p_strTitle">The message box's title, display in the title bar.</param>
    /// <seealso cref="ModScript.MessageBox(string, string)"/>
    public static void MessageBox(string p_strMessage, string p_strTitle)
    {
      ExecuteMethod(() => Installer.Script.MessageBox(p_strMessage, p_strTitle));
    }

    /// <summary>
    /// Shows a message box with the given message, title, and buttons.
    /// </summary>
    /// <param name="p_strMessage">The message to display in the message box.</param>
    /// <param name="p_strTitle">The message box's title, display in the title bar.</param>
    /// <param name="p_mbbButtons">The buttons to show in the message box.</param>
    /// <seealso cref="ModScript.MessageBox(string, string, MessageBoxButtons)"/>
    public static DialogResult MessageBox(string p_strMessage, string p_strTitle, MessageBoxButtons p_mbbButtons)
    {
      return (DialogResult) ExecuteMethod(() => Installer.Script.MessageBox(p_strMessage, p_strTitle, p_mbbButtons));
    }

    #endregion

    /// <summary>
    /// Displays a selection form to the user.
    /// </summary>
    /// <param name="p_sopOptions">The options from which to select.</param>
    /// <param name="p_strTitle">The title of the selection form.</param>
    /// <param name="p_booSelectMany">Whether more than one item can be selected.</param>
    /// <returns>The indices of the selected items.</returns>
    /// <seealso cref="Select(string[], string[], string[], string, bool)"/>
    public static int[] Select(SelectOption[] p_sopOptions, string p_strTitle, bool p_booSelectMany)
    {
      var booHasPreviews = false;
      var booHasDescriptions = false;
      foreach (var so in p_sopOptions)
      {
        if (so.Preview != null)
        {
          booHasPreviews = true;
        }
        if (so.Desc != null)
        {
          booHasDescriptions = true;
        }
      }
      var strItems = new string[p_sopOptions.Length];
      var strPreviews = booHasPreviews ? new string[p_sopOptions.Length] : null;
      var strDescriptions = booHasDescriptions ? new string[p_sopOptions.Length] : null;
      for (var i = 0; i < p_sopOptions.Length; i++)
      {
        strItems[i] = p_sopOptions[i].Item;
        if (booHasPreviews)
        {
          strPreviews[i] = p_sopOptions[i].Preview;
        }
        if (booHasDescriptions)
        {
          strDescriptions[i] = p_sopOptions[i].Desc;
        }
      }
      return Select(strItems, strPreviews, strDescriptions, p_strTitle, p_booSelectMany);
    }

    /// <summary>
    /// Displays a selection form to the user.
    /// </summary>
    /// <remarks>
    /// The items, previews, and descriptions are repectively ordered. In other words,
    /// the i-th item in <paramref name="p_strItems"/> uses the i-th preview in
    /// <paramref name="p_strPreviews"/> and the i-th description in <paramref name="p_strDescriptions"/>.
    /// 
    /// Similarly, the idices return as results correspond to the indices of the items in
    /// <paramref name="p_strItems"/>.
    /// </remarks>
    /// <param name="p_strItems">The items from which to select.</param>
    /// <param name="p_strPreviews">The preview image file names for the items.</param>
    /// <param name="p_strDescriptions">The descriptions of the items.</param>
    /// <param name="p_strTitle">The title of the selection form.</param>
    /// <param name="p_booSelectMany">Whether more than one item can be selected.</param>
    /// <returns>The indices of the selected items.</returns>
    /// <seealso cref="ModScript.Select(string[], string[], string[], string, bool)"/>
    public static int[] Select(string[] p_strItems, string[] p_strPreviews, string[] p_strDescriptions,
                               string p_strTitle, bool p_booSelectMany)
    {
      return
        (int[])
          ExecuteMethod(
            () => Installer.Script.Select(p_strItems, p_strPreviews, p_strDescriptions, p_strTitle, p_booSelectMany));
    }

    /// <summary>
    /// Creates a form that can be used in custom mod scripts.
    /// </summary>
    /// <returns>A form that can be used in custom mod scripts.</returns>
    /// <seealso cref="ModScript.CreateCustomForm()"/>
    public static Form CreateCustomForm()
    {
      return (Form) ExecuteMethod(() => Installer.Script.CreateCustomForm());
    }

    #endregion

    #region Version Checking

    /// <summary>
    /// Gets the version of FOMM.
    /// </summary>
    /// <returns>The version of FOMM.</returns>
    /// <seealso cref="ModScript.GetFommVersion()"/>
    public static Version GetFommVersion()
    {
      return (Version) ExecuteMethod(() => Installer.Script.GetFommVersion());
    }

    /// <summary>
    /// Gets the version of the game that is installed.
    /// </summary>
    /// <returns>The version of the game, or <lang langref="null"/> if Fallout
    /// is not installed.</returns>
    /// <seealso cref="ModScript.GetGameVersion()"/>
    public static Version GetGameVersion()
    {
      return (Version) ExecuteMethod(() => Installer.Script.GetGameVersion());
    }

    public static bool MeetsMinimumScriptExtenderVersion(int maj, int minor = 0, int build = 0, int priv = 0)
    {
      return (bool) ExecuteMethod(() => Installer.Script.MeetsMinimumScriptExtenderVersion(maj, minor, build, priv));
    }

    #endregion

    #region Plugin Management

    /// <summary>
    /// Gets a list of all install plugins.
    /// </summary>
    /// <returns>A list of all install plugins.</returns>
    /// <seealso cref="ModScript.GetAllPlugins()"/>
    public static string[] GetAllPlugins()
    {
      return (string[]) ExecuteMethod(() => Installer.Script.GetAllPlugins());
    }

    #region Plugin Activation Management

    /// <summary>
    /// Retrieves a list of currently active plugins.
    /// </summary>
    /// <returns>A list of currently active plugins.</returns>
    /// <seealso cref="ModScript.GetActivePlugins()"/>
    public static string[] GetActivePlugins()
    {
      return (string[]) ExecuteMethod(() => Installer.Script.GetActivePlugins());
    }

    /// <summary>
    /// Sets the activated status of a plugin (i.e., and esp or esm file).
    /// </summary>
    /// <param name="p_strName">The name of the plugin to activate or deactivate.</param>
    /// <param name="p_booActivate">Whether to activate the plugin.</param>
    /// <seealso cref="ModInstaller.SetPluginActivation(string, bool)"/>
    public static void SetPluginActivation(string p_strName, bool p_booActivate)
    {
      ExecuteMethod(() => Installer.Script.SetPluginActivation(p_strName, p_booActivate));
    }

    #endregion

    #region Load Order Management

    /// <summary>
    /// Sets the load order of the plugins.
    /// </summary>
    /// <remarks>
    /// Each plugin will be moved from its current index to its indice's position
    /// in <paramref name="p_intPlugins"/>.
    /// </remarks>
    /// <param name="p_intPlugins">The new load order of the plugins. Each entry in this array
    /// contains the current index of a plugin. This array must contain all current indices.</param>
    /// <seealso cref="ModScript.SetLoadOrder(int[])"/>
    public static void SetLoadOrder(int[] p_intPlugins)
    {
      ExecuteMethod(() => Installer.Script.SetLoadOrder(p_intPlugins));
    }

    /// <summary>
    /// Moves the specified plugins to the given position in the load order.
    /// </summary>
    /// <remarks>
    /// Note that the order of the given list of plugins is not maintained. They are re-ordered
    /// to be in the same order as they are in the before-operation load order. This, I think,
    /// is somewhat counter-intuitive and may change, though likely not so as to not break
    /// backwards compatibility.
    /// </remarks>
    /// <param name="p_intPlugins">The list of plugins to move to the given position in the
    /// load order. Each entry in this array contains the current index of a plugin.</param>
    /// <param name="p_intPosition">The position in the load order to which to move the specified
    /// plugins.</param>
    /// <seealso cref="ModScript.SetLoadOrder(int[], int)"/>
    public static void SetLoadOrder(int[] p_intPlugins, int p_intPosition)
    {
      ExecuteMethod(() => Installer.Script.SetLoadOrder(p_intPlugins, p_intPosition));
    }

    #endregion

    #endregion
  }
}