using System;
using StringList=System.Collections.Generic.List<string>;
using HashTable=System.Collections.Generic.Dictionary<ulong, fomm.BSAArchive.BSAFileInfo>;
using System.IO;
using System.IO.Compression;

namespace fomm {
    class BSAArchive {
        internal class BSALoadException : Exception { }

        [Flags]
        private enum FileFlags : int { Meshes=1, Textures=2 }

        internal struct BSAFileInfo {
            internal readonly BSAArchive bsa;
            internal readonly int offset;
            internal readonly int size;
            internal readonly bool compressed;

            internal BSAFileInfo(BSAArchive _bsa, int _offset, int _size) {
                bsa=_bsa;
                offset=_offset;
                size=_size;

                if((size&(1<<30))!=0) {
                    size^=1<<30;
                    compressed=!bsa.defaultCompressed;
                } else compressed=bsa.defaultCompressed;

            }

            internal byte[] GetRawData() {
                bsa.br.BaseStream.Seek(offset, SeekOrigin.Begin);
                if(bsa.SkipNames) bsa.br.BaseStream.Position+=bsa.br.ReadByte()+1;
                if(compressed) {
                    byte[] b=new byte[size-4];
                    byte[] output=new byte[bsa.br.ReadUInt32()];
                    bsa.br.Read(b, 0, size-4);

                    ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf=new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
                    inf.SetInput(b, 0, b.Length);
                    inf.Inflate(output);

                    return output;
                } else {
                    return bsa.br.ReadBytes(size);
                }
            }
        }

        private struct BSAFileInfo4 {
            internal string path;
            internal readonly ulong hash;
            internal readonly int size;
            internal readonly uint offset;

            internal BSAFileInfo4(BinaryReader br, bool defaultCompressed) {
                path=null;

                hash=br.ReadUInt64();
                size=br.ReadInt32();
                offset=br.ReadUInt32();
            }
        }

        private struct BSAFolderInfo4 {
            internal string path;
            internal readonly ulong hash;
            internal readonly int count;
            internal int offset;

            internal BSAFolderInfo4(BinaryReader br) {
                path=null;
                offset=0;

                hash=br.ReadUInt64();
                count=br.ReadInt32();
                //offset=br.ReadInt32();
                br.BaseStream.Position+=4; //Don't need the offset here
            }
        }

        private struct BSAHeader4 {
            internal readonly uint bsaVersion;
            internal readonly int directorySize;
            internal readonly int archiveFlags;
            internal readonly int folderCount;
            internal readonly int fileCount;
            internal readonly int totalFolderNameLength;
            internal readonly int totalFileNameLength;
            internal readonly FileFlags fileFlags;

            internal BSAHeader4(BinaryReader br) {
                br.BaseStream.Position+=4;
                bsaVersion=br.ReadUInt32();
                directorySize=br.ReadInt32();
                archiveFlags=br.ReadInt32();
                folderCount=br.ReadInt32();
                fileCount=br.ReadInt32();
                totalFolderNameLength=br.ReadInt32();
                totalFileNameLength=br.ReadInt32();
                fileFlags=(FileFlags)br.ReadInt32();
            }

            internal bool ContainsMeshes { get { return (fileFlags&FileFlags.Meshes)!=0; } }
            internal bool ContainsTextures { get { return (fileFlags&FileFlags.Textures)!=0; } }
        }

        private BinaryReader br;
        private string name;
        private bool defaultCompressed;
        private bool SkipNames;
        private HashTable files;

        internal BSAArchive(string path) {
            name=Path.GetFileNameWithoutExtension(path).ToLower();
            BSAHeader4 header;
            br=new BinaryReader(File.OpenRead(path), System.Text.Encoding.Default);
            header=new BSAHeader4(br);
            if(header.bsaVersion!=0x68) throw new BSALoadException();
            defaultCompressed=(header.archiveFlags & 4)>0;
            SkipNames=(header.archiveFlags&0x100)>0;
            files=new HashTable();

            //Read folder info
            BSAFolderInfo4[] folderInfo = new BSAFolderInfo4[header.folderCount];
            BSAFileInfo4[] fileInfo = new BSAFileInfo4[header.fileCount];
            for(int i=0;i<header.folderCount;i++) folderInfo[i]=new BSAFolderInfo4(br);
            int count=0;
            for(uint i=0;i<header.folderCount;i++) {
                folderInfo[i].path=new string(br.ReadChars(br.ReadByte()-1));
                br.BaseStream.Position++;
                folderInfo[i].offset=count;
                for(int j=0;j<folderInfo[i].count;j++) fileInfo[count+j]=new BSAFileInfo4(br, defaultCompressed);
                count += folderInfo[i].count;
            }
            for(uint i=0;i<header.fileCount;i++) {
                fileInfo[i].path="";
                char c;
                while((c=br.ReadChar())!='\0') fileInfo[i].path+=c;
            }

            for(int i=0;i<header.folderCount;i++) {
                for(int j=0;j<folderInfo[i].count;j++) {
                    BSAFileInfo4 fi4=fileInfo[folderInfo[i].offset+j];
                    string ext=Path.GetExtension(fi4.path);
                    BSAFileInfo fi=new BSAFileInfo(this, (int)fi4.offset, fi4.size);
                    string fpath=Path.Combine(folderInfo[i].path, Path.GetFileNameWithoutExtension(fi4.path));
                    ulong hash=GenHash(fpath, ext);
                    files[hash]=fi;
                }
            }
        }

        private static ulong GenHash(string file) {
            file=file.ToLowerInvariant().Replace('/', '\\');
            return GenHash(Path.ChangeExtension(file, null), Path.GetExtension(file));
        }
        private static ulong GenHash(string file, string ext) {
            file=file.ToLower();
            ext=ext.ToLower();
            ulong hash=0;
            if(file.Length>0) {
                hash=(ulong)(
                   (((byte)file[file.Length-1])*0x1)+
                    ((file.Length>2?(byte)file[file.Length-2]:(byte)0)*0x100)+
                     (file.Length*0x10000)+
                    (((byte)file[0])*0x1000000)
                );
            }
            if(file.Length>3) {
                hash+=(ulong)(GenHash2(file.Substring(1, file.Length-3))*0x100000000);
            }
            if(ext.Length>0) {
                hash+=(ulong)(GenHash2(ext)*0x100000000);
                byte i=0;
                switch(ext) {
                case ".nif": i=1; break;
                //case ".kf": i=2; break;
                case ".dds": i=3; break;
                //case ".wav": i=4; break;
                }
                if(i!=0) {
                    byte a=(byte)(((i&0xfc)<<5)+(byte)((hash&0xff000000)>>24));
                    byte b=(byte)(((i&0xfe)<<6)+(byte)(hash&0xff));
                    byte c=(byte)((i<<7)+(byte)((hash&0xff00)>>8));
                    hash-=hash&0xFF00FFFF;
                    hash+=(uint)((a<<24)+b+(c<<8));
                }
            }
            return hash;
        }

        private static uint GenHash2(string s) {
            uint hash=0;
            for(int i=0;i<s.Length;i++) {
                hash*=0x1003f;
                hash+=(byte)s[i];
            }
            return hash;
        }

        internal void Dispose() {
            if(files!=null) files.Clear();
            if(br!=null) {
                br.Close();
                br=null;
            }
        }

        internal byte[] GetFile(string path) {
            ulong hash=GenHash(path);
            if(!files.ContainsKey(hash)) return null;
            else return files[hash].GetRawData();
        }
    }
}
