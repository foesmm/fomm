using System.IO;

namespace Fomm.PackageManager
{
  public static class FileManagement
  {
    /// <summary>
    /// Verifies if the given path is safe to be written to.
    /// </summary>
    /// <remarks>
    /// A path is safe to be written to if it contains no charaters
    /// disallowed by the operating system, and if is is in the Data
    /// directory or one of its sub-directories.
    /// </remarks>
    /// <param name="p_strPath">The path whose safety is to be verified.</param>
    /// <returns><lang langref="true"/> if the given path is safe to write to;
    /// <lang langref="false"/> otherwise.</returns>
    private static bool IsSafeFilePath(string p_strPath)
    {
      if (p_strPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
      {
        return false;
      }
      if (Path.IsPathRooted(p_strPath))
      {
        return false;
      }
      if (p_strPath.Contains(".." + Path.AltDirectorySeparatorChar))
      {
        return false;
      }
      if (p_strPath.Contains(".." + Path.DirectorySeparatorChar))
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Ensures that the given path is safe to be accessed.
    /// </summary>
    /// <param name="p_strPath">The path whose safety is to be verified.</param>
    /// <exception cref="IllegalFilePathException">Thrown if the given path is not safe.</exception>
    /// <seealso cref="IsSafeFilePath"/>
    internal static void AssertFilePathIsSafe(string p_strPath)
    {
      if (!IsSafeFilePath(p_strPath))
      {
        throw new IllegalFilePathException(p_strPath);
      }
    }

    /// <summary>
    /// Determines if the specified file exists in the user's Data directory.
    /// </summary>
    /// <param name="p_strPath">The path of the file whose existence is to be verified.</param>
    /// <returns><lang langref="true"/> if the specified file exists; <lange langref="false"/>
    /// otherwise.</returns>
    /// <exception cref="IllegalFilePathException">Thrown if the given path is not safe.</exception>
    public static bool DataFileExists(string p_strPath)
    {
      AssertFilePathIsSafe(p_strPath);
      PermissionsManager.CurrentPermissions.Assert();
      var datapath = Path.Combine(Program.GameMode.PluginsPath, p_strPath);
      return File.Exists(datapath);
    }

    /// <summary>
    /// Gets a filtered list of all files in a user's Data directory.
    /// </summary>
    /// <param name="p_strPath">The subdirectory of the Data directory from which to get the listing.</param>
    /// <param name="p_strPattern">The pattern against which to filter the file paths.</param>
    /// <param name="p_booAllFolders">Whether or not to search through subdirectories.</param>
    /// <returns>A filtered list of all files in a user's Data directory.</returns>
    /// <exception cref="IllegalFilePathException">Thrown if the given path is not safe.</exception>
    public static string[] GetExistingDataFileList(string p_strPath, string p_strPattern, bool p_booAllFolders)
    {
      AssertFilePathIsSafe(p_strPath);
      PermissionsManager.CurrentPermissions.Assert();
      return Directory.GetFiles(Path.Combine(Program.GameMode.PluginsPath, p_strPath), p_strPattern,
                                p_booAllFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
    }

    /// <summary>
    /// Gets the speified file from the user's Data directory.
    /// </summary>
    /// <param name="p_strPath">The path of the file to retrieve.</param>
    /// <returns>The specified file.</returns>
    /// <exception cref="IllegalFilePathException">Thrown if the given path is not safe.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
    public static byte[] GetExistingDataFile(string p_strPath)
    {
      AssertFilePathIsSafe(p_strPath);
      PermissionsManager.CurrentPermissions.Assert();
      var datapath = Path.Combine(Program.GameMode.PluginsPath, p_strPath);
      if (!File.Exists(datapath))
      {
        throw new FileNotFoundException();
      }
      return File.ReadAllBytes(datapath);
    }
  }
}