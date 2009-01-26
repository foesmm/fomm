using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace fomm {
    internal partial class BSACreator : Form {

        internal static ulong GenHash(string s) {
            string extension="";
            int i=s.LastIndexOf('.');
            if(i!=-1) {
                extension=s.Substring(i);
                s=s.Remove(i);
            }
            return GenHash(s, extension);
        }

        internal static ulong GenHash(string file, string ext) {
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
            if(file.Length>3) hash+=(ulong)(GenHashInternal(file.Substring(1, file.Length-3))*0x100000000);
            if(ext.Length>0) {
                hash+=(ulong)(GenHashInternal(ext)*0x100000000);
                byte i=0;
                switch(ext) {
                case ".nif": i=1; break;
                case ".kf": i=2; break;
                case ".dds": i=3; break;
                case ".wav": i=4; break;
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

        private static uint GenHashInternal(string s) {
            uint hash=0;
            for(int i=0;i<s.Length;i++) {
                hash*=0x1003f;
                hash+=(byte)s[i];
            }
            return hash;
        }

        private class FolderRecord {
            internal readonly List<FileRecord> files;
            internal readonly string name;
            internal readonly ulong hash;
            internal long offsetpos;

            internal FolderRecord(string _name) {
                name=_name;
                hash=GenHash(name, "");
                files=new List<FileRecord>();
            }

            //internal override string ToString() { return name; }
        }
        private class FileRecord {
            internal readonly string filepath;
            internal readonly uint size;
            internal readonly string name;
            internal readonly ulong hash;
            internal long offsetpos;
            internal bool DoCompress;

            internal FileRecord(ListViewItem lvi) {
                name=Path.GetFileName(lvi.Text);
                hash=GenHash(name);
                filepath=lvi.SubItems[1].Text;
                size=(uint)(new FileInfo(filepath)).Length;
                DoCompress=lvi.Checked;
            }
        }

        private class FolderRecordComparer : IComparer<FolderRecord> {
            public int Compare(FolderRecord a, FolderRecord b) {
                if(a.hash==b.hash) return 0;
                if(a.hash>b.hash) return 1; else return -1;
            }
        }
        private class FileRecordComparer : IComparer<FileRecord> {
            public int Compare(FileRecord a, FileRecord b) {
                if(a.hash==b.hash) return 0;
                if(a.hash>b.hash) return 1; else return -1;
            }
        }
        private class ListViewSorter : System.Collections.IComparer {
            public int Compare(object oa, object ob) {
                ListViewItem a=(ListViewItem)oa;
                ListViewItem b=(ListViewItem)ob;
                int i=string.Compare(Path.GetDirectoryName(a.Text), Path.GetDirectoryName(b.Text));
                if(i!=0) return i;
                return string.Compare(Path.GetFileName(a.Text), Path.GetFileName(b.Text));
            }
        }

        private List<FolderRecord> folders;
        private List<FileRecord> files;
        private readonly ListViewSorter sorter=new ListViewSorter();

        private void CreateFileRecords() {
            folders=new List<FolderRecord>();
            files=new List<FileRecord>();
            string folder=null;
            FolderRecord currentfolder=null;
            foreach(ListViewItem lvi in lvFiles.Items) {
                string newfolder=Path.GetDirectoryName(lvi.Text);
                if(newfolder!=folder) {
                    currentfolder=new FolderRecord(newfolder);
                    folders.Add(currentfolder);
                    folder=newfolder;
                }
                FileRecord fr=new FileRecord(lvi);
                if(fr.size>=(1<<30)) {
                    MessageBox.Show("Error: File '"+fr.filepath+"' is too big to store in a BSA archive");
                    continue;
                }
                files.Add(fr);
                currentfolder.files.Add(fr);
            }
            FolderRecordComparer frc1=new FolderRecordComparer();
            FileRecordComparer frc2=new FileRecordComparer();
            folders.Sort(frc1);
            foreach(FolderRecord fr in folders) fr.files.Sort(frc2);
        }

        internal BSACreator() {
            InitializeComponent();
            lvFiles.ListViewItemSorter=sorter;
        }

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled=true;
        }

        private void bAddFile_Click(object sender, EventArgs e) {
            openFileDialog1.Title="Select files to include";
            openFileDialog1.Filter="any file|*.*";
            if(openFileDialog1.ShowDialog()!=DialogResult.OK) return;
            if(openFileDialog1.FileNames.Length>1) lvFiles.Sorting=SortOrder.None;
            foreach(string s in openFileDialog1.FileNames) {
                string path=s.ToLower();
                /*if(path.StartsWith(Program.CurrentDir+"data\\")) {
                    path=path.Substring((Program.CurrentDir+"data\\").Length);
                } else*/ if(path.StartsWith("data\\")) {
                    path=path.Substring(5);
                } else {
                    path=Path.GetFileName(path);
                }
                ListViewItem lvi=new ListViewItem(new string[] { path, s });
                lvFiles.Items.Add(lvi);
            }
            if(openFileDialog1.FileNames.Length>1) {
                lvFiles.Sorting=SortOrder.Ascending;
                lvFiles.ListViewItemSorter=sorter;
            }
            ValidateFiles();
        }

        private void bAddEsp_Click(object sender, EventArgs e) {
            MessageBox.Show("Option disabled until the fallout .esm/esp format has been checked", "Error");
            /*openFileDialog1.Title="Select plugins to add";
            openFileDialog1.Filter="any plugins (*.esp, *.esm)|*.esp;*.esm";
            if(openFileDialog1.ShowDialog()!=DialogResult.OK) return;
            lvFiles.Sorting=SortOrder.None;
            foreach(string file in openFileDialog1.FileNames) {
                string[] files=ConflictDetector.TesFile.GetDataFileList(file);
                foreach(string s in files) {
                    if(!File.Exists("data\\"+s)) continue;
                    ListViewItem lvi=new ListViewItem(new string[] { s.ToLower(), Path.GetFullPath("data\\"+s) });
                    lvFiles.Items.Add(lvi);
                }
            }
            lvFiles.Sorting=SortOrder.Ascending;
            lvFiles.ListViewItemSorter=sorter;
            ValidateFiles();*/
        }

        private void ValidateFiles() {
            //Remove missing files
            for(int i=0;i<lvFiles.Items.Count;i++) {
                if(!File.Exists(lvFiles.Items[i].SubItems[1].Text)) lvFiles.Items.RemoveAt(i--);
            }
            //Remove duplicate or absolute relative paths
            string previous=lvFiles.Items[0].Text.ToLower();
            string next;
            for(int i=1;i<lvFiles.Items.Count;i++) {
                next=lvFiles.Items[i].Text.ToLower();
                if(next==previous||!Program.IsSafeFileName(next)||next=="\\") {
                    lvFiles.Items.RemoveAt(i--);
                } else {
                    previous=next;
                }
            }
        }

        private void bAddFolder_Click(object sender, EventArgs e) {
            /*while(Settings.BSACreatorFolderBrowserDir.IndexOf('\\')!=-1) {
                if(Directory.Exists(Settings.BSACreatorFolderBrowserDir)) {
                    folderBrowserDialog1.SelectedPath=Settings.BSACreatorFolderBrowserDir;
                    break;
                }
                Settings.BSACreatorFolderBrowserDir=Settings.BSACreatorFolderBrowserDir.Substring(0, Settings.BSACreatorFolderBrowserDir.LastIndexOf('\\'));
            }*/
            if(folderBrowserDialog1.ShowDialog()!=DialogResult.OK) return;
            //Settings.BSACreatorFolderBrowserDir=folderBrowserDialog1.SelectedPath;
            string s=folderBrowserDialog1.SelectedPath.ToLower();
            string relative="";
            //if(s.StartsWith(Program.CurrentDir+"data\\")) relative=s.Substring((Program.CurrentDir+"data\\").Length)+"\\";
            //else relative="";
            lvFiles.Sorting=SortOrder.None;
            foreach(string file in Directory.GetFiles(s, "*", SearchOption.AllDirectories)) {
                ListViewItem lvi=new ListViewItem(new string[] { relative+file.Substring(s.Length+1).ToString(), file});
                lvFiles.Items.Add(lvi);
            }
            lvFiles.Sorting=SortOrder.Ascending;
            lvFiles.ListViewItemSorter=sorter;
            ValidateFiles();
        }

        private void bCreate_Click(object sender, EventArgs e) {
            if(saveFileDialog1.ShowDialog()!=DialogResult.OK) return;
            foreach(ListViewItem lvi in lvFiles.Items) {
                //Not sure why this was here. Cant htink of any reason why I wouldn't want to change the file name to lowercase
                //lvi.Text=Path.GetDirectoryName(lvi.Text).ToLower()+"\\"+Path.GetFileName(lvi.Text);
                lvi.Text=lvi.Text.ToLower();
            }
            lvFiles.Sort();
            CreateFileRecords();
            BinaryWriter bw;
            try {
                bw=new BinaryWriter(File.Create(saveFileDialog1.FileName));
            } catch(Exception ex) {
                MessageBox.Show("BSA creation failed\n"+ex.Message, "Error");
                return;
            }
            try {
                GenerateBSA(bw, false);
            } catch(Exception ex) {
                MessageBox.Show("An error occured during BSA generation\n"+ex.Message, "Error");
            } finally {
                bw.Close();
            }
        }

        private uint CheckFileTypes() {
            uint result=0;
            foreach(ListViewItem lvi in lvFiles.Items) {
                switch(Path.GetExtension(lvi.Text)) {
                case ".nif":
                    result|=0x001;
                    break;
                case ".dds":
                    result|=0x002;
                    break;
                case ".xml":
                    result|=0x004;
                    break;
                case ".wav":
                    result|=0x008;
                    break;
                case ".mp3":
                    result|=0x010;
                    break;
                case ".txt":
                case ".html":
                case ".htm":
                case ".bat":
                case ".scc":
                    result|=0x020;
                    break;
                case ".spt":
                    result|=0x040;
                    break;
                case ".tex":
                case ".fon":
                    result|=0x080;
                    break;
                case ".ctl":
                    result|=0x100;
                    break;
                }
            }
            //result|=0x002;
            return result;
        }

        private uint GetTotalFolderNameLength() {
            int i=0;
            foreach(FolderRecord fr in folders) i+=fr.name.Length+1;
            return (uint)i;
        }

        private uint GetTotalFileNameLength() {
            int i=0;
            foreach(FileRecord fr in files) i+=fr.name.Length+1;
            return (uint)i;
        }

        private void GenerateBSA(BinaryWriter bw, bool IsRedirection) {
            bw.Write((byte)'B');
            bw.Write((byte)'S');
            bw.Write((byte)'A');
            bw.Write((byte)0);
            bw.Write((uint)0x67);
            bw.Write((uint)36);
            uint flags;
            bool Compressed;
            if(cmbCompression.SelectedIndex==4||cmbCompression.SelectedIndex==5) {
                flags=7+1792;
                Compressed=true;
            } else {
                flags=3+1792;
                Compressed=false;
            }
            bw.Write((uint)flags);
            bw.Write((uint)folders.Count);
            bw.Write((uint)lvFiles.Items.Count);
            bw.Write(GetTotalFolderNameLength());
            bw.Write(GetTotalFileNameLength());
            bw.Write(CheckFileTypes()|(uint)(IsRedirection?2:0));
            //folder records
            foreach(FolderRecord fr in folders) {
                bw.Write(fr.hash);
                bw.Write(fr.files.Count);
                fr.offsetpos=bw.BaseStream.Position;
                bw.Write((uint)0);
            }
            //File Records
            foreach(FolderRecord fr in folders) {
                bw.BaseStream.Position=fr.offsetpos;
                bw.Write((uint)(bw.BaseStream.Length+GetTotalFileNameLength()));
                bw.BaseStream.Position=bw.BaseStream.Length;
                bw.Write((byte)(fr.name.Length+1));
                for(int i=0;i<fr.name.Length;i++) bw.Write((byte)fr.name[i]);
                bw.Write((byte)0);
                foreach(FileRecord fr2 in fr.files) {
                    bw.Write(fr2.hash);
                    fr2.offsetpos=bw.BaseStream.Position;
                    bw.Write((ulong)0);//size and data offset
                }
            }
            //Filenames
            foreach(FolderRecord fr in folders) {
                foreach(FileRecord fr2 in fr.files) {
                    for(int i=0;i<fr2.name.Length;i++) bw.Write((byte)fr2.name[i]);
                    bw.Write((byte)0);
                }
            }
            //raw data
            foreach(FolderRecord fr in folders) {
                foreach(FileRecord fr2 in fr.files) {
                    byte[] comp=null;
                    if(cmbCompression.SelectedIndex!=0&&(cmbCompression.SelectedIndex!=6||fr2.DoCompress)&&((comp=CompressRecord(fr2.filepath))!=null)) {
                        bw.BaseStream.Position=fr2.offsetpos;
                        bw.Write((uint)((uint)(comp.Length+4)|(uint)(Compressed?0:(1<<30))));
                        bw.Write((uint)bw.BaseStream.Length);
                        bw.BaseStream.Position=bw.BaseStream.Length;
                        bw.Write(fr2.size);
                        bw.Write(comp);
                    } else {
                        bw.BaseStream.Position=fr2.offsetpos;
                        bw.Write(fr2.size|(uint)(Compressed?(1<<30):0));
                        bw.Write((uint)bw.BaseStream.Length);
                        bw.BaseStream.Position=bw.BaseStream.Length;
                        bw.Write(File.ReadAllBytes(fr2.filepath));
                    }
                }
            }
        }

        private byte[] CompressRecord(string path) {
            int len=(int)((new FileInfo(path)).Length);
            int level=5;
            switch(cmbCompLevel.SelectedIndex) {
            case 0: level=9; break;
            case 1: level=7; break;
            case 2: level=5; break;
            case 3: level=3; break;
            case 4: level=1; break;
            }
            ICSharpCode.SharpZipLib.Zip.Compression.Deflater def=new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(level);
            byte[] In=File.ReadAllBytes(path);
            byte[] Out=new byte[In.Length+64];
            def.SetInput(In);
            def.Finish();
            int size=def.Deflate(Out);
            Array.Resize<byte>(ref Out, size);

            float mul=0;
            switch(cmbCompression.SelectedIndex) {
            case 6:
            case 5:
                return Out;
            case 4: mul=0.8f; break;
            case 3: mul=0.6f; break;
            case 2: mul=0.4f; break;
            case 1: mul=0.2f; break;
            }
            float f=(float)size/(float)len;
            if(f<mul) return Out; else  return null;
        }

        private void cmbCompression_SelectedIndexChanged(object sender, EventArgs e) {
            if(cmbCompression.SelectedIndex==6) lvFiles.CheckBoxes=true;
            else lvFiles.CheckBoxes=false;
            if(cmbCompression.SelectedIndex==0) cmbCompLevel.Enabled=false;
            else cmbCompLevel.Enabled=true;
        }

        private void lvFiles_AfterLabelEdit(object sender, LabelEditEventArgs e) {
            if(e.Label==null) return;
            if(e.Label.ToLower()!=e.Label) {
                e.CancelEdit=true;
                lvFiles.Items[e.Item].Text=e.Label.ToLower();
            }
        }
    }
}