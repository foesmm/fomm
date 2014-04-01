using System.Collections.Generic;
using System.IO;
using Fomm.Games.Fallout3.Tools.TESsnip;

namespace Fomm.Games.Fallout3.Tools.InstallTweaker
{
  internal static class EsmTrimmer
  {
    //private static ICSharpCode.SharpZipLib.Zip.Compression.Deflater deflater;

    internal static void Trim(bool stripEdids, bool stripRefs, string In, string Out, ReportProgressDelegate del)
    {
      Plugin p = new Plugin(In, false);

      del("Editing plugin");

      Queue<Rec> queue = new Queue<Rec>(p.Records);
      while (queue.Count > 0)
      {
        if (queue.Peek() is Record)
        {
          queue.Dequeue();
        }
        else
        {
          GroupRecord gr = (GroupRecord) queue.Dequeue();
          if (gr.ContentsType != "GMST")
          {
            foreach (Rec r in gr.Records)
            {
              queue.Enqueue(r);
            }
          }
        }
      }

      del("Generating new esm");

      //deflater=new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(9);
      BinaryWriter bw = new BinaryWriter(File.Create(Out));
      p.SaveData(bw);
      /*foreach(Rec r in p.Records) {
                if(r is GroupRecord) WriteGroup(bw, (GroupRecord)r);
                else WriteRecord(bw, (Record)r);
            }*/
      bw.Close();
    }
  }
}