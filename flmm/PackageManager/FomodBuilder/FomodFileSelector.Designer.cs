namespace Fomm.PackageManager.FomodBuilder
{
	partial class FomodFileSelector
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FomodFileSelector));
			this.tvwFomod = new System.Windows.Forms.TreeView();
			this.cmsFomod = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.newFolderToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.imlIcons = new System.Windows.Forms.ImageList(this.components);
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.sftSources = new Fomm.PackageManager.FomodBuilder.SourceFileTree();
			this.panel4 = new System.Windows.Forms.Panel();
			this.lblFomodFiles = new System.Windows.Forms.Label();
			this.cmsFomodNode = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.nodeNewFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new System.Windows.Forms.Panel();
			this.lnkHelp = new System.Windows.Forms.LinkLabel();
			this.rtbHelp = new System.Windows.Forms.RichTextBox();
			this.cmsFomod.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.panel4.SuspendLayout();
			this.cmsFomodNode.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tvwFomod
			// 
			this.tvwFomod.AllowDrop = true;
			this.tvwFomod.ContextMenuStrip = this.cmsFomod;
			this.tvwFomod.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvwFomod.HideSelection = false;
			this.tvwFomod.ImageIndex = 0;
			this.tvwFomod.ImageList = this.imlIcons;
			this.tvwFomod.LabelEdit = true;
			this.tvwFomod.Location = new System.Drawing.Point(0, 20);
			this.tvwFomod.Name = "tvwFomod";
			this.tvwFomod.SelectedImageIndex = 0;
			this.tvwFomod.Size = new System.Drawing.Size(161, 145);
			this.tvwFomod.TabIndex = 1;
			this.tvwFomod.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvwFomod_AfterLabelEdit);
			this.tvwFomod.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvwFomod_BeforeExpand);
			this.tvwFomod.DragDrop += new System.Windows.Forms.DragEventHandler(this.tvwFomod_DragDrop);
			this.tvwFomod.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvwFomod_MouseDown);
			this.tvwFomod.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvwFomod_KeyDown);
			this.tvwFomod.DragOver += new System.Windows.Forms.DragEventHandler(this.tvwFomod_DragOver);
			// 
			// cmsFomod
			// 
			this.cmsFomod.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newFolderToolStripMenuItem1});
			this.cmsFomod.Name = "cmsFomod";
			this.cmsFomod.Size = new System.Drawing.Size(135, 26);
			// 
			// newFolderToolStripMenuItem1
			// 
			this.newFolderToolStripMenuItem1.Name = "newFolderToolStripMenuItem1";
			this.newFolderToolStripMenuItem1.Size = new System.Drawing.Size(134, 22);
			this.newFolderToolStripMenuItem1.Text = "New Folder";
			this.newFolderToolStripMenuItem1.Click += new System.EventHandler(this.newFolderToolStripMenuItem_Click);
			// 
			// imlIcons
			// 
			this.imlIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlIcons.ImageStream")));
			this.imlIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.imlIcons.Images.SetKeyName(0, "folder");
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.IsSplitterFixed = true;
			this.splitContainer1.Location = new System.Drawing.Point(0, 75);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.sftSources);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.tvwFomod);
			this.splitContainer1.Panel2.Controls.Add(this.panel4);
			this.splitContainer1.Size = new System.Drawing.Size(337, 165);
			this.splitContainer1.SplitterDistance = 166;
			this.splitContainer1.SplitterWidth = 10;
			this.splitContainer1.TabIndex = 1;
			// 
			// sftSources
			// 
			this.sftSources.Cursor = System.Windows.Forms.Cursors.Default;
			this.sftSources.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sftSources.Location = new System.Drawing.Point(0, 0);
			this.sftSources.Name = "sftSources";
			this.sftSources.Size = new System.Drawing.Size(166, 165);
			this.sftSources.Sources = new string[0];
			this.sftSources.TabIndex = 0;
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.lblFomodFiles);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel4.Location = new System.Drawing.Point(0, 0);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(161, 20);
			this.panel4.TabIndex = 2;
			// 
			// lblFomodFiles
			// 
			this.lblFomodFiles.AutoSize = true;
			this.lblFomodFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblFomodFiles.Location = new System.Drawing.Point(3, 3);
			this.lblFomodFiles.Name = "lblFomodFiles";
			this.lblFomodFiles.Size = new System.Drawing.Size(77, 13);
			this.lblFomodFiles.TabIndex = 0;
			this.lblFomodFiles.Text = "FOMod Files";
			// 
			// cmsFomodNode
			// 
			this.cmsFomodNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.nodeNewFolderToolStripMenuItem});
			this.cmsFomodNode.Name = "cmsFomod";
			this.cmsFomodNode.Size = new System.Drawing.Size(153, 92);
			this.cmsFomodNode.Opening += new System.ComponentModel.CancelEventHandler(this.cmsFomodNode_Opening);
			// 
			// renameToolStripMenuItem
			// 
			this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
			this.renameToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.renameToolStripMenuItem.Text = "Rename";
			this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.deleteToolStripMenuItem.Text = "Delete";
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// nodeNewFolderToolStripMenuItem
			// 
			this.nodeNewFolderToolStripMenuItem.Name = "nodeNewFolderToolStripMenuItem";
			this.nodeNewFolderToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.nodeNewFolderToolStripMenuItem.Text = "New Folder";
			this.nodeNewFolderToolStripMenuItem.Click += new System.EventHandler(this.newFolderToolStripMenuItem_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.lnkHelp);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(337, 17);
			this.panel1.TabIndex = 0;
			// 
			// lnkHelp
			// 
			this.lnkHelp.AutoSize = true;
			this.lnkHelp.Location = new System.Drawing.Point(0, 0);
			this.lnkHelp.Name = "lnkHelp";
			this.lnkHelp.Size = new System.Drawing.Size(58, 13);
			this.lnkHelp.TabIndex = 0;
			this.lnkHelp.TabStop = true;
			this.lnkHelp.Text = "Open Help";
			this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHelp_LinkClicked);
			// 
			// rtbHelp
			// 
			this.rtbHelp.BackColor = System.Drawing.SystemColors.Control;
			this.rtbHelp.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.rtbHelp.Dock = System.Windows.Forms.DockStyle.Top;
			this.rtbHelp.Location = new System.Drawing.Point(0, 17);
			this.rtbHelp.Name = "rtbHelp";
			this.rtbHelp.ReadOnly = true;
			this.rtbHelp.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.rtbHelp.Size = new System.Drawing.Size(337, 58);
			this.rtbHelp.TabIndex = 1;
			this.rtbHelp.TabStop = false;
			this.rtbHelp.Text = "";
			// 
			// FomodFileSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.rtbHelp);
			this.Controls.Add(this.panel1);
			this.Name = "FomodFileSelector";
			this.Size = new System.Drawing.Size(337, 240);
			this.cmsFomod.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			this.cmsFomodNode.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TreeView tvwFomod;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ContextMenuStrip cmsFomodNode;
		private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ImageList imlIcons;
		private System.Windows.Forms.ToolStripMenuItem nodeNewFolderToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip cmsFomod;
		private System.Windows.Forms.ToolStripMenuItem newFolderToolStripMenuItem1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Label lblFomodFiles;
		private System.Windows.Forms.LinkLabel lnkHelp;
		private System.Windows.Forms.RichTextBox rtbHelp;
		private SourceFileTree sftSources;
	}
}
