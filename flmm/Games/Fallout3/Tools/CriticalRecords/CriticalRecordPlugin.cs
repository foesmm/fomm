using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Fomm.Games.Fallout3.Tools.TESsnip;
using System.IO;

namespace Fomm.Games.Fallout3.Tools.CriticalRecords
{
  /// <summary>
  /// A <see cref="Plugin"/> that contains information about critical records.
  /// </summary>
  /// <remarks>
  /// This class simply adds helper methods for interacting with the critical records data stored
  /// in a well-known MESG record.
  /// </remarks>
  public class CriticalRecordPlugin : Plugin
  {
    /// <summary>
    /// The well-known name of the MESG record that contains the critical record data.
    /// </summary>
    private const string CRITICAL_DATA_RECORD_EDID = "fommCriticalRecords";

    private Dictionary<UInt32, CriticalRecordInfo> m_dicCriticalRecords = new Dictionary<UInt32, CriticalRecordInfo>();

    #region Properties

    /// <summary>
    /// Gets whether the plugin has any critical record data.
    /// </summary>
    /// <value>Whether the plugin has any critical record data.</value>
    internal bool HasCriticalRecordData
    {
      get
      {
        return m_dicCriticalRecords.Count > 0;
      }
    }

    internal IList<UInt32> CriticalRecordFormIds
    {
      get
      {
        return new List<UInt32>(m_dicCriticalRecords.Keys);
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple contructor that loads the critical record data.
    /// </summary>
    /// <param name="p_bteData">The plugin data.</param>
    /// <param name="p_strPluginName">The name of the plugin.</param>
    public CriticalRecordPlugin(byte[] p_bteData, string p_strPluginName)
      : base(p_bteData, p_strPluginName)
    {
      loadCriticalData();
    }

    /// <summary>
    /// A simple contructor that loads the critical record data.
    /// </summary>
    /// <param name="p_strPath">The path to the plugin file.</param>
    /// <param name="p_booHeaderOnly">Whether to only load the header.</param>
    public CriticalRecordPlugin(string p_strPath, bool p_booHeaderOnly)
      : base(p_strPath, p_booHeaderOnly)
    {
      loadCriticalData();
    }

    #endregion

    /// <summary>
    /// Loads the critical record data from the well-known record.
    /// </summary>
    protected void loadCriticalData()
    {
      SubRecord srcCriticalData = getCriticalRecordData();
      string strCriticalData = srcCriticalData.GetStrData().Trim().Replace("\r\n", "\n").Replace("\n\r", "\n");
      string[] strCriticalRecords = strCriticalData.Split(new char[]
      {
        '\n'
      }, StringSplitOptions.RemoveEmptyEntries);
      UInt32 uintFormId = 0;
      CriticalRecordInfo criInfo = null;
      foreach (string strCriticalRecord in strCriticalRecords)
      {
        if (
          !UInt32.TryParse(strCriticalRecord.Substring(0, 8), NumberStyles.HexNumber, null,
                           out uintFormId))
        {
          continue;
        }
        criInfo = new CriticalRecordInfo();
        criInfo.Severity =
          (CriticalRecordInfo.ConflictSeverity)
            Int32.Parse(strCriticalRecord[9].ToString(), NumberStyles.HexNumber);
        criInfo.Reason = strCriticalRecord.Substring(11);
        m_dicCriticalRecords[uintFormId] = criInfo;
      }
    }

    /// <summary>
    /// Gets the well-known MESG record's subrecord data containg the critical record info.
    /// </summary>
    /// <remarks>
    /// If the required record is not found, it is created.
    /// </remarks>
    /// <returns>The well-known MESG record's subrecord data containg the critical record info.</returns>
    protected SubRecord getCriticalRecordData()
    {
      GroupRecord grcGroup = null;
      Record recCriticalRecords = null;
      SubRecord srcCriticalData = null;
      foreach (Rec rec in Records)
      {
        grcGroup = rec as GroupRecord;
        if (grcGroup == null)
        {
          continue;
        }
        if (grcGroup.ContentsType == "MESG")
        {
          foreach (Record recRecord in grcGroup.Records)
          {
            foreach (SubRecord srcSubRecord in recRecord.SubRecords)
            {
              switch (srcSubRecord.Name)
              {
                case "EDID":
                  if (srcSubRecord.GetStrData().Equals(CRITICAL_DATA_RECORD_EDID))
                  {
                    recCriticalRecords = recRecord;
                  }
                  break;
                case "DESC":
                  srcCriticalData = srcSubRecord;
                  break;
              }
            }
            if (recCriticalRecords != null)
            {
              return srcCriticalData;
            }
          }
        }
      }

      //if there is no MESG group, create one
      if (grcGroup == null)
      {
        grcGroup = new GroupRecord("MESG");
        AddRecord(grcGroup);
      }
      //if there is no fommCriticalRecords record, create one
      if (recCriticalRecords == null)
      {
        recCriticalRecords = new Record();
        recCriticalRecords.Name = "MESG";
        UInt32 uintMastersCount = (UInt32) Masters.Count << 24;
        UInt32 uintFormId = uintMastersCount + 1;
        while (ContainsFormId(uintFormId))
        {
          uintFormId++;
        }
        if ((uintFormId & 0xff000000) != uintMastersCount)
        {
          throw new PluginFullException("No available Form Id for new MESG record");
        }
        recCriticalRecords.FormID = uintFormId;
        recCriticalRecords.Flags2 = 0x00044210;
        recCriticalRecords.Flags3 = 0x0002000f;

        SubRecord srcSub = new SubRecord();
        srcSub.Name = "EDID";
        srcSub.SetStrData(CRITICAL_DATA_RECORD_EDID, true);
        recCriticalRecords.SubRecords.Add(srcSub);

        srcCriticalData = new SubRecord();
        srcCriticalData.Name = "DESC";
        recCriticalRecords.SubRecords.Add(srcCriticalData);

        srcSub = new SubRecord();
        srcSub.Name = "INAM";
        srcSub.SetData(new byte[]
        {
          0, 0, 0, 0
        });
        recCriticalRecords.SubRecords.Add(srcSub);

        srcSub = new SubRecord();
        srcSub.Name = "DNAM";
        srcSub.SetData(new byte[]
        {
          0, 0, 0, 0
        });
        recCriticalRecords.SubRecords.Add(srcSub);

        srcSub = new SubRecord();
        srcSub.Name = "TNAM";
        srcSub.SetData(new byte[]
        {
          0, 0, 0, 1
        });
        recCriticalRecords.SubRecords.Add(srcSub);

        grcGroup.AddRecord(recCriticalRecords);
      }

      return srcCriticalData;
    }

    /// <summary>
    /// Populates the well-known record and then saves the plugin as usual.
    /// </summary>
    /// <param name="bw">The writer to which to write the plugin data.</param>
    internal override void SaveData(BinaryWriter bw)
    {
      StringBuilder stbCriticalData = new StringBuilder();
      foreach (KeyValuePair<UInt32, CriticalRecordInfo> kvpCriticalRecords in m_dicCriticalRecords)
      {
        stbCriticalData.AppendFormat("{0:x8} {1}", kvpCriticalRecords.Key, kvpCriticalRecords.Value).AppendLine();
      }
      SubRecord srcCriticalData = getCriticalRecordData();
      srcCriticalData.SetStrData(stbCriticalData.ToString(), true);

      base.SaveData(bw);
    }

    /// <summary>
    /// Determines if the specified record is marked as critical.
    /// </summary>
    /// <param name="p_uintFormId">The record for which it is to be determined whether it is
    /// critical.</param>
    /// <returns><lang cref="true"/> if the specified record is critical;
    /// <lang cref="false"/> otherwise.</returns>
    internal bool IsRecordCritical(UInt32 p_uintFormId)
    {
      return m_dicCriticalRecords.ContainsKey(p_uintFormId);
    }

    /// <summary>
    /// Gets the info about the specified critical record.
    /// </summary>
    /// <remarks>
    /// The ifno includes the reason the record was marked as critical, and the severity of
    /// any conflicts with the record.
    /// </remarks>
    /// <param name="p_uintFormId">The record for which  to retrieve the critical record info.</param>
    /// <returns>The info about the specified critical record.</returns>
    public CriticalRecordInfo GetCriticalRecordInfo(UInt32 p_uintFormId)
    {
      if (!IsRecordCritical(p_uintFormId))
      {
        return null;
      }
      return m_dicCriticalRecords[p_uintFormId];
    }

    /// <summary>
    /// Marks a record as critical, supplying a reason to the critical marking.
    /// </summary>
    /// <param name="p_uintFormId">The form id that is being marked as critical.</param>
    /// <param name="p_csvSeverity">The severity of the conflict.</param>
    /// <param name="p_strReason">The reason the record is being marked as critical.</param>
    public void SetCriticalRecord(UInt32 p_uintFormId, CriticalRecordInfo.ConflictSeverity p_csvSeverity,
                                  string p_strReason)
    {
      m_dicCriticalRecords[p_uintFormId] = new CriticalRecordInfo(p_csvSeverity, p_strReason);
    }

    /// <summary>
    /// Unsets the sepecifed record as critical.
    /// </summary>
    /// <param name="p_uintFormId">The form id that is being unmarked as critical.</param>
    public void UnsetCriticalRecord(UInt32 p_uintFormId)
    {
      m_dicCriticalRecords.Remove(p_uintFormId);
    }
  }
}