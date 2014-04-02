using System;
using System.Collections.Generic;
using System.IO;
using Fomm.SharpZipLib;
using Fomm.SharpZipLib.Zip.Compression;

namespace Fomm.Games.Fallout3.Tools.TESsnip
{
  public class TESParserException : Exception
  {
    public TESParserException(string msg) : base(msg)
    {
    }
  }

  public abstract class BaseRecord
  {
    public string Name;

    public abstract long Size { get; }
    public abstract long Size2 { get; }

    private static byte[] input;
    private static byte[] output;
    private static MemoryStream ms;
    private static BinaryReader compReader;
    private static Inflater inf;

    protected static BinaryReader Decompress(BinaryReader br, int size, int outsize)
    {
      if (input.Length < size)
      {
        input = new byte[size];
      }
      if (output.Length < outsize)
      {
        output = new byte[outsize];
      }
      br.Read(input, 0, size);

      inf.SetInput(input, 0, size);
      try
      {
        inf.Inflate(output);
      }
      catch (SharpZipBaseException e)
      {
        //we ignore adler checksum mismatches, as I have a notion that they aren't always correctly
        // stored in the records.
        if (!e.Message.StartsWith("Adler"))
        {
          throw e;
        }
      }
      inf.Reset();

      ms.Position = 0;
      ms.Write(output, 0, outsize);
      ms.Position = 0;

      return compReader;
    }

    protected static void InitDecompressor()
    {
      inf = new Inflater(false);
      ms = new MemoryStream();
      compReader = new BinaryReader(ms);
      input = new byte[0x1000];
      output = new byte[0x4000];
    }

    protected static void CloseDecompressor()
    {
      compReader.Close();
      compReader = null;
      inf = null;
      input = null;
      output = null;
    }

    public abstract string GetDesc();
    public abstract void DeleteRecord(BaseRecord br);
    public abstract void AddRecord(BaseRecord br);
    internal abstract List<string> GetIDs(bool lower);
    internal abstract void SaveData(BinaryWriter bw);

    private static readonly byte[] RecByte = new byte[4];

    protected static string ReadRecName(BinaryReader br)
    {
      br.Read(RecByte, 0, 4);
      return "" + ((char) RecByte[0]) + ((char) RecByte[1]) + ((char) RecByte[2]) + ((char) RecByte[3]);
    }

    protected static void WriteString(BinaryWriter bw, string s)
    {
      byte[] b = new byte[s.Length];
      for (int i = 0; i < s.Length; i++)
      {
        b[i] = (byte) s[i];
      }
      bw.Write(b, 0, s.Length);
    }

    public abstract BaseRecord Clone();
  }

  public class Plugin : BaseRecord
  {
    public readonly List<Rec> Records = new List<Rec>();

    public override long Size
    {
      get
      {
        long size = 0;
        foreach (Rec rec in Records)
        {
          size += rec.Size2;
        }
        return size;
      }
    }

    public override long Size2
    {
      get
      {
        return Size;
      }
    }

    public IList<string> Masters
    {
      get
      {
        List<string> lstMasters = new List<string>();
        foreach (SubRecord sr in ((Record) Records[0]).SubRecords)
        {
          switch (sr.Name)
          {
            case "MAST":
              lstMasters.Add(sr.GetStrData());
              break;
          }
        }
        return lstMasters;
      }
    }

    public override void DeleteRecord(BaseRecord br)
    {
      Rec r = br as Rec;
      if (r == null)
      {
        return;
      }
      Records.Remove(r);
    }

    public override void AddRecord(BaseRecord br)
    {
      Rec r = br as Rec;
      if (r == null)
      {
        throw new TESParserException("Record to add was not of the correct type." +
                                     Environment.NewLine + "Plugins can only hold Groups or Records.");
      }
      Records.Add(r);
    }

    private void LoadPlugin(BinaryReader br, bool headerOnly)
    {
      string s;
      uint recsize;
      bool IsOblivion = false;

      InitDecompressor();

      s = ReadRecName(br);
      if (s != "TES4")
      {
        throw new Exception("File is not a valid TES4 plugin (Missing TES4 record)");
      }
      br.BaseStream.Position = 20;
      s = ReadRecName(br);
      if (s == "HEDR")
      {
        IsOblivion = true;
      }
      else
      {
        s = ReadRecName(br);
        if (s != "HEDR")
        {
          throw new Exception("File is not a valid TES4 plugin (Missing HEDR subrecord in the TES4 record)");
        }
      }
      br.BaseStream.Position = 4;
      recsize = br.ReadUInt32();
      Records.Add(new Record("TES4", recsize, br, IsOblivion));
      if (!headerOnly)
      {
        while (br.PeekChar() != -1)
        {
          s = ReadRecName(br);
          recsize = br.ReadUInt32();
          if (s == "GRUP")
          {
            Records.Add(new GroupRecord(recsize, br, IsOblivion));
          }
          else
          {
            Records.Add(new Record(s, recsize, br, IsOblivion));
          }
        }
      }

      CloseDecompressor();
    }

    public static bool GetIsEsm(string FilePath)
    {
      BinaryReader br = new BinaryReader(File.OpenRead(FilePath));
      try
      {
        string s = ReadRecName(br);
        if (s != "TES4")
        {
          return false;
        }
        br.ReadInt32();
        return (br.ReadInt32() & 1) != 0;
      }
      catch
      {
        return false;
      }
      finally
      {
        br.Close();
      }
    }

    public Plugin(byte[] data, string name)
    {
      Name = name;
      BinaryReader br = new BinaryReader(new MemoryStream(data));
      try
      {
        LoadPlugin(br, false);
      }
      finally
      {
        br.Close();
      }
    }

    internal Plugin(string FilePath, bool headerOnly)
    {
      Name = Path.GetFileName(FilePath);
      FileInfo fi = new FileInfo(FilePath);
      BinaryReader br = new BinaryReader(fi.OpenRead());
      try
      {
        LoadPlugin(br, headerOnly);
      }
      finally
      {
        br.Close();
      }
    }

    public Plugin()
    {
      Name = "New plugin";
    }

    public override string GetDesc()
    {
      return "[Fallout3 plugin]" + Environment.NewLine +
             "Filename: " + Name + Environment.NewLine +
             "File size: " + Size + Environment.NewLine +
             "Records: " + Records.Count;
    }

    public byte[] Save()
    {
      MemoryStream ms = new MemoryStream();
      BinaryWriter bw = new BinaryWriter(ms);
      SaveData(bw);
      byte[] b = ms.ToArray();
      bw.Close();
      return b;
    }

    internal void Save(string FilePath)
    {
      bool existed = false;
      DateTime timestamp = DateTime.Now;
      if (File.Exists(FilePath))
      {
        timestamp = new FileInfo(FilePath).LastWriteTime;
        existed = true;
        File.Delete(FilePath);
      }
      BinaryWriter bw = new BinaryWriter(File.OpenWrite(FilePath));
      try
      {
        SaveData(bw);
        Name = Path.GetFileName(FilePath);
      }
      finally
      {
        bw.Close();
      }
      try
      {
        if (existed)
        {
          new FileInfo(FilePath).LastWriteTime = timestamp;
        }
      }
      catch
      {
      }
    }

    internal override void SaveData(BinaryWriter bw)
    {
      foreach (Rec r in Records)
      {
        r.SaveData(bw);
      }
    }

    internal override List<string> GetIDs(bool lower)
    {
      List<string> list = new List<string>();
      foreach (Rec r in Records)
      {
        list.AddRange(r.GetIDs(lower));
      }
      return list;
    }

    public override BaseRecord Clone()
    {
      throw new NotImplementedException("The method or operation is not implemented.");
    }

    public bool ContainsFormId(UInt32 p_uintFormId)
    {
      return ContainsFormId(p_uintFormId, Records);
    }

    private bool ContainsFormId(uint p_uintFormId, List<Rec> p_lstRecords)
    {
      foreach (Rec rec in p_lstRecords)
      {
        if (rec is GroupRecord)
        {
          if (ContainsFormId(p_uintFormId, ((GroupRecord) rec).Records))
          {
            return true;
          }
        }
        else if (rec is Record)
        {
          if (((Record) rec).FormID == p_uintFormId)
          {
            return true;
          }
        }
      }
      return false;
    }

    public Int32 GetMasterIndex(string p_strPluginName)
    {
      IList<string> lstMaster = Masters;
      for (Int32 i = 0; i < lstMaster.Count; i++)
      {
        if (lstMaster[i].ToLowerInvariant().Equals(p_strPluginName.ToLowerInvariant()))
        {
          return i;
        }
      }
      return -1;
    }

    public string GetMaster(Int32 p_intIndex)
    {
      IList<string> lstMasters = Masters;
      if ((p_intIndex < 0) || (p_intIndex >= lstMasters.Count))
      {
        return null;
      }
      return lstMasters[p_intIndex];
    }
  }

  public abstract class Rec : BaseRecord
  {
    public string descriptiveName;

    public string DescriptiveName
    {
      get
      {
        return descriptiveName == null ? Name : (Name + descriptiveName);
      }
    }
  }

  public sealed class GroupRecord : Rec
  {
    public readonly List<Rec> Records = new List<Rec>();
    private readonly byte[] data;
    public uint groupType;
    public uint dateStamp;
    public uint flags;

    public string ContentsType
    {
      get
      {
        return "" + (char) data[0] + (char) data[1] + (char) data[2] + (char) data[3];
      }
    }

    public override long Size
    {
      get
      {
        long size = 24;
        foreach (Rec rec in Records)
        {
          size += rec.Size2;
        }
        return size;
      }
    }

    public override long Size2
    {
      get
      {
        return Size;
      }
    }

    public override void DeleteRecord(BaseRecord br)
    {
      Rec r = br as Rec;
      if (r == null)
      {
        return;
      }
      Records.Remove(r);
    }

    public override void AddRecord(BaseRecord br)
    {
      Rec r = br as Rec;
      if (r == null)
      {
        throw new TESParserException("Record to add was not of the correct type." +
                                     Environment.NewLine + "Groups can only hold records or other groups.");
      }
      Records.Add(r);
    }

    internal GroupRecord(uint Size, BinaryReader br, bool Oblivion)
    {
      Name = "GRUP";
      data = br.ReadBytes(4);
      groupType = br.ReadUInt32();
      dateStamp = br.ReadUInt32();
      if (!Oblivion)
      {
        flags = br.ReadUInt32();
      }
      uint AmountRead = 0;
      while (AmountRead < Size - (Oblivion ? 20 : 24))
      {
        string s = Plugin.ReadRecName(br);
        uint recsize = br.ReadUInt32();
        if (s == "GRUP")
        {
          GroupRecord gr = new GroupRecord(recsize, br, Oblivion);
          AmountRead += recsize;
          Records.Add(gr);
        }
        else
        {
          Record r = new Record(s, recsize, br, Oblivion);
          AmountRead += (uint) (recsize + (Oblivion ? 20 : 24));
          Records.Add(r);
        }
      }
      if (AmountRead > (Size - (Oblivion ? 20 : 24)))
      {
        throw new TESParserException("Record block did not match the size specified in the group header");
      }
      if (groupType == 0)
      {
        descriptiveName = " (" + (char) data[0] + (char) data[1] + (char) data[2] + (char) data[3] + ")";
      }
    }

    public GroupRecord(string data)
    {
      Name = "GRUP";
      this.data = new byte[4];
      for (int i = 0; i < 4; i++)
      {
        this.data[i] = (byte) data[i];
      }
      descriptiveName = " (" + data + ")";
    }

    private GroupRecord(GroupRecord gr)
    {
      Name = "GRUP";
      data = (byte[]) gr.data.Clone();
      groupType = gr.groupType;
      dateStamp = gr.dateStamp;
      flags = gr.flags;
      Records = new List<Rec>(gr.Records.Count);
      for (int i = 0; i < gr.Records.Count; i++)
      {
        Records.Add((Rec) gr.Records[i].Clone());
      }
      Name = gr.Name;
      descriptiveName = gr.descriptiveName;
    }

    private string GetSubDesc()
    {
      switch (groupType)
      {
        case 0:
          return "(Contains: " + (char) data[0] + (char) data[1] + (char) data[2] + (char) data[3] + ")";
        case 2:
        case 3:
          return "(Block number: " + (data[0] + data[1]*256 + data[2]*256*256 + data[3]*256*256*256).ToString() + ")";
        case 4:
        case 5:
          return "(Coordinates: [" + (data[0] + data[1]*256) + ", " + data[2] + data[3]*256 + "])";
        case 1:
        case 6:
        case 7:
        case 8:
        case 9:
        case 10:
          return "(Parent FormID: 0x" + data[3].ToString("x2") + data[2].ToString("x2") + data[1].ToString("x2") +
                 data[0].ToString("x2") + ")";
      }
      return null;
    }

    public override string GetDesc()
    {
      string desc = "[Record group]" + Environment.NewLine + "Record type: ";
      switch (groupType)
      {
        case 0:
          desc += "Top " + GetSubDesc();
          break;
        case 1:
          desc += "World children " + GetSubDesc();
          break;
        case 2:
          desc += "Interior Cell Block " + GetSubDesc();
          break;
        case 3:
          desc += "Interior Cell Sub-Block " + GetSubDesc();
          break;
        case 4:
          desc += "Exterior Cell Block " + GetSubDesc();
          break;
        case 5:
          desc += "Exterior Cell Sub-Block " + GetSubDesc();
          break;
        case 6:
          desc += "Cell Children " + GetSubDesc();
          break;
        case 7:
          desc += "Topic Children " + GetSubDesc();
          break;
        case 8:
          desc += "Cell Persistent Childen " + GetSubDesc();
          break;
        case 9:
          desc += "Cell Temporary Children " + GetSubDesc();
          break;
        case 10:
          desc += "Cell Visible Distant Children " + GetSubDesc();
          break;
        default:
          desc += "Unknown";
          break;
      }
      return desc + Environment.NewLine +
             "Records: " + Records.Count.ToString() + Environment.NewLine +
             "Size: " + Size.ToString() + " bytes (including header)";
    }

    internal override void SaveData(BinaryWriter bw)
    {
      WriteString(bw, "GRUP");
      bw.Write((uint) Size);
      bw.Write(data);
      bw.Write(groupType);
      bw.Write(dateStamp);
      bw.Write(flags);
      foreach (Rec r in Records)
      {
        r.SaveData(bw);
      }
    }

    internal override List<string> GetIDs(bool lower)
    {
      List<string> list = new List<string>();
      foreach (Record r in Records)
      {
        list.AddRange(r.GetIDs(lower));
      }
      return list;
    }

    public override BaseRecord Clone()
    {
      return new GroupRecord(this);
    }

    public byte[] GetData()
    {
      return (byte[]) data.Clone();
    }

    internal byte[] GetReadonlyData()
    {
      return data;
    }

    public void SetData(byte[] data)
    {
      if (data.Length != 4)
      {
        throw new ArgumentException("data length must be 4");
      }
      for (int i = 0; i < 4; i++)
      {
        this.data[i] = data[i];
      }
    }
  }

  public sealed class Record : Rec
  {
    public readonly List<SubRecord> SubRecords = new List<SubRecord>();
    public uint Flags1;
    public uint Flags2;
    public uint Flags3;
    public uint FormID;

    public override long Size
    {
      get
      {
        long size = 0;
        foreach (SubRecord rec in SubRecords)
        {
          size += rec.Size2;
        }
        return size;
      }
    }

    public override long Size2
    {
      get
      {
        long size = 24;
        foreach (SubRecord rec in SubRecords)
        {
          size += rec.Size2;
        }
        return size;
      }
    }

    public override void DeleteRecord(BaseRecord br)
    {
      SubRecord sr = br as SubRecord;
      if (sr == null)
      {
        return;
      }
      SubRecords.Remove(sr);
    }

    public override void AddRecord(BaseRecord br)
    {
      SubRecord sr = br as SubRecord;
      if (sr == null)
      {
        throw new TESParserException("Record to add was not of the correct type." +
                                     Environment.NewLine + "Records can only hold Subrecords.");
      }
      SubRecords.Add(sr);
    }

    internal Record(string name, uint Size, BinaryReader br, bool Oblivion)
    {
      Name = name;
      Flags1 = br.ReadUInt32();
      FormID = br.ReadUInt32();
      Flags2 = br.ReadUInt32();
      if (!Oblivion)
      {
        Flags3 = br.ReadUInt32();
      }
      if ((Flags1 & 0x00040000) > 0)
      {
        Flags1 ^= 0x00040000;
        uint newSize = br.ReadUInt32();
        br = Decompress(br, (int) (Size - 4), (int) newSize);
        Size = newSize;
      }
      uint AmountRead = 0;
      while (AmountRead < Size)
      {
        string s = ReadRecName(br);
        uint i = 0;
        if (s == "XXXX")
        {
          br.ReadUInt16();
          i = br.ReadUInt32();
          s = ReadRecName(br);
        }
        SubRecord r = new SubRecord(s, br, i);
        AmountRead += (uint) (r.Size2);
        SubRecords.Add(r);
      }
      if (AmountRead > Size)
      {
        throw new TESParserException("Subrecord block did not match the size specified in the record header");
      }

      //br.BaseStream.Position+=Size;
      if (SubRecords.Count > 0 && SubRecords[0].Name == "EDID")
      {
        descriptiveName = " (" + SubRecords[0].GetStrData() + ")";
      }
    }

    private Record(Record r)
    {
      SubRecords = new List<SubRecord>(r.SubRecords.Count);
      for (int i = 0; i < r.SubRecords.Count; i++)
      {
        SubRecords.Add((SubRecord) r.SubRecords[i].Clone());
      }
      Flags1 = r.Flags1;
      Flags2 = r.Flags2;
      Flags3 = r.Flags3;
      FormID = r.FormID;
      Name = r.Name;
      descriptiveName = r.descriptiveName;
    }

    public Record()
    {
      Name = "NEW_";
    }

    public override BaseRecord Clone()
    {
      return new Record(this);
    }

    private string GetBaseDesc()
    {
      return "Type: " + Name + Environment.NewLine +
             "FormID: " + FormID.ToString("x8") + Environment.NewLine +
             "Flags 1: " + Flags1.ToString("x8") +
             (Flags1 == 0 ? "" : " (" + FlagDefs.GetRecFlags1Desc(Flags1) + ")") +
             Environment.NewLine +
             "Flags 2: " + Flags2.ToString("x8") + Environment.NewLine +
             "Flags 3: " + Flags3.ToString("x8") + Environment.NewLine +
             "Subrecords: " + SubRecords.Count.ToString() + Environment.NewLine +
             "Size: " + Size.ToString() + " bytes (excluding header)";
    }

    private string GetExtendedDesc(SubrecordStructure[] sss, dFormIDLookupI formIDLookup)
    {
      if (sss == null)
      {
        return null;
      }
      string s = RecordStructure.Records[Name].description + Environment.NewLine;
      for (int i = 0; i < sss.Length; i++)
      {
        if (sss[i].elements == null)
        {
          return s;
        }
        if (sss[i].notininfo)
        {
          continue;
        }
        s += Environment.NewLine + SubRecords[i].GetFormattedData(sss[i], formIDLookup);
      }
      return s;
    }

    public override string GetDesc()
    {
      return "[Record]" + Environment.NewLine + GetBaseDesc();
    }

    internal string GetDesc(SubrecordStructure[] sss, dFormIDLookupI formIDLookup)
    {
      string start = "[Record]" + Environment.NewLine + GetBaseDesc();
      string end;
      try
      {
        end = GetExtendedDesc(sss, formIDLookup);
      }
      catch
      {
        end =
          "Warning: An error occured while processing the record. It may not conform to the strucure defined in RecordStructure.xml";
      }
      if (end == null)
      {
        return start;
      }
      else
      {
        return start + Environment.NewLine + Environment.NewLine + "[Formatted information]" + Environment.NewLine + end;
      }
    }

    internal override void SaveData(BinaryWriter bw)
    {
      WriteString(bw, Name);
      bw.Write((uint) Size);
      bw.Write(Flags1);
      bw.Write(FormID);
      bw.Write(Flags2);
      bw.Write(Flags3);
      foreach (SubRecord sr in SubRecords)
      {
        sr.SaveData(bw);
      }
    }

    internal override List<string> GetIDs(bool lower)
    {
      List<string> list = new List<string>();
      foreach (SubRecord sr in SubRecords)
      {
        list.AddRange(sr.GetIDs(lower));
      }
      return list;
    }
  }

  public sealed class SubRecord : BaseRecord
  {
    private byte[] Data;

    public override long Size
    {
      get
      {
        return Data.Length;
      }
    }

    public override long Size2
    {
      get
      {
        return 6 + Data.Length + (Data.Length > ushort.MaxValue ? 10 : 0);
      }
    }

    public byte[] GetData()
    {
      return (byte[]) Data.Clone();
    }

    internal byte[] GetReadonlyData()
    {
      return Data;
    }

    public void SetData(byte[] data)
    {
      Data = (byte[]) data.Clone();
    }

    public void SetStrData(string s, bool nullTerminate)
    {
      if (nullTerminate)
      {
        s += '\0';
      }
      Data = System.Text.Encoding.Default.GetBytes(s);
    }

    internal SubRecord(string name, BinaryReader br, uint size)
    {
      Name = name;
      if (size == 0)
      {
        size = br.ReadUInt16();
      }
      else
      {
        br.BaseStream.Position += 2;
      }
      Data = new byte[size];
      br.Read(Data, 0, Data.Length);
    }

    private SubRecord(SubRecord sr)
    {
      Name = sr.Name;
      Data = (byte[]) sr.Data.Clone();
    }

    public override BaseRecord Clone()
    {
      return new SubRecord(this);
    }

    public SubRecord()
    {
      Name = "NEW_";
      Data = new byte[0];
    }

    internal override void SaveData(BinaryWriter bw)
    {
      if (Data.Length > ushort.MaxValue)
      {
        WriteString(bw, "XXXX");
        bw.Write((ushort) 4);
        bw.Write(Data.Length);
        WriteString(bw, Name);
        bw.Write((ushort) 0);
        bw.Write(Data, 0, Data.Length);
      }
      else
      {
        WriteString(bw, Name);
        bw.Write((ushort) Data.Length);
        bw.Write(Data, 0, Data.Length);
      }
    }

    public override string GetDesc()
    {
      return "[Subrecord]" + Environment.NewLine +
             "Name: " + Name + Environment.NewLine +
             "Size: " + Size.ToString() + " bytes (Excluding header)";
    }

    public override void DeleteRecord(BaseRecord br)
    {
    }

    public override void AddRecord(BaseRecord br)
    {
      throw new TESParserException("Subrecords cannot contain additional data.");
    }

    public string GetStrData()
    {
      string s = "";
      foreach (byte b in Data)
      {
        if (b == 0)
        {
          break;
        }
        s += (char) b;
      }
      return s;
    }

    public string GetHexData()
    {
      string s = "";
      foreach (byte b in Data)
      {
        s += b.ToString("X").PadLeft(2, '0') + " ";
      }
      return s;
    }

    internal string GetFormattedData(SubrecordStructure ss, dFormIDLookupI formIDLookup)
    {
      int offset = 0;
      string s = ss.name + " (" + ss.desc + ")" + Environment.NewLine;
      try
      {
        for (int j = 0; j < ss.elements.Length; j++)
        {
          if (offset == Data.Length && j == ss.elements.Length - 1 && ss.elements[j].optional)
          {
            break;
          }
          string s2 = "";
          if (!ss.elements[j].notininfo)
          {
            s2 += ss.elements[j].name + ": ";
          }
          switch (ss.elements[j].type)
          {
            case ElementValueType.Int:
              string tmps =
                TypeConverter.h2si(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]).ToString();
              if (!ss.elements[j].notininfo)
              {
                if (ss.elements[j].hexview)
                {
                  s2 +=
                    TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]).ToString("X8");
                }
                else
                {
                  s2 += tmps;
                }
                if (ss.elements[j].options != null)
                {
                  for (int k = 0; k < ss.elements[j].options.Length; k += 2)
                  {
                    if (tmps == ss.elements[j].options[k + 1])
                    {
                      s2 += " (" + ss.elements[j].options[k] + ")";
                    }
                  }
                }
                else if (ss.elements[j].flags != null)
                {
                  uint val = TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]);
                  string tmp2 = "";
                  for (int k = 0; k < ss.elements[j].flags.Length; k++)
                  {
                    if ((val & (1 << k)) != 0)
                    {
                      if (tmp2.Length > 0)
                      {
                        tmp2 += ", ";
                      }
                      tmp2 += ss.elements[j].flags[k];
                    }
                  }
                  if (tmp2.Length > 0)
                  {
                    s2 += " (" + tmp2 + ")";
                  }
                }
              }
              offset += 4;
              break;
            case ElementValueType.Short:
              tmps = TypeConverter.h2ss(Data[offset], Data[offset + 1]).ToString();
              if (!ss.elements[j].notininfo)
              {
                if (ss.elements[j].hexview)
                {
                  s2 += TypeConverter.h2ss(Data[offset], Data[offset + 1]).ToString("X4");
                }
                else
                {
                  s2 += tmps;
                }
                if (ss.elements[j].options != null)
                {
                  for (int k = 0; k < ss.elements[j].options.Length; k += 2)
                  {
                    if (tmps == ss.elements[j].options[k + 1])
                    {
                      s2 += " (" + ss.elements[j].options[k] + ")";
                    }
                  }
                }
                else if (ss.elements[j].flags != null)
                {
                  uint val = TypeConverter.h2s(Data[offset], Data[offset + 1]);
                  string tmp2 = "";
                  for (int k = 0; k < ss.elements[j].flags.Length; k++)
                  {
                    if ((val & (1 << k)) != 0)
                    {
                      if (tmp2.Length > 0)
                      {
                        tmp2 += ", ";
                      }
                      tmp2 += ss.elements[j].flags[k];
                    }
                  }
                  if (tmp2.Length > 0)
                  {
                    s2 += " (" + tmp2 + ")";
                  }
                }
              }
              offset += 2;
              break;
            case ElementValueType.Byte:
              tmps = Data[offset].ToString();
              if (!ss.elements[j].notininfo)
              {
                if (ss.elements[j].hexview)
                {
                  s2 += Data[offset].ToString("X2");
                }
                else
                {
                  s2 += tmps;
                }
                if (ss.elements[j].options != null)
                {
                  for (int k = 0; k < ss.elements[j].options.Length; k += 2)
                  {
                    if (tmps == ss.elements[j].options[k + 1])
                    {
                      s2 += " (" + ss.elements[j].options[k] + ")";
                    }
                  }
                }
                else if (ss.elements[j].flags != null)
                {
                  int val = Data[offset];
                  string tmp2 = "";
                  for (int k = 0; k < ss.elements[j].flags.Length; k++)
                  {
                    if ((val & (1 << k)) != 0)
                    {
                      if (tmp2.Length > 0)
                      {
                        tmp2 += ", ";
                      }
                      tmp2 += ss.elements[j].flags[k];
                    }
                  }
                  if (tmp2.Length > 0)
                  {
                    s2 += " (" + tmp2 + ")";
                  }
                }
              }
              offset++;
              break;
            case ElementValueType.FormID:
              uint id = TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]);
              if (!ss.elements[j].notininfo)
              {
                s2 += id.ToString("X8");
              }
              if (formIDLookup != null)
              {
                s2 += ": " + formIDLookup(id);
              }
              offset += 4;
              break;
            case ElementValueType.Float:
              if (!ss.elements[j].notininfo)
              {
                s2 += TypeConverter.h2f(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]).ToString();
              }
              offset += 4;
              break;
            case ElementValueType.String:
              if (!ss.elements[j].notininfo)
              {
                while (Data[offset] != 0)
                {
                  s2 += (char) Data[offset++];
                }
              }
              else
              {
                while (Data[offset] != 0)
                {
                  offset++;
                }
              }
              offset++;
              break;
            case ElementValueType.fstring:
              s2 += GetStrData();
              break;
            case ElementValueType.Blob:
              s2 += GetHexData();
              break;
            default:
              throw new ApplicationException();
          }
          if (!ss.elements[j].notininfo)
          {
            s2 += Environment.NewLine;
          }
          if (offset < Data.Length && j == ss.elements.Length - 1 && ss.elements[j].repeat)
          {
            j--;
          }
          s += s2;
        }
      }
      catch
      {
        s += "Warning: Subrecord doesn't seem to match the expected structure" + Environment.NewLine;
      }
      return s;
    }

    internal override List<string> GetIDs(bool lower)
    {
      List<string> list = new List<string>();
      if (Name == "EDID")
      {
        if (lower)
        {
          list.Add(this.GetStrData().ToLower());
        }
        else
        {
          list.Add(this.GetStrData());
        }
      }
      return list;
    }
  }

  internal static class FlagDefs
  {
    public static readonly string[] RecFlags1 =
    {
      "ESM file",
      null,
      null,
      null,
      null,
      "Deleted",
      null,
      null,
      null,
      "Casts shadows",
      "Quest item / Persistent reference",
      "Initially disabled",
      "Ignored",
      null,
      null,
      "Visible when distant",
      null,
      "Dangerous / Off limits (Interior cell)",
      "Data is compressed",
      "Can't wait",
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
    };

    public static string GetRecFlags1Desc(uint flags)
    {
      string desc = "";
      bool b = false;
      for (int i = 0; i < 32; i++)
      {
        if ((flags & (uint) (1 << i)) > 0)
        {
          if (b)
          {
            desc += ", ";
          }
          b = true;
          desc += (RecFlags1[i] == null ? "Unknown (" + ((uint) (1 << i)).ToString("x") + ")" : RecFlags1[i]);
        }
      }
      return desc;
    }
  }
}