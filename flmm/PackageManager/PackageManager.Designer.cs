using System.Security.AccessControl;

namespace Fomm.PackageManager {
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
      this.butViewReadme = new System.Windows.Forms.Button();
      this.bEditInfo = new System.Windows.Forms.Button();
      this.bActivate = new System.Windows.Forms.Button();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.cbGroups = new System.Windows.Forms.CheckBox();
      this.bEditGroups = new System.Windows.Forms.Button();
      this.bActivateGroup = new System.Windows.Forms.Button();
      this.bDeactivateGroup = new System.Windows.Forms.Button();
      this.bDeactivateAll = new System.Windows.Forms.Button();
      this.cmbSortOrder = new System.Windows.Forms.ComboBox();
      this.butDeactivate = new System.Windows.Forms.Button();
      this.butExtractFomod = new System.Windows.Forms.Button();
      this.fbdExtractFomod = new System.Windows.Forms.FolderBrowserDialog();
      this.cmsAddFomod = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.addFOMODToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.addPFPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.createFromFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.createFOMODToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.editPFPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.sbtAddFomod = new Fomm.Controls.SplitButton();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.modListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.exportModListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.exportActiveModListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.fomodContextMenu.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.cmsAddFomod.SuspendLayout();
      this.menuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer1.Location = new System.Drawing.Point(12, 27);
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
      this.splitContainer1.Size = new System.Drawing.Size(475, 560);
      this.splitContainer1.SplitterDistance = 388;
      this.splitContainer1.TabIndex = 18;
      // 
      // lvModList
      // 
      this.lvModList.AutoArrange = false;
      this.lvModList.CheckBoxes = true;
      this.lvModList.ContextMenuStrip = this.fomodContextMenu;
      this.lvModList.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lvModList.FullRowSelect = true;
      this.lvModList.HideSelection = false;
      this.lvModList.Location = new System.Drawing.Point(0, 0);
      this.lvModList.MultiSelect = false;
      this.lvModList.Name = "lvModList";
      this.lvModList.Size = new System.Drawing.Size(475, 388);
      this.lvModList.TabIndex = 0;
      this.lvModList.UseCompatibleStateImageBehavior = false;
      this.lvModList.View = System.Windows.Forms.View.Details;
      this.lvModList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvModList_ColumnClick);
      this.lvModList.ItemActivate += new System.EventHandler(this.lvModList_ItemActivate);
      this.lvModList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvModList_ItemCheck);
      this.lvModList.SelectedIndexChanged += new System.EventHandler(this.lvModList_SelectedIndexChanged);
      // 
      // fomodContextMenu
      // 
      this.fomodContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.visitWebsiteToolStripMenuItem,
            this.emailAuthorToolStripMenuItem,
            this.fomodStatusToolStripMenuItem,
            this.deleteToolStripMenuItem});
      this.fomodContextMenu.Name = "fomodContextMenu";
      this.fomodContextMenu.Size = new System.Drawing.Size(147, 92);
      this.fomodContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.fomodContextMenu_Opening);
      // 
      // visitWebsiteToolStripMenuItem
      // 
      this.visitWebsiteToolStripMenuItem.Name = "visitWebsiteToolStripMenuItem";
      this.visitWebsiteToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
      this.visitWebsiteToolStripMenuItem.Text = "Visit website";
      this.visitWebsiteToolStripMenuItem.Click += new System.EventHandler(this.visitWebsiteToolStripMenuItem_Click);
      // 
      // emailAuthorToolStripMenuItem
      // 
      this.emailAuthorToolStripMenuItem.Name = "emailAuthorToolStripMenuItem";
      this.emailAuthorToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
      this.emailAuthorToolStripMenuItem.Text = "email author";
      this.emailAuthorToolStripMenuItem.Click += new System.EventHandler(this.emailAuthorToolStripMenuItem_Click);
      // 
      // fomodStatusToolStripMenuItem
      // 
      this.fomodStatusToolStripMenuItem.Name = "fomodStatusToolStripMenuItem";
      this.fomodStatusToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
      this.fomodStatusToolStripMenuItem.Text = "Fomod status";
      this.fomodStatusToolStripMenuItem.Click += new System.EventHandler(this.fomodStatusToolStripMenuItem_Click);
      // 
      // deleteToolStripMenuItem
      // 
      this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
      this.deleteToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
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
      this.tbModInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tbModInfo.Size = new System.Drawing.Size(475, 168);
      this.tbModInfo.TabIndex = 0;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pictureBox1.Location = new System.Drawing.Point(493, 27);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(120, 90);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox1.TabIndex = 16;
      this.pictureBox1.TabStop = false;
      this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
      // 
      // bEditScript
      // 
      this.bEditScript.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bEditScript.Location = new System.Drawing.Point(493, 385);
      this.bEditScript.Name = "bEditScript";
      this.bEditScript.Size = new System.Drawing.Size(120, 23);
      this.bEditScript.TabIndex = 9;
      this.bEditScript.Text = "Edit script";
      this.bEditScript.UseVisualStyleBackColor = true;
      this.bEditScript.Click += new System.EventHandler(this.bEditScript_Click);
      // 
      // butViewReadme
      // 
      this.butViewReadme.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.butViewReadme.Location = new System.Drawing.Point(493, 356);
      this.butViewReadme.Name = "butViewReadme";
      this.butViewReadme.Size = new System.Drawing.Size(120, 23);
      this.butViewReadme.TabIndex = 8;
      this.butViewReadme.Text = "View readme";
      this.butViewReadme.UseVisualStyleBackColor = true;
      this.butViewReadme.Click += new System.EventHandler(this.butViewReadme_Click);
      // 
      // bEditInfo
      // 
      this.bEditInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bEditInfo.Location = new System.Drawing.Point(493, 414);
      this.bEditInfo.Name = "bEditInfo";
      this.bEditInfo.Size = new System.Drawing.Size(120, 23);
      this.bEditInfo.TabIndex = 10;
      this.bEditInfo.Text = "Edit info";
      this.bEditInfo.UseVisualStyleBackColor = true;
      this.bEditInfo.Click += new System.EventHandler(this.bEditInfo_Click);
      // 
      // bActivate
      // 
      this.bActivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bActivate.Location = new System.Drawing.Point(493, 196);
      this.bActivate.Name = "bActivate";
      this.bActivate.Size = new System.Drawing.Size(120, 23);
      this.bActivate.TabIndex = 3;
      this.bActivate.Text = "Activate";
      this.bActivate.UseVisualStyleBackColor = true;
      this.bActivate.Click += new System.EventHandler(this.bActivate_Click);
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
      this.cbGroups.Location = new System.Drawing.Point(493, 487);
      this.cbGroups.Name = "cbGroups";
      this.cbGroups.Size = new System.Drawing.Size(97, 17);
      this.cbGroups.TabIndex = 12;
      this.cbGroups.Text = "Display Groups";
      this.cbGroups.UseVisualStyleBackColor = true;
      this.cbGroups.CheckedChanged += new System.EventHandler(this.cbGroups_CheckedChanged);
      // 
      // bEditGroups
      // 
      this.bEditGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bEditGroups.Location = new System.Drawing.Point(493, 458);
      this.bEditGroups.Name = "bEditGroups";
      this.bEditGroups.Size = new System.Drawing.Size(120, 23);
      this.bEditGroups.TabIndex = 11;
      this.bEditGroups.Text = "Edit groups";
      this.bEditGroups.UseVisualStyleBackColor = true;
      this.bEditGroups.Click += new System.EventHandler(this.bEditGroups_Click);
      // 
      // bActivateGroup
      // 
      this.bActivateGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bActivateGroup.Enabled = false;
      this.bActivateGroup.Location = new System.Drawing.Point(493, 254);
      this.bActivateGroup.Name = "bActivateGroup";
      this.bActivateGroup.Size = new System.Drawing.Size(120, 23);
      this.bActivateGroup.TabIndex = 5;
      this.bActivateGroup.Text = "Activate group";
      this.bActivateGroup.UseVisualStyleBackColor = true;
      this.bActivateGroup.Click += new System.EventHandler(this.bActivateGroup_Click);
      // 
      // bDeactivateGroup
      // 
      this.bDeactivateGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bDeactivateGroup.Enabled = false;
      this.bDeactivateGroup.Location = new System.Drawing.Point(493, 283);
      this.bDeactivateGroup.Name = "bDeactivateGroup";
      this.bDeactivateGroup.Size = new System.Drawing.Size(120, 23);
      this.bDeactivateGroup.TabIndex = 6;
      this.bDeactivateGroup.Text = "Deactivate group";
      this.bDeactivateGroup.UseVisualStyleBackColor = true;
      this.bDeactivateGroup.Click += new System.EventHandler(this.bDeactivateGroup_Click);
      // 
      // bDeactivateAll
      // 
      this.bDeactivateAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bDeactivateAll.Enabled = false;
      this.bDeactivateAll.Location = new System.Drawing.Point(493, 312);
      this.bDeactivateAll.Name = "bDeactivateAll";
      this.bDeactivateAll.Size = new System.Drawing.Size(120, 23);
      this.bDeactivateAll.TabIndex = 7;
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
      this.cmbSortOrder.Location = new System.Drawing.Point(493, 510);
      this.cmbSortOrder.Name = "cmbSortOrder";
      this.cmbSortOrder.Size = new System.Drawing.Size(120, 21);
      this.cmbSortOrder.TabIndex = 13;
      this.cmbSortOrder.Text = "Sort order";
      this.cmbSortOrder.SelectedIndexChanged += new System.EventHandler(this.cmbSortOrder_SelectedIndexChanged);
      this.cmbSortOrder.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmbSortOrder_KeyPress);
      // 
      // butDeactivate
      // 
      this.butDeactivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.butDeactivate.Location = new System.Drawing.Point(493, 225);
      this.butDeactivate.Name = "butDeactivate";
      this.butDeactivate.Size = new System.Drawing.Size(120, 23);
      this.butDeactivate.TabIndex = 4;
      this.butDeactivate.Text = "Deactivate";
      this.butDeactivate.UseVisualStyleBackColor = true;
      this.butDeactivate.Click += new System.EventHandler(this.butDeactivate_Click);
      // 
      // butExtractFomod
      // 
      this.butExtractFomod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.butExtractFomod.Location = new System.Drawing.Point(493, 152);
      this.butExtractFomod.Name = "butExtractFomod";
      this.butExtractFomod.Size = new System.Drawing.Size(120, 23);
      this.butExtractFomod.TabIndex = 2;
      this.butExtractFomod.Text = "Extract to...";
      this.butExtractFomod.UseVisualStyleBackColor = true;
      this.butExtractFomod.Click += new System.EventHandler(this.butExtractFomod_Click);
      // 
      // cmsAddFomod
      // 
      this.cmsAddFomod.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addFOMODToolStripMenuItem,
            this.addPFPToolStripMenuItem,
            this.createFromFolderToolStripMenuItem,
            this.createFOMODToolStripMenuItem,
            this.editPFPToolStripMenuItem});
      this.cmsAddFomod.Name = "cmsAddFomod";
      this.cmsAddFomod.Size = new System.Drawing.Size(176, 114);
      // 
      // addFOMODToolStripMenuItem
      // 
      this.addFOMODToolStripMenuItem.Name = "addFOMODToolStripMenuItem";
      this.addFOMODToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
      this.addFOMODToolStripMenuItem.Text = "Add FOMod";
      this.addFOMODToolStripMenuItem.Click += new System.EventHandler(this.addFOMODToolStripMenuItem_Click);
      // 
      // addPFPToolStripMenuItem
      // 
      this.addPFPToolStripMenuItem.Name = "addPFPToolStripMenuItem";
      this.addPFPToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
      this.addPFPToolStripMenuItem.Text = "Add PFP";
      this.addPFPToolStripMenuItem.Click += new System.EventHandler(this.addPFPToolStripMenuItem_Click);
      // 
      // createFromFolderToolStripMenuItem
      // 
      this.createFromFolderToolStripMenuItem.Name = "createFromFolderToolStripMenuItem";
      this.createFromFolderToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
      this.createFromFolderToolStripMenuItem.Text = "Create From Folder";
      this.createFromFolderToolStripMenuItem.Click += new System.EventHandler(this.createFromFolderToolStripMenuItem_Click);
      // 
      // createFOMODToolStripMenuItem
      // 
      this.createFOMODToolStripMenuItem.Name = "createFOMODToolStripMenuItem";
      this.createFOMODToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
      this.createFOMODToolStripMenuItem.Text = "Create FOMod";
      this.createFOMODToolStripMenuItem.Click += new System.EventHandler(this.createFOMODToolStripMenuItem_Click);
      // 
      // editPFPToolStripMenuItem
      // 
      this.editPFPToolStripMenuItem.Name = "editPFPToolStripMenuItem";
      this.editPFPToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
      this.editPFPToolStripMenuItem.Text = "Edit PFP";
      this.editPFPToolStripMenuItem.Click += new System.EventHandler(this.editPFPToolStripMenuItem_Click);
      // 
      // sbtAddFomod
      // 
      this.sbtAddFomod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.sbtAddFomod.AutoSize = true;
      this.sbtAddFomod.ContextMenuStrip = this.cmsAddFomod;
      this.sbtAddFomod.Location = new System.Drawing.Point(493, 123);
      this.sbtAddFomod.Name = "sbtAddFomod";
      this.sbtAddFomod.Size = new System.Drawing.Size(120, 23);
      this.sbtAddFomod.TabIndex = 19;
      this.sbtAddFomod.Text = "Add FOMod";
      this.sbtAddFomod.UseVisualStyleBackColor = true;
      // 
      // menuStrip1
      // 
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modListToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(625, 24);
      this.menuStrip1.TabIndex = 20;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // modListToolStripMenuItem
      // 
      this.modListToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportModListToolStripMenuItem,
            this.exportActiveModListToolStripMenuItem});
      this.modListToolStripMenuItem.Name = "modListToolStripMenuItem";
      this.modListToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
      this.modListToolStripMenuItem.Text = "Mod List";
      // 
      // exportModListToolStripMenuItem
      // 
      this.exportModListToolStripMenuItem.Name = "exportModListToolStripMenuItem";
      this.exportModListToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
      this.exportModListToolStripMenuItem.Text = "Export Mod List...";
      this.exportModListToolStripMenuItem.Click += new System.EventHandler(this.exportModListToolStripMenuItem_Click);
      // 
      // exportActiveModListToolStripMenuItem
      // 
      this.exportActiveModListToolStripMenuItem.Name = "exportActiveModListToolStripMenuItem";
      this.exportActiveModListToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
      this.exportActiveModListToolStripMenuItem.Text = "Export Active Mod List...";
      this.exportActiveModListToolStripMenuItem.Click += new System.EventHandler(this.exportActiveModListToolStripMenuItem_Click);
      // 
      // PackageManager
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(625, 599);
      this.Controls.Add(this.menuStrip1);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.sbtAddFomod);
      this.Controls.Add(this.butDeactivate);
      this.Controls.Add(this.butExtractFomod);
      this.Controls.Add(this.cbGroups);
      this.Controls.Add(this.cmbSortOrder);
      this.Controls.Add(this.bEditGroups);
      this.Controls.Add(this.bDeactivateAll);
      this.Controls.Add(this.bDeactivateGroup);
      this.Controls.Add(this.bActivateGroup);
      this.Controls.Add(this.butViewReadme);
      this.Controls.Add(this.bActivate);
      this.Controls.Add(this.bEditInfo);
      this.Controls.Add(this.bEditScript);
      this.MainMenuStrip = this.menuStrip1;
      this.Name = "PackageManager";
      this.Text = "Package Manager";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PackageManager_FormClosing);
      this.Load += new System.EventHandler(this.PackageManager_Load);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.fomodContextMenu.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.cmsAddFomod.ResumeLayout(false);
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
         private System.Windows.Forms.ListView lvModList;
        private System.Windows.Forms.TextBox tbModInfo;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button bEditScript;
        private System.Windows.Forms.Button butViewReadme;
        private System.Windows.Forms.Button bEditInfo;
    private System.Windows.Forms.Button bActivate;
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
    private System.Windows.Forms.Button butDeactivate;
    private System.Windows.Forms.Button butExtractFomod;
    private System.Windows.Forms.FolderBrowserDialog fbdExtractFomod;
    private Fomm.Controls.SplitButton sbtAddFomod;
    private System.Windows.Forms.ContextMenuStrip cmsAddFomod;
    private System.Windows.Forms.ToolStripMenuItem addFOMODToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createFromFolderToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createFOMODToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem addPFPToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem editPFPToolStripMenuItem;
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem modListToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem exportModListToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem exportActiveModListToolStripMenuItem;
    }
}