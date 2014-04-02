using System.Collections.Generic;
using Fomm.Games.Fallout3.Tools.TESsnip;
using System.IO;

namespace Fomm.Games.Fallout3.Tools.InstallTweaker
{
  internal static class EsmTrimmer
  {
    //private static ICSharpCode.SharpZipLib.Zip.Compression.Deflater deflater;

    private static void WriteString(BinaryWriter bw, string s)
    {
      var b = new byte[4];
      for (var i = 0; i < 4; i++)
      {
        b[i] = (byte) s[i];
      }
      bw.Write(b, 0, 4);
    }

    private static byte[] outputbuffer = new byte[ushort.MaxValue];

    private static void WriteRecord(BinaryWriter bw, Record r)
    {
      /*if(r.Size>4096) {
                MemoryStream ms=new MemoryStream();
                BinaryWriter bw2=new BinaryWriter(ms);
                foreach(SubRecord sr in r.SubRecords) sr.SaveData(bw2);
                int len=(int)ms.Length;
                deflater.Reset();
                deflater.SetInput(ms.GetBuffer(), 0, len);
                deflater.Finish();
                int i=deflater.Deflate(outputbuffer);
                bw2.Close();
                if(i==ushort.MaxValue) {
                    throw new Exception("Don't handle this yet!");
                }
                if(i<len*0.75) {
                    WriteString(bw, r.Name);
                    bw.Write(i+4);
                    bw.Write(r.Flags1|0x00040000);
                    bw.Write(r.FormID);
                    bw.Write(r.Flags2);
                    bw.Write(r.Flags3);
                    return;
                }
            }*/
      r.SaveData(bw);
    }

    private static void WriteGroup(BinaryWriter bw, GroupRecord gr)
    {
      WriteString(bw, "GRUP");
      var pos = bw.BaseStream.Position;
      bw.Write(0);
      bw.Write(gr.GetReadonlyData());
      bw.Write(gr.groupType);
      bw.Write(gr.dateStamp);
      bw.Write(gr.flags);
      var start = bw.BaseStream.Position;
      foreach (var r in gr.Records)
      {
        if (r is GroupRecord)
        {
          WriteGroup(bw, (GroupRecord) r);
        }
        else
        {
          WriteRecord(bw, (Record) r);
        }
      }
      var end = bw.BaseStream.Position;
      bw.BaseStream.Position = pos;
      bw.Write((uint) (24 + (end - start)));
      bw.BaseStream.Position = bw.BaseStream.Length;
    }

    internal static void Trim(bool stripEdids, bool stripRefs, string In, string Out, ReportProgressDelegate del)
    {
      var p = new Plugin(In, false);

      del("Editing plugin");

      var queue = new Queue<Rec>(p.Records);
      while (queue.Count > 0)
      {
        if (queue.Peek() is Record)
        {
          var r = (Record) queue.Dequeue();
          if (stripEdids)
          {
            //if(r.SubRecords.Count>0&&r.SubRecords[0].Name=="EDID") r.SubRecords.RemoveAt(0);
            for (var i = 0; i < r.SubRecords.Count; i++)
            {
              //if(r.SubRecords[i].Name=="SCTX") r.SubRecords.RemoveAt(i--);
            }
          }
        }
        else
        {
          var gr = (GroupRecord) queue.Dequeue();
          if (gr.ContentsType != "GMST")
          {
            foreach (var r in gr.Records)
            {
              queue.Enqueue(r);
            }
          }
        }
      }

      del("Generating new esm");

      //deflater=new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(9);
      var bw = new BinaryWriter(File.Create(Out));
      p.SaveData(bw);
      /*foreach(Rec r in p.Records) {
                if(r is GroupRecord) WriteGroup(bw, (GroupRecord)r);
                else WriteRecord(bw, (Record)r);
            }*/
      bw.Close();
    }
  }
}