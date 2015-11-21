namespace Fomm.Games.Fallout3.Tools.ShaderEdit {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.tbEdit = new System.Windows.Forms.TextBox();
            this.bSave = new System.Windows.Forms.Button();
            this.bOpen = new System.Windows.Forms.Button();
            this.cmbShaderSelect = new System.Windows.Forms.ComboBox();
            this.DudMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.bCompile = new System.Windows.Forms.Button();
            this.bClose = new System.Windows.Forms.Button();
            this.bImport = new System.Windows.Forms.Button();
            this.ImportMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.importHLSLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importBinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportBinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportBinaryAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.ImportMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbEdit
            // 
            this.tbEdit.AcceptsReturn = true;
            this.tbEdit.AcceptsTab = true;
            this.tbEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbEdit.Enabled = false;
            this.tbEdit.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbEdit.Location = new System.Drawing.Point(12, 12);
            this.tbEdit.Multiline = true;
            this.tbEdit.Name = "tbEdit";
            this.tbEdit.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbEdit.Size = new System.Drawing.Size(580, 350);
            this.tbEdit.TabIndex = 0;
            this.tbEdit.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.tbEdit_PreviewKeyDown);
            this.tbEdit.ModifiedChanged += new System.EventHandler(this.tbEdit_ModifiedChanged);
            // 
            // bSave
            // 
            this.bSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bSave.Enabled = false;
            this.bSave.Location = new System.Drawing.Point(355, 368);
            this.bSave.Name = "bSave";
            this.bSave.Size = new System.Drawing.Size(75, 23);
            this.bSave.TabIndex = 1;
            this.bSave.Text = "Save";
            this.bSave.UseVisualStyleBackColor = true;
            this.bSave.Click += new System.EventHandler(this.bSave_Click);
            // 
            // bOpen
            // 
            this.bOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bOpen.Location = new System.Drawing.Point(517, 368);
            this.bOpen.Name = "bOpen";
            this.bOpen.Size = new System.Drawing.Size(75, 23);
            this.bOpen.TabIndex = 2;
            this.bOpen.Text = "Open";
            this.bOpen.UseVisualStyleBackColor = true;
            this.bOpen.Click += new System.EventHandler(this.bOpen_Click);
            // 
            // cmbShaderSelect
            // 
            this.cmbShaderSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbShaderSelect.ContextMenuStrip = this.DudMenu;
            this.cmbShaderSelect.Enabled = false;
            this.cmbShaderSelect.FormattingEnabled = true;
            this.cmbShaderSelect.Location = new System.Drawing.Point(12, 370);
            this.cmbShaderSelect.MaxDropDownItems = 20;
            this.cmbShaderSelect.Name = "cmbShaderSelect";
            this.cmbShaderSelect.Size = new System.Drawing.Size(155, 21);
            this.cmbShaderSelect.TabIndex = 3;
            this.cmbShaderSelect.SelectedIndexChanged += new System.EventHandler(this.cmbShaderSelect_SelectedIndexChanged);
            this.cmbShaderSelect.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmbShaderSelect_KeyPress);
            // 
            // DudMenu
            // 
            this.DudMenu.Name = "DudMenu";
            this.DudMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.RestoreDirectory = true;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.RestoreDirectory = true;
            // 
            // bCompile
            // 
            this.bCompile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCompile.Enabled = false;
            this.bCompile.Location = new System.Drawing.Point(173, 368);
            this.bCompile.Name = "bCompile";
            this.bCompile.Size = new System.Drawing.Size(75, 23);
            this.bCompile.TabIndex = 4;
            this.bCompile.Text = "Compile";
            this.bCompile.UseVisualStyleBackColor = true;
            this.bCompile.Click += new System.EventHandler(this.bCompile_Click);
            // 
            // bClose
            // 
            this.bClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bClose.Enabled = false;
            this.bClose.Location = new System.Drawing.Point(436, 368);
            this.bClose.Name = "bClose";
            this.bClose.Size = new System.Drawing.Size(75, 23);
            this.bClose.TabIndex = 5;
            this.bClose.Text = "Close";
            this.bClose.UseVisualStyleBackColor = true;
            this.bClose.Click += new System.EventHandler(this.bClose_Click);
            // 
            // bImport
            // 
            this.bImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bImport.Enabled = false;
            this.bImport.Location = new System.Drawing.Point(254, 368);
            this.bImport.Name = "bImport";
            this.bImport.Size = new System.Drawing.Size(83, 23);
            this.bImport.TabIndex = 6;
            this.bImport.Text = "Import/Export";
            this.bImport.UseVisualStyleBackColor = true;
            this.bImport.Click += new System.EventHandler(this.bImport_Click);
            // 
            // ImportMenu
            // 
            this.ImportMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importHLSLToolStripMenuItem,
            this.importBinaryToolStripMenuItem,
            this.exportBinaryToolStripMenuItem,
            this.exportBinaryAllToolStripMenuItem});
            this.ImportMenu.Name = "ImportMenu";
            this.ImportMenu.Size = new System.Drawing.Size(153, 92);
            // 
            // importHLSLToolStripMenuItem
            // 
            this.importHLSLToolStripMenuItem.Name = "importHLSLToolStripMenuItem";
            this.importHLSLToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.importHLSLToolStripMenuItem.Text = "Import HLSL";
            this.importHLSLToolStripMenuItem.Click += new System.EventHandler(this.importHLSLToolStripMenuItem_Click);
            // 
            // importBinaryToolStripMenuItem
            // 
            this.importBinaryToolStripMenuItem.Name = "importBinaryToolStripMenuItem";
            this.importBinaryToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.importBinaryToolStripMenuItem.Text = "Import binary";
            this.importBinaryToolStripMenuItem.Click += new System.EventHandler(this.importBinaryToolStripMenuItem_Click);
            // 
            // exportBinaryToolStripMenuItem
            // 
            this.exportBinaryToolStripMenuItem.Name = "exportBinaryToolStripMenuItem";
            this.exportBinaryToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exportBinaryToolStripMenuItem.Text = "Export binary";
            this.exportBinaryToolStripMenuItem.Click += new System.EventHandler(this.exportBinaryToolStripMenuItem_Click);
            // 
            // exportBinaryAllToolStripMenuItem
            // 
            this.exportBinaryAllToolStripMenuItem.Name = "exportBinaryAllToolStripMenuItem";
            this.exportBinaryAllToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exportBinaryAllToolStripMenuItem.Text = "Export binary all";
            this.exportBinaryAllToolStripMenuItem.Click += new System.EventHandler(this.exportBinaryAllToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 403);
            this.Controls.Add(this.cmbShaderSelect);
            this.Controls.Add(this.bClose);
            this.Controls.Add(this.bImport);
            this.Controls.Add(this.tbEdit);
            this.Controls.Add(this.bCompile);
            this.Controls.Add(this.bOpen);
            this.Controls.Add(this.bSave);
            this.Name = "MainForm";
            this.Text = "SDP editor";
            this.ImportMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbEdit;
        private System.Windows.Forms.Button bSave;
        private System.Windows.Forms.Button bOpen;
        private System.Windows.Forms.ComboBox cmbShaderSelect;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button bCompile;
        private System.Windows.Forms.Button bClose;
        private System.Windows.Forms.ContextMenuStrip DudMenu;
        private System.Windows.Forms.Button bImport;
        private System.Windows.Forms.ContextMenuStrip ImportMenu;
        private System.Windows.Forms.ToolStripMenuItem importHLSLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importBinaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportBinaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportBinaryAllToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}

