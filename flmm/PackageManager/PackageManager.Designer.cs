namespace fomm.PackageManager {
    partial class PackageManager {
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lvModList = new System.Windows.Forms.ListView();
            this.fomodContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.visitWebsiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emailAuthorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fomodStatusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tbModInfo = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.bEditScript = new System.Windows.Forms.Button();
            this.bEditReadme = new System.Windows.Forms.Button();
            this.bEditInfo = new System.Windows.Forms.Button();
            this.bActivate = new System.Windows.Forms.Button();
            this.bAddNew = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.cbGroups = new System.Windows.Forms.CheckBox();
            this.bEditGroups = new System.Windows.Forms.Button();
            this.bActivateGroup = new System.Windows.Forms.Button();
            this.bDeactivateGroup = new System.Windows.Forms.Button();
            this.bDeactivateAll = new System.Windows.Forms.Button();
            this.cmbSortOrder = new System.Windows.Forms.ComboBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.fomodContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lvModList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tbModInfo);
            this.splitContainer1.Size = new System.Drawing.Size(453, 453);
            this.splitContainer1.SplitterDistance = 276;
            this.splitContainer1.TabIndex = 18;
            // 
            // lvModList
            // 
            this.lvModList.AutoArrange = false;
            this.lvModList.CheckBoxes = true;
            this.lvModList.ContextMenuStrip = this.fomodContextMenu;
            this.lvModList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvModList.FullRowSelect = true;
            this.lvModList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvModList.HideSelection = false;
            this.lvModList.Location = new System.Drawing.Point(0, 0);
            this.lvModList.MultiSelect = false;
            this.lvModList.Name = "lvModList";
            this.lvModList.Size = new System.Drawing.Size(453, 276);
            this.lvModList.TabIndex = 0;
            this.lvModList.UseCompatibleStateImageBehavior = false;
            this.lvModList.View = System.Windows.Forms.View.Details;
            this.lvModList.SelectedIndexChanged += new System.EventHandler(this.lvModList_SelectedIndexChanged);
            this.lvModList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvModList_ItemCheck);
            // 
            // fomodContextMenu
            // 
            this.fomodContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.visitWebsiteToolStripMenuItem,
            this.emailAuthorToolStripMenuItem,
            this.fomodStatusToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.fomodContextMenu.Name = "fomodContextMenu";
            this.fomodContextMenu.Size = new System.Drawing.Size(140, 92);
            this.fomodContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.fomodContextMenu_Opening);
            // 
            // visitWebsiteToolStripMenuItem
            // 
            this.visitWebsiteToolStripMenuItem.Name = "visitWebsiteToolStripMenuItem";
            this.visitWebsiteToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.visitWebsiteToolStripMenuItem.Text = "Visit website";
            this.visitWebsiteToolStripMenuItem.Click += new System.EventHandler(this.visitWebsiteToolStripMenuItem_Click);
            // 
            // emailAuthorToolStripMenuItem
            // 
            this.emailAuthorToolStripMenuItem.Name = "emailAuthorToolStripMenuItem";
            this.emailAuthorToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.emailAuthorToolStripMenuItem.Text = "email author";
            this.emailAuthorToolStripMenuItem.Click += new System.EventHandler(this.emailAuthorToolStripMenuItem_Click);
            // 
            // fomodStatusToolStripMenuItem
            // 
            this.fomodStatusToolStripMenuItem.Name = "fomodStatusToolStripMenuItem";
            this.fomodStatusToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.fomodStatusToolStripMenuItem.Text = "Fomod status";
            this.fomodStatusToolStripMenuItem.Click += new System.EventHandler(this.fomodStatusToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // tbModInfo
            // 
            this.tbModInfo.BackColor = System.Drawing.SystemColors.Window;
            this.tbModInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbModInfo.Location = new System.Drawing.Point(0, 0);
            this.tbModInfo.Multiline = true;
            this.tbModInfo.Name = "tbModInfo";
            this.tbModInfo.ReadOnly = true;
            this.tbModInfo.Size = new System.Drawing.Size(453, 173);
            this.tbModInfo.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Location = new System.Drawing.Point(471, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(120, 90);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 16;
            this.pictureBox1.TabStop = false;
            // 
            // bEditScript
            // 
            this.bEditScript.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bEditScript.Location = new System.Drawing.Point(471, 308);
            this.bEditScript.Name = "bEditScript";
            this.bEditScript.Size = new System.Drawing.Size(120, 23);
            this.bEditScript.TabIndex = 20;
            this.bEditScript.Text = "Edit script";
            this.bEditScript.UseVisualStyleBackColor = true;
            this.bEditScript.Click += new System.EventHandler(this.bEditScript_Click);
            // 
            // bEditReadme
            // 
            this.bEditReadme.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bEditReadme.Location = new System.Drawing.Point(471, 279);
            this.bEditReadme.Name = "bEditReadme";
            this.bEditReadme.Size = new System.Drawing.Size(120, 23);
            this.bEditReadme.TabIndex = 21;
            this.bEditReadme.Text = "View readme";
            this.bEditReadme.UseVisualStyleBackColor = true;
            this.bEditReadme.Click += new System.EventHandler(this.bEditReadme_Click);
            // 
            // bEditInfo
            // 
            this.bEditInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bEditInfo.Location = new System.Drawing.Point(471, 337);
            this.bEditInfo.Name = "bEditInfo";
            this.bEditInfo.Size = new System.Drawing.Size(120, 23);
            this.bEditInfo.TabIndex = 22;
            this.bEditInfo.Text = "Edit info";
            this.bEditInfo.UseVisualStyleBackColor = true;
            this.bEditInfo.Click += new System.EventHandler(this.bEditInfo_Click);
            // 
            // bActivate
            // 
            this.bActivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bActivate.Location = new System.Drawing.Point(471, 148);
            this.bActivate.Name = "bActivate";
            this.bActivate.Size = new System.Drawing.Size(120, 23);
            this.bActivate.TabIndex = 23;
            this.bActivate.Text = "Activate";
            this.bActivate.UseVisualStyleBackColor = true;
            this.bActivate.Click += new System.EventHandler(this.bActivate_Click);
            // 
            // bAddNew
            // 
            this.bAddNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bAddNew.Location = new System.Drawing.Point(471, 108);
            this.bAddNew.Name = "bAddNew";
            this.bAddNew.Size = new System.Drawing.Size(120, 23);
            this.bAddNew.TabIndex = 24;
            this.bAddNew.Text = "Add new";
            this.bAddNew.UseVisualStyleBackColor = true;
            this.bAddNew.Click += new System.EventHandler(this.bAddNew_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Compressed archives (fomod, zip, rar, 7z)|*.fomod;*.zip;*.rar;*.7z";
            this.openFileDialog1.RestoreDirectory = true;
            // 
            // cbGroups
            // 
            this.cbGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbGroups.AutoSize = true;
            this.cbGroups.Location = new System.Drawing.Point(471, 410);
            this.cbGroups.Name = "cbGroups";
            this.cbGroups.Size = new System.Drawing.Size(97, 17);
            this.cbGroups.TabIndex = 25;
            this.cbGroups.Text = "Display Groups";
            this.cbGroups.UseVisualStyleBackColor = true;
            this.cbGroups.CheckedChanged += new System.EventHandler(this.cbGroups_CheckedChanged);
            // 
            // bEditGroups
            // 
            this.bEditGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bEditGroups.Location = new System.Drawing.Point(471, 381);
            this.bEditGroups.Name = "bEditGroups";
            this.bEditGroups.Size = new System.Drawing.Size(120, 23);
            this.bEditGroups.TabIndex = 26;
            this.bEditGroups.Text = "Edit groups";
            this.bEditGroups.UseVisualStyleBackColor = true;
            this.bEditGroups.Click += new System.EventHandler(this.bEditGroups_Click);
            // 
            // bActivateGroup
            // 
            this.bActivateGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bActivateGroup.Enabled = false;
            this.bActivateGroup.Location = new System.Drawing.Point(471, 177);
            this.bActivateGroup.Name = "bActivateGroup";
            this.bActivateGroup.Size = new System.Drawing.Size(120, 23);
            this.bActivateGroup.TabIndex = 27;
            this.bActivateGroup.Text = "Activate group";
            this.bActivateGroup.UseVisualStyleBackColor = true;
            this.bActivateGroup.Click += new System.EventHandler(this.bActivateGroup_Click);
            // 
            // bDeactivateGroup
            // 
            this.bDeactivateGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bDeactivateGroup.Enabled = false;
            this.bDeactivateGroup.Location = new System.Drawing.Point(471, 206);
            this.bDeactivateGroup.Name = "bDeactivateGroup";
            this.bDeactivateGroup.Size = new System.Drawing.Size(120, 23);
            this.bDeactivateGroup.TabIndex = 28;
            this.bDeactivateGroup.Text = "Deactivate group";
            this.bDeactivateGroup.UseVisualStyleBackColor = true;
            this.bDeactivateGroup.Click += new System.EventHandler(this.bDeactivateGroup_Click);
            // 
            // bDeactivateAll
            // 
            this.bDeactivateAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bDeactivateAll.Location = new System.Drawing.Point(471, 235);
            this.bDeactivateAll.Name = "bDeactivateAll";
            this.bDeactivateAll.Size = new System.Drawing.Size(120, 23);
            this.bDeactivateAll.TabIndex = 29;
            this.bDeactivateAll.Text = "Deactivate all";
            this.bDeactivateAll.UseVisualStyleBackColor = true;
            this.bDeactivateAll.Click += new System.EventHandler(this.bDeactivateAll_Click);
            // 
            // cmbSortOrder
            // 
            this.cmbSortOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbSortOrder.FormattingEnabled = true;
            this.cmbSortOrder.Items.AddRange(new object[] {
            "File name",
            "Mod name",
            "Author"});
            this.cmbSortOrder.Location = new System.Drawing.Point(471, 433);
            this.cmbSortOrder.Name = "cmbSortOrder";
            this.cmbSortOrder.Size = new System.Drawing.Size(120, 21);
            this.cmbSortOrder.TabIndex = 30;
            this.cmbSortOrder.Text = "Sort order";
            this.cmbSortOrder.SelectedIndexChanged += new System.EventHandler(this.cmbSortOrder_SelectedIndexChanged);
            this.cmbSortOrder.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmbSortOrder_KeyPress);
            // 
            // PackageManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 477);
            this.Controls.Add(this.cmbSortOrder);
            this.Controls.Add(this.bDeactivateAll);
            this.Controls.Add(this.bDeactivateGroup);
            this.Controls.Add(this.bActivateGroup);
            this.Controls.Add(this.bAddNew);
            this.Controls.Add(this.bActivate);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.bEditGroups);
            this.Controls.Add(this.cbGroups);
            this.Controls.Add(this.bEditReadme);
            this.Controls.Add(this.bEditInfo);
            this.Controls.Add(this.bEditScript);
            this.Name = "PackageManager";
            this.Text = "PackageManager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PackageManager_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.fomodContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView lvModList;
        private System.Windows.Forms.TextBox tbModInfo;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button bEditScript;
        private System.Windows.Forms.Button bEditReadme;
        private System.Windows.Forms.Button bEditInfo;
        private System.Windows.Forms.Button bActivate;
        private System.Windows.Forms.Button bAddNew;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ContextMenuStrip fomodContextMenu;
        private System.Windows.Forms.ToolStripMenuItem visitWebsiteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem emailAuthorToolStripMenuItem;
        private System.Windows.Forms.CheckBox cbGroups;
        private System.Windows.Forms.Button bEditGroups;
        private System.Windows.Forms.ToolStripMenuItem fomodStatusToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Button bActivateGroup;
        private System.Windows.Forms.Button bDeactivateGroup;
        private System.Windows.Forms.Button bDeactivateAll;
        private System.Windows.Forms.ComboBox cmbSortOrder;
    }
}