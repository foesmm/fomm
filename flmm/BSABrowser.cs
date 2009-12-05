using System;
using System.IO;
using System.Windows.Forms;

namespace Fomm {
    internal partial class BSABrowser : Form {
        internal BSABrowser() {
            InitializeComponent();
            this.Icon=Fomm.Properties.Resources.fomm02;
            string path=Settings.GetString("LastBSAUnpackPath");
            if(path!=null) SaveAllDialog.SelectedPath=path;

            Settings.GetWindowPosition("BSABrowser", this);
        }

        private void BSABrowser_Load(object sender, EventArgs e) {
            string tmp=Settings.GetString("BSABrowserPanelSplit");
            if(tmp!=null) {
                try {
                    splitContainer1.SplitterDistance=Math.Max(splitContainer1.Panel1MinSize+1, Math.Min(splitContainer1.Width-(splitContainer1.Panel2MinSize+1), int.Parse(tmp)));
                } catch { }
            }
        }

        internal BSABrowser(string BSAPath) : this() {
            OpenArchive(BSAPath);
        }

        private class BSAFileEntry {
            private static readonly ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf=new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
            internal readonly bool Compressed;
            private string fileName;
            private string lowername;
            internal string FileName {
                get { return fileName; }
                set {
                    if(value==null) return;
                    fileName=value;
                    //lowername=Folder.ToLower()+"\\"+fileName.ToLower();
                    lowername=Path.Combine(Folder.ToLower(), fileName.ToLower());
                }
            }
            internal string LowerName {
                get { return lowername; }
            }
            internal readonly string Folder;
            internal readonly uint Offset;
            internal readonly uint Size;
            internal readonly uint RealSize;

            internal BSAFileEntry(bool compressed, string folder, uint offset,uint size) {
                Compressed=compressed;
                Folder=folder;
                Offset=offset;
                Size=size;
            }

            internal BSAFileEntry(string path, uint offset, uint size) {
                Folder=Path.GetDirectoryName(path);
                FileName=Path.GetFileName(path);
                Offset=offset;
                Size=size;
            }

            internal BSAFileEntry(string path, uint offset, uint size, uint realSize) {
                Folder=Path.GetDirectoryName(path);
                if(path.EndsWith("color.pal")) {
                    int iii=0;
                }
                FileName=Path.GetFileName(path);
                Offset=offset;
                Size=size;
                RealSize=realSize;
                Compressed=realSize!=0;
            }

            internal void Extract(string path, bool UseFolderName, BinaryReader br, bool SkipName) {
                if(UseFolderName) {
                    path+="\\"+Folder+"\\"+FileName;
                }
                if(!Directory.Exists(Path.GetDirectoryName(path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                FileStream fs=File.Create(path);
                br.BaseStream.Position=Offset;
                if(SkipName) br.BaseStream.Position+=br.ReadByte()+1;
                if(!Compressed) {
                    byte[] bytes=new byte[Size];
                    br.Read(bytes, 0, (int)Size);
                    fs.Write(bytes, 0, (int)Size);
                } else {
                    byte[] uncompressed;
                    if(RealSize==0) uncompressed=new byte[br.ReadUInt32()];
                    else uncompressed=new byte[RealSize];
                    byte[] compressed=new byte[Size-4];
                    br.Read(compressed, 0, (int)(Size-4));
                    inf.Reset();
                    inf.SetInput(compressed);
                    inf.Inflate(uncompressed);
                    fs.Write(uncompressed, 0, uncompressed.Length);
                }
                fs.Close();
            }
        }

        private bool ArchiveOpen;
        private BinaryReader br;
        private bool Compressed;
        private bool ContainsFileNameBlobs;
        private BSAFileEntry[] Files;
        private ListViewItem[] lvItems;
        private ListViewItem[] lvAllItems;

        private enum BSASortOrder { FolderName, FileName, FileSize, Offset }
        private class BSASorter : System.Collections.IComparer {
            internal static BSASortOrder order=0;
            public int Compare(object a, object b) {
                BSAFileEntry fa=(BSAFileEntry)((ListViewItem)a).Tag;
                BSAFileEntry fb=(BSAFileEntry)((ListViewItem)b).Tag;
                switch(order) {
                case BSASortOrder.FolderName: return string.Compare(fa.LowerName,fb.LowerName);
                case BSASortOrder.FileName: return string.Compare(fa.FileName, fb.FileName);
                case BSASortOrder.FileSize:
                    if(fa.Size==fb.Size) return 0;
                    if(fa.Size>fb.Size) return -1; else return 1;
                case BSASortOrder.Offset:
                    if(fa.Offset==fb.Offset) return 0;
                    if(fa.Offset>fb.Offset) return -1; else return 1;
                default: return 0;
                }
            }
        }

        private void CloseArchive() {
            lvAllItems=null;
            tvFolders.Nodes.Clear();
            ArchiveOpen=false;
            bOpen.Text="Open";
            lvFiles.Items.Clear();
            Files=null;
            lvItems=null;
            bExtract.Enabled=false;
            bExtractAll.Enabled=false;
            bPreview.Enabled=false;
            if(br!=null) br.Close();
            br=null;
        }

        private void OpenArchive(string path) {
            try {
                br=new BinaryReader(File.OpenRead(path), System.Text.Encoding.Default);
                //if(Program.ReadCString(br)!="BSA") throw new fommException("File was not a valid BSA archive");
                uint type=br.ReadUInt32();
                System.Text.StringBuilder sb=new System.Text.StringBuilder(64);
                if(type!=0x00415342&&type!=0x00000100) {
                    //Might be a fallout 2 dat
                    br.BaseStream.Position=br.BaseStream.Length-8;
                    uint TreeSize=br.ReadUInt32();
                    uint DataSize=br.ReadUInt32();
                    if(DataSize!=br.BaseStream.Length) {
                        MessageBox.Show("File is not a valid bsa archive");
                        br.Close();
                        return;
                    }
                    br.BaseStream.Position=DataSize - TreeSize - 8;
                    int FileCount=br.ReadInt32();
                    Files=new BSAFileEntry[FileCount];
                    for(int i=0;i<FileCount;i++) {
                        int fileLen=br.ReadInt32();
                        for(int j=0;j<fileLen;j++) sb.Append(br.ReadChar());
                        byte comp=br.ReadByte();
                        uint realSize=br.ReadUInt32();
                        uint compSize=br.ReadUInt32();
                        uint offset=br.ReadUInt32();
                        if(sb[0]=='\\') sb.Remove(0,1);
                        Files[i]=new BSAFileEntry(sb.ToString(), offset, compSize, comp==0?0:realSize);
                        sb.Length=0;
                    }
                } else if(type==0x0100) {
                    uint hashoffset=br.ReadUInt32();
                    uint FileCount=br.ReadUInt32();
                    Files=new BSAFileEntry[FileCount];

                    uint dataoffset=12+hashoffset+FileCount*8;
                    uint fnameOffset1=12+FileCount*8;
                    uint fnameOffset2=12+FileCount*12;

                    for(int i=0;i<FileCount;i++) {
                        br.BaseStream.Position=12+i*8;
                        uint size=br.ReadUInt32();
                        uint offset=br.ReadUInt32()+dataoffset;
                        br.BaseStream.Position=fnameOffset1+i*4;
                        br.BaseStream.Position=br.ReadInt32()+fnameOffset2;

                        sb.Length=0;
                        while(true) {
                            char b=br.ReadChar();
                            if(b=='\0') break;
                            sb.Append(b);
                        }
                        Files[i]=new BSAFileEntry(sb.ToString(), offset, size);
                    }
                } else {
                    int version=br.ReadInt32();
                    if(version!=0x67&&version!=0x68) {
                        if(MessageBox.Show("This BSA archive has an unknown version number.\n"+
                    "Attempt to open anyway?", "Warning", MessageBoxButtons.YesNo)!=DialogResult.Yes) {
                            br.Close();
                            return;
                        }
                    }
                    br.BaseStream.Position+=4;
                    uint flags=br.ReadUInt32();
                    if((flags&0x004)>0) Compressed=true; else Compressed=false;
                    if((flags&0x100)>0&&version==0x68) ContainsFileNameBlobs=true; else ContainsFileNameBlobs=false;
                    int FolderCount=br.ReadInt32();
                    int FileCount=br.ReadInt32();
                    br.BaseStream.Position+=12;
                    Files=new BSAFileEntry[FileCount];
                    int[] numfiles=new int[FolderCount];
                    br.BaseStream.Position+=8;
                    for(int i=0;i<FolderCount;i++) {
                        numfiles[i]=br.ReadInt32();
                        br.BaseStream.Position+=12;
                    }
                    br.BaseStream.Position-=8;
                    int filecount=0;
                    for(int i=0;i<FolderCount;i++) {
                        int k=br.ReadByte();
                        while(--k>0) sb.Append(br.ReadChar());
                        br.BaseStream.Position++;
                        string folder=sb.ToString();
                        for(int j=0;j<numfiles[i];j++) {
                            br.BaseStream.Position+=8;
                            uint size=br.ReadUInt32();
                            bool comp=Compressed;
                            if((size&(1<<30))!=0) {
                                comp=!comp;
                                size^=1<<30;
                            }
                            Files[filecount++]=new BSAFileEntry(comp, folder, br.ReadUInt32(), size);
                        }
                        sb.Length=0;
                    }
                    for(int i=0;i<FileCount;i++) {
                        while(true) {
                            char c=br.ReadChar();
                            if(c=='\0') break;
                            sb.Append(c);
                        }
                        Files[i].FileName=sb.ToString();
                        sb.Length=0;
                    }
                }
            } catch(Exception ex) {
                if(br!=null) br.Close();
                br=null;
                MessageBox.Show("An error occured trying to open the archive.\n"+ex.Message);
                return;
            }

            tvFolders.Nodes.Add(Path.GetFileNameWithoutExtension(path));
            tvFolders.Nodes[0].Nodes.Add("empty");
            if(tvFolders.Nodes[0].IsExpanded) tvFolders.Nodes[0].Collapse();
            tbSearch.Text="";
            UpdateFileList();
            bOpen.Text="Close";
            bExtract.Enabled=true;
            ArchiveOpen=true;
            bExtractAll.Enabled=true;
            bPreview.Enabled=true;
        }

        private void UpdateFileList() {
            lvFiles.BeginUpdate();
            lvItems=new ListViewItem[Files.Length];
            for(int i=0;i<Files.Length;i++) {
                //ListViewItem lvi=new ListViewItem(Files[i].Folder+"\\"+Files[i].FileName);
                ListViewItem lvi=new ListViewItem(Path.Combine(Files[i].Folder, Files[i].FileName));
                lvi.Tag=Files[i];
                lvi.ToolTipText="File size: "+Files[i].Size+" bytes\nFile offset: "+Files[i].Offset+" bytes\n"+(Files[i].Compressed?"Compressed":"Uncompressed");
                lvItems[i]=lvi;
            }
            lvFiles.Items.AddRange(lvItems);
            lvFiles.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            lvFiles.EndUpdate();
        }

        private void bOpen_Click(object sender, EventArgs e) {
            if(ArchiveOpen) {
                CloseArchive();
            } else {
                if(OpenBSA.ShowDialog()==DialogResult.OK) OpenArchive(OpenBSA.FileName);
            }
        }

        private void bExtract_Click(object sender, EventArgs e) {
            if(lvFiles.SelectedItems.Count==0) return;
            if(lvFiles.SelectedItems.Count==1) {
                BSAFileEntry fe=(BSAFileEntry)lvFiles.SelectedItems[0].Tag;
                SaveSingleDialog.FileName=fe.FileName;
                if(SaveSingleDialog.ShowDialog()==DialogResult.OK) {
                    fe.Extract(SaveSingleDialog.FileName, false, br, ContainsFileNameBlobs);
                }
            } else {
                if(SaveAllDialog.ShowDialog()==DialogResult.OK) {
                    ProgressForm pf=new ProgressForm("Unpacking archive", false);
                    pf.EnableCancel();
                    pf.SetProgressRange(lvFiles.SelectedItems.Count);
                    pf.Show();
                    int count=0;
                    try {
                        foreach(ListViewItem lvi in lvFiles.SelectedItems) {
                            BSAFileEntry fe=(BSAFileEntry)lvi.Tag;
                            fe.Extract(SaveAllDialog.SelectedPath, true, br, ContainsFileNameBlobs);
                            pf.UpdateProgress(count++);
                            Application.DoEvents();
                        }
                    } catch(fommException) {
                        MessageBox.Show("Operation cancelled", "Message");
                    } catch(Exception ex) {
                        MessageBox.Show(ex.Message, "Error");
                    }
                    pf.Unblock();
                    pf.Close();
                }
            }
        }

        private void bExtractAll_Click(object sender, EventArgs e) {
            if(SaveAllDialog.ShowDialog()==DialogResult.OK) {
                ProgressForm pf=new ProgressForm("Unpacking archive", false);
                pf.EnableCancel();
                pf.SetProgressRange(Files.Length);
                pf.Show();
                int count=0;
                try {
                    foreach(BSAFileEntry fe in Files) {
                        fe.Extract(SaveAllDialog.SelectedPath, true, br, ContainsFileNameBlobs);
                        pf.UpdateProgress(count++);
                        Application.DoEvents();
                    }
                } catch(fommCancelException) {
                    MessageBox.Show("Operation cancelled", "Message");
                } catch(Exception ex) {
                    MessageBox.Show(ex.Message, "Error");
                }
                pf.Unblock();
                pf.Close();
            }
        }

        private void cmbSortOrder_SelectedIndexChanged(object sender, EventArgs e) {
            BSASorter.order=(BSASortOrder)cmbSortOrder.SelectedIndex;
        }

        private void cmbSortOrder_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled=true;
        }

        private void bSort_Click(object sender, EventArgs e) {
            lvFiles.ListViewItemSorter=new BSASorter();
            lvFiles.ListViewItemSorter=null;
        }

        private void BSABrowser_FormClosing(object sender, FormClosingEventArgs e) {
            if(ArchiveOpen) CloseArchive();
            Settings.SetWindowPosition("BSABrowser", this);
            Settings.SetString("LastBSAUnpackPath", SaveAllDialog.SelectedPath);
            Settings.SetString("BSABrowserPanelSplit", splitContainer1.SplitterDistance.ToString());
        }

        private void bPreview_Click(object sender, EventArgs e) {
            if(lvFiles.SelectedItems.Count==0) return;
            if(lvFiles.SelectedItems.Count==1) {
                BSAFileEntry fe=(BSAFileEntry)lvFiles.SelectedItems[0].Tag;
                switch(Path.GetExtension(fe.LowerName)) {
                /*case ".nif":
                    MessageBox.Show("Viewing of nif's disabled as their format differs from oblivion");
                    return;
                case ".dds":
                case ".tga":
                case ".bmp":
                case ".jpg":
                    System.Diagnostics.Process.Start("obmm\\NifViewer.exe", fe.LowerName);
                    break;*/
                case ".lst":
                case ".txt":
                case ".xml":
                    string path=Program.CreateTempDirectory();
                    fe.Extract(Path.Combine(path, fe.FileName), false, br, ContainsFileNameBlobs);
                    System.Diagnostics.Process.Start(Path.Combine(path, fe.FileName));
                    break;
                default:
                    MessageBox.Show("Filetype not supported.\n"+
                        "Currently only txt or xml files can be previewed","Error");
                    break;
                }
            } else {
               MessageBox.Show("Can only preview one file at a time", "Error");
            }
        }

        private void lvFiles_ItemDrag(object sender, ItemDragEventArgs e) {
            if(lvFiles.SelectedItems.Count!=1) return;
            BSAFileEntry fe=(BSAFileEntry)lvFiles.SelectedItems[0].Tag;
            string path=Path.Combine(Program.CreateTempDirectory(),fe.FileName);
            fe.Extract(path, false, br, ContainsFileNameBlobs);

            DataObject obj=new DataObject();
            System.Collections.Specialized.StringCollection sc=new System.Collections.Specialized.StringCollection();
            sc.Add(path);
            obj.SetFileDropList(sc);
            lvFiles.DoDragDrop(obj, DragDropEffects.Move);
        }

        private void tbSearch_TextChanged(object sender, EventArgs e) {
            if(!ArchiveOpen) return;
            string str=tbSearch.Text;
            if(cbRegex.Checked&&str.Length>0) {
                System.Text.RegularExpressions.Regex regex;
                try {
                    regex=new System.Text.RegularExpressions.Regex(str, System.Text.RegularExpressions.RegexOptions.Singleline);
                } catch { return; }
                lvFiles.BeginUpdate();
                lvFiles.Items.Clear();
                System.Collections.Generic.List<ListViewItem> lvis=new System.Collections.Generic.List<ListViewItem>(Files.Length);
                for(int i=0;i<lvItems.Length;i++) {
                    if(regex.IsMatch(lvItems[i].Text)) lvis.Add(lvItems[i]);
                }
                lvFiles.Items.AddRange(lvis.ToArray());
                lvFiles.EndUpdate();
            } else {
                str=str.ToLowerInvariant();
                lvFiles.BeginUpdate();
                lvFiles.Items.Clear();
                if(str.Length==0) {
                    lvFiles.Items.AddRange(lvItems);
                } else {
                    System.Collections.Generic.List<ListViewItem> lvis=new System.Collections.Generic.List<ListViewItem>(Files.Length);
                    for(int i=0;i<lvItems.Length;i++) {
                        if(lvItems[i].Text.Contains(str)) lvis.Add(lvItems[i]);
                    }
                    lvFiles.Items.AddRange(lvis.ToArray());
                }
                lvFiles.EndUpdate();
            }
        }

        private void tvFolders_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
            if(!ArchiveOpen||lvAllItems!=null) return;
            tvFolders.Nodes[0].Nodes.Clear();
            System.Collections.Generic.Dictionary<string, TreeNode> nodes=new System.Collections.Generic.Dictionary<string, TreeNode>();
            lvAllItems=(ListViewItem[])lvItems.Clone();
            foreach(ListViewItem lvi in lvAllItems) {
                string path=Path.GetDirectoryName(lvi.Text);
                if(path==string.Empty||nodes.ContainsKey(path)) continue;
                string[] dirs=path.Split('\\');
                for(int i=0;i<dirs.Length;i++) {
                    string newpath=string.Join("\\", dirs, 0, i+1);
                    if(!nodes.ContainsKey(newpath)) {
                        TreeNode tn=new TreeNode(dirs[i]);
                        tn.Tag=newpath;
                        if(i==0) {
                            tvFolders.Nodes[0].Nodes.Add(tn);
                        } else {
                            nodes[path].Nodes.Add(tn);
                        }
                        nodes.Add(newpath, tn);
                    }
                    path=newpath;
                }
            }
        }

        private void tvFolders_AfterSelect(object sender, TreeViewEventArgs e) {
            if(lvAllItems==null) return;
            string s=e.Node.Tag as string;
            if(s==null) {
                lvItems=lvAllItems;
            } else {
                System.Collections.Generic.List<ListViewItem> lvis=new System.Collections.Generic.List<ListViewItem>(lvAllItems.Length);
                foreach(ListViewItem lvi in lvAllItems) {
                    if(lvi.Text.StartsWith(s)) lvis.Add(lvi);
                }
                lvItems=lvis.ToArray();
            }
            tbSearch_TextChanged(null, null);
        }
    }
}
