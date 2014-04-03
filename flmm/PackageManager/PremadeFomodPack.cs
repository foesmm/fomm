using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace Fomm.PackageManager
{
  /// <summary>
  /// Encapsulates a Premade FOMod Pack (PFP).
  /// </summary>
  public class PremadeFomodPack
  {
    private Archive m_arcPFP;
    private XmlDocument m_xmlMeta;
    private string m_strPremadePath;

    #region Validation

    /// <summary>
    /// Determines if the specified file is a valid PFP.
    /// </summary>
    /// <param name="p_strPFPPath">The path of the file to validate as a PFP.</param>
    /// <returns>An error string describing why the specified file is not a valid PFP, or
    /// <lang langref="null"/> if the specified file is a valid PFP.</returns>
    public static string ValidatePFP(string p_strPFPPath)
    {
      return ValidatePFP(new Archive(p_strPFPPath));
    }

    /// <summary>
    /// Determines if the given archive is a valid PFP.
    /// </summary>
    /// <param name="p_arcPFP">The archive to validate as a PFP.</param>
    /// <returns>An error string describing why the specified file is not a valid PFP, or
    /// <lang langref="null"/> if the given archive is a valid PFP.</returns>
    protected static string ValidatePFP(Archive p_arcPFP)
    {
      if (!p_arcPFP.ContainsFile("metadata.xml"))
      {
        return "Missing metadata.xml file.";
      }

      var xmlMeta = new XmlDocument();
      using (var msmMeta = new MemoryStream(p_arcPFP.GetFileContents("metadata.xml")))
      {
        xmlMeta.Load(msmMeta);
        msmMeta.Close();
      }

      var xndSources = xmlMeta.SelectSingleNode("premadeFomodPack/sources");
      foreach (XmlNode xndSource in xndSources.ChildNodes)
      {
        if ((xndSource.Attributes["name"] == null) || String.IsNullOrEmpty(xndSource.Attributes["name"].Value) ||
            (xndSource.Attributes["url"] == null) || String.IsNullOrEmpty(xndSource.Attributes["url"].Value))
        {
          return "Invalid metadata.xml file.";
        }
      }
      return null;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the path of the PFP file.
    /// </summary>
    /// <value>The path of the PFP file.</value>
    public string PFPPath
    {
      get
      {
        return m_arcPFP.ArchivePath;
      }
    }

    /// <summary>
    /// Gets the name of the FOMod this PFP will create.
    /// </summary>
    /// <value>The name of the FOMod this PFP will create.</value>
    public string FomodName
    {
      get
      {
        return m_strPremadePath.Substring(8);
      }
    }

    /// <summary>
    /// Gets the path to the Premade folder if the PFP.
    /// </summary>
    /// <value>The path to the Premade folder if the PFP.</value>
    public string PremadePath
    {
      get
      {
        return m_strPremadePath;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_strPFPPath">The path to the PFP file.</param>
    public PremadeFomodPack(string p_strPFPPath)
    {
      m_arcPFP = new Archive(p_strPFPPath);
      var strError = ValidatePFP(m_arcPFP);
      if (!String.IsNullOrEmpty(strError))
      {
        throw new ArgumentException("Specified Premade FOMod Pack is not valid: " + strError, "p_strPFPPath");
      }
      m_xmlMeta = new XmlDocument();
      using (var msmMeta = new MemoryStream(m_arcPFP.GetFileContents("metadata.xml")))
      {
        m_xmlMeta.Load(msmMeta);
        msmMeta.Close();
      }
      foreach (var strDirectory in m_arcPFP.GetDirectories("/"))
      {
        if (strDirectory.StartsWith("Premade", StringComparison.InvariantCultureIgnoreCase))
        {
          m_strPremadePath = strDirectory;
          break;
        }
      }
    }

    #endregion

    /// <summary>
    /// Gets the copy instructions for the PFP.
    /// </summary>
    /// <remarks>
    /// The returned copy instructions are made relative to the given source path. This means that
    /// all instructions will be adjusted to assume the source files are in the directory specified
    /// by <paramref name="p_strSourcesPath"/>. If you want the copy instructions as they are stored
    /// in the PFP, pass in <lang langref="null/"/>.
    /// 
    /// Note that copy instructions are strictly ordered.
    /// </remarks>
    /// <param name="p_strSourcesPath">The path to the directory containing the source files.</param>
    /// <returns>The copy instructions for the PFP.</returns>
    public List<KeyValuePair<string, string>> GetCopyInstructions(string p_strSourcesPath)
    {
      var lstCopyInstructions = new List<KeyValuePair<string, string>>();
      var xndIntructions = m_xmlMeta.SelectSingleNode("premadeFomodPack/copyInstructions");
      foreach (XmlNode xndInstruction in xndIntructions.ChildNodes)
      {
        var strSource = xndInstruction.Attributes["source"].Value;
        strSource = strSource.StartsWith(Archive.ARCHIVE_PREFIX) ? Archive.ChangeArchiveDirectory(strSource, p_strSourcesPath) : Path.Combine(p_strSourcesPath, strSource);
        var strDestination = xndInstruction.Attributes["destination"].Value;
        lstCopyInstructions.Add(new KeyValuePair<string, string>(strSource, strDestination));
      }
      return lstCopyInstructions;
    }

    /// <summary>
    /// Gets the list of sources required by the PFP.
    /// </summary>
    /// <returns>The list of sources required by the PFP.</returns>
    public List<SourceFile> GetSources()
    {
      var lstSources = new List<SourceFile>();
      var xndSources = m_xmlMeta.SelectSingleNode("premadeFomodPack/sources");
      foreach (XmlNode xndSource in xndSources.ChildNodes)
      {
        var strSource = xndSource.Attributes["name"].Value;
        var strUrl = xndSource.Attributes["url"].Value;
        var booHidden = false;
        if (xndSource.Attributes["hidden"] != null)
        {
          Boolean.TryParse(xndSource.Attributes["hidden"].Value, out booHidden);
        }
        var booGenerated = false;
        if (xndSource.Attributes["generated"] != null)
        {
          Boolean.TryParse(xndSource.Attributes["generated"].Value, out booGenerated);
        }
        lstSources.Add(new SourceFile(strSource, strUrl, String.IsNullOrEmpty(strUrl), booHidden, booGenerated));
      }
      return lstSources;
    }

    /// <summary>
    /// Gets the custom howto steps specified by the PFP.
    /// </summary>
    /// <returns>The custom howto steps specified by the PFP.</returns>
    public string GetCustomHowToSteps()
    {
      var xndCustomHowToSteps = m_xmlMeta.SelectSingleNode("premadeFomodPack/customHowToSteps");
      if (xndCustomHowToSteps != null)
      {
        return xndCustomHowToSteps.InnerXml;
      }
      return null;
    }
  }
}