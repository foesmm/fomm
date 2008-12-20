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
            GC.WaitForPendingFinalizers();
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
            List<ListViewItem> plugins=new List<ListViewItem>();
            DirectoryInfo di=new DirectoryInfo("data");
            List<FileInfo> files=new List<FileInfo>(di.GetFiles("*.esp"));
            files.AddRange(di.GetFiles("*.esm"));
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

        private bool SwitchEsps(int index) {
            ListViewItem bottom=lvEspList.Items[index];
            ListViewItem top=lvEspList.Items[index+1];
            DateTime oldtime=File.GetLastWriteTime("data\\"+bottom.Text);
            File.SetLastWriteTime("data\\"+bottom.Text, File.GetLastWriteTime("data\\"+top.Text));
            File.SetLastWriteTime("data\\"+top.Text, oldtime);
            
            lvEspList.Items.RemoveAt(index);
            lvEspList.Items.Insert(index+1, bottom);
            return true;
        }

        private void lvEspList_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
            System.Drawing.Point p=lvEspList.PointToClient(Form.MousePosition);
            ListViewItem lvi=lvEspList.GetItemAt(p.X, p.Y);
            if(lvi==null) lvEspList.SelectedIndices.Clear();
            else lvi.Selected=true;
        }

        private void lvEspList_DragDrop(object sender, DragEventArgs e) {
            if(lvEspList.SelectedIndices.Count!=1) return;
            int toswap=(int)e.Data.GetData(typeof(int)) - 1;
            if(toswap==-1) return;
            int swapwith=lvEspList.SelectedIndices[0];
            if(toswap==swapwith) return;
            if(swapwith>toswap) {
                for(int i=0;i<swapwith-toswap;i++) {
                    if(!SwitchEsps(toswap+i)) break;
                }
            } else {
                for(int i=0;i<toswap-swapwith;i++) {
                    if(!SwitchEsps(toswap-(i+1))) break;
                }
            }
        }

        private bool DragDropInProgress=false;
        private void lvEspList_ItemDrag(object sender, ItemDragEventArgs e) {
            if(lvEspList.SelectedIndices.Count!=1||e.Button!=MouseButtons.Left) return;
            //if(EspListSorter.order!=EspSortOrder.LoadOrder) return;
            DragDropInProgress=true;
            lvEspList.DoDragDrop(lvEspList.SelectedIndices[0]+1, DragDropEffects.Move);
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
    }
}