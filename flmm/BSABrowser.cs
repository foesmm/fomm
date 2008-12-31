/* This file is part of Oblivion Mod Manager.
 * 
 * Oblivion Mod Manager is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Oblivion Mod Manager is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Windows.Forms;

namespace fomm {
    internal partial class BSABrowser : Form {
        internal BSABrowser() {
            InitializeComponent();
        }

        internal BSABrowser(string BSAPath) {
            InitializeComponent();
            Hidden=false;
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
                    lowername=Folder.ToLower()+"\\"+fileName.ToLower();
                }
            }
            internal string LowerName {
                get { return lowername; }
            }
            internal readonly string Folder;
            internal readonly uint Offset;
            internal readonly uint Size;

            internal BSAFileEntry(bool compressed, string folder, uint offset,uint size) {
                Compressed=compressed;
                Folder=folder;
                Offset=offset;
                Size=size;
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
                    byte[] uncompressed=new byte[br.ReadUInt32()];
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

        private bool ArchiveOpen=false;
        private BinaryReader br;
        private bool Compressed;
        private bool ContainsFileNameBlobs;
        private BSAFileEntry[] Files;
        private int FolderCount;
        private int FileCount;
        private bool Hidden=true;

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
            ArchiveOpen=false;
            bOpen.Text="Open";
            if(!Hidden) lvFiles.Items.Clear();
            Files=null;
            bExtract.Enabled=false;
            bExtractAll.Enabled=false;
            bPreview.Enabled=false;
            if(br!=null) br.Close();
            br=null;
        }

        private string ReadString(int len) {
            string s="";
            for(int i=0;i<len;i++) s+=(char)br.ReadByte();
            return s;
        }

        private void OpenArchive(string path) {
            try {
                br=new BinaryReader(File.OpenRead(path));
                if(Program.ReadCString(br)!="BSA") throw new fommException("File was not a valid BSA archive");
                if(br.ReadUInt32()!=104) {
                    if(MessageBox.Show("This BSA archive has an unknown version number.\n"+
                    "Attempt to open anyway?", "Warning", MessageBoxButtons.YesNo)!=DialogResult.Yes) {
                        br.Close();
                        return;
                    }
                }
                br.ReadUInt32();
                uint flags=br.ReadUInt32();
                if((flags&0x004)>0) Compressed=true; else Compressed=false;
                if((flags&0x100)>0) ContainsFileNameBlobs=true; else ContainsFileNameBlobs=false;
                FolderCount=br.ReadInt32();
                FileCount=br.ReadInt32();
                br.BaseStream.Position+=12;
                //br.BaseStream.Position+=8;
                //uint testtest=br.ReadUInt32();
                Files=new BSAFileEntry[FileCount];
                int[] numfiles=new int[FolderCount];
                for(int i=0;i<FolderCount;i++) {
                    br.BaseStream.Position+=8;
                    numfiles[i]=br.ReadInt32();
                    br.ReadUInt32();
                }
                int filecount=0;
                for(int i=0;i<FolderCount;i++) {
                    string folder=ReadString(br.ReadByte()-1);

                    br.ReadByte();
                    for(int j=0;j<numfiles[i];j++) {
                        br.BaseStream.Position+=8;

                        uint size=br.ReadUInt32();
                        bool comp=Compressed;
                        if((size&(1<<30))>0) {
                            comp=!comp;
                            size^=1<<30;
                        }
                        Files[filecount++]=new BSAFileEntry(comp, folder, br.ReadUInt32(), size);
                    }
                }
                for(int i=0;i<FileCount;i++) {
                    string s="";
                    while(true) {
                        char c=br.ReadChar();
                        if(c=='\0') break;
                        s+=c;
                    }
                    Files[i].FileName=s;
                }
                
            } catch {
                if(br!=null) br.Close();
                br=null;
                return;
            }

            UpdateFileList();
            bOpen.Text="Close";
            bExtract.Enabled=true;
            ArchiveOpen=true;
            bExtractAll.Enabled=true;
            bPreview.Enabled=true;
        }

        private void UpdateFileList() {
            if(Hidden) return;
            lvFiles.SuspendLayout();
            lvFiles.Items.Clear();
            foreach(BSAFileEntry fe in Files) {
                ListViewItem lvi=new ListViewItem(fe.Folder+"\\"+fe.FileName);
                lvi.Tag=fe;
                string text="File size: "+fe.Size+" bytes\nFile offset: "+fe.Offset+" bytes\n";
                if(fe.Compressed) text+="Compressed"; else text+="Uncompressed";
                lvi.ToolTipText=text;
                lvFiles.Items.Add(lvi);
            }
            lvFiles.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            lvFiles.ResumeLayout();
        }

        private void bOpen_Click(object sender, EventArgs e) {
            if(ArchiveOpen) {
                CloseArchive();
            } else {
                if(OpenBSA.ShowDialog()==DialogResult.OK) {
                    try {
                        OpenArchive(OpenBSA.FileName);
                    } catch (Exception ex) {
                        MessageBox.Show("Unable to open archive\n"+ex.Message, "Error");
                        ArchiveOpen=false;
                        return;
                    }
                }
            }
        }

        private bool ExtractFileToData(string file) {
            file=file.ToLower();
            foreach(BSAFileEntry fe in Files) {
                if(fe.LowerName==file) {
                    fe.Extract("Data", true, br, ContainsFileNameBlobs);
                    return true;
                }
            }
            return false;
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
                    pf.EnableCancel("Operation cancelled");
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
                pf.EnableCancel("Operation cancelled");
                pf.SetProgressRange(FileCount);
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

        private void BSABrowser_Shown(object sender, EventArgs e) {
            Hidden=false;
        }

        private void BSABrowser_FormClosing(object sender, FormClosingEventArgs e) {
            if(ArchiveOpen) CloseArchive();
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
    }
}
