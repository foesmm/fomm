using System;
using System.IO;

namespace Fomm.Games.Fallout3.Tools.InstallTweaker
{
    static class BsaTrimmer {
        static int shrunkcount=0;

        private static void Commit(BinaryWriter bw, long offset, byte[] data, long offset2, int add, bool parse) {
            int newsize=0;
            IntPtr pdata;
            if(parse) pdata=NativeMethods.ddsShrink(data, data.Length, out newsize);
            else pdata=IntPtr.Zero;
            byte[] newdata;
            if(pdata==IntPtr.Zero) {
                newdata=data;
            } else {
                shrunkcount++;
                newdata=new byte[newsize];
                System.Runtime.InteropServices.Marshal.Copy(pdata, newdata, 0, newsize);
            }
            bw.Write(newdata);
            bw.BaseStream.Position=offset;
            bw.Write(newdata.Length+add);
            bw.Write((int)offset2);
            bw.BaseStream.Position=bw.BaseStream.Length;
        }

        public static void Trim(IntPtr hwnd, string In, string Out, ReportProgressDelegate del) {
            NativeMethods.ddsInit(hwnd);
            BinaryReader br=new BinaryReader(File.OpenRead(In), System.Text.Encoding.Default);
            BinaryWriter bw=new BinaryWriter(File.Create(Out), System.Text.Encoding.Default);
            System.Text.StringBuilder sb=new System.Text.StringBuilder(64);
            ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf=new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
            bool Compressed, SkipName;

            if(br.ReadInt32()!=0x00415342) throw new Exception("Invalid bsa");
            uint version=br.ReadUInt32();
            bw.Write((int)0x00415342);
            bw.Write(version);
            bw.Write(br.ReadInt32());
            uint flags=br.ReadUInt32();
            if((flags&0x004)>0) { Compressed=true; flags^=0x4; } else Compressed=false;
            if((flags&0x100)>0&&version==0x68) SkipName=true; else SkipName=false;
            flags^=0x2;
            int FolderCount=br.ReadInt32();
            int FileCount=br.ReadInt32();
            bw.Write(flags);

            bw.Write(FolderCount);
            bw.Write(FileCount);
            bw.Write(br.ReadInt32());
            bw.Write(br.ReadInt32());
            bw.Write(br.ReadInt32());

            int[] folderFileCount=new int[FolderCount];
            for(int i=0;i<FolderCount;i++) {
                bw.Write(br.ReadInt64());
                folderFileCount[i]=br.ReadInt32();
                bw.Write(folderFileCount[i]);
                bw.Write(br.ReadInt32());
            }
            int[] fileLengths=new int[FileCount];
            long[] offsetOffsets=new long[FileCount];
            uint[] fileOffsets=new uint[FileCount];
            bool[] parsefiles=new bool[FileCount];
            int file=0;
            for(int i=0;i<FolderCount;i++) {
                byte len=br.ReadByte();
                bw.Write(len);
                sb.Length=0;
                while(--len>0) {
                    char c=br.ReadChar();
                    sb.Append(c);
                    bw.Write(c);
                }
                br.ReadByte();
                bw.Write((byte)0);
                bool parse=true;
                if(sb.ToString().StartsWith("textures\\interface\\")) parse=false;

                for(int j=0;j<folderFileCount[i];j++) {
                    bw.Write(br.ReadUInt64());
                    offsetOffsets[file]=br.BaseStream.Position;
                    fileLengths[file]=br.ReadInt32();
                    bw.Write(fileLengths[file]);
                    fileOffsets[file]=br.ReadUInt32();
                    bw.Write(fileOffsets[file]);
                    parsefiles[file]=parse;
                    file++;
                }
            }

            for(int i=0;i<FileCount;i++) {
                sb.Length=0;
                while(true) {
                    char c=(char)br.ReadByte();
                    //bw.Write(c);
                    if(c=='\0') break;
                    sb.Append(c);
                }
                if(!sb.ToString().EndsWith(".dds", StringComparison.OrdinalIgnoreCase)) parsefiles[i]=false;
            }

            int count=0;
            for(int i=0;i<FileCount;i++) {
                if((i%100)==0) del("Processing file "+i+" of "+FileCount);
                br.BaseStream.Position=fileOffsets[i];
                long offset=bw.BaseStream.Position;
                int add=0;
                if(SkipName) {
                    byte len=br.ReadByte();
                    bw.Write(len);
                    bw.Write(br.ReadBytes(len+1));
                    add=len+2;
                }
                bool compressed2=Compressed;
                if((fileLengths[i]&(1<<30))!=0) {
                    compressed2=!compressed2;
                    fileLengths[i]^=(1<<30);
                }
                if(!compressed2) {
                    byte[] bytes=new byte[fileLengths[i]];
                    br.Read(bytes, 0, fileLengths[i]);
                    Commit(bw, offsetOffsets[i], bytes, offset, add, parsefiles[i]);
                } else {
                    count++;
                    byte[] uncompressed=new byte[br.ReadUInt32()];
                    byte[] compressed=new byte[fileLengths[i]-4];
                    br.Read(compressed, 0, fileLengths[i]-4);
                    inf.Reset();
                    inf.SetInput(compressed);
                    inf.Inflate(uncompressed);
                    Commit(bw, offsetOffsets[i], uncompressed, offset, add, parsefiles[i]);
                }
            }

            br.Close();
            bw.Close();
            NativeMethods.ddsClose();
        }
    }
    
}
