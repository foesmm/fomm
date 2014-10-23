using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Fomm.PackageManager.ModInstallLog;
using Fomm.Properties;
using Fomm.Util;
using GeMod.Interface;
using SevenZip;

/*
 * Installed data XML Structure
 * <installData>
 *   <installedFiles>
 *     <file>filepath</file>
 *   </installedFiles>
 *   <sdpEdits>
 *     <sdp package="1" shader=">hexcode</edit>
 * </installData>
 * 
 * Info XML structure
 * <fomod>
 *   <Name>bingle</Name>
 * </fomod>
 */

namespace Fomm.PackageManager
{
  public class fomod : IFomodInfo
  {
    private const string DEFAULT_AUTHOR = "UNKNOWN";
    private const string DEFAULT_VERSION = "1.0";

    private class fomodLoadException : Exception
    {
      public fomodLoadException(string msg) : base(msg) {}
    }

    private Archive m_arcCacheFile;

    internal readonly string filepath;

    public static readonly Version DefaultVersion = new Version(1, 0, 0, 0);
    public static readonly Version DefaultMinFommVersion = new Version(0, 0, 0, 0);

    private static readonly List<string> StopFolders = new List<string>
    {
      "fomod",
      "textures",
      "meshes",
      "music",
      "shaders",
      "video",
      "facegen",
      "menus",
      "lodsettings",
      "lsdata",
      "sound"
    };

    private bool isActive;

    private Dictionary<string, string> m_dicMovedArchiveFiles =
      new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

    private string m_strScreenshotPath;
    private string m_strScriptPath;
    private string m_strReadmePath;

    private readonly string baseName;

    #region Properties

    /// <summary>
    ///   Gets the path to the fomod's cache file.
    /// </summary>
    /// <value>The path to the fomod's cache file.</value>
    public string CachePath { get; private set; }

    /// <summary>
    ///   Gets the path prefix of the FOMod.
    /// </summary>
    /// <remarks>
    ///   The path prefix is used to present the FOMod file structure as if it were rooted at the prefix. This
    ///   normalizes the file structure of FOMods and adjusts for incorrectly packaged FOMods.
    /// </remarks>
    /// <value>The path prefix of the FOMod.</value>
    protected string PathPrefix { get; private set; }

    /// <summary>
    ///   Gets or sets the name of the fomod.
    /// </summary>
    /// <value>The name of the fomod.</value>
    public string ModName { get; set; }

    /// <summary>
    ///   Gets or sets the human readable form of the fomod's version.
    /// </summary>
    /// <value>The human readable form of the fomod's version.</value>
    public string HumanReadableVersion { get; set; }

    /// <summary>
    ///   Gets or sets the version of the fomod.
    /// </summary>
    /// <value>The version of the fomod.</value>
    public Version MachineVersion { get; set; }

    /// <summary>
    ///   Gets or sets the author of the fomod.
    /// </summary>
    /// <value>The author of the fomod.</value>
    public string Author { get; set; }

    /// <summary>
    ///   Gets or sets the description of the fomod.
    /// </summary>
    /// <value>The description of the fomod.</value>
    public string Description { get; set; }

    /// <summary>
    ///   Gets or sets the minimum version of FOMM required to load the fomod.
    /// </summary>
    /// <value>The minimum version of FOMM required to load the fomod.</value>
    public Version MinFommVersion { get; set; }

    /// <summary>
    ///   Gets or sets the contact email of the fomod.
    /// </summary>
    /// <value>The contact email of the fomod.</value>
    public string Email { get; set; }

    /// <summary>
    ///   Gets or sets the website of the fomod.
    /// </summary>
    /// <value>The website of the fomod.</value>
    public string Website { get; set; }

    /// <summary>
    ///   Gets or sets the FOMM groups to which the fomod belongs.
    /// </summary>
    /// <value>The FOMM groups to which the fomod belongs.</value>
    public string[] Groups { get; set; }

    /// <summary>
    ///   Gets the base name of the fomod.
    /// </summary>
    /// <value>The base name of the fomod.</value>
    internal virtual string BaseName
    {
      get
      {
        return baseName;
      }
    }

    /// <summary>
    ///   Gets the fomod file.
    /// </summary>
    /// <remarks>
    ///   The fomod file is a compressed file.
    /// </remarks>
    /// <value>The fomod file.</value>
    internal Archive FomodFile { get; private set; }

    /// <summary>
    ///   Gets whether the mod has metadata.
    /// </summary>
    /// <value>Whether the mod has metadata.</value>
    public bool HasInfo { get; private set; }

    /// <summary>
    ///   Gets or sets whether the mod is active.
    /// </summary>
    /// <value>Whether the mod is active.</value>
    public bool IsActive
    {
      get
      {
        return isActive;
      }
      set
      {
        isActive = value;
      }
    }

    /// <summary>
    ///   Gets whether the mod has a custom install script.
    /// </summary>
    /// <value>Whether the mod has a custom install script.</value>
    public bool HasInstallScript
    {
      get
      {
        return !String.IsNullOrEmpty(m_strScriptPath);
      }
    }

    /// <summary>
    ///   Gets whether or not this fomod requires a script.
    /// </summary>
    /// <remarks>
    ///   A FOMod requires a script if it contains plugins in subfolders.
    /// </remarks>
    /// <value>Whether or not this fomod requires a script.</value>
    public bool RequiresScript
    {
      get
      {
        var lstFiles = GetFileList();
        foreach (var strFile in lstFiles)
        {
          if (Program.GameMode.IsPluginFile(strFile) && !Path.GetFileName(strFile).Equals(strFile))
          {
            return true;
          }
        }
        return false;
      }
    }

    /// <summary>
    ///   Gets whether the mod has a screenshot.
    /// </summary>
    /// <value>Whether the mod has a screenshot.</value>
    public bool HasScreenshot
    {
      get
      {
        return !String.IsNullOrEmpty(m_strScreenshotPath);
      }
    }

    /// <summary>
    ///   Gets whether the mod has a custom uninstall script.
    /// </summary>
    /// <value>Whether the mod has a custom uninstall script.</value>
    public bool HasUninstallScript
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    ///   Gets whether the mod has a readme file.
    /// </summary>
    /// <value>Whether the mod has a readme file.</value>
    public bool HasReadme
    {
      get
      {
        return !String.IsNullOrEmpty(m_strReadmePath);
      }
    }

    /// <summary>
    ///   Gets the number of steps that need to be performed to put the FOMod into read-only mode.
    /// </summary>
    /// <value>The number of steps that need to be performed to put the FOMod into read-only mode.</value>
    public Int32 ReadOnlyInitStepCount
    {
      get
      {
        return FomodFile.ReadOnlyInitStepCount;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   A simple constructor that initializes the object.
    /// </summary>
    /// <param name="path">The path to the fomod file.</param>
    internal fomod(string path)
      : this(path, true) {}

    /// <summary>
    ///   A simple constructor that initializes the object.
    /// </summary>
    /// <param name="path">The path to the fomod file.</param>
    internal fomod(string path, bool p_booUseCache)
    {
      filepath = path;
      ModName = Path.GetFileNameWithoutExtension(path);

      FomodFile = new Archive(path);
      CachePath = Path.Combine(Program.GameMode.ModInfoCacheDirectory, ModName + ".zip");
      if (p_booUseCache && File.Exists(CachePath))
      {
        m_arcCacheFile = new Archive(CachePath);
      }

      FindPathPrefix();
      FomodFile.FilesChanged += Archive_FilesChanged;
      baseName = ModName.ToLowerInvariant();
      Author = DEFAULT_AUTHOR;
      Description = Email = Website = string.Empty;
      HumanReadableVersion = DEFAULT_VERSION;
      MachineVersion = DefaultVersion;
      MinFommVersion = DefaultMinFommVersion;
      Groups = new string[0];
      isActive = (InstallLog.Current.GetModKey(baseName) != null);

      //check for script
      foreach (var name in FomodScript.ScriptNames)
      {
        if (ContainsFile("fomod/" + name))
        {
          m_strScriptPath = "fomod/" + name;
          break;
        }
      }

      //check for readme
      foreach (var ext in Readme.ValidExtensions)
      {
        if (ContainsFile("readme - " + baseName + ext))
        {
          m_strReadmePath = "Readme - " + Path.GetFileNameWithoutExtension(path) + ext;
          break;
        }
      }
      if (String.IsNullOrEmpty(m_strReadmePath))
      {
        foreach (var ext in Readme.ValidExtensions)
        {
          if (ContainsFile("docs/readme - " + baseName + ext))
          {
            m_strReadmePath = "docs/Readme - " + Path.GetFileNameWithoutExtension(path) + ext;
            break;
          }
        }
      }

      //check for screenshot
      var extensions = new[]
      {
        ".png", ".jpg", ".bmp"
      };
      foreach (var ext in extensions)
      {
        if (ContainsFile("fomod/screenshot" + ext))
        {
          m_strScreenshotPath = "fomod/screenshot" + ext;
          break;
        }
      }

      if (p_booUseCache && !File.Exists(CachePath) && (FomodFile.IsSolid || FomodFile.ReadOnly))
      {
        var strTmpInfo = Program.CreateTempDirectory();
        try
        {
          Directory.CreateDirectory(Path.Combine(strTmpInfo, GetPrefixAdjustedPath("fomod")));

          if (ContainsFile("fomod/info.xml"))
          {
            File.WriteAllBytes(Path.Combine(strTmpInfo, GetPrefixAdjustedPath("fomod/info.xml")),
                               GetFileContents("fomod/info.xml"));
          }
          else
          {
            File.WriteAllText(Path.Combine(strTmpInfo, GetPrefixAdjustedPath("fomod/info.xml")), "<fomod/>");
          }

          if (!String.IsNullOrEmpty(m_strReadmePath))
          {
            File.WriteAllBytes(Path.Combine(strTmpInfo, GetPrefixAdjustedPath(m_strReadmePath)),
                               GetFileContents(m_strReadmePath));
          }

          if (!String.IsNullOrEmpty(m_strScreenshotPath))
          {
            File.WriteAllBytes(Path.Combine(strTmpInfo, GetPrefixAdjustedPath(m_strScreenshotPath)),
                               GetFileContents(m_strScreenshotPath));
          }

          var strFilesToCompress = Directory.GetFiles(strTmpInfo, "*.*", SearchOption.AllDirectories);
          if (strFilesToCompress.Length > 0)
          {
            var szcCompressor = new SevenZipCompressor();
            szcCompressor.ArchiveFormat = OutArchiveFormat.Zip;
            szcCompressor.CompressionLevel = CompressionLevel.Ultra;
            szcCompressor.CompressDirectory(strTmpInfo, CachePath);
          }
        }
        finally
        {
          FileUtil.ForceDelete(strTmpInfo);
        }
        if (File.Exists(CachePath))
        {
          m_arcCacheFile = new Archive(CachePath);
        }
      }

      LoadInfo();
    }

    #endregion

    #region Archive Interaction

    /// <summary>
    ///   This finds where in the archive the FOMod file structure begins.
    /// </summary>
    /// <remarks>
    ///   This methods finds the path prefix to the folder containing the core files and folders of the FOMod. If
    ///   there are any files that are above the core folder, than they are given new file names inside the
    ///   core folder.
    /// </remarks>
    /// <seealso cref="PathPrefix" />
    protected void FindPathPrefix()
    {
      m_dicMovedArchiveFiles.Clear();
      var strSourcePath = "/";
      //this code removes any top-level folders until it finds esp/esm/bsa, or the top-level folder
      // is a fomod/textures/meshes/music/shaders/video/facegen/menus/lodsettings/lsdata/sound folder.
      var directories = FomodFile.GetDirectories(strSourcePath);
      while (directories.Length == 1 &&
             ((FomodFile.GetFiles(strSourcePath, "*.esp").Length == 0 &&
               FomodFile.GetFiles(strSourcePath, "*.esm").Length == 0 &&
               FomodFile.GetFiles(strSourcePath, "*.bsa").Length == 0) ||
              Path.GetFileName(directories[0]).Equals("data", StringComparison.InvariantCultureIgnoreCase)))
      {
        directories = directories[0].Split(Path.DirectorySeparatorChar);
        var name = directories[directories.Length - 1].ToLowerInvariant();
        if (!StopFolders.Contains(name))
        {
          foreach (var file in FomodFile.GetFiles(strSourcePath))
          {
            var strNewFileName = Path.GetFileName(file);
            for (var i = 1; m_dicMovedArchiveFiles.ContainsKey(strNewFileName); i++)
            {
              strNewFileName = Path.GetFileNameWithoutExtension(file) + " " + i + Path.GetExtension(file);
            }
            m_dicMovedArchiveFiles[strNewFileName] = file;
          }
          strSourcePath = Path.Combine(strSourcePath, name);
          directories = FomodFile.GetDirectories(strSourcePath);
        }
        else
        {
          break;
        }
      }
      PathPrefix = strSourcePath.Trim('/');
    }

    /// <summary>
    ///   Handles the <see cref="Archive.FilesChanged" /> event of the FOMod's archive.
    /// </summary>
    /// <remarks>
    ///   This ensures that the path prefix that points to the folder in the archive that contains the core files
    ///   and folders of the FOMod is updated when the archive changes.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void Archive_FilesChanged(object sender, EventArgs e)
    {
      FindPathPrefix();
    }

    /// <summary>
    ///   Determines if the FOMod contains the given file.
    /// </summary>
    /// <remarks>
    ///   This method accounts for the <see cref="PathPrefix" />.
    /// </remarks>
    /// <param name="p_strPath">The filename whose existence in the FOMod is to be determined.</param>
    /// <returns><lang langref="true" /> if the specified file is in the FOMod; <lang langref="false" /> otherwise.</returns>
    public bool ContainsFile(string p_strPath)
    {
      var strPath = p_strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      strPath = strPath.Trim(Path.DirectorySeparatorChar);
      if (m_dicMovedArchiveFiles.ContainsKey(strPath))
      {
        return true;
      }
      if (FomodFile.ContainsFile(GetPrefixAdjustedPath(strPath)))
      {
        return true;
      }
      return ((m_arcCacheFile != null) && m_arcCacheFile.ContainsFile(GetPrefixAdjustedPath(strPath)));
    }

    /// <summary>
    ///   This method adjusts the given path to account for the <see cref="PathPrefix" />.
    /// </summary>
    /// <param name="p_strPath">The path to adjust.</param>
    /// <returns>The adjusted path.</returns>
    protected string GetPrefixAdjustedPath(string p_strPath)
    {
      var strPath = p_strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      strPath = strPath.Trim(Path.DirectorySeparatorChar);
      string strAdjustedPath;
      if (!m_dicMovedArchiveFiles.TryGetValue(strPath, out strAdjustedPath))
      {
        strAdjustedPath = Path.Combine(PathPrefix, strPath);
      }
      return strAdjustedPath;
    }

    /// <summary>
    ///   Retrieves the list of files in this FOMod.
    /// </summary>
    /// <remarks>
    ///   This method accounts for the <see cref="PathPrefix" />.
    /// </remarks>
    /// <returns>The list of files in this FOMod.</returns>
    public List<string> GetFileList()
    {
      PermissionsManager.CurrentPermissions.Assert();
      var lstFiles = new List<string>();
      var intTrimLength = (PathPrefix.Length == 0) ? 0 : PathPrefix.Length + 1;
      foreach (var strFile in FomodFile.GetFiles(null))
      {
        if (strFile.StartsWith(PathPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
          var strAdjustedFileName = strFile.Remove(0, intTrimLength);
          if (!strAdjustedFileName.StartsWith("fomod", StringComparison.OrdinalIgnoreCase))
          {
            lstFiles.Add(strAdjustedFileName);
          }
        }
      }
      foreach (var strFile in m_dicMovedArchiveFiles.Keys)
      {
        lstFiles.Add(strFile);
      }
      return lstFiles;
    }

    /// <summary>
    ///   Gets the contents of the specified file.
    /// </summary>
    /// <remarks>
    ///   This method accounts for the <see cref="PathPrefix" />.
    /// </remarks>
    /// <param name="p_strPath">The path of the file whose contents are to be retrieved.</param>
    /// <returns>The contents of the specified file.</returns>
    public byte[] GetFileContents(string p_strPath)
    {
      if ((m_arcCacheFile != null) && m_arcCacheFile.ContainsFile(GetPrefixAdjustedPath(p_strPath)))
      {
        return m_arcCacheFile.GetFileContents(GetPrefixAdjustedPath(p_strPath));
      }
      return FomodFile.GetFileContents(GetPrefixAdjustedPath(p_strPath));
    }

    /// <summary>
    ///   Deletes the specified file.
    /// </summary>
    /// <remarks>
    ///   This method accounts for the <see cref="PathPrefix" />.
    /// </remarks>
    /// <param name="p_strPath">The path of the file to delete.</param>
    protected void DeleteFile(string p_strPath)
    {
      if (!FomodFile.ReadOnly)
      {
        FomodFile.DeleteFile(GetPrefixAdjustedPath(p_strPath));
      }
      if ((m_arcCacheFile != null) && m_arcCacheFile.ContainsFile(GetPrefixAdjustedPath(p_strPath)))
      {
        m_arcCacheFile.DeleteFile(GetPrefixAdjustedPath(p_strPath));
      }
    }

    /// <summary>
    ///   Replaces the specified file with the given data.
    /// </summary>
    /// <remarks>
    ///   This method accounts for the <see cref="PathPrefix" />.
    /// </remarks>
    /// <param name="p_strPath">The path of the file to replace.</param>
    /// <param name="p_bteData">The new file data.</param>
    protected void ReplaceFile(string p_strPath, byte[] p_bteData)
    {
      if (!FomodFile.ReadOnly)
      {
        FomodFile.ReplaceFile(GetPrefixAdjustedPath(p_strPath), p_bteData);
      }
      if ((m_arcCacheFile != null) &&
          (m_arcCacheFile.ContainsFile(GetPrefixAdjustedPath(p_strPath)) || FomodFile.ReadOnly))
      {
        m_arcCacheFile.ReplaceFile(GetPrefixAdjustedPath(p_strPath), p_bteData);
      }
    }

    /// <summary>
    ///   Replaces the specified file with the given text.
    /// </summary>
    /// <remarks>
    ///   This method accounts for the <see cref="PathPrefix" />.
    /// </remarks>
    /// <param name="p_strPath">The path of the file to replace.</param>
    /// <param name="p_strData">The new file text.</param>
    protected void ReplaceFile(string p_strPath, string p_strData)
    {
      if (!FomodFile.ReadOnly)
      {
        FomodFile.ReplaceFile(GetPrefixAdjustedPath(p_strPath), p_strData);
      }
      if ((m_arcCacheFile != null) &&
          (m_arcCacheFile.ContainsFile(GetPrefixAdjustedPath(p_strPath)) || FomodFile.ReadOnly))
      {
        m_arcCacheFile.ReplaceFile(GetPrefixAdjustedPath(p_strPath), p_strData);
      }
    }

    public string ExtractToTemp(string srcFile)
    {
      string ret;

      PermissionsManager.CurrentPermissions.Assert();

      if (!ContainsFile(srcFile))
      {
        throw new FileNotFoundException("File doesn't exist in fomod", srcFile);
      }

      var tmpFN = Path.GetTempFileName();

      if ((m_arcCacheFile != null) && m_arcCacheFile.ContainsFile(GetPrefixAdjustedPath(srcFile)))
      {
        m_arcCacheFile.ExtractTo(GetPrefixAdjustedPath(srcFile), tmpFN);
        ret = tmpFN;
      }
      else
      {
        FomodFile.ExtractTo(GetPrefixAdjustedPath(srcFile), tmpFN);
        ret = tmpFN;
      }

      return ret;
    }

    #endregion

    #region Fomod Info Persistence

    /// <summary>
    ///   Loads the fomod info from the given info file into the given fomod info object.
    /// </summary>
    /// <param name="p_xmlInfo">The XML info file from which to read the info.</param>
    /// <param name="p_finFomodInfo">The fomod info object to populate with the info.</param>
    public static void LoadInfo(XmlDocument p_xmlInfo, IFomodInfo p_finFomodInfo)
    {
      LoadInfo(p_xmlInfo, p_finFomodInfo, true);
    }

    /// <summary>
    ///   Loads the fomod info from the given info file into the given fomod info object.
    /// </summary>
    /// <param name="p_xmlInfo">The XML info file from which to read the info.</param>
    /// <param name="p_finFomodInfo">The fomod info object to populate with the info.</param>
    /// <param name="p_booOverwriteExisitngValues">
    ///   Whether or not to overwrite any existing values
    ///   in the given <see cref="IFomodInfo" />.
    /// </param>
    public static void LoadInfo(XmlDocument p_xmlInfo, IFomodInfo p_finFomodInfo, bool p_booOverwriteExisitngValues)
    {
      XmlNode xndRoot = null;
      foreach (XmlNode xndNode in p_xmlInfo.ChildNodes)
      {
        if (xndNode.NodeType == XmlNodeType.Element)
        {
          xndRoot = xndNode;
          break;
        }
      }
      if (xndRoot == null)
      {
        throw new fomodLoadException("Root node was missing from fomod info.xml");
      }
      if (xndRoot.Name != "fomod")
      {
        throw new fomodLoadException("Unexpected root node type in info.xml");
      }
      foreach (XmlNode xndNode in xndRoot.ChildNodes)
      {
        if (xndNode.NodeType == XmlNodeType.Comment)
        {
          continue;
        }
        switch (xndNode.Name)
        {
          case "Name":
            if (p_booOverwriteExisitngValues || String.IsNullOrEmpty(p_finFomodInfo.ModName))
            {
              p_finFomodInfo.ModName = xndNode.InnerText;
            }
            break;
          case "Version":
            if (p_booOverwriteExisitngValues || String.IsNullOrEmpty(p_finFomodInfo.HumanReadableVersion))
            {
              p_finFomodInfo.HumanReadableVersion = xndNode.InnerText;
            }
            if (p_booOverwriteExisitngValues || (p_finFomodInfo.MachineVersion == DefaultVersion))
            {
              var xndMachineVersion = xndNode.Attributes.GetNamedItem("MachineVersion");
              if (xndMachineVersion != null)
              {
                p_finFomodInfo.MachineVersion = new Version(xndMachineVersion.Value);
              }
            }
            break;
          case "Author":
            if (p_booOverwriteExisitngValues || String.IsNullOrEmpty(p_finFomodInfo.Author))
            {
              p_finFomodInfo.Author = xndNode.InnerText;
            }
            break;
          case "Description":
            if (p_booOverwriteExisitngValues || String.IsNullOrEmpty(p_finFomodInfo.Description))
            {
              p_finFomodInfo.Description = xndNode.InnerText;
            }
            break;
          case "MinFommVersion":
            if (p_booOverwriteExisitngValues || (p_finFomodInfo.MachineVersion == DefaultMinFommVersion))
            {
              p_finFomodInfo.MinFommVersion = new Version(xndNode.InnerText);
            }
            break;
          case "Email":
            if (p_booOverwriteExisitngValues || String.IsNullOrEmpty(p_finFomodInfo.Email))
            {
              p_finFomodInfo.Email = xndNode.InnerText;
            }
            break;
          case "Website":
            if (p_booOverwriteExisitngValues || String.IsNullOrEmpty(p_finFomodInfo.Website))
            {
              p_finFomodInfo.Website = xndNode.InnerText;
            }
            break;
          case "Groups":
            if (p_booOverwriteExisitngValues || (p_finFomodInfo.Groups.Length == 0))
            {
              var strGroups = new string[xndNode.ChildNodes.Count];
              for (var i = 0; i < xndNode.ChildNodes.Count; i++)
              {
                strGroups[i] = xndNode.ChildNodes[i].InnerText;
              }
              p_finFomodInfo.Groups = strGroups;
            }
            break;
        }
      }
    }

    /// <summary>
    ///   Serializes the fomod info contained in the given fomod info object into an XML document.
    /// </summary>
    /// <param name="p_finFomodInfo">The fomod info object to serialize.</param>
    /// <returns>An XML file containing the fomod info.</returns>
    public static XmlDocument SaveInfo(IFomodInfo p_finFomodInfo)
    {
      var xmlInfo = new XmlDocument();
      xmlInfo.AppendChild(xmlInfo.CreateXmlDeclaration("1.0", "UTF-16", null));
      var xelRoot = xmlInfo.CreateElement("fomod");
      XmlElement xelTemp;

      xmlInfo.AppendChild(xelRoot);
      if (!String.IsNullOrEmpty(p_finFomodInfo.ModName))
      {
        xelTemp = xmlInfo.CreateElement("Name");
        xelTemp.InnerText = p_finFomodInfo.ModName;
        xelRoot.AppendChild(xelTemp);
      }
      if (!String.IsNullOrEmpty(p_finFomodInfo.Author) && !DEFAULT_AUTHOR.Equals(p_finFomodInfo.Author))
      {
        xelTemp = xmlInfo.CreateElement("Author");
        xelTemp.InnerText = p_finFomodInfo.Author;
        xelRoot.AppendChild(xelTemp);
      }
      if (!String.IsNullOrEmpty(p_finFomodInfo.HumanReadableVersion) ||
          (p_finFomodInfo.MachineVersion != DefaultVersion))
      {
        xelTemp = xmlInfo.CreateElement("Version");
        xelTemp.InnerText = String.IsNullOrEmpty(p_finFomodInfo.HumanReadableVersion)
          ? p_finFomodInfo.MachineVersion.ToString()
          : p_finFomodInfo.HumanReadableVersion;
        xelTemp.Attributes.Append(xmlInfo.CreateAttribute("MachineVersion"));
        xelTemp.Attributes[0].Value = p_finFomodInfo.MachineVersion.ToString();
        xelRoot.AppendChild(xelTemp);
      }
      if (!String.IsNullOrEmpty(p_finFomodInfo.Description))
      {
        xelTemp = xmlInfo.CreateElement("Description");
        xelTemp.InnerText = p_finFomodInfo.Description;
        xelRoot.AppendChild(xelTemp);
      }
      if (!String.IsNullOrEmpty(p_finFomodInfo.Email))
      {
        xelTemp = xmlInfo.CreateElement("Email");
        xelTemp.InnerText = p_finFomodInfo.Email;
        xelRoot.AppendChild(xelTemp);
      }
      if (!String.IsNullOrEmpty(p_finFomodInfo.Website))
      {
        xelTemp = xmlInfo.CreateElement("Website");
        xelTemp.InnerText = p_finFomodInfo.Website;
        xelRoot.AppendChild(xelTemp);
      }
      if (p_finFomodInfo.MinFommVersion != DefaultMinFommVersion)
      {
        xelTemp = xmlInfo.CreateElement("MinFommVersion");
        xelTemp.InnerText = p_finFomodInfo.MinFommVersion.ToString();
        xelRoot.AppendChild(xelTemp);
      }
      if ((p_finFomodInfo.Groups != null) && (p_finFomodInfo.Groups.Length > 0))
      {
        xelTemp = xmlInfo.CreateElement("Groups");
        foreach (var group in p_finFomodInfo.Groups)
        {
          xelTemp.AppendChild(xmlInfo.CreateElement("element")).InnerText = group;
        }
        xelRoot.AppendChild(xelTemp);
      }

      return xmlInfo;
    }

    #endregion

    protected void LoadInfo()
    {
      if (ContainsFile("fomod/info.xml"))
      {
        HasInfo = true;
        var doc = new XmlDocument();
        doc.LoadXml(TextUtil.ByteToString(GetFileContents("fomod/info.xml")));
        LoadInfo(doc, this);
        if (Program.MVersion < MinFommVersion)
        {
          throw new fomodLoadException("This fomod requires a newer version of Fallout mod manager to load." +
                                       Environment.NewLine + "Expected " + MachineVersion);
        }
      }
    }

    internal FomodScript GetInstallScript()
    {
      if (!HasInstallScript)
      {
        return null;
      }
      return new FomodScript(m_strScriptPath, TextUtil.ByteToString(GetFileContents(m_strScriptPath)));
    }

    /// <summary>
    ///   Gets the custom uninstall script.
    /// </summary>
    /// <remarks>
    ///   Currently, custom uninstall scripts are not supported.
    ///   Indeed, if they were added significant change would have to be made
    ///   to the acessibility of methods, and safeties of one sort or another
    ///   would have to be put in place to insure everything is cleaned up
    ///   nicely.
    ///   Implementing a custom uninstall script to support re-configuration
    ///   of an installed mod seems to be a bad idea. If re-configuring an
    ///   installed mod is a desired feature it should be implemented separately,
    ///   and be more closey aligned to a mod install than a mod uninstall.
    /// </remarks>
    /// <returns></returns>
    internal string GetUninstallScript()
    {
      return null;
    }

    internal void SetScript(FomodScript p_fscScript)
    {
      if (p_fscScript == null)
      {
        if (HasInstallScript)
        {
          DeleteFile(m_strScriptPath);
          m_strScriptPath = null;
        }
      }
      else
      {
        if (m_strScriptPath == null)
        {
          m_strScriptPath = Path.Combine("fomod", p_fscScript.FileName);
        }
        ReplaceFile(m_strScriptPath, p_fscScript.Text);
      }
    }

    public Readme GetReadme()
    {
      if (String.IsNullOrEmpty(m_strReadmePath) || !Readme.IsValidReadme(m_strReadmePath))
      {
        return null;
      }
      return new Readme(m_strReadmePath, TextUtil.ByteToString(GetFileContents(m_strReadmePath)));
    }

    internal void SetReadme(Readme p_rmeReadme)
    {
      if (HasReadme)
      {
        DeleteFile(m_strReadmePath);
      }
      if (p_rmeReadme == null)
      {
        m_strReadmePath = null;
      }
      else
      {
        if (m_strReadmePath == null)
        {
          m_strReadmePath = (Settings.Default.UseDocsFolder ? "docs/" : "") + "Readme - " +
                            Path.GetFileNameWithoutExtension(filepath) + ".rtf";
        }
        m_strReadmePath = Path.ChangeExtension(m_strReadmePath, p_rmeReadme.Extension);
        ReplaceFile(m_strReadmePath, p_rmeReadme.Text);
      }
    }

    internal void CommitInfo(bool SetScreenshot, Screenshot p_shtScreenshot)
    {
      var xmlInfo = SaveInfo(this);
      HasInfo = true;

      var ms = new MemoryStream();
      xmlInfo.Save(ms);
      ReplaceFile("fomod/info.xml", ms.ToArray());
      if (SetScreenshot)
      {
        if (p_shtScreenshot == null)
        {
          if (HasScreenshot)
          {
            DeleteFile(m_strScreenshotPath);
            m_strScreenshotPath = null;
          }
        }
        else
        {
          if (!HasScreenshot)
          {
            m_strScreenshotPath = "fomod/screenshot.jpg";
          }
          m_strScreenshotPath = Path.ChangeExtension(m_strScreenshotPath, p_shtScreenshot.Extension).ToLowerInvariant();
          ReplaceFile(m_strScreenshotPath, p_shtScreenshot.Data);
        }
      }
    }

    public Image GetScreenshotImage()
    {
      return HasScreenshot ? GetScreenshot().Image : null;
    }

    public Screenshot GetScreenshot()
    {
      if (!HasScreenshot)
      {
        return null;
      }

      var shtScreenshot = new Screenshot(m_strScreenshotPath, GetFile(m_strScreenshotPath));
      return shtScreenshot;
    }

    internal void Dispose()
    {
      FomodFile.Dispose();
    }

    internal string GetStatusString()
    {
      var sb = new StringBuilder();
      sb.AppendLine("Mod name: " + ModName);
      sb.AppendLine("File name: " + baseName);
      if (!DEFAULT_AUTHOR.Equals(Author))
      {
        sb.AppendLine("Author: " + Author);
      }
      if (HumanReadableVersion != DEFAULT_VERSION)
      {
        sb.AppendLine("Version: " + HumanReadableVersion);
      }
      if (Email.Length > 0)
      {
        sb.AppendLine("email: " + Email);
      }
      if (Website.Length > 0)
      {
        sb.AppendLine("website: " + Website);
      }
      if (MinFommVersion != new Version(0, 0, 0, 0))
      {
        sb.AppendLine("Minimum required fomm version: " + MinFommVersion);
      }
      if (Description.Length > 0)
      {
        sb.AppendLine("Description:" + Environment.NewLine + Description);
      }
      if (Groups.Length > 0)
      {
        sb.AppendLine(Environment.NewLine + "Group tags: " + string.Join(", ", Groups));
      }
      sb.AppendLine();
      sb.AppendLine("Has readme: " + (HasReadme ? ("Yes (" + GetReadme().Format + ")") : "No"));
      sb.AppendLine("Has script: " + (HasInstallScript ? "Yes" : "No"));
      sb.AppendLine("Has screenshot: " + (HasScreenshot ? ("Yes (" + GetScreenshot().Extension + ")") : "No"));
      sb.AppendLine("Is active: " + (isActive ? "Yes" : "No"));
      sb.AppendLine();
      sb.AppendLine("-- fomod contents list:");
      sb.AppendLine(String.Join(Environment.NewLine, GetFileList().ToArray()));
      if (isActive)
      {
        sb.AppendLine();
        sb.AppendLine("Activation data" + Environment.NewLine);

        var ilmMergeModule = InstallLog.Current.GetMergeModule(baseName);
        if (ilmMergeModule.DataFiles.Count > 0)
        {
          sb.AppendLine("-- Installed data files");
          foreach (var strFile in ilmMergeModule.DataFiles)
          {
            sb.AppendLine(strFile);
          }
          sb.AppendLine();
        }
        if (ilmMergeModule.IniEdits.Count > 0)
        {
          sb.AppendLine("-- Ini edits");
          foreach (var iniEdit in ilmMergeModule.IniEdits)
          {
            sb.AppendLine("File: " + iniEdit.File);
            sb.AppendLine("Section: " + iniEdit.Section);
            sb.AppendLine("Key: " + iniEdit.Key);
            sb.AppendLine();
          }
        }
      }
      return sb.ToString();
    }

    #region Read Transactions

    /// <summary>
    ///   Raised when a read-only initialization step has started.
    /// </summary>
    public event CancelEventHandler ReadOnlyInitStepStarted
    {
      add
      {
        FomodFile.ReadOnlyInitStepStarted += value;
      }
      remove
      {
        FomodFile.ReadOnlyInitStepStarted -= value;
      }
    }

    /// <summary>
    ///   Raised when a read-only initialization step has finished.
    /// </summary>
    public event CancelEventHandler ReadOnlyInitStepFinished
    {
      add
      {
        FomodFile.ReadOnlyInitStepFinished += value;
      }
      remove
      {
        FomodFile.ReadOnlyInitStepFinished -= value;
      }
    }

    /// <summary>
    ///   Starts a read-only transaction.
    /// </summary>
    /// <remarks>
    ///   This puts the FOMod into read-only mode.
    ///   Read-only mode can greatly increase the speed at which multiple file are extracted.
    /// </remarks>
    public void BeginReadOnlyTransaction()
    {
      FomodFile.BeginReadOnlyTransaction();
    }

    /// <summary>
    ///   Ends a read-only transaction.
    /// </summary>
    /// <remarks>
    ///   This takes the FOMod out of read-only mode.
    ///   Read-only mode can greatly increase the speed at which multiple file are extracted.
    /// </remarks>
    public void EndReadOnlyTransaction()
    {
      FomodFile.EndReadOnlyTransaction();
    }

    #endregion

    /// <summary>
    ///   Retrieves the specified file from the fomod.
    /// </summary>
    /// <param name="p_strFile">The file to retrieve.</param>
    /// <returns>The requested file data.</returns>
    /// <exception cref="FileNotFoundException">
    ///   Thrown if the specified file
    ///   is not in the fomod.
    /// </exception>
    /// <exception cref="DecompressionException">
    ///   Thrown if the specified file
    ///   cannot be extracted from the zip.
    /// </exception>
    public byte[] GetFile(string p_strFile)
    {
      PermissionsManager.CurrentPermissions.Assert();
      if (!ContainsFile(p_strFile))
      {
        throw new FileNotFoundException("File doesn't exist in fomod", p_strFile);
      }
      return GetFileContents(p_strFile);
    }

    /// <summary>
    ///   Retrieves the specified image from the fomod.
    /// </summary>
    /// <param name="p_strFile">The path of the image to extract.</param>
    /// <returns>The request image.</returns>
    /// <exception cref="FileNotFoundException">
    ///   Thrown if the specified file
    ///   is not in the fomod.
    /// </exception>
    /// <exception cref="DecompressionException">
    ///   Thrown if the specified file
    ///   cannot be extracted from the zip.
    /// </exception>
    public Image GetImage(string p_strFile)
    {
      PermissionsManager.CurrentPermissions.Assert();
      if (!ContainsFile(p_strFile))
      {
        throw new FileNotFoundException("File doesn't exist in fomod", p_strFile);
      }
      Image imgImage;
      using (var msmImage = new MemoryStream(GetFileContents(p_strFile)))
      {
        imgImage = Image.FromStream(msmImage);
        msmImage.Close();
      }
      return imgImage;
    }

    #region Mod Info

    /// <summary>
    ///   Fills in missing info about the mod from the given info.
    /// </summary>
    /// <param name="p_mifInfo">A <see cref="ModInfo" /> describing the info of the mod.</param>
    public void SetMissingInfo(ModInfo p_mifInfo)
    {
      Version mv = null;

      if (p_mifInfo == null)
      {
        return;
      }
      var booUpdated = false;
      if (ModName.Equals(Path.GetFileNameWithoutExtension(filepath)) && !String.IsNullOrEmpty(p_mifInfo.ModName))
      {
        booUpdated = true;
        ModName = p_mifInfo.ModName;
      }
      if (DEFAULT_AUTHOR.Equals(Author) && !String.IsNullOrEmpty(p_mifInfo.Author))
      {
        booUpdated = true;
        Author = p_mifInfo.Author;
      }

      if (HumanReadableVersion.Equals(DEFAULT_VERSION) && (MachineVersion == DefaultVersion) &&
          !String.IsNullOrEmpty(p_mifInfo.Version))
      {
        HumanReadableVersion = p_mifInfo.Version;
        var strVersionString = p_mifInfo.Version;
        var rgxCleanVersion = new Regex(@"[^\d\.]");
        strVersionString = rgxCleanVersion.Replace(strVersionString, "");
        if (String.IsNullOrEmpty(strVersionString))
        {
          strVersionString = "0.0.0.0";
        }

        if (Version.TryParse(strVersionString, out mv))
        {
          MachineVersion = mv;
        }
      }

      if (!HasScreenshot && (p_mifInfo.Screenshot != null))
      {
        booUpdated = true;
      }

      if (String.IsNullOrEmpty(Website) && (p_mifInfo.URL != null))
      {
        booUpdated = true;
        Website = p_mifInfo.URL.ToString();
      }
      if (booUpdated)
      {
        CommitInfo((p_mifInfo.Screenshot != null), p_mifInfo.Screenshot);
      }
    }

    #endregion
  }
}