namespace fomm {
    partial class BSABrowser {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BSABrowser));
            this.lvFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.bOpen = new System.Windows.Forms.Button();
            this.bExtractAll = new System.Windows.Forms.Button();
            this.OpenBSA = new System.Windows.Forms.OpenFileDialog();
            this.bExtract = new System.Windows.Forms.Button();
            this.SaveAllDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.SaveSingleDialog = new System.Windows.Forms.SaveFileDialog();
            this.cmbSortOrder = new System.Windows.Forms.ComboBox();
            this.DudMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.bSort = new System.Windows.Forms.Button();
            this.bPreview = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lvFiles
            // 
            this.lvFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvFiles.AutoArrange = false;
            this.lvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvFiles.Location = new System.Drawing.Point(12, 12);
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.ShowItemToolTips = true;
            this.lvFiles.Size = new System.Drawing.Size(558, 312);
            this.lvFiles.TabIndex = 0;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = System.Windows.Forms.View.Details;
            this.lvFiles.ItemActivate += new System.EventHandler(this.bExtract_Click);
            this.lvFiles.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.lvFiles_ItemDrag);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "File path";
            this.columnHeader1.Width = 375;
            // 
            // bOpen
            // 
            this.bOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bOpen.Location = new System.Drawing.Point(12, 330);
            this.bOpen.Name = "bOpen";
            this.bOpen.Size = new System.Drawing.Size(75, 23);
            this.bOpen.TabIndex = 1;
            this.bOpen.Text = "Open";
            this.bOpen.UseVisualStyleBackColor = true;
            this.bOpen.Click += new System.EventHandler(this.bOpen_Click);
            // 
            // bExtractAll
            // 
            this.bExtractAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bExtractAll.Enabled = false;
            this.bExtractAll.Location = new System.Drawing.Point(273, 330);
            this.bExtractAll.Name = "bExtractAll";
            this.bExtractAll.Size = new System.Drawing.Size(75, 23);
            this.bExtractAll.TabIndex = 4;
            this.bExtractAll.Text = "Extract all";
            this.bExtractAll.UseVisualStyleBackColor = true;
            this.bExtractAll.Click += new System.EventHandler(this.bExtractAll_Click);
            // 
            // OpenBSA
            // 
            this.OpenBSA.Filter = "Fallout or Oblivion BSA archives|*.bsa";
            this.OpenBSA.RestoreDirectory = true;
            this.OpenBSA.Title = "Select archive to open";
            // 
            // bExtract
            // 
            this.bExtract.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bExtract.Enabled = false;
            this.bExtract.Location = new System.Drawing.Point(192, 330);
            this.bExtract.Name = "bExtract";
            this.bExtract.Size = new System.Drawing.Size(75, 23);
            this.bExtract.TabIndex = 3;
            this.bExtract.Text = "Extract";
            this.bExtract.UseVisualStyleBackColor = true;
            this.bExtract.Click += new System.EventHandler(this.bExtract_Click);
            // 
            // SaveAllDialog
            // 
            this.SaveAllDialog.Description = "Select folder to unpack archive to";
            // 
            // SaveSingleDialog
            // 
            this.SaveSingleDialog.Filter = "All files|*.*";
            this.SaveSingleDialog.RestoreDirectory = true;
            this.SaveSingleDialog.Title = "Save to";
            // 
            // cmbSortOrder
            // 
            this.cmbSortOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbSortOrder.ContextMenuStrip = this.DudMenu;
            this.cmbSortOrder.FormattingEnabled = true;
            this.cmbSortOrder.Items.AddRange(new object[] {
            "Folder name",
            "File name",
            "File size",
            "Offset"});
            this.cmbSortOrder.Location = new System.Drawing.Point(368, 332);
            this.cmbSortOrder.Name = "cmbSortOrder";
            this.cmbSortOrder.Size = new System.Drawing.Size(121, 21);
            this.cmbSortOrder.TabIndex = 5;
            this.cmbSortOrder.Text = "Folder name";
            this.cmbSortOrder.SelectedIndexChanged += new System.EventHandler(this.cmbSortOrder_SelectedIndexChanged);
            this.cmbSortOrder.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmbSortOrder_KeyPress);
            // 
            // DudMenu
            // 
            this.DudMenu.Name = "DudMenu";
            this.DudMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // bSort
            // 
            this.bSort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bSort.Location = new System.Drawing.Point(495, 330);
            this.bSort.Name = "bSort";
            this.bSort.Size = new System.Drawing.Size(75, 23);
            this.bSort.TabIndex = 6;
            this.bSort.Text = "Sort";
            this.bSort.UseVisualStyleBackColor = true;
            this.bSort.Click += new System.EventHandler(this.bSort_Click);
            // 
            // bPreview
            // 
            this.bPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bPreview.Enabled = false;
            this.bPreview.Location = new System.Drawing.Point(111, 330);
            this.bPreview.Name = "bPreview";
            this.bPreview.Size = new System.Drawing.Size(75, 23);
            this.bPreview.TabIndex = 2;
            this.bPreview.Text = "Preview";
            this.bPreview.UseVisualStyleBackColor = true;
            this.bPreview.Click += new System.EventHandler(this.bPreview_Click);
            // 
            // BSABrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 365);
            this.Controls.Add(this.bPreview);
            this.Controls.Add(this.bExtract);
            this.Controls.Add(this.bExtractAll);
            this.Controls.Add(this.bOpen);
            this.Controls.Add(this.cmbSortOrder);
            this.Controls.Add(this.bSort);
            this.Controls.Add(this.lvFiles);
            this.MinimumSize = new System.Drawing.Size(590, 150);
            this.Name = "BSABrowser";
            this.Text = "BSA Browser";
            this.Shown += new System.EventHandler(this.BSABrowser_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BSABrowser_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvFiles;
        private System.Windows.Forms.Button bOpen;
        private System.Windows.Forms.Button bExtractAll;
        private System.Windows.Forms.OpenFileDialog OpenBSA;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button bExtract;
        private System.Windows.Forms.FolderBrowserDialog SaveAllDialog;
        private System.Windows.Forms.SaveFileDialog SaveSingleDialog;
        private System.Windows.Forms.ComboBox cmbSortOrder;
        private System.Windows.Forms.ContextMenuStrip DudMenu;
        private System.Windows.Forms.Button bSort;
        private System.Windows.Forms.Button bPreview;
    }
}
