namespace Fomm.Games.Fallout3.Tools
{
    partial class SaveForm {
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
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.lName = new System.Windows.Forms.Label();
      this.lLocation = new System.Windows.Forms.Label();
      this.lDate = new System.Windows.Forms.Label();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.lvPlugins = new System.Windows.Forms.ListView();
      this.SaveImageList = new System.Windows.Forms.ImageList(this.components);
      this.lvSaves = new System.Windows.Forms.ListView();
      this.cmbSort = new System.Windows.Forms.ComboBox();
      this.DudMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.bExport = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // pictureBox1
      // 
      this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pictureBox1.Location = new System.Drawing.Point(49, 12);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(320, 240);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox1.TabIndex = 0;
      this.pictureBox1.TabStop = false;
      // 
      // lName
      // 
      this.lName.AutoSize = true;
      this.lName.Location = new System.Drawing.Point(12, 255);
      this.lName.Name = "lName";
      this.lName.Size = new System.Drawing.Size(71, 13);
      this.lName.TabIndex = 0;
      this.lName.Text = "Player name: ";
      // 
      // lLocation
      // 
      this.lLocation.AutoSize = true;
      this.lLocation.Location = new System.Drawing.Point(12, 268);
      this.lLocation.Name = "lLocation";
      this.lLocation.Size = new System.Drawing.Size(82, 13);
      this.lLocation.TabIndex = 1;
      this.lLocation.Text = "Player location: ";
      // 
      // lDate
      // 
      this.lDate.AutoSize = true;
      this.lDate.Location = new System.Drawing.Point(12, 281);
      this.lDate.Name = "lDate";
      this.lDate.Size = new System.Drawing.Size(68, 13);
      this.lDate.TabIndex = 2;
      this.lDate.Text = "Date saved: ";
      // 
      // splitContainer1
      // 
      this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.splitContainer1.Location = new System.Drawing.Point(15, 297);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.lvPlugins);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.lvSaves);
      this.splitContainer1.Size = new System.Drawing.Size(386, 264);
      this.splitContainer1.SplitterDistance = 132;
      this.splitContainer1.TabIndex = 6;
      // 
      // lvPlugins
      // 
      this.lvPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lvPlugins.Location = new System.Drawing.Point(0, 0);
      this.lvPlugins.Name = "lvPlugins";
      this.lvPlugins.Size = new System.Drawing.Size(386, 132);
      this.lvPlugins.SmallImageList = this.SaveImageList;
      this.lvPlugins.TabIndex = 0;
      this.lvPlugins.UseCompatibleStateImageBehavior = false;
      this.lvPlugins.View = System.Windows.Forms.View.List;
      // 
      // SaveImageList
      // 
      this.SaveImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
      this.SaveImageList.ImageSize = new System.Drawing.Size(16, 16);
      this.SaveImageList.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // lvSaves
      // 
      this.lvSaves.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lvSaves.Location = new System.Drawing.Point(0, 0);
      this.lvSaves.MultiSelect = false;
      this.lvSaves.Name = "lvSaves";
      this.lvSaves.ShowItemToolTips = true;
      this.lvSaves.Size = new System.Drawing.Size(386, 128);
      this.lvSaves.SmallImageList = this.SaveImageList;
      this.lvSaves.TabIndex = 0;
      this.lvSaves.UseCompatibleStateImageBehavior = false;
      this.lvSaves.View = System.Windows.Forms.View.List;
      this.lvSaves.SelectedIndexChanged += new System.EventHandler(this.lvSaves_SelectedIndexChanged);
      // 
      // cmbSort
      // 
      this.cmbSort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cmbSort.ContextMenuStrip = this.DudMenu;
      this.cmbSort.FormattingEnabled = true;
      this.cmbSort.Items.AddRange(new object[] {
            "File name",
            "Player name",
            "Location",
            "Date saved",
            "File size"});
      this.cmbSort.Location = new System.Drawing.Point(15, 567);
      this.cmbSort.Name = "cmbSort";
      this.cmbSort.Size = new System.Drawing.Size(121, 21);
      this.cmbSort.TabIndex = 3;
      this.cmbSort.Text = "File name";
      this.cmbSort.SelectedIndexChanged += new System.EventHandler(this.cmbSort_SelectedIndexChanged);
      this.cmbSort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmbSort_KeyPress);
      // 
      // DudMenu
      // 
      this.DudMenu.Name = "DudMenu";
      this.DudMenu.Size = new System.Drawing.Size(61, 4);
      // 
      // bExport
      // 
      this.bExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bExport.Location = new System.Drawing.Point(204, 567);
      this.bExport.Name = "bExport";
      this.bExport.Size = new System.Drawing.Size(118, 23);
      this.bExport.TabIndex = 7;
      this.bExport.Text = "Export load order";
      this.bExport.UseVisualStyleBackColor = true;
      this.bExport.Click += new System.EventHandler(this.bExport_Click);
      // 
      // SaveForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(413, 596);
      this.Controls.Add(this.bExport);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.cmbSort);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.lDate);
      this.Controls.Add(this.lLocation);
      this.Controls.Add(this.lName);
      this.MinimumSize = new System.Drawing.Size(421, 555);
      this.Name = "SaveForm";
      this.Text = "Save games";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lName;
        private System.Windows.Forms.Label lLocation;
        private System.Windows.Forms.Label lDate;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView lvPlugins;
        private System.Windows.Forms.ListView lvSaves;
        private System.Windows.Forms.ImageList SaveImageList;
        private System.Windows.Forms.ComboBox cmbSort;
        private System.Windows.Forms.ContextMenuStrip DudMenu;
        private System.Windows.Forms.Button bExport;
    }
}