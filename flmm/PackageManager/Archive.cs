using System;
using SevenZip;
using System.Collections.Generic;
using System.IO;
using Fomm.Util;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Fomm.PackageManager
{
  /// <summary>
  /// Encapsulates the interactions with an archive file.
  /// </summary>
  public class Archive : IDisposable
  {
    /// <summary>
    /// Raised when the files in the archive have changed.
    /// </summary>
    public event EventHandler FilesChanged = delegate
    {
    };

    /// <summary>
    /// The path prefix use to identify a file as being contained in an archive.
    /// </summary>
    public const string ARCHIVE_PREFIX = "arch:";

    protected static List<string> m_lstNonArchiveExtensions = new List<string>
    {
      ".esp",
      ".esm",
      ".txt",
      ".htm",
      ".html",
      ".nif",
      ".dds",
      ".png",
      ".rtf",
      ".jpg",
      ".bmp",
      ".cs",
      ".xml",
      ".xsd",
      ".ico"
    };

    private string m_strPath;
    private SevenZipCompressor m_szcCompressor;
    private List<string> m_strFiles;
    private Dictionary<string, ArchiveFileInfo> m_dicFileInfo;
    private bool m_booCanEdit;
    private bool m_booIsSolid;
    private ThreadSafeSevenZipExtractor m_szeReadOnlyExtractor;
    private string m_strReadOnlyTempDirectory;

    #region Properties

    /// <summary>
    /// Gets whether or not the archive is read-only.
    /// </summary>
    /// <remarks>
    /// RAR files are the only read-only archives. This is because FOMM isn't allow to create/edit RAR files
    /// (from a licensing standpoint).
    /// </remarks>
    /// <value>Whether or not the archive is read-only.</value>
    public bool ReadOnly
    {
      get
      {
        return !m_booCanEdit;
      }
    }

    /// <summary>
    /// Gets the path of the archive.
    /// </summary>
    /// <value>The path of the archive.</value>
    public string ArchivePath
    {
      get
      {
        return m_strPath;
      }
    }

    /// <summary>
    /// Gets the names of the volumes that make up this archive.
    /// </summary>
    /// <value>The names of the volumes that make up this archive.</value>
    public string[] VolumeFileNames
    {
      get
      {
        using (var szeExtractor = GetExtractor(m_strPath))
        {
          IList<string> lstVolumes = szeExtractor.VolumeFileNames;
          var strVolumes = new string[lstVolumes.Count];
          lstVolumes.CopyTo(strVolumes, 0);
          return strVolumes;
        }
      }
    }

    /// <summary>
    /// Gets whether the archive is solid.
    /// </summary>
    /// <value>Whether the archive is solid.</value>
    public bool IsSolid
    {
      get
      {
        return m_booIsSolid;
      }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets a <see cref="SevenZipExtractor"/> for the given path.
    /// </summary>
    /// <remarks>
    /// This builds a <see cref="SevenZipExtractor"/> for the given path. The path can
    /// be to a nested archive (an archive in another archive).
    /// </remarks> 
    /// <param name="p_strPath">The path to the archive for which to get a <see cref="SevenZipExtractor"/>.</param>
    /// <returns>A <see cref="SevenZipExtractor"/> for the given path.</returns>
    public static SevenZipExtractor GetExtractor(string p_strPath)
    {
      return (SevenZipExtractor) GetExtractor(p_strPath, false);
    }

    /// <summary>
    /// Gets a <see cref="ThreadSafeSevenZipExtractor"/> for the given path.
    /// </summary>
    /// <remarks>
    /// This builds a <see cref="ThreadSafeSevenZipExtractor"/> for the given path. The path can
    /// be to a nested archive (an archive in another archive).
    /// </remarks> 
    /// <param name="p_strPath">The path to the archive for which to get a <see cref="ThreadSafeSevenZipExtractor"/>.</param>
    /// <returns>A <see cref="ThreadSafeSevenZipExtractor"/> for the given path.</returns>
    public static ThreadSafeSevenZipExtractor GetThreadSafeExtractor(string p_strPath)
    {
      return (ThreadSafeSevenZipExtractor) GetExtractor(p_strPath, true);
    }

    /// <summary>
    /// Gets a <see cref="SevenZipExtractor"/> for the given path.
    /// </summary>
    /// <remarks>
    /// This builds a <see cref="SevenZipExtractor"/> for the given path. The path can
    /// be to a nested archive (an archive in another archive).
    /// </remarks> 
    /// <param name="p_strPath">The path to the archive for which to get a <see cref="SevenZipExtractor"/>.</param>
    /// <param name="p_booThreadSafe">Indicates if the returned extractor need to be thread safe.</param>
    /// <returns>A <see cref="SevenZipExtractor"/> for the given path if the extractor doesn't need to be
    /// thread safe; a <see cref="ThreadSafeSevenZipExtractor"/> otherwise.</returns>
    private static object GetExtractor(string p_strPath, bool p_booThreadSafe)
    {
      if (p_strPath.StartsWith(ARCHIVE_PREFIX))
      {
        var stkFiles = new Stack<KeyValuePair<string, string>>();
        var strPath = p_strPath;
        while (strPath.StartsWith(ARCHIVE_PREFIX))
        {
          stkFiles.Push(ParseArchivePath(strPath));
          strPath = stkFiles.Peek().Key;
        }
        var stkExtractors = new Stack<SevenZipExtractor>();
        try
        {
          var kvpArchive = stkFiles.Pop();
          var szeArchive = new SevenZipExtractor(kvpArchive.Key);
          stkExtractors.Push(szeArchive);
          for (; stkFiles.Count > 0; kvpArchive = stkFiles.Pop())
          {
            var msmArchive = new MemoryStream();
            szeArchive.ExtractFile(kvpArchive.Value, msmArchive);
            msmArchive.Position = 0;
            szeArchive = new SevenZipExtractor(msmArchive);
            stkExtractors.Push(szeArchive);
          }

          var msmFile = new MemoryStream();
          szeArchive.ExtractFile(kvpArchive.Value, msmFile);
          msmFile.Position = 0;
          if (p_booThreadSafe)
          {
            return new ThreadSafeSevenZipExtractor(msmFile);
          }
          return new SevenZipExtractor(msmFile);
        }
        finally
        {
          while (stkExtractors.Count > 0)
          {
            stkExtractors.Pop().Dispose();
          }
        }
      }
      else
      {
        if (p_booThreadSafe)
        {
          return new ThreadSafeSevenZipExtractor(p_strPath);
        }
        return new SevenZipExtractor(p_strPath);
      }
    }

    /// <summary>
    /// Determines whether or not the file specified by the given path
    /// is an archive.
    /// </summary>
    /// <returns><lang cref="true"/> if the specified file is an archive;
    /// <lang cref="false"/> otherwise.</returns>
    public static bool IsArchive(string p_strPath)
    {
      if (!p_strPath.StartsWith(ARCHIVE_PREFIX) && !File.Exists(p_strPath))
      {
        return false;
      }
      var booIsAchive = true;
      if (m_lstNonArchiveExtensions.Contains(Path.GetExtension(p_strPath)))
      {
        return false;
      }
      try
      {
        using (var szeExtractor = GetExtractor(p_strPath))
        {
          var g = szeExtractor.FilesCount;
        }
      }
      catch (Exception)
      {
        booIsAchive = false;
      }
      return booIsAchive;
    }

    /// <summary>
    /// Changes the directory of the archive referenced in the given path to the specified
    /// new directory.
    /// </summary>
    /// <remarks>
    /// This changes something of the form:
    ///    arch:old\path\archive.zip//interior/path/file.txt
    ///  to:
    ///    arch:new\path\archive.zip//interior/path/file.txt
    /// </remarks>
    /// <param name="p_strArchivePath">The archive path whose directory is to be replaced.</param>
    /// <param name="p_strNewArchiveDirectory">The new directory to put into the given archive path.</param>
    /// <returns>The archive path with the new directory.</returns>
    public static string ChangeArchiveDirectory(string p_strArchivePath, string p_strNewArchiveDirectory)
    {
      if (!p_strArchivePath.StartsWith(ARCHIVE_PREFIX))
      {
        throw new ArgumentException("The given path is not an archive path: " + p_strArchivePath, "p_strArchivePath");
      }
      var strNewDirectory = p_strNewArchiveDirectory ?? "";
      var kvpArchive = ParseArchivePath(p_strArchivePath);
      var stkArchives = new Stack<string>();
      while (kvpArchive.Key.StartsWith(ARCHIVE_PREFIX))
      {
        stkArchives.Push(kvpArchive.Value);
        kvpArchive = ParseArchivePath(kvpArchive.Key);
      }
      var strSource = GenerateArchivePath(Path.Combine(strNewDirectory, Path.GetFileName(kvpArchive.Key)),
                                             kvpArchive.Value);
      while (stkArchives.Count > 0)
      {
        strSource = GenerateArchivePath(strSource, stkArchives.Pop());
      }
      return strSource;
    }

    /// <summary>
    /// Parses the given path to extract the path to the archive file, and the path to
    /// a file within said archive.
    /// </summary>
    /// <param name="p_strPath">The file path to parse.</param>
    /// <returns>The path to an archive file, and the path to a file within said archive.</returns>
    public static KeyValuePair<string, string> ParseArchivePath(string p_strPath)
    {
      if (!p_strPath.StartsWith(ARCHIVE_PREFIX))
      {
        return new KeyValuePair<string, string>(null, null);
      }
      var intEndIndex = p_strPath.LastIndexOf("//");
      if (intEndIndex < 0)
      {
        intEndIndex = p_strPath.Length;
      }
      var strArchive = p_strPath.Substring(ARCHIVE_PREFIX.Length, intEndIndex - ARCHIVE_PREFIX.Length);
      var strPath = p_strPath.Substring(intEndIndex + 2);
      return new KeyValuePair<string, string>(strArchive, strPath);
    }

    /// <summary>
    /// Generates a path to a file in an archive.
    /// </summary>
    /// <param name="p_strArchivePath">The path of the archive file.</param>
    /// <param name="p_strInternalPath">The path of the file in the archive.</param>
    /// <returns></returns>
    public static string GenerateArchivePath(string p_strArchivePath, string p_strInternalPath)
    {
      return String.Format("{0}{1}//{2}", ARCHIVE_PREFIX, p_strArchivePath, p_strInternalPath);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor the initializes the object with the given values.
    /// </summary>
    /// <param name="p_strPath">The path to the archive file.</param>
    public Archive(string p_strPath)
    {
      m_strPath = p_strPath;
      if (!p_strPath.StartsWith(ARCHIVE_PREFIX))
      {
        m_strPath = p_strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        using (var szeExtractor = new SevenZipExtractor(m_strPath))
        {
          if (Enum.IsDefined(typeof (OutArchiveFormat), szeExtractor.Format.ToString()))
          {
            m_szcCompressor = new SevenZipCompressor();
            m_szcCompressor.CompressionMode = CompressionMode.Append;
            m_szcCompressor.ArchiveFormat =
              (OutArchiveFormat) Enum.Parse(typeof (OutArchiveFormat), szeExtractor.Format.ToString());
            m_booCanEdit = true;
          }
        }
      }
      m_dicFileInfo = new Dictionary<string, ArchiveFileInfo>(StringComparer.InvariantCultureIgnoreCase);
      m_strFiles = new List<string>();
      LoadFileIndices();
    }

    #endregion

    #region Read Transactions

    /// <summary>
    /// Raised when a read-only initialization step has started.
    /// </summary>
    public event CancelEventHandler ReadOnlyInitStepStarted = delegate
    {
    };

    /// <summary>
    /// Raised when a read-only initialization step has finished.
    /// </summary>
    public event CancelEventHandler ReadOnlyInitStepFinished = delegate
    {
    };

    /// <summary>
    /// Gets whether the archive is in read-only mode.
    /// </summary>
    /// <remarks>
    /// Read-only mode maintains a single extractor for all operations, greatly increasing
    /// extraction speed as the extractor isn't created/destroyed for each operation. While
    /// in read-only mod the underlying file is left open (this class holds a handle to the file).
    /// </remarks>
    /// <value>Whether the archive is in read-only mode.</value>
    protected bool IsReadonly
    {
      get
      {
        return ((object) m_strReadOnlyTempDirectory ?? m_szeReadOnlyExtractor) != null;
      }
    }

    /// <summary>
    /// Gets the number of steps that need to be performed to put the archive into read-only mode.
    /// </summary>
    /// <value>The number of steps that need to be performed to put the archive into read-only mode.</value>
    public Int32 ReadOnlyInitStepCount
    {
      get
      {
        using (var szeExtractor = GetExtractor(m_strPath))
        {
          if (szeExtractor.IsSolid)
          {
            return (Int32) szeExtractor.FilesCount;
          }
        }
        return 1;
      }
    }

    /// <summary>
    /// Starts a read-only transaction.
    /// </summary>
    /// <remarks>
    /// This puts the archive into read-only mode.
    /// </remarks>
    public void BeginReadOnlyTransaction()
    {
      if (m_szeReadOnlyExtractor == null)
      {
        m_szeReadOnlyExtractor = GetThreadSafeExtractor(m_strPath);
        if (m_szeReadOnlyExtractor.IsSolid)
        {
          m_szeReadOnlyExtractor.Dispose();
          m_szeReadOnlyExtractor = null;
          m_strReadOnlyTempDirectory = Program.CreateTempDirectory();
          using (var szeExtractor = GetExtractor(m_strPath))
          {
            szeExtractor.FileExtractionFinished += FileExtractionFinished;
            szeExtractor.FileExtractionStarted += FileExtractionStarted;
            szeExtractor.ExtractArchive(m_strReadOnlyTempDirectory);
          }
        }
      }
    }

    #region Callbacks

    /// <summary>
    /// Called when a file has been extracted from a source archive.
    /// </summary>
    /// <remarks>
    /// This notifies listeners that a read-only initialization step has finished.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void FileExtractionFinished(object sender, FileInfoEventArgs e)
    {
      var ceaArgs = new CancelEventArgs(false);
      ReadOnlyInitStepFinished(this, ceaArgs);
      e.Cancel = ceaArgs.Cancel;
    }

    /// <summary>
    /// Called when a file is about to be extracted from a source archive.
    /// </summary>
    /// <remarks>
    /// This notifies listeners that a read-only initialization step has started.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A <see cref="FileNameEventArgs"/> describing the event arguments.</param>
    private void FileExtractionStarted(object sender, FileInfoEventArgs e)
    {
      var ceaArgs = new CancelEventArgs(false);
      ReadOnlyInitStepStarted(this, ceaArgs);
      e.Cancel = ceaArgs.Cancel;
    }

    #endregion

    /// <summary>
    /// Ends a read-only transaction.
    /// </summary>
    /// <remarks>
    /// This takes the archive out of read-only mode, and releases any used resources.
    /// </remarks>
    public void EndReadOnlyTransaction()
    {
      if (m_szeReadOnlyExtractor != null)
      {
        m_szeReadOnlyExtractor.Dispose();
      }
      m_szeReadOnlyExtractor = null;
      if (!String.IsNullOrEmpty(m_strReadOnlyTempDirectory))
      {
        FileUtil.ForceDelete(m_strReadOnlyTempDirectory);
      }
      m_strReadOnlyTempDirectory = null;
    }

    #endregion

    /// <summary>
    /// Caches information about the files in the archive.
    /// </summary>
    protected void LoadFileIndices()
    {
      m_dicFileInfo.Clear();
      m_strFiles.Clear();
      using (var szeExtractor = GetExtractor(m_strPath))
      {
        m_booIsSolid = szeExtractor.IsSolid;
        foreach (var afiFile in szeExtractor.ArchiveFileData)
        {
          if (!afiFile.IsDirectory)
          {
            m_dicFileInfo[afiFile.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)] =
              afiFile;
            m_strFiles.Add(afiFile.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
          }
        }
      }
      FilesChanged(this, new EventArgs());
    }

    /// <summary>
    /// Determins if the given path is a directory in this archive.
    /// </summary>
    /// <param name="p_strPath">The path to examine.</param>
    /// <returns><lang cref="true"/> if the given path is a directory in this archive;
    /// <lang cref="false"/> otherwise.</returns>
    public bool IsDirectory(string p_strPath)
    {
      var strPath = p_strPath.Trim(new char[]
      {
        Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar
      });
      strPath = strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      var strPathWithSep = strPath + Path.DirectorySeparatorChar;

      if (m_dicFileInfo.ContainsKey(strPath))
      {
        return false;
      }

      foreach (var strFile in m_dicFileInfo.Keys)
      {
        if (strFile.StartsWith(strPathWithSep, StringComparison.InvariantCultureIgnoreCase))
        {
          return true;
        }
      }

      var afiFile = default(ArchiveFileInfo);
      using (var szeExtractor = GetExtractor(m_strPath))
      {
        foreach (var afiTmp in szeExtractor.ArchiveFileData)
        {
          var strArchiveFileName = afiTmp.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
          if (strArchiveFileName.Equals(strPath, StringComparison.InvariantCultureIgnoreCase))
          {
            afiFile = afiTmp;
            break;
          }
        }
      }
      return afiFile.IsDirectory;
    }

    /// <summary>
    /// Gets a list of directories that are in the specified directory in this archive.
    /// </summary>
    /// <param name="p_strDirectory">The directory in the archive whose descendents are to be returned.</param>
    /// <returns>A list of directories that are in the specified directory in this archive.</returns>
    public string[] GetDirectories(string p_strDirectory)
    {
      var strPrefix = p_strDirectory;
      if (String.IsNullOrEmpty(p_strDirectory))
      {
        strPrefix = "";
      }
      strPrefix = strPrefix.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      strPrefix = strPrefix.Trim(Path.DirectorySeparatorChar);
      if (strPrefix.Length > 0)
      {
        strPrefix += Path.DirectorySeparatorChar;
      }
      var lstFolders = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
      foreach (var strFile in m_strFiles)
      {
        if (strFile.StartsWith(strPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
          var intStopIndex = strFile.IndexOf(Path.DirectorySeparatorChar, strPrefix.Length);
          if (intStopIndex < 0)
          {
            continue;
          }
          lstFolders.Add(String.Copy(strFile.Substring(0, intStopIndex)));
        }
      }
      return lstFolders.ToArray();
    }

    /// <summary>
    /// Gets a list of files that are in the specified directory in this archive.
    /// </summary>
    /// <param name="p_strDirectory">The directory in the archive whose descendents are to be returned.</param>
    /// <returns>A list of files that are in the specified directory in this archive.</returns>
    public string[] GetFiles(string p_strDirectory)
    {
      var lstFiles = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
      if (String.IsNullOrEmpty(p_strDirectory))
      {
        m_strFiles.ForEach((s) =>
        {
          lstFiles.Add(String.Copy(s));
        });
      }
      else
      {
        var strPrefix = p_strDirectory;
        strPrefix = strPrefix.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        strPrefix = strPrefix.Trim(Path.DirectorySeparatorChar);
        if (strPrefix.Length > 0)
        {
          strPrefix += Path.DirectorySeparatorChar;
        }
        foreach (var strFile in m_strFiles)
        {
          if (strFile.StartsWith(strPrefix, StringComparison.InvariantCultureIgnoreCase))
          {
            var intStopIndex = strFile.IndexOf(Path.DirectorySeparatorChar, strPrefix.Length);
            if (intStopIndex > 0)
            {
              continue;
            }
            lstFiles.Add(String.Copy(strFile));
          }
        }
      }
      return lstFiles.ToArray();
    }

    /// <summary>
    /// Gets a list of files that are in the specified directory and match the given pattern in this archive.
    /// </summary>
    /// <param name="p_strDirectory">The directory in the archive whose descendents are to be returned.</param>
    /// <param name="p_strPattern">The filename pattern of the files to be returned.</param>
    /// <returns>A list of files that are in the specified directory and match the given pattern in this archive.</returns>
    public string[] GetFiles(string p_strDirectory, string p_strPattern)
    {
      var lstFiles = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
      var strFiles = GetFiles(p_strDirectory);

      var strPattern = p_strPattern.Replace(".", "\\.").Replace("*", ".*");
      var rgxPattern = new Regex(strPattern);

      foreach (var strFile in strFiles)
      {
        if (rgxPattern.IsMatch(strFile))
        {
          lstFiles.Add(strFile);
        }
      }
      return lstFiles.ToArray();
    }

    /// <summary>
    /// Determins if the archive contains the specified file.
    /// </summary>
    /// <param name="p_strPath">The path of the file whose presence in the archive is to be determined.</param>
    /// <returns><lang cref="true"/> if the file is in the archive;
    /// <lang cref="false"/> otherwise.</returns>
    public bool ContainsFile(string p_strPath)
    {
      var strPath = p_strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();
      return m_dicFileInfo.ContainsKey(strPath);
    }

    // Based on Archive.GetFileContents.
    public void ExtractTo(string srcFN, string dstFN)
    {
      string strPath;
      ArchiveFileInfo afiFile;
      FileStream fsOut;

      strPath = srcFN.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();

      if (!m_dicFileInfo.ContainsKey(strPath))
      {
        throw new FileNotFoundException("The requested file does not exist in the archive.", srcFN);
      }

      afiFile = m_dicFileInfo[strPath];

      if (IsReadonly)
      {
        if (m_szeReadOnlyExtractor == null)
        {
          File.Copy(Path.Combine(m_strReadOnlyTempDirectory, strPath), dstFN, true);
        }
        else
        {
          fsOut = new FileStream(dstFN, FileMode.Truncate);
          m_szeReadOnlyExtractor.ExtractFile(afiFile.Index, fsOut);
          fsOut.Close();
        }
      }
      else
      {
        using (var szeExtractor = GetExtractor(m_strPath))
        {
          fsOut = new FileStream(dstFN, FileMode.Truncate);
          szeExtractor.ExtractFile(afiFile.Index, fsOut);
          fsOut.Close();
        }
      }
    }

    /// <summary>
    /// Gets the contents of the specified file in the archive.
    /// </summary>
    /// <param name="p_strPath">The file whose contents are to be retrieved.</param>
    /// <returns>The contents of the specified file in the archive.</returns>
    public byte[] GetFileContents(string p_strPath)
    {
      var strPath = p_strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();
      if (!m_dicFileInfo.ContainsKey(strPath))
      {
        throw new FileNotFoundException("The requested file does not exist in the archive.", p_strPath);
      }

      byte[] bteFile = null;
      var afiFile = m_dicFileInfo[strPath];
      bteFile = new byte[afiFile.Size];
      using (var msmFile = new MemoryStream(bteFile))
      {
        //check to see if we are on the same thread as the extractor
        // if not, then marshall the call to the extractor's thread.
        // this needs to be done as the 7zip dll cannot handle calls from other
        // threads.
        if (IsReadonly)
        {
          if (m_szeReadOnlyExtractor == null)
          {
            bteFile = File.ReadAllBytes(Path.Combine(m_strReadOnlyTempDirectory, strPath));
          }
          else
          {
            m_szeReadOnlyExtractor.ExtractFile(afiFile.Index, msmFile);
          }
        }
        else
        {
          using (var szeExtractor = GetExtractor(m_strPath))
          {
            szeExtractor.ExtractFile(afiFile.Index, msmFile);
          }
        }
        msmFile.Close();
      }
      if (bteFile.LongLength != (Int64) afiFile.Size)
      {
        //if I understand things correctly, this block should never execute
        // as bteFile should always be exactly the right size to hold the extracted file
        //however, just to be safe, I've included this code to make sure we only return
        // valid bytes
        var bteReal = new byte[afiFile.Size];
        Array.Copy(bteFile, bteReal, bteReal.LongLength);
        bteFile = bteReal;
      }
      return bteFile;
    }

    /// <summary>
    /// Replaces the specified file in the archive with the given data.
    /// </summary>
    /// <remarks>
    /// If the specified file doesn't exist in the archive, the file is added.
    /// </remarks>
    /// <param name="p_strFileName">The path to the file to replace in the archive.</param>
    /// <param name="p_strData">The new file data.</param>
    public void ReplaceFile(string p_strFileName, string p_strData)
    {
      ReplaceFile(p_strFileName, Encoding.Default.GetBytes(p_strData));
    }

    /// <summary>
    /// Replaces the specified file in the archive with the given data.
    /// </summary>
    /// <remarks>
    /// If the specified file doesn't exist in the archive, the file is added.
    /// </remarks>
    /// <param name="p_strFileName">The path to the file to replace in the archive.</param>
    /// <param name="p_bteData">The new file data.</param>
    /// <exception cref="InvalidOperationException">Thrown if modification of archives of the current
    /// archive type is not supported.</exception>
    /// <exception cref="InvalidOperationException">Thrown if modification of archive is attempted
    /// while the archive is in a ready only transaction.</exception>
    public void ReplaceFile(string p_strFileName, byte[] p_bteData)
    {
      if (IsReadonly)
      {
        throw new InvalidOperationException("Cannot replace a file while Archive is in a Read Only Transaction.");
      }
      if (!m_booCanEdit)
      {
        using (var szeExtractor = GetExtractor(m_strPath))
        {
          throw new InvalidOperationException("Cannot modify archive of type: " + szeExtractor.Format);
        }
      }
      var strPath =
        p_strFileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();
      if (m_dicFileInfo.ContainsKey(strPath))
      {
        var dicDelete = new Dictionary<int, string>()
        {
          {
            m_dicFileInfo[strPath].Index, null
          }
        };
        m_szcCompressor.ModifyArchive(m_strPath, dicDelete);
      }
      using (var msmData = new MemoryStream(p_bteData))
      {
        m_szcCompressor.CompressStreamDictionary(new Dictionary<string, Stream>()
        {
          {
            p_strFileName, msmData
          }
        }, m_strPath);
        msmData.Close();
      }
      LoadFileIndices();
    }

    /// <summary>
    /// Deletes the specified file from the archive.
    /// </summary>
    /// <remarks>
    /// If the specified file doesn't exist in the archive, nothing is done.
    /// </remarks>
    /// <param name="p_strFileName">The path to the file to delete from the archive.</param>
    /// <exception cref="InvalidOperationException">Thrown if modification of archives of the current
    /// archive type is not supported.</exception>
    /// <exception cref="InvalidOperationException">Thrown if modification of archive is attempted
    /// while the archive is in a ready only transaction.</exception>
    public void DeleteFile(string p_strFileName)
    {
      if (IsReadonly)
      {
        throw new InvalidOperationException("Cannot delete a file while Archive is in a Read Only Transaction.");
      }
      if (!m_booCanEdit)
      {
        using (var szeExtractor = GetExtractor(m_strPath))
        {
          throw new InvalidOperationException("Cannot modify archive of type: " + szeExtractor.Format);
        }
      }
      var strPath =
        p_strFileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();
      if (m_dicFileInfo.ContainsKey(strPath))
      {
        var dicDelete = new Dictionary<int, string>()
        {
          {
            m_dicFileInfo[strPath].Index, null
          }
        };
        m_szcCompressor.ModifyArchive(m_strPath, dicDelete);
      }
      LoadFileIndices();
    }

    #region IDisposable Members

    /// <summary>
    /// Disposes of the resources used by the object.
    /// </summary>
    public void Dispose()
    {
      EndReadOnlyTransaction();
    }

    #endregion
  }
}