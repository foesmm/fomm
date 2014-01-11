using System;
using BSAList=System.Collections.Generic.List<fomm.NifViewer.BSAArchive>;
using StringList=System.Collections.Generic.List<string>;
using HashTable=System.Collections.Generic.Dictionary<ulong, fomm.NifViewer.BSAArchive.BSAFileInfo>;
using System.IO;
using System.IO.Compression;

namespace fomm.NifViewer {
    public class BSAArchive {
        private class MeshSorter : System.Collections.Generic.IComparer<string> {
            public int Compare(string a, string b) {
                int i=string.Compare(Path.GetDirectoryName(a), Path.GetDirectoryName(b));
                if(i!=0) return i;
                return string.Compare(Path.GetFileName(a), Path.GetFileName(b));
            }
        }

        [Flags]
        private enum FileFlags : int { Meshes=1, Textures=2 }

        internal struct BSAFileInfo {
            public readonly BinaryReader br;
            public readonly int offset;
            public readonly int size;
            public readonly bool compressed;

            public BSAFileInfo(BinaryReader _br, int _offset, int _size) {
                br=_br;
                offset=_offset;
                size=_size;

                compressed=(size&(1<<30))>0;
                if(compressed) size^=1<<30;

            }

            public MemoryStream Data() {
                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                if(compressed) {
                    byte[] b=new byte[size-4];
                    byte[] output=new byte[br.ReadUInt32()];
                    br.Read(b, 0, size-4);

                    ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf=new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
                    inf.SetInput(b, 0, b.Length);
                    inf.Inflate(output);

                    return new MemoryStream(output);
                } else {
                    return new MemoryStream(br.ReadBytes(size));
                }
            }

            public byte[] RawData {
                get {
                    br.BaseStream.Seek(offset, SeekOrigin.Begin);
                    if(compressed) {
                        byte[] b=new byte[size-4];
                        byte[] output=new byte[br.ReadUInt32()];
                        br.Read(b, 0, size-4);

                        ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf=new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
                        inf.SetInput(b, 0, b.Length);
                        inf.Inflate(output);

                        return output;
                    } else {
                        return br.ReadBytes(size);
                    }
                }
            }
        }

        private struct BSAFileInfo4 {
            public string path;
            public readonly ulong hash;
            public readonly int size;
            public readonly uint offset;

            public BSAFileInfo4(BinaryReader br, bool defaultCompressed) {
                path=null;

                hash=br.ReadUInt64();
                size=br.ReadInt32();
                offset=br.ReadUInt32();

                if(defaultCompressed) size^=(1 << 30);
            }
        }

        private struct BSAFolderInfo4 {
            public string path;
            public readonly ulong hash;
            public readonly int count;
            public int offset;

            public BSAFolderInfo4(BinaryReader br) {
                path=null;
                offset=0;

                hash=br.ReadUInt64();
                count=br.ReadInt32();
                //offset=br.ReadInt32();
                br.BaseStream.Position+=4; //Don't need the offset here
            }
        }

        private struct BSAHeader4 {
            public readonly uint bsaVersion;
            public readonly int directorySize;
            public readonly int archiveFlags;
            public readonly int folderCount;
            public readonly int fileCount;
            public readonly int totalFolderNameLength;
            public readonly int totalFileNameLength;
            public readonly FileFlags fileFlags;

            public BSAHeader4(BinaryReader br) {
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

            public bool ContainsMeshes { get { return (fileFlags&FileFlags.Meshes)>0; } }
            public bool ContainsTextures { get { return (fileFlags&FileFlags.Textures)>0; } }
        }

        private BinaryReader br;
        private static bool Loaded=false;

        private BSAArchive(string path) {
            BSAHeader4 header;
            br=new BinaryReader(File.OpenRead(path), System.Text.Encoding.Default);
            header=new BSAHeader4(br);
            if(header.bsaVersion!=0x67||(!header.ContainsMeshes&&!header.ContainsTextures)) {
                br.Close();
                return;
            }
            bool defaultCompressed=(header.archiveFlags & 0x100)>0;

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
                    if(ext!=".nif"&&ext!=".dds") continue;
                    BSAFileInfo fi=new BSAFileInfo(br, (int)fi4.offset, fi4.size);
                    string fpath=Path.Combine(folderInfo[i].path, Path.GetFileNameWithoutExtension(fi4.path));
                    ulong hash=GenHash(fpath, ext);
                    if(ext==".nif") {
                        Meshes[hash]=fi;
                        AvailableMeshes.Add(fpath);
                    } else Textures[hash]=fi;
                }
            }
            LoadedArchives.Add(this);
        }

        private static readonly BSAList LoadedArchives=new BSAList();
        private static readonly HashTable Meshes=new HashTable();
        private static readonly HashTable Textures=new HashTable();
        private static readonly StringList AvailableMeshes=new StringList();
        private static readonly MeshSorter meshSorter=new MeshSorter();

        public static string[] MeshList {
            get {
                if(!Loaded) Load();
                return AvailableMeshes.ToArray();
            }
        }

        public static Stream GetTexture(string path) {
            if(!Loaded) Load();

            path=path.ToLower().Replace('/', '\\');
            string ext=Path.GetExtension(path);
            if(ext!=".dds") return null;
            if(File.Exists(path)) return File.OpenRead(path);
            ulong hash=GenHash(Path.ChangeExtension(path,null),ext);
            if(!Textures.ContainsKey(hash)) return null;
            return Textures[hash].Data();
        }

        public static Stream GetGlowTexture(string path) {
            path=Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)+"_g")+Path.GetExtension(path);
            return GetTexture(path);
        }

        public static Stream GetNormalTexture(string path) {
            path=Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)+"_n")+Path.GetExtension(path);
            return GetTexture(path);
        }

        public static NifFile LoadMesh(string path) {
            if(!Loaded) Load();

            path=path.ToLower().Replace('/', '\\');
            string ext=Path.GetExtension(path);
            switch(ext) {
            case ".nif":
                if(File.Exists(path)) return new NifFile(path, BasicHLSL.Device);
                ulong hash=GenHash(Path.ChangeExtension(path, null), ext);
                if(!Meshes.ContainsKey(hash)) return null;
                return new NifFile(Meshes[hash].RawData, BasicHLSL.Device);
            case ".dds":
            case ".tga":
            case ".bmp":
            case ".jpg":
                hash=GenHash("meshes\\editorlandplane", ".nif");
                return new NifFile(Meshes[hash].RawData, BasicHLSL.Device, path);
            default:
                return null;
            }
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

        private void Dispose() {
            if(br!=null) {
                br.Close();
                br=null;
            }
        }

        private static void Load() {
            string path="";
            if(File.Exists(path)) new BSAArchive(path);
            else if(Directory.Exists(path)) {
                foreach(string s in Directory.GetFiles(path, "*.bsa")) new BSAArchive(s);
                foreach(string s in Directory.GetFiles(path, "*.nif", SearchOption.AllDirectories)) {
                    string newpath=s.Substring(path.Length).ToLower();
                    if(newpath[0]=='\\') newpath=newpath.Remove(0, 1);
                    AvailableMeshes.Add(Path.ChangeExtension(newpath,null));
                }
            }

            AvailableMeshes.Sort(meshSorter);
            for(int i=0;i<AvailableMeshes.Count-1;i++) if(AvailableMeshes[i]==AvailableMeshes[i+1]) AvailableMeshes.RemoveAt(i--);
            Loaded=true;
        }

        public static void Clear() {
            foreach(BSAArchive BSA in LoadedArchives) BSA.Dispose();
            Meshes.Clear();
            Textures.Clear();
            AvailableMeshes.Clear();
            Loaded=false;
        }
    }
}