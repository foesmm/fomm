using System;

namespace Fomm.Games.Fallout3.Tools.CriticalRecords
{
  /// <summary>
  ///   Describes a critical record conflict.
  /// </summary>
  public class CriticalRecordInfo
  {
    /// <summary>
    ///   Describes the different conflict severities.
    /// </summary>
    public enum ConflictSeverity
    {
      /// <summary>
      ///   Info only; conflict is not serious.
      /// </summary>
      Info = 0,

      /// <summary>
      ///   Warning only; conflict is not serious, but may cause problems under certain circumstances.
      /// </summary>
      Warning,

      /// <summary>
      ///   Full conflict; conflict is likely to cause problems.
      /// </summary>
      Conflict
    }

    #region Properties

    /// <summary>
    ///   Gets or sets the reason the record is marked as critical.
    /// </summary>
    /// <value>The reason the record is marked as critical.</value>
    public string Reason { get; set; }

    /// <summary>
    ///   Gets or sets the severity of the conflict.
    /// </summary>
    /// <value>The severity of the conflict.</value>
    public ConflictSeverity Severity { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    ///   The default constructor.
    /// </summary>
    public CriticalRecordInfo() {}

    /// <summary>
    ///   A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_csvSeverity">The severity of the conflict.</param>
    /// <param name="p_strReason">The reason the record is marked as critical.</param>
    public CriticalRecordInfo(ConflictSeverity p_csvSeverity, string p_strReason)
    {
      Severity = p_csvSeverity;
      Reason = p_strReason;
    }

    #endregion

    /// <summary>
    ///   Returns the string representation of the info.
    /// </summary>
    /// <remarks>
    ///   The returned string is the interger representation of the severity
    ///   foloowed by a space and then the reason.
    /// </remarks>
    /// <returns>The string representation of the info.</returns>
    public override string ToString()
    {
      return ((Int32) Severity) + " " + Reason;
    }
  }
}