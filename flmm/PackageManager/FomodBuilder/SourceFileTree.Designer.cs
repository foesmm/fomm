namespace Fomm.PackageManager.FomodBuilder
{
	partial class SourceFileTree
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
			this.imlIcons = new System.Windows.Forms.ImageList(this.components);
			this.panel2 = new System.Windows.Forms.Panel();
			this.butAddFolder = new System.Windows.Forms.Button();
			this.butAddFiles = new System.Windows.Forms.Button();
			this.panel3 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.tvwSource = new System.Windows.Forms.TreeView();
			this.ofdFileChooser = new System.Windows.Forms.OpenFileDialog();
			this.fbdFolderChooser = new System.Windows.Forms.FolderBrowserDialog();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// imlIcons
			// 
			this.imlIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlIcons.ImageStream")));
			this.imlIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.imlIcons.Images.SetKeyName(0, "folder");
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.butAddFolder);
			this.panel2.Controls.Add(this.butAddFiles);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 20);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(229, 34);
			this.panel2.TabIndex = 1;
			// 
			// butAddFolder
			// 
			this.butAddFolder.Location = new System.Drawing.Point(84, 6);
			this.butAddFolder.Name = "butAddFolder";
			this.butAddFolder.Size = new System.Drawing.Size(75, 23);
			this.butAddFolder.TabIndex = 1;
			this.butAddFolder.Text = "Add Folder...";
			this.butAddFolder.UseVisualStyleBackColor = true;
			this.butAddFolder.Click += new System.EventHandler(this.butAddFolder_Click);
			// 
			// butAddFiles
			// 
			this.butAddFiles.Location = new System.Drawing.Point(3, 6);
			this.butAddFiles.Name = "butAddFiles";
			this.butAddFiles.Size = new System.Drawing.Size(75, 23);
			this.butAddFiles.TabIndex = 0;
			this.butAddFiles.Text = "Add Files...";
			this.butAddFiles.UseVisualStyleBackColor = true;
			this.butAddFiles.Click += new System.EventHandler(this.butAddFiles_Click);
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.label1);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel3.Location = new System.Drawing.Point(0, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(229, 20);
			this.panel3.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(3, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(77, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Source Files";
			// 
			// tvwSource
			// 
			this.tvwSource.AllowDrop = true;
			this.tvwSource.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvwSource.HideSelection = false;
			this.tvwSource.ImageIndex = 0;
			this.tvwSource.ImageList = this.imlIcons;
			this.tvwSource.Location = new System.Drawing.Point(0, 54);
			this.tvwSource.Name = "tvwSource";
			this.tvwSource.SelectedImageIndex = 0;
			this.tvwSource.Size = new System.Drawing.Size(229, 163);
			this.tvwSource.TabIndex = 0;
			this.tvwSource.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.tvwSource_QueryContinueDrag);
			this.tvwSource.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvwSource_BeforeExpand);
			this.tvwSource.DragDrop += new System.Windows.Forms.DragEventHandler(this.tvwSource_DragDrop);
			this.tvwSource.DragEnter += new System.Windows.Forms.DragEventHandler(this.tvwSource_DragEnter);
			this.tvwSource.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tvwSource_ItemDrag);
			// 
			// ofdFileChooser
			// 
			this.ofdFileChooser.Multiselect = true;
			this.ofdFileChooser.RestoreDirectory = true;
			// 
			// SourceFileTree
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tvwSource);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel3);
			this.Name = "SourceFileTree";
			this.Size = new System.Drawing.Size(229, 217);
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Button butAddFolder;
		private System.Windows.Forms.Button butAddFiles;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.ImageList imlIcons;
		private System.Windows.Forms.TreeView tvwSource;
		private System.Windows.Forms.OpenFileDialog ofdFileChooser;
		private System.Windows.Forms.FolderBrowserDialog fbdFolderChooser;
	}
}
