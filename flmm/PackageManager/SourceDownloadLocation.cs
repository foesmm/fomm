using System;
using System.IO;

namespace Fomm.PackageManager
{
  /// <summary>
  ///   Encasuplates a source file.
  /// </summary>
  /// <remarks>
  ///   Source files are used to build FOMods and PFPs.
  /// </remarks>
  public class SourceFile : IEquatable<SourceFile>
  {
    #region Properties

    /// <summary>
    ///   Gets or sets the path of the source.
    /// </summary>
    /// <value>The path of the source.</value>
    public string Source { get; set; }

    /// <summary>
    ///   Gets the filename of the source.
    /// </summary>
    /// <value>The filename of the source.</value>
    public string SourceFileName
    {
      get
      {
        return Path.GetFileName(Source);
      }
    }

    /// <summary>
    ///   Gets or sets the url where the source can be downloaded.
    /// </summary>
    /// <value>The url where the source can be downloaded.</value>
    public string URL { get; set; }

    /// <summary>
    ///   Gets or sets whether the source is included in the PFP.
    /// </summary>
    /// <value>Whether the source is included in the PFP.</value>
    public bool Included { get; set; }

    /// <summary>
    ///   Gets or sets whether the source is hidden in the file selector of the fomod builder.
    /// </summary>
    /// <value>Whether the source is hidden in the file selector of the fomod builder.</value>
    public bool Hidden { get; set; }

    /// <summary>
    ///   Gets or sets whether the source is a generated file that is not downloaded.
    /// </summary>
    /// <value>Whether the source is a generated file that is not downloaded.</value>
    public bool Generated { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    ///   A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_strSource">The path of the source.</param>
    /// <param name="p_strUrl">The url where the source can be downloaded.</param>
    /// <param name="p_booIncluded">Whether the source is included in the PFP.</param>
    /// <param name="p_booHidden">Whether the source is hidden in the file selector of the fomod builder.</param>
    /// <param name="p_booGenerated">Whether the source is a generated file that is not downloaded.</param>
    public SourceFile(string p_strSource, string p_strUrl, bool p_booIncluded, bool p_booHidden, bool p_booGenerated)
    {
      Source = p_strSource;
      URL = p_strUrl;
      Included = p_booIncluded;
      Hidden = p_booHidden;
      Generated = p_booGenerated;
    }

    #endregion

    #region IEquatable<SourceFile> Members

    /// <summary>
    ///   Determins if this <see cref="SourceFile" /> is equal to the
    ///   given <see cref="SourceFile" />.
    /// </summary>
    /// <remarks>
    ///   Two <see cref="SourceFile" />s are equal if and only if thier
    ///   <see cref="SourceFile.Source" />s are equal.
    /// </remarks>
    /// <param name="other">The <see cref="SourceFile" /> to which to equate this <see cref="SourceFile" />.</param>
    /// <returns>
    ///   <lang langref="true" /> is the two <see cref="SourceDownloadLocation" />s are equal;
    ///   <lang langref="false" /> otherwise.
    /// </returns>
    public bool Equals(SourceFile other)
    {
      if (other == null)
      {
        return false;
      }
      return Source.Equals(other.Source);
    }

    #endregion
  }
}