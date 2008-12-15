using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace fomm {
    public partial class UtilitiesOnlyForm : Form {
        public UtilitiesOnlyForm() {
            InitializeComponent();
        }

        private void bBSAUnpack_Click(object sender, EventArgs e) {
            new BSABrowser().Show();
        }

        private void cBSACreator_Click(object sender, EventArgs e) {
            MessageBox.Show("Warning: This tool hasn't been fully updated for fallout yet. BSAs created with it may not be able to be read.", "Warning");
            new BSACreator().Show();
        }

        private void bTESsnip_Click(object sender, EventArgs e) {
            new TESsnip.TESsnip().Show();
            GC.Collect();
        }

        private void bShaderEdit_Click(object sender, EventArgs e) {
            new ShaderEdit.MainForm().Show();
        }

        private void UtilitiesOnlyForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(Application.OpenForms.Count>1) {
                MessageBox.Show("Please close all utility windows before closing fomm");
                e.Cancel=true;
            }
        }
    }
}