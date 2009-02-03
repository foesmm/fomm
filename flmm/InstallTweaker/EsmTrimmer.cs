using System;
using System.Collections.Generic;
using fomm.TESsnip;
using System.IO;

namespace fomm.InstallTweaker {
    static class EsmTrimmer {
        private static ICSharpCode.SharpZipLib.Zip.Compression.Deflater deflater;

        private static void WriteString(BinaryWriter bw, string s) {
            byte[] b = new byte[4];
            for(int i=0;i<4;i++) b[i]=(byte)s[i];
            bw.Write(b, 0, 4);
        }

        private static byte[] outputbuffer=new byte[ushort.MaxValue];
        private static void WriteRecord(BinaryWriter bw, Record r) {
            if(r.Size>4096) {
                MemoryStream ms=new MemoryStream();
                BinaryWriter bw2=new BinaryWriter(ms);
                foreach(SubRecord sr in r.SubRecords) sr.SaveData(bw2);
                deflater.Reset();
                deflater.SetInput(ms.GetBuffer(), 0, (int)ms.Length);
                deflater.Finish();
                int i=deflater.Deflate(outputbuffer);
                bw2.Close();
                if(i==ushort.MaxValue) {
                    throw new Exception("Don't handle this yet!");
                }
                if(i<ms.Length*0.75) {
                    WriteString(bw, r.Name);
                    bw.Write(i+4);
                    bw.Write(r.Flags1|0x00040000);
                    bw.Write(r.FormID);
                    bw.Write(r.Flags2);
                    bw.Write(r.Flags3);
                }
            }
            r.SaveData(bw);
        }

        private static void WriteGroup(BinaryWriter bw, GroupRecord gr) {
            WriteString(bw, "GRUP");
            long pos=bw.BaseStream.Position;
            bw.Write(0);
            bw.Write(gr.GetReadonlyData());
            bw.Write(gr.groupType);
            bw.Write(gr.dateStamp);
            bw.Write(gr.flags);
            long start=bw.BaseStream.Position;
            foreach(Rec r in gr.Records) {
                if(r is GroupRecord) WriteGroup(bw, (GroupRecord)r);
                else WriteRecord(bw, (Record)r);
            }
            long end=bw.BaseStream.Position;
            bw.BaseStream.Position=pos;
            bw.Write((uint)(24+(end-start)));
            bw.BaseStream.Position=bw.BaseStream.Length;
        }

        internal static void Trim(bool stripEdids, bool stripRefs) {
            Plugin p=new Plugin("data\\", false);

            Queue<Rec> queue=new Queue<Rec>(p.Records);
            while(queue.Count>0) {
                if(queue.Peek() is Record) {
                    Record r=(Record)queue.Dequeue();
                    if(stripEdids) {
                        if(r.SubRecords.Count>0&&r.SubRecords[0].Name=="EDID") r.SubRecords.RemoveAt(0);
                        for(int i=0;i<r.SubRecords.Count;i++) {
                            if(r.SubRecords[i].Name=="SCTX") r.SubRecords.RemoveAt(i--);
                        }
                    }
                } else {
                    GroupRecord gr=(GroupRecord)queue.Dequeue();
                    if(gr.Name!="GMST") {
                        foreach(Rec r in gr.Records) queue.Enqueue(r);
                    }
                }
            }

            deflater=new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(9);
            BinaryWriter bw=new BinaryWriter(File.Create(""));
            foreach(Rec r in p.Records) {
                if(r is GroupRecord) WriteGroup(bw, (GroupRecord)r);
                else WriteRecord(bw, (Record)r);
            }
            bw.Close();
        }
    }
}
