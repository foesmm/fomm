using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using fomm.TESsnip;

namespace fomm {
    public partial class MainForm : Form {

        public MainForm() {
            InitializeComponent();
            Settings.GetWindowPosition("MainForm", this);
            
            Text+=" ("+Program.Version+")";
        }

        private void MainForm_Load(object sender, EventArgs e) {
            RefreshEspList();
        }

        private void lvEspList_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
            System.Drawing.Point p=lvEspList.PointToClient(Form.MousePosition);
            ListViewItem lvi=lvEspList.GetItemAt(p.X, p.Y);
            lvEspList.SelectedIndices.Clear();
            if(lvi!=null) lvi.Selected=true;
        }

        private void lvEspList_DragDrop(object sender, DragEventArgs e) {
            if(lvEspList.SelectedIndices.Count!=1) return;
            int[] toswap=(int[])e.Data.GetData(typeof(int[]));
            if(toswap==null) return;
            int swapwith=lvEspList.SelectedIndices[0];
            if(toswap[0]==swapwith) return;

            RefreshingList=true;

            string[] names=new string[toswap.Length];
            Array.Sort<int>(toswap);
            for(int i=0;i<toswap.Length;i++) names[i]=lvEspList.Items[toswap[i]].Text;
            for(int i=toswap.Length-1;i>=0;i--) {
                if(toswap[i]<swapwith) swapwith--;
                lvEspList.Items.RemoveAt(toswap[i]);
            }
            DateTime time=File.GetLastWriteTime("data\\"+lvEspList.Items[swapwith].Text) + TimeSpan.FromMinutes(2);
            for(int i=0;i<toswap.Length;i++) {
                File.SetLastWriteTime("data\\"+names[i], time);
                lvEspList.Items.Insert(swapwith+i+1, new ListViewItem(names[i]));
                time+=TimeSpan.FromMinutes(2);
            }
            int index=swapwith+toswap.Length;
            while(index<lvEspList.Items.Count&&File.GetLastWriteTime("data\\"+lvEspList.Items[index].Text)<time) {
                File.SetLastWriteTime("data\\"+lvEspList.Items[index++].Text, time);
                time+=TimeSpan.FromMinutes(2);
            }

            RefreshEspList();
        }

        private bool DragDropInProgress=false;
        private void lvEspList_ItemDrag(object sender, ItemDragEventArgs e) {
            if(lvEspList.SelectedIndices.Count==0||e.Button!=MouseButtons.Left) return;
            //if(EspListSorter.order!=EspSortOrder.LoadOrder) return;
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
            MessageBox.Show("Warning: This tool hasn't been fully updated for fallout yet. BSAs created with it may not be able to be read.", "Warning");
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
            if(File.Exists("fose_loader.exe")) System.Diagnostics.Process.Start("fose_loader.exe");
            else if(File.Exists("fallout3.exe")) System.Diagnostics.Process.Start("fallout3.exe");
            else System.Diagnostics.Process.Start("fallout3ng.exe");
        }

        private void bSaveGames_Click(object sender, EventArgs e) {
            (new SaveForm()).Show();
        }

        private void bHelp_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start(Path.Combine(Program.fommDir, "fomm.chm"));
        }
        #endregion

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
    }
}