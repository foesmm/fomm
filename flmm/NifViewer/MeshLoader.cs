using System;
using System.Windows.Forms;
using System.IO;

namespace fomm.NifViewer {
    public partial class MeshLoader : Form {
        public string SelectedMesh;

        public MeshLoader() {
            InitializeComponent();
            listBox1.Items.AddRange(BSAArchive.MeshList);
        }

        private void bCancel_Click(object sender, EventArgs e) {
            DialogResult=DialogResult.Cancel;
            Close();
        }

        private void bLoad_Click(object sender, EventArgs e) {
            if(listBox1.SelectedItems.Count!=1) return;
            SelectedMesh=(string)listBox1.SelectedItem+".nif";
            DialogResult=DialogResult.OK;
            Close();
        }
    }
}