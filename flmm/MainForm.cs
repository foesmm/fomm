using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using fomm.TESsnip;

namespace fomm {
    public partial class MainForm : Form {

        public MainForm(string fomod) {
            InitializeComponent();
            Settings.GetWindowPosition("MainForm", this);
            
            Text+=" ("+Program.Version+")";

            if(fomod!=null) {
                bPackageManager_Click(null, null);
                PackageManagerForm.AddNewFomod(fomod);
            }
        }

        private void MainForm_Load(object sender, EventArgs e) {
            RefreshEspList();
        }

        private int DragDropIndex=-1;
        private void lvEspList_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
            System.Drawing.Point p=lvEspList.PointToClient(Form.MousePosition);
            ListViewItem lvi=lvEspList.GetItemAt(p.X, p.Y);
            if(lvi==null) {
                DragDropIndex=-1;
                return;
            }
            int newDragDropIndex=lvi.Index;
            System.Drawing.Rectangle itemBounds = lvEspList.GetItemRect(newDragDropIndex);
            if(p.Y > itemBounds.Top + (itemBounds.Height / 2)) {
                newDragDropIndex++;
            }
            if(DragDropIndex==newDragDropIndex) return;
            DragDropIndex=newDragDropIndex;
            lvEspList.SelectedIndices.Clear();
            if(DragDropIndex!=-1) {
                if(DragDropIndex!=lvEspList.Items.Count) lvEspList.SelectedIndices.Add(DragDropIndex);
                if(DragDropIndex!=0) lvEspList.SelectedIndices.Add(DragDropIndex-1);
            }
        }

        private void lvEspList_DragDrop(object sender, DragEventArgs e) {
            if(DragDropIndex==-1) return;
            int[] toswap=(int[])e.Data.GetData(typeof(int[]));
            if(toswap==null) return;
            CommitLoadOrder(DragDropIndex, toswap);
            DragDropIndex=-1;
        }

        private bool DragDropInProgress=false;
        private void lvEspList_ItemDrag(object sender, ItemDragEventArgs e) {
            if(lvEspList.SelectedIndices.Count==0||e.Button!=MouseButtons.Left) return;
            DragDropInProgress=true;
            int[] indicies=new int[lvEspList.SelectedIndices.Count];
            for(int i=0;i<indicies.Length;i++) indicies[i]=lvEspList.SelectedIndices[i];
            lvEspList.DoDragDrop(indicies, DragDropEffects.Move);

        }

        private void lvEspList_DragEnter(object sender, DragEventArgs e) {
            if(!DragDropInProgress) return;
            e.Effect=DragDropEffects.Move;
            DragDropInProgress=false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(Application.OpenForms.Count>1) {
                MessageBox.Show("Please close all utility windows before closing fomm");
                e.Cancel=true;
                return;
            }
            
            Settings.SetWindowPosition("MainForm", this);
        }

        private void lvEspList_SelectedIndexChanged(object sender, EventArgs e) {
            if(lvEspList.SelectedItems.Count!=1) return;
            Plugin p;
            try {
                p=new Plugin(Path.Combine("data", lvEspList.SelectedItems[0].Text), true);
            } catch {
                p=null;
            }
            if(p==null||p.Records.Count==0||p.Records[0].Name!="TES4") {
                tbPluginInfo.Text=lvEspList.SelectedItems[0].Text+Environment.NewLine+"Warning: Plugin appears corrupt";
                pictureBox1.Image=null;
            }
            string name=null;
            string desc=null;
            byte[] pic=null;
            List<string> masters=new List<string>();
            foreach(SubRecord sr in ((Record)p.Records[0]).SubRecords) {
                switch(sr.Name) {
                case "CNAM":
                    name=sr.GetStrData();
                    break;
                case "SNAM":
                    desc=sr.GetStrData();
                    break;
                case "MAST":
                    masters.Add(sr.GetStrData());
                    break;
                case "SCRN":
                    pic=sr.GetData();
                    break;
                }
            }
            if(pic!=null) {
                pictureBox1.Image=System.Drawing.Bitmap.FromStream(new MemoryStream(pic));
            } else pictureBox1.Image=null;
            string desc2=lvEspList.SelectedItems[0].Text+Environment.NewLine+(name==null?"":("Author: "+name+Environment.NewLine))+
                Environment.NewLine+(desc==null?"":("Description:"+Environment.NewLine+desc+Environment.NewLine+Environment.NewLine));
            if(masters.Count>0) {
                desc2+="Masters:"+Environment.NewLine;
                for(int i=0;i<masters.Count;i++) desc2+=masters[i];
            }
            tbPluginInfo.Text=desc2;
        }

        private PackageManager.PackageManager PackageManagerForm;
        private void bPackageManager_Click(object sender, EventArgs e) {
            if(PackageManagerForm!=null) PackageManagerForm.Focus();
            else {
                PackageManagerForm=new fomm.PackageManager.PackageManager(this);
                PackageManagerForm.FormClosed+=delegate(object sender2, FormClosedEventArgs e2)
                {
                    RefreshEspList();
                    PackageManagerForm=null;
                };
                PackageManagerForm.Show();
            }
        }
        public void RefreshEspList() {
            RefreshingList=true;
            lvEspList.BeginUpdate();
            lvEspList.Items.Clear();

            List<ListViewItem> plugins=new List<ListViewItem>();
            DirectoryInfo di=new DirectoryInfo("data");
            List<FileInfo> files=new List<FileInfo>(di.GetFiles("*.esp"));
            files.AddRange(di.GetFiles("*.esm"));
            int count=0;
            for(int i=0;i<files.Count;i++) {
                try {
                    count+=files[i].LastWriteTime.Hour;
                } catch(ArgumentOutOfRangeException) {
                    MessageBox.Show("File '"+files[i].Name+"' had an invalid time stamp, and has been reset.\n"+
                        "Please check its position in the load order.", "Warning");
                    files[i].LastWriteTime=DateTime.Now;
                }
            }
            files.Sort(delegate(FileInfo a, FileInfo b)
            {
                return a.LastWriteTime.CompareTo(b.LastWriteTime);
            });
            foreach(FileInfo fi in files) {
                plugins.Add(new ListViewItem(fi.Name));
            }

            int icount=0;
            if(File.Exists(Program.PluginsFile)) {
                string[] lines=File.ReadAllLines(Program.PluginsFile);
                for(int i=0;i<lines.Length;i++) lines[i]=lines[i].Trim().ToLowerInvariant();
                Array.Sort<string>(lines);
                foreach(ListViewItem lvi in plugins) {
                    if(Array.IndexOf<string>(lines, lvi.Text.ToLowerInvariant())!=-1) {
                        lvi.Checked=true;
                        lvi.SubItems.Add(icount.ToString("X2"));
                        icount++;
                    } else {
                        lvi.SubItems.Add("NA");
                    }
                }
            } else {
                foreach(ListViewItem lvi in plugins) {
                    lvi.SubItems.Add("NA");
                }
            }

            lvEspList.Items.AddRange(plugins.ToArray());
            lvEspList.EndUpdate();
            RefreshingList=false;
        }

        private void bEnableAI_Click(object sender, EventArgs e) {
            ArchiveInvalidation.Update();
        }

        #region toolbuttons
        private void openInTESsnipToolStripMenuItem_Click(object sender, EventArgs e) {
            if(lvEspList.SelectedItems.Count==0) return;
            string[] mods=new string[lvEspList.SelectedItems.Count];
            for(int i=0;i<mods.Length;i++) mods[i]="data\\"+lvEspList.SelectedItems[i].Text;
            TESsnip.TESsnip tes=new TESsnip.TESsnip(mods);
            tes.FormClosed+=delegate(object sender2, FormClosedEventArgs e2)
                {
                    RefreshEspList();
                };
            tes.Show();
            GC.Collect();
        }

        private void bBSAUnpack_Click(object sender, EventArgs e) {
            new BSABrowser().Show();
        }

        private void cBSACreator_Click(object sender, EventArgs e) {
            new BSACreator().Show();
        }

        private void bTESsnip_Click(object sender, EventArgs e) {
            TESsnip.TESsnip tes=new TESsnip.TESsnip();
            tes.FormClosed+=delegate(object sender2, FormClosedEventArgs e2)
                {
                    RefreshEspList();
                };
            tes.Show();
            GC.Collect();
        }

        private void bShaderEdit_Click(object sender, EventArgs e) {
            new ShaderEdit.MainForm().Show();
        }

        private void bLaunch_Click(object sender, EventArgs e) {
            if(Application.OpenForms.Count>1) {
                MessageBox.Show("Please close all utility windows before launching fallout");
                return;
            }
            Close();
            string command=Settings.GetString("LaunchCommand");
            if(command!=null) {
                System.Diagnostics.Process.Start(command);
            } else {
                if(File.Exists("fose_loader.exe")) System.Diagnostics.Process.Start("fose_loader.exe");
                else if(File.Exists("fallout3.exe")) System.Diagnostics.Process.Start("fallout3.exe");
                else System.Diagnostics.Process.Start("fallout3ng.exe");
            }
        }

        private void bSaveGames_Click(object sender, EventArgs e) {
            (new SaveForm()).Show();
        }

        private void bHelp_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start(Path.Combine(Program.fommDir, "fomm.chm"));
        }
        #endregion

        private void CommitLoadOrder(int position, int[] indicies) {
            Array.Sort<int>(indicies);
            DateTime timestamp=new DateTime(2008, 1, 1);
            TimeSpan twomins=TimeSpan.FromMinutes(2);
            List<ListViewItem> items=new List<ListViewItem>();
            RefreshingList=true;
            lvEspList.BeginUpdate();
            for(int i=0;i<position;i++) {
                if(Array.BinarySearch<int>(indicies, i)>=0) continue;
                File.SetLastWriteTime(Path.Combine("data\\",lvEspList.Items[i].Text), timestamp);
                timestamp+=twomins;
                items.Add(lvEspList.Items[i]);
                items[items.Count-1].Selected=false;
            }
            for(int i=0;i<indicies.Length;i++) {
                File.SetLastWriteTime(Path.Combine("data\\",lvEspList.Items[indicies[i]].Text), timestamp);
                timestamp+=twomins;
                items.Add(lvEspList.Items[indicies[i]]);
                items[items.Count-1].Selected=true;
            }
            for(int i=position;i<lvEspList.Items.Count;i++) {
                if(Array.BinarySearch<int>(indicies, i)>=0) continue;
                File.SetLastWriteTime(Path.Combine("data\\",lvEspList.Items[i].Text), timestamp);
                timestamp+=twomins;
                items.Add(lvEspList.Items[i]);
                items[items.Count-1].Selected=false;
            }
            lvEspList.Items.Clear();
            lvEspList.Items.AddRange(items.ToArray());
            int count=0;
            for(int i=0;i<lvEspList.Items.Count;i++) {
                if(lvEspList.Items[i].Checked) {
                    lvEspList.Items[i].SubItems[1].Text=count.ToString("X2");
                    count++;
                } else lvEspList.Items[i].SubItems[1].Text="NA";
            }
            lvEspList.EndUpdate();
            RefreshingList=false;
            lvEspList.EnsureVisible(position==lvEspList.Items.Count?position-1:position);
        }

        private bool RefreshingList;
        private void lvEspList_ItemChecked(object sender, ItemCheckedEventArgs e) {
            if(RefreshingList) return;
            string[] plugins=new string[lvEspList.CheckedItems.Count];
            for(int i=0;i<plugins.Length;i++) {
                plugins[i]=lvEspList.CheckedItems[i].Text;
            }
            File.WriteAllLines(Program.PluginsFile, plugins);
            int icount=0;
            foreach(ListViewItem lvi in lvEspList.Items) {
                if(lvi.Checked) {
                    lvi.SubItems[1].Text=icount.ToString("X2");
                    icount++;
                } else {
                    lvi.SubItems[1].Text="NA";
                }
            }

        }

        private void sendToTopToolStripMenuItem_Click(object sender, EventArgs e) {
            if(lvEspList.SelectedIndices.Count==0) return;
            int[] toswap=new int[lvEspList.SelectedIndices.Count];
            for(int i=0;i<lvEspList.SelectedIndices.Count;i++) toswap[i]=lvEspList.SelectedIndices[i];
            CommitLoadOrder(0, toswap);
        }

        private void sendToBottomToolStripMenuItem_Click(object sender, EventArgs e) {
            if(lvEspList.SelectedIndices.Count==0) return;
            int[] toswap=new int[lvEspList.SelectedIndices.Count];
            for(int i=0;i<lvEspList.SelectedIndices.Count;i++) toswap[i]=lvEspList.SelectedIndices[i];
            CommitLoadOrder(lvEspList.Items.Count, toswap);
        }

        private void copyLoadOrderToClipboardToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Text.StringBuilder sb=new System.Text.StringBuilder();
            for(int i=0;i<lvEspList.CheckedItems.Count;i++) sb.AppendLine(lvEspList.CheckedItems[i].Text);
            sb.AppendLine();
            sb.AppendLine("Total active plugins: "+lvEspList.CheckedItems.Count);
            sb.AppendLine("Total plugins: "+lvEspList.Items.Count);
            Clipboard.SetText(sb.ToString());
        }
    }
}