namespace Fomm.FileManager
{
	partial class FileManager
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileManager));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tvwFolders = new System.Windows.Forms.TreeView();
			this.imlFolders = new System.Windows.Forms.ImageList(this.components);
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.lvwFiles = new System.Windows.Forms.ListView();
			this.chdName = new System.Windows.Forms.ColumnHeader();
			this.chdDateCreated = new System.Windows.Forms.ColumnHeader();
			this.chdDateModified = new System.Windows.Forms.ColumnHeader();
			this.chdSize = new System.Windows.Forms.ColumnHeader();
			this.imlFiles = new System.Windows.Forms.ImageList(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.radByFile = new System.Windows.Forms.RadioButton();
			this.radByMod = new System.Windows.Forms.RadioButton();
			this.rlvOverwrites = new L0ki.Controls.ReordableItemListView();
			this.chdModName = new System.Windows.Forms.ColumnHeader();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tvwFolders);
			this.splitContainer1.Panel1.Controls.Add(this.panel2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(806, 466);
			this.splitContainer1.SplitterDistance = 268;
			this.splitContainer1.TabIndex = 0;
			// 
			// tvwFolders
			// 
			this.tvwFolders.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvwFolders.HideSelection = false;
			this.tvwFolders.ImageKey = "Folder_Open.png";
			this.tvwFolders.ImageList = this.imlFolders;
			this.tvwFolders.Location = new System.Drawing.Point(0, 28);
			this.tvwFolders.Name = "tvwFolders";
			this.tvwFolders.SelectedImageKey = "Folder_Open.png";
			this.tvwFolders.Size = new System.Drawing.Size(268, 438);
			this.tvwFolders.TabIndex = 0;
			this.tvwFolders.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwFolders_AfterSelect);
			// 
			// imlFolders
			// 
			this.imlFolders.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlFolders.ImageStream")));
			this.imlFolders.TransparentColor = System.Drawing.Color.Transparent;
			this.imlFolders.Images.SetKeyName(0, "Folder_Open.png");
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.lvwFiles);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.rlvOverwrites);
			this.splitContainer2.Panel2.Controls.Add(this.panel1);
			this.splitContainer2.Size = new System.Drawing.Size(534, 466);
			this.splitContainer2.SplitterDistance = 168;
			this.splitContainer2.TabIndex = 0;
			// 
			// lvwFiles
			// 
			this.lvwFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chdName,
            this.chdDateCreated,
            this.chdDateModified,
            this.chdSize});
			this.lvwFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvwFiles.HideSelection = false;
			this.lvwFiles.Location = new System.Drawing.Point(0, 0);
			this.lvwFiles.Name = "lvwFiles";
			this.lvwFiles.Size = new System.Drawing.Size(534, 168);
			this.lvwFiles.SmallImageList = this.imlFiles;
			this.lvwFiles.TabIndex = 0;
			this.lvwFiles.UseCompatibleStateImageBehavior = false;
			this.lvwFiles.View = System.Windows.Forms.View.Details;
			this.lvwFiles.SelectedIndexChanged += new System.EventHandler(this.lvwFiles_SelectedIndexChanged);
			// 
			// chdName
			// 
			this.chdName.Text = "Name";
			this.chdName.Width = 200;
			// 
			// chdDateCreated
			// 
			this.chdDateCreated.Text = "Date Created";
			this.chdDateCreated.Width = 125;
			// 
			// chdDateModified
			// 
			this.chdDateModified.Text = "Date Modified";
			this.chdDateModified.Width = 125;
			// 
			// chdSize
			// 
			this.chdSize.Text = "Size";
			this.chdSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.chdSize.Width = 80;
			// 
			// imlFiles
			// 
			this.imlFiles.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imlFiles.ImageSize = new System.Drawing.Size(16, 16);
			this.imlFiles.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.label1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(534, 41);
			this.panel1.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(329, 39);
			this.label1.TabIndex = 0;
			this.label1.Text = "Drag/Drop to modify which mod\'s file version is used.\r\nMods towards the bottom ov" +
				"erride those above them.\r\nThe green highlighted mod is the one whose file is cur" +
				"rently installed.\r\n";
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.radByMod);
			this.panel2.Controls.Add(this.radByFile);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(268, 28);
			this.panel2.TabIndex = 1;
			// 
			// radByFile
			// 
			this.radByFile.Appearance = System.Windows.Forms.Appearance.Button;
			this.radByFile.AutoSize = true;
			this.radByFile.Checked = true;
			this.radByFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radByFile.Location = new System.Drawing.Point(3, 3);
			this.radByFile.Name = "radByFile";
			this.radByFile.Size = new System.Drawing.Size(79, 25);
			this.radByFile.TabIndex = 1;
			this.radByFile.TabStop = true;
			this.radByFile.Text = "Order By File";
			this.radByFile.UseVisualStyleBackColor = true;
			this.radByFile.CheckedChanged += new System.EventHandler(this.radByFile_CheckedChanged);
			// 
			// radByMod
			// 
			this.radByMod.Appearance = System.Windows.Forms.Appearance.Button;
			this.radByMod.AutoSize = true;
			this.radByMod.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radByMod.Location = new System.Drawing.Point(80, 3);
			this.radByMod.Name = "radByMod";
			this.radByMod.Size = new System.Drawing.Size(84, 25);
			this.radByMod.TabIndex = 2;
			this.radByMod.Text = "Order By Mod";
			this.radByMod.UseVisualStyleBackColor = true;
			// 
			// rlvOverwrites
			// 
			this.rlvOverwrites.AllowDrop = true;
			this.rlvOverwrites.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chdModName});
			this.rlvOverwrites.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rlvOverwrites.FullRowSelect = true;
			this.rlvOverwrites.Location = new System.Drawing.Point(0, 41);
			this.rlvOverwrites.Name = "rlvOverwrites";
			this.rlvOverwrites.ShowGroups = false;
			this.rlvOverwrites.Size = new System.Drawing.Size(534, 253);
			this.rlvOverwrites.TabIndex = 0;
			this.rlvOverwrites.UseCompatibleStateImageBehavior = false;
			this.rlvOverwrites.View = System.Windows.Forms.View.Details;
			this.rlvOverwrites.SizeChanged += new System.EventHandler(this.rlvOverwrites_SizeChanged);
			this.rlvOverwrites.DragDrop += new System.Windows.Forms.DragEventHandler(this.rlvOverwrites_DragDrop);
			// 
			// chdModName
			// 
			this.chdModName.Text = "Mod Name";
			this.chdModName.Width = 400;
			// 
			// FileManager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(806, 466);
			this.Controls.Add(this.splitContainer1);
			this.Name = "FileManager";
			this.ShowInTaskbar = false;
			this.Text = "File Manager";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TreeView tvwFolders;
		private System.Windows.Forms.ImageList imlFolders;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.ListView lvwFiles;
		private L0ki.Controls.ReordableItemListView rlvOverwrites;
		private System.Windows.Forms.ColumnHeader chdName;
		private System.Windows.Forms.ColumnHeader chdDateCreated;
		private System.Windows.Forms.ColumnHeader chdDateModified;
		private System.Windows.Forms.ColumnHeader chdSize;
		private System.Windows.Forms.ImageList imlFiles;
		private System.Windows.Forms.ColumnHeader chdModName;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RadioButton radByMod;
		private System.Windows.Forms.RadioButton radByFile;
	}
}