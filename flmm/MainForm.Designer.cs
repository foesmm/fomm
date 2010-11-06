namespace Fomm
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.bLaunch = new System.Windows.Forms.Button();
			this.cmsPlugins = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.sendToTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.sendToBottomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.uncheckAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.checkAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.lvEspList = new L0ki.Controls.ReordableItemListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.rtbPluginInfo = new System.Windows.Forms.RichTextBox();
			this.bPackageManager = new System.Windows.Forms.Button();
			this.bHelp = new System.Windows.Forms.Button();
			this.bSettings = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.launchGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadOrderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gameSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.viewReadmeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.visitForumsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.checkForUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.butFileManager = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.cmsPlugins.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pictureBox1.Location = new System.Drawing.Point(417, 27);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(120, 90);
			this.pictureBox1.TabIndex = 4;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
			// 
			// bLaunch
			// 
			this.bLaunch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.bLaunch.Location = new System.Drawing.Point(417, 123);
			this.bLaunch.Name = "bLaunch";
			this.bLaunch.Size = new System.Drawing.Size(120, 23);
			this.bLaunch.TabIndex = 0;
			this.bLaunch.Text = "Launch Fallout";
			this.bLaunch.UseVisualStyleBackColor = true;
			this.bLaunch.Click += new System.EventHandler(this.bLaunch_Click);
			// 
			// cmsPlugins
			// 
			this.cmsPlugins.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sendToTopToolStripMenuItem,
            this.sendToBottomToolStripMenuItem,
            this.uncheckAllToolStripMenuItem,
            this.checkAllToolStripMenuItem,
            this.deleteToolStripMenuItem});
			this.cmsPlugins.Name = "cmsPlugins";
			this.cmsPlugins.Size = new System.Drawing.Size(136, 114);
			// 
			// sendToTopToolStripMenuItem
			// 
			this.sendToTopToolStripMenuItem.Name = "sendToTopToolStripMenuItem";
			this.sendToTopToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.sendToTopToolStripMenuItem.Text = "Load first";
			this.sendToTopToolStripMenuItem.Click += new System.EventHandler(this.sendToTopToolStripMenuItem_Click);
			// 
			// sendToBottomToolStripMenuItem
			// 
			this.sendToBottomToolStripMenuItem.Name = "sendToBottomToolStripMenuItem";
			this.sendToBottomToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.sendToBottomToolStripMenuItem.Text = "Load last";
			this.sendToBottomToolStripMenuItem.Click += new System.EventHandler(this.sendToBottomToolStripMenuItem_Click);
			// 
			// uncheckAllToolStripMenuItem
			// 
			this.uncheckAllToolStripMenuItem.Name = "uncheckAllToolStripMenuItem";
			this.uncheckAllToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.uncheckAllToolStripMenuItem.Text = "Uncheck all";
			this.uncheckAllToolStripMenuItem.Click += new System.EventHandler(this.uncheckAllToolStripMenuItem_Click);
			// 
			// checkAllToolStripMenuItem
			// 
			this.checkAllToolStripMenuItem.Name = "checkAllToolStripMenuItem";
			this.checkAllToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.checkAllToolStripMenuItem.Text = "Check all";
			this.checkAllToolStripMenuItem.Click += new System.EventHandler(this.checkAllToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.deleteToolStripMenuItem.Text = "Delete";
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = new System.Drawing.Point(12, 27);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.lvEspList);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.rtbPluginInfo);
			this.splitContainer1.Size = new System.Drawing.Size(399, 562);
			this.splitContainer1.SplitterDistance = 402;
			this.splitContainer1.TabIndex = 11;
			// 
			// lvEspList
			// 
			this.lvEspList.AllowDrop = true;
			this.lvEspList.CheckBoxes = true;
			this.lvEspList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.lvEspList.ContextMenuStrip = this.cmsPlugins;
			this.lvEspList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvEspList.FullRowSelect = true;
			this.lvEspList.Location = new System.Drawing.Point(0, 0);
			this.lvEspList.Name = "lvEspList";
			this.lvEspList.ShowGroups = false;
			this.lvEspList.Size = new System.Drawing.Size(399, 402);
			this.lvEspList.TabIndex = 0;
			this.lvEspList.UseCompatibleStateImageBehavior = false;
			this.lvEspList.View = System.Windows.Forms.View.Details;
			this.lvEspList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvEspList_ItemChecked);
			this.lvEspList.SelectedIndexChanged += new System.EventHandler(this.lvEspList_SelectedIndexChanged);
			this.lvEspList.DragDrop += new System.Windows.Forms.DragEventHandler(this.lvEspList_DragDrop);
			this.lvEspList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvEspList_ColumnClick);
			this.lvEspList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvEspList_KeyDown);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "File";
			this.columnHeader1.Width = 219;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Mod index";
			this.columnHeader2.Width = 87;
			// 
			// rtbPluginInfo
			// 
			this.rtbPluginInfo.BackColor = System.Drawing.SystemColors.Window;
			this.rtbPluginInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.rtbPluginInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rtbPluginInfo.Location = new System.Drawing.Point(0, 0);
			this.rtbPluginInfo.Name = "rtbPluginInfo";
			this.rtbPluginInfo.ReadOnly = true;
			this.rtbPluginInfo.Size = new System.Drawing.Size(399, 156);
			this.rtbPluginInfo.TabIndex = 0;
			this.rtbPluginInfo.Text = "Drag/Drop to modify load order\nAlternatively, hold alt and use the arrow keys\nMod" +
				"s towards the bottom override those above them\nRight click in the plugins list f" +
				"or additional options";
			this.rtbPluginInfo.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtbPluginInfo_LinkClicked);
			// 
			// bPackageManager
			// 
			this.bPackageManager.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.bPackageManager.Location = new System.Drawing.Point(417, 152);
			this.bPackageManager.Name = "bPackageManager";
			this.bPackageManager.Size = new System.Drawing.Size(120, 23);
			this.bPackageManager.TabIndex = 1;
			this.bPackageManager.Text = "Package manager";
			this.bPackageManager.UseVisualStyleBackColor = true;
			this.bPackageManager.Click += new System.EventHandler(this.bPackageManager_Click);
			// 
			// bHelp
			// 
			this.bHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.bHelp.Location = new System.Drawing.Point(417, 288);
			this.bHelp.Name = "bHelp";
			this.bHelp.Size = new System.Drawing.Size(120, 23);
			this.bHelp.TabIndex = 8;
			this.bHelp.Text = "Open readme";
			this.bHelp.UseVisualStyleBackColor = true;
			this.bHelp.Click += new System.EventHandler(this.bHelp_Click);
			// 
			// bSettings
			// 
			this.bSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.bSettings.Location = new System.Drawing.Point(417, 259);
			this.bSettings.Name = "bSettings";
			this.bSettings.Size = new System.Drawing.Size(120, 23);
			this.bSettings.TabIndex = 7;
			this.bSettings.Text = "Settings";
			this.bSettings.UseVisualStyleBackColor = true;
			this.bSettings.Click += new System.EventHandler(this.bSettings_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.loadOrderToolStripMenuItem,
            this.gameSettingsToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(549, 24);
			this.menuStrip1.TabIndex = 9;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.launchGameToolStripMenuItem,
            this.closeToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// launchGameToolStripMenuItem
			// 
			this.launchGameToolStripMenuItem.Name = "launchGameToolStripMenuItem";
			this.launchGameToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.launchGameToolStripMenuItem.Text = "Launch Game";
			// 
			// closeToolStripMenuItem
			// 
			this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
			this.closeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.closeToolStripMenuItem.Text = "Close";
			this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
			this.toolsToolStripMenuItem.Text = "Tools";
			// 
			// loadOrderToolStripMenuItem
			// 
			this.loadOrderToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.copyToClipboardToolStripMenuItem});
			this.loadOrderToolStripMenuItem.Name = "loadOrderToolStripMenuItem";
			this.loadOrderToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
			this.loadOrderToolStripMenuItem.Text = "Load Order";
			// 
			// importToolStripMenuItem
			// 
			this.importToolStripMenuItem.Name = "importToolStripMenuItem";
			this.importToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
			this.importToolStripMenuItem.Text = "Import";
			this.importToolStripMenuItem.Click += new System.EventHandler(this.importLoadOrderToolStripMenuItem_Click);
			// 
			// exportToolStripMenuItem
			// 
			this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
			this.exportToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
			this.exportToolStripMenuItem.Text = "Export";
			this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportLoadOrderToolStripMenuItem_Click);
			// 
			// copyToClipboardToolStripMenuItem
			// 
			this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
			this.copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
			this.copyToClipboardToolStripMenuItem.Text = "Copy to clipboard";
			this.copyToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyLoadOrderToClipboardToolStripMenuItem_Click);
			// 
			// gameSettingsToolStripMenuItem
			// 
			this.gameSettingsToolStripMenuItem.Name = "gameSettingsToolStripMenuItem";
			this.gameSettingsToolStripMenuItem.Size = new System.Drawing.Size(95, 20);
			this.gameSettingsToolStripMenuItem.Text = "Game Settings";
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewReadmeToolStripMenuItem,
            this.visitForumsToolStripMenuItem,
            this.checkForUpdateToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "Help";
			// 
			// viewReadmeToolStripMenuItem
			// 
			this.viewReadmeToolStripMenuItem.Name = "viewReadmeToolStripMenuItem";
			this.viewReadmeToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
			this.viewReadmeToolStripMenuItem.Text = "View readme";
			this.viewReadmeToolStripMenuItem.Click += new System.EventHandler(this.bHelp_Click);
			// 
			// visitForumsToolStripMenuItem
			// 
			this.visitForumsToolStripMenuItem.Name = "visitForumsToolStripMenuItem";
			this.visitForumsToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
			this.visitForumsToolStripMenuItem.Text = "Visit forums";
			this.visitForumsToolStripMenuItem.Click += new System.EventHandler(this.visitForumsToolStripMenuItem_Click);
			// 
			// checkForUpdateToolStripMenuItem
			// 
			this.checkForUpdateToolStripMenuItem.Name = "checkForUpdateToolStripMenuItem";
			this.checkForUpdateToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
			this.checkForUpdateToolStripMenuItem.Text = "Check for update";
			this.checkForUpdateToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdateToolStripMenuItem_Click);
			// 
			// butFileManager
			// 
			this.butFileManager.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butFileManager.Location = new System.Drawing.Point(417, 181);
			this.butFileManager.Name = "butFileManager";
			this.butFileManager.Size = new System.Drawing.Size(120, 23);
			this.butFileManager.TabIndex = 12;
			this.butFileManager.Text = "File manager";
			this.butFileManager.UseVisualStyleBackColor = true;
			this.butFileManager.Click += new System.EventHandler(this.butFileManager_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(549, 600);
			this.Controls.Add(this.butFileManager);
			this.Controls.Add(this.menuStrip1);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.bPackageManager);
			this.Controls.Add(this.bLaunch);
			this.Controls.Add(this.bSettings);
			this.Controls.Add(this.bHelp);
			this.MainMenuStrip = this.menuStrip1;
			this.MinimumSize = new System.Drawing.Size(300, 457);
			this.Name = "MainForm";
			this.Text = "Fallout Mod Manager";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.cmsPlugins.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button bLaunch;
		private L0ki.Controls.ReordableItemListView lvEspList;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.RichTextBox rtbPluginInfo;
		private System.Windows.Forms.Button bPackageManager;
		private System.Windows.Forms.ContextMenuStrip cmsPlugins;
		private System.Windows.Forms.Button bHelp;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ToolStripMenuItem sendToTopToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem sendToBottomToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem uncheckAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem checkAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.Button bSettings;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem launchGameToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadOrderToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToClipboardToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewReadmeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem visitForumsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem checkForUpdateToolStripMenuItem;
		private System.Windows.Forms.Button butFileManager;
		private System.Windows.Forms.ToolStripMenuItem gameSettingsToolStripMenuItem;
	}
}

