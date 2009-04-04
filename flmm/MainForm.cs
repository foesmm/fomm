using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using Fomm.TESsnip;

namespace Fomm {
    partial class MainForm : Form {

        public MainForm(string fomod) {
            InitializeComponent();
            Settings.GetWindowPosition("MainForm", this);
            
            Text+=" ("+Program.Version+")";

            if(fomod!=null) {
                bPackageManager_Click(null, null);
                if(fomod.Length>0) PackageManagerForm.AddNewFomod(fomod);
            }

            if(Settings.GetString("LaunchCommand")==null&&File.Exists("fose_loader.exe")) bLaunch.Text="Launch FOSE";

            if(!Settings.GetBool("DisableIPC")) {
                Timer newFommTimer=new Timer();
                newFommTimer.Interval=1000;
                newFommTimer.Tick+=new EventHandler(newFommTimer_Tick);
                newFommTimer.Start();
                Messaging.ServerSetup(RecieveMessage);
            }
        }

        private void MainForm_Load(object sender, EventArgs e) {
            string tmp=Settings.GetString("MainFormPanelSplit");
            if(tmp!=null) {
                try {
                    splitContainer1.SplitterDistance=Math.Max(splitContainer1.Panel1MinSize+1, Math.Min(splitContainer1.Height-(splitContainer1.Panel2MinSize+1), int.Parse(tmp)));
                } catch { }
            }
            tmp=Settings.GetString("MainFormCol1Width");
            if(tmp!=null) lvEspList.Columns[0].Width=int.Parse(tmp);
            tmp=Settings.GetString("MainFormCol2Width");
            if(tmp!=null) lvEspList.Columns[1].Width=int.Parse(tmp);
            RefreshEspList();
        }

        private void lvEspList_DragDrop(object sender, DragEventArgs e) {
            DateTime timestamp=new DateTime(2008, 1, 1);
            TimeSpan twomins=TimeSpan.FromMinutes(2);

            for(int i=0;i<lvEspList.Items.Count;i++) {
                File.SetLastWriteTime(Path.Combine("data", lvEspList.Items[i].Text), timestamp);
                timestamp+=twomins;
            }
            RefreshIndexCounts();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(Application.OpenForms.Count>1) {
                MessageBox.Show("Please close all utility windows before closing fomm");
                e.Cancel=true;
                return;
            }

            Settings.SetWindowPosition("MainForm", this);
            Settings.SetString("MainFormPanelSplit", splitContainer1.SplitterDistance.ToString());
            Settings.SetString("MainFormCol1Width", lvEspList.Columns[0].Width.ToString());
            Settings.SetString("MainFormCol2Width", lvEspList.Columns[1].Width.ToString());
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
            string desc2=string.Empty;
            if((Path.GetExtension(lvEspList.SelectedItems[0].Text).CompareTo(".esp")==0)!=((((Record)p.Records[0]).Flags1&1)==0)) {
                if((((Record)p.Records[0]).Flags1&1)==0) {
                    desc2+="WARNING: This plugin has the file extension .esm, but its file header marks it as an esp!"+Environment.NewLine+Environment.NewLine;
                } else {
                    desc2+="WARNING: This plugin has the file extension .esp, but its file header marks it as an esm!"+Environment.NewLine+Environment.NewLine;
                }
            }
            desc2+=lvEspList.SelectedItems[0].Text+Environment.NewLine+(name==null?"":("Author: "+name+Environment.NewLine))+
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
                PackageManagerForm=new Fomm.PackageManager.PackageManager(this);
                PackageManagerForm.FormClosed+=delegate(object sender2, FormClosedEventArgs e2)
                {
                    RefreshEspList();
                    PackageManagerForm=null;
                };
                PackageManagerForm.Show();
            }
        }
        private void RefreshIndexCounts() {
            if(lvEspList.Items.Count==0) return;
            bool add=lvEspList.Items[0].SubItems.Count==1;
            bool boldify=false;

            if(add) {
                boldify=Settings.GetBool("ShowEsmInBold");
            }

            if(File.Exists(Program.PluginsFile)) {
                string[] lines=File.ReadAllLines(Program.PluginsFile);
                List<Pair<FileInfo, bool>> files=new List<Pair<FileInfo, bool>>(lines.Length);
                char[] invalidChars=Path.GetInvalidFileNameChars();
                for(int i=0;i<lines.Length;i++) {
                    lines[i]=lines[i].Trim();
                    if(lines[i].Length==0||lines[i][0]=='#'||lines[i].IndexOfAny(invalidChars)!=-1) continue;
                    string path=Path.Combine("Data", lines[i]);
                    if(!File.Exists(path)) continue;
                    files.Add(new Pair<FileInfo, bool>(new FileInfo(path), TESsnip.Plugin.GetIsEsm(path)));
                }
                files.Sort(delegate(Pair<FileInfo, bool> a, Pair<FileInfo, bool> b) {
                    if(a.b==b.b) return a.a.LastWriteTime.CompareTo(b.a.LastWriteTime);
                    else return a.b?-1:1;
                });
                if(lines.Length!=files.Count) lines=new string[files.Count];
                for(int i=0;i<lines.Length;i++) lines[i]=files[i].a.Name.Trim().ToLowerInvariant();
                foreach(ListViewItem lvi in lvEspList.Items) {
                    int i=Array.IndexOf<string>(lines, lvi.Text.ToLowerInvariant());
                    if(i!=-1) {
                        if(add) {
                            if(boldify&&files[i].b) lvi.Font=new System.Drawing.Font(lvi.Font, System.Drawing.FontStyle.Bold);
                            lvi.Checked=true;
                            lvi.SubItems.Add(i.ToString("X2"));
                        } else lvi.SubItems[1].Text=i.ToString("X2");
                    } else {
                        if(add) {
                            if(boldify&&TESsnip.Plugin.GetIsEsm(Path.Combine("Data", lvi.Text))) lvi.Font=new System.Drawing.Font(lvi.Font, System.Drawing.FontStyle.Bold);
                            lvi.SubItems.Add("NA");
                        } else lvi.SubItems[1].Text="NA";
                    }
                }
            } else {
                foreach(ListViewItem lvi in lvEspList.Items) {
                    if(add) lvi.SubItems.Add("NA");
                    else lvi.SubItems[1].Text="NA";
                }
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

            lvEspList.Items.AddRange(plugins.ToArray());
            RefreshIndexCounts();
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
            for(int i=0;i<mods.Length;i++) mods[i]=Path.Combine("data", lvEspList.SelectedItems[i].Text);
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
            string command=Settings.GetString("LaunchCommand");
            if(command==null) {
                if(File.Exists("fose_loader.exe")) command="fose_loader.exe";
                else if(File.Exists("fallout3.exe")) command="fallout3.exe";
                else command="fallout3ng.exe";
            }
            try {
                if(System.Diagnostics.Process.Start(command)==null) {
                    MessageBox.Show("Failed to launch '"+command+"'");
                    return;
                }
            } catch(Exception ex) {
                MessageBox.Show("Failed to launch '"+command+"'\n"+ex.Message);
                return;
            }
            Close();
        }

        private void bSaveGames_Click(object sender, EventArgs e) {
            (new SaveForm()).Show();
        }

        private void bHelp_Click(object sender, EventArgs e) {
            //System.Diagnostics.Process.Start(Path.Combine(Program.fommDir, "fomm.chm"));
            System.Diagnostics.Process.Start(@"http://fomm.wiki.sourceforge.net/");
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
            RefreshIndexCounts();
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
            File.WriteAllLines(Program.PluginsFile, plugins, System.Text.Encoding.Default);
            RefreshIndexCounts();
        }

        private void sendToTopToolStripMenuItem_Click(object sender, EventArgs e) {
            if(lvEspList.SelectedIndices.Count==0) return;
            int[] toswap=new int[lvEspList.SelectedIndices.Count];
            for(int i=0;i<lvEspList.SelectedIndices.Count;i++) toswap[i]=lvEspList.SelectedIndices[i];
            Array.Sort<int>(toswap);
            CommitLoadOrder(0, toswap);
        }

        private void sendToBottomToolStripMenuItem_Click(object sender, EventArgs e) {
            if(lvEspList.SelectedIndices.Count==0) return;
            int[] toswap=new int[lvEspList.SelectedIndices.Count];
            for(int i=0;i<lvEspList.SelectedIndices.Count;i++) toswap[i]=lvEspList.SelectedIndices[i];
            Array.Sort<int>(toswap);
            CommitLoadOrder(lvEspList.Items.Count, toswap);
        }

        private void copyLoadOrderToClipboardToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Text.StringBuilder sb=new System.Text.StringBuilder();
            ListViewItem[] lvis=new ListViewItem[lvEspList.CheckedItems.Count];
            for(int i=0;i<lvEspList.CheckedItems.Count;i++) lvis[i]=lvEspList.CheckedItems[i];
            Array.Sort<ListViewItem>(lvis, delegate(ListViewItem a, ListViewItem b)
            {
                return int.Parse(a.SubItems[1].Text, System.Globalization.NumberStyles.AllowHexSpecifier).CompareTo(int.Parse(b.SubItems[1].Text, System.Globalization.NumberStyles.AllowHexSpecifier));
            });
            for(int i=0;i<lvis.Length;i++) sb.AppendLine(lvis[i].Text);
            sb.AppendLine();
            sb.AppendLine("Total active plugins: "+lvEspList.CheckedItems.Count);
            sb.AppendLine("Total plugins: "+lvEspList.Items.Count);
            Clipboard.SetText(sb.ToString());
        }

        private volatile string newFommMessage;

        private void newFommTimer_Tick(object sender, EventArgs e) {
            string tmp=newFommMessage;
            if(tmp==null) return;
            newFommMessage=null;
            if(PackageManagerForm==null) bPackageManager_Click(null, null);
            PackageManagerForm.AddNewFomod(tmp);
        }

        private void RecieveMessage(string msg) { newFommMessage=msg; }

        private void lvEspList_KeyDown(object sender, KeyEventArgs e) {
            if(e.Alt&&(e.KeyCode==Keys.Up||e.KeyCode==Keys.Down)) {
                e.Handled=true;
                if(lvEspList.SelectedItems.Count>0) {
                    int[] indicies=new int[lvEspList.SelectedIndices.Count];
                    for(int i=0;i<indicies.Length;i++) indicies[i]=lvEspList.SelectedIndices[i];
                    Array.Sort<int>(indicies);
                    if(e.KeyCode==Keys.Up) {
                        if(indicies[0]>0) {
                            CommitLoadOrder(indicies[0]-1, indicies);
                        }
                    } else {
                        if(indicies[indicies.Length-1]<lvEspList.Items.Count-1) {
                            CommitLoadOrder(indicies[indicies.Length-1]+2, indicies);
                        }
                    }
                }
            } else if(e.KeyCode==Keys.Delete) {
                deleteToolStripMenuItem_Click(null, null);
                e.Handled=true;
            }
        }

        private void exportLoadOrderToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveFileDialog ofd=new SaveFileDialog();
            ofd.Filter="Text file (*.txt)|*.txt";
            ofd.AddExtension=true;
            ofd.RestoreDirectory=true;
            if(ofd.ShowDialog()!=DialogResult.OK) return;
            StreamWriter sw=new StreamWriter(ofd.FileName);
            for(int i=0;i<lvEspList.Items.Count;i++) {
                sw.WriteLine("["+(lvEspList.Items[i].Checked?"X":" ")+"] "+lvEspList.Items[i].Text);
            }
            sw.Close();
        }

        private void importLoadOrderToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog ofd=new OpenFileDialog();
            ofd.Filter="Text file (*.txt)|*.txt";
            ofd.AddExtension=true;
            ofd.RestoreDirectory=true;
            if(ofd.ShowDialog()!=DialogResult.OK) return;
            string[] lines=File.ReadAllLines(ofd.FileName);
            List<string> active=new List<string>();
            for(int i=0;i<lines.Length;i++) {
                if(lines[i].Length<5||lines[i][0]!='['||lines[i][2]!=']'||lines[i][3]!=' ') {
                    MessageBox.Show("File does not appear to be an exported load order list", "Error");
                    return;
                }
                bool bactive=lines[i][1]=='X';
                lines[i]=lines[i].Substring(4).ToLowerInvariant();
                if(bactive) active.Add(lines[i]);
            }

            string[] order=new string[lvEspList.Items.Count];
            int upto=0;
            for(int i=0;i<lines.Length;i++) {
                if(File.Exists(Path.Combine("data", lines[i]))) order[upto++]=lines[i];
            }
            for(int i=0;i<lvEspList.Items.Count;i++) {
                if(Array.IndexOf<string>(order, lvEspList.Items[i].Text.ToLowerInvariant())==-1) order[upto++]=lvEspList.Items[i].Text;
            }
            DateTime timestamp=new DateTime(2008, 1, 1);
            TimeSpan twomins=TimeSpan.FromMinutes(2);
            for(int i=0;i<order.Length;i++) {
                File.SetLastWriteTime(Path.Combine("data\\", order[i]), timestamp);
                timestamp+=twomins;
            }

            RefreshEspList();

            RefreshingList=true;
            for(int i=0;i<lvEspList.Items.Count;i++) lvEspList.Items[i].Checked=active.Contains(lvEspList.Items[i].Text.ToLowerInvariant());
            RefreshingList=false;
            lvEspList_ItemChecked(null, null);
        }

        private void uncheckAllToolStripMenuItem_Click(object sender, EventArgs e) {
            RefreshingList=true;
            for(int i=0;i<lvEspList.Items.Count;i++) lvEspList.Items[i].Checked=false;
            RefreshingList=false;
            lvEspList_ItemChecked(null, null);
        }

        private void checkAllToolStripMenuItem_Click(object sender, EventArgs e) {
            RefreshingList=true;
            for(int i=0;i<lvEspList.Items.Count;i++) lvEspList.Items[i].Checked=true;
            RefreshingList=false;
            lvEspList_ItemChecked(null, null);
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            if(pictureBox1.Image!=null&&(pictureBox1.Image.Size.Width>pictureBox1.Width||pictureBox1.Image.Size.Height>pictureBox1.Height)) {
                (new ImageForm(pictureBox1.Image)).ShowDialog();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e) {
            if(lvEspList.SelectedIndices.Count==0) return;
            ListViewItem[] files=new ListViewItem[lvEspList.SelectedItems.Count];
            for(int i=0;i<lvEspList.SelectedItems.Count;i++) {
                files[i]=lvEspList.SelectedItems[i];
                if(files[i].Text.Equals("fallout3.esm", StringComparison.OrdinalIgnoreCase)) {
                    MessageBox.Show("Cannot delete Fallout3.esm", "Error");
                    return;
                }
            }
            if(MessageBox.Show("Are you sure you want to delete the selected plugins?", "Warning", MessageBoxButtons.YesNo)!=DialogResult.Yes) return;
            lvEspList.SelectedItems.Clear();
            for(int i=0;i<files.Length;i++) {
                File.Delete(Path.Combine("data", files[i].Text));
                lvEspList.Items.Remove(files[i]);
            }
            RefreshIndexCounts();
        }

        private void bInstallTweaker_Click(object sender, EventArgs e) {
            if(PackageManagerForm!=null) {
                MessageBox.Show("Please close the package manager before running the install tweaker");
                return;
            }
            (new InstallTweaker.InstallationTweaker()).ShowDialog();
        }

        private void bSettings_Click(object sender, EventArgs e) {
            if(Application.OpenForms.Count>1) {
                MessageBox.Show("Please close all utility windows before changing the settings");
                return;
            }
            (new SetupForm(true)).ShowDialog();
            RefreshEspList();
        }
    }
}