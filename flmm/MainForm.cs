using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using fomm.TESsnip;

namespace fomm {
    public partial class MainForm : Form {
        private readonly string PluginsFile;

        public MainForm() {
            InitializeComponent();
            Settings.GetWindowPosition("MainForm", this);
            PluginsFile=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fallout3\\plugins.txt");
            Text+=" ("+Program.Version+")";
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
            tes.FormClosed+=pm_FormClosed;
            tes.Show();
            GC.Collect();
            //GC.WaitForPendingFinalizers();
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

        private void MainForm_Load(object sender, EventArgs e) {
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
            files.Sort(delegate(FileInfo a, FileInfo b) {
                return a.LastWriteTime.CompareTo(b.LastWriteTime);
            });
            foreach(FileInfo fi in files) {
                plugins.Add(new ListViewItem(fi.Name));
            }

            if(File.Exists(PluginsFile)) {
                string[] lines=File.ReadAllLines(PluginsFile);
                for(int i=0;i<lines.Length;i++) lines[i]=lines[i].Trim().ToLowerInvariant();
                Array.Sort<string>(lines);
                foreach(ListViewItem lvi in plugins) {
                    if(Array.IndexOf<string>(lines, lvi.Text.ToLowerInvariant())!=-1) lvi.Checked=true;
                }
            }

            lvEspList.Items.AddRange(plugins.ToArray());
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
            List<string> plugins=new List<string>();
            foreach(ListViewItem lvi in lvEspList.CheckedItems) {
                plugins.Add(lvi.Text);
            }
            File.WriteAllLines(PluginsFile, plugins.ToArray());
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
                PackageManagerForm=new fomm.PackageManager.PackageManager();
                PackageManagerForm.FormClosed+=new FormClosedEventHandler(pm_FormClosed);
                PackageManagerForm.Show();
            }
        }

        void pm_FormClosed(object sender, FormClosedEventArgs e) {
            MainForm_Load(null, null);
            PackageManagerForm=null;
        }

        private void bEnableAI_Click(object sender, EventArgs e) {
            ArchiveInvalidation.Update();
        }

        private void bSaveGames_Click(object sender, EventArgs e) {
            (new SaveForm()).Show();
        }

        private void openInTESsnipToolStripMenuItem_Click(object sender, EventArgs e) {
            if(lvEspList.SelectedItems.Count==0) return;
            string[] mods=new string[lvEspList.SelectedItems.Count];
            for(int i=0;i<mods.Length;i++) mods[i]="data\\"+lvEspList.SelectedItems[i].Text;
            TESsnip.TESsnip tes=new TESsnip.TESsnip(mods);
            tes.FormClosed+=pm_FormClosed;
            tes.Show();
            GC.Collect();
            //GC.WaitForPendingFinalizers();
        }

        private void bHelp_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start(Path.Combine(Program.fommDir, "fomm.chm"));
        }
    }
}